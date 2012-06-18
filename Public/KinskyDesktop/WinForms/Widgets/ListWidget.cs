using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using Linn.Kinsky;

namespace KinskyDesktop.Widgets
{
    public partial class ListWidget : UserControl
    {
        public class IEntry
        {
        }

        public class Item : IEntry
        {
            public Item(string aTitle)
            {
                Title = aTitle;
            }

            public override string ToString()
            {
                return Title;
            }

            public string Title;
        }

        public class ListWidgetItemCollection : IList<IEntry>
        {
            public ListWidgetItemCollection(ListWidget aListWidget)
            {
                iListWidget = aListWidget;

                iItems = new List<IEntry>();
            }

            public int IndexOf(IEntry item)
            {
                return iItems.IndexOf(item);
            }

            public void Insert(int index, IEntry item)
            {
                iListWidget.BeginUpdate();
                iItems.Insert(index, item);
                iListWidget.CalculateBounds();
                iListWidget.EndUpdate();
            }

            public void RemoveAt(int index)
            {
                iListWidget.BeginUpdate();
                iItems.RemoveAt(index);
                iListWidget.CalculateBounds();
                iListWidget.EndUpdate();
            }

            public void RemoveRange(int index, int count)
            {
                iListWidget.BeginUpdate();
                iItems.RemoveRange(index, count);
                iListWidget.CalculateBounds();
                iListWidget.EndUpdate();
            }

            public IEntry this[int index]
            {
                get
                {
                    return iItems[index];
                }
                set
                {
                    iListWidget.BeginUpdate();
                    iItems[index] = value;
                    iListWidget.CalculateBounds();
                    iListWidget.EndUpdate();
                }
            }

            public void Add(IEntry item)
            {
                iListWidget.BeginUpdate();
                iItems.Add(item);
                iListWidget.CalculateBounds();
                iListWidget.EndUpdate();
            }

            public void AddRange(IEnumerable<IEntry> collection)
            {
                iListWidget.BeginUpdate();
                iItems.AddRange(collection);
                iListWidget.CalculateBounds();
                iListWidget.EndUpdate();
            }

            public void Clear()
            {
                iListWidget.BeginUpdate();
                iItems.Clear();
                iListWidget.CalculateBounds();
                iListWidget.EndUpdate();
            }

            public bool Contains(IEntry item)
            {
                return iItems.Contains(item);
            }

            public void CopyTo(IEntry[] array, int arrayIndex)
            {
                iItems.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get
                {
                    return iItems.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public bool Remove(IEntry item)
            {
                iListWidget.BeginUpdate();
                bool result = iItems.Remove(item);
                iListWidget.CalculateBounds();
                iListWidget.EndUpdate();
                return result;
            }

            public IEnumerator<IEntry> GetEnumerator()
            {
                return iItems.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return iItems.GetEnumerator();
            }

            private ListWidget iListWidget;
            private List<IEntry> iItems;
        }

        public ListWidget()
        {
            iViewIndex = -1;
            iViews = new List<IListWidgetView>();

            InitializeComponent();

            iUpdate = true;
            iOffset = 0;

            iItems = new ListWidgetItemCollection(this);

            scrollBarControl1.EventScroll += EventScroll;
            scrollBarControl1.EventValueChanged += EventScroll;
        }

        public IViewSupport ViewSupport
        {
            get
            {
                return iViewSupport;
            }
            set
            {
                if (iViewSupport != null)
                {
                    iViewSupport.EventSupportChanged -= EventSupportChanged;
                }

                iViewSupport = value;
                iViewSupport.EventSupportChanged += EventSupportChanged;
            }
        }

        public void Add(IListWidgetView aView)
        {
            iViews.Add(aView);
            aView.SetListWidget(this);
            if (iViewIndex == -1)
            {
                iViewIndex = 0;
            }
        }

        public void Remove(IListWidgetView aView)
        {
            iViews.Remove(aView);
            aView.SetListWidget(null);

            if (iViewIndex == iViews.Count)
            {
                iViewIndex = iViews.Count - 1;
                Invalidate();
            }
        }

        public void NextView()
        {
            iViewIndex = (iViewIndex + 1) % iViews.Count;
        }

        public void BeginUpdate()
        {
            iUpdate = false;
        }

        internal void CalculateBounds()
        {
            if (iViewIndex > -1)
            {
                int height = iViews[iViewIndex].Height;

                float oldMaximum = scrollBarControl1.Maximum;
                scrollBarControl1.Maximum = height - DisplayRectangle.Height;
                scrollBarControl1.Minimum = 0;
                scrollBarControl1.LargeChange = scrollBarControl1.Maximum / (scrollBarControl1.Height + height);
                scrollBarControl1.SmallChange = 15;

                scrollBarControl1.Value = (int)(scrollBarControl1.Maximum * (iOffset / oldMaximum));
            }
        }

        public void EndUpdate()
        {
            iUpdate = true;
            Invalidate();
        }

        public ListWidgetItemCollection Items
        {
            get
            {
                return iItems;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (iUpdate)
            {
                //e.Graphics.Clear(BackColor);

                if (iViewIndex > -1)
                {
                    iViews[iViewIndex].Render(e.Graphics, iOffset);
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            CalculateBounds();
            Invalidate(true);
        }

        private void EventSupportChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private int iOffset;

        private IViewSupport iViewSupport;

        private ListWidgetItemCollection iItems;

        private bool iUpdate;
        private int iViewIndex;
        private List<IListWidgetView> iViews;

        private void EventScroll(object sender, EventArgs e)
        {
            iOffset = scrollBarControl1.Value;
            Invalidate(false);
            //Update();
        }
    }

    public interface IListWidgetItem
    {
    }

    public interface IListWidgetView
    {
        void SetListWidget(ListWidget aListWidget);
        void Render(Graphics aGraphics, int iOffset);

        int Height { get; }
    }

    public class ListWidgetViewIcon : IListWidgetView
    {
        public ListWidgetViewIcon()
        {
            IconSize = new Size(64, 64);
        }

        public Size IconSize
        {
            get
            {
                return iIconSize;
            }
            set
            {
                iIconSize = value;
            }
        }

        public void SetListWidget(ListWidget aListWidget)
        {
            iListWidget = aListWidget;
        }

        public void Render(Graphics aGraphics, int aOffset)
        {
            int startIndex = (int)(Math.Floor(aOffset / (float)iIconSize.Height) * iNumColumns);
            for (int i = startIndex; i < iListWidget.Items.Count; ++i)
            {
                int y = (int)((Math.Floor(i / (double)iNumColumns) * iIconSize.Height) - aOffset);
                
                if (y > iListWidget.ClientRectangle.Height)
                {
                    break;
                }

                int x = (int)((i % iNumColumns) * iItemWidth);
                
                aGraphics.DrawRectangle(SystemPens.ControlDarkDark, new Rectangle(x, y, iItemWidth, iIconSize.Height));
                aGraphics.DrawString(iItemWidth.ToString() + "x" + iIconSize.Height.ToString() + "\n" + iListWidget.Items[i], SystemFonts.DefaultFont, Brushes.Black, new Rectangle(x, y, iItemWidth, iIconSize.Height));
            }
        }

        public int Height
        {
            get
            {
                if (iListWidget.Items.Count > 0)
                {
                    iNumColumns = (uint)Math.Floor((iListWidget.ClientSize.Width - 1) / (double)iIconSize.Width);
                    if (iNumColumns == 0)
                    {
                        iNumColumns = 1;
                    }
                    if (iListWidget.Items.Count < iNumColumns)
                    {
                        iNumColumns = (uint)iListWidget.Items.Count;
                    }
                    iItemWidth = (int)(iListWidget.ClientSize.Width / (float)iNumColumns);

                    return (int)(Math.Ceiling(iListWidget.Items.Count / (float)iNumColumns) * iIconSize.Height);
                }
                else
                {
                    iNumColumns = 0;
                    iItemWidth = 0;
                }

                return 0;
            }
        }

        private ListWidget iListWidget;

        private Size iIconSize;
        private uint iNumColumns;
        private int iItemWidth;
    }
} // Linn.Kinsky
