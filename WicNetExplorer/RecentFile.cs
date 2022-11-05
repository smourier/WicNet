using System;

namespace WicNetExplorer
{
    public class RecentFile
    {
        public string? FilePath { get; set; }
        public DateTime LastAccessTime { get; set; } = DateTime.Now;

        public override string ToString() => LastAccessTime + " " + FilePath;
    }
}
