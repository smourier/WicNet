using System;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    public class DecoderFileExtensionModel : ICollectionFormItem
    {
        public DecoderFileExtensionModel(string extension)
        {
            ArgumentNullException.ThrowIfNull(extension);
            Extension = extension;
            Decoder = new DecoderModel(WicDecoder.FromFileExtension(extension));
        }

        public string Extension { get; }
        public DecoderModel Decoder { get; }

        string ICollectionFormItem.TypeName => Extension;
        string ICollectionFormItem.Name => Decoder.ToString();
        object ICollectionFormItem.Value => Decoder;

        public override string ToString() => Decoder.ToString();
    }
}
