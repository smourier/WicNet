namespace WicNet;

public sealed class WicBitmapLock : InterlockedComObject<IWICBitmapLock>
{
    public WicBitmapLock(IComObject<IWICBitmapLock> comObject)
        : base(comObject)
    {
        NativeObject.GetDataPointer(out var size, out var ptr);
        DataSize = size;
        DataPointer = ptr;

        NativeObject.GetPixelFormat(out var format);
        PixelFormat = WicImagingComponent.FromClsid<WicPixelFormat>(format);

        NativeObject.GetSize(out var width, out var height);
        Width = width;
        Height = height;

        NativeObject.GetStride(out var stride);
        Stride = stride;
    }

    public nint DataPointer { get; private set; }
    public uint DataSize { get; private set; }
    public WicPixelFormat? PixelFormat { get; private set; }
    public uint Width { get; private set; }
    public uint Height { get; private set; }
    public uint Stride { get; private set; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        DataPointer = 0;
        DataSize = 0;
        PixelFormat = null;
        Width = 0;
        Height = 0;
        Stride = 0;
    }

    public void WriteRectangle(int left, int top, byte[] input, uint inputStride, uint inputIndex = 0, uint? height = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(left);
        ArgumentOutOfRangeException.ThrowIfNegative(top);
        ArgumentNullException.ThrowIfNull(input);
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
