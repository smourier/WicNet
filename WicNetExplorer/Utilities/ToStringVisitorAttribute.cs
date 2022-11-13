using System;

namespace WicNetExplorer.Utilities
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class ToStringVisitorAttribute : Attribute
    {
        public bool ForceIsValue { get; set; }
        public bool Ignore { get; set; }
    }
}
