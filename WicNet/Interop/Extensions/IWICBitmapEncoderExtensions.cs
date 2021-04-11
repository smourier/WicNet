using System;

namespace WicNet.Interop
{
    public static class IWICBitmapEncoderExtensions
    {
        public static Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> CreateNewFrame(this IComObject<IWICBitmapEncoder> encoder) => CreateNewFrame(encoder?.Object);
        public static Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>> CreateNewFrame(this IWICBitmapEncoder encoder)
        {
            if (encoder == null)
                throw new ArgumentNullException(nameof(encoder));

            encoder.CreateNewFrame(out var frame, out var bag).ThrowOnError();
            return new Tuple<IComObject<IWICBitmapFrameEncode>, IComObject<IPropertyBag2>>(new ComObject<IWICBitmapFrameEncode>(frame), new ComObject<IPropertyBag2>(bag));
        }
    }
}
