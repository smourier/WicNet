using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace WicNetExplorer.Utilities
{
    public class ByteArrayEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (value is byte[] bytes)
            {
                if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService svc)
                {
                    var form = new ByteArrayForm
                    {
                        Text = context.PropertyDescriptor.DisplayName
                    };
                    form.SetBytes(bytes);
                    svc.ShowDialog(form);
                }
            }
            return base.EditValue(context, provider, value);
        }
    }
}
