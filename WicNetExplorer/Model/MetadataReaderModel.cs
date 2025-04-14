using System.ComponentModel;
using WicNet;

namespace WicNetExplorer.Model;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class MetadataReaderModel(WicMetadataReader handler) : MetadataHandlerModel(handler)
{
}
