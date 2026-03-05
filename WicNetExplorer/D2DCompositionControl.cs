using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.Versioning;
using System.Threading;
using System.Windows.Forms;
using DirectN;
using DirectN.Extensions;
using DirectN.Extensions.Com;
using WicNetExplorer.Utilities;
using Windows.Graphics;
using Windows.Graphics.DirectX;
using Windows.UI.Composition;
using Windows.UI.Composition.Desktop;
using WinRT;

namespace WicNetExplorer;

[SupportedOSPlatform("windows10.0.17134.0")]
public class D2DCompositionControl : Control, ID2DControl
{
    // device independent resources
    private static readonly Lazy<IComObject<ID3D11Device>> _d3d11Device = new(() => D3D11Functions.D3D11CreateDevice(null, D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
#if DEBUG
         D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_DEBUG | D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT
#else
        D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT
#endif
    ));
    private static readonly Lazy<IComObject<ID2D1Factory1>> _d2dFactory = new(() => D2D1Functions.D2D1CreateFactory<ID2D1Factory1>());
    private static readonly Lazy<CompositionGraphicsDevice> _graphicsDevice = new(CreateCompositionGraphicsDevice);

    private static CompositionGraphicsDevice CreateCompositionGraphicsDevice()
    {
        var dev = _d3d11Device.Value.As<IDXGIDevice>(throwOnError: true)!;
        using var d2dDevice = _d2dFactory.Value.CreateDevice(dev);
        var compositor = new Compositor();
        var interop = compositor.As<ICompositorInterop>();
        return ComObject.WithComInstance(d2dDevice, unk =>
        {
            interop.CreateGraphicsDevice(unk, out var obj).ThrowOnError();
            return MarshalInterface<CompositionGraphicsDevice>.FromAbi(obj);
        });
    }

    internal static void DisposeResources()
    {
        if (_d2dFactory.IsValueCreated)
        {
            _d2dFactory.Value?.Dispose();
        }

        if (_d3d11Device.IsValueCreated)
        {
            _d3d11Device.Value?.Dispose();
        }

        if (_graphicsDevice.IsValueCreated)
        {
            _graphicsDevice.Value?.Dispose();
        }
    }

    private DesktopWindowTarget? _target;
    private CompositionDrawingSurface? _surface;

    public event EventHandler<D2DDrawEventArgs>? Draw;

    public D2DCompositionControl() => PixelFormat = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;

    // possible formats are
    //  DXGI_FORMAT_R16G16B16A16_FLOAT
    //  DXGI_FORMAT_R8G8B8A8_UNORM
    //  DXGI_FORMAT_A8_UNORM
    //  DXGI_FORMAT_B8G8R8A8_UNORM
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual DXGI_FORMAT PixelFormat { get; set; }

    [MemberNotNullWhen(true, nameof(_target))]
    protected virtual bool IsValidTarget => _target != null;

    public virtual void WithDeviceContext(Action<IComObject<ID2D1DeviceContext>> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (_surface == null)
            return;

        var size = _surface.Size;
        if (size._width < 0.5f || size._height < 0.5f)
            return;

        using var surfaceInterop = _surface.AsComObject<ICompositionDrawingSurfaceInterop>();
        using var dc = surfaceInterop.BeginDraw<ID2D1DeviceContext>();
        action(dc);

        surfaceInterop.Object.EndDraw(); // don't throw
    }

    public IComObject<ID2D1Bitmap1>? GetSurfaceBitmap()
    {
        if (_surface == null)
            return null;

        var device = (ID3D11Device)_d3d11Device.Value;
        var desc = new D3D11_TEXTURE2D_DESC
        {
            Width = (uint)_surface.Size.Width,
            Height = (uint)_surface.Size.Height,
            Format = PixelFormat,
            ArraySize = 1,
            MipLevels = 1, // to be able to query for IDXGISurface
            SampleDesc = new DXGI_SAMPLE_DESC { Count = 1 },
        };

        using var tex = device.CreateTexture2D<ID3D11Texture2D>(desc);
        using var surfaceInterop = _surface.AsComObject<ICompositionDrawingSurfaceInterop2>();
        var hr = ComObject.WithComInstance(tex, unk => surfaceInterop.Object.CopySurface(unk, 0, 0, 0));
        if (hr.IsError)
            return null;

        var surface = tex.As<IDXGISurface>(throwOnError: true)!;
        IComObject<ID2D1Bitmap1>? bmp = null;
        WithDeviceContext(dc =>
        {
            bmp = dc.CreateBitmapFromDxgiSurface(surface);
        });
        return bmp;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        EnsureTarget();
        OnDraw();
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
            OnResize();
        }
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
        interop.CreateDesktopWindowTarget(Handle, true, out var target).ThrowOnError();
        _target = MarshalInterface<DesktopWindowTarget>.FromAbi(target);

        var root = _graphicsDevice.Value.Compositor.CreateSpriteVisual();
        root.Size = new Vector2(Width, Height);
        _target.Root = root;

        var pf = (DirectXPixelFormat)PixelFormat;
        _surface = _graphicsDevice.Value.CreateDrawingSurface2(new SizeInt32(Width, Height), pf, DirectXAlphaMode.Premultiplied);
        root.Brush = _graphicsDevice.Value.Compositor.CreateSurfaceBrush(_surface);
    }

    protected virtual void OnResize()
    {
        if (_surface == null)
            return;

        _surface.Resize(new SizeInt32(Width, Height));
        OnDraw();
    }

    void ID2DControl.Redraw() => OnDraw();
    protected virtual void OnDraw() => WithDeviceContext(dc => OnDraw(this, new D2DDrawEventArgs(dc)));
    protected virtual void OnDraw(object sender, D2DDrawEventArgs e) => Draw?.Invoke(sender, e);

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
