using System;

namespace WicNetExplorer.Utilities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class StringFormatterAttribute : Attribute
    {
        public StringFormatterAttribute(string format)
        {
            Format = format;
        }

        public string Format { get; set; }
        public bool ThrowOnError { get; set; }
        public Type? ResourcesType { get; set; }
    }
}
