using System;
using System.Collections.Generic;
using System.Linq;
using WicNet;

namespace DirectN
{
    public static class IWICMetadataQueryWriterExtensions
    {
        public static void EncodeMetadata(this IComObject<IWICMetadataQueryWriter> writer, IEnumerable<WicMetadataKeyValue> metadata) => EncodeMetadata(writer?.Object, metadata);
        public static void EncodeMetadata(this IWICMetadataQueryWriter writer, IEnumerable<WicMetadataKeyValue> metadata)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (metadata == null)
                return;

            if (!metadata.Any())
                return;

            WICImagingFactory.WithFactory(factory =>
            {
                foreach (var kv in metadata)
                {
                    if (kv.Value is IEnumerable<WicMetadataKeyValue> childMetadata)
                    {
                        if (!childMetadata.Any())
                            continue;

                        factory.CreateQueryWriter(kv.Key.Format, IntPtr.Zero, out var childWriter).ThrowOnError();
                        using (var pv = new PropVariant(childWriter))
                        {
                            var hr = writer.SetMetadataByName(kv.Key.Key, pv).ThrowOnError();
                        }
                        EncodeMetadata(childWriter, childMetadata);
                    }
                    else
                    {
                        using (var pv = new PropVariant(kv.Value, kv.Type))
                        {
                            var hr = writer.SetMetadataByName(kv.Key.Key, pv).ThrowOnError();
                        }
                    }
                }
            });
        }

        public static void SetMetadataByName(this IComObject<IWICMetadataQueryWriter> writer, string name, object value, PropertyType? type = null) => SetMetadataByName(writer?.Object, name, value, type);
        public static void SetMetadataByName(this IWICMetadataQueryWriter writer, string name, object value, PropertyType? type = null)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            using (var pv = new PropVariant(value, type))
            {
                writer.SetMetadataByName(name, pv).ThrowOnError();
            }
        }

        public static void SetMetadataByName(this IComObject<IWICMetadataQueryWriter> writer, string name, PropVariant pv) => SetMetadataByName(writer?.Object, name, pv);
        public static void SetMetadataByName(this IWICMetadataQueryWriter writer, string name, PropVariant pv)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (pv == null)
                throw new ArgumentNullException(nameof(pv));

            writer.SetMetadataByName(name, pv).ThrowOnError();
        }

        public static void RemoveMetadataByName(this IComObject<IWICMetadataQueryWriter> writer, string name) => RemoveMetadataByName(writer?.Object, name);
        public static void RemoveMetadataByName(this IWICMetadataQueryWriter writer, string name)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            writer.RemoveMetadataByName(name).ThrowOnError();
        }
    }
}
