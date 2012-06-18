using System;
using System.Threading;
using System.Collections.Generic;

using Linn;
using Linn.Gui.Scenegraph;

using Upnp;
using Linn.Topology;

namespace KinskyClassic
{
    class AdapterPlaylist : IListEntryProvider
    {
        public class ListableItem : IListable
        {
            internal ListableItem(MrItem aPlaylistItem)
            {
                iPlaylistItem = aPlaylistItem;
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

            public MrItem PlaylistItem
            {
                get
                {
                    return iPlaylistItem;
                }
            }


            private MrItem iPlaylistItem;
        }

        public AdapterPlaylist()
        {
            iMutex = new Mutex(false);
            iList = new Dictionary<uint, ListableItem>();
            iPlaylist = new List<MrItem>();
        }

        public void Dispose() { }

        public void SetPlaylist(IList<MrItem> aPlaylist)
        {
            iMutex.WaitOne();

            Clear();
            iPlaylist = aPlaylist;

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
            iPlaylist.Clear();
            iMutex.ReleaseMutex();
        }

        public void SetList(IList aList)
        {
            iParent = aList;
        }

        public uint Count
        {
            get
            {
                iMutex.WaitOne();
                uint count = (uint)iPlaylist.Count;
                iMutex.ReleaseMutex();
                return count;
            }
        }

        public int Index(MrItem aItem)
        {
            int index = 0;

            iMutex.WaitOne();

            foreach (MrItem i in iPlaylist)
            {
                if (i == aItem)
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

            if(iList.TryGetValue(aIndex, out item))
            {
                iMutex.ReleaseMutex();

                return item;
            }
            
            item = new ListableItem(iPlaylist[(int)aIndex]);
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

        private IList<MrItem> iPlaylist;
        private Dictionary<uint, ListableItem> iList;

        private IList iParent;
    }
} // KinskyClassic
