using System;
using System.Threading;
using System.Collections.Generic;

using Linn.Gui.Scenegraph;
using Linn.Kinsky;

namespace KinskyClassic
{
    public class AdapterSource : IListEntryProvider
    {
        public class ListableSource : IListable
        {
            internal ListableSource(Linn.Kinsky.Source aSource)
            {
                iSource = aSource;
                iText = aSource.Name;
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

            public void Dispose()
            {
            }

            public Linn.Kinsky.Source Source
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

            private Linn.Kinsky.Source iSource;
            private string iText;
        }

        public class ListableSourceComparer : IComparer<ListableSource>
        {
            public int Compare(ListableSource x, ListableSource y)
            {
                return (x.Text.CompareTo(y.Text));
            }
        }

        public AdapterSource()
        {
            iMutex = new Mutex();
            iList = new List<ListableSource>();
            iComparer = new ListableSourceComparer();
        }

        public void Add(Linn.Kinsky.Source aSource)
        {
            iMutex.WaitOne();

            ListableSource listable = new ListableSource(aSource);
            iList.Add(listable);
            iList.Sort(iComparer);
            int index = iList.IndexOf(listable);

            iMutex.ReleaseMutex();

            if (iParent != null)
            {
                iParent.Add((uint)index, 1);
            }
        }

        public void Remove(Linn.Kinsky.Source aSource)
        {
            iMutex.WaitOne();

            int index = 0;
            foreach (ListableSource s in iList)
            {
                if (s.Source == aSource)
                {
                    iList.Remove(s);
                    break;
                }
                ++index;
            }

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

        public int Index(Linn.Kinsky.Source aSource)
        {
            int index = 0;

            iMutex.WaitOne();

            foreach (ListableSource s in iList)
            {
                if (s.Source == aSource)
                {
                    iMutex.ReleaseMutex();

                    return (index);
                }

                index++;
            }

            iMutex.ReleaseMutex();

            return -1;
        }
        /*public int CurrentIndex()
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
        }*/

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
