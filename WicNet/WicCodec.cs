using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DirectN;

namespace WicNet
{
    public abstract class WicCodec : WicImagingComponent
    {
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
        public string ContainerFormatName => GetFormatName(ContainerFormat);

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

        public override string ToString() => base.ToString() + " " + ContainerFormatName;

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

        public static string GetFormatName(Guid guid) => Extensions.GetGuidName(typeof(WicCodec), guid);

        public static readonly Guid GUID_ContainerFormatAdng = new Guid("f3ff6d0d-38c0-41c4-b1fe-1f3824f17b84");
        public static readonly Guid GUID_ContainerFormatBmp = new Guid("0af1d87e-fcfe-4188-bdeb-a7906471cbe3");
        public static readonly Guid GUID_ContainerFormatDds = new Guid("9967cb95-2e85-4ac8-8ca2-83d7ccd425c9");
        public static readonly Guid GUID_ContainerFormatGif = new Guid("1f8a5601-7d4d-4cbd-9c82-1bc8d4eeb9a5");
        public static readonly Guid GUID_ContainerFormatHeif = new Guid("e1e62521-6787-405b-a339-500715b5763f");
        public static readonly Guid GUID_ContainerFormatIco = new Guid("a3a860c4-338f-4c17-919a-fba4b5628f21");
        public static readonly Guid GUID_ContainerFormatJpeg = new Guid("19e4a5aa-5662-4fc5-a0c0-1758028e1057");
        public static readonly Guid GUID_ContainerFormatPng = new Guid("1b7cfaf4-713f-473c-bbcd-6137425faeaf");
        public static readonly Guid GUID_ContainerFormatRaw = new Guid("fe99ce60-f19c-433c-a3ae-00acefa9ca21");
        public static readonly Guid GUID_ContainerFormatTiff = new Guid("163bcc30-e2e9-4f0b-961d-a3e9fdb788a3");
        public static readonly Guid GUID_ContainerFormatWebp = new Guid("e094b0e2-67f2-45b3-b0ea-115337ca7cf3");
        public static readonly Guid GUID_ContainerFormatWmp = new Guid("57a37caa-367a-4540-916b-f183c5093a4b");

        // manual
        public static readonly Guid GUID_ContainerFormatCur = new Guid("0444f35f-587c-4570-9646-64dcd8f17573");
    }
}