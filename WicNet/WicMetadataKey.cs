using System;

namespace WicNet
{
    public class WicMetadataKey : IEquatable<WicMetadataKey>
    {
        public WicMetadataKey(Guid format, string key)
        {
            if (format == Guid.Empty)
                throw new ArgumentException(null, nameof(format));

            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Format = format;
            Key = key;
        }

        public Guid Format { get; }
        public string Key { get; }
        public WicMetadataHandler Handler => WicMetadataHandler.FromFormatGuid<WicMetadataHandler>(Format);
        public string HandlerFriendlyName => WicMetadataHandler.FriendlyNameFromGuid(Format);

        public override string ToString() => HandlerFriendlyName + " " + Key;
        public override bool Equals(object obj) => Equals(obj as WicMetadataKey);
        public override int GetHashCode() => Format.GetHashCode() ^ StringComparer.Ordinal.GetHashCode(Key);
        public bool Equals(WicMetadataKey other) => other != null && Format == other.Format && Key == other.Key;
    }
}
