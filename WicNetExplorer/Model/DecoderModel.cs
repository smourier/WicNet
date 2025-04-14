using System.ComponentModel;
using WicNet;

namespace WicNetExplorer.Model;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class DecoderModel(WicDecoder codec) : CodecModel(codec)
{
}
