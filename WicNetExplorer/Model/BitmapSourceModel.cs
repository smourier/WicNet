using System;
using System.ComponentModel;
using WicNet;

namespace WicNetExplorer.Model
{
    public class BitmapSourceModel
    {
        public BitmapSourceModel(string filePath)
        {
            ArgumentNullException.ThrowIfNull(filePath);
            FilePath = filePath;
            using var source = WicBitmapSource.Load(filePath);
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
        }

        [DisplayName("File Path")]
        public string FilePath { get; }
        public WicIntSize Size { get; }
        public int Stride { get; }
        public PaletteModel? Palette { get; }

        [DisplayName("Dpi X")]
        public double DpiX { get; }

        [DisplayName("Dpi Y")]
        public double DpiY { get; }

        [DisplayName("Pixel Format")]
        public PixelFormatModel PixelFormat { get; }

        public override string ToString() => FilePath;
    }
}
