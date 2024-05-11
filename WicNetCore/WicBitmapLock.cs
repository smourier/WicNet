using System;
using System.Runtime.InteropServices;
using DirectN;
using DirectNAot.Extensions.Com;

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

    public nint DataPointer { get; }
    public uint DataSize { get; }
    public WicPixelFormat? PixelFormat { get; }
    public int Width { get; }
    public int Height { get; }
    public int Stride { get; }

    public void WriteRectangle(int left, int top, byte[] input, uint inputStride, uint inputIndex = 0, uint? height = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(left);
        ArgumentOutOfRangeException.ThrowIfNegative(top);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(inputIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(inputStride);
        if (height < 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        if (PixelFormat == null)
            throw new InvalidOperationException();

        height ??= (uint)input.Length / inputStride;
        var bpp = PixelFormat.BitsPerPixel;
        var offset = inputIndex;
        for (var y = 0; y < height; y++)
        {
            var ptr = nint.Add(DataPointer, (int)((top + y) * Stride + left * bpp / 8));
            Marshal.Copy(input, (int)offset, ptr, (int)inputStride);
            offset += inputStride;
        }
    }
}
