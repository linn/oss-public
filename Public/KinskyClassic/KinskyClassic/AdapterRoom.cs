using System;
using System.Threading;
using System.Collections.Generic;
using Linn.Gui.Scenegraph;
using Linn.Topology;

namespace KinskyClassic
{

    class AdapterRoom : IListEntryProvider
    {
        public class ListableRoom : IListable
        {
            internal ListableRoom(Linn.Kinsky.Room aRoom)
            {
                iRoom = aRoom;
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

            public Linn.Kinsky.Room Room
            {
                get
                {
                    return (iRoom);
                }
            }


            private Linn.Kinsky.Room iRoom;
        }

        public AdapterRoom()
        {
            iMutex = new Mutex(false);
            iList = new SortedList<string, ListableRoom>();
        }

        public void Add(Linn.Kinsky.Room aRoom)
        {
            iMutex.WaitOne();
            iList.Add(aRoom.Name, new ListableRoom(aRoom));
            int index = iList.IndexOfKey(aRoom.Name);
            Console.WriteLine("Add:" + aRoom + ", " + index);
            iMutex.ReleaseMutex();

            if (iParent != null)
            {
                iParent.Add((uint)index, 1);
            }
        }

        public void Remove(Linn.Kinsky.Room aRoom)
        {
            iMutex.WaitOne();
            int index = iList.IndexOfKey(aRoom.Name);
            iList.Remove(aRoom.Name);
            iMutex.ReleaseMutex();

            if (iParent != null)
            {
                iParent.Delete((uint)index, 1);
            }
        }

        public void Clear()
        {
            if (iParent != null)
            {
                iParent.Clear();
            }

            iMutex.WaitOne();
            iList.Clear();
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
                return ((uint)iList.Count);
            }
        }

        public int Index(Linn.Kinsky.Room aRoom)
        {
            int index = 0;

            iMutex.WaitOne();

            foreach (ListableRoom r in iList.Values)
            {
                if (r.Room == aRoom)
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
            return (iList.Values[(int)aIndex]);
        }

        public List<IListable> Entries(uint aStartIndex, uint aCount)
        {
            List<IListable> list = new List<IListable>();

            for (uint i = 0; i < aCount; i++)
            {
                list.Add(Entry(aStartIndex + i));
            }

            return (list);
        }

        public void Dispose()
        {
        }

        private Mutex iMutex;
        private IList iParent;
        private SortedList<string, ListableRoom> iList;
    }

}
