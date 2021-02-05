using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WicNet.Interop;

namespace WicNet
{
    public abstract class WicCodec : WicImagingComponent
    {
        internal static readonly Guid ContainerFormatPng = new Guid(0x1b7cfaf4, 0x713f, 0x473c, 0xbb, 0xcd, 0x61, 0x37, 0x42, 0x5f, 0xae, 0xaf);
        internal static readonly Guid ContainerFormatJpeg = new Guid(0x19e4a5aa, 0x5662, 0x4fc5, 0xa0, 0xc0, 0x17, 0x58, 0x02, 0x8e, 0x10, 0x57);
        internal static readonly Guid ContainerFormatBmp = new Guid(0x0af1d87e, 0xfcfe, 0x4188, 0xbd, 0xeb, 0xa7, 0x90, 0x64, 0x71, 0xcb, 0xe3);
        internal static readonly Guid ContainerFormatIco = new Guid(0xa3a860c4, 0x338f, 0x4c17, 0x91, 0x9a, 0xfb, 0xa4, 0xb5, 0x62, 0x8f, 0x21);
        internal static readonly Guid ContainerFormatTiff = new Guid(0x163bcc30, 0xe2e9, 0x4f0b, 0x96, 0x1d, 0xa3, 0xe9, 0xfd, 0xb7, 0x88, 0xa3);
        internal static readonly Guid ContainerFormatGif = new Guid(0x1f8a5601, 0x7d4d, 0x4cbd, 0x9c, 0x82, 0x1b, 0xc8, 0xd4, 0xee, 0xb9, 0xa5);
        internal static readonly Guid ContainerFormatWMPhoto = new Guid(0x57a37caa, 0x367a, 0x4540, 0x91, 0x6b, 0xf1, 0x83, 0xc5, 0x09, 0x3a, 0x4b);
        internal static readonly Guid ContainerFormatDds = new Guid(0x9967cb95, 0x2e85, 0x4ac8, 0x8c, 0xa2, 0x83, 0xd7, 0xcc, 0xd4, 0x25, 0xc9);
        internal static readonly Guid ContainerFormatRaw = new Guid(0xc1fc85cb, 0xd64f, 0x478b, 0xa4, 0xec, 0x69, 0xad, 0xc9, 0xee, 0x13, 0x92);

        private readonly Lazy<IReadOnlyList<WicPixelFormat>> _pixelFormatsList;

        protected WicCodec(object comObject)
            : base(comObject)
        {
            var info = new ComObjectWrapper<IWICBitmapCodecInfo>(comObject).ComObject;

            _pixelFormatsList = new Lazy<IReadOnlyList<WicPixelFormat>>(GetPixelFormatsList, true);
            info.Object.GetContainerFormat(out Guid guid);
            ContainerFormat = guid;

            info.Object.GetFileExtensions(0, null, out var len);
            if (len >= 0)
            {
                var sb = new StringBuilder(len);
                info.Object.GetFileExtensions(len + 1, sb, out _);
                FileExtensions = sb.ToString();
            }

            info.Object.GetColorManagementVersion(0, null, out len);
            if (len >= 0)
            {
                var sb = new StringBuilder(len);
                info.Object.GetColorManagementVersion(len + 1, sb, out _);
                ColorManagementVersion = sb.ToString();
            }

            info.Object.GetDeviceManufacturer(0, null, out len);
            if (len >= 0)
            {
                var sb = new StringBuilder(len);
                info.Object.GetDeviceManufacturer(len + 1, sb, out _);
                DeviceManufacturer = sb.ToString();
            }

            info.Object.GetDeviceModels(0, null, out len);
            if (len >= 0)
            {
                var sb = new StringBuilder(len);
                info.Object.GetDeviceModels(len + 1, sb, out _);
                DeviceModels = sb.ToString();
            }

            info.Object.GetMimeTypes(0, null, out len);
            if (len >= 0)
            {
                var sb = new StringBuilder(len);
                info.Object.GetMimeTypes(len + 1, sb, out _);
                MimeTypes = sb.ToString();
            }

            info.Object.DoesSupportAnimation(out bool b);
            SupportsAnimation = b;

            info.Object.DoesSupportChromakey(out b);
            SupportsChromakey = b;

            info.Object.DoesSupportLossless(out b);
            SupportsLossless = b;

            info.Object.DoesSupportMultiframe(out b);
            SupportsMultiframe = b;

            FileExtensionsList = FileExtensions.SplitToList(',').Select(s => s.ToLowerInvariant()).OrderBy(s => s).ToList().AsReadOnly();
            MimeTypesList = MimeTypes.SplitToList(',').OrderBy(s => s).ToList().AsReadOnly();

            info.Object.GetPixelFormats(0, null, out len);
            if (len > 0)
            {
                var pf = new Guid[len];
                info.Object.GetPixelFormats(len, pf, out _).ThrowOnError();
                PixelFormats = pf;
            }
            else
            {
                PixelFormats = Array.Empty<Guid>();
            }
        }

        public IReadOnlyList<Guid> PixelFormats { get; }
        public Guid ContainerFormat { get; }
        public string FileExtensions { get; }
        public string ColorManagementVersion { get; }
        public string DeviceManufacturer { get; }
        public string DeviceModels { get; }
        public string MimeTypes { get; }
        public bool SupportsAnimation { get; }
        public bool SupportsChromakey { get; }
        public bool SupportsLossless { get; }
        public bool SupportsMultiframe { get; }
        public IReadOnlyList<string> FileExtensionsList { get; }
        public IReadOnlyList<string> MimeTypesList { get; }

        public IReadOnlyList<WicPixelFormat> PixelFormatsList => _pixelFormatsList.Value;

        private IReadOnlyList<WicPixelFormat> GetPixelFormatsList()
        {
            var list = new List<WicPixelFormat>();
            foreach (Guid pf in PixelFormats)
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

        public bool SupportsFileExtension(string ext)
        {
            if (ext == null)
                throw new ArgumentNullException(nameof(ext));

            if (!ext.StartsWith(".", StringComparison.Ordinal))
            {
                ext = Path.GetExtension(ext);
            }

            return FileExtensionsList.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        public bool SupportsMimeType(string mimeType)
        {
            if (mimeType == null)
                throw new ArgumentNullException(nameof(mimeType));

            return MimeTypesList.Contains(mimeType, StringComparer.OrdinalIgnoreCase);
        }

        public static T FromContainerFormatGuid<T>(Guid guid) where T : WicCodec => AllComponents.OfType<T>().FirstOrDefault(c => c.ContainerFormat == guid);
    }
}