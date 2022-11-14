using System;
using System.ComponentModel;
using System.Linq;
using WicNet;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PixelFormatConverter : ImagingComponentModel
    {
        private readonly WicPixelFormatConverter _converter;

        public PixelFormatConverter(WicPixelFormatConverter converter)
            : base(converter)
        {
            ArgumentNullException.ThrowIfNull(converter);
            _converter = converter;
            PixelFormats = _converter.PixelFormatsList.Select(f => new PixelFormatModel(f)).ToArray();
        }

        [DisplayName("Pixel Formats")]
        public PixelFormatModel[] PixelFormats { get; }
    }
}
