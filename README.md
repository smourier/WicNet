# WicNet
.NET interop classes for WIC (Windows Imaging Component) and Direct2D, based on netstandard 2.0, with zero dependency.

Work in still progress, however most operations should work. Create an issue if something's not working or feature is missing.

Simple use case is load & save:

    using (var bmp = WicBitmapSource.Load(@"myJpegFile.jpg"))
    {
        bmp.Save("myHeicFile.heic");
    }

Draw ellipse over an image & save (uses D2D):

    using (var bmp = WicBitmapSource.Load("myJpegFile.jpg"))
    // we need a D2D1-compatible Wic bitmap
    using (var converted = bmp.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPRGBA))
    {
        var width = 200;
        var height = width * bmp.Height / bmp.Width;
                
        // create a bitmap (black)
        using (var clone = new WicBitmapSource(width, height, WicPixelFormat.GUID_WICPixelFormat32bppPRGBA))
        using (var rt = clone.CreateDeviceContext())
        using (var dbmp = rt.CreateBitmapFromWicBitmap(converted.ComObject)) // import Wic => D2D (CPU => GPU)
        using (var brush = rt.CreateSolidColorBrush(_D3DCOLORVALUE.Red)) // create a red brush
        {
            rt.BeginDraw();
                    
            // draw bitmap
            rt.DrawBitmap(dbmp, destinationRectangle: new D2D_RECT_F(new D2D_SIZE_F(clone.Size)));

            // draw red ellipse
            rt.DrawEllipse(new D2D1_ELLIPSE(width / 2, height / 2, Math.Min(width, height) / 2), brush, 4);
            rt.EndDraw();

            // save
            clone.Save("ellipse.jpg");
        }
    }

Convert image to grayscale & save (uses D2D effects):

    using (var bmp = WicBitmapSource.Load("myJpegFile.jpg"))
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
