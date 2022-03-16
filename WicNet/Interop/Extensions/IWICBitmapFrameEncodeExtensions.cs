using System;
using System.Collections.Generic;
using System.Linq;

namespace DirectN
{
    public static class IWICBitmapFrameEncodeExtensions
    {
        public static void Initialize(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag) => Initialize(frameBag?.Item1, frameBag?.Item2);
        public static void Initialize(this IComObject<IWICBitmapFrameEncode> frame, IComObject<IPropertyBag2> bag = null) => Initialize(frame?.Object, bag?.Object);
        public static void Initialize(this IWICBitmapFrameEncode frame, IPropertyBag2 bag = null)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            if (bag != null)
            {
                frame.Initialize(bag).ThrowOnError();
            }
        }

        public static void SetPixelFormat(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag, Guid pixelFormat) => SetPixelFormat(frameBag?.Item1, pixelFormat);
        public static void SetPixelFormat(this IComObject<IWICBitmapFrameEncode> frame, Guid pixelFormat) => SetPixelFormat(frame?.Object, pixelFormat);
        public static void SetPixelFormat(this IWICBitmapFrameEncode frame, Guid pixelFormat)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.SetPixelFormat(pixelFormat).ThrowOnError();
        }

        public static void SetSize(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag, uint width, uint height) => SetSize(frameBag?.Item1, width, height);
        public static void SetSize(this IComObject<IWICBitmapFrameEncode> frame, uint width, uint height) => SetSize(frame?.Object, width, height);
        public static void SetSize(this IWICBitmapFrameEncode frame, uint width, uint height)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.SetSize(width, height).ThrowOnError();
        }

        public static void SetResolution(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag, double dpiX, double dpiY) => SetResolution(frameBag?.Item1, dpiX, dpiY);
        public static void SetResolution(this IComObject<IWICBitmapFrameEncode> frame, double dpiX, double dpiY) => SetResolution(frame?.Object, dpiX, dpiY);
        public static void SetResolution(this IWICBitmapFrameEncode frame, double dpiX, double dpiY)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.SetResolution(dpiX, dpiY).ThrowOnError();
        }

        public static void WriteSource(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag, IComObject<IWICBitmapSource> source, WICRect? sourceRectangle = null) => WriteSource(frameBag?.Item1, source, sourceRectangle);
        public static void WriteSource(this IComObject<IWICBitmapFrameEncode> frame, IComObject<IWICBitmapSource> source, WICRect? sourceRectangle = null) => WriteSource(frame?.Object, source?.Object, sourceRectangle);
        public static void WriteSource(this IWICBitmapFrameEncode frame, IWICBitmapSource source, WICRect? sourceRectangle = null)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            using (var mem = new ComMemory(sourceRectangle))
            {
                frame.WriteSource(source, mem.Pointer).ThrowOnError();
            }
        }

        public static void WritePixels(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag, uint lineCount, uint stride, byte[] pixels) => WritePixels(frameBag?.Item1, lineCount, stride, pixels);
        public static void WritePixels(this IComObject<IWICBitmapFrameEncode> frame, uint lineCount, uint stride, byte[] pixels) => WritePixels(frame?.Object, lineCount, stride, pixels);
        public static void WritePixels(this IWICBitmapFrameEncode frame, uint lineCount, uint stride, byte[] pixels)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            if (pixels == null)
                throw new ArgumentNullException(nameof(pixels));

            frame.WritePixels(lineCount, stride, pixels.Length, pixels).ThrowOnError();
        }

        public static IComObject<IWICMetadataQueryWriter> GetMetadataQueryWriter(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag) => GetMetadataQueryWriter(frameBag?.Item1);
        public static IComObject<IWICMetadataQueryWriter> GetMetadataQueryWriter(this IComObject<IWICBitmapFrameEncode> frame) => GetMetadataQueryWriter(frame?.Object);
        public static IComObject<IWICMetadataQueryWriter> GetMetadataQueryWriter(this IWICBitmapFrameEncode frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.GetMetadataQueryWriter(out var writer).ThrowOnError();
            return new ComObject<IWICMetadataQueryWriter>(writer);
        }

        public static void Commit(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag) => Commit(frameBag?.Item1);
        public static void Commit(this IComObject<IWICBitmapFrameEncode> frame) => Commit(frame?.Object);
        public static void Commit(this IWICBitmapFrameEncode frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.Commit().ThrowOnError();
        }

        public static void SetColorContexts(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag, IEnumerable<IWICColorContext> contexts) => SetColorContexts(frameBag?.Item1, contexts);
        public static void SetColorContexts(this IComObject<IWICBitmapFrameEncode> frame, IEnumerable<IWICColorContext> contexts) => SetColorContexts(frame?.Object, contexts);
        public static void SetColorContexts(this IWICBitmapFrameEncode frame, IEnumerable<IWICColorContext> contexts)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.SetColorContexts((contexts?.Count()).GetValueOrDefault(), contexts?.ToArray()).ThrowOnError();
        }

        public static void SetPalette(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag, IComObject<IWICPalette> palette) => SetPalette(frameBag?.Item1, palette);
        public static void SetPalette(this IComObject<IWICBitmapFrameEncode> frame, IComObject<IWICPalette> palette) => SetPalette(frame?.Object, palette?.Object);
        public static void SetPalette(this IWICBitmapFrameEncode frame, IWICPalette palette)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.SetPalette(palette).ThrowOnError();
        }

        public static void SetThumbnail(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag, IComObject<IWICBitmapSource> thumbnail) => SetThumbnail(frameBag?.Item1, thumbnail);
        public static void SetThumbnail(this IComObject<IWICBitmapFrameEncode> frame, IComObject<IWICBitmapSource> thumbnail) => SetThumbnail(frame?.Object, thumbnail?.Object);
        public static void SetThumbnail(this IWICBitmapFrameEncode frame, IWICBitmapSource thumbnail)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.SetThumbnail(thumbnail).ThrowOnError();
        }
    }
}
