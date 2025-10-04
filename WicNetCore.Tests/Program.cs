using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using DirectN;
using DirectN.Extensions;
using DirectN.Extensions.Com;
using DirectN.Extensions.Utilities;
using WicNet;
using WicNet.Utilities;

[assembly: SupportedOSPlatform("windows8.0")]

namespace WicNetCore.Tests;

#pragma warning disable CS0162 // Unreachable code detected
internal class Program
{
    static void Main()
    {
        DumpLocation("CopyrightInFotos-MdTest01a.jpg");
        return;
        DumpAllComponents();
        DumpApp1Gps("ski.jpg");
        BuildTransparentBitmap(300, 100);
        BuildCrop();
        BuildStraighten();
        BuildBlur(20);
        BuildTurbulences();
        RotateAndGrayscale();
        Posterize(256);
        ToTiff("file_example_TIFF_1MB.tiff");
        LoadFromIcon();
        LoadAndScale();
        ConvertSvgAsPng("tiger.svg");
        DumpAllComponentsPossibleConversions();
        BuildAtlasWithCPU();
        BuildAtlasWithGPU();
        DrawEllipse();
        DumpPossibleWicBitmapRenderTargetFormats();
        TryVariousConversions();
        CopyFile();
        ExtractGif();
        ConvertToBW();
        DumpAllFormats();
        CopyWithColorContext();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    static void DumpLocation(string path)
    {
        using var bmp = WicBitmapSource.Load(path);
        using var r = bmp.GetMetadataReader();
        if (r == null)
            return;

        var c = r.GetMetadataByName<object>("/app13/irb/8bimiptc/iptc/{str=Country}");

        var obj = r.GetMetadataByName<object>("/app13/irb/8bimiptc");
        if (obj is not IWICMetadataQueryReader)
            return;

        using var qr = new ComObject<IWICMetadataQueryReader>(obj);
        using var reader = new WicMetadataQueryReader(qr);

        Dump(reader);
    }

    static void DumpApp1Gps(string path)
    {
        using var bmp = WicBitmapSource.Load(path);
        using var reader = bmp.GetMetadataReader();
        if (reader == null)
            return;

        var obj = reader.GetMetadataByName<object>("/app1/{ushort=0}/{ushort=34853}");
        if (obj is not IWICMetadataQueryReader)
            return;

        using var gpsReader = new ComObject<IWICMetadataQueryReader>(obj);
        // https://learn.microsoft.com/en-us/windows/win32/wic/-wic-native-image-format-metadata-queries#gps-metadata
        using var r = new WicMetadataQueryReader(gpsReader);

        // https://learn.microsoft.com/en-us/windows/win32/wic/-wic-photoprop-system-gps-altituderef
        var altitudeRef = r.GetMetadataByName<byte>("/{ushort=5}") == 0 ? "+" : "-";

        var altitude = r.GetMetadataByName<IReadOnlyList<uint>>("/{ushort=6}")!;
        Console.WriteLine($"GPS Altitude: {altitudeRef} {altitude[0]} / {altitude[1]}");

        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // force '.' as decimal separator

        var latitudeRef = r.GetMetadataByName<string>("/{ushort=1}");
        var latitudeArray = r.GetMetadataByName<IReadOnlyList<ulong>>("/{ushort=2}")!;
        var longitudeRef = r.GetMetadataByName<string>("/{ushort=3}");
        var longitudeArray = r.GetMetadataByName<IReadOnlyList<ulong>>("/{ushort=4}")!;

        var latitude = Dms.From(latitudeArray);
        var longitude = Dms.From(longitudeArray);
        Console.WriteLine($"GPS (DD): {latitudeRef}/{longitudeRef} {latitude.DecimalDegrees},{longitude.DecimalDegrees}");
        Console.WriteLine($"GPS (DMS): {latitude} {latitudeRef} {longitude} {longitudeRef}");
    }

    public class Dms(double degrees, double minutes, double seconds)
    {
        public double Degrees => degrees;
        public double Minutes => minutes;
        public double Seconds => seconds;
        public double DecimalDegrees => (degrees * 3600 + minutes * 60 + seconds) / 3600;

        public override string ToString() => $"{degrees}° {minutes}' {seconds}\"";

        // https://learn.microsoft.com/en-us/windows/win32/properties/props-system-gps-latitude
        public static Dms From(IReadOnlyList<ulong> array)
        {
            var degreesArray = WicMetadataQueryReader.ChangeType<IReadOnlyList<uint>>(array[0])!;
            var minutesArray = WicMetadataQueryReader.ChangeType<IReadOnlyList<uint>>(array[1])!;
            var secondsArray = WicMetadataQueryReader.ChangeType<IReadOnlyList<uint>>(array[2])!;
            var degrees = degreesArray[0] / (double)degreesArray[1];
            var minutes = minutesArray[0] / (double)minutesArray[1];
            var seconds = secondsArray[0] / (double)secondsArray[1];
            return new Dms(degrees, minutes, seconds);
        }
    }

    static void BuildAtlasWithCPU(int thumbSize = 96, int dimension = 20)
    {
        using var memBmp = new WicBitmapSource((uint)(thumbSize * dimension), (uint)(thumbSize * dimension), WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        memBmp.WithLock(WICBitmapLockFlags.WICBitmapLockWrite, block =>
        {
            var row = 0;
            var col = 0;
            var path = @"d:\temp";
            var sw = new Stopwatch();
            sw.Start();
            foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly))
            {
                var ext = Path.GetExtension(file);
                if (!WicImagingComponent.DecoderFileExtensions.Contains(ext))
                    continue;

                if (new FileInfo(file).Length < 32)
                    continue;

                using var bmp = WicBitmapSource.Load(file);
                bmp.Scale(thumbSize, thumbSize, WICBitmapInterpolationMode.WICBitmapInterpolationModeFant);
                if (bmp.PixelFormat != memBmp.PixelFormat)
                {
                    bmp.ConvertTo(memBmp.PixelFormat);
                }

                var stride = bmp.DefaultStride;
                var bytes = bmp.CopyPixels(stride);
                block.WriteRectangle(col * thumbSize, row * thumbSize, bytes, stride);

                col++;
                if (col > dimension)
                {
                    col = 0;
                    row++;
                }
            }
            Console.WriteLine(sw.Elapsed);
        });

        memBmp.Save("atlascpu.jpg");
    }

    static void BuildTransparentBitmap(uint width, uint height)
    {
        using var fac = DWriteFunctions.DWriteCreateFactory(DWRITE_FACTORY_TYPE.DWRITE_FACTORY_TYPE_SHARED);
        using var format = fac.CreateTextFormat("Segoe UI", 20);
        using var bmp = new WicBitmapSource(width, height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        using var dc = bmp.CreateDeviceContext();
        using var brush = dc.CreateSolidColorBrush(D3DCOLORVALUE.Blue);
        dc.BeginDraw();
        dc.DrawText("Hello World!" + Environment.NewLine + "🤩😛😂", format, new D2D_RECT_F(10, 10, 200, 30), brush, D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);
        dc.EndDraw();
        bmp.Save("transparent.png");
        bmp.Dispose();
        Process.Start(new ProcessStartInfo("transparent.png") { UseShellExecute = true });
    }

    static void BuildAtlasWithGPU(int thumbSize = 96, int dimension = 20)
    {
        using var memBmp = new WicBitmapSource((uint)(thumbSize * dimension), (uint)(thumbSize * dimension), WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        using var dc = memBmp.CreateDeviceContext();
        var row = 0;
        var col = 0;
        var path = @"d:\temp";
        var sw = new Stopwatch();
        sw.Start();
        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly))
        {
            var ext = Path.GetExtension(file);
            if (!WicImagingComponent.DecoderFileExtensions.Contains(ext))
                continue;

            if (new FileInfo(file).Length < 32)
                continue;

            using var bmp = WicBitmapSource.Load(file);
            bmp.Scale(thumbSize, thumbSize, WICBitmapInterpolationMode.WICBitmapInterpolationModeFant);
            if (bmp.PixelFormat != WicPixelFormat.GUID_WICPixelFormat32bppPRGBA)
            {
                bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
            }

            using var cb = dc.CreateBitmapFromWicBitmap(bmp.ComObject);
            var dr = D2D_RECT_F.Sized(col * thumbSize, row * thumbSize, bmp.Width, bmp.Height);
            dc.BeginDraw();
            dc.DrawBitmap(cb, destinationRectangle: dr);
            dc.EndDraw();

            col++;
            if (col > dimension)
            {
                col = 0;
                row++;
            }
        }

        Console.WriteLine(sw.Elapsed);
        memBmp.Save("atlasgpu.jpg");
    }

    static void ConvertSvgAsPng(string fileName)
    {
        using var memBmp = new WicBitmapSource(1000, 1000, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        using var dc = memBmp.CreateDeviceContext();
        var dc5 = dc.As<ID2D1DeviceContext5>()!;
        using var stream = new DirectN.Extensions.Utilities.UnmanagedMemoryStream(fileName);
        using var doc = dc5.CreateSvgDocument(stream, new D2D_SIZE_F(memBmp.Width, memBmp.Height));
        dc.BeginDraw();
        dc5.DrawSvgDocument(doc);
        dc.EndDraw();
        memBmp.Save(Path.ChangeExtension(fileName, ".jpg"));
    }

    static float Distance1(float[] h1, float[] h2)
    {
        var distance = 0f;
        for (var i = 0; i < h1.Length; i++)
        {
            var diff = h2[i] - h1[i];
            distance += diff * diff;
        }
        return (float)Math.Sqrt(distance);
    }

    static void Histograms(uint thumbSize = 100)
    {
        var hists = new List<Tuple<string, float[]>>();
        using var memBmp = new WicBitmapSource(thumbSize, thumbSize, WicPixelFormat.GUID_WICPixelFormat32bppBGR);
        using var dc = memBmp.CreateDeviceContext();
        using var fx = dc.CreateEffect(Constants.CLSID_D2D1Histogram)!;
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

        for (var i = 0; i < hists.Count; i++)
        {
            Console.WriteLine(hists[i].Item1);
            for (var j = 0; j < hists.Count; j++)
            {
                var dist1 = Distance1(hists[i].Item2, hists[j].Item2);
                Console.WriteLine(" " + dist1 + " " + hists[j].Item1);
            }
        }
    }

    static float[] GetHistogram(string filePath, uint thumbSize = 100)
    {
        using var memBmp = new WicBitmapSource(thumbSize, thumbSize, WicPixelFormat.GUID_WICPixelFormat32bppBGR);
        using var dc = memBmp.CreateDeviceContext();
        using var fx = dc.CreateEffect(Constants.CLSID_D2D1Histogram)!;
        return GetHistogram(filePath, dc, fx);
    }

    static float[] GetHistogram(string filePath, IComObject<ID2D1DeviceContext> dc, IComObject<ID2D1Effect> fx)
    {
        using var bmp = WicBitmapSource.Load(filePath);
        bmp.Scale(100, null);
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
        using var cb = dc.CreateBitmapFromWicBitmap(bmp.ComObject);
        fx.SetInput(cb);
        dc.BeginDraw();
        dc.DrawImage(fx);
        dc.EndDraw();
        var bytes = fx.GetValue<byte[]>("HistogramOutput", null)!;
        return MemoryMarshal.Cast<byte, float>(bytes).ToArray();
    }

    static void LoadFromIcon()
    {
        using var bmp = WicBitmapSource.Load("test.ico");
        bmp.Save("testicon.png");
        //Process.Start(new ProcessStartInfo("testicon.png") { UseShellExecute = true });
    }

    static void LoadAndScale(int? boxWidth = null, int? boxHeight = null)
    {
        using var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg");
        bmp.Scale(boxWidth, boxHeight);
        var name = boxWidth + "x" + boxHeight + ".jpg";
        bmp.Save(name);
        //Process.Start(new ProcessStartInfo(name) { UseShellExecute = true });
    }

    static void BuildTurbulences()
    {
        for (var offset = 0f; offset < 10; offset += 1f)
        {
            using var newBmp = new WicBitmapSource(400, 400, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
            using var rt = newBmp.CreateDeviceContext();
            using var fx = rt.CreateEffect(Constants.CLSID_D2D1Turbulence)!;
            using var cb = rt.CreateBitmapFromWicBitmap(newBmp.ComObject);
            fx.SetInput(cb);
            fx.SetValue((int)D2D1_TURBULENCE_PROP.D2D1_TURBULENCE_PROP_OFFSET, new D2D_VECTOR_2F(offset, 0));
            rt.BeginDraw();
            rt.DrawImage(fx);
            rt.EndDraw();
            newBmp.Save($"noise{offset}.jpg");
            //Process.Start(new ProcessStartInfo($"noise{offset}.jpg") { UseShellExecute = true });
        }
    }

    static void BuildBlur(float width)
    {
        using var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg");
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
        using var newBmp = new WicBitmapSource(bmp.Width, bmp.Height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        using var rt = newBmp.CreateDeviceContext();
        using var fx = rt.CreateEffect(Constants.CLSID_D2D1GaussianBlur)!;
        using var cb = rt.CreateBitmapFromWicBitmap(bmp.ComObject);
        fx.SetInput(cb);
        fx.SetValue((int)D2D1_GAUSSIANBLUR_PROP.D2D1_GAUSSIANBLUR_PROP_STANDARD_DEVIATION, width * 3.0f);
        fx.SetValue((int)D2D1_GAUSSIANBLUR_PROP.D2D1_GAUSSIANBLUR_PROP_OPTIMIZATION, D2D1_DIRECTIONALBLUR_OPTIMIZATION.D2D1_DIRECTIONALBLUR_OPTIMIZATION_QUALITY);

        using var img = fx.GetOutput();
        var bounds = rt.GetImageLocalBounds(img);

        rt.BeginDraw();
        rt.DrawImage(fx);
        rt.EndDraw();
        newBmp.Save("blur.jpg");
        //Process.Start(new ProcessStartInfo("blur.jpg") { UseShellExecute = true });
    }

    static void BuildStraighten()
    {
        using var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg");
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
        using var newBmp = new WicBitmapSource(bmp.Width, bmp.Height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        using var rt = newBmp.CreateDeviceContext();
        using var fx = rt.CreateEffect(Constants.CLSID_D2D1Straighten)!;
        using var cb = rt.CreateBitmapFromWicBitmap(bmp.ComObject);
        fx.SetInput(cb);
        fx.SetValue((int)D2D1_STRAIGHTEN_PROP.D2D1_STRAIGHTEN_PROP_ANGLE, 12.0f);
        fx.SetValue((int)D2D1_STRAIGHTEN_PROP.D2D1_STRAIGHTEN_PROP_MAINTAIN_SIZE, false);
        fx.SetValue((int)D2D1_STRAIGHTEN_PROP.D2D1_STRAIGHTEN_PROP_SCALE_MODE, D2D1_STRAIGHTEN_SCALE_MODE.D2D1_STRAIGHTEN_SCALE_MODE_LINEAR);

        using (var img = fx.GetOutput())
        {
            var bounds = rt.GetImageLocalBounds(img);
        }

        rt.BeginDraw();
        rt.DrawImage(fx);
        rt.EndDraw();
        newBmp.Save("straighten.jpg");
        Process.Start(new ProcessStartInfo("straighten.jpg") { UseShellExecute = true });
    }

    static void BuildCrop()
    {
        using var bmp = WicBitmapSource.Load(@"SamsungSGH-P270.jpg");
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
        using var newBmp = new WicBitmapSource(bmp.Width, bmp.Height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        using var rt = newBmp.CreateDeviceContext();
        var breush = rt.CreateCheckerboardBrush(8);
        using var fx = rt.CreateEffect(Constants.CLSID_D2D1Crop)!;
        using var cb = rt.CreateBitmapFromWicBitmap(bmp.ComObject);
        fx.SetInput(cb);
        fx.SetValue((int)D2D1_CROP_PROP.D2D1_CROP_PROP_RECT, new D2D_VECTOR_4F(bmp.Width / 2, bmp.Height / 2, bmp.Width, bmp.Height));

        rt.BeginDraw();
        rt.DrawImage(fx);
        rt.EndDraw();
        newBmp.Save("crop.jpg");
        Process.Start(new ProcessStartInfo("crop.jpg") { UseShellExecute = true });
    }

    static void RotateAndGrayscale()
    {
        using var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg");
        bmp.FlipRotate(WICBitmapTransformOptions.WICBitmapTransformRotate90);
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
        using var newBmp = new WicBitmapSource(bmp.Width, bmp.Height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        using var rt = newBmp.CreateDeviceContext();
        using var fx = rt.CreateEffect(Constants.CLSID_D2D1Grayscale)!;
        using var cb = rt.CreateBitmapFromWicBitmap(bmp.ComObject);
        fx.SetInput(cb);
        rt.BeginDraw();
        rt.DrawImage(fx);
        rt.EndDraw();
        newBmp.Save("gray.jpg");
        //Process.Start(new ProcessStartInfo("gray.jpg") { UseShellExecute = true });
    }

    static void Posterize(uint numberOfColors)
    {
        using var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg");
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
        using var newBmp = new WicBitmapSource(bmp.Width, bmp.Height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        using var rt = newBmp.CreateDeviceContext();
        using var fx = rt.CreateEffect(Constants.CLSID_D2D1Posterize)!;
        using var cb = rt.CreateBitmapFromWicBitmap(bmp.ComObject);
        fx.SetInput(cb);
        fx.SetValue((int)D2D1_POSTERIZE_PROP.D2D1_POSTERIZE_PROP_RED_VALUE_COUNT, numberOfColors);
        fx.SetValue((int)D2D1_POSTERIZE_PROP.D2D1_POSTERIZE_PROP_GREEN_VALUE_COUNT, numberOfColors);
        fx.SetValue((int)D2D1_POSTERIZE_PROP.D2D1_POSTERIZE_PROP_BLUE_VALUE_COUNT, numberOfColors);
        rt.BeginDraw();
        rt.DrawImage(fx);
        rt.EndDraw();
        var name = "posterize" + numberOfColors + ".jpg";
        newBmp.Save(name);
        //Process.Start(new ProcessStartInfo(name) { UseShellExecute = true });
    }

    static void DrawEllipse()
    {
        using var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg");
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
        uint width = 200;
        var height = width * bmp.Height / bmp.Width;
        using var memBmp = new WicBitmapSource(width, height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        using var rt = memBmp.CreateDeviceContext();
        using var dbmp = rt.CreateBitmapFromWicBitmap(bmp.ComObject);
        using var brush = rt.CreateSolidColorBrush(D3DCOLORVALUE.Red);
        rt.BeginDraw();
        rt.DrawBitmap(dbmp, destinationRectangle: new D2D_RECT_F(memBmp.Size.ToD2D_SIZE_F()));
        rt.DrawEllipse(new D2D1_ELLIPSE(width / 2, height / 2, Math.Min(width, height) / 2), brush, 4);
        rt.EndDraw();
        memBmp.Save("ellipse.jpg");
        //Process.Start(new ProcessStartInfo("ellipse.jpg") { UseShellExecute = true });
    }

    static void DrawText()
    {
        using var fac = DWriteFunctions.DWriteCreateFactory(DWRITE_FACTORY_TYPE.DWRITE_FACTORY_TYPE_SHARED);
        using var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg");
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);
        using var memBmp = new WicBitmapSource(bmp.Width, bmp.Height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        using var rt = memBmp.CreateDeviceContext();
        using var dbmp = rt.CreateBitmapFromWicBitmap(bmp.ComObject);
        using var brush = rt.CreateSolidColorBrush(D3DCOLORVALUE.Green);
        using var format = fac.CreateTextFormat("Segoe UI", 20);
        rt.BeginDraw();
        rt.DrawBitmap(dbmp);
        rt.DrawText("Hello World!" + Environment.NewLine + "🤩😛😂", format, new D2D_RECT_F(10, 10, 200, 30), brush, D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);
        rt.EndDraw();
        memBmp.Save("helloworld.jpg");
        //Process.Start(new ProcessStartInfo("helloworld.jpg") { UseShellExecute = true });
    }

    static void DumpAllComponentsPossibleConversions()
    {
        var formats = WicImagingComponent.AllComponents.OfType<WicPixelFormat>();
        var converters = WicImagingComponent.AllComponents.OfType<WicPixelFormatConverter>();
        foreach (var comp in converters)
        {
            Console.WriteLine(comp.Type + " " + comp);
            foreach (var to in comp.PixelFormatsList)
            {
                foreach (var from in formats)
                {
                    if (from.Guid == to.Guid)
                        continue;

                    if (comp.CanConvert(from.Guid, to.Guid))
                    {
                        Console.WriteLine(" " + from + " => " + to);
                    }
                }
            }
            Console.WriteLine();
        }
    }

    static void DumpAllComponents()
    {
        foreach (var comp in WicImagingComponent.AllComponents)
        {
            Console.WriteLine(comp.Type + " " + comp);
        }
    }

    static void DumpPossibleWicBitmapRenderTargetFormats()
    {
        using var fac = D2D1Functions.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_MULTI_THREADED);
        foreach (var format in WicImagingComponent.AllComponents.OfType<WicPixelFormat>().Where(f => f.BitsPerPixel == 32))
        {
            using var bmp = WicImagingFactory.CreateBitmap(100, 100, format.Guid, WICBitmapCreateCacheOption.WICBitmapCacheOnDemand);
            foreach (var dxgiFormat in Enum.GetValues<DXGI_FORMAT>().OfType<DXGI_FORMAT>())
            {
                foreach (var alpha in Enum.GetValues<D2D1_ALPHA_MODE>().OfType<D2D1_ALPHA_MODE>())
                {

                    var p = new D2D1_RENDER_TARGET_PROPERTIES
                    {
                        pixelFormat = new D2D1_PIXEL_FORMAT
                        {
                            alphaMode = alpha,
                            format = dxgiFormat,
                        }
                    };

                    var hr = fac.Object.CreateWicBitmapRenderTarget(bmp.Object, p, out var target);
                    if (target != null)
                    {
                        Console.WriteLine($"{format} {dxgiFormat} {alpha}");
                        target.FinalRelease();
                    }
                }
            }
        }
    }

    static void CopyWithColorContext()
    {
        using var bmp = WicBitmapSource.Load("hdr-image.jpg");

        // save with color contexts
        var ctx = bmp.GetColorContexts();
        bmp.Save("hdr-image-copy.jpg", colorContexts: ctx);
    }

    static void DumpAllFormats()
    {
        var formats = WicImagingComponent.AllComponents.OfType<WicPixelFormat>();
        foreach (var format in formats)
        {
            Console.WriteLine(format);
            using var ctx = format.GetColorContext();
            if (ctx != null)
            {
                if (ctx.Type == WICColorContextType.WICColorContextExifColorSpace)
                {
                    Console.WriteLine(" Color Context. Space: " + ctx.ExifColorSpace);
                }
                else
                {
                    Console.WriteLine(" Color Context. Profile: " + ctx.Profile);
                }
            }
        }
    }

    static void ConvertToBW()
    {
        using var bmp = WicBitmapSource.Load("SamsungSGH-P270.jpg");
        //bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat1bppIndexed, ditherType: WICBitmapDitherType.WICBitmapDitherTypeErrorDiffusion, paletteTranslate: WICBitmapPaletteType.WICBitmapPaletteTypeFixedBW);
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormatBlackWhite, ditherType: WICBitmapDitherType.WICBitmapDitherTypeDualSpiral8x8, paletteTranslate: WICBitmapPaletteType.WICBitmapPaletteTypeFixedHalftone256);
        bmp.Save("bw.bmp");
        //Process.Start(new ProcessStartInfo("bw.bmp") { UseShellExecute = true });
    }

    static void ExtractGif()
    {
        var path = @"source.gif";
        using var dec = WicBitmapDecoder.Load(path);
        Console.WriteLine("Frames: " + dec.FrameCount);
        var reader = dec.GetMetadataQueryReader();
        Dump(reader);
        Console.WriteLine();

        var i = 1;
        var name = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);

        var dir = Path.GetFullPath(name);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        foreach (var frame in dec)
        {
            frame.Save(Path.Combine(dir, name + "." + i + ext));
            reader = frame.GetMetadataReader();
            Console.WriteLine("Frame " + i + " metadata");
            Dump(reader);
            Console.WriteLine();
            i++;
        }
    }

    static void CopyGif()
    {
        using var dec = WicBitmapDecoder.Load(@"source.gif");
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

        using var encoder = WicImagingFactory.CreateEncoder(dec.ContainerFormat);
        using var file = File.OpenWrite("test.gif");
        var mis = new ManagedIStream(file);
        encoder.Initialize(mis);

        foreach (var frame in dec)
        {
            using var newFrame = encoder.CreateNewFrame();
            newFrame.Initialize();

            var md = frame.GetMetadataReader()!.Enumerate();
            using var writer = newFrame.GetMetadataQueryWriter();
            writer.EncodeMetadata(md);

            // change delay here
            writer.SetMetadataByName("/grctlext/Delay", (ushort)5);

            if (frame.Palette != null)
            {
                newFrame.SetPalette(frame.Palette.ComObject);
            }

            newFrame.WriteSource(frame.ComObject);
            newFrame.Commit();
        }
        encoder.Commit();
    }

    static void DumpEncoderComponents()
    {
        foreach (var ext in WicImagingComponent.EncoderFileExtensions)
        {
            Console.WriteLine(ext + " " + WicDecoder.FromFileExtension(ext));
        }
    }

    static void ToTiff(string path)
    {
        using var bmp = WicBitmapSource.Load(path);
        Dump(bmp.GetMetadataReader());

        for (var i = 0; i < 8; i++)
        {
            var option = (WICTiffCompressionOption)i;
            using var file = File.OpenWrite("test." + option + ".tiff");
            using var encoder = WicImagingFactory.CreateEncoder(WicCodec.GUID_ContainerFormatTiff);
            var mis = new ManagedIStream(file);
            encoder.Initialize(mis);

            using var newFrame = encoder.CreateNewFrame();
            var dic = new Dictionary<string, object>
            {
                ["TiffCompressionMethod"] = option
            };
            newFrame.Bag.Write(dic);
            newFrame.Initialize();

            using var writer = newFrame.GetMetadataQueryWriter();
            //writer.SetMetadataByName("/ifd/{ushort=296}", 1);
            writer.SetMetadataByName("/ifd/xmp/exif:ImageUniqueID", "ImageId" + option);

            newFrame.WriteSource(bmp.ComObject);
            newFrame.Commit();
            encoder.Commit();
        }
    }

    static void Dump(WicMetadataQueryReader? reader)
    {
        if (reader == null)
            return;

        // gt policies (well known Windows metadata)
        var policies = new WicMetadataPolicies(reader);

        // dump metadata to console
        reader.Visit((r, kv) =>
        {
            var value = kv.Value;
            var type = value?.GetType().Name;
            if (value is byte[] bytes)
            {
                value = Conversions.ToHexa(bytes, 64) + " (" + bytes.Length + ")";
            }

            Console.WriteLine(WicMetadataKey.CombineKeys(r.Location, kv.Key.Key) + " [" + type + "/" + kv.Type + "]= " + value);
        });
    }

    static void CopyFile()
    {
        using var bmp = WicBitmapSource.Load(@"SamsungSGH-P270.jpg");
        // get metadata source (reader)
        using var reader = bmp.GetMetadataReader()!;

        //var th = bmp.GetThumbnail();

        // gt policies (well known Windows metadata)
        var policies = new WicMetadataPolicies(reader);

        // dump metadata to console
        reader.Visit((r, kv) =>
        {
            var value = kv.Value;
            var type = value?.GetType().Name;
            value = valueToString(value);
            Console.WriteLine(WicMetadataKey.CombineKeys(r.Location, kv.Key.Key) + " [" + type + "/" + kv.Type + "]: " + value);
        });

        // save with metadata
        bmp.Save("copy.jpg", metadata: reader);

        static string valueToString(object? value)
        {
            if (value == null)
                return "<null>";

            if (value is byte[] bytes)
                return Conversions.ToHexa(bytes, 64) + " (" + bytes.Length + ")";

            if (value is Array array)
                return string.Join(",", array.Cast<object>().Select(o => valueToString(o))) + " (" + array.Length + ")";

            return string.Format("{0}", value);
        }
    }

    [SupportedOSPlatform("windows8.0")]
    static void TryVariousConversions()
    {
        // build a 1 pixel image
        using var bmp = new WicBitmapSource(1, 1, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
        // build RGB color
        var color = D3DCOLORVALUE.FromArgb(255, 255, 255, 255);

        // print ScRGB equivalent (should match Float formats)
        Console.WriteLine(color.ScA + " " + color.ScR + " " + color.ScG + " " + color.ScB);

        using var dc = bmp.CreateDeviceContext();
        dc.Object.BeginDraw();
        unsafe
        {
            dc.Object.Clear((nint)(&color));
        }
        dc.Object.EndDraw(0, 0);

        Console.WriteLine("GUID_WICPixelFormat32bppPRGBA");
        var bytes = bmp.CopyPixels();
        for (var i = 0; i < bytes.Length; i += 4)
        {
            var r = bytes[i];
            var g = bytes[i + 1];
            var b = bytes[i + 2];
            var a = bytes[i + 3];
            Console.WriteLine("R=" + r + " G=" + g + " B=" + b + " A=" + a); // 0 => 255 (RGB)
        }
        Console.WriteLine();

        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat128bppPRGBAFloat);
        Console.WriteLine("GUID_WICPixelFormat128bppPRGBAFloat");

        bytes = bmp.CopyPixels();
        for (var i = 0; i < bytes.Length; i += 16)
        {
            var r = BitConverter.ToSingle(bytes, i);
            var g = BitConverter.ToSingle(bytes, i + 4);
            var b = BitConverter.ToSingle(bytes, i + 8);
            var a = BitConverter.ToSingle(bytes, i + 12);
            Console.WriteLine("R=" + r + " G=" + g + " B=" + b + " A=" + a); // 0.0f => 1.0f (ScRGB)
        }
        Console.WriteLine();

        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat64bppRGBA);
        Console.WriteLine("GUID_WICPixelFormat64bppRGBA");
        bytes = bmp.CopyPixels();
        for (var i = 0; i < bytes.Length; i += 8)
        {
            var r = (ushort)((bytes[i] << 8) + bytes[i + 1]);
            var g = (ushort)((bytes[i + 2] << 8) + bytes[i + 3]);
            var b = (ushort)((bytes[i + 4] << 8) + bytes[i + 5]);
            var a = (ushort)((bytes[i + 6] << 8) + bytes[i + 7]);
            Console.WriteLine("R=" + r + " G=" + g + " B=" + b + " A=" + a); // 0 => 65535 (RGB)
        }
        Console.WriteLine();

        // red channel => lsb
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppRGBA1010102);
        Console.WriteLine("GUID_WICPixelFormat32bppRGBA1010102");

        bytes = bmp.CopyPixels();
        for (var i = 0; i < bytes.Length; i += 4)
        {
            var r = ((bytes[i + 1] & 0x3) << 8) + bytes[i]; // 10 bits
            var g = ((bytes[i + 2] & 0xF) << 6) + (bytes[i + 1] >> 2); // 10 bits
            var b = ((bytes[i + 3] & 0x3F) << 4) + (bytes[i + 2] >> 4); // 10 bits
            var a = bytes[i + 3] >> 6; // 2 bits
            Console.WriteLine("R=" + r + " G=" + g + " B=" + b + " A=" + a); // RGB: 0 => 1023 A: 0 => 3 (RGB)
        }
        Console.WriteLine();
    }

}
#pragma warning restore CS0162 // Unreachable code detected
