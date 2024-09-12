using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using WicNet;
using WicNetExplorer.Utilities;

namespace WicNetExplorer.Model
{
    public class BitmapSourceModelEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context) => UITypeEditorEditStyle.Modal;
        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
        {
            if (value is WicBitmapSource bitmap)
            {
                if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService svc)
                {
                    using var form = new BitmapSourceForm(bitmap);
                    form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                    form.ClientSize = bitmap.Size.ToSize();
                    svc.ShowDialog(form);
                }
            }
            return base.EditValue(context, provider, value);
        }
    }
}
