using System;
using System.Collections.Generic;
using System.Linq;

namespace WicNet.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var ext in WicImagingComponent.DecoderFileExtensions)
            {
                Console.WriteLine(ext + " " + WicDecoder.FromFileExtension(ext));
            }

            using (var bmp = WicBitmapSource.Load(@"SamsungSGH-I777.jpg"))
            {
                var reader = bmp.GetMetadataReader();
                //var th = bmp.GetThumbnail();

                var policies = new WicMetadataPolicies(reader);

                foreach (var kv in reader.Enumerate(true))
                {
                    Console.WriteLine(kv.Key + "=" + kv.Value);
                }
            }
        }
    }
}
