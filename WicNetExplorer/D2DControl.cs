using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using DirectN;

namespace WicNetExplorer
{
    public class D2DControl : Control, ID2Control
    {
        private IComObject<ID2D1HwndRenderTarget>? _target;
        private IComObject<ID2D1DeviceContext>? _dc;

        public event EventHandler<D2DDrawEventArgs>? Draw;
        public event EventHandler<D2DReleaseEventArgs>? Releasing;

        public D2DControl()
        {
        }

        protected virtual bool IsValidTarget => _target != null && !_target.IsDisposed;
        public IComObject<ID2D1DeviceContext>? DeviceContext => _dc;
        public IReadOnlyList<DXGI_FORMAT> SupportedDxgiFormats
        {
            get
            {
                var target = DeviceContext;
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

        protected virtual void ReleaseTarget()
        {
            if (_dc != null)
            {
                OnReleasing(this, new D2DReleaseEventArgs(_dc));
            }
            Interlocked.Exchange(ref _dc, null)?.Dispose();
            Interlocked.Exchange(ref _target, null)?.Dispose();
        }

        protected virtual void CreateTarget()
        {
            ReleaseTarget();

#if DEBUG
            var level = D2D1_DEBUG_LEVEL.D2D1_DEBUG_LEVEL_INFORMATION;
#else
            var level = D2D1_DEBUG_LEVEL.D2D1_DEBUG_LEVEL_NONE;
#endif
            using var fac = D2D1Functions.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED, new D2D1_FACTORY_OPTIONS { debugLevel = level });

            _target = fac.CreateHwndRenderTarget(new D2D1_HWND_RENDER_TARGET_PROPERTIES { hwnd = Handle, pixelSize = new D2D_SIZE_U(Width, Height) });
            _dc = _target.AsComObject<ID2D1DeviceContext>(true);
        }

        protected virtual void EnsureTarget()
        {
            if (!IsValidTarget)
            {
                CreateTarget();
            }
        }

        protected virtual void ResizeTarget()
        {
            if (IsValidTarget)
            {
                var d2dsize = new D2D_SIZE_U(Width, Height);
                var hr = _target!.Object.Resize(ref d2dsize);
                if (hr.IsError)
                {
                    ReleaseTarget();
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
            EnsureTarget();
            if (_target != null && !_target.IsDisposed && !_target.CheckWindowState().HasFlag(D2D1_WINDOW_STATE.D2D1_WINDOW_STATE_OCCLUDED))
            {
                _target.BeginDraw();
                try
                {
                    var ed = new D2DDrawEventArgs(_dc!);
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
                        ReleaseTarget();
                        Invalidate();
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResizeTarget();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ReleaseTarget();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            ReleaseTarget();
        }
    }
}
