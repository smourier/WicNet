using System;
using System.ComponentModel;
using DirectN;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BitmapModel
    {
        public BitmapModel(IComObject<ID2D1Bitmap1> bitmap)
        {
            ArgumentNullException.ThrowIfNull(bitmap);
            Dpi = bitmap.GetDpi();
            var fmt = bitmap.GetPixelFormat(); ;
            PixelFormat = fmt.format.GetEnumName();
            AlphaMode = fmt.alphaMode.GetEnumName();
            PixelSize = bitmap.GetPixelSize();
            Size = bitmap.GetSize();
            Options = bitmap.GetOptions().GetEnumName();
            using var ctx = bitmap.GetColorContext();
            if (ctx != null)
            {
                ColorContext = new BitmapColorContextModel(ctx);
            }
        }

        public D2D_SIZE_F Dpi { get; }
        public D2D_SIZE_F Size { get; }
        public string Options { get; }

        [DisplayName("Alpha Mode")]
        public string AlphaMode { get; }

        [DisplayName("Color Context")]
        public BitmapColorContextModel? ColorContext { get; }

        [DisplayName("Pixel Size")]
        public D2D_SIZE_U PixelSize { get; }

        [DisplayName("Pixel Format")]
        public string PixelFormat { get; }

        public override string ToString() => PixelFormat + " " + Size.ToString();
    }
}
