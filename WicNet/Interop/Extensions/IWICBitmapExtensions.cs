using System;

namespace WicNet.Interop
{
    public static class IWICBitmapExtensions
    {
        public static IComObject<IWICBitmapLock> Lock(this IComObject<IWICBitmap> bitmap, WICBitmapLockFlags flags, WICRect? rect = null) => Lock(bitmap?.Object, flags, rect);
        public static IComObject<IWICBitmapLock> Lock(this IWICBitmap bitmap, WICBitmapLockFlags flags, WICRect? rect = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            using (var mem = new ComMemory(rect))
            {
                bitmap.Lock(mem.Pointer, flags, out var value).ThrowOnError();
                return new ComObject<IWICBitmapLock>(value);
            }
        }
    }
}
