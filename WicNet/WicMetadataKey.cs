using System;
using System.Collections.Generic;

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

        public override string ToString() => "[" + WicMetadataHandler.FriendlyNameFromGuid(Format) + "] '" + Key + "'";
        public override bool Equals(object obj) => Equals(obj as WicMetadataKey);
        public override int GetHashCode() => Format.GetHashCode() ^ StringComparer.Ordinal.GetHashCode(Key);
        public bool Equals(WicMetadataKey other) => other != null && Format == other.Format && Key == other.Key;

        public static void CopyMetadata(IEnumerable<KeyValuePair<WicMetadataKey, object>> source, IDictionary<WicMetadataKey, object> target)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            foreach (var kv in source)
            {
                if (kv.Value is IEnumerable<KeyValuePair<WicMetadataKey, object>> child)
                {
                    var dic = new Dictionary<WicMetadataKey, object>();
                    target[kv.Key] = dic;
                    CopyMetadata(child, dic);
                }
                else
                {
                    target[kv.Key] = kv.Value;
                }
            }
        }

        public static void VisitMetadata(IEnumerable<KeyValuePair<WicMetadataKey, object>> source, Action<KeyValuePair<WicMetadataKey, object>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (source == null)
                return;

            foreach (var kv in source)
            {
                func(kv);
                if (kv.Value is IEnumerable<KeyValuePair<WicMetadataKey, object>> child)
                {
                    VisitMetadata(child, func);
                }
            }
        }
    }
}
