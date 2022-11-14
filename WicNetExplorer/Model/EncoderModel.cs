using System.ComponentModel;
using WicNet;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class EncoderModel : CodecModel
    {
        public EncoderModel(WicEncoder codec)
            : base(codec)
        {
        }
    }
}
