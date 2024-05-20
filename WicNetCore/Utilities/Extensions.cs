namespace WicNet.Utilities;

public static partial class Extensions
{
    //[SupportedOSPlatform("windows")]
    //public static D3DCOLORVALUE ToD3DCOLORVALUE(this Color color) => D3DCOLORVALUE.FromArgb(color.A, color.R, color.G, color.B);

    [SupportedOSPlatform("windows6.1")]
    public static IComObject<ID2D1BitmapBrush> CreateCheckerboardBrush(this IComObject<ID2D1RenderTarget> renderTarget, float size) => CreateCheckerboardBrush(renderTarget?.Object!, size);

    [SupportedOSPlatform("windows6.1")]
    public static IComObject<ID2D1BitmapBrush> CreateCheckerboardBrush(this ID2D1RenderTarget renderTarget, float size, D3DCOLORVALUE? color = null)
    {
        ArgumentNullException.ThrowIfNull(renderTarget);
        color ??= new D3DCOLORVALUE(0xFFBFBFBF);
        IComObject<ID2D1Bitmap> bmp;

        unsafe
        {
            var sizef = new D2D_SIZE_F(size * 2, size * 2);
            renderTarget.CreateCompatibleRenderTarget((nint)Unsafe.AsPointer(ref sizef), 0, 0, D2D1_COMPATIBLE_RENDER_TARGET_OPTIONS.D2D1_COMPATIBLE_RENDER_TARGET_OPTIONS_NONE, out var obj).ThrowOnError();
            using var rt = new ComObject<ID2D1BitmapRenderTarget>(obj);
            {
                rt.Object.CreateSolidColorBrush(color.Value, 0, out var sc).ThrowOnError();
                using var br = new ComObject<ID2D1SolidColorBrush>(sc);
                rt.Object.BeginDraw();
                rt.Object.FillRectangle(D2D_RECT_F.Sized(0, 0, size, size), br.Object);
                rt.Object.FillRectangle(D2D_RECT_F.Sized(size, size, size, size), br.Object);
                rt.Object.EndDraw(0, 0);
                rt.Object.GetBitmap(out var b).ThrowOnError();
                bmp = new ComObject<ID2D1Bitmap>(b);
            }
        }

        renderTarget.CreateBitmapBrush(bmp.Object, 0, 0, out var brush).ThrowOnError();
        bmp.Dispose();
        brush.SetExtendModeX(D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_WRAP);
        brush.SetExtendModeY(D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_WRAP);
        return new ComObject<ID2D1BitmapBrush>(brush);
    }

    public static void EncodeMetadata(this IComObject<IWICMetadataQueryWriter> writer, IEnumerable<WicMetadataKeyValue> metadata)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (metadata == null)
            return;

        if (!metadata.Any())
            return;

        WicImagingFactory.WithFactory(factory => EncodeMetadata(factory, writer, metadata));
    }

    private static void EncodeMetadata(IWICImagingFactory factory, IComObject<IWICMetadataQueryWriter> writer, IEnumerable<WicMetadataKeyValue> metadata)
    {
        foreach (var kv in metadata)
        {
            if (kv.Value is IEnumerable<WicMetadataKeyValue> childMetadata)
            {
                if (!childMetadata.Any())
                    continue;

                factory.CreateQueryWriter(kv.Key.Format, Unsafe.NullRef<Guid>(), out var obj).ThrowOnError();
                if (!ComWrappers.TryGetComInstance(obj, out var unk))
                    throw new InvalidOperationException();

                writer.SetMetadataByName(kv.Key.Key, unk);
                Marshal.Release(unk);
                using var childWriter = new ComObject<IWICMetadataQueryWriter>(obj);
                EncodeMetadata(factory, childWriter, childMetadata);
            }
            else
            {
                writer.SetMetadataByName(kv.Key.Key, kv.Value, kv.Type);
            }
        }
    }

    public static D2D_SIZE_F GetScaleFactor(this D2D_SIZE_U size, uint? width = null, uint? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default) => new D2D_SIZE_F(size.width, size.height).GetScaleFactor(width, height, options);
    public static D2D_SIZE_F GetScaleFactor(this D2D_SIZE_U size, int? width = null, int? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default) => new D2D_SIZE_F(size.width, size.height).GetScaleFactor(width, height, options);
    public static D2D_SIZE_F GetScaleFactor(this D2D_SIZE_F size, int? width = null, int? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default)
    {
        float? fw = width.HasValue ? width : null;
        float? fh = height.HasValue ? height : null;
        return GetScaleFactor(size, fw, fh, options);
    }

    public static D2D_SIZE_F GetScaleFactor(this D2D_SIZE_F size, uint? width = null, uint? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default)
    {
        float? fw = width.HasValue ? width : null;
        float? fh = height.HasValue ? height : null;
        return GetScaleFactor(size, fw, fh, options);
    }

    public static D2D_SIZE_F GetScaleFactor(this D2D_SIZE_F size, float? width = null, float? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default)
    {
        if (width.HasValue && width.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height.HasValue && height.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        if (size.width == 0 || size.height == 0 || (!width.HasValue && !height.HasValue))
            return new D2D_SIZE_F(1, 1);

        var scaleW = size.width == 0 ? 0 : (width ?? float.PositiveInfinity) / size.width;
        var scaleH = size.height == 0 ? 0 : (height ?? float.PositiveInfinity) / size.height;
        if (!width.HasValue)
        {
            scaleW = scaleH;
        }
        else if (!height.HasValue)
        {
            scaleH = scaleW;
        }
        else if (options.HasFlag(WicBitmapScaleOptions.Uniform))
        {
            var minscale = scaleW < scaleH ? scaleW : scaleH;
            scaleW = scaleH = minscale;
        }
        else if (options.HasFlag(WicBitmapScaleOptions.UniformToFill))
        {
            var maxscale = scaleW > scaleH ? scaleW : scaleH;
            scaleW = scaleH = maxscale;
        }

        if (options.HasFlag(WicBitmapScaleOptions.UpOnly))
        {
            if (scaleW < 1)
            {
                scaleW = 1;
            }

            if (scaleH < 1)
            {
                scaleH = 1;
            }
        }

        if (options.HasFlag(WicBitmapScaleOptions.DownOnly))
        {
            if (scaleW > 1)
            {
                scaleW = 1;
            }

            if (scaleH > 1)
            {
                scaleH = 1;
            }
        }
        return new D2D_SIZE_F(scaleW, scaleH);
    }

    public static uint GetStride(uint width, uint bitsPerPixel)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bitsPerPixel);
        return ((width * bitsPerPixel + 31) / 32 * 4);
    }

    public static IEnumerable<string> SplitToList(this string str, params char[] separators)
    {
        if (str == null || separators == null || separators.Length == 0)
            yield break;

        foreach (var s in str.Split(separators))
        {
            if (!string.IsNullOrWhiteSpace(s))
                yield return s;
        }
    }

    public static string? GetString(Func<PWSTR, uint, uint> action)
    {
        var len = action(PWSTR.Null, 0);
        if (len <= 0)
            return null;

        using var p = new AllocPwstr(len * 2);
        action(p, len);
        return p.ToString();
    }
}
