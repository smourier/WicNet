using System.ComponentModel;

namespace WicNetExplorer.Model;

public enum ExifColorSpace
{
    [Description("")]
    None = 0,
    sRGB = 1,

    [Description("Adobe RGB")]
    AdobeRGB = 2,
    Uncalibrated = 0xFFFF
}
