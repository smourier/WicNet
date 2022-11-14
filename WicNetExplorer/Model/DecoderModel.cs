using System.ComponentModel;
using WicNet;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DecoderModel : CodecModel
    {
        public DecoderModel(WicDecoder codec)
            : base(codec)
        {
        }
    }
}
