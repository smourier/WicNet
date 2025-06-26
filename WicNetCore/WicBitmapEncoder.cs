namespace WicNet;

public sealed class WicBitmapEncoder(IComObject<IWICBitmapEncoder> comObject) : InterlockedComObject<IWICBitmapEncoder>(comObject)
{
    public Guid ContainerFormat => NativeObject.GetContainerFormat();

    public WicMetadataQueryWriter? GetMetadataQueryWriter()
    {
        var writer = NativeObject.GetMetadataQueryWriter();
        return writer != null ? new WicMetadataQueryWriter(writer) : null;
    }

    public WicBitmapFrameEncode CreateNewFrame() => NativeObject.CreateNewFrame();
    public void Commit() => NativeObject.Commit();
    public void SetPreview(WicBitmapSource source) => NativeObject.SetPreview(source.NativeObject);
    public void SetThumbnail(WicBitmapSource source) => NativeObject.SetThumbnail(source.NativeObject);
    public void SetThumbnail(WicPalette palette) => NativeObject.SetPalette(palette.NativeObject);
    public void SetColorContexts(IEnumerable<WicColorContext> contexts)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        NativeObject.SetColorContexts(contexts.Select(c => c.ComObject.Object));
    }

    public static WicBitmapEncoder Load(Guid guidContainerFormat, Guid? guidVendor = null) => new(WicImagingFactory.CreateEncoder(guidContainerFormat, guidVendor));
}
