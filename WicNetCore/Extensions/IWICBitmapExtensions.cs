using System;
using DirectN;
using DirectNAot.Extensions.Com;
using DirectNAot.Extensions.Utilities;

namespace WicNet.Extensions;

public static class IWICBitmapExtensions
{
    public static IComObject<IWICBitmapLock> Lock(this IComObject<IWICBitmap> bitmap, WICBitmapLockFlags flags, WICRect? rect = null) => Lock(bitmap?.Object!, flags, rect);
    public static IComObject<IWICBitmapLock> Lock(this IWICBitmap bitmap, WICBitmapLockFlags flags, WICRect? rect = null)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        var ptr = rect.ToPointerByRef();
        bitmap.Lock(ptr, (uint)flags, out var value).ThrowOnError();
        return new ComObject<IWICBitmapLock>(value);
    }
}
