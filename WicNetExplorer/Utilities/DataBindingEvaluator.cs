using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace WicNetExplorer.Utilities;

public static class DataBindingEvaluator
{
    private static readonly char[] _expressionPartSeparator = ['.'];
    private static readonly char[] _indexExprEndChars = [']', ')'];
    private static readonly char[] _indexExprStartChars = ['[', '('];

    public static string? Eval(object container, string expression, string format, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(expression);

        if (provider == null)
            return string.Format(format, Eval(container, expression));

        return string.Format(provider, format, Eval(container, expression));
    }

    public static object? Eval(object container, string expression, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(expression);

        var expressionParts = expression.Split(_expressionPartSeparator);
        return Eval(container, expressionParts, throwOnError);
    }

    public static object? GetPropertyValue(object container, string propertyName, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(propertyName);

        var props = TypeDescriptor.GetProperties(container);
        var descriptor = props?.Find(propertyName, true);
        if (descriptor == null)
        {
            if (throwOnError)
                throw new ArgumentException("DataBindingEvaluator: '" + container.GetType().FullName + "' does not contain a property with the name '" + propertyName + "'.", nameof(propertyName));

            return null;
        }
        return descriptor.GetValue(container);
    }

    public static object? GetIndexedPropertyValue(object container, string expression, bool throwOnError = true)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(expression);

        var isIndex = false;
        var startPos = expression.IndexOfAny(_indexExprStartChars);
        var endPos = expression.IndexOfAny(_indexExprEndChars, startPos + 1);
        if (startPos < 0 || endPos < 0 || endPos == (startPos + 1))
        {
            if (throwOnError)
                throw new ArgumentException("DataBindingEvaluator: '" + expression + "' is not a valid indexed expression.", nameof(expression));

            return null;
        }

        string? propName = null;
        object? index = null;
        var str = expression.Substring(startPos + 1, (endPos - startPos) - 1).Trim();
        if (startPos != 0)
        {
            propName = expression[..startPos];
        }

        if (str.Length != 0)
        {
            if ((str[0] == '"' && str[^1] == '"') || (str[0] == '\'' && str[^1] == '\''))
            {
                index = str[1..^1];
            }
            else if (char.IsDigit(str[0]))
            {
                isIndex = int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number);
                if (isIndex)
                {
                    index = number;
                }
                else
                {
                    index = str;
                }
            }
            else
            {
                index = str;

            }
        }

        if (index == null)
        {
            if (throwOnError)
                throw new ArgumentException("DataBindingEvaluator: '" + expression + "' is not a valid indexed expression.", nameof(expression));

            return null;
        }

        object? propertyValue;
        if (propName != null && propName.Length != 0)
        {
            propertyValue = GetPropertyValue(container, propName, throwOnError);
        }
        else
        {
            propertyValue = container;
        }

        if (propertyValue == null)
            return null;

        if (isIndex && propertyValue is Array array && array.Rank == 1)
        {
            var idx = (int)index;
            if ((idx < 0 || idx > (array.Length - 1)) && !throwOnError)
                return null;

            return array.GetValue(idx);
        }

        if (isIndex && propertyValue is IList list1)
        {
            var idx = (int)index;
            if ((idx < 0 || idx > (list1.Count - 1)) && !throwOnError)
                return null;

            return list1[idx];
        }

        if (propertyValue is IList list2 && isIndex)
        {
            var idx = (int)index;
            if ((idx < 0 || idx > (list2.Count - 1)) && !throwOnError)
                return null;

            return list2[idx];
        }

        var item = propertyValue.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, null, [index.GetType()], null);
        if (item == null)
        {
            if (throwOnError)
                throw new ArgumentException("DataBindingEvaluator: '" + propertyValue.GetType().FullName + "' does not allow indexed access.", nameof(container));

            return null;
        }

        return item.GetValue(propertyValue, [index]);
    }

    private static object? Eval(object container, string[] expressionParts, bool throwOnError)
    {
        var propertyValue = container;
        for (var i = 0; i < expressionParts.Length && propertyValue != null; i++)
        {
            var propName = expressionParts[i];
            if (propName.IndexOfAny(_indexExprStartChars) < 0)
            {
                propertyValue = GetPropertyValue(propertyValue, propName, throwOnError);
            }
            else
            {
                propertyValue = GetIndexedPropertyValue(propertyValue, propName, throwOnError);
            }
        }
        return propertyValue;
    }
}
