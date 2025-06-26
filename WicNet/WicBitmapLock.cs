using System;
using System.Runtime.InteropServices;
using DirectN;

namespace WicNet;

public sealed class WicBitmapLock
{
    public WicBitmapLock(object comObject)
    {
        using var wrapper = new ComObjectWrapper<IWICBitmapLock>(comObject);
        wrapper.Object.GetDataPointer(out var size, out var ptr);
        DataSize = size;
        DataPointer = ptr;

        wrapper.Object.GetPixelFormat(out var format);
        PixelFormat = WicImagingComponent.FromClsid<WicPixelFormat>(format);

        wrapper.Object.GetSize(out var width, out var height);
        Width = (int)width;
        Height = (int)height;

        wrapper.Object.GetStride(out var stride);
        Stride = (int)stride;
    }

    public IntPtr DataPointer { get; }
    public uint DataSize { get; }
    public WicPixelFormat PixelFormat { get; }
    public int Width { get; }
    public int Height { get; }
    public int Stride { get; }

    public void WriteRectangle(int left, int top, byte[] input, int inputStride, int inputIndex = 0, int? height = null)
    {
        if (left < 0)
            throw new ArgumentOutOfRangeException(nameof(left));

        if (top < 0)
            throw new ArgumentOutOfRangeException(nameof(top));

        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (inputIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(inputIndex));

        if (inputStride < 0)
            throw new ArgumentOutOfRangeException(nameof(inputStride));

        height ??= input.Length / inputStride;
        var bpp = PixelFormat.BitsPerPixel;
        var offset = inputIndex;
        for (var y = 0; y < height; y++)
        {
            var ptr = DataPointer + (top + y) * Stride + left * bpp / 8;
            Marshal.Copy(input, offset, ptr, inputStride);
            offset += inputStride;
        }
    }
}
