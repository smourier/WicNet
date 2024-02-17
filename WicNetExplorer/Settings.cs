using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using DirectN;
using WicNetExplorer.Model;
using WicNetExplorer.Utilities;

namespace WicNetExplorer
{
    public class Settings : Serializable<Settings>
    {
        private const int _maxArrayElementDisplayedDefault = 32;
        private const bool _copyEmptyElementsToClipboardDefault = true;
        private const int _maxArrayElementToCopyToClipboardDefault = int.MaxValue;
        private const bool _honorOrientationDefault = true;
        private const bool _honorColorContextsDefault = true;
        private const bool _pdfIgnoreHighContrast = false;
        private const bool _useBackgroundColorForSvgTransparency = false;

        public const string FileName = "settings.json";

        public static Settings Current { get; }
        public static string ConfigurationFilePath { get; }

        static Settings()
        {
            // data is stored in user's Documents
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), typeof(Settings).Namespace!);

            ConfigurationFilePath = Path.Combine(path, FileName);

            // build settings
            Current = Deserialize(ConfigurationFilePath)!;

            // force settings from cmdline
            foreach (var arg in CommandLine.NamedArguments)
            {
                var pi = Current.GetType().GetProperty(arg.Key);
                if (pi == null || !pi.CanWrite)
                    continue;

                if (pi.PropertyType == typeof(bool) && string.IsNullOrWhiteSpace(arg.Value))
                {
                    try
                    {
                        pi.SetValue(Current, true);
                    }
                    catch
                    {
                        // continue;
                    }
                    continue;
                }

                if (Conversions.TryChangeType(arg.Value, pi.PropertyType, out var value))
                {
                    try
                    {
                        pi.SetValue(Current, value);
                    }
                    catch
                    {
                        // continue
                    }
                }
            }
        }

        public virtual void SerializeToConfiguration() => Serialize(ConfigurationFilePath);
        public static void BackupConfiguration() => Backup(ConfigurationFilePath);

        [DefaultValue(null)]
        [Browsable(false)]
        public virtual IList<RecentFile>? RecentFilesPaths { get => GetPropertyValue((IList<RecentFile>?)null); set { SetPropertyValue(value); } }

        [DefaultValue(false)]
        [DisplayName("Force Half Floating Point (16-bit) Surface")]
        public bool ForceFP16 { get => GetPropertyValue(false); set { SetPropertyValue(value); } }

        [DefaultValue(false)]
        [DisplayName("Force Windows 7 Mode")]
        public bool ForceW7 { get => GetPropertyValue(false); set { SetPropertyValue(value); } }

        [DisplayName("Background Color")]
        [DefaultValue(typeof(Color), nameof(Color.Transparent))]
        public Color BackgroundColor { get => GetPropertyValue(Color.Transparent); set { SetPropertyValue(value); } }

        [DisplayName("Scaling Interpolation Mode")]
        [DefaultValue(InterpolationMode.NearestNeighbor)]
        public InterpolationMode ScalingInterpolationMode { get => GetPropertyValue(InterpolationMode.NearestNeighbor); set { SetPropertyValue(value); } }

        [DefaultValue(_maxArrayElementDisplayedDefault)]
        [DisplayName("Maximum Array Element Displayed")]
        public int MaxArrayElementDisplayed { get => GetPropertyValue(_maxArrayElementDisplayedDefault); set { SetPropertyValue(value); } }

        [DefaultValue(_maxArrayElementToCopyToClipboardDefault)]
        [DisplayName("Maximum Array Element to Copy to Clipboard")]
        public int MaxArrayElementToCopyToClipboard { get => GetPropertyValue(_maxArrayElementToCopyToClipboardDefault); set { SetPropertyValue(value); } }

        [DefaultValue(_copyEmptyElementsToClipboardDefault)]
        [DisplayName("Copy Empty Elements to Clipboard")]
        public bool CopyEmptyElementsToClipboard { get => GetPropertyValue(_copyEmptyElementsToClipboardDefault); set { SetPropertyValue(value); } }

        [DefaultValue(_honorOrientationDefault)]
        [DisplayName("Honor Orientation")]
        public bool HonorOrientation { get => GetPropertyValue(_honorOrientationDefault); set { SetPropertyValue(value); } }

        [DefaultValue(_honorColorContextsDefault)]
        [DisplayName("Honor Color Spaces")]
        public bool HonorColorContexts { get => GetPropertyValue(_honorColorContextsDefault); set { SetPropertyValue(value); } }

        [DefaultValue(_pdfIgnoreHighContrast)]
        [DisplayName("Ignore High Contrast for PDF rendering")]
        public bool PdfIgnoreHighContrast { get => GetPropertyValue(_pdfIgnoreHighContrast); set { SetPropertyValue(value); } }

        [DefaultValue(_useBackgroundColorForSvgTransparency)]
        [DisplayName("Use Background Color For Svg Transparency")]
        public bool UseBackgroundColorForSvgTransparency { get => GetPropertyValue(_useBackgroundColorForSvgTransparency); set { SetPropertyValue(value); } }

        private Dictionary<string, DateTime> GetRecentFiles()
        {
            var dic = new Dictionary<string, DateTime>(StringComparer.Ordinal);
            var recents = RecentFilesPaths;
            if (recents != null)
            {
                foreach (var recent in recents)
                {
                    if (recent?.FilePath == null)
                        continue;

                    if (!IOUtilities.PathIsFile(recent.FilePath))
                        continue;

                    dic[recent.FilePath] = recent.LastAccessTime;
                }
            }
            return dic;
        }

        private void SaveRecentFiles(Dictionary<string, DateTime> dic)
        {
            var list = dic.Select(kv => new RecentFile { FilePath = kv.Key, LastAccessTime = kv.Value }).OrderByDescending(r => r.LastAccessTime).ToList();
            if (list.Count == 0)
            {
                RecentFilesPaths = null;
            }
            else
            {
                RecentFilesPaths = list;
            }
        }

        public void CleanRecentFiles() => SaveRecentFiles(GetRecentFiles());
        public void ClearRecentFiles()
        {
            RecentFilesPaths = null;
            SerializeToConfiguration();
        }

        public void AddRecentFile(string filePath)
        {
            ArgumentNullException.ThrowIfNull(filePath);
            if (!IOUtilities.PathIsFile(filePath))
                return;

            var dic = GetRecentFiles();
            dic[filePath] = DateTime.UtcNow;
            SaveRecentFiles(dic);
        }
    }
}
