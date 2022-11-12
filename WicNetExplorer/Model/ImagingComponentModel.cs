using System;
using System.ComponentModel;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ImagingComponentModel
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

        public override string ToString() => _component.ToString();
    }
}
