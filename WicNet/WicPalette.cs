using System;
using System.Collections.Generic;
using System.Linq;
using DirectN;

namespace WicNet;

public sealed class WicPalette : IDisposable
{
    private readonly IComObject<IWICPalette> _comObject;
    private readonly Lazy<IReadOnlyList<WicColor>> _colors;

    public WicPalette(WICBitmapPaletteType type, bool addTransparentColor = false)
        : this() => _comObject.Object.InitializePredefined((WICBitmapPaletteType)type, addTransparentColor);

    public WicPalette(WicPalette palette)
        : this((object)palette)
    {
    }

    public WicPalette(IWICPalette palette)
        : this((object)palette)
    {
    }

    public WicPalette(IComObject<IWICPalette> palette)
        : this((object)palette)
    {
    }

    public WicPalette(WicBitmapSource bitmap, int count, bool addTransparentColor = false)
        : this()
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));

        _comObject.Object.InitializeFromBitmap(bitmap.ComObject.Object, (uint)count, addTransparentColor);
    }

    public WicPalette(IEnumerable<WicColor> colors)
        : this()
    {
        if (colors == null)
            throw new ArgumentNullException(nameof(colors));

        var cols = colors.Select(c => (uint)c.ToArgb()).ToArray();
        _comObject.Object.InitializeCustom(cols, cols.Length);
    }

    public WicPalette()
        : this((object)null)
    {
    }

    public WicPalette(object source)
    {
        if (_comObject != null)
        {
            if (source is IWICPalette p)
            {
                _comObject = new ComObject<IWICPalette>(p);
            }
            else if (source is WicPalette wp)
            {
                _comObject = WICImagingFactory.CreatePalette();
                _comObject.Object.InitializeFromPalette(wp._comObject.Object);
            }
            else
            {
                _comObject = source as IComObject<IWICPalette>;
                if (_comObject == null)
                    throw new ArgumentException("Source must be an " + nameof(IWICPalette) + ".", nameof(source));
            }
        }
        else
        {
            _comObject = WICImagingFactory.CreatePalette();
        }

        _colors = new Lazy<IReadOnlyList<WicColor>>(GetColors, true);
    }

    public IComObject<IWICPalette> ComObject => _comObject;

    public bool HasAlpha
    {
        get
        {
            _comObject.Object.HasAlpha(out var value).ThrowOnError();
            return value;
        }
    }

    public bool IsBlackWhite
    {
        get
        {
            _comObject.Object.IsBlackWhite(out var value).ThrowOnError();
            return value;
        }
    }

    public bool IsGrayscale
    {
        get
        {
            _comObject.Object.IsGrayscale(out var value).ThrowOnError();
            return value;
        }
    }

    public int ColorCount
    {
        get
        {
            _comObject.Object.GetColorCount(out var count).ThrowOnError();
            return (int)count;
        }
    }

    public WICBitmapPaletteType Type
    {
        get
        {
            _comObject.Object.GetType(out var type).ThrowOnError();
            return type;
        }
    }

    public IReadOnlyList<WicColor> Colors => _colors.Value;
    private IReadOnlyList<WicColor> GetColors()
    {
        var count = ColorCount;
        if (count == 0)
            return [];

        var colors = new uint[count];
        _comObject.Object.GetColors(count, colors, out _).ThrowOnError();
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

    public void Dispose() => _comObject.SafeDispose();
}
