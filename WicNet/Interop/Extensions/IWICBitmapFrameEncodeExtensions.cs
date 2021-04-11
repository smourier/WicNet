using System;

namespace WicNet.Interop
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

        public static IComObject<IWICMetadataQueryWriter> GetMetadataQueryWriter(this Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> frameBag) => GetMetadataQueryWriter(frameBag?.Item1);
        public static IComObject<IWICMetadataQueryWriter> GetMetadataQueryWriter(this IComObject<IWICBitmapFrameEncode> frame) => GetMetadataQueryWriter(frame?.Object);
        public static IComObject<IWICMetadataQueryWriter> GetMetadataQueryWriter(this IWICBitmapFrameEncode frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.GetMetadataQueryWriter(out var writer).ThrowOnError();
            return new ComObject<IWICMetadataQueryWriter>(writer);
        }
    }
}
