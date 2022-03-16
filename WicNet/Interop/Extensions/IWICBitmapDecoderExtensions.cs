using System;

namespace DirectN
{
    public static class IWICBitmapDecoderExtensions
    {
        public static Guid GetContainerFormat(this IComObject<IWICBitmapDecoder> decoder) => GetContainerFormat(decoder?.Object);
        public static Guid GetContainerFormat(this IWICBitmapDecoder decoder)
        {
            if (decoder == null)
                throw new ArgumentNullException(nameof(decoder));

            decoder.GetContainerFormat(out var value).ThrowOnError();
            return value;
        }

        public static int GetFrameCount(this IComObject<IWICBitmapDecoder> decoder) => GetFrameCount(decoder?.Object);
        public static int GetFrameCount(this IWICBitmapDecoder decoder)
        {
            if (decoder == null)
                throw new ArgumentNullException(nameof(decoder));

            decoder.GetFrameCount(out var value).ThrowOnError();
            return value;
        }

        public static IComObject<IWICBitmapFrameDecode> GetFrame(this IComObject<IWICBitmapDecoder> decoder, int index) => GetFrame(decoder?.Object, index);
        public static IComObject<IWICBitmapFrameDecode> GetFrame(this IWICBitmapDecoder decoder, int index)
        {
            if (decoder == null)
                throw new ArgumentNullException(nameof(decoder));

            decoder.GetFrame(index, out var value).ThrowOnError();
            return new ComObject<IWICBitmapFrameDecode>(value);
        }

        public static IComObject<IWICMetadataQueryReader> GetMetadataQueryReader(this IComObject<IWICBitmapDecoder> decoder) => GetMetadataQueryReader(decoder?.Object);
        public static IComObject<IWICMetadataQueryReader> GetMetadataQueryReader(this IWICBitmapDecoder decoder)
        {
            if (decoder == null)
                throw new ArgumentNullException(nameof(decoder));

            decoder.GetMetadataQueryReader(out var value).ThrowOnError();
            return new ComObject<IWICMetadataQueryReader>(value);
        }

        public static IComObject<IWICBitmapDecoderInfo> GetDecoderInfo(this IComObject<IWICBitmapDecoder> decoder) => GetDecoderInfo(decoder?.Object);
        public static IComObject<IWICBitmapDecoderInfo> GetDecoderInfo(this IWICBitmapDecoder decoder)
        {
            if (decoder == null)
                throw new ArgumentNullException(nameof(decoder));

            decoder.GetDecoderInfo(out var value).ThrowOnError();
            return new ComObject<IWICBitmapDecoderInfo>(value);
        }

        public static IComObject<IWICBitmapSource> GetThumbnail(this IComObject<IWICBitmapDecoder> decoder) => GetThumbnail(decoder?.Object);
        public static IComObject<IWICBitmapSource> GetThumbnail(this IWICBitmapDecoder decoder)
        {
            if (decoder == null)
                throw new ArgumentNullException(nameof(decoder));

            decoder.GetThumbnail(out var value).ThrowOnError(false);
            if (value == null)
                return null;

            return new ComObject<IWICBitmapSource>(value);
        }

        public static IComObject<IWICBitmapSource> GetPreview(this IComObject<IWICBitmapDecoder> decoder) => GetPreview(decoder?.Object);
        public static IComObject<IWICBitmapSource> GetPreview(this IWICBitmapDecoder decoder)
        {
            if (decoder == null)
                throw new ArgumentNullException(nameof(decoder));

            decoder.GetPreview(out var value).ThrowOnError(false);
            if (value == null)
                return null;

            return new ComObject<IWICBitmapSource>(value);
        }
    }
}
