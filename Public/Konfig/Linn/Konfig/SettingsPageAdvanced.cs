using System;
using System.Collections.Generic;

using OpenHome.Net.Core;
using OpenHome.Xapp;

using Linn.Toolkit;

namespace Linn.Konfig
{
    public class SettingsPageAdvanced : SettingsPageBasic, IUpdateListener
    {
        public SettingsPageAdvanced(IInvoker aInvoker, INetworkManager aNetworkManager, Preferences aPreferences, HelperAutoUpdate aHelperAutoUpdate, string aId, string aViewId)
            : base(aInvoker, aPreferences, aId, aViewId)
        {
            iInvoker = aInvoker;

            iHelperAutoUpdate = aHelperAutoUpdate;
            iHelperAutoUpdate.OptionPageUpdates.EventAutoUpdateChanged += HandleAutoUpdateChanged;
            iHelperAutoUpdate.OptionPageUpdates.EventBetaVersionsChanged += HandleBetaVersionsChanged;

            iNetworkManager = aNetworkManager;
            iNetworkManager.AdapterListChanged += AdapterListChanged;
            iAdapterList = iNetworkManager.AdapterList;
        }

        public override void Dispose()
        {
            base.Dispose();

            iNetworkManager.AdapterListChanged -= AdapterListChanged;

            iHelperAutoUpdate.OptionPageUpdates.EventAutoUpdateChanged -= HandleAutoUpdateChanged;
            iHelperAutoUpdate.OptionPageUpdates.EventBetaVersionsChanged -= HandleBetaVersionsChanged;
        }

        public override void SetUpdating(bool aUpdating)
        {
            base.SetUpdating(aUpdating);

            lock (iLock)
            {
                Send("EnableNetworkSettings", aUpdating);
            }
        }

        private delegate void DOnActivated(Session aSession);
        protected override void OnActivated(Session aSession)
        {
            if (iInvoker.TryBeginInvoke(new DOnActivated(OnActivated), aSession))
                return;

            base.OnActivated(aSession);

            lock(iLock)
            {
                aSession.Send("EnableAdvancedSettings", true);
                aSession.Send("EnableNetworkSettings", !iUpdating);
                aSession.Send("SetNetworks", GetNetworks());
                aSession.Send("SetAutoUpdate", iHelperAutoUpdate.OptionPageUpdates.AutoUpdate);
                aSession.Send("SetBeta", iHelperAutoUpdate.OptionPageUpdates.BetaVersions);
            }
        }

        private delegate void DOnReceive(Session aSession, string aName, string aValue);
        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            if (iInvoker.TryBeginInvoke(new DOnReceive(OnReceive), aSession, aName, aValue))
                return;

            base.OnReceive(aSession, aName, aValue);

            if(aName == "Network")
            {
                OnReceiveNetwork(aValue);
            }

            if(aName == "Beta")
            {
                iHelperAutoUpdate.OptionPageUpdates.BetaVersions = bool.Parse(aValue);
            }

            if(aName == "AppUpdateChecks")
            {
                iHelperAutoUpdate.OptionPageUpdates.AutoUpdate = bool.Parse(aValue);
            }

            if(aName == "CheckForUpdates")
            {
                iHelperAutoUpdate.CheckForUpdates();
            }
        }

        private void OnReceiveNetwork(string aNetwork)
        {
            try
            {
                int index = int.Parse(aNetwork);

                lock(iLock)
                {
                    int i = 0;
                    foreach(uint subnet in iAdapterList.Subnets)
                    {
                        if(i == index)
                        {
                            iNetworkManager.SetSubnet(subnet);
                        }
                        ++i;
                    }
                }
            }
            catch(FormatException) { }
        }

        private void AdapterListChanged(object sender, EventArgs e)
        {
            lock(iLock)
            {
                iAdapterList = iNetworkManager.AdapterList;
                Send ("SetNetworks", GetNetworks());
            }
        }

        private JsonObject GetNetworks()
        {
            JsonArray<JsonValueString> array = new JsonArray<JsonValueString>();

            int i = 0;
            List<string> adapters = new List<string>(iAdapterList.Adapters);
            foreach(uint subnet in iAdapterList.Subnets)
            {

                array.Add(new JsonValueString(string.Format("{0} ({1})", new System.Net.IPAddress(subnet), adapters[i])));
                ++i;
            }

            JsonObject json = new JsonObject();

            json.Add("Current", new JsonValueUint(iAdapterList.Current));
            json.Add("Networks", array);

            return json;
        }

        private void HandleAutoUpdateChanged(object sender, EventArgs e)
        {
            Send("SetAutoUpdate", iHelperAutoUpdate.OptionPageUpdates.AutoUpdate);
        }

        private void HandleBetaVersionsChanged(object sender, EventArgs e)
        {
            Send("SetBeta", iHelperAutoUpdate.OptionPageUpdates.BetaVersions);
        }

        private HelperAutoUpdate iHelperAutoUpdate;

        private IInvoker iInvoker;
        private INetworkManager iNetworkManager;
        private IAdapterList iAdapterList;
    }
}
