﻿using System.ComponentModel;
using DirectN;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class DisplayDeviceModel
{
    private readonly DISPLAY_DEVICE _device;

    public DisplayDeviceModel(DISPLAY_DEVICE device)
    {
        _device = device;
        Modes = [.. _device.GetModes()];
        StateFlags = _device.StateFlags.GetEnumName(nameof(DISPLAY_DEVICE));
    }

    public string Name => _device.DeviceName;
    public string Id => _device.DeviceID;

    [DisplayName("Registry Key")]
    public string Key => _device.DeviceKey;

    [DisplayName("Adapter Name")]
    public string AdapterName => _device.DeviceString;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public Monitor Monitor => _device.Monitor;

    [DisplayName("State Flags")]
    public string StateFlags { get; }

    [DisplayName("Is Primary")]
    public bool IsPrimary => _device.IsPrimary;

    [DisplayName("Current Mode")]
    public DEVMODE CurrentMode => _device.CurrentSettings;

    [DisplayName("Registry Mode")]
    public DEVMODE RegistryMode => _device.RegistrySettings;

    [TypeConverter(typeof(StringFormatterArrayConverter))]
    [StringFormatter("{Length}")]
    public DEVMODE[] Modes { get; }

    public override string ToString() => $"{Name}";
}
