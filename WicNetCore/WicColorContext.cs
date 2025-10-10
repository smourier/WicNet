namespace WicNet;

public sealed class WicColorContext : InterlockedComObject<IWICColorContext>
{
    private readonly Lazy<ColorProfile?> _profile;
    private readonly Lazy<byte[]?> _profileBytes;

    public WicColorContext(IComObject<IWICColorContext> context)
        : base(context)
    {
        _profileBytes = new Lazy<byte[]?>(GetProfileBytes);
        _profile = new Lazy<ColorProfile?>(() =>
        {
            var bytes = ProfileBytes;
            if (bytes == null || bytes.Length == 0)
                return null;

            return ColorProfile.FromMemory(bytes);
        }, true);
    }

    public WicColorContext(uint colorSpace)
        : this(From(colorSpace))
    {
    }

    public WicColorContext(string fileName)
        : this(From(fileName))
    {
    }

    public WicColorContext(byte[] bytes)
        : this(From(bytes))
    {
    }

    public WicColorContext()
        : this(WicImagingFactory.CreateColorContext())
    {
    }

    private static IComObject<IWICColorContext> From(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        var comObject = WicImagingFactory.CreateColorContext();
        using var fn = new Pwstr(fileName);
        comObject.Object.InitializeFromFilename(fn);
        return comObject;
    }

    private static IComObject<IWICColorContext> From(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        var comObject = WicImagingFactory.CreateColorContext();
        comObject.Object.InitializeFromMemory(bytes.AsPointer(), bytes.Length());
        return comObject;
    }

    private static IComObject<IWICColorContext> From(uint colorSpace)
    {
        var comObject = WicImagingFactory.CreateColorContext();
        comObject.Object.InitializeFromExifColorSpace(colorSpace);
        return comObject;
    }

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

            NativeObject.GetExifColorSpace(out var value).ThrowOnError();
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
            NativeObject.GetType(out var value).ThrowOnError();
            return value;
        }
    }

    public byte[]? ProfileBytes => _profileBytes.Value;
    private byte[]? GetProfileBytes()
    {
        var hr = NativeObject.GetProfileBytes(0, 0, out var count);
        if (hr.IsError)
            return null;

        var bytes = new byte[count];
        NativeObject.GetProfileBytes(count, bytes.AsPointer(), out _).ThrowOnError();
        return bytes;
    }

    public override string ToString() => Profile?.Description ?? ExifColorSpaceName;

    public static WicColorContext Standard { get; } = new WicColorContext(StandardColorSpaceProfile); // sRGB
    public static string StandardColorSpaceProfile => ColorProfile.GetStandardColorSpaceProfile();
    public static string? ColorDirectory => ColorProfile.GetColorDirectoryPath();
}
