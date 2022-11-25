using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_TARGET_PREFERRED_MODE
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint width;
        public uint height;
        public DISPLAYCONFIG_TARGET_MODE targetMode;

        public override string ToString() => width + "x" + height;
    }
}
