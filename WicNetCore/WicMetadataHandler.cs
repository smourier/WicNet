using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DirectN;
using DirectNAot.Extensions.Com;
using DirectNAot.Extensions.Utilities;

namespace WicNet;

public abstract class WicMetadataHandler : WicImagingComponent
{
    protected WicMetadataHandler(object comObject)
        : base(comObject)
    {
        using var info = new ComObjectWrapper<IWICMetadataHandlerInfo>(comObject);
        info.Object.GetMetadataFormat(out Guid guid);
        Guid = guid;

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

        unsafe
        {
            var g = Unsafe.AsRef<Guid[]>(null);
            info.Object.GetContainerFormats(0, ref g, out var count);
            if (count > 0)
            {
                var guids = new Guid[count];
                if (info.Object.GetContainerFormats(count, ref guids, out _) == 0)
                {
                    ContainerFormats = guids;
                }
            }
        }

        ContainerFormats ??= [];

        info.Object.DoesRequireFullStream(out var b);
        RequiresFullStream = b;

        info.Object.DoesSupportPadding(out b);
        SupportsPadding = b;

        info.Object.DoesRequireFixedSize(out b);
        RequiresFixedSize = b;
    }

    public Guid Guid { get; }
    public IReadOnlyList<Guid> ContainerFormats { get; }
    public string? DeviceManufacturer { get; }
    public string? DeviceModels { get; }
    public bool RequiresFullStream { get; }
    public bool SupportsPadding { get; }
    public bool RequiresFixedSize { get; }

    public static T? FromFormatGuid<T>(Guid guid) where T : WicMetadataHandler => AllComponents.OfType<T>().FirstOrDefault(c => c.Guid == guid);
    public static T? FromFriendlyName<T>(string friendlyName) where T : WicMetadataHandler => AllComponents.OfType<T>().FirstOrDefault(c => c.FriendlyName.EqualsIgnoreCase(friendlyName));

    public static string? FriendlyNameFromGuid(Guid guid)
    {
        var handler = FromFormatGuid<WicMetadataHandler>(guid);
        return handler != null ? handler.FriendlyName : guid.ToString();
    }
}
