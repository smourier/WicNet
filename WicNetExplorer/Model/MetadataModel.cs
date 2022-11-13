using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MetadataModel
    {
        private readonly WicMetadataQueryReader _reader;

        public MetadataModel(WicMetadataQueryReader reader)
        {
            ArgumentNullException.ThrowIfNull(reader);
            _reader = reader;

            Values = new DynamicObject();
            var children = new List<MetadataModel>();
            var i = 0;
            foreach (var kv in _reader.Enumerate())
            {
                if (kv.Value is WicMetadataQueryReader childReader)
                {
                    var child = new MetadataModel(childReader);
                    children.Add(child);
                }
                else
                {
                    var atts = new List<Attribute>
                    {
                        new DisplayNameAttribute(kv.Key.Key)
                    };

                    if (kv.Value is byte[])
                    {
                        atts.Add(new TypeConverterAttribute(typeof(ByteArrayExpandableConverter)));
                        atts.Add(new EditorAttribute(typeof(ByteArrayEditor), typeof(UITypeEditor)));
                    }
                    else if (kv.Value is Array array && array.Rank == 1)
                    {
                        atts.Add(new TypeConverterAttribute(typeof(ArrayDisplayExpandableConverter)));
                        atts.Add(new EditorAttribute(typeof(ArrayDisplayEditor), typeof(UITypeEditor)));
                    }
                    else
                    {
                        atts.Add(new ReadOnlyAttribute(true));
                    }

                    Values.AddProperty("prop" + i++, new MetadataKeyValueModel(kv), null, atts.ToArray());
                }
            }
            Children = children.ToArray();
        }

        public string Location => _reader.Location;
        [DisplayName("Friendly Name")]
        public string Name => _reader.HandlerFriendlyName;

        [DisplayName("Container Format")]
        public Guid ContainerFormat => _reader.ContainerFormat;

        [DisplayName("Container Format Name")]
        public string ContainerFormatName => _reader.ContainerFormatName;

        public DynamicObject Values { get; }

        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public MetadataModel[] Children { get; }
        public override string ToString() => Name;
    }
}
