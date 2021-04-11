# WicNet
.NET interop classes for WIC (Windows Imaging Component) based on netstandard 2.0.

Work in still progress, however basic operations should work. Create an issue if something's not working or feature is missing.



Most simple use case is Load & Save:

    using (var bmp = WicBitmapSource.Load(@"myJpegFile.jpg"))
    {
    	bmp.Save("myHeicFile.heic");
    }