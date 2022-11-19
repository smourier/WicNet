using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PixelFormatModel : ImagingComponentModel
    {
        private readonly WicPixelFormat _format;
        private readonly Lazy<PixelFormatModel[]> _targetConversions;
        private readonly Lazy<PixelFormatModel[]> _sourceConversions;

        public PixelFormatModel(WicPixelFormat format)
            : base(format)
        {
            ArgumentNullException.ThrowIfNull(format);
            _format = format;
            using var ctx = _format.GetColorContext();
            if (ctx != null)
            {
                ColorContext = new ColorContextModel(ctx);
            }

            _targetConversions = new Lazy<PixelFormatModel[]>(GetTargetConversions);
            _sourceConversions = new Lazy<PixelFormatModel[]>(GetSourceConversions);
        }

        private PixelFormatModel[] GetTargetConversions()
        {
            var list = new HashSet<PixelFormatModel>();
            foreach (var kv in PixelFormatConverter._allConversions.Value)
            {
                foreach (var cv in kv.Value)
                {
                    if (cv.From.Guid == Guid)
                    {
                        list.Add(cv.To);
                    }
                }
            }
            return list.ToArray();
        }

        private PixelFormatModel[] GetSourceConversions()
        {
            var list = new HashSet<PixelFormatModel>();
            foreach (var kv in PixelFormatConverter._allConversions.Value)
            {
                foreach (var cv in kv.Value)
                {
                    if (cv.To.Guid == Guid)
                    {
                        list.Add(cv.From);
                    }
                }
            }
            return list.ToArray();
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

        [DisplayName("Supported Target Formats")]
        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public PixelFormatModel[] TargetConversions => _targetConversions.Value;

        [DisplayName("Supported Source Formats")]
        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public PixelFormatModel[] SourceConversions => _sourceConversions.Value;

        public override string ToString() => FriendlyName + " - " + NumericRepresentation;
    }
}
