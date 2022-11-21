using System;
using System.ComponentModel;
using System.Linq;
using WicNet;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MetadataHandlerModel : ImagingComponentModel
    {
        private readonly WicMetadataHandler _handler;

        public MetadataHandlerModel(WicMetadataHandler handler)
            : base(handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
            _handler = handler;
            ContainerFormats = _handler.ContainerFormats.ToArray();
        }

        public Guid Guid => _handler.Guid;
        public Guid[] ContainerFormats { get; }

        [DisplayName("Device Manufacturer")]
        public string DeviceManufacturer => _handler.DeviceManufacturer;

        [DisplayName("Device Models")]
        public string DeviceModels => _handler.DeviceModels;

        [DisplayName("Supports Padding")]
        public bool SupportsPadding => _handler.SupportsPadding;

        [DisplayName("Requires Fixed Size")]
        public bool RequiresFixedSize => _handler.RequiresFixedSize;

        [DisplayName("Requires Full Stream")]
        public bool RequiresFullStream => _handler.RequiresFullStream;
    }
}
