using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_SET_TARGET_PERSISTENCE
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint value;

        public bool bootPersistenceOn => (value & 0x1) == 0x1;
    }
}
