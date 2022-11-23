using System;
using System.Collections.Generic;
using System.ComponentModel;
using DirectN;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DeviceContextModel
    {
        private readonly string _text;

        public DeviceContextModel(IComObject<ID2D1DeviceContext> context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _text = context.InterfaceType.Name;
            var fmt = context.GetPixelFormat();
            PixelFormat = fmt.format;
            PixelAlphaMode = fmt.alphaMode;
            PixelSize = context.GetPixelSize();
            Size = context.GetSize();
            PrimitiveBlend = context.Object.GetPrimitiveBlend();
            UnitMode = context.Object.GetUnitMode();
            MaximumBitmapSize = context.Object.GetMaximumBitmapSize();
            context.Object.GetDpi(out var x, out var y);
            Dpi = new D2D_SIZE_F(x, y);

            var formats = new List<DXGI_FORMAT>();
            foreach (DXGI_FORMAT format in Enum.GetValues(typeof(DXGI_FORMAT)))
            {
                if (format == DXGI_FORMAT.DXGI_FORMAT_UNKNOWN || format == DXGI_FORMAT.DXGI_FORMAT_FORCE_UINT)
                    continue;

                if (context.Object.IsDxgiFormatSupported(format))
                {
                    formats.Add(format);
                }
            }
            DxgiFormats = formats.ToArray();

            var precisions = new List<D2D1_BUFFER_PRECISION>();
            foreach (D2D1_BUFFER_PRECISION precision in Enum.GetValues(typeof(D2D1_BUFFER_PRECISION)))
            {
                if (precision == D2D1_BUFFER_PRECISION.D2D1_BUFFER_PRECISION_FORCE_DWORD || precision == D2D1_BUFFER_PRECISION.D2D1_BUFFER_PRECISION_UNKNOWN)
                    continue;

                if (context.Object.IsBufferPrecisionSupported(precision))
                {
                    precisions.Add(precision);
                }
            }
            BufferPrecisions = precisions.ToArray();
        }

        [DisplayName("Pixel Format")]
        public DXGI_FORMAT PixelFormat { get; }

        [DisplayName("Pixel Alpha Mode")]
        public D2D1_ALPHA_MODE PixelAlphaMode { get; }

        [DisplayName("Pixel Size")]
        public D2D_SIZE_U PixelSize { get; }

        [DisplayName("Primitive Blend")]
        public D2D1_PRIMITIVE_BLEND PrimitiveBlend { get; }

        [DisplayName("Unit Mode")]
        public D2D1_UNIT_MODE UnitMode { get; }

        [DisplayName("Maximum Bitmap Size")]
        public uint MaximumBitmapSize { get; }

        [DisplayName("Antialias Mode")]
        public D2D1_ANTIALIAS_MODE AntialiasMode { get; }

        [DisplayName("Dxgi Formats Supported")]
        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public DXGI_FORMAT[] DxgiFormats { get; }

        [DisplayName("Buffer Precisions Supported")]
        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public D2D1_BUFFER_PRECISION[] BufferPrecisions { get; }

        public D2D_SIZE_F Dpi { get; }
        public D2D_SIZE_F Size { get; }

        public override string ToString() => _text;
    }
}
