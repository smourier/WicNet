namespace WicNet;

public sealed class WicBitmapDecoder(IComObject<IWICBitmapDecoder> comObject) : InterlockedComObject<IWICBitmapDecoder>(comObject), IEnumerable<WicBitmapSource>
{
    public uint FrameCount => NativeObject.GetFrameCount();
    public Guid ContainerFormat => NativeObject.GetContainerFormat();

    public WicBitmapSource GetFrame(uint index = 0) => new(NativeObject.GetFrame(index));

    public WicBitmapSource? GetPreview()
    {
        var bmp = NativeObject.GetPreview();
        return bmp != null ? new WicBitmapSource(bmp) : null;
    }

    public WicBitmapSource? GetThumbnail()
    {
        var bmp = NativeObject.GetThumbnail();
        return bmp != null ? new WicBitmapSource(bmp) : null;
    }

    public WicMetadataQueryReader? GetMetadataQueryReader()
    {
        var reader = NativeObject.GetMetadataQueryReader();
        return reader != null ? new WicMetadataQueryReader(reader) : null;
    }

    public IReadOnlyList<WicColorContext> GetColorContexts()
    {
        var list = new List<WicColorContext>();
        var contexts = NativeObject.GetColorContexts();
        list.AddRange(contexts.Select(cc => new WicColorContext(cc)));
        return list;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<WicBitmapSource> GetEnumerator() => EnumerateFrames().GetEnumerator();

    public IEnumerable<WicBitmapSource> EnumerateFrames()
    {
        for (uint i = 0; i < FrameCount; i++)
        {
            yield return GetFrame(i);
        }
    }

    public static WicBitmapDecoder Load(string filePath, Guid? guidVendor = null, FileAccess access = FileAccess.Read, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException(null, filePath);

        return new WicBitmapDecoder(WicImagingFactory.CreateDecoderFromFilename(filePath, guidVendor, access, metadataOptions: options));
    }

    public static WicBitmapDecoder Load(nuint fileHandle, Guid? guidVendor = null, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        if (fileHandle == 0)
            throw new ArgumentNullException(nameof(fileHandle));

        return new WicBitmapDecoder(WicImagingFactory.CreateDecoderFromFileHandle(fileHandle, guidVendor, options));
    }

    public static WicBitmapDecoder Load(Stream stream, Guid? guidVendor = null, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return new WicBitmapDecoder(WicImagingFactory.CreateDecoderFromStream(stream, guidVendor, options));
    }

    public static WicBitmapDecoder Load(IStream stream, Guid? guidVendor = null, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return new WicBitmapDecoder(WicImagingFactory.CreateDecoderFromStream(stream, guidVendor, options));
    }
}
