using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DirectN;
using WicNet;
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

            var colorProfiles = new List<ColorProfileModel>();
            foreach (var file in Directory.GetFiles(WicColorContext.ColorDirectory))
            {
                var name = Path.GetFileName(file);
                var cc = new WicColorContext(file);
                try
                {
                    var profile = new ColorProfileModel(cc.Profile);
                    profile.FilePath = file;
                    colorProfiles.Add(profile);
                }
                catch
                {
                    // do nothing
                }
            }

            ColorProfiles = colorProfiles.ToArray();
        }

        [DisplayName("Windows Version")]
        public string WindowsVersion => Environment.OSVersion.VersionString;

        [DisplayName("Kernel Version")]
        public string KernelVersion => WindowsUtilities.KernelVersion.ToString();

        [DisplayName("Display Devices")]
        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public DisplayDeviceModel[] DisplayDevices { get; protected set; }

        [DisplayName("Color Profiles")]
        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public ColorProfileModel[] ColorProfiles { get; protected set; }
    }
}
