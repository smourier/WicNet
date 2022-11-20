using System;
using System.ComponentModel;
using DirectN;

namespace WicNetExplorer
{
    public class D2DDrawEventArgs : HandledEventArgs
    {
        public D2DDrawEventArgs(IComObject<ID2D1DeviceContext> target)
        {
            ArgumentNullException.ThrowIfNull(target);
            Target = target;
        }

        public IComObject<ID2D1DeviceContext> Target { get; }
    }
}
