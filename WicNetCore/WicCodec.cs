using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DirectN;
using DirectNAot.Extensions.Com;
using DirectNAot.Extensions.Utilities;
using WicNet.Utilities;

namespace WicNet;

public abstract class WicCodec : WicImagingComponent
{
    private readonly Lazy<IReadOnlyList<WicPixelFormat>> _pixelFormatsList;

    protected WicCodec(object comObject)
        : base(comObject)
    {
        using var info = new ComObjectWrapper<IWICBitmapCodecInfo>(comObject).ComObject;
        _pixelFormatsList = new Lazy<IReadOnlyList<WicPixelFormat>>(GetPixelFormatsList, true);
        info.Object.GetContainerFormat(out Guid guid);
        ContainerFormat = guid;

        FileExtensions = Utilities.Extensions.GetString((s, capacity) =>
        {
            info.Object.GetFileExtensions(capacity, ref s, out var size);
            return size;
        });

        ColorManagementVersion = Utilities.Extensions.GetString((s, capacity) =>
        {
            info.Object.GetColorManagementVersion(capacity, ref s, out var size);
            return size;
        });

        DeviceManufacturer = Utilities.Extensions.GetString((s, capacity) =>
        {
            info.Object.GetDeviceManufacturer(capacity, ref s, out var size);
            return size;
        });

        DeviceModels = Utilities.Extensions.GetString((s, capacity) =>
        {
            info.Object.GetDeviceModels(capacity, ref s, out var size);
            return size;
        });

        MimeTypes = Utilities.Extensions.GetString((s, capacity) =>
        {
            info.Object.GetMimeTypes(capacity, ref s, out var size);
            return size;
        });

        info.Object.DoesSupportAnimation(out bool b);
        SupportsAnimation = b;

        info.Object.DoesSupportChromakey(out b);
        SupportsChromakey = b;

        info.Object.DoesSupportLossless(out b);
        SupportsLossless = b;

        info.Object.DoesSupportMultiframe(out b);
        SupportsMultiframe = b;

        FileExtensionsList = [.. (FileExtensions?.SplitToList(',').Select(s => s.ToLowerInvariant()).OrderBy(s => s).ToList() ?? [])];
        MimeTypesList = [.. (MimeTypes?.SplitToList(',').OrderBy(s => s).ToList() ?? [])];

        info.Object.GetPixelFormats(0, ref Unsafe.NullRef<Guid[]>(), out var len);
        if (len > 0)
        {
            var pf = new Guid[len];
            info.Object.GetPixelFormats(len, ref pf, out _).ThrowOnError();
            PixelFormats = pf;
        }
        else
        {
            PixelFormats = [];
        }
    }

    public IReadOnlyList<Guid> PixelFormats { get; }
    public Guid ContainerFormat { get; }
    public string? FileExtensions { get; }
    public string? ColorManagementVersion { get; }
    public string? DeviceManufacturer { get; }
    public string? DeviceModels { get; }
    public string? MimeTypes { get; }
    public bool SupportsAnimation { get; }
    public bool SupportsChromakey { get; }
    public bool SupportsLossless { get; }
    public bool SupportsMultiframe { get; }
    public IReadOnlyList<string> FileExtensionsList { get; }
    public IReadOnlyList<string> MimeTypesList { get; }
    public string ContainerFormatName => GetFormatName(ContainerFormat);

    public IReadOnlyList<WicPixelFormat> PixelFormatsList => _pixelFormatsList.Value;

    private IReadOnlyList<WicPixelFormat> GetPixelFormatsList()
    {
        var list = new List<WicPixelFormat>();
        foreach (var pf in PixelFormats)
        {
            var format = WicPixelFormat.FromClsid(pf);
            if (format != null)
            {
                list.Add(format);
            }
        }

        list.Sort();
        return list;
    }

    public override string ToString() => base.ToString() + " " + ContainerFormatName;

    public bool SupportsFileExtension(string ext)
    {
        ArgumentNullException.ThrowIfNull(ext);
        if (!ext.StartsWith('.'))
        {
            ext = Path.GetExtension(ext);
        }

        return FileExtensionsList.Contains(ext, StringComparer.OrdinalIgnoreCase);
    }

    public bool SupportsMimeType(string mimeType)
    {
        ArgumentNullException.ThrowIfNull(mimeType);
        return MimeTypesList.Contains(mimeType, StringComparer.OrdinalIgnoreCase);
    }

    public static T? FromContainerFormatGuid<T>(Guid guid) where T : WicCodec => AllComponents.OfType<T>().FirstOrDefault(c => c.ContainerFormat == guid);

    public static string GetFormatName(Guid guid) => typeof(WicCodec).GetGuidName(guid);

    public static readonly Guid GUID_ContainerFormatAdng = new("f3ff6d0d-38c0-41c4-b1fe-1f3824f17b84");
    public static readonly Guid GUID_ContainerFormatBmp = new("0af1d87e-fcfe-4188-bdeb-a7906471cbe3");
    public static readonly Guid GUID_ContainerFormatDds = new("9967cb95-2e85-4ac8-8ca2-83d7ccd425c9");
    public static readonly Guid GUID_ContainerFormatGif = new("1f8a5601-7d4d-4cbd-9c82-1bc8d4eeb9a5");
    public static readonly Guid GUID_ContainerFormatHeif = new("e1e62521-6787-405b-a339-500715b5763f");
    public static readonly Guid GUID_ContainerFormatIco = new("a3a860c4-338f-4c17-919a-fba4b5628f21");
    public static readonly Guid GUID_ContainerFormatJpeg = new("19e4a5aa-5662-4fc5-a0c0-1758028e1057");
    public static readonly Guid GUID_ContainerFormatPng = new("1b7cfaf4-713f-473c-bbcd-6137425faeaf");
    public static readonly Guid GUID_ContainerFormatRaw = new("fe99ce60-f19c-433c-a3ae-00acefa9ca21");
    public static readonly Guid GUID_ContainerFormatTiff = new("163bcc30-e2e9-4f0b-961d-a3e9fdb788a3");
    public static readonly Guid GUID_ContainerFormatWebp = new("e094b0e2-67f2-45b3-b0ea-115337ca7cf3");
    public static readonly Guid GUID_ContainerFormatWmp = new("57a37caa-367a-4540-916b-f183c5093a4b");

    // manual
    public static readonly Guid GUID_ContainerFormatCur = new("0444f35f-587c-4570-9646-64dcd8f17573");
}