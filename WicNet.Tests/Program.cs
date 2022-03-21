using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DirectN;

namespace WicNet.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Histogram();
            //Grayscale();
            //CopyGif();
            //DrawEllipse();
        }

        static void Histogram()
        {
            using (var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg"))
            using (var converted = bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA))
            using (var clone = converted.Clone())
            using (var newBmp = new WicBitmapSource(bmp.Width, bmp.Height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA))
            using (var rt = newBmp.CreateDeviceContext())
            using (var cb = rt.CreateBitmapFromWicBitmap(clone.AsBitmap()))
            using (var fx = rt.CreateEffect(Direct2DEffects.CLSID_D2D1Histogram))
            {
                fx.SetInput(0, cb);
                rt.BeginDraw();
                rt.DrawImage(fx);
                rt.EndDraw();

                var floats = fx.GetValue<float[]>("HistogramOutput", null);
                foreach (var o in floats)
                {
                    Console.WriteLine(o);
                }
            }
        }

        static void Grayscale()
        {
            using (var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg"))
            using (var converted = bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA))
            using (var clone = converted.Clone())
            using (var newBmp = new WicBitmapSource(bmp.Width, bmp.Height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA))
            using (var rt = newBmp.CreateDeviceContext())
            using (var fx = rt.CreateEffect(Direct2DEffects.CLSID_D2D1Grayscale))
            using (var cb = rt.CreateBitmapFromWicBitmap(clone.AsBitmap()))
            {
                fx.SetInput(0, cb);
                rt.BeginDraw();
                rt.DrawImage(fx);
                rt.EndDraw();
                newBmp.Save("gray.jpg");
            }
        }

        static void DrawEllipse()
        {
            using (var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg"))
            {
                using (var converted = bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPRGBA))
                {
                    var width = 200;
                    var height = width * bmp.Height / bmp.Width;
                    using (var clone = new WicBitmapSource(width, height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA))
                    using (var rt = clone.CreateDeviceContext())
                    using (var dbmp = rt.CreateBitmapFromWicBitmap(converted.ComObject))
                    using (var brush = rt.CreateSolidColorBrush(_D3DCOLORVALUE.Red))
                    {
                        rt.BeginDraw();
                        rt.DrawBitmap(dbmp, destinationRectangle: new D2D_RECT_F(new D2D_SIZE_F(clone.Size)));
                        rt.DrawEllipse(new D2D1_ELLIPSE(width / 2, height / 2, Math.Min(width, height) / 2), brush, 4);
                        rt.EndDraw();
                        clone.Save("ellipse.jpg");
                    }
                }
            }
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
