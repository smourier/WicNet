using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WicNet.WinUI3Tests;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "WicNet - WinUI3 Tests";
    }

    private async void MyButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        foreach (var type in WicImagingComponent.DecoderFileExtensions)
        {
            picker.FileTypeFilter.Add(type);
        }

        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));
        var file = await picker.PickSingleFileAsync();

        var softwareBitmap = GetSoftwareBitmap(file.Path);
        var source = new SoftwareBitmapSource();
        await source.SetBitmapAsync(softwareBitmap);
        myImage.Source = source;
    }

    private static WicBitmapSource GetTransformed(WicBitmapSource bmp)
    {
        try
        {
            return bmp.GetColorTransform(bmp.GetBestColorContext(), WicColorContext.Standard, WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);
        }
        catch
        {
            // some color context don't support transformation
            return null;
        }
    }

    private static SoftwareBitmap GetSoftwareBitmap(string filePath)
    {
        using var bmp = WicBitmapSource.Load(filePath);

        // note: software bitmap doesn't seem to support color contexts
        // so we must transform it ourselves, building one using pixels after color transformation
        // this is the moral equivalent to WinRT's BitmapDecoder.GetPixelDataAsync (which uses Wic underneath...)
        // https://learn.microsoft.com/en-us/uwp/api/windows.graphics.imaging.bitmapdecoder.getpixeldataasync
        var ctx = bmp.GetColorContexts();
        if (ctx.Count > 0)
        {
            using var transformed = GetTransformed(bmp);
            if (transformed != null)
            {
                // get pixels as an array of bytes
                var bytes = transformed.CopyPixels();

                // get WinRT SoftwareBitmap
                var softwareBitmap = new SoftwareBitmap(
                    BitmapPixelFormat.Bgra8,
                    bmp.Width,
                    bmp.Height,
                    BitmapAlphaMode.Premultiplied);
                softwareBitmap.CopyFromBuffer(bytes.AsBuffer());
                return softwareBitmap;
            }
        }

        // software bitmap doesn't support all formats
        // https://learn.microsoft.com/en-us/uwp/api/windows.graphics.imaging.bitmappixelformat
        // and SoftwareBitmapSource only support Bgra8...
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPBGRA);

        // software bitmap doesn't support "raw" IWicBitmapSource, it wants an IWicBitmap
        using var clone = bmp.Clone();
        return clone.WithSoftwareBitmap(true, ptr => SoftwareBitmap.FromAbi(ptr));
    }
}
