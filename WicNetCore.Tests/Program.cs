using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using DirectN;
using DirectN.Extensions.Utilities;
using WicNet;

[assembly: SupportedOSPlatform("windows8.0")]

namespace WicNetCore.Tests;

internal class Program
{
    static void Main()
    {
        //TryVariousConversions();
        //return;
        CopyFile();
        ExtractGif();
        ConvertToBW();
        DumpAllFormats();
        CopyWithColorContext();
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
        Process.Start(new ProcessStartInfo("bw.bmp") { UseShellExecute = true });
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

        string valueToString(object? value)
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

        using (var dc = bmp.CreateDeviceContext())
        {
            dc.Object.BeginDraw();
            unsafe
            {
                dc.Object.Clear((nint)(&color));
            }
            dc.Object.EndDraw(0, 0);
        }

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

