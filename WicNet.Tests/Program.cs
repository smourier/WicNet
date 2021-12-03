using System;
using System.Collections.Generic;
using System.Linq;
using WicNet.Interop;

namespace WicNet.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            //foreach (var ext in WicImagingComponent.DecoderFileExtensions)
            //{
            //    Console.WriteLine(ext + " " + WicDecoder.FromFileExtension(ext));
            //}
            //return;

            using (var bmp = WicBitmapSource.Load(@"SamsungSGH-I777.jpg"))
            {
                // get metadata source (reader)
                var reader = bmp.GetMetadataReader();
                
                //var th = bmp.GetThumbnail();

                // gt policies (well known Windows metadata)
                var policies = new WicMetadataPolicies(reader);

                // dump metadata to console
                reader.Visit((r, kv) =>
                {
                    var value = kv.Value;
                    var type = value != null ? value.GetType().Name : null;
                    if (value is byte[] bytes)
                    {
                        value = Conversions.ToHexa(bytes, 64) + " (" + bytes.Length + ")";
                    }

                    Console.WriteLine(WicMetadataKey.CombineKeys(r.Location, kv.Key.Key) + " [" + type + "/" + kv.Type + "]= " + value);
                });

                // save with metadata
                bmp.Save("copy.jpg", metadata: reader);
            }
        }
    }
}
