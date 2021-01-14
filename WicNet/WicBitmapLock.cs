using System;
using WicNet.Interop;

namespace WicNet
{
    public sealed class WicBitmapLock
    {
        public WicBitmapLock(object comObject)
        {
            using (var wrapper = new ComObjectWrapper<IWICBitmapLock>(comObject))
            {
                wrapper.Object.GetDataPointer(out var size, out var ptr);
                DataSize = size;
                DataPointer = ptr;

                wrapper.Object.GetPixelFormat(out var format);
                PixelFormat = WicImagingComponent.FromClsid<WicPixelFormat>(format);

                wrapper.Object.GetSize(out var width, out var height);
                Width = width;
                Height = height;

                wrapper.Object.GetStride(out var stride);
                Stride = stride;
            }
        }

        public IntPtr DataPointer { get; }
        public uint DataSize { get; }
        public WicPixelFormat PixelFormat { get; }
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
    }
}
