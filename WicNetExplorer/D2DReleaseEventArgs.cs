using System;
using DirectN;

namespace WicNetExplorer
{
    public class D2DReleaseEventArgs : EventArgs
    {
        public D2DReleaseEventArgs(IComObject<ID2D1HwndRenderTarget> target)
        {
            ArgumentNullException.ThrowIfNull(target);
            Target = target;
        }

        public IComObject<ID2D1HwndRenderTarget> Target { get; }
    }
}
