using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Histograms();
            //foreach (var f in GetHistogram("SamsungSGH-P270.jpg"))
            //{
            //    Console.WriteLine(f);
            //}
            //RotateAndGrayscale();
            //CopyGif();
            //DrawEllipse();
        }

        static float distance1(float[] h1, float[] h2)
        {
            var distance = 0f;
            for (var i = 0; i < h1.Length; i++)
            {
                var diff = h2[i] - h1[i];
                distance += diff * diff;
            }
            return (float)Math.Sqrt(distance);
        }

        static void Histograms(int thumbSize = 100)
        {
            var hists = new List<Tuple<string, float[]>>();
            using (var memBmp = new WicBitmapSource(thumbSize, thumbSize, WicPixelFormat.GUID_WICPixelFormat32bppBGR))
            using (var dc = memBmp.CreateDeviceContext())
            using (var fx = dc.CreateEffect(Direct2DEffects.CLSID_D2D1Histogram))
            {
                foreach (var file in Directory.EnumerateFiles(@"d:\temp\", "*.*", SearchOption.TopDirectoryOnly))
                {
                    var ext = Path.GetExtension(file);
                    if (!WicImagingComponent.DecoderFileExtensions.Contains(ext))
                        continue;

                    if (new FileInfo(file).Length == 0)
                        continue;

                    Console.WriteLine(file);
                    var hist = GetHistogram(file, dc, fx);
                    hists.Add(new Tuple<string, float[]>(file, hist));
                    //if (hists.Count > 10)
                    //    break;
                }
            }

            for (var i = 0; i < hists.Count; i++)
            {
                Console.WriteLine(hists[i].Item1);
                for (var j = 0; j < hists.Count; j++)
                {
                    var dist1 = distance1(hists[i].Item2, hists[j].Item2);
                    Console.WriteLine(" " + dist1 + " " + hists[j].Item1);
                }
            }
        }


        static float[] GetHistogram(string filePath, int thumbSize = 100)
        {
            using (var memBmp = new WicBitmapSource(thumbSize, thumbSize, WicPixelFormat.GUID_WICPixelFormat32bppBGR))
            using (var dc = memBmp.CreateDeviceContext())
            using (var fx = dc.CreateEffect(Direct2DEffects.CLSID_D2D1Histogram))
                return GetHistogram(filePath, dc, fx);
        }

        static float[] GetHistogram(string filePath, IComObject<ID2D1DeviceContext> dc, IComObject<ID2D1Effect> fx)
        {
            using (var bmp = WicBitmapSource.Load(filePath))
            {
                bmp.Scale(100, null);
                bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
                using (var cb = dc.CreateBitmapFromWicBitmap(bmp.ComObject))
                {
                    fx.SetInput(0, cb);
                    dc.BeginDraw();
                    dc.DrawImage(fx);
                    dc.EndDraw();
                    return fx.GetValue<float[]>("HistogramOutput", null);
                }
            }
        }

        static void RotateAndGrayscale()
        {
            using (var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg"))
            {
                bmp.Rotate(WICBitmapTransformOptions.WICBitmapTransformRotate90);
                bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
                using (var newBmp = new WicBitmapSource(bmp.Width, bmp.Height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA))
                using (var rt = newBmp.CreateDeviceContext())
                using (var fx = rt.CreateEffect(Direct2DEffects.CLSID_D2D1Grayscale))
                using (var cb = rt.CreateBitmapFromWicBitmap(bmp.ComObject))
                {
                    fx.SetInput(0, cb);
                    rt.BeginDraw();
                    rt.DrawImage(fx);
                    rt.EndDraw();
                    newBmp.Save("gray.jpg");
                    Process.Start(new ProcessStartInfo("gray.jpg") { UseShellExecute = true });
                }
            }
        }

        static void DrawEllipse()
        {
            using (var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg"))
            {
                bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
                var width = 200;
                var height = width * bmp.Height / bmp.Width;
                using (var memBmp = new WicBitmapSource(width, height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA))
                using (var rt = memBmp.CreateDeviceContext())
                using (var dbmp = rt.CreateBitmapFromWicBitmap(bmp.ComObject))
                using (var brush = rt.CreateSolidColorBrush(_D3DCOLORVALUE.Red))
                {
                    rt.BeginDraw();
                    rt.DrawBitmap(dbmp, destinationRectangle: new D2D_RECT_F(new D2D_SIZE_F(memBmp.Size)));
                    rt.DrawEllipse(new D2D1_ELLIPSE(width / 2, height / 2, Math.Min(width, height) / 2), brush, 4);
                    rt.EndDraw();
                    memBmp.Save("ellipse.jpg");
                    Process.Start(new ProcessStartInfo("ellipse.jpg") { UseShellExecute = true });
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
