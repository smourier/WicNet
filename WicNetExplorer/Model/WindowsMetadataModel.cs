using System.ComponentModel;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class WindowsMetadataModel : MetadataModel
    {
        public WindowsMetadataModel(WicMetadataQueryReader reader)
            : base(reader)
        {
            Policies = new WicMetadataPolicies(reader);
        }

        [DisplayName("Windows Policies")]
        [TypeConverter(typeof(StringFormatterExpandableConverter))]
        [StringFormatter("{FilledValues}")]
        public WicMetadataPolicies Policies { get; }
    }
}
