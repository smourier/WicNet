using System.Collections;
using System.Windows.Forms;

namespace WicNetExplorer.Utilities;

public partial class ArrayForm : Form
{
    public ArrayForm()
    {
        InitializeComponent();
        Icon = Resources.WicNetIcon;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }
        base.OnKeyDown(e);
    }

    public void SetArray(IEnumerable enumerable)
    {
        if (enumerable == null)
            return;

        var textSet = false;
        var i = 0;
        foreach (var obj in enumerable)
        {
            if (!textSet && obj != null)
            {
                Text = string.Format(Resources.ArrayOf, obj.GetType().Name);
                textSet = true;
            }

            var item = listViewArray.Items.Add(i.ToString());
            var text = obj?.ToString() ?? string.Empty;
            item.SubItems.Add(text);

            i++;
        }
    }
}
