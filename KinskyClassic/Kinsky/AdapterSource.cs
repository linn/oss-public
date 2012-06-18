using System;
using System.Threading;
using System.Collections.Generic;
using Linn.Gui.Scenegraph;
using Linn.Topology;

namespace Linn.Kinsky
{

    public class AdapterSource : IListEntryProvider
    {
        public class ListableSource : IListable
        {
            internal ListableSource(Source aSource)
            {
                iSource = aSource;

                iText = aSource.Name;

                SourceProduct p = aSource as SourceProduct;

                if (p != null)
                {
                    iText += " (Volkano " + iSource.Type + ")";
                    return;
                }

                SourceUpnp u = aSource as SourceUpnp;

                if (u != null)
                {
                    iText += " (Upnp " + iSource.Type + ")";
                    return;
                }

                SourceAux a = aSource as SourceAux;

                if (a != null)
                {
                    iText += " (Aux " + iSource.Type + ")";
                    return;
                }

                iText += " (Unknown " + iSource.Type + ")";
            }

            public void Highlight()
            {
            }

            public void UnHighlight()
            {
            }

            public void Select()
            {
                iSource.Select();
            }

            public void UnSelect()
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

            public void Dispose()
            {
            }

            public Source Source
            {
                get
                {
                    return (iSource);
                }
            }

            public string Text
            {
                get
                {
                    return (iText);
                }
            }

            private Source iSource;
            private string iText;
        }

        public class ListableSourceComparer : IComparer<ListableSource>
        {
            public int Compare(ListableSource x, ListableSource y)
            {
                return (x.Text.CompareTo(y.Text));
            }
        }

        public AdapterSource(Room aRoom)
        {
            iRoom = aRoom;
            iRoom.EventSourceAdded += EventSourceAdded;
            iRoom.EventSourceRemoved += EventSourceRemoved;
            iRoom.EventSourcesChanged += EventSourcesChanged;
            iMutex = new Mutex();
            iList = new List<ListableSource>();
            iComparer = new ListableSourceComparer();
        }

        private void EventSourceAdded(object obj, Room.EventArgsSource e)
        {
            iMutex.WaitOne();

            iList.Add(new ListableSource(e.Source));

            iList.Sort(iComparer);

            iMutex.ReleaseMutex();
        }

        private void EventSourceRemoved(object obj, Room.EventArgsSource e)
        {
            iMutex.WaitOne();

            foreach (ListableSource s in iList)
            {
                if (s.Source == e.Source)
                {
                    iList.Remove(s);
                    break;
                }
            }

            iMutex.ReleaseMutex();
        }

        private void EventSourcesChanged(object obj, EventArgs e)
        {
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

        public Room Room
        {
            get
            {
                return (iRoom);
            }
        }

        public int CurrentIndex()
        {
            Source current = iRoom.Current;

            if (current == null)
            {
                return (-1);
            }

            int index = 0;

            foreach (ListableSource s in iList)
            {
                if (s.Source == current)
                {
                    return (index);
                }

                index++;
            }

            return (-1);
        }

        public IListable Entry(uint aIndex)
        {
            return (iList[(int)aIndex]);
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
        private Room iRoom;
        private Mutex iMutex;
        private List<ListableSource> iList;
        private ListableSourceComparer iComparer;
    }

}
