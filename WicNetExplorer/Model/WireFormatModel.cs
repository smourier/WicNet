using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using Windows.Devices.Display.Core;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [SupportedOSPlatform("windows10.0.17763")]
    public class WireFormatModel
    {
        private readonly DisplayWireFormat _format;

        public WireFormatModel(DisplayWireFormat format)
        {
            ArgumentNullException.ThrowIfNull(format);
            _format = format;
        }

        [DisplayName("Bits Per Channel")]
        public int BitsPerChannel => _format.BitsPerChannel;

        [DisplayName("Color Space")]
        public DisplayWireFormatColorSpace ColorSpace => _format.ColorSpace;

        [DisplayName("Electro-optical Transfer Function")]
        public DisplayWireFormatEotf Eotf => _format.Eotf;

        [DisplayName("Hdr Metadata")]
        public DisplayWireFormatHdrMetadata HdrMetadata => _format.HdrMetadata;

        [DisplayName("Pixel Encoding")]
        public DisplayWireFormatPixelEncoding PixelEncoding => _format.PixelEncoding;

        public override string ToString()
        {
            var s = BitsPerChannel + "bpc " + ColorSpace + " " + PixelEncoding + " " + Eotf;
            if (HdrMetadata != DisplayWireFormatHdrMetadata.None)
            {
                s += " " + HdrMetadata;
            }
            return s;
        }
    }
}
