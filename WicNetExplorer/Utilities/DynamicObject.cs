using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace WicNetExplorer.Utilities;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class DynamicObject : CustomTypeDescriptor
{
    private readonly List<PropertyDescriptor> _descriptors = [];

    public virtual void AddProperty(string name, object? value, Type? type = null, params Attribute[] attributes)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (type == null)
        {
            if (value == null)
            {
                type = typeof(string);
            }
            else
            {
                type = value.GetType();
            }
        }

        var desc = new SimplePropertyDescriptor(name, type, attributes)
        {
            Value = value
        };
        _descriptors.Add(desc);
    }

    public override string ToString() => _descriptors.Count.ToString();

    public override PropertyDescriptorCollection GetProperties() => GetProperties(null);
    public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes) => new([.. _descriptors]);

    private class SimplePropertyDescriptor(string name, Type type, Attribute[]? attrs = null) : PropertyDescriptor(name, attrs)
    {
        public object? Value { get; set; }
        public override Type PropertyType { get; } = type;
        public override Type ComponentType => typeof(DynamicObject);
        public override bool IsReadOnly
        {
            get
            {
                var att = Attributes.OfType<ReadOnlyAttribute>().FirstOrDefault();
                if (att == null)
                    return false;

                return att.IsReadOnly;
            }
        }

        public override string ToString() => Name + " (" + PropertyType.Name + "): " + Value;
        public override bool ShouldSerializeValue(object component) => false;
        public override bool CanResetValue(object component) => false;
        public override object? GetValue(object? component) => Value;
        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object? component, object? value)
        {
            Value = value;
        }
    }
}
