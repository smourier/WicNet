using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using DirectN;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ColorProfileModel
    {
        private readonly ColorProfile _profile;

        public ColorProfileModel(ColorProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);
            _profile = profile;

            var list = new List<LocalizedStringModel>();
            foreach (var kv in _profile.LocalizedStrings)
            {
                list.Add(new LocalizedStringModel(kv.Key, kv.Value));
            }
            LocalizedStrings = list.ToArray();
        }

        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public ColorProfileElementModel[] Elements => _profile.Elements.Select(e => new ColorProfileElementModel(e)).OrderBy(e => e.Tag).ToArray();

        public int Size => _profile.Size;
        public string Version => _profile.VersionMajor + "." + _profile.VersionMinor;

        [DisplayName("Local File Path")]
        public string? FilePath { get; internal set; }

        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public ColorProfileAttribute[] Attributes => _profile.Attributes;

        [DisplayName("Rendering Intent")]
        public int RenderingIntent => _profile.RenderingIntent;
        public string Illuminant => "X:" + _profile.Illuminant.ciexyzX + ",Y:" + _profile.Illuminant.ciexyzY + ",Z:" + _profile.Illuminant.ciexyzZ;
        public ColorProfileFlags Flags => _profile.Flags;

        [DisplayName("CMM Type")]
        public string CmmType => _profile.CmmTypeString;
        public string Class => _profile.ClassString;

        [DisplayName("Data Color Space")]
        public string DataColorSpace => _profile.DataColorSpaceString;

        [DisplayName("Model Description")]
        public string ModelDescription => _profile.ModelDescription;

        [DisplayName("Connection Space")]
        public string ConnectionSpace => _profile.ConnectionSpaceString;
        public string Signature => _profile.SignatureString;
        public string Platform => _profile.PlatformString;
        public string Manufacturer => _profile.ManufacturerString;
        public string Model => _profile.ModelString;
        public string Creator => _profile.CreatorString;
        public string Description => _profile.Description;
        public string Copyright => _profile.Copyright;

        [DisplayName("Viewing Condition")]
        public string ViewingCondition => _profile.ViewingCondition;

        [DisplayName("Manufacturer Description")]
        public string ManufacturerDescription => _profile.ManufacturerDescription;

        [DisplayName("Unicode Language Code")]
        public int UnicodeLanguageCode => _profile.UnicodeLanguageCode;

        [DisplayName("Registered Characterization")]
        public string RegisteredCharacterization => _profile.RegisteredCharacterization;

        [DisplayName("Localized Strings")]
        [Editor(typeof(CollectionEditorNoType), typeof(UITypeEditor))]
        [TypeConverter(typeof(StringFormatterArrayConverter))]
        [StringFormatter("{Length}")]
        public LocalizedStringModel[] LocalizedStrings { get; set; }

        public override string ToString()
        {
            var str = _profile.ToString();
            if (!string.IsNullOrWhiteSpace(str))
                return str;

            if (FilePath != null)
                return FilePath;

            return string.Empty;
        }
    }
}
