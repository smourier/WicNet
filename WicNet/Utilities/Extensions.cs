using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace WicNet.Utilities
{
    public static class Extensions
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Guid, string>> _guidsNames = new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, string>>();

        public static string GetGuidName(this Type type, Guid guid)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (TryGetGuidName(type, guid, out var name))
                return name;

            return guid.ToString();
        }

        public static bool TryGetGuidName(this Type type, Guid guid, out string name)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!_guidsNames.TryGetValue(type, out var dic))
            {
                dic = new ConcurrentDictionary<Guid, string>();
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    dic[(Guid)field.GetValue(null)] = field.Name;
                }
                _guidsNames[type] = dic;
            }

            if (dic.TryGetValue(guid, out name))
                return true;

            name = null;
            return false;
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
    }
}
