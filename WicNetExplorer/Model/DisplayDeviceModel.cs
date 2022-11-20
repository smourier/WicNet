using System;
using System.ComponentModel;
using System.Linq;
using DirectN;
using WicNet.Utilities;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DisplayDeviceModel
    {
        private readonly DISPLAY_DEVICE _device;

        public DisplayDeviceModel(DISPLAY_DEVICE device)
        {
            ArgumentNullException.ThrowIfNull(device);
            _device = device;
            Modes = _device.GetModes().ToArray();
        }

        public string Name => _device.DeviceName;
        public string Id => _device.DeviceID;
        public string Key => _device.DeviceKey;
        public string Adapter => _device.DeviceString;

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Monitor Monitor => _device.Monitor;

        [DisplayName("Is Primary")]
        public bool IsPrimary => _device.IsPrimary;

        [DisplayName("Current Mode")]
        public DEVMODE CurrentMode => _device.CurrentSettings;

        [DisplayName("Registry Mode")]
        public DEVMODE RegistryMode => _device.RegistrySettings;

        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public DEVMODE[] Modes { get; }

        public override string ToString() => Name;
    }
}
