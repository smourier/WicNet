﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace WicNet.Interop
{
    public static class IWICMetadataQueryWriterExtensions
    {
        public static void EncodeMetadata(this IComObject<IWICMetadataQueryWriter> writer, IEnumerable<KeyValuePair<WicMetadataKey, object>> metadata) => EncodeMetadata(writer?.Object, metadata);
        public static void EncodeMetadata(this IWICMetadataQueryWriter writer, IEnumerable<KeyValuePair<WicMetadataKey, object>> metadata)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (metadata?.Any() == false)
                return;

            WICImagingFactory.WithFactory(factory =>
            {
                foreach (var kv in metadata)
                {
                    if (kv.Value is IEnumerable<KeyValuePair<WicMetadataKey, object>> childMetadata)
                    {
                        if (!childMetadata.Any())
                            continue;

                        factory.CreateQueryWriter(kv.Key.Format, IntPtr.Zero, out var childWriter).ThrowOnError();
                        using (var pv = new PropVariant(childWriter))
                        {
                            writer.SetMetadataByName(kv.Key.Key, pv).ThrowOnError();
                        }
                        EncodeMetadata(childWriter, childMetadata);
                    }
                    else
                    {
                        using (var pv = new PropVariant(kv.Value))
                        {
                            writer.SetMetadataByName(kv.Key.Key, pv).ThrowOnError();
                        }
                    }
                }
            });
        }
    }
}
