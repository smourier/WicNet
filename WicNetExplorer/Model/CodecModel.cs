using System;
using System.ComponentModel;
using System.Linq;
using WicNet;

namespace WicNetExplorer.Model;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class CodecModel : ImagingComponentModel
{
    private readonly WicCodec _codec;

    public CodecModel(WicCodec codec)
        : base(codec)
    {
        ArgumentNullException.ThrowIfNull(codec);
        _codec = codec;
        PixelFormats = [.. _codec.PixelFormatsList.Select(f => new PixelFormatModel(f))];
        FileExtensions = [.. _codec.FileExtensionsList];
        MimeTypes = [.. _codec.MimeTypesList];
    }

    [DisplayName("Container Format")]
    public Guid ContainerFormat => _codec.ContainerFormat;

    [DisplayName("Color Management Version")]
    public string ColorManagementVersion => _codec.ColorManagementVersion;

    [DisplayName("Device Manufacturer")]
    public string DeviceManufacturer => _codec.DeviceManufacturer;

    [DisplayName("Device Models")]
    public string DeviceModels => _codec.DeviceModels;

    [DisplayName("Supports Animation")]
    public bool SupportsAnimation => _codec.SupportsAnimation;

    [DisplayName("Supports Chromakey")]
    public bool SupportsChromakey => _codec.SupportsChromakey;

    [DisplayName("Supports Lossless")]
    public bool SupportsLossless => _codec.SupportsLossless;

    [DisplayName("Supports Multiframe")]
    public bool SupportsMultiframe => _codec.SupportsMultiframe;

    [DisplayName("Container Format Name")]
    public string ContainerFormatName => _codec.ContainerFormatName;

    [DisplayName("File Exensions")]
    public string[] FileExtensions { get; }

    [DisplayName("Mime Types")]
    public string[] MimeTypes { get; }

    [DisplayName("Pixel Formats")]
    public PixelFormatModel[] PixelFormats { get; }
}
