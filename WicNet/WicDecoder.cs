using System;
using System.IO;
using System.Linq;

namespace WicNet
{
    public sealed class WicDecoder : WicCodec
    {
        public static WicDecoder Png { get; private set; }
        public static WicDecoder Jpeg { get; private set; }
        public static WicDecoder Bmp { get; private set; }
        public static WicDecoder Ico { get; private set; }
        public static WicDecoder Tiff { get; private set; }
        public static WicDecoder Gif { get; private set; }
        public static WicDecoder WMPhoto { get; private set; }
        public static WicDecoder Dds { get; private set; }
        public static WicDecoder Raw { get; private set; }

        internal static void CreateBuiltIn()
        {
            Png = FromContainerFormatGuid(ContainerFormatPng);
            Jpeg = FromContainerFormatGuid(ContainerFormatJpeg);
            Bmp = FromContainerFormatGuid(ContainerFormatBmp);
            Ico = FromContainerFormatGuid(ContainerFormatIco);
            Tiff = FromContainerFormatGuid(ContainerFormatTiff);
            Gif = FromContainerFormatGuid(ContainerFormatGif);
            WMPhoto = FromContainerFormatGuid(ContainerFormatWMPhoto);
            Dds = FromContainerFormatGuid(ContainerFormatDds);
            Raw = FromContainerFormatGuid(ContainerFormatRaw);
        }

        public WicDecoder(object comObject)
            : base(comObject)
        {
        }

        // they are supposed to have the same container format
        public static WicDecoder FromEncoder(WicEncoder encoder) => encoder != null ? FromContainerFormatGuid(encoder.ContainerFormat) : null;

        public static WicDecoder FromFileExtension(string ext)
        {
            if (ext == null)
                return null;

            if (!ext.StartsWith(".", StringComparison.Ordinal))
            {
                ext = Path.GetExtension(ext);
            }

            return AllComponents.OfType<WicDecoder>().FirstOrDefault(e => e.SupportsFileExtension(ext));
        }

        public static WicDecoder FromMimeType(string mimeType)
        {
            if (mimeType == null)
                return null;

            return AllComponents.OfType<WicDecoder>().FirstOrDefault(c => c.SupportsMimeType(mimeType));
        }

        public static WicDecoder FromName(string name)
        {
            if (name == null)
                return null;

            var item = AllComponents.OfType<WicDecoder>().FirstOrDefault(c => c.FriendlyName.EqualsIgnoreCase(name));
            if (item != null)
                return item;

            return AllComponents.OfType<WicDecoder>().FirstOrDefault(f => f.FriendlyName.ToLowerInvariant().Replace("decoder", "").Trim().EqualsIgnoreCase(name));
        }

        public static WicDecoder FromContainerFormatGuid(Guid guid) => FromContainerFormatGuid<WicDecoder>(guid);
    }
}
