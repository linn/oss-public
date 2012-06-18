using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Threading;

using OpenHome.Songcast;

namespace Linn.Songcast
{
    public class Subnet : INotifyPropertyChanged
    {
        internal Subnet(ISubnet aSubnet)
        {
            iSubnet = aSubnet;
            
            IPAddress address = new IPAddress(iSubnet.Address);
            
            iDescription = address + " (" + iSubnet.AdapterName + ")";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal bool MapsTo(ISubnet aSubnet)
        {
            return (aSubnet == iSubnet);
        }

        public uint Address
        {
            get
            {
                return (iSubnet.Address);
            }
        }

        public string Description
        {
            get
            {
                return (iDescription);
            }
        }

        internal void Changed()
        {
            IPAddress address = new IPAddress(iSubnet.Address);

            iDescription = address + " (" + iSubnet.AdapterName + ")";

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Description"));
            }
        }

        private ISubnet iSubnet;
        private string iDescription;
    }

    internal delegate void DelegateSubnetList(ISubnet aSubnet);

    public class SubnetList : ISubnetHandler, IEnumerable, INotifyCollectionChanged
    {
        public SubnetList(Dispatcher aDispatcher)
        {
            iDispatcher = aDispatcher;
            iList = new List<Subnet>();
        }

        public Subnet SubnetAt(int aIndex)
        {
            return (iList[aIndex]);
        }


        public void SubnetAdded(ISubnet aSubnet)
        {
            iDispatcher.BeginInvoke(new DelegateSubnetList(Added), new object[] { aSubnet });
        }

        internal void Added(ISubnet aSubnet)
        {
            Subnet subnet = new Subnet(aSubnet);

            iList.Add(subnet);

            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, subnet));
            }

            if (CountChanged != null)
            {
                CountChanged(this, EventArgs.Empty);
            }
        }

        public void SubnetChanged(ISubnet aSubnet)
        {
            iDispatcher.BeginInvoke(new DelegateSubnetList(Changed), new object[] { aSubnet });
        }

        internal void Changed(ISubnet aSubnet)
        {
            foreach (Subnet subnet in iList)
            {
                if (subnet.MapsTo(aSubnet))
                {
                    subnet.Changed();
                    return;
                }
            }
        }

        public void SubnetRemoved(ISubnet aSubnet)
        {
            iDispatcher.BeginInvoke(new DelegateSubnetList(Removed), new object[] { aSubnet });
        }

        internal void Removed(ISubnet aSubnet)
        {
            /* Do nothing on subnet removal
             *  - probably better to present users with a list of all the subnets they have had around since turning on
             *  - definitely better on closing down - all subnets are removed but we don't want the combobox to end up setting our configured subnet to 0 in the process
             */
            /*  
            int index = 0;

            foreach (Subnet subnet in iList)
            {
                if (subnet.MapsTo(aSubnet))
                {
                    iList.Remove(subnet);

                    if (CollectionChanged != null)
                    {
                        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, subnet, index));
                    }

                    if (CountChanged != null)
                    {
                        CountChanged(this, EventArgs.Empty);
                    }

                    return;
                }

                index++;
            }
            */
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return (iList.GetEnumerator());
        }

        public int Count
        {
            get
            {
                return (iList.Count);
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public EventHandler<EventArgs> CountChanged;

        Dispatcher iDispatcher;

        List<Subnet> iList;
    }

}