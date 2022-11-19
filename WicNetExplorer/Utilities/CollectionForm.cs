using System;
using System.Collections;
using System.Windows.Forms;

namespace WicNetExplorer.Utilities
{
    public partial class CollectionForm : Form
    {
        private readonly IEnumerable _enumerable;

        public CollectionForm(IEnumerable enumerable, bool hideTypeColumn = false)
        {
            ArgumentNullException.ThrowIfNull(enumerable);
            _enumerable = enumerable;
            InitializeComponent();
            Icon = Resources.WicNetIcon;

            listViewMain.SuspendLayout();
            foreach (var instance in enumerable)
            {
                if (instance == null)
                    continue;

                string? type = null;
                string? name = null;
                object? value = null;
                if (instance is ICollectionFormItem display)
                {
                    type = display.TypeName;
                    name = display.Name;
                    value = display.Value;
                }

                type ??= instance.GetType().Name?.Decamelize();
                name ??= instance.ToString() ?? string.Empty;
                value ??= instance;

                ListViewItem item;
                if (hideTypeColumn)
                {
                    item = listViewMain.Items.Add(name);
                }
                else
                {
                    item = listViewMain.Items.Add(type);
                }

                item.Tag = value;
                item.SubItems.Add(name);
            }

            if (hideTypeColumn)
            {
                listViewMain.Columns.Remove(columnHeaderType);
            }

            listViewMain.ResumeLayout();
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
            var text = ToStringVisitor.Visit(_enumerable, "  ");
            Clipboard.SetText(text);
            this.ShowMessage(string.Format(Resources.CopiedToClipboard, text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length));
        }

        private void ContextMenuStripGrid_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            expandChildrenToolStripMenuItem.Enabled = propertyGridObject.SelectedGridItem != null;
            collapseAllItemsToolStripMenuItem.Enabled = expandChildrenToolStripMenuItem.Enabled;
        }

        private void ListViewMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewMain.SelectedItems.Count == 0)
                return;

            propertyGridObject.SelectedObject = listViewMain.SelectedItems[0].Tag;
        }
    }
}
