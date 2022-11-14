using System.ComponentModel;
using WicNet;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MetadataReaderModel : MetadataHandlerModel
    {
        public MetadataReaderModel(WicMetadataReader handler)
            : base(handler)
        {
        }
    }
}
