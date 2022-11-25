using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_GET_MONITOR_SPECIALIZATION
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint value;

        public bool isSpecializationEnabled => (value & 0x1) == 0x1;
        public bool isSpecializationAvailableForMonitor => (value & 0x2) == 0x2;
        public bool isSpecializationAvailableForSystem => (value & 0x4) == 0x4;
    }
}
