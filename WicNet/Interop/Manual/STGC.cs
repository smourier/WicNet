using System;

namespace WicNet.Interop.Manual
{
    [Flags]
    public enum STGC
    {
        STGC_DEFAULT = 0x0,
        STGC_OVERWRITE = 0x1,
        STGC_ONLYIFCURRENT = 0x2,
        STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 0x4,
        STGC_CONSOLIDATE = 0x8
    }
}
