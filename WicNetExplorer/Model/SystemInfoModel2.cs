using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using DirectN;
using Windows.Devices.Display.Core;

namespace WicNetExplorer.Model;

[SupportedOSPlatform("windows10.0.17763")]
public class SystemInfoModel2 : SystemInfoModel
{
    public SystemInfoModel2()
    {
        var list = new List<DisplayDeviceModel>();
        var dic = new Dictionary<string, DisplayPath>();
        using var mgr = DisplayManager.Create(DisplayManagerOptions.None);
        var state = mgr.TryReadCurrentStateForAllTargets().State;
        foreach (var view in state.Views)
        {
            foreach (var path in view.Paths)
            {
                var monitor = path.Target.TryGetMonitor();
                if (monitor != null)
                {
                    var ip = WinRT.CastExtensions.As<IDisplayPathInterop>(path);
                    ip.GetSourceId(out var sourceId);
                    var gdiDeviceName = DisplayConfig.GetGdiDeviceName(monitor.DisplayAdapterId.HighPart, monitor.DisplayAdapterId.LowPart, sourceId, false);
                    if (gdiDeviceName != null)
                    {
                        var dd = DISPLAY_DEVICE.Active.FirstOrDefault(dd => dd.DeviceName == gdiDeviceName);
                        if (dd.DeviceName != null)
                        {
                            dic[gdiDeviceName] = path;
                        }
                    }
                }
            }
        }

        foreach (var dd in DISPLAY_DEVICE.All)
        {
            DisplayDeviceModel ddModel;
            if (dic.TryGetValue(dd.DeviceName, out var path))
            {
                ddModel = new DisplayDeviceModel2(dd, path);
            }
            else
            {
                ddModel = new DisplayDeviceModel(dd);
            }
            list.Add(ddModel);
        }
        AllDisplayDevices = [.. list];
    }
}
