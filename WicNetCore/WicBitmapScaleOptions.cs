namespace WicNet;

[Flags]
public enum WicBitmapScaleOptions
{
    None = 0x0,
    DownOnly = 0x1,
    UpOnly = 0x2,
    Uniform = 0x4,
    UniformToFill = 0x8,
    UseWidthAndHeight = 0x10,

    Default = Uniform,
}
