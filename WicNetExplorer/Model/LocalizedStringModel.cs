using System;
using System.Collections.Generic;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model;

public class LocalizedStringModel : ICollectionFormItem
{
    public LocalizedStringModel(string languageCode, IEnumerable<string> strings)
    {
        ArgumentNullException.ThrowIfNull(languageCode);
        ArgumentNullException.ThrowIfNull(strings);
        LanguageCode = languageCode;
        Strings = [.. strings];
    }

    public string LanguageCode { get; }
    public string[] Strings { get; }

    string? ICollectionFormItem.TypeName => null;
    string ICollectionFormItem.Name => LanguageCode;
    object ICollectionFormItem.Value => Strings;

    public override string ToString() => LanguageCode;
}
