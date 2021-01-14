using System;

namespace WicNet
{
    [Flags]
    public enum WicBitmapScaleOptions
    {
        None = 0x0,
        DownOnly = 0x1,

        Default = DownOnly,
    }
}
