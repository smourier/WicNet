using System;
using System.Collections.Generic;
using DirectN;

namespace WicNet
{
    public sealed class WicPixelFormatConverter : WicImagingComponent, IDisposable
    {
        private readonly IComObject<IWICFormatConverterInfo> _comObject;
        private readonly Lazy<IReadOnlyList<WicPixelFormat>> _pixelFormatsList;

        public WicPixelFormatConverter(object comObject)
            : base(comObject)
        {
            _comObject = new ComObjectWrapper<IWICFormatConverterInfo>(comObject).ComObject;
            _comObject.Object.GetPixelFormats(0, null, out var len);
            var pf = new Guid[len];
            if (len > 0)
            {
                _comObject.Object.GetPixelFormats(len, pf, out _);
            }

            PixelFormats = pf;
            _pixelFormatsList = new Lazy<IReadOnlyList<WicPixelFormat>>(GetPixelFormatsList, true);
        }

        public IReadOnlyList<Guid> PixelFormats { get; }
        public IReadOnlyList<WicPixelFormat> PixelFormatsList => _pixelFormatsList.Value;

        private IReadOnlyList<WicPixelFormat> GetPixelFormatsList()
        {
            var list = new List<WicPixelFormat>();
            foreach (var pf in PixelFormats)
            {
                var format = WicPixelFormat.FromClsid(pf);
                if (format != null)
                {
                    list.Add(format);
                }
            }

            list.Sort();
            return list.AsReadOnly();
        }

        public bool CanConvert(Guid from, Guid to)
        {
            using (var cvt = _comObject.CreateInstance())
            {
                cvt.Object.CanConvert(from, to, out var can).ThrowOnError();
                return can;
            }
        }

        public IComObject<IWICFormatConverter> Convert(WicBitmapSource source, Guid targetFormat, WICBitmapDitherType ditherType = WICBitmapDitherType.WICBitmapDitherTypeNone, WicPalette palette = null, double alphaThresholdPercent = 0, WICBitmapPaletteType paletteTranslate = WICBitmapPaletteType.WICBitmapPaletteTypeCustom)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            WicPalette pal = null;
            var p = palette;
            if (p != null && p.ColorCount > 0)
            {
                // we must clone it
                pal = p.CopyColors();
            }

            var cvt = _comObject.CreateInstance();
            cvt.Object.Initialize(source.ComObject.Object, targetFormat, ditherType, pal?.ComObject.Object, alphaThresholdPercent, paletteTranslate).ThrowOnError();
            return cvt;
        }

        public void Dispose() => _comObject?.Dispose();
    }
}
