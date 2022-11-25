using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_TARGET_BASE_TYPE
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY baseOutputTechnology;
    }
}
