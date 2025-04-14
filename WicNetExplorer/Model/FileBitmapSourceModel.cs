using System;
using System.ComponentModel;
using System.IO;
using WicNet;

namespace WicNetExplorer.Model;

public class FileBitmapSourceModel : BitmapSourceModel
{
    public static FileBitmapSourceModel Load(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        using var source = WicBitmapSource.Load(filePath);
        {
            return new FileBitmapSourceModel(filePath, source);
        }
    }

    private FileBitmapSourceModel(string filePath, WicBitmapSource bitmap)
        : base(bitmap)
    {
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
