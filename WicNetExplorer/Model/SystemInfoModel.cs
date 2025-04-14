using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DirectN;
using WicNet;
using WicNet.Utilities;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class SystemInfoModel
{
    public SystemInfoModel()
    {
        AllDisplayDevices = [.. DISPLAY_DEVICE.Active.Select(d => new DisplayDeviceModel(d))];

        var colorProfiles = new List<ColorProfileModel>();
        foreach (var file in Directory.GetFiles(WicColorContext.ColorDirectory))
        {
            var name = Path.GetFileName(file);
            var cc = new WicColorContext(file);
            try
            {
                var profile = new ColorProfileModel(cc.Profile) { FilePath = file };
                colorProfiles.Add(profile);
            }
            catch
            {
                // do nothing
            }
        }

        ColorProfiles = [.. colorProfiles];
    }

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1822 // Mark members as static
    [DisplayName("Windows Version")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
    public string WindowsVersion => Environment.OSVersion.VersionString;

    [DisplayName("Kernel Version")]
    public string KernelVersion => WindowsUtilities.KernelVersion.ToString();
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning restore CA1822 // Mark members as static

    [DisplayName("All Display Devices")]
    [TypeConverter(typeof(StringFormatterArrayConverter))]
    [StringFormatter("{Length}")]
    public DisplayDeviceModel[] AllDisplayDevices { get; protected set; }

    [DisplayName("Color Profiles")]
    [TypeConverter(typeof(StringFormatterArrayConverter))]
    [StringFormatter("{Length}")]
    public ColorProfileModel[] ColorProfiles { get; protected set; }
}
