using System.ComponentModel.Design;
using System.Windows.Forms;

namespace WicNetExplorer.Utilities;

public partial class ByteArrayForm : Form
{
    private readonly ByteViewer _byteViewer;

    public ByteArrayForm()
    {
        InitializeComponent();
        Icon = Resources.WicNetIcon;
        _byteViewer = new ByteViewer
        {
            Dock = DockStyle.Fill
        };
        Controls.Add(_byteViewer);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }
        base.OnKeyDown(e);
    }

    public void SetBytes(byte[] bytes)
    {
        _byteViewer.SetBytes(bytes);
    }
}
