using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
    {
        public ulong pixelRate;
        public DISPLAYCONFIG_RATIONAL hSyncFreq;
        public DISPLAYCONFIG_RATIONAL vSyncFreq;
        public DISPLAYCONFIG_2DREGION activeSize;
        public DISPLAYCONFIG_2DREGION totalSize;
        public uint value;
        public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;

        public D3DKMDT_VIDEO_SIGNAL_STANDARD videoStandard => (D3DKMDT_VIDEO_SIGNAL_STANDARD)(value & 0xFF);
        public uint vSyncFreqDivider => (value >> 16) & 0x3F;
    }
}
