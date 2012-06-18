using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KinskyPda.Widgets
{
    public class BaseListView : ListView
    {
        protected const int kScrollbarWidth = 30;

        protected int? GetSelectedItem(out ListViewItem selectedItem)
        {
            selectedItem = null;
            int? selectedIndex;

            if (this.SelectedIndices.Count == 0)
            {
                selectedIndex = null;
            }
            else
            {
                selectedIndex = this.SelectedIndices[0];
                selectedItem = this.Items[selectedIndex.Value];
            }

            return selectedIndex;
        }

        protected int? FindByTag(object aTag, out ListViewItem foundItem)
        {
            foundItem = null;
            int selectedIndex = 0;
            foreach (ListViewItem item in this.Items)
            {
                if (item.Tag != null)
                {
                    if (item.Tag == aTag)
                    {
                        foundItem = item;
                        return selectedIndex;
                    }
                }
                selectedIndex++;
            }

            return null;
        }

    }
}
