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

        private void ButtonCopyToClipboard_Click(object sender, System.EventArgs e)
        {
            var text = ToStringVisitor.Visit(propertyGridObject.SelectedObject, " ");
            this.ShowMessage(text);
        }
    }
}
