using System;
using System.ComponentModel;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PixelFormatModel
    {
        private readonly WicPixelFormat _format;

        public PixelFormatModel(WicPixelFormat format)
        {
            ArgumentNullException.ThrowIfNull(format);
            _format = format;
            using var ctx = _format.GetColorContext();
            if (ctx != null)
            {
                ColorContext = new ColorContextModel(ctx);
            }
        }

        public Guid Guid => _format.Guid;

        [DisplayName("Color Context")]
        public ColorContextModel? ColorContext { get; }

        [DisplayName("Numeric Representation")]
        public string NumericRepresentation => _format.NumericRepresentation.GetEnumName().Decamelize();

        [DisplayName("Channel Count")]
        public int ChannelCount => _format.ChannelCount;

        [DisplayName("Bits Per Pixel")]
        public int BitsPerPixel => _format.BitsPerPixel;

        [DisplayName("Supports Transparency")]
        public bool SupportsTransparency => _format.SupportsTransparency;

        [DisplayName("Name")]
        public string ClsidName => _format.ClsidName;

        public override string ToString() => _format.ToString();
    }
}
