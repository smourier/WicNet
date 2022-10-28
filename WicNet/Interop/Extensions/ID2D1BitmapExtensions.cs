using System;

namespace DirectN
{
    public static class ID2D1BitmapExtensions
    {
        public static D2D_SIZE_F GetDpi(this IComObject<ID2D1Bitmap> bitmap) => GetDpi(bitmap?.Object);
        public static D2D_SIZE_F GetDpi(this ID2D1Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            bitmap.GetDpi(out var x, out var y);
            return new D2D_SIZE_F(x, y);
        }

        public static D2D1_PIXEL_FORMAT GetPixelFormat(this IComObject<ID2D1Bitmap> bitmap) => GetPixelFormat(bitmap?.Object);
        public static D2D1_PIXEL_FORMAT GetPixelFormat(this ID2D1Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            bitmap.GetPixelFormat(out var format);
            return format;
        }

        public static D2D_SIZE_U GetPixelSize(this IComObject<ID2D1Bitmap> bitmap) => GetPixelSize(bitmap?.Object);
        public static D2D_SIZE_U GetPixelSize(this ID2D1Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            bitmap.GetPixelSize(out var size);
            return size;
        }

        public static D2D_SIZE_F GetSize(this IComObject<ID2D1Bitmap> bitmap) => GetSize(bitmap?.Object);
        public static D2D_SIZE_F GetSize(this ID2D1Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            bitmap.GetSize(out var size);
            return size;
        }

        public static void CopyFromBitmap(this IComObject<ID2D1Bitmap> bitmap, IComObject<ID2D1Bitmap> source, D2D_POINT_2U? destinationPoint = null, D2D_RECT_U? sourceRect = null) => CopyFromBitmap(bitmap?.Object, source?.Object, destinationPoint, sourceRect);
        public static void CopyFromBitmap(this ID2D1Bitmap bitmap, ID2D1Bitmap source, D2D_POINT_2U? destinationPoint = null, D2D_RECT_U? sourceRect = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            using (var memPt = destinationPoint.StructureToMemory())
            {
                using (var memRc = sourceRect.StructureToMemory())
                {
                    bitmap.CopyFromBitmap(memPt.Pointer, source, memRc.Pointer).ThrowOnError();
                }
            }
        }

        public static void CopyFromRenderTarget(this IComObject<ID2D1Bitmap> bitmap, IComObject<ID2D1RenderTarget> renderTarget, D2D_POINT_2U? destinationPoint = null, D2D_RECT_U? sourceRect = null) => CopyFromRenderTarget(bitmap?.Object, renderTarget?.Object, destinationPoint, sourceRect);
        public static void CopyFromRenderTarget(this ID2D1Bitmap bitmap, ID2D1RenderTarget renderTarget, D2D_POINT_2U? destinationPoint = null, D2D_RECT_U? sourceRect = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            using (var memPt = destinationPoint.StructureToMemory())
            {
                using (var memRc = sourceRect.StructureToMemory())
                {
                    bitmap.CopyFromRenderTarget(memPt.Pointer, renderTarget, memRc.Pointer).ThrowOnError();
                }
            }
        }

        public static void CopyFromMemory(this IComObject<ID2D1Bitmap> bitmap, IntPtr sourceData, uint pitch, D2D_RECT_U? destinationRect = null) => CopyFromMemory(bitmap?.Object, sourceData, pitch, destinationRect);
        public static void CopyFromMemory(this ID2D1Bitmap bitmap, IntPtr sourceData, uint pitch, D2D_RECT_U? destinationRect = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            if (sourceData == IntPtr.Zero)
                throw new ArgumentException(null, nameof(sourceData));

            if (pitch == 0)
                throw new ArgumentException(null, nameof(pitch));

            using (var mem = destinationRect.StructureToMemory())
            {
                bitmap.CopyFromMemory(mem.Pointer, sourceData, pitch).ThrowOnError();
            }
        }
    }
}
