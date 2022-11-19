using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using DirectN;

namespace WicNetExplorer
{
    public class D2DControl : Control
    {
        private IComObject<ID2D1HwndRenderTarget>? _target;

        public event EventHandler<D2DDrawEventArgs>? Draw;
        public event EventHandler<D2DReleaseEventArgs>? Releasing;

        public D2DControl()
        {
        }

        protected virtual bool IsValidRenderTarget => _target != null && !_target.IsDisposed;
        public IComObject<ID2D1HwndRenderTarget>? Target => _target;
        public IReadOnlyList<DXGI_FORMAT> SupportedDxgiFormats
        {
            get
            {
                var target = Target;
                if (target == null)
                    return Array.Empty<DXGI_FORMAT>();

                using (var dc = target.AsComObject<ID2D1DeviceContext>())
                {
                    var list = new List<DXGI_FORMAT>();
                    foreach (DXGI_FORMAT format in Enum.GetValues(typeof(DXGI_FORMAT)))
                    {
                        if (dc.Object.IsDxgiFormatSupported(format))
                        {
                            list.Add(format);
                        }
                    }
                    return list.AsReadOnly();
                }
            }
        }

        protected virtual void ReleaseRenderTarget()
        {
            if (_target != null)
            {
                OnReleasing(this, new D2DReleaseEventArgs(_target));
            }
            Interlocked.Exchange(ref _target, null)?.Dispose();
        }

        protected virtual void CreateRenderTarget()
        {
            ReleaseRenderTarget();

#if DEBUG
            var level = D2D1_DEBUG_LEVEL.D2D1_DEBUG_LEVEL_INFORMATION;
#else
            var level = D2D1_DEBUG_LEVEL.D2D1_DEBUG_LEVEL_NONE;
#endif
            using var fac = D2D1Functions.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED, new D2D1_FACTORY_OPTIONS { debugLevel = level });

            _target = fac.CreateHwndRenderTarget(new D2D1_HWND_RENDER_TARGET_PROPERTIES { hwnd = Handle, pixelSize = new D2D_SIZE_U(Width, Height) });
        }

        protected virtual void EnsureRenderTarget()
        {
            if (!IsValidRenderTarget)
            {
                CreateRenderTarget();
            }
        }

        protected virtual void ResizeRenderTarget()
        {
            if (IsValidRenderTarget)
            {
                var d2dsize = new D2D_SIZE_U(Width, Height);
                var hr = _target!.Object.Resize(ref d2dsize);
                if (hr.IsError)
                {
                    ReleaseRenderTarget();
                }
            }
            Invalidate();
        }

        protected virtual void OnReleasing(object sender, D2DReleaseEventArgs e) => Releasing?.Invoke(sender, e);
        protected virtual void OnDraw(object sender, D2DDrawEventArgs e) => Draw?.Invoke(sender, e);
        protected virtual void OnDraw(IComObject<ID2D1HwndRenderTarget> target)
        {
            target.Clear(_D3DCOLORVALUE.FromColor(BackColor));
        }

        protected sealed override void OnPaintBackground(PaintEventArgs pevent)
        {
            // use Clear (or not)
        }

        protected sealed override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            EnsureRenderTarget();
            if (_target != null && !_target.IsDisposed && !_target.CheckWindowState().HasFlag(D2D1_WINDOW_STATE.D2D1_WINDOW_STATE_OCCLUDED))
            {
                _target.BeginDraw();
                try
                {
                    var ed = new D2DDrawEventArgs(_target);
                    OnDraw(this, ed);
                    if (ed.Handled)
                        return;

                    OnDraw(_target);
                }
                finally
                {
                    var hr = _target.Object.EndDraw(IntPtr.Zero, IntPtr.Zero);
                    if (hr == HRESULTS.D2DERR_RECREATE_TARGET)
                    {
                        ReleaseRenderTarget();
                        Invalidate();
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeRenderTarget();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ReleaseRenderTarget();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            ReleaseRenderTarget();
        }
    }
}
