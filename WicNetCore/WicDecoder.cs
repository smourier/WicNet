namespace WicNet;

public sealed class WicDecoder(IComObject<IWICBitmapDecoderInfo> comObject) : WicCodec(comObject)
{
    public WicBitmapDecoder CreateInstance() => WicImagingFactory.WithFactory(f =>
    {
        f.Object.CreateComponentInfo(Clsid, out var info).ThrowOnError();
        var decoderInfo = (IWICBitmapDecoderInfo)info;
        decoderInfo.CreateInstance(out var decoder).ThrowOnError();
        return new WicBitmapDecoder(new ComObject<IWICBitmapDecoder>(decoder));
    });

    // they are supposed to have the same container format
    public static WicDecoder? FromEncoder(WicEncoder encoder) => encoder != null ? FromContainerFormatGuid(encoder.ContainerFormat) : null;

    public static WicDecoder? FromFileExtension(string? ext)
    {
        if (ext == null)
            return null;

        if (!ext.StartsWith('.'))
        {
            ext = Path.GetExtension(ext);
        }

        return AllComponents.OfType<WicDecoder>().FirstOrDefault(e => e.SupportsFileExtension(ext));
    }

    public static WicDecoder? FromMimeType(string? mimeType)
    {
        if (mimeType == null)
            return null;

        return AllComponents.OfType<WicDecoder>().FirstOrDefault(c => c.SupportsMimeType(mimeType));
    }

    public static WicDecoder? FromName(string? name)
    {
        if (name == null)
            return null;

        var item = AllComponents.OfType<WicDecoder>().FirstOrDefault(c => c.FriendlyName.EqualsIgnoreCase(name));
        if (item != null)
            return item;

        return AllComponents.OfType<WicDecoder>().FirstOrDefault(f => f.FriendlyName?.ToLowerInvariant()?.Replace("decoder", string.Empty).EqualsIgnoreCase(name) == true);
    }

    public static WicDecoder? FromContainerFormatGuid(Guid guid) => FromContainerFormatGuid<WicDecoder>(guid);
}
