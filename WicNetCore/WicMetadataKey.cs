namespace WicNet;

public class WicMetadataKey : IEquatable<WicMetadataKey>
{
    public WicMetadataKey(Guid format, string key)
    {
        if (format == Guid.Empty)
            throw new ArgumentException(null, nameof(format));

        ArgumentNullException.ThrowIfNull(key);
        Format = format;
        Key = key;
    }

    public Guid Format { get; }
    public string Key { get; }
    public WicMetadataHandler? Handler => WicMetadataHandler.FromFormatGuid<WicMetadataHandler>(Format);
    public string? HandlerFriendlyName => WicMetadataHandler.FriendlyNameFromGuid(Format);

    public override string ToString() => Key;
    public override bool Equals(object? obj) => Equals(obj as WicMetadataKey);
    public override int GetHashCode() => Format.GetHashCode() ^ StringComparer.Ordinal.GetHashCode(Key);
    public bool Equals(WicMetadataKey? other) => other != null && Format == other.Format && Key == other.Key;

    public static string? CombineKeys(string? key1, string? key2)
    {
        if (key1 == null)
            return key2;

        if (key2 == null)
            return key1;

        if (key1.EndsWith('/'))
        {
            if (key2.StartsWith('/'))
                return string.Concat(key1, key2.AsSpan(1));
        }
        else if (!key2.StartsWith('/'))
            return key1 + "/" + key2;

        return key1 + key2;
    }
}
