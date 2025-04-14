using System.ComponentModel;
using WicNet;

namespace WicNetExplorer.Model;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class MetadataWriterModel(WicMetadataWriter handler) : MetadataHandlerModel(handler)
{
}
