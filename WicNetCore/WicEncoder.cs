using System;
using System.IO;
using System.Linq;
using DirectNAot.Extensions.Utilities;

namespace WicNet;

public sealed class WicEncoder(object comObject) : WicCodec(comObject)
{

    // they are supposed to have the same container format
    public static WicEncoder? FromDecoder(WicDecoder decoder) => decoder != null ? FromContainerFormatGuid(decoder.ContainerFormat) : null;

    public static WicEncoder? FromFileExtension(string? ext)
    {
        if (ext == null)
            return null;

        if (!ext.StartsWith('.'))
        {
            ext = Path.GetExtension(ext);
        }

        return AllComponents.OfType<WicEncoder>().FirstOrDefault(e => e.SupportsFileExtension(ext));
    }

    public static WicEncoder? FromMimeType(string? mimeType)
    {
        if (mimeType == null)
            return null;

        return AllComponents.OfType<WicEncoder>().FirstOrDefault(c => c.SupportsMimeType(mimeType));
    }

    public static WicEncoder? FromName(string? name)
    {
        if (name == null)
            return null;

        var item = AllComponents.OfType<WicEncoder>().FirstOrDefault(c => c.FriendlyName.EqualsIgnoreCase(name));
        if (item != null)
            return item;

        return AllComponents.OfType<WicEncoder>().FirstOrDefault(f => f.FriendlyName.ToLowerInvariant().Replace("encoder", "").Trim().EqualsIgnoreCase(name));
    }

    public static WicEncoder? FromContainerFormatGuid(Guid guid) => FromContainerFormatGuid<WicEncoder>(guid);
}
