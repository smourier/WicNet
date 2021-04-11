using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WicNet.Interop;

namespace WicNet
{
#pragma warning disable CA1036 // Override methods on comparable types
    public sealed class WicBitmapSource : IDisposable, IComparable, IComparable<WicBitmapSource>
#pragma warning restore CA1036 // Override methods on comparable types
    {
        public static bool NoHardwareSupport { get; set; } // azure

        private IComObject<IWICBitmapSource> _comObject;
        private WicPalette _palette;
        internal Dictionary<WicMetadataKey, object> _medatata = new Dictionary<WicMetadataKey, object>();

        public WicBitmapSource(object comObject)
        {
            _comObject = new ComObjectWrapper<IWICBitmapSource>(comObject).ComObject;
            //Decoder = WicDecoder.FromContainerFormatGuid(encoderFormat);
        }

        public WicBitmapSource(int width, int height, WicPixelFormat pixelFormat, WICBitmapCreateCacheOption option = WICBitmapCreateCacheOption.WICBitmapCacheOnDemand)
        {
            if (pixelFormat == null)
                throw new ArgumentNullException(nameof(pixelFormat));

            _comObject = WICImagingFactory.CreateBitmap(width, height, pixelFormat.Guid, option);
        }

        public IComObject<IWICBitmapSource> ComObject => _comObject;
        //public WicDecoder Decoder { get; }
        public WicIntSize Size => new WicIntSize(Width, Height);
        public WICRect Bounds => new WICRect(0, 0, Width, Height);
        public IDictionary<WicMetadataKey, object> Metadata => _medatata;

        //public WicPalette Palette
        //{
        //    get
        //    {
        //        if (_palette == null)
        //        {
        //            _palette = new Palette();
        //            _comObject.Object.CopyPalette(palette.BaseObject as IWICPalette);
        //            return palette.ColorCount != 0 ? palette : null;
        //        }
        //        return _palette;
        //    }
        //    set
        //    {
        //        _palette?.Dispose();
        //        _palette = value;
        //    }
        //}

        private static IEnumerable<KeyValuePair<WicMetadataKey, object>> EnumerateMetadata(IEnumerable<KeyValuePair<WicMetadataKey, object>> md)
        {
            foreach (var kv in md)
            {
                if (kv.Value is IEnumerable<KeyValuePair<WicMetadataKey, object>> child)
                {
                    foreach (var childKv in EnumerateMetadata(child))
                    {
                        yield return childKv;
                    }
                }
                else
                {
                    yield return kv;
                }
            }
        }

        public IEnumerable<KeyValuePair<WicMetadataKey, object>> AllMetadata => EnumerateMetadata(Metadata);

        public WicPixelFormat PixelFormat
        {
            get
            {
                _comObject.Object.GetPixelFormat(out var pixelFormat);
                return WicImagingComponent.FromClsid<WicPixelFormat>(pixelFormat);
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

        public IEnumerable<Guid> GetMetadataFormats() => AllMetadata.Select(kv => kv.Key.Format).Distinct();
        public IEnumerable<WicMetadataHandler> GetMetadataHandlers() => GetMetadataFormats().Select(g => WicMetadataHandler.FromFormatGuid<WicMetadataHandler>(g));

        public IEnumerable<KeyValuePair<WicMetadataKey, object>> GetMetadata(string readerFriendlyName)
        {
            if (readerFriendlyName == null)
                throw new ArgumentNullException(nameof(readerFriendlyName));

            var handler = WicMetadataHandler.FromFriendlyName<WicMetadataHandler>(readerFriendlyName);
            if (handler == null)
                return Enumerable.Empty<KeyValuePair<WicMetadataKey, object>>();

            return GetMetadata(handler.Guid);
        }

        public IEnumerable<KeyValuePair<WicMetadataKey, object>> GetMetadata(Guid readerFormat) => AllMetadata.Where(kv => kv.Key.Format == readerFormat);
        public void CopyMetadata(IDictionary<WicMetadataKey, object> target) => WicMetadataKey.CopyMetadata(Metadata, target);
        public void VisitMetadata(Action<KeyValuePair<WicMetadataKey, object>> func) => WicMetadataKey.VisitMetadata(Metadata, func);

        private class MdWithParent
        {
            public IDictionary<WicMetadataKey, object> dic;
            public WicMetadataKey key;
        }

        private static void RemoveMetadata(List<MdWithParent> mds, IEnumerable<KeyValuePair<WicMetadataKey, object>> md, Func<KeyValuePair<WicMetadataKey, object>, bool> func) =>
            WicMetadataKey.VisitMetadata(md, (kv) =>
            {
                if (func(kv))
                {
                    var mdw = new MdWithParent();
                    mdw.dic = (IDictionary<WicMetadataKey, object>)md;
                    mdw.key = kv.Key;
                    mds.Add(mdw);
                }
            });

        public int RemoveMetadata(params WicMetadataKey[] keys)
        {
            if (keys == null || keys.Length == 0)
                return 0;

            return RemoveMetadata(kv => keys.Contains(kv.Key));
        }

        public int RemoveMetadata(string readerFriendlyName)
        {
            if (readerFriendlyName == null)
                throw new ArgumentNullException(nameof(readerFriendlyName));

            var handler = WicMetadataHandler.FromFriendlyName<WicMetadataHandler>(readerFriendlyName);
            if (handler == null)
                return 0;

            return RemoveMetadata(handler.Guid);
        }

        public int RemoveMetadata(Guid readerFormat) => RemoveMetadata(kv => kv.Key.Format == readerFormat);
        public int RemoveMetadata(Func<KeyValuePair<WicMetadataKey, object>, bool> removeFunc)
        {
            var mds = new List<MdWithParent>();
            RemoveMetadata(mds, Metadata, removeFunc);
            foreach (var md in mds)
            {
                md.dic.Remove(md.key);
            }
            return mds.Count;
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

        public void CopyPixels(int stride, int bufferSize, IntPtr buffer) => _comObject.Object.CopyPixels(IntPtr.Zero, stride, bufferSize, buffer).ThrowOnError();
        public void CopyPixels(int left, int top, int width, int height, int stride, int bufferSize, IntPtr buffer)
        {
            var rect = new WICRect();
            rect.X = left;
            rect.Y = top;
            rect.Width = width;
            rect.Height = height;
            using (var mem = new ComMemory(rect))
            {
                _comObject.Object.CopyPixels(mem.Pointer, stride, bufferSize, buffer).ThrowOnError();
            }
        }

        //public WicBitmapSource Clone(WICBitmapCreateCacheOption option = WICBitmapCreateCacheOption.WICBitmapNoCache)
        //{
        //    // cache on load means we clone the memory (or seek underlying streams)
        //    var copy = Wic.Copy(CheckDisposed(), option);
        //    return new WicBitmap(copy, Decoder != null ? Decoder.ContainerFormat : Guid.Empty);
        //}

        //public static WicBitmapSource FromHBitmap(IntPtr hBitmap) => new WicBitmap(Wic.CreateBitmapFromHBITMAP(hBitmap), Guid.Empty);

        public void Scale(int? width, int? height, WICBitmapInterpolationMode mode = WICBitmapInterpolationMode.WICBitmapInterpolationModeNearestNeighbor, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default)
        {
            if (!width.HasValue && !height.HasValue)
                return;

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

        public static WicBitmapSource Load(string filePath, int frameIndex = 0, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand) => WicBitmapDecoder.Load(filePath, options: options).GetFrame(frameIndex);
        public static WicBitmapSource Load(IntPtr fileHandle, int frameIndex = 0, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand) => WicBitmapDecoder.Load(fileHandle, options: options).GetFrame(frameIndex);
        public static WicBitmapSource Load(Stream stream, int frameIndex = 0, WICDecodeOptions options = WICDecodeOptions.WICDecodeMetadataCacheOnDemand) => WicBitmapDecoder.Load(stream, options: options).GetFrame(frameIndex);

        public void WithLock(WICBitmapLockFlags flags, Action<WicBitmapLock> action, WICRect? rect = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using (var lck = CheckBitmap().Lock(flags, rect))
            {
                action(new WicBitmapLock(lck));
            }
        }

        public T WithLock<T>(WICBitmapLockFlags flags, Func<WicBitmapLock, T> func, WICRect? rect = null)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            using (var lck = CheckBitmap().Lock(flags, rect))
            {
                return func(new WicBitmapLock(lck));
            }
        }

        private IWICBitmap CheckBitmap()
        {
            var bmp = _comObject.As<IWICBitmap>();
            if (bmp == null)
                throw new WicNetException("WIC0002: Lock is only supported on in-memory bitmaps. You must Clone this bitmap first.");

            return bmp;
        }

        public void Save(
            string filePath,
            WicPixelFormat pixelFormat = null,
            WICBitmapEncoderCacheOption cacheOptions = WICBitmapEncoderCacheOption.WICBitmapEncoderCacheInMemory,
            IEnumerable<KeyValuePair<string, object>> options = null,
            IEnumerable<KeyValuePair<WicMetadataKey, object>> metadata = null)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            var encoder = WicEncoder.FromFileExtension(filePath);
            if (encoder == null)
                throw new WicNetException("WIC0001: Encoder cannot be determined for file name '" + Path.GetFileName(filePath) + "'.");

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                Save(stream, encoder, pixelFormat, cacheOptions, options, metadata);
            }
        }

        public void Save(
            string filePath,
            WicEncoder encoder,
            WicPixelFormat pixelFormat = null,
            WICBitmapEncoderCacheOption cacheOptions = WICBitmapEncoderCacheOption.WICBitmapEncoderCacheInMemory,
            IEnumerable<KeyValuePair<string, object>> options = null,
            IEnumerable<KeyValuePair<WicMetadataKey, object>> metadata = null)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                Save(file, encoder, pixelFormat, cacheOptions, options, metadata);
            }
        }

        public void Save(Stream stream, IEnumerable<KeyValuePair<string, object>> options) => Save(stream, options: options);
        public void Save(Stream stream, WicPixelFormat pixelFormat) => Save(stream, null, pixelFormat);
        public void Save(
            Stream stream,
            WicEncoder encoder = null,
            WicPixelFormat pixelFormat = null,
            WICBitmapEncoderCacheOption cacheOptions = WICBitmapEncoderCacheOption.WICBitmapEncoderCacheInMemory,
            IEnumerable<KeyValuePair<string, object>> options = null,
            IEnumerable<KeyValuePair<WicMetadataKey, object>> metadata = null,
            WicPalette encoderPalette = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            //if (encoder == null)
            //{
            //    encoder = WicEncoder.FromDecoder(Decoder);
            //    if (encoder == null)
            //        throw new ArgumentNullException(nameof(encoder));
            //}

            //metadata = metadata ?? Metadata;

            //var pfg = pixelFormat == null ? Guid.Empty : pixelFormat.Guid;
            //var ep = encoderPalette?.BaseObject as IWICPalette;

            //IWICPalette palette = null;
            //var p = Palette;
            //if (p != null && p.ColorCount > 0)
            //{
            //    // we must clone it
            //    palette = Wic.CreatePalette();
            //    var cols = p.Colors.Select(c => c.ToArgb()).ToArray();
            //    palette.InitializeCustom(cols, cols.Length);
            //}
            //Wic.Save(CheckDisposed(), stream, encoder.ContainerFormat, pfg, (WICBitmapEncoderCacheOption)cacheOptions, null, metadata, options,
            //    ep,
            //    palette);
        }

        public void Dispose()
        {
            _palette?.Dispose();
            _comObject?.Dispose();
        }

        public WicBitmapSource ConvertTo(Guid pixelFormat, WICBitmapDitherType ditherType = WICBitmapDitherType.WICBitmapDitherTypeNone, WicPalette palette = null, double alphaThresholdPercent = 0, WICBitmapPaletteType paletteTranslate = WICBitmapPaletteType.WICBitmapPaletteTypeCustom)
        {
            if (PixelFormat == null)
                throw new InvalidOperationException();

            var cvt = PixelFormat.GetPixelFormatConvertersTo(pixelFormat).FirstOrDefault();
            if (cvt == null)
                throw new InvalidOperationException();

            return cvt.Convert(this, pixelFormat, ditherType, palette, alphaThresholdPercent, paletteTranslate);
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
