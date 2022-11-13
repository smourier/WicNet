using System;
using System.ComponentModel;
using System.Linq;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PaletteModel
    {
        public PaletteModel(WicPalette palette)
        {
            ArgumentNullException.ThrowIfNull(palette);
            HasAlpha = palette.HasAlpha;
            IsBlackWhite = palette.IsBlackWhite;
            IsGrayscale= palette.IsGrayscale;
            Type = palette.Type.GetEnumName().Decamelize();
            Colors = palette.Colors.ToArray();
            if (ColorCount == 0)
            {
                ColorCount = Colors.Length; // 0 => 256
            }
        }

        [DisplayName("Has Alpha")]
        public bool HasAlpha { get; }

        [DisplayName("Is Black and White")]
        public bool IsBlackWhite { get; }

        [DisplayName("Is Graycale")]
        public bool IsGrayscale { get; }

        [DisplayName("Color Count")]
        public int ColorCount { get; }

        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public WicColor[] Colors { get; }

        public string Type { get; }
        
        public override string ToString() => Type;
    }
}
