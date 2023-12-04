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

