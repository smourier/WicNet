using System;
using System.IO;
using System.Linq;

namespace WicNet
{
    public sealed class WicEncoder : WicCodec
    {
        public static WicEncoder Png { get; private set; }
        public static WicEncoder Jpeg { get; private set; }
        public static WicEncoder Bmp { get; private set; }
        public static WicEncoder Tiff { get; private set; }
        public static WicEncoder Gif { get; private set; }
        public static WicEncoder WMPhoto { get; private set; }
        public static WicEncoder Dds { get; private set; }

        internal static void CreateBuiltIn()
        {
            Png = FromContainerFormatGuid(ContainerFormatPng);
            Jpeg = FromContainerFormatGuid(ContainerFormatJpeg);
            Bmp = FromContainerFormatGuid(ContainerFormatBmp);
            Tiff = FromContainerFormatGuid(ContainerFormatTiff);
            Gif = FromContainerFormatGuid(ContainerFormatGif);
            WMPhoto = FromContainerFormatGuid(ContainerFormatWMPhoto);
            Dds = FromContainerFormatGuid(ContainerFormatDds);
        }

        public WicEncoder(object comObject)
            : base(comObject)
        {
        }

        // they are supposed to have the same container format
        public static WicEncoder FromDecoder(WicDecoder decoder) => decoder != null ? FromContainerFormatGuid(decoder.ContainerFormat) : null;

        public static WicEncoder FromFileExtension(string ext)
        {
            if (ext == null)
                return null;

            if (!ext.StartsWith(".", StringComparison.Ordinal))
            {
                ext = Path.GetExtension(ext);
            }

            return AllComponents.OfType<WicEncoder>().FirstOrDefault(e => e.SupportsFileExtension(ext));
        }

        public static WicEncoder FromMimeType(string mimeType)
        {
            if (mimeType == null)
                return null;

            return AllComponents.OfType<WicEncoder>().FirstOrDefault(c => c.SupportsMimeType(mimeType));
        }

        public static WicEncoder FromName(string name)
        {
            if (name == null)
                return null;

            var item = AllComponents.OfType<WicEncoder>().FirstOrDefault(c => c.FriendlyName.EqualsIgnoreCase(name));
            if (item != null)
                return item;

            return AllComponents.OfType<WicEncoder>().FirstOrDefault(f => f.FriendlyName.ToLowerInvariant().Replace("encoder", "").Trim().EqualsIgnoreCase(name));
        }

        public static WicEncoder FromContainerFormatGuid(Guid guid) => FromContainerFormatGuid<WicEncoder>(guid);
    }
}
