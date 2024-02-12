using System;
using DirectN;

namespace WicNetExplorer
{
    public interface ID2DControl : IDisposable
    {
        public event EventHandler<D2DDrawEventArgs>? Draw;

        void Redraw();
        void WithDeviceContext(Action<IComObject<ID2D1DeviceContext>> action);
    }
}
