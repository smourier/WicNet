using System;

namespace DirectN
{
    public static class IWICBitmapFrameDecodeExtensions
    {
        public static IComObject<IWICMetadataQueryReader> GetMetadataQueryReader(this IComObject<IWICBitmapFrameDecode> frame) => GetMetadataQueryReader(frame?.Object);
        public static IComObject<IWICMetadataQueryReader> GetMetadataQueryReader(this IWICBitmapFrameDecode frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.GetMetadataQueryReader(out var value).ThrowOnError(false);
            if (value == null)
                return null;

            return new ComObject<IWICMetadataQueryReader>(value);
        }

        public static IComObject<IWICBitmapSource> GetThumbnail(this IComObject<IWICBitmapFrameDecode> frame) => GetThumbnail(frame?.Object);
        public static IComObject<IWICBitmapSource> GetThumbnail(this IWICBitmapFrameDecode frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.GetThumbnail(out var value).ThrowOnError(false);
            if (value == null)
                return null;

            return new ComObject<IWICBitmapSource>(value);
        }
    }
}
