using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace WicNetExplorer.Utilities
{
    public class CollectionEditor : UITypeEditor
    {
        public virtual bool HideTypeColumn { get; set; }

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
                    var form = new CollectionForm(enumerable, HideTypeColumn);
                    svc.ShowDialog(form);

                }
            }
            return base.EditValue(context, provider, value);
        }
    }
}
