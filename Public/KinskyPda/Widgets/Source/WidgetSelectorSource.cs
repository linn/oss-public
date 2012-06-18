
using System;
using System.Drawing;
using System.Windows.Forms;

using Upnp;
using Linn.Kinsky;

namespace KinskyPda.Widgets
{
    public class WidgetSelectorSource : BaseListView, IViewWidgetSelector<Linn.Kinsky.Source>
    {
        public event EventHandler<EventArgsSelection<Linn.Kinsky.Source>> EventSelectionChanged;

        public WidgetSelectorSource()
        {
            iSmallImageList = new ImageList();
            iSmallImageList.ImageSize = LayoutManager.Instance.SmallImageSize;
            iSmallImageList.Images.Add(TextureManager.Instance.IconSelectedServer);

            this.Activation = ItemActivation.TwoClick;
            this.ForeColor = ViewSupport.Instance.ForeColourBright;
            this.HeaderStyle = ColumnHeaderStyle.None;
            this.SmallImageList = iSmallImageList;
            this.View = View.Details;
            this.Columns.Clear();
            this.Font = ViewSupport.Instance.FontMedium;
            this.FullRowSelect = true;
            this.ItemActivate += ListViewItemActivate;
            this.SelectedIndexChanged += ListViewSelectedIndexChanged;

            this.Columns.Add("", 480 - BaseListView.kScrollbarWidth, HorizontalAlignment.Left);
        }

        public void Open()
        {
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
                iCurrentlySelectedIndex = -1;
            }
        }

        private delegate void InsertItemDelegate(int aIndex, Linn.Kinsky.Source aItem);
        public void InsertItem(int aIndex, Linn.Kinsky.Source aItem)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new InsertItemDelegate(InsertItem), aIndex, aItem);
            }
            else
            {
                ListViewItem item = new ListViewItem(aItem.Name);
                item.Tag = aItem;

                Items.Insert(aIndex, item);
                if (aIndex <= iCurrentlySelectedIndex)
                {
                    iCurrentlySelectedIndex++;
                }
            }
        }

        private delegate void RemoveItemDelegate(Linn.Kinsky.Source aItem);
        public void RemoveItem(Linn.Kinsky.Source aItem)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new RemoveItemDelegate(RemoveItem), aItem);
            }
            else
            {
                for(int i = 0; i < Items.Count; ++i)
                {
                    if (Items[i].Tag == aItem)
                    {
                        Items.RemoveAt(i);
                        if (i < iCurrentlySelectedIndex)
                        {
                            iCurrentlySelectedIndex--;
                        }
                        break;
                    }
                }
            }
        }

        private delegate void ItemChangedDelegate(Linn.Kinsky.Source aItem);
        public void ItemChanged(Linn.Kinsky.Source aItem)
        {
			if (InvokeRequired)
            {
                BeginInvoke(new ItemChangedDelegate(ItemChanged), aItem);
            }
            else
            {
                for(int i = 0; i < Items.Count; ++i)
                {
                    if (Items[i].Tag == aItem)
                    {
                        Items[i].Text = aItem.Name;
						break;
                    }
                }
            }
        }
        
        private delegate void SetSelectedDelegate(Linn.Kinsky.Source aItem);
        public void SetSelected(Linn.Kinsky.Source aItem)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetSelectedDelegate(SetSelected), aItem);
            }
            else
            {
                ListViewItem foundItem = null;
                int? foundIndex = base.FindByTag(aItem, out foundItem);

                if (foundIndex != null)
                {
                    //set listview item icon
                    foundItem.ImageIndex = kSelectedServerIcon;

                    //remove now playing icon from previously selected if the track was changed from a different device
                    if (iCurrentlySelectedIndex > -1)
                    {
                        if (iCurrentlySelectedIndex != foundIndex)
                        {
                            ListViewItem currentlySelected = this.Items[iCurrentlySelectedIndex];
                            currentlySelected.ImageIndex = -1;
                        }
                    }
                    iCurrentlySelectedIndex = foundIndex.Value;
                }
            }
        }

        private void ListViewSelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (SelectedIndices.Count == 0)
            {
                //make focussed item unfocussed
                ListViewItem item = this.FocusedItem;
                if (item != null)
                {
                    item.Focused = false;
                }
            }
        }

        private void ListViewItemActivate(object sender, EventArgs e)
        {
            ListViewItem selectedItem = null;
            int? selectedIndex = base.GetSelectedItem(out selectedItem);

            if (selectedItem != null)
            {
                //make unselected to remove highlight
                selectedItem.Selected = false;
                selectedItem.Focused = false;

                //remove now playing icon from previously selected
                if (iCurrentlySelectedIndex > -1 && iCurrentlySelectedIndex != selectedIndex.Value)
                {
                    ListViewItem currentlySelected = Items[iCurrentlySelectedIndex];
                    currentlySelected.ImageIndex = -1;
                    iCurrentlySelectedIndex = -1;
                }

                if (EventSelectionChanged != null)
                {
                    EventSelectionChanged(this, new EventArgsSelection<Linn.Kinsky.Source>(selectedItem.Tag as Linn.Kinsky.Source));
                }
            }
        }

        private const int kSelectedServerIcon = 0;
        private int iCurrentlySelectedIndex = -1;
        private ImageList iSmallImageList;
    }
}
