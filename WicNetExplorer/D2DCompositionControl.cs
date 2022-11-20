using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Versioning;
using System.Threading;
using System.Windows.Forms;
using DirectN;
using WicNetExplorer.Utilities;
using Windows.Graphics;
using Windows.Graphics.DirectX;
using Windows.UI.Composition;
using Windows.UI.Composition.Desktop;
using WinRT;

namespace WicNetExplorer
{
    [SupportedOSPlatform("windows10.0.17134.0")]
    public class D2DCompositionControl : Control, ID2Control
    {
        // device independent resources
        private static readonly Lazy<IDispatcherQueueController> _dispatcherQueueController = new(() => DispatcherQueueController.Create());
        private static readonly Lazy<object> _d3d11Device = new(() => D3D11Functions.D3D11CreateDevice());
        private static readonly Lazy<IComObject<ID2D1Factory1>> _d2dFactory = new(() => D2D1Functions.D2D1CreateFactory<ID2D1Factory1>());
        private static readonly Lazy<CompositionGraphicsDevice> _graphicsDevice = new(CreateCompositionGraphicsDevice);

        private static CompositionGraphicsDevice CreateCompositionGraphicsDevice()
        {
            _ = _dispatcherQueueController.Value;
            using var dev = new ComObject<IDXGIDevice>((IDXGIDevice)_d3d11Device.Value);
            using var d2dDevice = _d2dFactory.Value.CreateDevice(dev);
            var compositor = new Compositor();
            var interop = compositor.As<ICompositorInterop>();
            interop.CreateGraphicsDevice(d2dDevice.Object, out var graphicsDevice);
            return MarshalInterface<CompositionGraphicsDevice>.FromAbi(graphicsDevice);
        }

        private DesktopWindowTarget? _target;
        private CompositionDrawingSurface? _surface;

        public event EventHandler<D2DDrawEventArgs>? Draw;
        public event EventHandler<D2DReleaseEventArgs>? Releasing;

        public D2DCompositionControl()
        {
        }

        [MemberNotNullWhen(true, "_target")]
        protected virtual bool IsValidTarget => _target != null;
        public IComObject<ID2D1DeviceContext> DeviceContext => throw new NotImplementedException();

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            EnsureTarget();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            ReleaseTarget();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeTarget();
        }

        protected virtual void ResizeTarget()
        {
            if (IsValidTarget)
            {
                _target.Root.Size = new Vector2(Width, Height);
            }
            Invalidate();
        }

        protected virtual void EnsureTarget()
        {
            if (!IsValidTarget)
            {
                CreateTarget();
            }
        }

        protected virtual void CreateTarget()
        {
            ReleaseTarget();

            var interop = _graphicsDevice.Value.Compositor.As<ICompositorDesktopInterop>();
            interop.CreateDesktopWindowTarget(Handle, true, out var ptr).ThrowOnError();
            _target = MarshalInterface<DesktopWindowTarget>.FromAbi(ptr);
            
            var root = _graphicsDevice.Value.Compositor.CreateSpriteVisual();
            root.Size = new Vector2(Width, Height);
            _target.Root = root;

            // we need this for D2D
            _surface = _graphicsDevice.Value.CreateDrawingSurface2(new SizeInt32(Width, Height), DirectXPixelFormat.R16G16B16A16Float, DirectXAlphaMode.Premultiplied);
            root.Brush = _graphicsDevice.Value.Compositor.CreateSurfaceBrush(_surface);

            using var surfaceInterop = new ComObject<ICompositionDrawingSurfaceInterop>(_surface.As<ICompositionDrawingSurfaceInterop>());
            using var dc = surfaceInterop.BeginDraw();
            //dc.Clear(_D3DCOLORVALUE.Pink);
            surfaceInterop.EndDraw();
        }

        protected virtual void ReleaseTarget()
        {
            Interlocked.Exchange(ref _surface, null)?.Dispose();
            Interlocked.Exchange(ref _target, null)?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ReleaseTarget();
            }
        }
    }
}
