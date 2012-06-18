using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Threading;

using OpenHome.Songcast;

namespace Linn.Songcast
{
    public class Receiver : INotifyPropertyChanged
    {
        internal Receiver(IReceiver aReceiver)
        {
            iLock = new object();

            iReceiver = aReceiver;

            Init(aReceiver);
        }

        internal Receiver(string aUdn, string aRoom, string aGroup, string aName)
        {
            iLock = new object();

            iUdn = aUdn;
            iRoom = aRoom;
            iGroup = aGroup;
            iName = aName;

            iDescription = iRoom + " (" + iGroup + ")";
        }

        internal void SetReceiver(IReceiver aReceiver)
        {
            lock (iLock)
            {
                iReceiver = aReceiver;

                Init(aReceiver);

                Changed();
            }
        }

        private void Init(IReceiver aReceiver)
        {
            iUdn = aReceiver.Udn;
            iRoom = aReceiver.Room;
            iGroup = aReceiver.Group;
            iName = aReceiver.Name;

            iDescription = iReceiver.Room + " (" + iReceiver.Group + ")";
        }

        public void Play()
        {
            lock (iLock)
            {
                iReceiver.Play();
            }
        }

        public void Stop()
        {
            lock (iLock)
            {
                iReceiver.Stop();
            }
        }

        public void Standby()
        {
            lock (iLock)
            {
                iReceiver.Standby();
            }
        }

        public string Udn
        {
            get
            {
                lock (iLock)
                {
                    return (iUdn);
                }
            }
        }

        public string Room
        {
            get
            {
                lock (iLock)
                {
                    return (iRoom);
                }
            }
        }

        public string Group
        {
            get
            {
                lock (iLock)
                {
                    return (iGroup);
                }
            }
        }

        public string Name
        {
            get
            {
                lock (iLock)
                {
                    return (iName);
                }
            }
        }

        public EReceiverStatus Status
        {
            get
            {
                lock (iLock)
                {
                    return (iReceiver.Status);
                }
            }
        }

        public override string ToString()
        {
            lock (iLock)
            {
                return (iDescription);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal bool MapsTo(IReceiver aReceiver)
        {
            lock (iLock)
            {
                return (aReceiver == iReceiver);
            }
        }

        internal void Changed()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Status"));
            }
        }

        private object iLock;

        private IReceiver iReceiver;
        
        private string iUdn;
        private string iRoom;
        private string iGroup;
        private string iName;
        private string iDescription;
    }

    internal delegate void DelegateReceiverList(IReceiver aReceiver);

    public class ReceiverList : IReceiverHandler, IEnumerable, INotifyCollectionChanged
    {
        public ReceiverList(Dispatcher aDispatcher)
        {
            iDispatcher = aDispatcher;
            iList = new List<Receiver>();
        }

        public void ReceiverAdded(IReceiver aReceiver)
        {
            iDispatcher.BeginInvoke(new DelegateReceiverList(Added), new object[] { aReceiver });
        }

        internal void Add(Receiver aReceiver)
        {
            iList.Add(aReceiver);

            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, aReceiver));
            }
        }

        internal void Added(IReceiver aReceiver)
        {
            foreach (Receiver r in iList)
            {
                if (r.Udn == aReceiver.Udn)
                {
                    r.SetReceiver(aReceiver);
                    return;
                }
            }

            Receiver receiver = new Receiver(aReceiver);
            Add(receiver);
        }

        public void ReceiverChanged(IReceiver aReceiver)
        {
            iDispatcher.BeginInvoke(new DelegateReceiverList(Changed), new object[] { aReceiver });
        }

        internal void Changed(IReceiver aReceiver)
        {
            foreach (Receiver receiver in iList)
            {
                if (receiver.MapsTo(aReceiver))
                {
                    receiver.Changed();
                    return;
                }
            }
        }

        public void ReceiverRemoved(IReceiver aReceiver)
        {
            iDispatcher.BeginInvoke(new DelegateReceiverList(Removed), new object[] { aReceiver });
        }

        internal void Removed(IReceiver aReceiver)
        {
            int index = 0;

            foreach (Receiver receiver in iList)
            {
                if (receiver.MapsTo(aReceiver))
                {
                    iList.Remove(receiver);

                    if (CollectionChanged != null)
                    {
                        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, receiver, index));
                    }

                    return;
                }

                index++;
            }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return (iList.GetEnumerator());
        }
        

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        Dispatcher iDispatcher;
        List<Receiver> iList;
    }

}