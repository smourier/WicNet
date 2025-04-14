using System.ComponentModel;
using WicNet;

namespace WicNetExplorer.Model;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class EncoderModel(WicEncoder codec) : CodecModel(codec)
{
}
