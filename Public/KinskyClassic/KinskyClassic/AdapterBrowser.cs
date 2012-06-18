using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

using Linn;
using Linn.Gui.Scenegraph;

using Upnp;
using Linn.Topology;

namespace KinskyClassic
{
    class AdapterBrowser : IListEntryProvider
    {
        public class ListableItem : IListable
        {
            internal ListableItem(ListViewItem aObject)
            {
                iObject = aObject;
            }

            public void Highlight()
            {
            }

            public void UnHighlight()
            {
            }

            public void Select()
            {
            }

            public void UnSelect()
            {
            }

            public void Dispose()
            {
            }

            public int State
            {
                get { return 0; }
                set { }
            }

            public NodeHit NodeHit
            {
                get { return null; }
                set { }
            }

            public ListViewItem Object
            {
                get
                {
                    return iObject;
                }
            }


            private ListViewItem iObject;
        }

        public AdapterBrowser()
        {
            iMutex = new Mutex(false);
            iList = new Dictionary<uint, ListableItem>();
            iListViewItems = new ArrayList();
        }

        public void Dispose() { }

        public void SetListViewItems(System.Collections.IList aListViewItems)
        {
            iMutex.WaitOne();

            iList.Clear();
            iListViewItems = aListViewItems;

            iMutex.ReleaseMutex();
        }

        public void Clear()
        {
            if (iParent != null)
            {
                iParent.Clear();
            }

            iMutex.WaitOne();
            iList.Clear();
            iListViewItems.Clear();
            iMutex.ReleaseMutex();
        }

        public void SetList(Linn.Gui.Scenegraph.IList aList)
        {
            iParent = aList;
        }

        public uint Count
        {
            get
            {
                iMutex.WaitOne();
                uint count = (uint)iListViewItems.Count;
                iMutex.ReleaseMutex();
                return count;
            }
        }

        public int Index(ListableItem aItem)
        {
            int index = 0;

            iMutex.WaitOne();

            foreach (object i in iListViewItems)
            {
                if (i == aItem.Object)
                {
                    iMutex.ReleaseMutex();

                    return (index);
                }

                index++;
            }

            iMutex.ReleaseMutex();

            return -1;
        }

        public IListable Entry(uint aIndex)
        {
            ListableItem item;

            iMutex.WaitOne();

            if (iList.TryGetValue(aIndex, out item))
            {
                iMutex.ReleaseMutex();

                return item;
            }

            item = new ListableItem(iListViewItems[(int)aIndex] as ListViewItem);
            iList[aIndex] = item;

            iMutex.ReleaseMutex();

            return item;
        }

        public List<IListable> Entries(uint aStartIndex, uint aCount)
        {
            List<IListable> list = new List<IListable>();

            for (uint i = 0; i < aCount; i++)
            {
                list.Add(Entry(aStartIndex + i));
            }

            return list;
        }

        private Mutex iMutex;

        private System.Collections.IList iListViewItems;
        private Dictionary<uint, ListableItem> iList;

        private Linn.Gui.Scenegraph.IList iParent;
    }
} // KinskyClassic
