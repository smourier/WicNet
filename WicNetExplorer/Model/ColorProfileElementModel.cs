using System;
using System.ComponentModel;
using System.Drawing.Design;
using DirectN;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class ColorProfileElementModel
{
    private readonly ColorProfileElement _element;

    public ColorProfileElementModel(ColorProfileElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        _element = element;
    }

    public int Tag => _element.Tag;

    [DisplayName("Tag (Hexa)")]
    public string TagHexa => Conversions.ToHexa(BitConverter.GetBytes(Tag));

    [DisplayName("Tag (Text)")]
    public string TagString => _element.TagString;
    public string Type => _element.Type;

    [Editor(typeof(ByteArrayEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(ByteArrayConverter))]
    public byte[] Bytes { get => _element.Bytes; set => throw new NotSupportedException(); } // set for property grid support

    public override string ToString() => _element.ToString();
}
