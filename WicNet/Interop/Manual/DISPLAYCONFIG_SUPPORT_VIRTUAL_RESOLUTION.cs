using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint value;

        public bool disableMonitorVirtualResolution => (value & 0x1) == 0x1;
    }
}
