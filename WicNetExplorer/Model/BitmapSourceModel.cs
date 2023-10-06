using System;
using System.ComponentModel;
using System.Linq;
using DirectN;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BitmapSourceModel : IDisposable
    {
        private bool _disposedValue;

        public BitmapSourceModel(WicBitmapSource source)
        {
            ArgumentNullException.ThrowIfNull(source);
            Size = source.Size;
            Stride = source.Stride;
            PixelFormat = new PixelFormatModel(source.WicPixelFormat);
            DpiX = source.DpiX;
            DpiY = source.DpiY;
            var palette = source.Palette;
            if (palette != null)
            {
                Palette = new PaletteModel(palette);
            }

            MemorySize = Stride * source.Height;
            ColorContexts = source.GetColorContexts().Select(cc => new ColorContextModel(cc)).ToArray();

            if (EnableThumbnail)
            {
                var th = source.GetThumbnail(); // no using here
                if (th != null)
                {
                    Thumbnail = new PreviewBitmapSourceModel(th);
                }
            }
        }

        protected virtual bool EnableThumbnail => true;

        public WicIntSize Size { get; }
        public int Stride { get; }
        public PaletteModel? Palette { get; }
        public BitmapSourceModel? Thumbnail { get; }

        [DisplayName("Color Contexts")]
        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public ColorContextModel[] ColorContexts { get; } = Array.Empty<ColorContextModel>();

        [DisplayName("Dpi X")]
        public double DpiX { get; }

        [DisplayName("Dpi Y")]
        public double DpiY { get; }

        [DisplayName("Pixel Format")]
        public PixelFormatModel? PixelFormat { get; }

        [DisplayName("Image Size")]
        public long MemorySize { get; }

        public override string ToString() => Size.ToString();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Thumbnail?.Dispose();
                }

                _disposedValue = true;
            }
        }

        ~BitmapSourceModel() { Dispose(disposing: false); }
        public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
    }
}
