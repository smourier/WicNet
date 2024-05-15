using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using DirectN;
using DirectNAot.Extensions.Utilities;
using WicNet;

[assembly: SupportedOSPlatform("windows6.0")]

namespace WicNetCore.Tests;

internal class Program
{
    static void Main()
    {
        CopyFile();
        return;
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

