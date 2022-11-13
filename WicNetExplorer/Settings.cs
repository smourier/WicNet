using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        }

        public virtual void SerializeToConfiguration() => Serialize(ConfigurationFilePath);
        public static void BackupConfiguration() => Backup(ConfigurationFilePath);

        [DefaultValue(null)]
        [Browsable(false)]
        public virtual IList<RecentFile>? RecentFilesPaths { get => GetPropertyValue((IList<RecentFile>?)null); set { SetPropertyValue(value); } }

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
        [DisplayName("Honor Color Contexts")]
        public bool HonorColorContexts { get => GetPropertyValue(_honorColorContextsDefault); set { SetPropertyValue(value); } }

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
