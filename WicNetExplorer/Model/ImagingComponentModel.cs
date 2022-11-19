using System;
using System.ComponentModel;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ImagingComponentModel : ICollectionFormItem
    {
        private readonly WicImagingComponent _component;

        public ImagingComponentModel(WicImagingComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            _component = component;
        }

        public Guid Clsid => _component.Clsid;

        [DisplayName("Signing Status")]
        public string SigningStatus => _component.SigningStatus.GetEnumName("WICComponent");

        [DisplayName("Friendly Name")]
        public string FriendlyName => _component.FriendlyName;
        public string Author => _component.Author;
        public string Version => _component.Version;

        [DisplayName("Spec Version")]
        public string SpecVersion => _component.SpecVersion;

        [DisplayName("Name")]
        public string ClsidName => _component.ClsidName;

        string? ICollectionFormItem.TypeName
        {
            get
            {
                var name = GetType().Name;
                if (name.EndsWith("Model"))
                    return name.Substring(0, name.Length - 5).Decamelize();

                return name.Decamelize();
            }
        }

        string ICollectionFormItem.Name => ToString();
        object ICollectionFormItem.Value => this;

        public override string ToString() => FriendlyName;

        public static ImagingComponentModel From(WicImagingComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);
            if (component is WicPixelFormat format)
                return new PixelFormatModel(format);

            if (component is WicPixelFormatConverter converter)
                return new PixelFormatConverter(converter);

            if (component is WicEncoder encoder)
                return new EncoderModel(encoder);

            if (component is WicDecoder decoder)
                return new DecoderModel(decoder);

            if (component is WicMetadataReader reader)
                return new MetadataReaderModel(reader);

            if (component is WicMetadataWriter writer)
                return new MetadataWriterModel(writer);

            throw new NotSupportedException();
        }
    }
}
