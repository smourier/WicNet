using System.ComponentModel;
using WicNet;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MetadataWriterModel : MetadataHandlerModel
    {
        public MetadataWriterModel(WicMetadataWriter handler)
            : base(handler)
        {
        }
    }
}
