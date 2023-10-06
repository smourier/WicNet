using System;
using System.Collections.Generic;
using DirectN;

namespace WicNetExplorer.Utilities
{
    public static class CommandLine
    {
        private static readonly Dictionary<string, string?> _namedArguments;
        private static readonly Dictionary<int, string?> _positionArguments;

        static CommandLine()
        {
            _namedArguments = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            _positionArguments = new Dictionary<int, string?>();

            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (i == 0)
                    continue;

                var arg = args[i].Nullify();
                if (arg == null)
                    continue;

                if (string.Equals(arg, "/?", StringComparison.Ordinal) ||
                    string.Equals(arg, "-?", StringComparison.Ordinal) ||
                    string.Equals(arg, "/HELP", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "-HELP", StringComparison.OrdinalIgnoreCase))
                {
                    HelpRequested = true;
                }

                var named = false;
                if (arg[0] == '-' || arg[0] == '/')
                {
                    arg = arg.Substring(1);
                    named = true;
                }

                string name;
                string? value;
                int pos = arg.IndexOf(':');
                if (pos < 0)
                {
                    name = arg;
                    value = null;
                }
                else
                {
                    name = arg.Substring(0, pos).Trim();
                    value = arg.Substring(pos + 1).Trim();
                }

                _positionArguments[i - 1] = arg;
                if (named)
                {
                    _namedArguments[name] = value;
                }
            }
        }

        public static IReadOnlyDictionary<string, string?> NamedArguments => _namedArguments;
        public static IReadOnlyDictionary<int, string?> PositionArguments => _positionArguments;
        public static bool HelpRequested { get; }

        public static string CommandLineWithoutExe
        {
            get
            {
                var line = Environment.CommandLine;
                var inParens = false;
                for (var i = 0; i < line.Length; i++)
                {
                    if (line[i] == ' ' && !inParens)
                        return line.Substring(i + 1).TrimStart();

                    if (line[i] == '"')
                    {
                        inParens = !inParens;
                    }
                }
                return line;
            }
        }

        public static T? GetArgument<T>(IEnumerable<string> arguments, string name, T? defaultValue = default)
        {
            ArgumentNullException.ThrowIfNull(name);
            if (arguments == null)
                return defaultValue;

            foreach (var arg in arguments)
            {
                if (arg.StartsWith("-", StringComparison.Ordinal) || arg.StartsWith("/", StringComparison.Ordinal))
                {
                    var pos = arg.IndexOfAny(new[] { '=', ':' }, 1);
                    var argName = pos < 0 ? arg.Substring(1) : arg.Substring(1, pos - 1);
                    if (string.Compare(name, argName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var value = pos < 0 ? string.Empty : arg.Substring(pos + 1).Trim();
                        if (value.Length == 0)
                        {
                            if (typeof(T) == typeof(bool)) // special case for bool args: if it's there, return true
                                return (T)(object)true;

                            return defaultValue;
                        }
                        return Conversions.ChangeType(value, defaultValue);
                    }
                }
            }
            return defaultValue;
        }

        public static string? GetNullifiedArgument(string name, string? defaultValue = null)
        {
            ArgumentNullException.ThrowIfNull(name);
            if (!_namedArguments.TryGetValue(name, out var s))
                return defaultValue.Nullify();

            return s.Nullify();
        }

        public static string? GetNullifiedArgument(int index, string? defaultValue = null)
        {
            if (!_positionArguments.TryGetValue(index, out var s))
                return defaultValue.Nullify();

            return s.Nullify();
        }

        public static T? GetArgument<T>(int index, T? defaultValue = default, IFormatProvider? provider = null)
        {
            if (!_positionArguments.TryGetValue(index, out var s))
                return defaultValue;

            return Conversions.ChangeType(s, defaultValue, provider);
        }

        public static object? GetArgument(int index, object? defaultValue, Type conversionType, IFormatProvider? provider = null)
        {
            if (!_positionArguments.TryGetValue(index, out var s))
                return defaultValue;

            return Conversions.ChangeType(s, conversionType, defaultValue, provider);
        }

        public static T? GetArgument<T>(string name, T? defaultValue = default, IFormatProvider? provider = null)
        {
            ArgumentNullException.ThrowIfNull(name);
            if (!_namedArguments.TryGetValue(name, out var s))
                return defaultValue;

            if (typeof(T) == typeof(bool) && string.IsNullOrEmpty(s))
                return (T)(object)true;

            return Conversions.ChangeType(s, defaultValue, provider);
        }

        public static bool HasArgument(string name)
        {
            ArgumentNullException.ThrowIfNull(name);
            return _namedArguments.TryGetValue(name, out _);
        }

        public static object? GetArgument(string name, object defaultValue, Type conversionType, IFormatProvider? provider = null)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(conversionType);

            if (!_namedArguments.TryGetValue(name, out var s))
                return defaultValue;

            if (conversionType == typeof(bool) && string.IsNullOrEmpty(s))
                return true;

            return Conversions.ChangeType(s, conversionType, defaultValue, provider);
        }
    }
}
