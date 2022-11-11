using System.Windows.Forms;

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
    }
}
