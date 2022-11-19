using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WicNet;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PixelFormatConverter : ImagingComponentModel
    {
        private readonly WicPixelFormatConverter _converter;

        internal static readonly Lazy<ConcurrentDictionary<Guid, List<ConversionModel>>> _allConversions = new(GetAllConversions);

        private static ConcurrentDictionary<Guid, List<ConversionModel>> GetAllConversions()
        {
            var dic = new ConcurrentDictionary<Guid, List<ConversionModel>>();
            var formats = WicImagingComponent.AllComponents.OfType<WicPixelFormat>().Select(f => f.Guid).ToArray();
            foreach (var converter in WicImagingComponent.AllComponents.OfType<WicPixelFormatConverter>())
            {
                foreach (var from in formats)
                {
                    foreach (var to in formats)
                    {
                        if (from == to)
                            continue;

                        if (converter.CanConvert(from, to))
                        {
                            if (!dic.TryGetValue(converter.Clsid, out var list))
                            {
                                list = new List<ConversionModel>();
                                dic[converter.Clsid] = list;
                            }
                            list.Add(new ConversionModel(from, to));
                        }
                    }
                }
            }
            return dic;
        }

        public PixelFormatConverter(WicPixelFormatConverter converter)
            : base(converter)
        {
            ArgumentNullException.ThrowIfNull(converter);
            _converter = converter;
            PixelFormats = _converter.PixelFormatsList.Select(f => new PixelFormatModel(f)).ToArray();
            if (_allConversions.Value.TryGetValue(Clsid, out var conversions))
            {
                Conversions = conversions.ToArray();
            }
            else
            {
                Conversions = Array.Empty<ConversionModel>();
            }
        }

        [DisplayName("Pixel Formats")]
        public PixelFormatModel[] PixelFormats { get; }

        [DisplayName("Supported Conversions")]
        public ConversionModel[] Conversions { get; }
    }
}
