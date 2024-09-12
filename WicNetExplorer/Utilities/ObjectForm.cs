using System;
using System.Windows.Forms;

namespace WicNetExplorer.Utilities
{
    public partial class ObjectForm : Form
    {
        public ObjectForm(object obj, bool closeMode = false)
        {
            InitializeComponent();
            Icon = Resources.WicNetIcon;
            propertyGridObject.SelectedObject = obj;

            if (closeMode)
            {
                ((Control)AcceptButton!).Visible = false;
                ((Control)CancelButton!).Text = Resources.Close;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            base.OnKeyDown(e);
        }

        private void ExpandChildrenToolStripMenuItem_Click(object sender, EventArgs e) => propertyGridObject.SelectedGridItem.ExpandAllItems();
        private void CollapseChildrenToolStripMenuItem_Click(object sender, EventArgs e) => propertyGridObject.SelectedGridItem.CollapseAllItems();
        private void ExpandAllItemsToolStripMenuItem_Click(object sender, EventArgs e) => propertyGridObject.ExpandAllGridItems();
        private void CollapseAllItemsToolStripMenuItem_Click(object sender, EventArgs e) => propertyGridObject.CollapseAllGridItems();
        private void ButtonCopyToClipboard_Click(object sender, EventArgs e)
        {
            var text = ToStringVisitor.Visit(propertyGridObject.SelectedObject, "  ");
            Clipboard.SetText(text);
            this.ShowMessage(string.Format(Resources.CopiedToClipboard, text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Length));
        }

        private void ContextMenuStripGrid_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            expandChildrenToolStripMenuItem.Enabled = propertyGridObject.SelectedGridItem != null;
            collapseAllItemsToolStripMenuItem.Enabled = expandChildrenToolStripMenuItem.Enabled;
        }
    }
}
