using System;
using System.Collections.Generic;
using System.Linq;
using DirectN;
using WicNet.Utilities;

namespace WicNet
{
    public abstract class WicMetadataHandler : WicImagingComponent
    {
        protected WicMetadataHandler(object comObject)
            : base(comObject)
        {
            using (var info = new ComObjectWrapper<IWICMetadataHandlerInfo>(comObject))
            {
                info.Object.GetMetadataFormat(out Guid guid);
                Guid = guid;

                DeviceManufacturer = Utilities.Extensions.GetString((s, capacity) =>
                {
                    info.Object.GetDeviceManufacturer(capacity, s, out var size);
                    return size;
                });

                DeviceModels = Utilities.Extensions.GetString((s, capacity) =>
                {
                    info.Object.GetDeviceModels(capacity, s, out var size);
                    return size;
                });

                info.Object.GetContainerFormats(0, null, out var count);
                if (count > 0)
                {
                    var guids = new Guid[count];
                    if (info.Object.GetContainerFormats((int)count, guids, out _) == 0)
                    {
                        ContainerFormats = guids;
                    }
                }

                ContainerFormats = ContainerFormats ?? Array.Empty<Guid>();

                info.Object.DoesRequireFullStream(out var b);
                RequiresFullStream = b;

                info.Object.DoesSupportPadding(out b);
                SupportsPadding = b;

                info.Object.DoesRequireFixedSize(out b);
                RequiresFixedSize = b;
            }
        }

        public Guid Guid { get; }
        public IReadOnlyList<Guid> ContainerFormats { get; }
        public string DeviceManufacturer { get; }
        public string DeviceModels { get; }
        public bool RequiresFullStream { get; }
        public bool SupportsPadding { get; }
        public bool RequiresFixedSize { get; }

        public static T FromFormatGuid<T>(Guid guid) where T : WicMetadataHandler => AllComponents.OfType<T>().FirstOrDefault(c => c.Guid == guid);
        public static T FromFriendlyName<T>(string friendlyName) where T : WicMetadataHandler => AllComponents.OfType<T>().FirstOrDefault(c => c.FriendlyName.EqualsIgnoreCase(friendlyName));

        public static string FriendlyNameFromGuid(Guid guid)
        {
            var handler = FromFormatGuid<WicMetadataHandler>(guid);
            return handler != null ? handler.FriendlyName : guid.ToString();
        }
    }
}
