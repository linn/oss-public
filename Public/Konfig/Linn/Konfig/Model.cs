using System;
using System.Collections.Generic;

using Linn;

using OpenHome.Net.Core;

namespace Linn.Konfig
{
    public interface IUpdateListener
    {
        void SetUpdating(bool aUpdating);
    }

    public class UpdateListenerRepeater : IUpdateListener
    {
        public UpdateListenerRepeater(IEnumerable<IUpdateListener> aList)
        {
            iUpdateListeners = new List<IUpdateListener>(aList);
        }

        public void SetUpdating(bool aUpdating)
        {
            foreach (IUpdateListener l in iUpdateListeners)
            {
                l.SetUpdating(aUpdating);
            }
        }

        private List<IUpdateListener> iUpdateListeners;
    }

    public interface IAdapterList
    {
        uint Current { get; }
        NetworkAdapter Adapter { get; }
        IEnumerable<uint> Subnets { get; }
        IEnumerable<string> Adapters { get; }
    }

    public interface INetworkManager
    {
        IAdapterList AdapterList { get; }
        void SetSubnet(uint aSubnet);
        event EventHandler<EventArgs> AdapterListChanged;
    }

    public class AdapterList : IAdapterList, IDisposable
    {
        public AdapterList(uint aSubnet)
        {
            using(SubnetList subnets = new SubnetList())
            {
                bool found = false;

                uint count = subnets.Size();

                iSubnets = new List<uint>();
                iAdapters = new List<string>();

                for(uint i = 0; i < count; ++i)
                {
                    NetworkAdapter adapter = subnets.SubnetAt(i);

                    iSubnets.Add(adapter.Subnet());
                    iAdapters.Add(adapter.Name());

                    if(!found)
                    {
                        if(adapter.Subnet() == aSubnet)
                        {
                            found = true;
                            iCurrent = i;
                            iAdapter = adapter;
                            iAdapter.AddRef("Konfig-Adapter-List");
                        }
                    }
                }

                if(!found)
                {
                    if(aSubnet == 0 || !found)
                    {
                        // uninitialised or not found

                        if(count > 0)
                        {
                            iCurrent = 0;
                            iAdapter = subnets.SubnetAt(0);
                            iAdapter.AddRef("Konfig-Adapter-List");
                        }
                    }

                    if (aSubnet != 0)
                    {
                        iSubnets.Add(aSubnet);
                        iAdapters.Add("Network not present");
                    }
                }
            }
        }

        #region IDisposable implementation
        public void Dispose ()
        {
            if(iAdapter != null)
            {
                iAdapter.RemoveRef("Konfig-Adapter-List");
                iAdapter = null;
            }
        }
        #endregion

        #region IAdapterList implementation
        public uint Current
        {
            get
            {
                return iCurrent;
            }
        }

        public NetworkAdapter Adapter
        {
            get
            {
                return iAdapter;
            }
        }

        public IEnumerable<uint> Subnets
        {
            get
            {
                return iSubnets;
            }
        }

        public IEnumerable<string> Adapters
        {
            get
            {
                return iAdapters;
            }
        }
        #endregion

        private uint iCurrent;
        private List<uint> iSubnets;
        private List<string> iAdapters;
        private NetworkAdapter iAdapter;
    }

    public class Model : INetworkManager, IDisposable
    {
        public static string kOnlineManualUri = "http://oss.linn.co.uk/trac/wiki/Konfig_DavaarManual";

        private static Model iInstance;
        public static Model Instance
        {
            get
            {
                Assert.Check(iInstance != null);
                return iInstance;
            }
            set
            {
                Assert.Check(iInstance == null);
                iInstance = value;
            }
        }

        public Model(Preferences aPreferences)
        {
            iLock = new object();

            iPreferences = aPreferences;
            iPreferences.EventNetworkChanged += HandleEventNetworkChanged;

            Library.SetDebugLevel(Library.DebugLevel.Trace);

            iInitParams = new InitParams();
            iInitParams.LogOutput = new MessageListener(LogOutput);
            iInitParams.SubnetListChangedListener = new ChangedListener(SubnetListChanged);

            iLibrary = Library.Create(iInitParams);

            iSubnet = iPreferences.Network;

            iAdapterList = new AdapterList(iSubnet);

            if(iAdapterList.Adapter != null)
            {
                iSubnet = iAdapterList.Adapter.Subnet();
            }

            iLibrary.StartCp(iSubnet);
        }

        internal Preferences Preferences { get { return iPreferences; } }

        void HandleEventNetworkChanged(object sender, EventArgs e)
        {
            UpdateSubnet(iPreferences.Network);
        }

        public void Dispose()
        {
            if(iAdapterList != null)
            {
                iAdapterList.Dispose();
                iAdapterList = null;
            }

            if(iLibrary != null)
            {
                iLibrary.Dispose();
                iLibrary = null;
            }

            if(iPreferences != null)
            {
                iPreferences.EventNetworkChanged -= HandleEventNetworkChanged;
                iPreferences = null;
            }
        }

        #region INetworkManager implementation
        public event EventHandler<EventArgs> AdapterListChanged;

        public void SetSubnet(uint aSubnet)
        {
            if(aSubnet == iSubnet)
            {
                return;
            }

            iPreferences.Network = aSubnet;

            UpdateSubnet(aSubnet);
        }

        private void UpdateSubnet(uint aSubnet)
        {
            NetworkAdapter adapter = null;
            
            lock(iLock)
            {
                if (iAdapterList != null)
                {
                    iAdapterList.Dispose();
                }
                
                iAdapterList = new AdapterList(aSubnet);
                
                adapter = iAdapterList.Adapter;
                
                iSubnet = aSubnet;
            }
            
            iLibrary.SetCurrentSubnet(adapter);
            
            if(AdapterListChanged != null)
            {
                AdapterListChanged(this, EventArgs.Empty);
            }
        }

        public IAdapterList AdapterList
        {
            get
            {
                lock(iLock)
                {
                    return iAdapterList;
                }
            }
        }
        #endregion

        private void SubnetListChanged()
        {
            UpdateSubnet(iPreferences.Network);
        }

        private void LogOutput(string aMessage)
        {
            UserLog.Write(aMessage);
        }

        private object iLock;

        private Preferences iPreferences;

        private InitParams iInitParams;
        private Library iLibrary;

        private AdapterList iAdapterList;
        private uint iSubnet;
    }
}

