using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WicNetExplorer.Utilities;

namespace WicNetExplorer
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            Icon = Resources.WicNetIcon;

            Task.Run(() => Settings.Current.CleanRecentFiles());
        }

        public ImageForm? ActiveImageForm => ActiveMdiChild as ImageForm;

        private void OpenFile(string? fileName = null) => ImageForm.OpenFile(this, fileName);

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
    }
}