using DirectN;

namespace WicNetExplorer.Model
{
    // *must* match WICBitmapInterpolationMode
    public enum InterpolationMode
    {
        NearestNeighbor = WICBitmapInterpolationMode.WICBitmapInterpolationModeNearestNeighbor,
        Linear = WICBitmapInterpolationMode.WICBitmapInterpolationModeLinear,
        Cubic = WICBitmapInterpolationMode.WICBitmapInterpolationModeCubic,
        Fant = WICBitmapInterpolationMode.WICBitmapInterpolationModeFant,
        HighQualityCubic = WICBitmapInterpolationMode.WICBitmapInterpolationModeHighQualityCubic,
    }
}
