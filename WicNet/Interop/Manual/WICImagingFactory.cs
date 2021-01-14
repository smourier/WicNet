using System;
using System.Runtime.InteropServices;

namespace WicNet.Interop
{
    public static class WICImagingFactory
    {
        public static IComObject<IWICPalette> CreatePalette() => WithFactory(f => f.CreatePalette());
        public static IComObject<IWICBitmapClipper> CreateBitmapClipper() => WithFactory(f => f.CreateBitmapClipper());
        public static IComObject<IWICBitmapFlipRotator> CreateBitmapFlipRotator() => WithFactory(f => f.CreateBitmapFlipRotator());
        public static IComObject<IWICBitmapScaler> CreateBitmapScaler() => WithFactory(f => f.CreateBitmapScaler());
        public static IComObject<IWICBitmap> CreateBitmap(int width, int height, Guid pixelFormat, WICBitmapCreateCacheOption option = WICBitmapCreateCacheOption.WICBitmapNoCache) => WithFactory(f => f.CreateBitmap(width, height, pixelFormat, option));

        public static T WithFactory<T>(Func<IWICImagingFactory, T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var factory = (IWICImagingFactory)new ComFactory();
            try
            {
                return func(factory);
            }
            finally
            {
                Marshal.ReleaseComObject(factory);
            }
        }

        public static void WithFactory(Action<IWICImagingFactory> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var factory = (IWICImagingFactory)new ComFactory();
            try
            {
                action(factory);
            }
            finally
            {
                Marshal.ReleaseComObject(factory);
            }
        }

        [ComImport]
        [Guid("CACAF262-9370-4615-A13B-9F5539DA4C0A")]
        private class ComFactory
        {
        }
    }
}
