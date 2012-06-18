using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace KinskyPda
{
    public class ContextMenuHelper
    {
        private ContextMenu iContextMenu;
        private List<MenuItem> iVisibleItems;

        public ContextMenuHelper(ContextMenu aContextMenu)
        {
            iContextMenu = aContextMenu;

            iVisibleItems = new List<MenuItem>();

            foreach (MenuItem item in aContextMenu.MenuItems)
            {
                iVisibleItems.Add(item);
            }
        }

        public void ShowAllItems()
        {
            iContextMenu.MenuItems.Clear();

            foreach (MenuItem item in iVisibleItems)
            {
                iContextMenu.MenuItems.Add(item);
            }
        }

        public void ShowItems(List<int> itemPositions)
        {
            List<int> positions = new List<int>();
            positions.AddRange(itemPositions);

            foreach (MenuItem i in iContextMenu.MenuItems)
            {
                int index = iVisibleItems.IndexOf(i);
                if (!positions.Contains(index))
                {
                    positions.Add(index);
                }
            }

            iContextMenu.MenuItems.Clear();

            for (int i = 0; i < iVisibleItems.Count; ++i)
            {
                if (positions.Contains(i))
                {
                    iContextMenu.MenuItems.Add(iVisibleItems[i]);
                }
            }
        }

        public void HideItems(List<int> itemPositions)
        {
            for (int i = 0; i < itemPositions.Count; i++)
            {
                iContextMenu.MenuItems.Remove(iVisibleItems[itemPositions[i]]);
            }
        }
    }
}
