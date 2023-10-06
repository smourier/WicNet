using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DirectN;

namespace WicNet
{
    public sealed class WicColorContext : IDisposable
    {
        public static WicColorContext Standard { get; } = new WicColorContext(StandardColorSpaceProfile); // sRGB

        private readonly IComObject<IWICColorContext> _comObject;
        private readonly Lazy<ColorProfile> _profile;
        private readonly Lazy<byte[]> _profileBytes;

        public WicColorContext(IWICColorContext palette)
            : this((object)palette)
        {
        }

        public WicColorContext(IComObject<IWICColorContext> palette)
            : this((object)palette)
        {
        }

        public WicColorContext(uint colorSpace)
            : this((object)colorSpace)
        {
        }

        public WicColorContext(string fileName)
            : this((object)fileName)
        {
        }

        public WicColorContext(byte[] bytes)
            : this((object)bytes)
        {
        }

        public WicColorContext()
            : this((object)null)
        {
        }

        public WicColorContext(object source)
        {
            _profileBytes = new Lazy<byte[]>(GetProfileBytes);
            if (source == null)
            {
                _comObject = WICImagingFactory.CreateColorContext();
            }
            else if (source is IWICColorContext p)
            {
                _comObject = new ComObject<IWICColorContext>(p);
            }
            else if (source is uint colorSpace)
            {
                _comObject = WICImagingFactory.CreateColorContext();
                _comObject.Object.InitializeFromExifColorSpace(colorSpace);
            }
            else if (source is string fileName)
            {
                _comObject = WICImagingFactory.CreateColorContext();
                _comObject.Object.InitializeFromFilename(fileName);
            }
            else if (source is byte[] memory)
            {
                _comObject = WICImagingFactory.CreateColorContext();
                _comObject.Object.InitializeFromMemory(memory, memory.Length);
            }
            else
            {
                _comObject = source as IComObject<IWICColorContext>;
                if (_comObject == null)
                    throw new ArgumentException("Source must be an " + nameof(IWICColorContext) + ".", nameof(source));
            }
            _profile = new Lazy<ColorProfile>(() =>
            {
                var bytes = ProfileBytes;
                if (bytes == null || bytes.Length == 0)
                    return null;

                return ColorProfile.FromMemory(ProfileBytes);
            }, true);
        }

        public IComObject<IWICColorContext> ComObject => _comObject;

        public ColorProfile Profile => _profile.Value;

        // 1 => A sRGB color space
        // 2 => An Adobe RGB color space
        // 0xFFFF => Uncalibrated cf https://home.jeita.or.jp/tsc/std-pdf/CP3451C.pdf "Tags Relating to ColorSpace"
        public uint? ExifColorSpace
        {
            get
            {
                if (Type != WICColorContextType.WICColorContextExifColorSpace)
                    return null;

                _comObject.Object.GetExifColorSpace(out var value).ThrowOnError();
                return value;
            }
        }

        public string ExifColorSpaceName
        {
            get
            {
                var exif = ExifColorSpace;
                if (!exif.HasValue)
                    return "Unspecified";

                switch (exif.Value)
                {
                    case 1:
                        return "sRGB";

                    case 2:
                        return "Adobe RGB";

                    case 0xFFFF:
                        return "Uncalibrated";

                    default:
                        return "Unknown";
                }
            }
        }

        public WICColorContextType Type
        {
            get
            {
                _comObject.Object.GetType(out var value).ThrowOnError();
                return value;
            }
        }

        public byte[] ProfileBytes => _profileBytes.Value;
        private byte[] GetProfileBytes()
        {
            var hr = _comObject.Object.GetProfileBytes(0, null, out var count);
            if (hr.IsError)
                return null;

            var bytes = new byte[count];
            _comObject.Object.GetProfileBytes((int)count, bytes, out _).ThrowOnError();
            return bytes;
        }

        public override string ToString() => Profile?.Description ?? ExifColorSpaceName;
        public void Dispose() => _comObject.SafeDispose();

        public static string StandardColorSpaceProfile
        {
            get
            {
                const int sRGB = 0x73524742; //  'sRGB'
                GetStandardColorSpaceProfile(IntPtr.Zero, sRGB, null, out var size);
                var sb = new StringBuilder(size);
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                GetStandardColorSpaceProfile(IntPtr.Zero, sRGB, sb, out size);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                var s = sb.ToString();
                if (string.IsNullOrWhiteSpace(s))
                    return string.Empty;

                if (Path.IsPathRooted(s))
                    return s;

                var dir = ColorDirectory;
                if (string.IsNullOrEmpty(dir))
                    return string.Empty;

                return Path.Combine(dir, s);
            }
        }

        public static string ColorDirectory
        {
            get
            {
                GetColorDirectory(IntPtr.Zero, null, out var size);
                var sb = new StringBuilder(size);
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                GetColorDirectory(IntPtr.Zero, sb, out size);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                return sb.ToString();
            }
        }

        [DllImport("mscms", CharSet = CharSet.Unicode)]
        private static extern HRESULT GetColorDirectory(IntPtr pMachineName, StringBuilder pBuffer, out int pdwSize);

        [DllImport("mscms", CharSet = CharSet.Unicode)]
        private static extern HRESULT GetStandardColorSpaceProfile(IntPtr pMachineName, uint dwProfileID, StringBuilder pProfileName, out int pdwSize);
    }
}
