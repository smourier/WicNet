using System;
using System.ComponentModel;
using System.Linq;
using WicNet;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BitmapSourceModel
    {
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
            using var th = source.GetThumbnail();
            if (th != null)
            {
                Thumbnail = new BitmapSourceModel(th);
            }
        }

        public WicIntSize Size { get; }
        public int Stride { get; }
        public PaletteModel? Palette { get; }
        public BitmapSourceModel? Thumbnail { get; }

        [DisplayName("Color Contexts")]
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
    }
}
