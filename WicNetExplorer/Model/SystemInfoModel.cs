using System;
using System.ComponentModel;
using System.Linq;
using DirectN;
using WicNet.Utilities;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SystemInfoModel
    {
        public SystemInfoModel()
        {
            DisplayDevices = DISPLAY_DEVICE.Active.Select(d => new DisplayDeviceModel(d)).ToArray();
        }

        [DisplayName("Windows Version")]
        public string WindowsVersion => Environment.OSVersion.VersionString;

        [DisplayName("Kernel Version")]
        public string KernelVersion => WindowsUtilities.KernelVersion.ToString();

        [DisplayName("Display Devices")]
        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public DisplayDeviceModel[] DisplayDevices { get; protected set; }
    }
}
