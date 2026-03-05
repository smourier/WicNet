using System;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model;

public class EncoderFileExtensionModel : ICollectionFormItem
{
    public EncoderFileExtensionModel(string extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        Extension = extension;
        var codec = WicEncoder.FromFileExtension(extension);
        if (codec != null)
        {
            Encoder = new EncoderModel(codec);
        }
    }

    public string Extension { get; }
    public EncoderModel? Encoder { get; }

    string? ICollectionFormItem.TypeName => Extension;
    string ICollectionFormItem.Name => Encoder?.ToString() ?? Extension;
    object? ICollectionFormItem.Value => Encoder;

    public override string ToString() => Encoder?.ToString() ?? Extension;
}
