using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using DirectN;
using WicNet;

namespace WicNetExplorer.Utilities
{
    public static class Extensions
    {
        public static object D3D11CreateDevice()
        {
            var flags = D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT;
            var hr = D3D11CreateDevice(
                null,
                 D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
                IntPtr.Zero,
                flags,
                null,
                0,
                Constants.D3D11_SDK_VERSION,
                out var device,
                out var _,
                IntPtr.Zero);

            if (hr.IsSuccess)
                return device;

            var hr2 = D3D11CreateDevice(
                null,
                 D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_WARP,
                IntPtr.Zero,
                flags,
                null,
                0,
                Constants.D3D11_SDK_VERSION,
                out device,
                out var _,
                IntPtr.Zero);
            if (hr2.IsSuccess)
                return device;

            hr.ThrowOnError(true);
            return device;
        }

        public static IPdfRendererNative? PdfCreateRenderer(IDXGIDevice device, bool throwOnError = true)
        {
            ArgumentNullException.ThrowIfNull(device);
            PdfCreateRenderer(device, out var renderer).ThrowOnError(throwOnError);
            return renderer;
        }

        [DllImport("d3d11", ExactSpelling = true)]
        private static extern HRESULT D3D11CreateDevice(/*IDXGIAdapter*/ [MarshalAs(UnmanagedType.IUnknown)] object? pAdapter, D3D_DRIVER_TYPE DriverType, IntPtr Software, D3D11_CREATE_DEVICE_FLAG Flags, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] D3D_FEATURE_LEVEL[]? pFeatureLevels, uint FeatureLevels, uint SDKVersion, /*ID3D11Device*/ [MarshalAs(UnmanagedType.IUnknown)] out object ppDevice, out D3D_FEATURE_LEVEL pFeatureLevel, /*out ID3D11DeviceContext*/ IntPtr ppImmediateContext);

        [DllImport("windows.data.pdf", ExactSpelling = true)]
        private static extern HRESULT PdfCreateRenderer(IDXGIDevice pDevice, out IPdfRendererNative ppRenderer);

        public static void OpenUrl(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);
            OpenUrl(uri.ToString());
        }

        public static void OpenUrl(string url)
        {
            ArgumentNullException.ThrowIfNull(url);
            Process.Start(url);
        }

        public static void OpenExplorer(string directoryPath)
        {
            if (directoryPath == null)
                return;

            if (!IOUtilities.PathIsDirectory(directoryPath))
                return;

            // see http://support.microsoft.com/kb/152457/en-us
#pragma warning disable S4036
            Process.Start("explorer.exe", "/e,/root,/select," + directoryPath);
#pragma warning restore S4036
        }

        public static bool IsSvg(string fileName)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            var ext = Path.GetExtension(fileName);
            return ext.EqualsIgnoreCase(".svg") || ext.EqualsIgnoreCase(".svgz");
        }

        [SupportedOSPlatformGuard("windows10.0.10240.0")]
        public static bool IsPdfDocumentSupported() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240) && !Settings.Current.ForceW7;
        public static bool IsPdf(string fileName)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            var ext = Path.GetExtension(fileName);
            return ext.EqualsIgnoreCase(".pdf");
        }

        public static _D3DCOLORVALUE FromColor(this Windows.UI.Color color) => _D3DCOLORVALUE.FromArgb(color.A, color.R, color.G, color.B);
        public static Windows.UI.Color ToColor(this _D3DCOLORVALUE color) => Windows.UI.Color.FromArgb(color.BA, color.BR, color.BG, color.BB);

        public static PHOTO_ORIENTATION? GetOrientation(this WicBitmapSource source)
        {
            if (source == null)
                return null;

            using var reader = source.GetMetadataReader();
            if (reader == null)
                return null;

            var orientation = new WicMetadataPolicies(reader).PhotoOrientation;
            if (!orientation.HasValue)
                return null;

            return (PHOTO_ORIENTATION)orientation.Value;
        }

        public static Size ToSize(this WicIntSize size) => new((int)size.Width, (int)size.Height);
        public static SizeF ToSizeF(this WicIntSize size) => new(size.Width, size.Height);
        public static Size ToSize(this WicSize size) => new((int)size.Width, (int)size.Height);
        public static SizeF ToSizeF(this WicSize size) => new((float)size.Width, (float)size.Height);

        public static bool EqualsIgnoreCase(this string? thisString, string? text, bool trim = false)
        {
            if (trim)
            {
                thisString = thisString.Nullify();
                text = text.Nullify();
            }

            if (thisString == null)
                return text == null;

            if (text == null)
                return false;

            if (thisString.Length != text.Length)
                return false;

            return string.Compare(thisString, text, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static string? Nullify(this string? text)
        {
            if (text == null)
                return null;

            if (string.IsNullOrWhiteSpace(text))
                return null;

            var t = text.Trim();
            return t.Length == 0 ? null : t;
        }

        public static string GetEnumName(this object value, string? prefix = null)
        {
            ArgumentNullException.ThrowIfNull(value);
            var type = value.GetType();
            if (!type.IsEnum)
                throw new ArgumentException(null, nameof(value));

            var name = value.ToString()!;
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            if (field != null)
            {
                var desc = field.GetCustomAttribute<DescriptionAttribute>();
                if (desc != null)
                    return desc.Description;
            }

            prefix ??= type.Name;
            if (name.StartsWith(prefix))
            {
                var ss = name.Substring(prefix.Length);
                if (ss.Length > 1 && ss[0] == '_')
                    return ss.Substring(1);

                return ss;
            }

            const string tok = "Type";
            if (prefix.Length > tok.Length && prefix.EndsWith(tok))
            {
                prefix = prefix.Substring(0, prefix.Length - tok.Length);
                if (name.StartsWith(prefix))
                {
                    var ss = name.Substring(prefix.Length);
                    if (ss.Length > 1 && ss[0] == '_')
                        return ss.Substring(1);
                }
            }
            return name;
        }
    }
}
