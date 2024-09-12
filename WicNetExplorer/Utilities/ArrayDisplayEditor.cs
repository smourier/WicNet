using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace WicNetExplorer.Utilities
{
    public class ArrayDisplayEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context) => UITypeEditorEditStyle.Modal;
        public override object? EditValue(ITypeDescriptorContext? context, IServiceProvider provider, object? value)
        {
            if (value is IValueProvider valueProvider)
            {
                value = valueProvider.Value!;
            }

            if (value is IEnumerable enumerable)
            {
                if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService svc)
                {
                    var form = new ArrayForm();
                    form.SetArray(enumerable);
                    svc.ShowDialog(form);

                }
            }
            return base.EditValue(context, provider, value);
        }
    }
}
