using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using DirectN;
using Windows.Devices.Display.Core;
using Windows.Graphics.DirectX;

namespace WicNetExplorer.Model;

[SupportedOSPlatform("windows10.0.17763")]
public class DisplayDeviceModel2 : DisplayDeviceModel
{
    public DisplayDeviceModel2(DISPLAY_DEVICE device, DisplayPath path)
        : base(device)
    {
        ArgumentNullException.ThrowIfNull(path);
        WireFormat = new WireFormatModel(path.WireFormat);
        SourcePixelFormat = path.SourcePixelFormat;
        IsStereo = path.IsStereo;
        IsInterlaced = path.IsInterlaced == true;
        Rotation = path.Rotation;
        Scaling = path.Scaling;

        var monitor = path.Target.TryGetMonitor();
        if (monitor != null)
        {
            Monitor = new MonitorModel(monitor, base.Monitor);
        }
    }

    public new MonitorModel? Monitor { get; }

    [DisplayName("Wire Format")]
    public WireFormatModel WireFormat { get; }

    [DisplayName("Source Pixel Format")]
    public DirectXPixelFormat SourcePixelFormat { get; }

    [DisplayName("Is Stereo")]
    public bool IsStereo { get; }

    [DisplayName("Is Interlaced")]
    public bool IsInterlaced { get; }

    public DisplayRotation Rotation { get; }
    public DisplayPathScaling Scaling { get; }

    public override string ToString()
    {
        var str = Name;
        if (Monitor != null)
        {
            str += " - " + Monitor;
        }
        return str;
    }
}
