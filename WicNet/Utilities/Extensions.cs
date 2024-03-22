using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DirectN;

namespace WicNet.Utilities
{
    public static class Extensions
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Guid, string>> _guidsNames = new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, string>>();

        public static int GET_X_LPARAM(this IntPtr lParam) => LOWORD(lParam.ToInt32());
        public static int GET_Y_LPARAM(this IntPtr lParam) => HIWORD(lParam.ToInt32());
        public static int HIWORD(int i) => (short)(i >> 16);
        public static int LOWORD(int i) => (short)(i & 65535);

        public static _D3DCOLORVALUE ToD3DCOLORVALUE(this Color color) => _D3DCOLORVALUE.FromArgb(color.A, color.R, color.G, color.B);

        public static IComObject<ID2D1BitmapBrush> CreateCheckerboardBrush(this IComObject<ID2D1RenderTarget> renderTarget, float size) => CreateCheckerboardBrush(renderTarget?.Object, size);
        public static IComObject<ID2D1BitmapBrush> CreateCheckerboardBrush(this ID2D1RenderTarget renderTarget, float size, _D3DCOLORVALUE? color = null)
        {
            if (renderTarget == null)
                throw new ArgumentNullException(nameof(renderTarget));

            color = color ?? new _D3DCOLORVALUE(0xFFBFBFBF);
            IComObject<ID2D1Bitmap> bmp;
            using (var rt = renderTarget.CreateCompatibleRenderTarget(new D2D_SIZE_F(size * 2, size * 2)))
            {
                using (var colorBrush = rt.CreateSolidColorBrush(color.Value))
                {
                    rt.BeginDraw();
                    rt.FillRectangle(D2D_RECT_F.Sized(0, 0, size, size), colorBrush);
                    rt.FillRectangle(D2D_RECT_F.Sized(size, size, size, size), colorBrush);
                    rt.EndDraw();
                    bmp = rt.GetBitmap();
                }
            }

            renderTarget.CreateBitmapBrush(bmp.Object, IntPtr.Zero, IntPtr.Zero, out var brush).ThrowOnError();
            bmp.Dispose();
            brush.SetExtendModeX(D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_WRAP);
            brush.SetExtendModeY(D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_WRAP);
            return new ComObject<ID2D1BitmapBrush>(brush);
        }

        public static void EncodeMetadata(this IComObject<IWICMetadataQueryWriter> writer, IEnumerable<WicMetadataKeyValue> metadata) => EncodeMetadata(writer?.Object, metadata);
        public static void EncodeMetadata(this IWICMetadataQueryWriter writer, IEnumerable<WicMetadataKeyValue> metadata)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (metadata == null)
                return;

            if (!metadata.Any())
                return;

            WICImagingFactory.WithFactory(factory =>
            {
                foreach (var kv in metadata)
                {
                    if (kv.Value is IEnumerable<WicMetadataKeyValue> childMetadata)
                    {
                        if (!childMetadata.Any())
                            continue;

                        factory.CreateQueryWriter(kv.Key.Format, IntPtr.Zero, out var childWriter).ThrowOnError();
                        using (var pv = new PropVariant(childWriter))
                        {
                            var hr = writer.SetMetadataByName(kv.Key.Key, pv.Detached).ThrowOnError();
                        }
                        EncodeMetadata(childWriter, childMetadata);
                    }
                    else
                    {
                        using (var pv = new PropVariant(kv.Value, kv.Type))
                        {
                            var hr = writer.SetMetadataByName(kv.Key.Key, pv.Detached).ThrowOnError();
                        }
                    }
                }
            });
        }

        public static D2D_SIZE_F GetScaleFactor(this WicIntSize size, uint? width = null, uint? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default) => new D2D_SIZE_F(size.Width, size.Height).GetScaleFactor(width, height, options);
        public static D2D_SIZE_F GetScaleFactor(this WicIntSize size, int? width = null, int? height = null, WicBitmapScaleOptions options = WicBitmapScaleOptions.Default) => new D2D_SIZE_F(size.Width, size.Height).GetScaleFactor(width, height, options);
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

        public static int GetStride(int width, int bitsPerPixel)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (bitsPerPixel <= 0)
                throw new ArgumentOutOfRangeException(nameof(bitsPerPixel));

            return (width * bitsPerPixel + 31) / 32 * 4;
        }

        public static bool EqualsIgnoreCase(this string str, string text, bool trim = false)
        {
            if (trim)
            {
                str = str.Nullify();
                text = text.Nullify();
            }

            if (str == null)
                return text == null;

            if (text == null)
                return false;

            if (str.Length != text.Length)
                return false;

            return string.Compare(str, text, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static string Nullify(this string str)
        {
            if (str == null)
                return null;

            if (string.IsNullOrWhiteSpace(str))
                return null;

            var t = str.Trim();
            return t.Length == 0 ? null : t;
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

        public static string GetString(Func<string, int, int> action)
        {
            var len = action(null, 0);
            if (len <= 0)
                return null;

            var str = new string('\0', len);
            action(str, len);
            return str.Substring(0, len - 1);
        }

        public static string GetString(Func<string, uint, uint> action)
        {
            var len = action(null, 0);
            if (len <= 0)
                return null;

            var str = new string('\0', (int)len);
            action(str, len);
            return str.Substring(0, (int)len - 1);
        }
    }
}
