using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WicNet.Interop;
using WicNet.Interop.Manual;

namespace WicNet.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            CopyGif();
        }

        static void CopyGif()
        {
            using (var dec = WicBitmapDecoder.Load(@"source.gif"))
            {
                var reader = dec.GetMetadataQueryReader();
                Dump(reader);
                Console.WriteLine();

                foreach (var frame in dec)
                {
                    Console.WriteLine(frame.Size);

                    reader = frame.GetMetadataReader();
                    Dump(reader);
                    Console.WriteLine();
                }


                using (var encoder = WICImagingFactory.CreateEncoder(dec.ContainerFormat))
                {
                    using (var file = File.OpenWrite("test.gif"))
                    {
                        var mis = new ManagedIStream(file);
                        encoder.Initialize(mis);

                        foreach (var frame in dec)
                        {
                            var newFrame = encoder.CreateNewFrame();
                            newFrame.Initialize();

                            var md = frame.GetMetadataReader().Enumerate();
                            using (var writer = newFrame.GetMetadataQueryWriter())
                            {
                                writer.EncodeMetadata(md);
                                
                                // change delay here
                                writer.SetMetadataByName("/grctlext/Delay", (ushort)5);
                            }

                            if (frame.Palette != null)
                            {
                                newFrame.SetPalette(frame.Palette.ComObject);
                            }

                            newFrame.WriteSource(frame.ComObject);
                            newFrame.Item1.Commit();
                        }
                        encoder.Commit();
                    }
                }
            }
        }

        static void DumpComponents()
        {
            foreach (var ext in WicImagingComponent.EncoderFileExtensions)
            {
                Console.WriteLine(ext + " " + WicDecoder.FromFileExtension(ext));
            }
        }

        static void Dump(string path)
        {
            using (var dec = WicBitmapDecoder.Load(path))
            {
                var reader = dec.GetMetadataQueryReader();
                Dump(reader);
                Console.WriteLine();

                foreach (var frame in dec)
                {
                    Console.WriteLine(frame.Size);

                    reader = frame.GetMetadataReader();
                    Dump(reader);
                    Console.WriteLine();
                }
            }
        }

        static void Dump(WicMetadataQueryReader reader)
        {
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
        }

        static void CopyFile()
        {
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
