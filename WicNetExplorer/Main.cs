using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WicNet;
using WicNetExplorer.Model;
using WicNetExplorer.Utilities;

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
            saveAsToolStripMenuItem.Enabled = ActiveMdiChild != null;
            saveToolStripMenuItem.Enabled = ActiveMdiChild != null;
            closeToolStripMenuItem.Enabled = ActiveMdiChild != null;

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

            using var model = FileBitmapSourceModel.Load(fileName);
            var dlg = new ObjectForm(model);
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
                var dlg = new ObjectForm(model);
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
            var dlg = new CollectionForm(WicImagingComponent.AllComponents.Select(c => ImagingComponentModel.From(c)).OrderBy(e => e.GetType().Name).ThenBy(e => e.FriendlyName));
            dlg.Text = Resources.Components;
            ((Control)dlg.AcceptButton).Visible = false;
            ((Control)dlg.CancelButton).Text = Resources.Close;
            dlg.ShowDialog(this);
        }

        private void ShowDecodableFileExtensionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new CollectionForm(WicImagingComponent.DecoderFileExtensions.Select(e => new DecoderFileExtensionModel(e)).OrderBy(e => e.Extension));
            dlg.Text = Resources.DecodableExtensions;
            ((Control)dlg.AcceptButton).Visible = false;
            ((Control)dlg.CancelButton).Text = Resources.Close;
            dlg.ShowDialog(this);
        }

        private void ShowEncodableFileExtensionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new CollectionForm(WicImagingComponent.EncoderFileExtensions.Select(e => new EncoderFileExtensionModel(e)).OrderBy(e => e.Extension));
            dlg.Text = Resources.EncodableExtensions;
            ((Control)dlg.AcceptButton).Visible = false;
            ((Control)dlg.CancelButton).Text = Resources.Close;
            dlg.ShowDialog(this);
        }
    }
}