using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DirectN;
using WicNet;
using WicNetExplorer.Utilities;
using static System.Windows.Forms.DataFormats;

namespace WicNetExplorer
{
    public partial class Main : Form
    {
        private string? _openedFileName;
        private IComObject<ID2D1Bitmap1>? _bitmap;
        private WicBitmapSource? _bitmapSource;
        private readonly D2DControl _d2d = new();
        private readonly ComponentResourceManager _resourceManager;

        public Main()
        {
            InitializeComponent();
            _resourceManager = new ComponentResourceManager(GetType());
            Icon = Resources.WicNetIcon;

            Task.Run(() => Settings.Current.CleanRecentFiles());

            _d2d.Draw += (s, e) =>
            {
                if (_bitmapSource != null)
                {
                    if (_bitmap == null)
                    {
                        using (var dc = e.Target.AsComObject<ID2D1DeviceContext>())
                        {
                            _bitmap = dc.CreateBitmapFromWicBitmap(_bitmapSource.ComObject);
                            _bitmapSource.Dispose();
                            _bitmapSource = null;
                            var lo = ComObject.LiveObjects;
                        }
                    }
                }

                if (_bitmap != null)
                {
                    e.Target.Clear(_D3DCOLORVALUE.FromColor(_d2d.BackColor));

                    // keep proportions
                    var size = _bitmap.GetSize();
                    var factor = size.GetScaleFactor(_d2d.Width, _d2d.Height);
                    var rc = new D2D_RECT_F(0, 0, size.width * factor.width, size.height * factor.height);
                    e.Target.DrawBitmap(_bitmap, interpolationMode: D2D1_BITMAP_INTERPOLATION_MODE.D2D1_BITMAP_INTERPOLATION_MODE_LINEAR, destinationRectangle: rc);
                    e.Handled = true;
                }
            };
            _d2d.Dock = DockStyle.Fill;
            panelMain.Controls.Add(_d2d);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bitmap?.Dispose();
                _bitmapSource?.Dispose();
                components?.Dispose();
                _d2d?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void OpenFile(string fileName)
        {
            if (!IOUtilities.PathIsFile(fileName))
                return;

            _bitmap?.Dispose();
            _bitmap = null;
            _bitmapSource?.Dispose();

            _bitmapSource = WicBitmapSource.Load(fileName);
            _bitmapSource.ConvertTo(WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);
            _d2d.Invalidate();

            // reset title
            _resourceManager.ApplyResources(this, "$this");
            Text += " - " + fileName;
            _openedFileName = fileName;
            Settings.Current.AddRecentFile(_openedFileName);
            Settings.Current.SerializeToConfiguration();
        }

        private static Tuple<string, int> BuildFilters(IEnumerable<string> extensions)
        {
            var allExts = extensions.OrderBy(ext => ext).ToArray();
            var imageExts = string.Join(";", allExts.Select(ext => "*" + ext));
            return new Tuple<string, int>(string.Join("|", allExts.Select(ext => string.Format(Resources.OneImageFileFilter, ext[1..].ToUpperInvariant(), ext))) + string.Format(Resources.AllImagesFilesFilter, imageExts) + Resources.AllFilesFilter, allExts.Length + 1);
        }

        private void ClearRecentListToolStripMenuItem_Click(object sender, EventArgs e) => Settings.Current.ClearRecentFiles();
        private void GCCollectToolStripMenuItem_Click(object sender, EventArgs e) => GC.Collect();
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e) => Close();
        private void ToolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            var filter = BuildFilters(WicImagingComponent.DecoderFileExtensions);
            var fd = new OpenFileDialog
            {
                RestoreDirectory = true,
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = filter.Item1,
                // note the "all image" list is super long but what other choice do we have?
                FilterIndex = filter.Item2, // select all images by default
            };
            if (fd.ShowDialog(this) != DialogResult.OK)
                return;

            OpenFile(fd.FileName);
        }

        private void FileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            saveAsToolStripMenuItem.Enabled = _bitmap != null;
            saveToolStripMenuItem.Enabled = _bitmap != null;
            closeToolStripMenuItem.Enabled = _bitmap != null;

            const int fixedRecentItemsCount = 2;
            while (openRecentToolStripMenuItem.DropDownItems.Count > fixedRecentItemsCount)
            {
                openRecentToolStripMenuItem.DropDownItems.RemoveAt(0);
            }

            var recents = Settings.Current.RecentFilesPaths;
            if (recents != null)
            {
                foreach (var recent in recents)
                {
                    var item = new ToolStripMenuItem(recent.FilePath);
                    openRecentToolStripMenuItem.DropDownItems.Insert(openRecentToolStripMenuItem.DropDownItems.Count - fixedRecentItemsCount, item);
                    item.Click += (s, e) => OpenFile(recent.FilePath!);
                }
            }
            openRecentToolStripMenuItem.Enabled = openRecentToolStripMenuItem.DropDownItems.Count > fixedRecentItemsCount;
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_bitmap == null)
                return;
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_bitmap == null || _d2d.Target == null)
                return;

            var filter = BuildFilters(WicImagingComponent.DecoderFileExtensions);
            var fd = new SaveFileDialog
            {
                RestoreDirectory = true,
                CheckPathExists = true,
                Filter = filter.Item1,
                FilterIndex = filter.Item2, // select all images by default
            };
            if (fd.ShowDialog(this) != DialogResult.OK)
                return;

            var encoder = WicEncoder.FromFileExtension(Path.GetExtension(fd.FileName));
            if (encoder == null)
                throw new WicNetException("WIC0003: Cannot determine encoder from file path.");

            IOUtilities.FileDelete(fd.FileName, true, false);
            using (var device = _d2d.Target.As<ID2D1DeviceContext>(true).GetDevice())
            {
                using (var image = _bitmap.AsComObject<ID2D1Image>(true))
                {
                    using (var stream = File.OpenWrite(fd.FileName))
                    {
                        image.Save(device, encoder.ContainerFormat, stream);
                    }
                }
            }

            if (new FileInfo(fd.FileName).Length == 0)
            {
                IOUtilities.FileDelete(fd.FileName, true, false);
            }

            OpenFile(fd.FileName);
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _openedFileName = null;
            _bitmap?.Dispose();
            _bitmap = null;
            _bitmapSource?.Dispose();
            _bitmapSource = null;
            _d2d.Invalidate();
        }
    }
}