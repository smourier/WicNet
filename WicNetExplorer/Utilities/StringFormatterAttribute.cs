using System;

namespace WicNetExplorer.Utilities;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class StringFormatterAttribute(string format) : Attribute
{
    public string Format { get; set; } = format;
    public bool ThrowOnError { get; set; }
    public Type? ResourcesType { get; set; }
}
