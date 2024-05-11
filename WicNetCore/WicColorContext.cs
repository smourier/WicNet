using System;
using DirectN;
using DirectNAot.Extensions.Com;
using DirectNAot.Extensions.Utilities;

namespace WicNet;

public sealed class WicColorContext : IDisposable
{
    private readonly IComObject<IWICColorContext> _comObject;
    private readonly Lazy<ColorProfile?> _profile;
    private readonly Lazy<byte[]?> _profileBytes;

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
        : this((object?)null)
    {
    }

    public WicColorContext(object? source)
    {
        _profileBytes = new Lazy<byte[]?>(GetProfileBytes);
        if (source == null)
        {
            _comObject = WicImagingFactory.CreateColorContext();
        }
        else if (source is IWICColorContext p)
        {
            _comObject = new ComObject<IWICColorContext>(p);
        }
        else if (source is uint colorSpace)
        {
            _comObject = WicImagingFactory.CreateColorContext();
            _comObject.Object.InitializeFromExifColorSpace(colorSpace);
        }
        else if (source is string fileName)
        {
            _comObject = WicImagingFactory.CreateColorContext();
            using var fn = new Pwstr(fileName);
            _comObject.Object.InitializeFromFilename(fn);
        }
        else if (source is byte[] memory)
        {
            _comObject = WicImagingFactory.CreateColorContext();
            unsafe
            {
                fixed (byte* ptr = memory)
                {
                    _comObject.Object.InitializeFromMemory((nint)ptr, (uint)memory.Length);
                }
            }
        }
        else
        {
            _comObject = source as IComObject<IWICColorContext>;
            if (_comObject == null)
                throw new ArgumentException("Source must be an " + nameof(IWICColorContext) + ".", nameof(source));
        }
        _profile = new Lazy<ColorProfile?>(() =>
        {
            var bytes = ProfileBytes;
            if (bytes == null || bytes.Length == 0)
                return null;

            return ColorProfile.FromMemory(ProfileBytes);
        }, true);
    }

    public IComObject<IWICColorContext> ComObject => _comObject;

    public ColorProfile? Profile => _profile.Value;

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

            return exif.Value switch
            {
                1 => "sRGB",
                2 => "Adobe RGB",
                0xFFFF => "Uncalibrated",
                _ => "Unknown",
            };
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

    public byte[]? ProfileBytes => _profileBytes.Value;
    private byte[]? GetProfileBytes()
    {
        var hr = _comObject.Object.GetProfileBytes(0, 0, out var count);
        if (hr.IsError)
            return null;

        var bytes = new byte[count];
        unsafe
        {
            fixed (byte* ptr = bytes)
            {
                _comObject.Object.GetProfileBytes(count, (nint)ptr, out _).ThrowOnError();
                return bytes;
            }
        }
    }

    public override string ToString() => Profile?.Description ?? ExifColorSpaceName;
    public void Dispose() => _comObject.SafeDispose();

    public static WicColorContext Standard { get; } = new WicColorContext(StandardColorSpaceProfile); // sRGB
    public static string StandardColorSpaceProfile => ColorProfile.GetStandardColorSpaceProfile();
    public static string? ColorDirectory => ColorProfile.GetColorDirectoryPath();
}
