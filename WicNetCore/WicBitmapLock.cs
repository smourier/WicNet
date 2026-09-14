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

    public unsafe Span<byte> AsSpan()
    {
        ObjectDisposedException.ThrowIf(DataPointer == 0, this);
        return new Span<byte>((void*)DataPointer, checked((int)DataSize));
    }

    public unsafe ReadOnlySpan<byte> AsReadOnlySpan()
    {
        ObjectDisposedException.ThrowIf(DataPointer == 0, this);
        return new ReadOnlySpan<byte>((void*)DataPointer, checked((int)DataSize));
    }

    public void WriteRectangle(int left, int top, byte[] input, uint inputStride, uint inputIndex = 0, uint? height = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(left);
        ArgumentOutOfRangeException.ThrowIfNegative(top);
        ArgumentNullException.ThrowIfNull(input);
        if (PixelFormat == null)
            throw new InvalidOperationException();

        WriteRectangle(left, top, input.AsSpan(), inputStride, inputIndex, height);
    }

    public unsafe void WriteRectangle(int left, int top, ReadOnlySpan<byte> input, uint inputStride, uint inputIndex = 0, uint? height = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(left);
        ArgumentOutOfRangeException.ThrowIfNegative(top);
        if (PixelFormat == null)
            throw new InvalidOperationException();

        if (height == 0)
            return;

        ArgumentOutOfRangeException.ThrowIfZero(inputStride);
        if (inputIndex > (uint)input.Length)
            throw new ArgumentOutOfRangeException(nameof(inputIndex));

        var rows = height ?? ((uint)input.Length - inputIndex) / inputStride;
        if (rows == 0)
            return;

        var bitOffset = (ulong)(uint)left * PixelFormat.BitsPerPixel;
        if ((uint)left >= Width || (bitOffset & 7) != 0)
            throw new ArgumentOutOfRangeException(nameof(left));

        if ((uint)top >= Height || rows > Height - (uint)top)
            throw new ArgumentOutOfRangeException(nameof(height));

        var byteOffset = bitOffset / 8;
        if (byteOffset > Stride || inputStride > Stride - byteOffset)
            throw new ArgumentOutOfRangeException(nameof(inputStride));

        if (inputIndex + (ulong)rows * inputStride > (ulong)input.Length)
            throw new ArgumentException("The source buffer does not contain all requested rows.", nameof(input));

        var destinationOffset = (ulong)(uint)top * Stride + byteOffset;
        var destinationEnd = destinationOffset + (ulong)(rows - 1) * Stride + inputStride;
        if (destinationEnd > DataSize)
            throw new ArgumentException("The requested rows exceed the locked buffer.", nameof(height));

        for (uint y = 0; y < rows; y++)
        {
            var sourceOffset = checked((int)(inputIndex + (ulong)y * inputStride));
            var destPtr = (byte*)DataPointer + destinationOffset + (ulong)y * Stride;
            input.Slice(sourceOffset, checked((int)inputStride))
                .CopyTo(new Span<byte>(destPtr, checked((int)inputStride)));
        }
    }
}
