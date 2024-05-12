using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DirectN;
using DirectNAot.Extensions;
using DirectNAot.Extensions.Com;

namespace WicNet;

public sealed class WicPixelFormatConverter : WicImagingComponent
{
    private readonly Lazy<IReadOnlyList<WicPixelFormat>> _pixelFormatsList;

    public WicPixelFormatConverter(object comObject)
        : base(comObject)
    {
        using var info = new ComObjectWrapper<IWICFormatConverterInfo>(comObject);
        unsafe
        {
            var n = Unsafe.AsRef<Guid[]>(null);
            info.Object.GetPixelFormats(0, ref n, out var len);
            var pf = new Guid[len];
            if (len > 0)
            {
                info.Object.GetPixelFormats(len, ref pf, out _);
            }

            PixelFormats = pf;
            _pixelFormatsList = new Lazy<IReadOnlyList<WicPixelFormat>>(GetPixelFormatsList, true);
        }
    }

    public IReadOnlyList<Guid> PixelFormats { get; }
    public IReadOnlyList<WicPixelFormat> PixelFormatsList => _pixelFormatsList.Value;

    private IComObject<IWICFormatConverterInfo> GetComObject() => WicImagingFactory.WithFactory(f =>
    {
        f.CreateComponentInfo(Clsid, out var info).ThrowOnError();
        return new ComObject<IWICFormatConverterInfo>((IWICFormatConverterInfo)info);
    });

    private IReadOnlyList<WicPixelFormat> GetPixelFormatsList()
    {
        var list = new List<WicPixelFormat>();
        foreach (var pf in PixelFormats)
        {
            var format = WicPixelFormat.FromClsid(pf);
            if (format != null)
            {
                list.Add(format);
            }
        }

        list.Sort();
        return list.AsReadOnly();
    }

    public bool CanConvert(Guid from, Guid to)
    {
        using var co = GetComObject();
        using var cvt = co.CreateInstance();
        if (!cvt.Object.CanConvert(from, to, out var can).IsSuccess)
            return false;

        return can;
    }

    public IComObject<IWICFormatConverter> Convert(
        WicBitmapSource source,
        Guid targetFormat,
        WICBitmapDitherType ditherType = WICBitmapDitherType.WICBitmapDitherTypeNone,
        WicPalette? palette = null,
        double alphaThresholdPercent = 0,
        WICBitmapPaletteType paletteTranslate = WICBitmapPaletteType.WICBitmapPaletteTypeCustom)
    {
        ArgumentNullException.ThrowIfNull(source);
        WicPalette? pal = null;
        var p = palette;
        if (p != null && p.ColorCount > 0)
        {
            // we must clone it
            pal = p.CopyColors();
        }

        using var co = GetComObject();
        var cvt = co.CreateInstance();
        cvt.Object.Initialize(source.ComObject.Object, targetFormat, ditherType, pal?.ComObject.Object!, alphaThresholdPercent, paletteTranslate).ThrowOnError();
        return cvt;
    }
}
