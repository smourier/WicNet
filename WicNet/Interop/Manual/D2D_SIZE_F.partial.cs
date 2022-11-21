using System;
using System.ComponentModel;
using System.Diagnostics;
using WicNet;

namespace DirectN
{
    [DebuggerDisplay("{ToString(),nq}")]
    public partial struct D2D_SIZE_F : IEquatable<D2D_SIZE_F>
    {
        public D2D_SIZE_F(float width, float height)
        {
#if DEBUG
            if (width.IsInvalid())
                throw new ArgumentException(null, nameof(width));

            if (height.IsInvalid())
                throw new ArgumentException(null, nameof(height));

            if (width < 0)
                throw new ArgumentException(null, nameof(width));

            if (height < 0)
                throw new ArgumentException(null, nameof(height));

#endif

            this.width = width;
            this.height = height;
        }

        public D2D_SIZE_F(WicIntSize size)
            : this(size.Width, size.Height)
        {
        }

        public D2D_SIZE_F(double width, double height)
            : this((float)width, (float)height)
        {
        }

        public override string ToString() => "W: " + width + " H: " + height;

        [Browsable(false)]
        public bool IsZero => width.Equals(0f) && height.Equals(0f);

        [Browsable(false)]
        public bool IsEmpty => width.Equals(0f) || height.Equals(0f);

        [Browsable(false)]
        public bool IsValid => !IsInvalid;

        [Browsable(false)]
        public bool IsInvalid => width.IsInvalid() || height.IsInvalid();

        [Browsable(false)]
        public bool IsAllInvalid => width.IsInvalid() && height.IsInvalid();

        [Browsable(false)]
        public bool IsSet => width.IsSet() && height.IsSet();

        [Browsable(false)]
        public bool IsNotSet => width.IsNotSet() || height.IsNotSet();

        [Browsable(false)]
        public bool IsMax => width.IsMax() || height.IsMax();

        [Browsable(false)]
        public bool IsMin => width.IsMin() || height.IsMin();
        public bool Equals(D2D_SIZE_F other) => width.Equals(other.width) && height.Equals(other.height);
        public override bool Equals(object obj) => (obj is D2D_SIZE_F sz && Equals(sz));
        public override int GetHashCode() => width.GetHashCode() ^ height.GetHashCode();
        public static bool operator ==(D2D_SIZE_F left, D2D_SIZE_F right) => left.Equals(right);
        public static bool operator !=(D2D_SIZE_F left, D2D_SIZE_F right) => !left.Equals(right);
        public static D2D_SIZE_F operator +(D2D_SIZE_F left, D2D_SIZE_F right) => new D2D_SIZE_F(left.width + right.width, left.height + right.height);
        public static D2D_SIZE_F operator -(D2D_SIZE_F left, D2D_SIZE_F right) => new D2D_SIZE_F(left.width - right.width, left.height - right.height);

        public tagSIZE TotagSize() => new tagSIZE(width, height);
        public D2D_SIZE_U ToD2D_SIZE_U() => new D2D_SIZE_U(width, height);
        public D2D_SIZE_F ToD2D_SIZE_F() => new D2D_SIZE_F(width, height);
        public D2D_VECTOR_2F ToD2D_VECTOR_2F() => new D2D_VECTOR_2F(width, height);

#if DEBUG
        public static readonly D2D_SIZE_F Invalid = new D2D_SIZE_F { width = float.NaN, height = float.NaN };
#else
        public static readonly D2D_SIZE_F Invalid = new D2D_SIZE_F(float.NaN, float.NaN);
#endif

#if DEBUG
        public static readonly D2D_SIZE_F MaxValue = new D2D_SIZE_F { width = float.MaxValue, height = float.MaxValue };
        public static readonly D2D_SIZE_F PositiveInfinity = new D2D_SIZE_F { width = float.PositiveInfinity, height = float.PositiveInfinity };
#else
        public static readonly D2D_SIZE_F MaxValue = new D2D_SIZE_F(float.MaxValue, float.MaxValue);
        public static readonly D2D_SIZE_F PositiveInfinity = new D2D_SIZE_F(float.PositiveInfinity, float.PositiveInfinity);
#endif

        public D2D_SIZE_F PixelToHiMetric()
        {
            var dpi = D2D1Functions.Dpi;
            return new D2D_SIZE_F(tagSIZE.HIMETRIC_PER_INCH * width / dpi.width, tagSIZE.HIMETRIC_PER_INCH * height / dpi.height);
        }

        public D2D_SIZE_F HiMetricToPixel()
        {
            var dpi = D2D1Functions.Dpi;
            return new D2D_SIZE_F(width * dpi.width / tagSIZE.HIMETRIC_PER_INCH, height * dpi.height / tagSIZE.HIMETRIC_PER_INCH);
        }

        public D2D_SIZE_F PixelToDip()
        {
            var scale = D2D1Functions.DpiScale;
            return new D2D_SIZE_F(width / scale.width, height / scale.height);
        }

        public D2D_SIZE_F DipToPixel()
        {
            var scale = D2D1Functions.DpiScale;
            return new D2D_SIZE_F(width * scale.width, height * scale.height);
        }

        public D2D_SIZE_F GetScaleFactor(int? width = null, int? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default)
        {
            float? fw = width.HasValue ? width : null;
            float? fh = height.HasValue ? height : null;
            return GetScaleFactor(fw, fh, options);
        }

        public D2D_SIZE_F GetScaleFactor(uint? width = null, uint? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default)
        {
            float? fw = width.HasValue ? width : null;
            float? fh = height.HasValue ? height : null;
            return GetScaleFactor(fw, fh, options);
        }

        public D2D_SIZE_F GetScaleFactor(float? width = null, float? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default)
        {
            if (width.HasValue && width.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height.HasValue && height.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            if (this.width == 0 || this.height == 0 || (!width.HasValue && !height.HasValue))
                return new D2D_SIZE_F(1, 1);

            var scaleW = this.width == 0 ? 0 : (width ?? float.PositiveInfinity) / this.width;
            var scaleH = this.height == 0 ? 0 : (height ?? float.PositiveInfinity) / this.height;
            if (!width.HasValue)
            {
                scaleW = scaleH;
            }
            else if (!height.HasValue)
            {
                scaleH = scaleW;
            }
            else if (options.HasFlag(WicBitmapScaleOptions.Uniform))
            {
                var minscale = scaleW < scaleH ? scaleW : scaleH;
                scaleW = scaleH = minscale;
            }
            else if (options.HasFlag(WicBitmapScaleOptions.UniformToFill))
            {
                var maxscale = scaleW > scaleH ? scaleW : scaleH;
                scaleW = scaleH = maxscale;
            }

            if (options.HasFlag(WicBitmapScaleOptions.UpOnly))
            {
                if (scaleW < 1)
                {
                    scaleW = 1;
                }

                if (scaleH < 1)
                {
                    scaleH = 1;
                }
            }

            if (options.HasFlag(WicBitmapScaleOptions.DownOnly))
            {
                if (scaleW > 1)
                {
                    scaleW = 1;
                }

                if (scaleH > 1)
                {
                    scaleH = 1;
                }
            }
            return new D2D_SIZE_F(scaleW, scaleH);
        }
    }
}
