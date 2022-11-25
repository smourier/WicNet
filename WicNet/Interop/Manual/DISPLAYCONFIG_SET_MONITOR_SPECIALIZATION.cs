using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_SET_MONITOR_SPECIALIZATION
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint value;
        public Guid specializationType;
        public Guid specializationSubType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string specializationApplicationName;

        public bool isSpecializationEnabled => (value & 0x1) == 0x1;
    }
}
