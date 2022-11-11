using System;

namespace WicNetExplorer.Utilities
{
    public static class Extensions
    {
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

            prefix ??= type.Name;
            var name = value.ToString()!;
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
