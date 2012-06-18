using System;
using System.Threading;
using System.Collections.Generic;

using Linn;
using Linn.Gui.Scenegraph;

using Upnp;
using Linn.Topology;

namespace KinskyClassic
{
    class AdapterLibrary : IListEntryProvider
    {
        public class ListableItem : IListable
        {
            internal ListableItem(upnpObject aObject)
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

            public upnpObject Object
            {
                get
                {
                    return iObject;
                }
            }


            private upnpObject iObject;
        }

        public AdapterLibrary()
        {
            iMutex = new Mutex(false);
            iList = new Dictionary<uint, ListableItem>();
            iItems = new List<upnpObject>();
        }

        public void Dispose() { }

        public void AddItems(IList<upnpObject> aItems)
        {
            iMutex.WaitOne();

            iItems.AddRange(aItems);

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
            iItems.Clear();
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
                uint count = (uint)iItems.Count;
                iMutex.ReleaseMutex();
                return count;
            }
        }

        public int Index(ListableItem aItem)
        {
            int index = 0;

            iMutex.WaitOne();

            foreach (object i in iItems)
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

            item = new ListableItem(iItems[(int)aIndex]);
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

        private List<upnpObject> iItems;
        private Dictionary<uint, ListableItem> iList;

        private IList iParent;
    }
} // KinskyClassic
