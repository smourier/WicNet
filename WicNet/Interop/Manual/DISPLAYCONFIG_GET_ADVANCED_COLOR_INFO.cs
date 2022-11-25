using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint value;
        public DISPLAYCONFIG_COLOR_ENCODING colorEncoding;
        public uint bitsPerColorChannel;

        public bool advancedColorSupported => (value & 0x1) == 0x1;
        public bool advancedColorEnabled => (value & 0x2) == 0x2;
        public bool wideColorEnforced => (value & 0x4) == 0x4;
        public bool advancedColorForceDisabled => (value & 0x8) == 0x8;
    }
}
