using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WicNet;
using WicNetExplorer.Model;
using WicNetExplorer.Utilities;
using Extensions = WicNetExplorer.Utilities.Extensions;

namespace WicNetExplorer
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            Icon = Resources.WicNetIcon;

            imageToolStripMenuItem.Visible = false;

#if !DEBUG
            toolStripSeparatorDebug.Visible = false;
            gCCollectToolStripMenuItem.Visible = false;
#endif

            Task.Run(() => Settings.Current.CleanRecentFiles());
        }

        public ImageForm? ActiveImageForm => ActiveMdiChild as ImageForm;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
            {
                D2DCompositionControl.DisposeResources();
            }
        }

        private void OpenFile(string? fileName = null)
        {
            var form = ImageForm.OpenFile(this, fileName);
            if (form == null)
                return;

            imageToolStripMenuItem.Visible = true;
            if (MdiChildren.Length == 1)
            {
                form.Left = form.Padding.Top;
                form.Top = form.Left;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.T && e.Shift && e.Control)
            {
                var lastRecent = Settings.Current.RecentFilesPaths?.FirstOrDefault();
                if (lastRecent != null)
                {
                    OpenFile(lastRecent.FilePath);
                }
            }
        }

        private void About()
        {
            var dlg = new AboutForm();
            dlg.ShowDialog(this);
        }

        private void SysInfo()
        {
            SystemInfoModel model;
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
            {
                model = new SystemInfoModel2();
            }
            else
            {
                model = new SystemInfoModel();
            }

            var dlg = new ObjectForm(model, true)
            {
                Text = Resources.SysInfo
            };
            dlg.ShowDialog(this);
        }

        private void DxInfo()
        {
            if (ActiveImageForm == null)
                return;

            var d2d = ActiveImageForm.D2DControl;
            if (d2d == null)
                return;

            d2d.WithDeviceContext(dc =>
            {
                var dlg = new ObjectForm(new DirectXInfoModel(d2d, dc, ActiveImageForm.Bitmap), true)
                {
                    Text = Resources.DirectXInfo
                };
                dlg.ShowDialog(this);
            });
        }

        private void OpenLocation()
        {
            var fileName = ActiveImageForm?.FileName;
            if (fileName != null && IOUtilities.PathIsFile(fileName))
            {
                Extensions.OpenExplorer(System.IO.Path.GetDirectoryName(fileName)!);
            }
        }

        private void OpenFileLocationToolStripMenuItem_Click(object sender, EventArgs e) => OpenLocation();
        private void DirectXInfoToolStripMenuItem_Click(object sender, EventArgs e) => DxInfo();
        private void ShowSystemInformationToolStripMenuItem_Click(object sender, EventArgs e) => SysInfo();
        private void AboutWicNetExplorerToolStripMenuItem_Click(object sender, EventArgs e) => About();
        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e) => MdiForm.LayoutMdi(this, MdiLayout.Cascade);
        private void TileHorizontallyToolStripMenuItem_Click(object sender, EventArgs e) => MdiForm.LayoutMdi(this, MdiLayout.TileHorizontal);
        private void TileVerticallyToolStripMenuItem_Click(object sender, EventArgs e) => MdiForm.LayoutMdi(this, MdiLayout.TileVertical);
        private void CloseToolStripMenuItem_Click(object sender, EventArgs e) => ActiveMdiChild?.Close();
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e) => ActiveImageForm?.SaveFile();
        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e) => ActiveImageForm?.SaveFileAs();
        private void ClearRecentListToolStripMenuItem_Click(object sender, EventArgs e) => Settings.Current.ClearRecentFiles();
        private void GCCollectToolStripMenuItem_Click(object sender, EventArgs e) => GC.Collect();
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e) => Close();
        private void ToolStripMenuItemOpen_Click(object sender, EventArgs e) => OpenFile(null);
        private void FileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            closeToolStripMenuItem.Enabled = ActiveMdiChild != null;
            saveAsToolStripMenuItem.Enabled = ActiveMdiChild != null;
            saveToolStripMenuItem.Enabled = ActiveMdiChild != null;

            if (saveToolStripMenuItem.Enabled)
            {
                var fileName = ActiveImageForm?.FileName;
                if (fileName == null || Extensions.IsSvg(fileName) || Extensions.IsPdf(fileName))
                {
                    saveToolStripMenuItem.Enabled = false;
                }
            }

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
                    item.Click += (s, e) => OpenFile(recent.FilePath);
                }
            }
            openRecentToolStripMenuItem.Enabled = openRecentToolStripMenuItem.DropDownItems.Count > fixedRecentItemsCount;
        }

        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileName = ActiveImageForm?.FileName;
            if (fileName == null)
                return;

            if (Extensions.IsSvg(fileName) || Extensions.IsPdf(fileName))
            {
                var fileModel = new FileModel(fileName);
                var fileDlg = new ObjectForm(fileModel, true);
                fileDlg.ShowDialog(this);
                return;
            }

            using var model = FileBitmapSourceModel.Load(fileName);
            var dlg = new ObjectForm(model, true);
            dlg.ShowDialog(this);
        }

        protected override void OnMdiChildActivate(EventArgs e)
        {
            imageToolStripMenuItem.Visible = ActiveImageForm != null;
        }

        private void MetadataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileName = ActiveImageForm?.FileName;
            if (fileName == null)
                return;

            using var source = WicBitmapSource.Load(fileName);
            {
                using var reader = source?.GetMetadataReader();
                if (reader == null)
                {
                    this.ShowMessage(Resources.NoMetadata);
                    return;
                }

                var model = new WindowsMetadataModel(reader);
                var dlg = new ObjectForm(model, true)
                {
                    Text = Resources.Metadata
                };
                dlg.ShowDialog(this);
            }
        }

        private void PreferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new ObjectForm(Settings.Current)
            {
                Text = Resources.Preferences
            };
            dlg.CopyToClipboard.Visible = false;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            Settings.Current.SerializeToConfiguration();
        }

        private void OptionsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            honorOrientationToolStripMenuItem.Checked = Settings.Current.HonorOrientation;
            honorColorContextsToolStripMenuItem.Checked = Settings.Current.HonorColorContexts;
        }

        private void HonorOrientationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Current.HonorOrientation = honorOrientationToolStripMenuItem.Checked;
            Settings.Current.SerializeToConfiguration();
        }

        private void HonorColorContextsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Current.HonorColorContexts = honorColorContextsToolStripMenuItem.Checked;
            Settings.Current.SerializeToConfiguration();
        }

        private void ShowWicComponentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new CollectionForm(WicImagingComponent.AllComponents.Select(c => ImagingComponentModel.From(c)).OrderBy(e => e.GetType().Name).ThenBy(e => e.FriendlyName))
            {
                Text = Resources.Components
            };
            ((Control)dlg.AcceptButton!).Visible = false;
            ((Control)dlg.CancelButton!).Text = Resources.Close;
            dlg.ShowDialog(this);
        }

        private void ShowDecodableFileExtensionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new CollectionForm(WicImagingComponent.DecoderFileExtensions.Select(e => new DecoderFileExtensionModel(e)).OrderBy(e => e.Extension))
            {
                Text = Resources.DecodableExtensions
            };
            ((Control)dlg.AcceptButton!).Visible = false;
            ((Control)dlg.CancelButton!).Text = Resources.Close;
            dlg.ShowDialog(this);
        }

        private void ShowEncodableFileExtensionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new CollectionForm(WicImagingComponent.EncoderFileExtensions.Select(e => new EncoderFileExtensionModel(e)).OrderBy(e => e.Extension))
            {
                Text = Resources.EncodableExtensions
            };
            ((Control)dlg.AcceptButton!).Visible = false;
            ((Control)dlg.CancelButton!).Text = Resources.Close;
            dlg.ShowDialog(this);
        }

        private void ImageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var fileName = ActiveImageForm?.FileName;
            if (fileName == null)
                return;

            metadataToolStripMenuItem.Enabled = !Extensions.IsSvg(fileName) && !Extensions.IsPdf(fileName);
        }
    }
}