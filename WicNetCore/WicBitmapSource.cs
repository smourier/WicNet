namespace WicNet;

public sealed class WicBitmapSource : InterlockedComObject<IWICBitmapSource>, IComparable, IComparable<WicBitmapSource>
{
    private WicPalette? _palette;

    public WicBitmapSource(IComObject<IWICBitmapSource> comObject)
        : base(comObject)
    {
    }

    public WicBitmapSource(uint width, uint height, Guid pixelFormat, WICBitmapCreateCacheOption option = WICBitmapCreateCacheOption.WICBitmapCacheOnDemand)
        : base(WicImagingFactory.CreateBitmap(width, height, pixelFormat, option))
    {
    }

    public uint DecoderFrameCount { get; internal set; } = 1;
    public IconInformation? IconInformation { get; internal set; }
    public D2D_SIZE_U Size => new(Width, Height);
    public D2D_RECT_U Bounds => new() { right = Width, bottom = Height };
    public uint DefaultStride => Utilities.Extensions.GetStride(Width, WicPixelFormat?.BitsPerPixel ?? 0);
    public bool IsSupportedRenderTarget => IsSupportedRenderTargetFormat(PixelFormat);

    public WicPalette? Palette
    {
        get
        {
            if (_palette == null)
            {
                var palette = new WicPalette();
                NativeObject.CopyPalette(palette.ComObject.Object).ThrowOnError(false);
                if (palette.ColorCount != 0)
                {
                    _palette = palette;
                }
            }
            return _palette;
        }
        set
        {
            _palette?.Dispose();
            _palette = value;
        }
    }

    public WicPixelFormat? WicPixelFormat => WicImagingComponent.FromClsid<WicPixelFormat>(PixelFormat);
    public Guid PixelFormat
    {
        get
        {
            NativeObject.GetPixelFormat(out var pixelFormat);
            return pixelFormat;
        }
    }

    public uint Width
    {
        get
        {
            NativeObject.GetSize(out var width, out _);
            return width;
        }
    }

    public uint Height
    {
        get
        {
            NativeObject.GetSize(out _, out var height);
            return height;
        }
    }

    public double DpiX
    {
        get
        {
            NativeObject.GetResolution(out var dpix, out _);
            return dpix;
        }
    }

    public double DpiY
    {
        get
        {
            NativeObject.GetResolution(out _, out var dpiy);
            return dpiy;
        }
    }

    public uint Stride
    {
        get
        {
            var format = WicPixelFormat;
            if (format == null)
                return 0;

            return format.BitsPerPixel * Width / 8;
        }
    }

    public override string ToString() => Size.ToString();

    public Stream? GetStream()
    {
        if (NativeObject is not IWICStreamProvider prov)
            return null;

        var strm = prov.GetStream();
        var sois = new StreamOnIStream(strm, true);
        return sois;
    }

    public Coordinates? GetCoordinates() => Coordinates.Get(this);
    public PHOTO_ORIENTATION? GetOrientation()
    {
        using var reader = GetMetadataReader();
        if (reader == null)
            return null;

        var orientation = new WicMetadataPolicies(reader).PhotoOrientation;
        if (!orientation.HasValue)
            return null;

        return (PHOTO_ORIENTATION)orientation.Value;
    }

    public WicBitmapSource? GetThumbnail()
    {
        var bmp = (NativeObject as IWICBitmapFrameDecode)?.GetThumbnail();
        return bmp != null ? new WicBitmapSource(bmp) : null;
    }

    public WicMetadataQueryReader? GetMetadataReader()
    {
        var reader = (NativeObject as IWICBitmapFrameDecode)?.GetMetadataQueryReader();
        return reader != null ? new WicMetadataQueryReader(reader) : null;
    }

    public IReadOnlyList<WicColorContext> GetColorContexts()
    {
        var list = new List<WicColorContext>();
        var contexts = (NativeObject as IWICBitmapFrameDecode)?.GetColorContexts();
        if (contexts != null)
        {
            list.AddRange(contexts.Select(cc => new WicColorContext(cc)));
        }
        return list;
    }

    public WicColorContext? GetBestColorContext()
    {
        var contexts = GetColorContexts();
        if (contexts.Count == 0)
            return null;

        if (contexts.Count == 1)
            return contexts[0];

        // https://stackoverflow.com/a/70215280/403671
        // get last not uncalibrated color context
        WicColorContext? best = null;
        foreach (var ctx in contexts.Reverse())
        {
            if (ctx.ExifColorSpace.HasValue && ctx.ExifColorSpace.Value == 0xFFFF)
                continue;

            best = ctx;
        }

        // last resort
        best ??= contexts[contexts.Count - 1];
        foreach (var context in contexts)
        {
            if (best?.Equals(context) == true)
                continue;

            context.Dispose();
        }
        return best;
    }

    [SupportedOSPlatform("windows8.0")]
    public WicBitmapSource GetColorTransform(WicColorContext sourceColorContext, WicColorContext destinationColorContext, Guid destinationPixelFormat)
    {
        ArgumentNullException.ThrowIfNull(sourceColorContext);
        ArgumentNullException.ThrowIfNull(destinationColorContext);
        var transformer = WicImagingFactory.CreateColorTransformer();
        transformer.Initialize(ComObject, sourceColorContext.ComObject, destinationColorContext.ComObject, destinationPixelFormat);
        return new WicBitmapSource(transformer);
    }

    public void CenterClip(uint? width, uint? height)
    {
        if (!width.HasValue && !height.HasValue)
            return;

        var rect = new WICRect();
        var w = Width;
        var h = Height;
        if (width.HasValue && width.Value < w)
        {
            rect.Width = (int)width.Value;
            rect.X = (int)((w - width.Value) / 2);
        }
        else
        {
            rect.Width = (int)w;
            rect.X = 0;
        }

        if (height.HasValue && height.Value < h)
        {
            rect.Height = (int)height.Value;
            rect.Y = (int)((h - height.Value) / 2);
        }
        else
        {
            rect.Height = (int)h;
            rect.Y = 0;
        }

        var clip = WicImagingFactory.CreateBitmapClipper();
        clip.Object.Initialize(NativeObject, rect).ThrowOnError();
        ExchangeDisposable(clip);
    }

    public void Clip(int left, int top, int width, int height)
    {
        var rect = new WICRect
        {
            X = left,
            Y = top,
            Width = width,
            Height = height
        };

        var clip = WicImagingFactory.CreateBitmapClipper();
        clip.Object.Initialize(NativeObject, rect).ThrowOnError();
        ExchangeDisposable(clip);
    }

    public void FlipRotate(WICBitmapTransformOptions options)
    {
        var clip = WicImagingFactory.CreateBitmapFlipRotator();
        clip.Object.Initialize(NativeObject, options).ThrowOnError();
        ExchangeDisposable(clip);
    }

    public bool ConvertTo(
        Guid pixelFormat,
        WICBitmapDitherType ditherType = WICBitmapDitherType.WICBitmapDitherTypeNone,
        WicPalette? palette = null,
        double alphaThresholdPercent = 0,
        WICBitmapPaletteType paletteTranslate = WICBitmapPaletteType.WICBitmapPaletteTypeCustom)
    {
        if (pixelFormat == PixelFormat)
            return false;

        if (WicPixelFormat == null)
            throw new InvalidOperationException();

        var cvt = WicPixelFormat.GetPixelFormatConvertersTo(pixelFormat).FirstOrDefault();
        if (cvt == null)
        {
            var pf = WicPixelFormat.FromClsid(pixelFormat);
            var format = pf?.ToString() ?? pixelFormat.ToString();
            throw new WicNetException("WIC0005: There's no converter available to convert from '" + WicPixelFormat + "' to '" + format + "'.");
        }

        var converter = cvt.Convert(this, pixelFormat, ditherType, palette, alphaThresholdPercent, paletteTranslate);
        ExchangeDisposable(converter);
        return true;
    }

    public void CopyPixels(uint bufferSize, nint buffer, uint? stride = null)
    {
        ArgumentOutOfRangeException.ThrowIfZero(buffer);
        stride ??= DefaultStride;
        NativeObject.CopyPixels(Unsafe.NullRef<WICRect>(), stride.Value, bufferSize, buffer).ThrowOnError();
    }

    public void CopyPixels(int left, int top, uint width, uint height, uint bufferSize, nint buffer, uint? stride = null)
    {
        if (stride.HasValue && stride.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(stride));

        ArgumentOutOfRangeException.ThrowIfZero(buffer);
        stride ??= DefaultStride;
        var rect = new WICRect
        {
            X = left,
            Y = top,
            Width = (int)width,
            Height = (int)height
        };
        NativeObject.CopyPixels(rect, stride.Value, bufferSize, buffer).ThrowOnError();
    }

    public byte[] CopyPixels(uint? stride = null) => CopyPixels(0, 0, Width, Height, stride);
    public byte[] CopyPixels(int left, int top, uint width, uint height, uint? stride = null)
    {
        if (stride.HasValue && stride.Value <= width)
            throw new ArgumentOutOfRangeException(nameof(stride));

        stride ??= DefaultStride;
        var size = height * stride.Value;
        var bytes = new byte[size];
        if (size > 0)
        {
            CopyPixels(left, top, width, height, size, bytes.AsPointer(), stride);
        }
        return bytes;
    }

    public void WithSoftwareBitmap(bool forceReadOnly, Action<object?> action, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(action);
        var softwareBitmap = ToSoftwareBitmap(forceReadOnly, throwOnError);
        try
        {
            action(softwareBitmap);
        }
        finally
        {
            softwareBitmap.FinalRelease();
        }
    }

    public T WithSoftwareBitmap<T>(bool forceReadOnly, Func<object?, T> func, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(func);
        var softwareBitmap = ToSoftwareBitmap(forceReadOnly, throwOnError);
        try
        {
            return func(softwareBitmap);
        }
        finally
        {
            softwareBitmap.FinalRelease();
        }
    }

    public object ToSoftwareBitmap(bool forceReadOnly, bool throwOnError = true)
    {
        if (NativeObject is not IWICBitmap bmp)
            throw new WicNetException("WIC0004: Converting to WinRT SoftwareBitmap is only supported on in-memory bitmaps. You must Clone this bitmap first.");

        var factory = ComObject<ISoftwareBitmapNativeFactory>.CoCreate(Constants.CLSID_SoftwareBitmapNativeFactory)!;

        // note: parameter incorrect is probably a format issue
        // SoftwareBitmap only supports a few https://learn.microsoft.com/en-us/uwp/api/windows.graphics.imaging.bitmappixelformat
        // and SoftwareBitmapSource only supports Brga8...
        factory.Object.CreateFromWICBitmap(bmp, forceReadOnly, Constants.IID_ISoftwareBitmap, out var softwareBitmap).ThrowOnError(throwOnError);
        return softwareBitmap;
    }

    // use it like this 
    //    var wic = WicBitmapSource.FromSoftwareBitmap(((IWinRTObject)softwareBitmap).NativeObject.ThisPtr);
    public static WicBitmapSource? FromSoftwareBitmap(nint nativeSoftwareBitmap)
    {
        if (nativeSoftwareBitmap == 0)
            return null;

        using var native = DirectN.Extensions.Com.ComObject.FromPointer<ISoftwareBitmapNative>(nativeSoftwareBitmap);
        if (native == null)
            return null;

        native.Object.GetData(typeof(IWICBitmap).GUID, out var unk);
        if (unk == 0)
            return null;

        var bmp = DirectN.Extensions.Com.ComObject.FromPointer<IWICBitmapSource>(unk)!;
        return new WicBitmapSource(bmp);
    }

    public static WicBitmapSource FromHIcon(HICON iconHandle) => new(WicImagingFactory.CreateBitmapFromHICON(iconHandle));
    public static WicBitmapSource FromMemory(uint width, uint height, Guid pixelFormat, uint stride, byte[] buffer) => new(WicImagingFactory.CreateBitmapFromMemory(width, height, pixelFormat, stride, buffer));
    public static WicBitmapSource FromHBitmap(HBITMAP bitmapHandle, WICBitmapAlphaChannelOption options = WICBitmapAlphaChannelOption.WICBitmapUseAlpha) => new(WicImagingFactory.CreateBitmapFromHBITMAP(bitmapHandle, options));
    public static WicBitmapSource FromHBitmap(HBITMAP bitmapHandle, HPALETTE paletteHandle, WICBitmapAlphaChannelOption options = WICBitmapAlphaChannelOption.WICBitmapUseAlpha) => new(WicImagingFactory.CreateBitmapFromHBITMAP(bitmapHandle, paletteHandle, options));
    public static WicBitmapSource FromSource(WicBitmapSource source, WICBitmapCreateCacheOption option = WICBitmapCreateCacheOption.WICBitmapNoCache) => new(WicImagingFactory.CreateBitmapFromSource(source?.ComObject!, option));
    public static WicBitmapSource FromSourceRect(WicBitmapSource source, uint x, uint y, uint width, uint height) => new(WicImagingFactory.CreateBitmapFromSourceRect(source?.ComObject!, x, y, width, height));

    public void Scale(int boxSize, WICBitmapInterpolationMode mode = WICBitmapInterpolationMode.WICBitmapInterpolationModeNearestNeighbor, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(boxSize);
        if (Width > Height)
        {
            Scale(boxSize, null, mode, options);
            return;
        }
        Scale(null, boxSize, mode, options);
    }

    public void Scale(int? width, int? height, WICBitmapInterpolationMode mode = WICBitmapInterpolationMode.WICBitmapInterpolationModeNearestNeighbor, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default)
    {
        if (width.HasValue && width.Value < 0)
            throw new ArgumentException(null, nameof(width));

        if (height.HasValue && height.Value < 0)
            throw new ArgumentException(null, nameof(height));

        var size = Size;
        var factor = size.GetScaleFactor(width, height, options);
        if (factor.width == 1 && factor.height == 1)
            return;

        var neww = (uint)(size.width * factor.width);
        var newh = (uint)(size.height * factor.height);
        var clip = WicImagingFactory.CreateBitmapScaler();
        clip.Object.Initialize(NativeObject, neww, newh, mode).ThrowOnError();
        ExchangeDisposable(clip);
    }

    public static WicBitmapSource Load(string filePath, uint frameIndex = 0, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        using var decoder = WicBitmapDecoder.Load(filePath, options: options);
        var frame = decoder.GetFrame(frameIndex);
        frame.DecoderFrameCount = decoder.FrameCount;
        return frame;
    }

    public static WicBitmapSource Load(nuint fileHandle, uint frameIndex = 0, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        using var decoder = WicBitmapDecoder.Load(fileHandle, options: options);
        var frame = decoder.GetFrame(frameIndex);
        frame.DecoderFrameCount = decoder.FrameCount;
        return frame;
    }

    public static WicBitmapSource Load(Stream stream, uint frameIndex = 0, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
    {
        using var decoder = WicBitmapDecoder.Load(stream, options: options);
        var frame = decoder.GetFrame(frameIndex);
        frame.DecoderFrameCount = decoder.FrameCount;
        return frame;
    }

    [SupportedOSPlatform("windows6.1")]
    public IComObject<ID2D1RenderTarget> CreateRenderTarget(D2D1_RENDER_TARGET_PROPERTIES? renderTargetProperties = null) => CreateRenderTarget<ID2D1RenderTarget>(renderTargetProperties);

    [SupportedOSPlatform("windows8.0")]
    public IComObject<ID2D1DeviceContext> CreateDeviceContext(D2D1_RENDER_TARGET_PROPERTIES? renderTargetProperties = null) => CreateRenderTarget<ID2D1DeviceContext>(renderTargetProperties);

    [SupportedOSPlatform("windows6.1")]
    public IComObject<T> CreateRenderTarget<T>(D2D1_RENDER_TARGET_PROPERTIES? renderTargetProperties = null) where T : ID2D1RenderTarget
    {
        var bitmap = AsBitmap() ?? throw new InvalidOperationException();
        Functions.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED, typeof(ID2D1Factory).GUID, 0, out var unk).ThrowOnError();
        using var fac = DirectN.Extensions.Com.ComObject.FromPointer<ID2D1Factory>(unk)!;
        var props = renderTargetProperties ?? new D2D1_RENDER_TARGET_PROPERTIES();
        fac.Object.CreateWicBitmapRenderTarget(bitmap.Object, props, out var rt).ThrowOnError();
        return new ComObject<T>(rt);
    }

    public void WithLock(WICBitmapLockFlags flags, Action<WicBitmapLock> action, WICRect? rect = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        using var lck = CheckBitmap().Lock(flags, rect);
        using var blck = new WicBitmapLock(lck);
        action(blck);
    }

    public T WithLock<T>(WICBitmapLockFlags flags, Func<WicBitmapLock, T> func, WICRect? rect = null)
    {
        ArgumentNullException.ThrowIfNull(func);
        using var lck = CheckBitmap().Lock(flags, rect);
        using var blck = new WicBitmapLock(lck);
        return func(blck);
    }

    public IComObject<IWICBitmap>? AsBitmap() => ComObject.As<IWICBitmap>();
    public WicBitmapSource Clone(WICBitmapCreateCacheOption options = WICBitmapCreateCacheOption.WICBitmapNoCache)
    {
        var bmp = WicImagingFactory.CreateBitmapFromSource(ComObject, options);
        return new WicBitmapSource(bmp);
    }

    private IWICBitmap CheckBitmap()
    {
        if (NativeObject is not IWICBitmap bmp)
            throw new WicNetException("WIC0002: Lock is only supported on in-memory bitmaps. You must Clone this bitmap first.");

        return bmp;
    }

    // https://stackoverflow.com/a/30669562/403671
    public static bool IsSupportedRenderTargetFormat(Guid format) =>
        format == WicPixelFormat.GUID_WICPixelFormat8bppAlpha ||
        format == WicPixelFormat.GUID_WICPixelFormat32bppBGR ||
        format == WicPixelFormat.GUID_WICPixelFormat32bppRGB ||
        format == WicPixelFormat.GUID_WICPixelFormat32bppPBGRA ||
        format == WicPixelFormat.GUID_WICPixelFormat32bppPRGBA ||
        format == WicPixelFormat.GUID_WICPixelFormat64bppRGB ||
        format == WicPixelFormat.GUID_WICPixelFormat64bppPRGBA ||
        format == WicPixelFormat.GUID_WICPixelFormat64bppPRGBAHalf ||
        format == WicPixelFormat.GUID_WICPixelFormat64bppRGBHalf ||
        format == WicPixelFormat.GUID_WICPixelFormat128bppPRGBAFloat ||
        format == WicPixelFormat.GUID_WICPixelFormat128bppRGBFloat;

    public void Save(string filePath,
        Guid? encoderContainerFormat = null,
        Guid? pixelFormat = null,
        WICBitmapEncoderCacheOption cacheOptions = WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache,
        IEnumerable<KeyValuePair<string, object>>? encoderOptions = null,
        IEnumerable<WicMetadataKeyValue>? metadata = null,
        WicPalette? encoderPalette = null,
        WicPalette? framePalette = null,
        WICRect? sourceRectangle = null,
        IEnumerable<WicColorContext>? colorContexts = null)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        Guid format;
        if (!encoderContainerFormat.HasValue)
        {
            var encoder = WicEncoder.FromFileExtension(Path.GetExtension(filePath)) ?? throw new WicNetException("WIC0003: Cannot determine encoder from file path.");
            format = encoder.ContainerFormat;
        }
        else
        {
            format = encoderContainerFormat.Value;
        }

        using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        Save(file, format, pixelFormat, cacheOptions, encoderOptions, metadata, encoderPalette, framePalette, sourceRectangle, colorContexts);
    }

    public void Save(
        Stream stream,
        Guid encoderContainerFormat,
        Guid? pixelFormat = null,
        WICBitmapEncoderCacheOption cacheOptions = WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache,
        IEnumerable<KeyValuePair<string, object>>? encoderOptions = null,
        IEnumerable<WicMetadataKeyValue>? metadata = null,
        WicPalette? encoderPalette = null,
        WicPalette? framePalette = null,
        WICRect? sourceRectangle = null,
        IEnumerable<WicColorContext>? colorContexts = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var encoder = WicImagingFactory.CreateEncoder(encoderContainerFormat);
        var mis = new ManagedIStream(stream);
        encoder.Initialize(mis, cacheOptions);

        if (encoderPalette != null)
        {
            // gifs...
            encoder.SetPalette(encoderPalette.ComObject);
        }

        using var frame = encoder.CreateNewFrame();
        if (encoderOptions != null)
        {
            frame.Bag.Object.Write(encoderOptions);
        }

        frame.Initialize();

        if (metadata?.Any() == true)
        {
            using var writer = frame.GetMetadataQueryWriter();
            writer.EncodeMetadata(metadata);
        }

        if (pixelFormat.HasValue)
        {
            frame.SetPixelFormat(pixelFormat.Value);
        }

        if (framePalette != null)
        {
            frame.Encode.SetPalette(framePalette.ComObject);
        }

        if (colorContexts?.Any() == true)
        {
            frame.SetColorContexts(colorContexts.Select(c => c.ComObject));
        }

        // "WIC error 0x88982F0C. The component is not initialized" here can mean the palette is not set
        // "WIC error 0x88982F45. The bitmap palette is unavailable" here means for example we're saving a file that doesn't support palette (even if we called SetPalette before, it may be useless)
        frame.WriteSource(ComObject, sourceRectangle);
        frame.Commit();
        encoder.Commit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _palette?.Dispose();
        }
        base.Dispose(disposing);
    }

    int IComparable.CompareTo(object? obj) => CompareTo(obj as WicBitmapSource);
    public int CompareTo(WicBitmapSource? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (ReferenceEquals(this, other))
            return 0;

        if (Width != Height)
        {
            if (other.Width == other.Height)
                return -1;
        }
        else
        {
            if (other.Width != other.Height)
                return 1;
        }

        var size = Width * Height;
        var otherSize = other.Width * other.Height;
        if (size != otherSize)
            return size.CompareTo(otherSize);

        if (WicPixelFormat == null)
            return 1;

        if (other.WicPixelFormat == null)
            return -1;

        return WicPixelFormat.CompareTo(other.WicPixelFormat);
    }
}
