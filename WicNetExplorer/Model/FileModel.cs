using System;
using System.ComponentModel;
using System.IO;

namespace WicNetExplorer.Model
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FileModel
    {
        public FileModel(string filePath)
        {
            ArgumentNullException.ThrowIfNull(filePath);
            FilePath = filePath;
            try
            {
                var fi = new FileInfo(filePath);
                FileSize = fi.Length;
                FileLastWriteTime = fi.LastWriteTime;
            }
            catch
            {
                // do nothing
            }
        }

        [DisplayName("File Size")]
        public long FileSize { get; }

        [DisplayName("File Path")]
        public string FilePath { get; }

        [DisplayName("File Last Write Time")]
        public DateTime FileLastWriteTime { get; }

        public override string ToString() => FilePath;
    }
}
