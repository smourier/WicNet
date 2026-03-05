using System;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model;

public class DecoderFileExtensionModel : ICollectionFormItem
{
    public DecoderFileExtensionModel(string extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        Extension = extension;
        var codec = WicDecoder.FromFileExtension(extension);
        if (codec != null)
        {
            Decoder = new DecoderModel(codec);
        }
    }

    public string Extension { get; }
    public DecoderModel? Decoder { get; }

    string? ICollectionFormItem.TypeName => Extension;
    string ICollectionFormItem.Name => Decoder?.ToString() ?? Extension;
    object? ICollectionFormItem.Value => Decoder;

    public override string ToString() => Decoder?.ToString() ?? Extension;
}
