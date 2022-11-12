using System;
using System.Windows.Forms;
using WicNetExplorer.Utilities;

namespace WicNetExplorer
{
    public partial class ObjectForm : Form
    {
        public ObjectForm(object obj)
        {
            InitializeComponent();
            Icon = Resources.WicNetIcon;
            propertyGridObject.SelectedObject = obj;
        }

        private void ExpandAllItemsToolStripMenuItem_Click(object sender, EventArgs e) => propertyGridObject.ExpandAllGridItems();
        private void CollapseAllItemsToolStripMenuItem_Click(object sender, EventArgs e) => propertyGridObject.CollapseAllGridItems();
        private void ButtonCopyToClipboard_Click(object sender, EventArgs e)
        {
            var text = ToStringVisitor.Visit(propertyGridObject.SelectedObject, "  ");
            Clipboard.SetText(text);
            this.ShowMessage(Resources.CopiedToClipboard);
        }
    }
}
