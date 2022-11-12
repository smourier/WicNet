using System.ComponentModel;
using WicNet;

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
        public WicMetadataPolicies Policies { get; }
    }
}
