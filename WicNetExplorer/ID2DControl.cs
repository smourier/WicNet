using System;
using DirectN;

namespace WicNetExplorer
{
    public interface ID2DControl : IDisposable
    {
        public event EventHandler<D2DDrawEventArgs>? Draw;

        IComObject<ID2D1Bitmap1>? GetSurfaceBitmap();
        void Redraw();
        void WithDeviceContext(Action<IComObject<ID2D1DeviceContext>> action);
    }
}
