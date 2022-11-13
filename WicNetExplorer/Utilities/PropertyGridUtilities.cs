using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace WicNetExplorer.Utilities
{
    public static class PropertyGridUtilities
    {
        private static readonly Lazy<FieldInfo?> _gridViewField = new(() =>
            {
                var fi = typeof(PropertyGrid)?.GetField("gridView", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi == null)
                {
                    fi = typeof(PropertyGrid)?.GetField("_gridView", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                return fi;
            });

        private static readonly Lazy<FieldInfo?> _gridEntryFlagsField = new(() =>
        {
            var type = Type.GetType("System.Windows.Forms.PropertyGridInternal.GridEntry, System.Windows.Forms", false);
            return type?.GetField("_flags", BindingFlags.Instance | BindingFlags.NonPublic);
        });

        // https://stackoverflow.com/a/5181423/403671
        public static GridItemCollection? GetAllGridEntries(this PropertyGrid grid)
        {
            ArgumentNullException.ThrowIfNull(grid);
            if (_gridViewField.Value == null)
                return null;

            try
            {
                var view = _gridViewField.Value.GetValue(grid);
                if (view == null)
                    return null;

                return (GridItemCollection?)view.GetType().InvokeMember("GetAllGridEntries", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, view, null);
            }
            catch
            {
                return null;
            }
        }

        public static void ExpandAllItems(this GridItem item)
        {
            if (item == null)
                return;

            var sva = item.Value?.GetType().GetCustomAttribute<ToStringVisitorAttribute>();
            if (sva != null)
            {
                if (sva.ForceIsValue)
                    return;
            }

            item.Expanded = true;
            foreach (var child in item.GridItems.OfType<GridItem>())
            {
                child.ExpandAllItems();
            }
        }

        public static void CollapseAllItems(this GridItem item)
        {
            if (item == null)
                return;

            item.Expanded = false;
            foreach (var child in item.GridItems.OfType<GridItem>())
            {
                child.CollapseAllItems();
            }
        }
    }
}
