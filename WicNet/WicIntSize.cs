using System;
using DirectN;

namespace WicNet
{
    public struct WicIntSize
    {
        public uint Width;
        public uint Height;

        public WicIntSize(uint width, uint height)
        {
            Width = width;
            Height = height;
        }

        public WicIntSize(int width, int height)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            Width = (uint)width;
            Height = (uint)height;
        }

        public override string ToString() => Width + " x " + Height;
        public override bool Equals(object obj) => obj is WicIntSize size && size.Width == Width && size.Height == Height;
        public override int GetHashCode() => Width.GetHashCode() ^ Height.GetHashCode();

        public static bool operator ==(WicIntSize left, WicIntSize right) => left.Equals(right);
        public static bool operator !=(WicIntSize left, WicIntSize right) => !left.Equals(right);

        public D2D_SIZE_F GetScaleFactor(uint? width = null, uint? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default) => new D2D_SIZE_F(Width, Height).GetScaleFactor(width, height, options);
        public D2D_SIZE_F GetScaleFactor(int? width = null, int? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default) => new D2D_SIZE_F(Width, Height).GetScaleFactor(width, height, options);
    }
}
