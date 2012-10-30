
using System;
using System.Net;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Linq;


namespace Linn.Songcast
{
    // Class to implement the data bindings between the preferences window and the preferences/model data
    public class PreferenceBindings : INotifyPropertyChanged
    {
        public PreferenceBindings(Model aModel, OptionPageUpdates aOptionPageUpdates)
        {
            iModel = aModel;
            iOptionPageUpdates = aOptionPageUpdates;
            iPreferences = iModel.Preferences;
            iReceiverList = new BindingList<ReceiverBinding>(new ReceiverEqualityComparer(), new ReceiverSortComparer());
            iSubnetList = new BindingList<SubnetBinding>(new SubnetEqualityComparer(), new SubnetSortComparer());

            iOptionPageUpdates.EventAutoUpdateChanged += OptionAutoUpdateChanged;

            // hook event handlers from model events
            iModel.EventEnabledChanged += ModelEnabledChanged;
            iModel.EventReceiverListChanged += ModelReceiverListChanged;
            iModel.EventSubnetListChanged += ModelSubnetListChanged;

            // hook event handlers from preferences events - these preferences can be changed by the
            // model - all other preferences can only be changed by this class so no need to handle events
            iPreferences.EventSelectedReceiverChanged += PreferencesSelectedReceiverChanged;
            iPreferences.EventSelectedSubnetChanged += PreferencesSelectedSubnetChanged;
        }

        public ObservableCollection<ReceiverBinding> ReceiverList
        {
            get { return iReceiverList; }
        }

        public int SelectedReceiverIndex
        {
            get
            {
                string udn = iPreferences.SelectedReceiverUdn;

                ReceiverBinding recv = iReceiverList.FirstOrDefault(r => r.Udn == udn);

                return (recv != null) ? iReceiverList.IndexOf(recv) : -1;
            }
            set
            {
                if (value >= 0 && value < iReceiverList.Count)
                {
                    iPreferences.SelectedReceiverUdn = iReceiverList[value].Udn;
                }
                else
                {
                    iPreferences.SelectedReceiverUdn = string.Empty;
                }
                Notify("SelectedReceiverIndex");
                Notify("ImageReceiverStatus");
                Notify("TextReceiverStatus");
            }
        }

        public ObservableCollection<SubnetBinding> SubnetList
        {
            get { return iSubnetList; }
        }

        public int SelectedSubnetIndex
        {
            get
            {
                uint addr = iPreferences.SelectedSubnetAddress;

                SubnetBinding subnet = iSubnetList.FirstOrDefault(s => s.Address == addr);

                return (subnet != null) ? iSubnetList.IndexOf(subnet) : -1;
            }
            set
            {
                if (value >= 0 && value < iSubnetList.Count)
                {
                    iPreferences.SelectedSubnetAddress = iSubnetList[value].Address;
                }
                else
                {
                    iPreferences.SelectedSubnetAddress = 0;
                }
                Notify("SelectedSubnetIndex");
            }
        }

        public ImageSource ImageReceiverStatus
        {
            get
            {
                if (iModel.Enabled)
                {
                    if (SelectedReceiverIndex != -1)
                    {
                        ReceiverBinding recv = iReceiverList[SelectedReceiverIndex];

                        if (recv.IsOnline && recv.Status == OpenHome.Songcast.EReceiverStatus.eConnected)
                        {
                            return ResourceManager.Connected;
                        }
                        else
                        {
                            return ResourceManager.Disconnected;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return ResourceManager.Disconnected;
                }
            }
        }

        public string TextReceiverStatus
        {
            get
            {
                if (iModel.Enabled)
                {
                    if (SelectedReceiverIndex != -1)
                    {
                        ReceiverBinding recv = iReceiverList[SelectedReceiverIndex];

                        if (recv.IsOnline)
                        {
                            switch (recv.Status)
                            {
                                case OpenHome.Songcast.EReceiverStatus.eConnected:
                                    return "Connected";
                                case OpenHome.Songcast.EReceiverStatus.eConnecting:
                                    return "Connecting";
                                case OpenHome.Songcast.EReceiverStatus.eDisconnected:
                                default:
                                    return "Disconnected";
                            }
                        }
                        else
                        {
                            return "Unavailable";
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return "Songcast Off";
                }
            }
        }

        public bool Unicast
        {
            get
            {
                return !Multicast;
            }
            set
            {
                Multicast = !value;
            }
        }

        public bool Multicast
        {
            get
            {
                return iPreferences.MulticastEnabled;
            }
            set
            {
                iPreferences.MulticastEnabled = value;
                Notify("Unicast");
                Notify("Multicast");
            }
        }

        public uint Channel
        {
            get
            {
                return iPreferences.MulticastChannel;
            }
            set
            {
                iPreferences.MulticastChannel = value;
                Notify("Channel");
            }
        }

        public uint MusicLatency
        {
            get
            {
                return iPreferences.MusicLatencyMs;
            }
            set
            {
                iPreferences.MusicLatencyMs = value;
                Notify("MusicLatency");
            }
        }

        public uint VideoLatency
        {
            get
            {
                return iPreferences.VideoLatencyMs;
            }
            set
            {
                iPreferences.VideoLatencyMs = value;
                Notify("VideoLatency");
            }
        }

        public bool RotaryVolumeControl
        {
            get
            {
                return iPreferences.RotaryVolumeControl;
            }
            set
            {
                iPreferences.RotaryVolumeControl = value;
                Notify("RotaryVolumeControl");
            }
        }

        public bool RockerVolumeControl
        {
            get
            {
                return !iPreferences.RotaryVolumeControl;
            }
            set
            {
                RotaryVolumeControl = !value;
                Notify("RockerVolumeControl");
            }
        }

        public bool AutomaticUpdateChecks
        {
            get
            {
                return iOptionPageUpdates.AutoUpdate;
            }
            set
            {
                iOptionPageUpdates.AutoUpdate = value;
                Notify("AutomaticUpdateChecks");
            }
        }

        public bool ParticipateInBeta
        {
            get
            {
                return iOptionPageUpdates.BetaVersions;
            }
            set
            {
                iOptionPageUpdates.BetaVersions = value;
                Notify("ParticipateInBeta");
            }
        }

        public bool UsageData
        {
            get
            {
                return iPreferences.UsageData;
            }
            set
            {
                iPreferences.UsageData = value;
                Notify("UsageData");
            }
        }

        private void Notify(string aName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(aName));
            }
        }

        private void OptionAutoUpdateChanged(object sender, EventArgs e)
        {
            Notify("AutomaticUpdateChecks");
        }

        private void ModelEnabledChanged(object sender, EventArgs e)
        {
            Notify("ImageReceiverStatus");
            Notify("TextReceiverStatus");
        }

        private void ModelReceiverListChanged(object sender, EventArgs e)
        {
            // create a new list of binding receivers from the receivers in the model
            List<ReceiverBinding> newList = new List<ReceiverBinding>();

            foreach (Receiver r in iModel.ReceiverList)
            {
                newList.Add(new ReceiverBinding(r));
            }

            // update the bindings receiver list
            iReceiverList.Update(newList);

            // update relevant bits of the preferences UI
            Notify("SelectedReceiverIndex");
            Notify("ImageReceiverStatus");
            Notify("TextReceiverStatus");
        }

        private void ModelSubnetListChanged(object sender, EventArgs e)
        {
            // create a new list of binding subnets from the subnets in the model
            List<SubnetBinding> newList = new List<SubnetBinding>();

            foreach (OpenHome.Songcast.ISubnet s in iModel.SubnetList)
            {
                newList.Add(new SubnetBinding(s));
            }

            // update the bindings subnet list
            iSubnetList.Update(newList);

            // update relevant bits of the preferences UI
            Notify("SelectedSubnetIndex");
        }

        private void PreferencesSelectedReceiverChanged(object sender, EventArgs e)
        {
            Notify("SelectedReceiverIndex");
            Notify("ImageReceiverStatus");
            Notify("TextReceiverStatus");
        }

        private void PreferencesSelectedSubnetChanged(object sender, EventArgs e)
        {
            Notify("SelectedSubnetIndex");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private Preferences iPreferences;
        private OptionPageUpdates iOptionPageUpdates;
        private Model iModel;
        private BindingList<ReceiverBinding> iReceiverList;
        private BindingList<SubnetBinding> iSubnetList;
    }


    // Generic item and list class used in the bindings
    public interface IBindingItem<T>
    {
        void Update(T aItem);
    }


    public class BindingList<T> : ObservableCollection<T> where T : IBindingItem<T>
    {
        public BindingList(IEqualityComparer<T> aEqComparer, IComparer<T> aSortComparer)
        {
            iEqComparer = aEqComparer;
            iSortComparer = aSortComparer;
        }

        private IEqualityComparer<T> iEqComparer;
        private IComparer<T> iSortComparer;

        public void Update(IEnumerable<T> aList)
        {
            // get the list of items in 'aList' that are NOT in 'this' - create an explicit list since the 'Except' method uses
            // deferred execution - this means that you cannot change the lists used in the 'Except' method while iterating over
            // the result
            List<T> toAdd = new List<T>();
            foreach (T item in aList.Except(this, iEqComparer))
            {
                toAdd.Add(item);
            }

            // get the list of items in 'this' that are NOT in 'aList'
            List<T> toRemove = new List<T>();
            foreach (T item in this.Except(aList, iEqComparer))
            {
                toRemove.Add(item);
            }

            // update items that are in 'aList' and 'this'
            foreach (T newItem in aList)
            {
                foreach (T existingItem in this)
                {
                    if (iEqComparer.Equals(newItem, existingItem))
                    {
                        existingItem.Update(newItem);
                        break;
                    }
                }
            }

            // remove items
            foreach (T item in toRemove)
            {
                this.Remove(item);
            }

            // add items
            foreach (T item in toAdd)
            {
                bool added = false;

                for (int i = 0; i < this.Count; i++)
                {
                    if (iSortComparer.Compare(this[i], item) > 0)
                    {
                        this.Insert(i, item);
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    this.Add(item);
                }
            }
        }
    }


    // class to handle the data bindings for a receiver
    public class ReceiverBinding : INotifyPropertyChanged, IBindingItem<ReceiverBinding>
    {
        public ReceiverBinding(Receiver aReceiver)
        {
            iReceiver = aReceiver;
        }

        public string Udn
        {
            get { return iReceiver.Udn; }
        }

        public OpenHome.Songcast.EReceiverStatus Status
        {
            get { return iReceiver.Status; }
        }

        public bool IsOnline
        {
            get { return iReceiver.IsOnline; }
        }

        public string Description
        {
            get { return string.Format("{0} ({1})", iReceiver.Room, iReceiver.Group); }
        }

        public void Update(ReceiverBinding aReceiver)
        {
            string desc = Description;

            iReceiver = aReceiver.iReceiver;

            if (desc != Description && PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Description"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private Receiver iReceiver;
    }

    public class ReceiverEqualityComparer : EqualityComparer<ReceiverBinding>
    {
        public override bool Equals(ReceiverBinding x, ReceiverBinding y)
        {
            return x.Udn == y.Udn;
        }

        public override int GetHashCode(ReceiverBinding obj)
        {
            return obj.Udn.GetHashCode();
        }
    }

    public class ReceiverSortComparer : IComparer<ReceiverBinding>
    {
        public int Compare(ReceiverBinding x, ReceiverBinding y)
        {
            return x.Description.CompareTo(y.Description);
        }
    }


    // class to handle the data bindings for a subnet
    public class SubnetBinding : INotifyPropertyChanged, IBindingItem<SubnetBinding>
    {
        public SubnetBinding(OpenHome.Songcast.ISubnet aSubnet)
        {
            iSubnet = aSubnet;
            iDescription = string.Format("{0} ({1})", new IPAddress(iSubnet.Address), iSubnet.AdapterName);
        }

        public uint Address
        {
            get { return iSubnet.Address; }
        }

        public string Description
        {
            get { return iDescription; }
        }

        public void Update(SubnetBinding aSubnet)
        {
            string desc = Description;

            iSubnet = aSubnet.iSubnet;

            iDescription = string.Format("{0} ({1})", new IPAddress(iSubnet.Address), iSubnet.AdapterName);

            if (desc != Description && PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Description"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private OpenHome.Songcast.ISubnet iSubnet;
        private string iDescription;
    }

    public class SubnetEqualityComparer : EqualityComparer<SubnetBinding>
    {
        public override bool Equals(SubnetBinding x, SubnetBinding y)
        {
            return x.Address == y.Address;
        }

        public override int GetHashCode(SubnetBinding obj)
        {
            return obj.Address.GetHashCode();
        }
    }

    public class SubnetSortComparer : IComparer<SubnetBinding>
    {
        public int Compare(SubnetBinding x, SubnetBinding y)
        {
            return x.Description.CompareTo(y.Description);
        }
    }
}




