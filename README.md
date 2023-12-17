# WicNet
.NET interop classes for WIC (Windows Imaging Component) Direct2D, and DirectWrite, based on netstandard 2.0, with zero dependency (except for DirectN https://www.nuget.org/packages/DirectNStandard/)

Nuget Package is available here: https://www.nuget.org/packages/WicNet

Work in still progress, however most operations should work. Create an issue if something's not working or feature is missing.

Simple use case is load & save:

    using (var bmp = WicBitmapSource.Load(@"myJpegFile.jpg"))
    {
        bmp.Save("myHeicFile.heic");
    }

Draw ellipse over an image and save (uses D2D):

    using (var bmp = WicBitmapSource.Load("MyImag.jpg"))
    {
        bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR);  // needed to be able to work with Direct2D
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
        }
    }

Rotate image, convert to grayscale and save (uses Direct2D effects):

    static void RotateAndGrayscale()
    {
        using (var bmp = WicBitmapSource.Load("MyImage.jpg"))
        {
            bmp.Rotate(WICBitmapTransformOptions.WICBitmapTransformRotate90);
            bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppBGR); // needed to be able to work with Direct2D
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
            }
        }
    }
    
## WicNetExplorer
WicNetExplorer is a GUI sample program that demonstrates how to use WicNet, it's capable of loading and saving images and shows WIC information:

![image](https://github.com/smourier/WicNet/assets/5328574/af1795f4-3627-4193-a849-3e2c50f87aac)

WicNetExplorer demonstrates two Windows technologies for the WIC display surface:

* Direct2D's `ID2D1HwndRenderTarget` interface: https://learn.microsoft.com/en-us/windows/win32/api/d2d1/nn-d2d1-id2d1hwndrendertarget
* Windows Direct Composition (aka the [Visual Layer](https://learn.microsoft.com/en-us/windows/uwp/composition/visual-layer) ), through the use of `CompositionDrawingSurface` Class: https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.compositiondrawingsurface

## WinUI3/WinRT interop
The WinUI3Tests program demonstrates WicNet (and therefore WIC) interop with WinRT's [SoftwareBitmap](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.imaging.softwarebitmap) and WinUI3:

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

