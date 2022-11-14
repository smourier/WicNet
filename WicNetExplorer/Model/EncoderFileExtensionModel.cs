using System;
using System.Text;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    public class EncoderFileExtensionModel : ICollectionFormItem
    {
        public EncoderFileExtensionModel(string extension)
        {
            ArgumentNullException.ThrowIfNull(extension);
            Extension = extension;
            Encoder = new EncoderModel(WicEncoder.FromFileExtension(extension));
        }

        public string Extension { get; }
        public EncoderModel Encoder { get; }

        string ICollectionFormItem.TypeName => Extension;
        string ICollectionFormItem.Name => Encoder.ToString();
        object ICollectionFormItem.Value => Encoder;

        public override string ToString() => Encoder.ToString();
    }
}
