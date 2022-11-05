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

        public Settings()
        {
        }

        public virtual void SerializeToConfiguration() => Serialize(ConfigurationFilePath);
        public static void BackupConfiguration() => Backup(ConfigurationFilePath);

        [DefaultValue(null)]
        public virtual IList<RecentFile>? RecentFilesPaths { get => GetPropertyValue((IList<RecentFile>?)null); set { SetPropertyValue(value); } }

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
