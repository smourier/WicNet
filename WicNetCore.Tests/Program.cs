using System;
using System.Linq;
using System.Runtime.Versioning;
using DirectN;
using WicNet;

[assembly: SupportedOSPlatform("windows6.0")]

namespace WicNetCore.Tests;

internal class Program
{
    static void Main()
    {
        DumpAllFormats();
        //CopyWithColorContext();
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
}

