using System;
using System.Threading;
using System.Collections.Generic;

using Linn;
using Linn.Gui.Scenegraph;

using Upnp;
using Linn.Topology;

namespace KinskyClassic
{
    class AdapterRadio : IListEntryProvider
    {
        public class ListableItem : IListable
        {
            internal ListableItem(MrItem aPreset)
            {
                iPreset = aPreset;
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

            public MrItem Preset
            {
                get
                {
                    return iPreset;
                }
            }


            private MrItem iPreset;
        }

        public AdapterRadio()
        {
            iMutex = new Mutex(false);
            iList = new Dictionary<uint, ListableItem>();
            iPresets = new List<MrItem>();
        }

        public void Dispose() { }

        public void SetPresets(IList<MrItem> aPresets)
        {
            iMutex.WaitOne();

            Clear();
            iPresets = aPresets;

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
            iPresets.Clear();
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
                uint count = (uint)iPresets.Count;
                iMutex.ReleaseMutex();
                return count;
            }
        }

        public int Index(MrItem aItem)
        {
            int index = 0;

            iMutex.WaitOne();

            foreach (MrItem i in iPresets)
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

            item = new ListableItem(iPresets[(int)aIndex]);
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

        private IList<MrItem> iPresets;
        private Dictionary<uint, ListableItem> iList;

        private IList iParent;
    }
} // KinskyClassic
