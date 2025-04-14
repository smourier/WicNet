using System.ComponentModel;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class WindowsMetadataModel(WicMetadataQueryReader reader) : MetadataModel(reader)
{
    [DisplayName("Windows Policies")]
    [TypeConverter(typeof(StringFormatterExpandableConverter))]
    [StringFormatter("{FilledValues}")]
    public WicMetadataPolicies Policies { get; } = new WicMetadataPolicies(reader);
}
