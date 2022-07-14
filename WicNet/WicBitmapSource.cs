using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DirectN;
using WicNet.Utilities;

namespace WicNet
{
#pragma warning disable CA1036 // Override methods on comparable types
    public sealed class WicBitmapSource : IDisposable, IComparable, IComparable<WicBitmapSource>
#pragma warning restore CA1036 // Override methods on comparable types
    {
        public static bool NoHardwareSupport { get; set; } // azure

        private IComObject<IWICBitmapSource> _comObject;
        private WicPalette _palette;

        public WicBitmapSource(object comObject)
        {
            _comObject = new ComObjectWrapper<IWICBitmapSource>(comObject).ComObject;
        }

        public WicBitmapSource(int width, int height, Guid pixelFormat, WICBitmapCreateCacheOption option = WICBitmapCreateCacheOption.WICBitmapCacheOnDemand)
        {
            _comObject = WICImagingFactory.CreateBitmap(width, height, pixelFormat, option);
        }

        public IComObject<IWICBitmapSource> ComObject => _comObject;
        public WicIntSize Size => new WicIntSize(Width, Height);
        public WICRect Bounds => new WICRect(0, 0, Width, Height);
        public int DefaultStride => Utilities.Extensions.GetStride(Width, WicPixelFormat.BitsPerPixel);

        public WicPalette Palette
        {
            get
            {
                if (_palette == null)
                {
                    var palette = new WicPalette();
                    _comObject.Object.CopyPalette(palette.ComObject.Object).ThrowOnError(false);
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

        public WicPixelFormat WicPixelFormat => WicImagingComponent.FromClsid<WicPixelFormat>(PixelFormat);
        public Guid PixelFormat
        {
            get
            {
                _comObject.Object.GetPixelFormat(out var pixelFormat);
                return pixelFormat;
            }
        }

        public int Width
        {
            get
            {
                _comObject.Object.GetSize(out var width, out _);
                return width;
            }
        }

        public int Height
        {
            get
            {
                _comObject.Object.GetSize(out _, out var height);
                return height;
            }
        }

        public double DpiX
        {
            get
            {
                _comObject.Object.GetResolution(out var dpix, out _);
                return dpix;
            }
        }

        public double DpiY
        {
            get
            {
                _comObject.Object.GetResolution(out _, out var dpiy);
                return dpiy;
            }
        }

        public int Stride => WicPixelFormat.BitsPerPixel * Width / 8;

        public WicBitmapSource GetThumbnail()
        {
            var bmp = _comObject.As<IWICBitmapFrameDecode>(false)?.GetThumbnail();
            return bmp != null ? new WicBitmapSource(bmp) : null;
        }

        public WicMetadataQueryReader GetMetadataReader()
        {
            var reader = _comObject.As<IWICBitmapFrameDecode>(false)?.GetMetadataQueryReader();
            return reader != null ? new WicMetadataQueryReader(reader) : null;
        }

        public void CenterClip(int? width, int? height)
        {
            if (!width.HasValue && !height.HasValue)
                return;

            var rect = new WICRect();
            int w = Width;
            int h = Height;
            if (width.HasValue && width.Value < w)
            {
                rect.Width = width.Value;
                rect.X = (w - width.Value) / 2;
            }
            else
            {
                rect.Width = w;
                rect.X = 0;
            }

            if (height.HasValue && height.Value < h)
            {
                rect.Height = height.Value;
                rect.Y = (h - height.Value) / 2;
            }
            else
            {
                rect.Height = h;
                rect.Y = 0;
            }

            var clip = WICImagingFactory.CreateBitmapClipper();
            clip.Object.Initialize(_comObject.Object, ref rect).ThrowOnError();
            _comObject?.Dispose();
            _comObject = clip;
        }

        public void Clip(int left, int top, int width, int height)
        {
            var rect = new WICRect();
            rect.X = left;
            rect.Y = top;
            rect.Width = width;
            rect.Height = height;

            var clip = WICImagingFactory.CreateBitmapClipper();
            clip.Object.Initialize(_comObject.Object, ref rect).ThrowOnError();
            _comObject?.Dispose();
            _comObject = clip;
        }

        public void Rotate(WICBitmapTransformOptions options)
        {
            var clip = WICImagingFactory.CreateBitmapFlipRotator();
            clip.Object.Initialize(_comObject.Object, options).ThrowOnError();
            _comObject?.Dispose();
            _comObject = clip;
        }

        public void ConvertTo(Guid pixelFormat, WICBitmapDitherType ditherType = WICBitmapDitherType.WICBitmapDitherTypeNone, WicPalette palette = null, double alphaThresholdPercent = 0, WICBitmapPaletteType paletteTranslate = WICBitmapPaletteType.WICBitmapPaletteTypeCustom)
        {
            if (WicPixelFormat == null)
                throw new InvalidOperationException();

            var cvt = WicPixelFormat.GetPixelFormatConvertersTo(pixelFormat).FirstOrDefault();
            if (cvt == null)
                throw new InvalidOperationException();

            var converter = cvt.Convert(this, pixelFormat, ditherType, palette, alphaThresholdPercent, paletteTranslate);
            _comObject?.Dispose();
            _comObject = converter;
        }

        public void CopyPixels(int bufferSize, IntPtr buffer, int? stride = null)
        {
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            if (buffer == IntPtr.Zero)
                throw new ArgumentOutOfRangeException(nameof(buffer));

            stride = stride ?? DefaultStride;
            _comObject.Object.CopyPixels(IntPtr.Zero, stride.Value, bufferSize, buffer).ThrowOnError();
        }

        public void CopyPixels(int left, int top, int width, int height, int bufferSize, IntPtr buffer, int? stride = null)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            if (stride.HasValue && stride.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(stride));

            if (buffer == IntPtr.Zero)
                throw new ArgumentOutOfRangeException(nameof(buffer));

            stride = stride ?? DefaultStride;
            var rect = new WICRect();
            rect.X = left;
            rect.Y = top;
            rect.Width = width;
            rect.Height = height;
            using (var mem = new ComMemory(rect))
            {
                _comObject.Object.CopyPixels(mem.Pointer, stride.Value, bufferSize, buffer).ThrowOnError();
            }
        }

        public byte[] CopyPixels(int? stride = null) => CopyPixels(0, 0, Width, Height, stride);
        public byte[] CopyPixels(int left, int top, int width, int height, int? stride = null)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            if (stride.HasValue && stride.Value <= width)
                throw new ArgumentOutOfRangeException(nameof(stride));

            stride = stride ?? DefaultStride;
            var size = height * stride.Value;
            var bytes = new byte[size];
            if (size > 0)
            {
                var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
                CopyPixels(left, top, width, height, size, ptr, stride);
            }
            return bytes;
        }

        public static WicBitmapSource FromHIcon(IntPtr iconHandle) => new WicBitmapSource(WICImagingFactory.CreateBitmapFromHICON(iconHandle));
        public static WicBitmapSource FromMemory(int width, int height, Guid pixelFormat, int stride, byte[] buffer) => new WicBitmapSource(WICImagingFactory.CreateBitmapFromMemory(width, height, pixelFormat, stride, buffer));
        public static WicBitmapSource FromHBitmap(IntPtr bitmapHandle, WICBitmapAlphaChannelOption options = WICBitmapAlphaChannelOption.WICBitmapUseAlpha) => new WicBitmapSource(WICImagingFactory.CreateBitmapFromHBITMAP(bitmapHandle, options));
        public static WicBitmapSource FromHBitmap(IntPtr bitmapHandle, IntPtr paletteHandle, WICBitmapAlphaChannelOption options = WICBitmapAlphaChannelOption.WICBitmapUseAlpha) => new WicBitmapSource(WICImagingFactory.CreateBitmapFromHBITMAP(bitmapHandle, paletteHandle, options));
        public static WicBitmapSource FromSource(WicBitmapSource source, WICBitmapCreateCacheOption option = WICBitmapCreateCacheOption.WICBitmapNoCache) => new WicBitmapSource(WICImagingFactory.CreateBitmapFromSource(source?.ComObject, option));
        public static WicBitmapSource FromSourceRect(WicBitmapSource source, int x, int y, int width, int height) => new WicBitmapSource(WICImagingFactory.CreateBitmapFromSourceRect(source?.ComObject, x, y, width, height));

        public void Scale(int boxSize, WICBitmapInterpolationMode mode = WICBitmapInterpolationMode.WICBitmapInterpolationModeNearestNeighbor, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default)
        {
            if (boxSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(boxSize));

            if (Width > Height)
            {
                Scale(boxSize, null, mode, options);
                return;
            }
            Scale(null, boxSize, mode, options);
        }

        public void Scale(int? width, int? height, WICBitmapInterpolationMode mode = WICBitmapInterpolationMode.WICBitmapInterpolationModeNearestNeighbor, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default)
        {
            if (!width.HasValue && !height.HasValue)
                return;

            if (width.HasValue && width.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height.HasValue && height.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            int neww;
            int newh;
            if (width.HasValue && height.HasValue)
            {
                neww = width.Value;
                newh = height.Value;
            }
            else
            {
                int w = Width;
                int h = Height;
                if (w == 0 || h == 0)
                    return;

                if (width.HasValue)
                {
                    if ((options & WicBitmapScaleOptions.DownOnly) == WicBitmapScaleOptions.DownOnly)
                    {
                        if (width.Value > w)
                            return;
                    }

                    neww = width.Value;
                    newh = width.Value * h / w;
                }
                else // height.HasValue
                {
                    if ((options & WicBitmapScaleOptions.DownOnly) == WicBitmapScaleOptions.DownOnly)
                    {
                        if (height.Value > h)
                            return;
                    }

                    newh = height.Value;
                    neww = height.Value * w / h;
                }
            }

            if (neww == 0 || newh == 0)
                return;

            var clip = WICImagingFactory.CreateBitmapScaler();
            clip.Object.Initialize(_comObject.Object, neww, newh, mode).ThrowOnError();
            _comObject?.Dispose();
            _comObject = clip;
        }

        public static WicBitmapSource Load(string filePath, int frameIndex = 0, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            using (var decoder = WicBitmapDecoder.Load(filePath, options: options))
                return decoder.GetFrame(frameIndex);
        }

        public static WicBitmapSource Load(IntPtr fileHandle, int frameIndex = 0, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            using (var decoder = WicBitmapDecoder.Load(fileHandle, options: options))
                return decoder.GetFrame(frameIndex);
        }

        public static WicBitmapSource Load(Stream stream, int frameIndex = 0, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand)
        {
            using (var decoder = WicBitmapDecoder.Load(stream, options: options))
                return decoder.GetFrame(frameIndex);
        }

        public IComObject<ID2D1RenderTarget> CreateRenderTarget(D2D1_RENDER_TARGET_PROPERTIES? renderTargetProperties = null) => CreateRenderTarget<ID2D1RenderTarget>(renderTargetProperties);
        public IComObject<ID2D1DeviceContext> CreateDeviceContext(D2D1_RENDER_TARGET_PROPERTIES? renderTargetProperties = null) => CreateRenderTarget<ID2D1DeviceContext>(renderTargetProperties);
        public IComObject<T> CreateRenderTarget<T>(D2D1_RENDER_TARGET_PROPERTIES? renderTargetProperties = null) where T : ID2D1RenderTarget
        {
            var bitmap = AsBitmap();
            using (var fac = D2D1Functions.D2D1CreateFactory())
                return fac.CreateWicBitmapRenderTarget<T>(bitmap, renderTargetProperties);
        }

        public void WithLock(WICBitmapLockFlags flags, Action<WicBitmapLock> action, WICRect? rect = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using (var lck = CheckBitmap().Lock(flags, rect))
                action(new WicBitmapLock(lck));
        }

        public T WithLock<T>(WICBitmapLockFlags flags, Func<WicBitmapLock, T> func, WICRect? rect = null)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            using (var lck = CheckBitmap().Lock(flags, rect))
                return func(new WicBitmapLock(lck));
        }

        public IComObject<IWICBitmap> AsBitmap(bool throwOnError = true) => _comObject.AsComObject<IWICBitmap>(throwOnError);

        public WicBitmapSource Clone(WICBitmapCreateCacheOption options = WICBitmapCreateCacheOption.WICBitmapNoCache)
        {
            var bmp = WICImagingFactory.CreateBitmapFromSource(_comObject, options);
            return new WicBitmapSource(bmp);
        }

        private IWICBitmap CheckBitmap()
        {
            var bmp = _comObject.As<IWICBitmap>();
            if (bmp == null)
                throw new WicNetException("WIC0002: Lock is only supported on in-memory bitmaps. You must Clone this bitmap first.");

            return bmp;
        }

        public bool IsSupportedRenderTarget => IsSupportedRenderTargetFormat(PixelFormat);

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
            IEnumerable<KeyValuePair<string, object>> encoderOptions = null,
            IEnumerable<WicMetadataKeyValue> metadata = null,
            WicPalette encoderPalette = null,
            WicPalette framePalette = null,
            WICRect? sourceRectangle = null)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            Guid format;
            if (!encoderContainerFormat.HasValue)
            {
                var encoder = WicEncoder.FromFileExtension(Path.GetExtension(filePath));
                if (encoder == null)
                    throw new WicNetException("WIC0003: Cannot determine encoder from file path.");

                format = encoder.ContainerFormat;
            }
            else
            {
                format = encoderContainerFormat.Value;
            }

            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                Save(file, format, pixelFormat, cacheOptions, encoderOptions, metadata, encoderPalette, framePalette, sourceRectangle);
        }

        public void Save(
            Stream stream,
            Guid encoderContainerFormat,
            Guid? pixelFormat = null,
            WICBitmapEncoderCacheOption cacheOptions = WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache,
            IEnumerable<KeyValuePair<string, object>> encoderOptions = null,
            IEnumerable<WicMetadataKeyValue> metadata = null,
            WicPalette encoderPalette = null,
            WicPalette framePalette = null,
            WICRect? sourceRectangle = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var encoder = WICImagingFactory.CreateEncoder(encoderContainerFormat))
            {
                var mis = new ManagedIStream(stream);
                encoder.Initialize(mis, cacheOptions);

                if (encoderPalette != null)
                {
                    // gifs...
                    encoder.SetPalette(encoderPalette.ComObject);
                }

                var frameBag = encoder.CreateNewFrame();

                if (encoderOptions != null)
                {
                    frameBag.Item2.Write(encoderOptions);
                }

                frameBag.Initialize();

                if (metadata?.Any() == true)
                {
                    using (var writer = frameBag.GetMetadataQueryWriter())
                    {
                        writer.EncodeMetadata(metadata);
                    }
                }

                if (pixelFormat.HasValue)
                {
                    frameBag.SetPixelFormat(pixelFormat.Value);
                }

                if (framePalette != null)
                {
                    frameBag.Item1.SetPalette(framePalette.ComObject);
                }

                // "WIC error 0x88982F0C. The component is not initialized" here can mean the palette is not set
                // "WIC error 0x88982F45. The bitmap palette is unavailable" here means for example we're saving a file that doesn't support palette (even if we called SetPalette before, it may be useless)
                frameBag.WriteSource(_comObject, sourceRectangle);
                frameBag.Item1.Commit();
                encoder.Commit();
            }
        }

        public void Dispose()
        {
            _palette?.Dispose();
            _comObject?.Dispose();
        }

        int IComparable.CompareTo(object obj) => CompareTo(obj as WicBitmapSource);
        public int CompareTo(WicBitmapSource other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

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

            if (PixelFormat == null)
                return 1;

            if (other.PixelFormat == null)
                return -1;

            return PixelFormat.CompareTo(other.PixelFormat);
        }
    }
}
