using System;
using DirectN;

namespace WicNetExplorer;

public class D2DDrawEventArgs : EventArgs
{
    public D2DDrawEventArgs(IComObject<ID2D1DeviceContext> deviceContext)
    {
        ArgumentNullException.ThrowIfNull(deviceContext);
        DeviceContext = deviceContext;
    }

    public IComObject<ID2D1DeviceContext> DeviceContext { get; }
}
