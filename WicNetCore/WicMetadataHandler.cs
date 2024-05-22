namespace WicNet;

public abstract class WicMetadataHandler : WicImagingComponent
{
    protected WicMetadataHandler(IComObject<IWICMetadataHandlerInfo> comObject)
        : base(comObject)
    {
        comObject.Object.GetMetadataFormat(out Guid guid);
        Guid = guid;

        DeviceManufacturer = Utilities.Extensions.GetString((s, capacity) =>
        {
            comObject.Object.GetDeviceManufacturer(capacity, s, out var size);
            return size;
        });

        DeviceModels = Utilities.Extensions.GetString((s, capacity) =>
        {
            comObject.Object.GetDeviceModels(capacity, s, out var size);
            return size;
        });

        unsafe
        {
            comObject.Object.GetContainerFormats(0, null!, out var count);
            if (count > 0)
            {
                var guids = new Guid[count];
                if (comObject.Object.GetContainerFormats(count, guids, out _) == 0)
                {
                    ContainerFormats = guids;
                }
            }
        }

        ContainerFormats ??= [];

        comObject.Object.DoesRequireFullStream(out var b);
        RequiresFullStream = b;

        comObject.Object.DoesSupportPadding(out b);
        SupportsPadding = b;

        comObject.Object.DoesRequireFixedSize(out b);
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
