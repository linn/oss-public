
using System;
using System.Drawing;
using System.Windows.Forms;

using Linn;
using Linn.Kinsky;

namespace KinskyPda.Widgets
{
    public class WidgetSelectorRoom : ComboBox, IViewWidgetSelector<Linn.Kinsky.Room>
    {
        public event EventHandler<EventArgsSelection<Linn.Kinsky.Room>> EventSelectionChanged;

        public WidgetSelectorRoom()
        {
            iOpen = false;
            SelectedIndexChanged += EventSelectedIndexChanged;
        }

        private delegate void OpenDelegate();
        public void Open()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new OpenDelegate(Open));
            }
            else
            {
                Items.Add(new ComboBoxItem(null, kNotSelected, null));
                SelectedIndex = 0;
                iOpen = true;
            }
        }

        private delegate void CloseDelegate();
        public void Close()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new CloseDelegate(Close));
            }
            else
            {
                Items.Clear();
                iOpen = false;
            }
        }

        private delegate void InsertItemDelegate(int aIndex, Linn.Kinsky.Room aItem);
        public void InsertItem(int aIndex, Linn.Kinsky.Room aItem)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new InsertItemDelegate(InsertItem), aIndex, aItem);
            }
            else
            {
                // increment index by 1 to take account of the "not selected" item
                ComboBoxItem item = new ComboBoxItem(null, aItem.Name, aItem);
                Items.Insert(aIndex + 1, item);
            }
        }

        private delegate void RemoveItemDelegate(Linn.Kinsky.Room aItem);
        public void RemoveItem(Linn.Kinsky.Room aItem)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new RemoveItemDelegate(RemoveItem), aItem);
            }
            else
            {
                for(int i = 0; i < Items.Count; ++i)
                {
                    ComboBoxItem item = Items[i] as ComboBoxItem;
                    if (item.Tag == aItem)
                    {
                        if (i == SelectedIndex)
                        {
                            SelectedIndex = 0;
                        }
                        Items.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public void ItemChanged(Linn.Kinsky.Room aItem)
        {
        }

        private delegate void SetSelectedDelegate(Linn.Kinsky.Room aItem);
        public void SetSelected(Linn.Kinsky.Room aItem)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetSelectedDelegate(SetSelected), aItem);
            }
            else
            {
                if (iOpen == false)
                {
                    return;
                }

                if (aItem != null)
                {
                    for (int i = 0; i < Items.Count; ++i)
                    {
                        ComboBoxItem item = Items[i] as ComboBoxItem;
                        if (item.Tag == aItem)
                        {
                            SelectedIndex = i;

                            break;
                        }
                    }
                }
                else //if aItem is null then standby has been selected so set to no room selected
                {
                    SelectedIndex = 0;
                }
            }
        }

        private void EventSelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItem item = this.SelectedItem as ComboBoxItem;
            object tag = (item == null) ? null : item.Tag;

            if (EventSelectionChanged != null)
            {
                EventSelectionChanged(this, new EventArgsSelection<Linn.Kinsky.Room>(tag as Linn.Kinsky.Room));
            }
        }

        private const string kNotSelected = "No room selected";
        private bool iOpen;
    }
}
