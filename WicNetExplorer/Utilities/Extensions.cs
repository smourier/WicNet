using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using DirectN;
using DirectN.Extensions.Com;
using WicNet;
using WinRT;

namespace WicNetExplorer.Utilities;

public static class Extensions
{
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
        Process.Start("explorer.exe", "/e,/root,/select," + directoryPath);
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

    // this is to replace WinRT's As<T> on C#/WinRT object which doesn't work well under AOT once published in release...
    // throws "Target type is not a projected type: DirectN.ICompositorInterop" from WinRT.TypeExtensions.GetHelperType(Type)
    [return: NotNullIfNotNull(nameof(winRTObject))]
    public static IComObject<T>? AsComObject<T>(this object? winRTObject, CreateObjectFlags flags = CreateObjectFlags.UniqueInstance)
    {
        if (winRTObject == null)
            return null;

        var ptr = MarshalInspectable<object>.FromManaged(winRTObject);
        var obj = ComObject.FromPointer<T>(ptr, flags);
        return obj ?? throw new InvalidCastException($"Object of type '{winRTObject.GetType().FullName}' is not of type '{typeof(T).FullName}'.");
    }

    public static D3DCOLORVALUE FromColor(this Windows.UI.Color color) => D3DCOLORVALUE.FromArgb(color.A, color.R, color.G, color.B);
    public static Windows.UI.Color ToColor(this D3DCOLORVALUE color) => Windows.UI.Color.FromArgb(color.BA, color.BR, color.BG, color.BB);

    public static D3DCOLORVALUE FromColor(this System.Drawing.Color color) => D3DCOLORVALUE.FromArgb(color.A, color.R, color.G, color.B);
    public static System.Drawing.Color ToGdiColor(this D3DCOLORVALUE color) => System.Drawing.Color.FromArgb(color.BA, color.BR, color.BG, color.BB);

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

    public static Size ToGdiSize(this D2D_SIZE_U size) => new((int)size.width, (int)size.height);
    //public static SizeF ToSizeF(this D2D_SIZE_U size) => new(size.width, size.height);
    //public static Size ToSize(this WicSize size) => new((int)size.Width, (int)size.Height);
    //public static SizeF ToSizeF(this WicSize size) => new((float)size.Width, (float)size.Height);

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
            var ss = name[prefix.Length..];
            if (ss.Length > 1 && ss[0] == '_')
                return ss[1..];

            return ss;
        }

        const string tok = "Type";
        if (prefix.Length > tok.Length && prefix.EndsWith(tok))
        {
            prefix = prefix[..^tok.Length];
            if (name.StartsWith(prefix))
            {
                var ss = name[prefix.Length..];
                if (ss.Length > 1 && ss[0] == '_')
                    return ss[1..];
            }
        }
        return name;
    }
}
