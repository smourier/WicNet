using System;
using DirectN;

namespace WicNetExplorer
{
    public interface ID2Control : IDisposable
    {
        public event EventHandler<D2DDrawEventArgs>? Draw;
        public event EventHandler<D2DReleaseEventArgs>? Releasing;

        IComObject<ID2D1DeviceContext>? DeviceContext { get; }
        void Invalidate();
    }
}
