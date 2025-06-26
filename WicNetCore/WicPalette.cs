namespace WicNet;

public sealed class WicPalette : InterlockedComObject<IWICPalette>
{
    private readonly Lazy<IReadOnlyList<WicColor>> _colors;

    public WicPalette(IComObject<IWICPalette> palette)
        : base(palette) => _colors = new Lazy<IReadOnlyList<WicColor>>(GetColors, true);

    public WicPalette(WICBitmapPaletteType type, bool addTransparentColor = false)
        : this(From(type, addTransparentColor))
    {
    }

    public WicPalette(WicBitmapSource bitmap, int count, bool addTransparentColor = false)
        : this(From(bitmap, count, addTransparentColor))
    {
    }

    public WicPalette(IEnumerable<WicColor> colors)
        : this(From(colors))
    {
    }

    public WicPalette(WicPalette palette)
        : this(From(palette))
    {
    }

    public WicPalette()
        : this(WicImagingFactory.CreatePalette())
    {
    }

    private static IComObject<IWICPalette> From(WICBitmapPaletteType type, bool addTransparentColor = false)
    {
        var comObject = WicImagingFactory.CreatePalette();
        comObject.Object.InitializePredefined(type, addTransparentColor);
        return comObject;
    }

    private static IComObject<IWICPalette> From(WicBitmapSource bitmap, int count, bool addTransparentColor = false)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        var comObject = WicImagingFactory.CreatePalette();
        comObject.Object.InitializeFromBitmap(bitmap.ComObject.Object, (uint)count, addTransparentColor);
        return comObject;
    }

    private static IComObject<IWICPalette> From(IEnumerable<WicColor> colors)
    {
        ArgumentNullException.ThrowIfNull(colors);
        var cols = colors.Select(c => (uint)c.ToArgb()).ToArray();
        var comObject = WicImagingFactory.CreatePalette();
        comObject.Object.InitializeCustom(cols, (uint)cols.Length);
        return comObject;
    }

    private static IComObject<IWICPalette> From(WicPalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        var comObject = WicImagingFactory.CreatePalette();
        comObject.Object.InitializeFromPalette(palette.ComObject.Object);
        return comObject;
    }

    public bool HasAlpha
    {
        get
        {
            NativeObject.HasAlpha(out var value).ThrowOnError();
            return value;
        }
    }

    public bool IsBlackWhite
    {
        get
        {
            NativeObject.IsBlackWhite(out var value).ThrowOnError();
            return value;
        }
    }

    public bool IsGrayscale
    {
        get
        {
            NativeObject.IsGrayscale(out var value).ThrowOnError();
            return value;
        }
    }

    public uint ColorCount
    {
        get
        {
            NativeObject.GetColorCount(out var count).ThrowOnError();
            return count;
        }
    }

    public WICBitmapPaletteType Type
    {
        get
        {
            NativeObject.GetType(out var type).ThrowOnError();
            return type;
        }
    }

    public IReadOnlyList<WicColor> Colors => _colors.Value;
    private WicColor[] GetColors()
    {
        var count = ColorCount;
        if (count == 0)
            return [];

        var colors = new uint[count];
        NativeObject.GetColors(count, colors, out _).ThrowOnError();
        return [.. colors.Select(c => WicColor.FromArgb(c))];
    }

    public WicPalette CopyColors() => new(Colors);

    public override string ToString()
    {
        var list = new List<string>
        {
            Type.ToString(),
            ColorCount + " Color(s)"
        };

        if (HasAlpha)
        {
            list.Add(nameof(HasAlpha));
        }

        if (IsBlackWhite)
        {
            list.Add(nameof(IsBlackWhite));
        }

        if (IsGrayscale)
        {
            list.Add(nameof(IsGrayscale));
        }
        return string.Join(" ", list);
    }
}
