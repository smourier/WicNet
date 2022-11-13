using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using DirectN;
using WicNet;

namespace WicNetExplorer.Utilities
{
    public static class Extensions
    {
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

        public static WicColorContext? GetBestColorContext(this WicBitmapSource source)
        {
            if (source == null)
                return null;

            var contexts = source.GetColorContexts();
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
            contexts.Dispose(new[] { best });
            return best;
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
                return name.Substring(prefix.Length);

            const string tok = "Type";
            if (prefix.Length > tok.Length && prefix.EndsWith(tok))
            {
                prefix = prefix.Substring(0, prefix.Length - tok.Length);
                if (name.StartsWith(prefix))
                    return name.Substring(prefix.Length);
            }
            return name;
        }
    }
}
