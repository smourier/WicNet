# WicNet
.NET interop classes for WIC (Windows Imaging Component) Direct2D, and DirectWrire, based on netstandard 2.0, with zero dependency.

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

Rotate image, convert to grayscale and save (uses D2D effects):

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
