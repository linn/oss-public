using System;
using System.Collections.Generic;
using Linn.Gui.Scenegraph;
using Linn.Topology;

namespace Linn.Kinsky
{

    class AdapterRoom : IListEntryProvider
    {
        public class ListableRoom : IListable
        {
            internal ListableRoom(Room aRoom)
            {
                iRoom = aRoom;
                iAdapterSource = new AdapterSource(iRoom);
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

            public int State {
                get { return 0; }
                set {}
            }

	    public NodeHit NodeHit {
                get { return null; }
                set {}
            }

            public Room Room
            {
                get
                {
                    return (iRoom);
                }
            }

            public AdapterSource AdapterSource
            {
                get
                {
                    return (iAdapterSource);
                }
            }


            private Room iRoom;
            private AdapterSource iAdapterSource;
        }

        public AdapterRoom(House aHouse)
        {
            iHouse = aHouse;
            iHouse.EventRoomAdded += EventRoomAdded;
            iHouse.EventRoomRemoved += EventRoomRemoved;
            iList = new SortedList<string, ListableRoom>();
        }

        private void EventRoomAdded(object obj, House.EventArgsRoom e)
        {
            iList.Add(e.Room.Name, new ListableRoom(e.Room));

            if (EventChanged != null)
            {
                EventChanged(this, EventArgs.Empty);
            }
        }

        private void EventRoomRemoved(object obj, House.EventArgsRoom e)
        {
            iList.Remove(e.Room.Name);

            if (EventChanged != null)
            {
                EventChanged(this, EventArgs.Empty);
            }
        }

        public EventHandler<EventArgs> EventChanged;

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

        private IList iParent;
        private House iHouse;
        private SortedList<string, ListableRoom> iList;
    }

}
