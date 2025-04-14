using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace WicNetExplorer.Utilities;

public static class StringFormatter
{
    [return: NotNullIfNotNull(nameof(format))]
    public static string? FormatWith(this string? format, object container, bool throwOnError = true, IFormatProvider? provider = null)
    {
        if (string.IsNullOrWhiteSpace(format))
            return format;

        var result = new StringBuilder(format.Length * 2);
        using (var reader = new StringReader(format))
        {
            var expression = new StringBuilder();
            var state = State.OutsideExpression;
            do
            {
                int c;
                switch (state)
                {
                    case State.OutsideExpression:
                        c = reader.Read();
                        switch (c)
                        {
                            case -1:
                                state = State.End;
                                break;

                            case '{':
                                state = State.OnOpenBracket;
                                break;

                            case '}':
                                state = State.OnCloseBracket;
                                break;

                            default:
                                result.Append((char)c);
                                break;
                        }
                        break;

                    case State.OnOpenBracket:
                        c = reader.Read();
                        switch (c)
                        {
                            case '{':
                                result.Append('{');
                                state = State.OutsideExpression;
                                break;

                            default:
                                if (c >= 0)
                                {
                                    expression.Append((char)c);
                                }
                                state = State.InsideExpression;
                                break;
                        }
                        break;

                    case State.InsideExpression:
                        c = reader.Read();
                        switch (c)
                        {
                            case -1:
                            case '}':
                                result.Append(OutExpression(container, expression.ToString(), throwOnError, provider));
                                expression.Length = 0;
                                state = State.OutsideExpression;
                                break;

                            default:
                                expression.Append((char)c);
                                break;
                        }
                        break;

                    //case State.OnCloseBracket:
                    default:
                        c = reader.Read();
                        switch (c)
                        {
                            case '}':
                                result.Append('}');
                                state = State.OutsideExpression;
                                break;

                            default:
                                result.Append('}');
                                state = State.OutsideExpression;
                                if (c >= 0)
                                {
                                    result.Append((char)c);
                                }
                                break;
                        }
                        break;
                }
            }
            while (state != State.End);
        }
        return result.ToString();
    }

    [return: NotNullIfNotNull(nameof(format))]
    private static string NormalizeEvalFormat(string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return "{0}";

        if (format.Contains("{0"))
            return format;

        return "{0:" + format + "}";
    }

    private static string? OutExpression(object container, string expression, bool throwOnError, IFormatProvider? provider)
    {
        string? format = null;
        var pos = expression.IndexOf(':');
        if (pos > 0)
        {
            format = expression[(pos + 1)..];
            expression = expression[..pos];
        }

        if (string.IsNullOrWhiteSpace(expression) || expression == "}")
            return null;

        var obj = container != null ? DataBindingEvaluator.Eval(container, expression, throwOnError) : null;
        format = NormalizeEvalFormat(format);
        return string.Format(provider, format, obj);
    }

    private enum State
    {
        OutsideExpression,
        OnOpenBracket,
        InsideExpression,
        OnCloseBracket,
        End,
    }
}
