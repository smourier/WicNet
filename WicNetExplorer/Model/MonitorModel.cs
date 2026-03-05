using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using DirectN;
using DirectN.Extensions.Utilities;
using WicNetExplorer.Utilities;
using Windows.Devices.Display;
using Windows.Foundation;

namespace WicNetExplorer.Model;

[SupportedOSPlatform("windows10.0.17763")]
[TypeConverter(typeof(ExpandableObjectConverter))]
public class MonitorModel
{
    private readonly DisplayMonitor _monitor;
    private readonly Monitor _baseMonitor;

    public MonitorModel(DisplayMonitor monitor, Monitor baseMonitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentNullException.ThrowIfNull(baseMonitor);
        _monitor = monitor;
        _baseMonitor = baseMonitor;
        DeviceId = monitor.DeviceId;

        foreach (var path in DisplayConfig.Query())
        {
            var target = DisplayConfig.GetTargetName(path);
            OutputTechnology = target.outputTechnology.GetEnumName(nameof(DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY));
            if (target.monitorDevicePath.ToString() == monitor.DeviceId)
            {
                var aci = DisplayConfig.GetAdvancedColorInfo(path);
                ColorEncoding = aci.colorEncoding.GetEnumName();
                BitsPerColorChannel = aci.bitsPerColorChannel;
                AdvancedColorSupported = (aci.Anonymous.value & 0x1) == 0x1;
                AdvancedColorEnabled = (aci.Anonymous.value & 0x2) == 0x2;
                WideColorEnforced = (aci.Anonymous.value & 0x4) == 0x4;
                AdvancedColorForceDisabled = (aci.Anonymous.value & 0x8) == 0x8;

                var swl = DisplayConfig.GetSdrWhiteLevel(path);
                SdrWhiteLevel = swl.SDRWhiteLevel;
                break;
            }
        }
    }

    [DisplayName("Device Id")]
    public string? DeviceId { get; }

    [DisplayName("Raw DPI")]
    public D2D_SIZE_F RawDpi => new(_monitor.RawDpiY, _monitor.RawDpiY);

    public string Handle => _baseMonitor.Handle.Value.ToHex();
    public RECT Bounds => _baseMonitor.Bounds;

    [DisplayName("Is Primary")]
    public bool IsPrimary => _baseMonitor.IsPrimary;

    [DisplayName("Working Area")]
    public RECT WorkingArea => _baseMonitor.WorkingArea;

    [DisplayName("Scale Factor")]
    public DEVICE_SCALE_FACTOR ScaleFactor => _baseMonitor.ScaleFactor;

    [DisplayName("Chromacity White Point")]
    public Point WhitePoint => _monitor.WhitePoint;

    [DisplayName("Chromacity Red Primary")]
    public Point RedPrimary => _monitor.RedPrimary;

    [DisplayName("Chromacity Blue Primary")]
    public Point BluePrimary => _monitor.BluePrimary;

    [DisplayName("Chromacity Green Primary")]
    public Point GreenPrimary => _monitor.GreenPrimary;

    [DisplayName("Usage Kind")]
    public DisplayMonitorUsageKind UsageKind => _monitor.UsageKind;

    [DisplayName("SDR White Level")]
    public uint SdrWhiteLevel { get; }

    [DisplayName("Color Encoding")]
    public string? ColorEncoding { get; }

    [DisplayName("Output Technology")]
    public string? OutputTechnology { get; }

    [DisplayName("Bits Per Color Channel")]
    public uint BitsPerColorChannel { get; }

    [DisplayName("Advanced Color Supported")]
    public bool AdvancedColorSupported { get; }

    [DisplayName("Advanced Color Enabled")]
    public bool AdvancedColorEnabled { get; }

    [DisplayName("Wide Color Enforced")]
    public bool WideColorEnforced { get; }

    [DisplayName("Advanced Color Force Disabled")]
    public bool AdvancedColorForceDisabled { get; }

    [DisplayName("Min Luminance In Nits")]
    public float MinLuminanceInNits => _monitor.MinLuminanceInNits;

    [DisplayName("Max Luminance In Nits")]
    public float MaxLuminanceInNits => _monitor.MaxLuminanceInNits;

    [DisplayName("Max Average Full Frame Luminance In Nits")]
    public float MaxAverageFullFrameLuminanceInNits => _monitor.MaxAverageFullFrameLuminanceInNits;

    [DisplayName("Physical Size In Inches")]
    public D2D_SIZE_F? PhysicalSizeInInches
    {
        get
        {
            var size = _monitor.PhysicalSizeInInches;
            if (!size.HasValue)
                return null;

            return new(size.Value.Width, size.Value.Height);
        }
    }

    [DisplayName("Physical Size In Centimeters")]
    public D2D_SIZE_F? PhysicalSizeInCentimeters
    {
        get
        {
            var size = _monitor.PhysicalSizeInInches;
            if (!size.HasValue)
                return null;

            return new(size.Value.Width * 2.54, size.Value.Height * 2.54);
        }
    }

    public override string ToString() => _monitor.DisplayName;
}
