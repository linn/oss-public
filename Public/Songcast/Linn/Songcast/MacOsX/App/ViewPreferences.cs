
using System;
using System.Collections.Generic;

using MonoMac;
using MonoMac.ObjCRuntime;
using MonoMac.Foundation;

using OpenHome.Songcast;
using Linn.Toolkit;


namespace Linn.Songcast
{
    // Class for the preferences 'view' - not a real view, but a view in another process - this class handles the comminication
    // required. The preference pane app and this app communicate by changing the preferences file (accessed via the NSUserDefaults
    // class) and then sending distributed notifications using the NSDistributedNotificationCenter
    public class ViewPreferences : NSObject
    {
        private const string kPreferenceEnabled = "Enabled";
        private const string kPreferenceReceiverList = "ReceiverList";
        private const string kPreferenceSubnetList = "SubnetList";
        private const string kPreferenceSelectedReceiverUdn = "SelectedReceiverUdn";
        private const string kPreferenceSelectedSubnetAddress = "SelectedSubnetAddress";
        private const string kPreferenceMulticastEnabled = "MulticastEnabled";
        private const string kPreferenceMulticastChannel = "MulticastChannel";
        private const string kPreferenceMusicLatencyMs = "MusicLatencyMs";
        private const string kPreferenceVideoLatencyMs = "VideoLatencyMs";
        private const string kPreferenceRotaryVolumeControl = "RotaryVolumeControl";
        private const string kPreferenceAutoUpdatesEnabled = "AutoUpdatesEnabled";
        private const string kPreferenceBetaUpdatesEnabled = "BetaUpdatesEnabled";
        
        public ViewPreferences(IInvoker aInvoker, Model aModel, HelperAutoUpdate aHelperAutoUpdate)
        {
            iInvoker = aInvoker;
            iAppId = "uk.co.linn.songcast.app";
            iModel = aModel;
            iHelperAutoUpdate = aHelperAutoUpdate;
            
            // register the preference keys and default values
            object[] keys = new object[] { kPreferenceEnabled,
                                           kPreferenceReceiverList,
                                           kPreferenceSubnetList,
                                           kPreferenceSelectedReceiverUdn,
                                           kPreferenceSelectedSubnetAddress,
                                           kPreferenceMulticastEnabled,
                                           kPreferenceMulticastChannel,
                                           kPreferenceMusicLatencyMs,
                                           kPreferenceVideoLatencyMs,
                                           kPreferenceRotaryVolumeControl,
                                           kPreferenceAutoUpdatesEnabled,
                                           kPreferenceBetaUpdatesEnabled };
            object[] vals = new object[] { false,
                                           new NSArray(),
                                           new NSArray(),
                                           string.Empty,
                                           0,
                                           false,
                                           0,
                                           iModel.Preferences.DefaultMusicLatencyMs,
                                           iModel.Preferences.DefaultVideoLatencyMs,
                                           true,
                                           true,
                                           false };
            
            NSUserDefaults.StandardUserDefaults.RegisterDefaults(NSDictionary.FromObjectsAndKeys(vals, keys));
            
            // check the multicast channel - if it is currently the default (0), set it to a random value - this ensures that this
            // preference is always set to a valid value
            if (GetIntegerValue(kPreferenceMulticastChannel) == 0)
            {
                Random r = new Random();
                int byte1 = r.Next(254) + 1;    // in range [1,254]
                int byte2 = r.Next(254) + 1;    // in range [1,254]
                int channel = byte1 << 8 | byte2;
                NSUserDefaults.StandardUserDefaults.SetInt(channel, kPreferenceMulticastChannel);
            }

            // add observers for the distributed notifications from the preferences pane app
            NSDistributedNotificationCenter centre = NSDistributedNotificationCenter.DefaultCenter as NSDistributedNotificationCenter;            
            NSString prefAppId = new NSString("uk.co.linn.songcast.prefs");

            centre.AddObserver(this, new Selector("selectedReceiverChanged:"), new NSString("PreferenceSelectedReceiverChanged"), prefAppId);
            centre.AddObserver(this, new Selector("selectedSubnetChanged:"), new NSString("PreferenceSelectedSubnetChanged"), prefAppId);
            centre.AddObserver(this, new Selector("multicastEnabledChanged:"), new NSString("PreferenceMulticastEnabledChanged"), prefAppId);
            centre.AddObserver(this, new Selector("multicastChannelChanged:"), new NSString("PreferenceMulticastChannelChanged"), prefAppId);
            centre.AddObserver(this, new Selector("musicLatencyMsChanged:"), new NSString("PreferenceMusicLatencyMsChanged"), prefAppId);
            centre.AddObserver(this, new Selector("videoLatencyMsChanged:"), new NSString("PreferenceVideoLatencyMsChanged"), prefAppId);
            centre.AddObserver(this, new Selector("volumeControlClicked:"), new NSString("PreferenceRotaryVolumeControlChanged"), prefAppId);
            centre.AddObserver(this, new Selector("autoUpdatesEnabledChanged:"), new NSString("PreferenceAutoUpdatesEnabledChanged"), prefAppId);
            centre.AddObserver(this, new Selector("betaUpdatesEnabledChanged:"), new NSString("PreferenceBetaUpdatesEnabledChanged"), prefAppId);
            centre.AddObserver(this, new Selector("refreshReceiverListClicked:"), new NSString("RefreshReceiverList"), prefAppId);
            centre.AddObserver(this, new Selector("checkForUpdatesClicked:"), new NSString("CheckForUpdates"), prefAppId);
            
            // hook up event handlers from model events - these changes in the model are reflected in the preference pane
            iHelperAutoUpdate.OptionPageUpdates.EventChanged += OptionPageUpdatesChanged;
            iModel.EventEnabledChanged += ModelEnabledChanged;
            iModel.EventReceiverListChanged += ModelReceiverListChanged;
            iModel.EventSubnetListChanged += ModelSubnetListChanged;
            
            // hook up event handlers from preferences events - the model can change these preferences - all other preferences
            // are only changed by the preference pane
            iModel.Preferences.EventSelectedReceiverChanged += PreferencesSelectedReceiverChanged;
            iModel.Preferences.EventSelectedSubnetChanged += PreferencesSelectedSubnetChanged;

            // initialise the view with current values of defaults
            NSUserDefaults.StandardUserDefaults.SetBool(iModel.Enabled, kPreferenceEnabled);
            NSUserDefaults.StandardUserDefaults[kPreferenceReceiverList] = BuildReceiverList();
            NSUserDefaults.StandardUserDefaults[kPreferenceSubnetList] = BuildSubnetList();
            NSUserDefaults.StandardUserDefaults.SetString(iModel.Preferences.SelectedReceiverUdn, kPreferenceSelectedReceiverUdn);
            NSUserDefaults.StandardUserDefaults.SetInt((int)iModel.Preferences.SelectedSubnetAddress, kPreferenceSelectedSubnetAddress);
            NSUserDefaults.StandardUserDefaults.SetBool(iModel.Preferences.MulticastEnabled, kPreferenceMulticastEnabled);
            NSUserDefaults.StandardUserDefaults.SetInt((int)iModel.Preferences.MulticastChannel, kPreferenceMulticastChannel);

            NSUserDefaults.StandardUserDefaults.SetInt((int)iModel.Preferences.MusicLatencyMs, kPreferenceMusicLatencyMs);
            NSUserDefaults.StandardUserDefaults.SetInt((int)iModel.Preferences.VideoLatencyMs, kPreferenceVideoLatencyMs);
            NSUserDefaults.StandardUserDefaults.SetBool(iModel.Preferences.RotaryVolumeControl, kPreferenceRotaryVolumeControl);
            NSUserDefaults.StandardUserDefaults.SetBool(iHelperAutoUpdate.OptionPageUpdates.AutoUpdate, kPreferenceAutoUpdatesEnabled);
            NSUserDefaults.StandardUserDefaults.SetBool(iHelperAutoUpdate.OptionPageUpdates.BetaVersions, kPreferenceBetaUpdatesEnabled);
            NSUserDefaults.StandardUserDefaults.Synchronize();
            centre.PostNotificationName("PreferenceAllChanged", iAppId, null, true);
        }        
        
  
        #region Handlers for the notifications from the preference pane app - all these should run in the main thread
        
        [Export("selectedReceiverChanged:")]
        private void SelectedReceiverChanged(NSNotification aNotification)
        {
            if (iInvoker.TryBeginInvoke(new Action<NSNotification>(SelectedReceiverChanged), aNotification))
                return;

            iModel.Preferences.SelectedReceiverUdn = GetStringValue(kPreferenceSelectedReceiverUdn);
        }

        [Export("selectedSubnetChanged:")]
        private void SelectedSubnetChanged(NSNotification aNotification)
        {
            if (iInvoker.TryBeginInvoke(new Action<NSNotification>(SelectedSubnetChanged), aNotification))
                return;

            iModel.Preferences.SelectedSubnetAddress = GetIntegerValue(kPreferenceSelectedSubnetAddress);                
        }

        [Export("multicastEnabledChanged:")]
        private void MulticastEnabledChanged(NSNotification aNotification)
        {
            if (iInvoker.TryBeginInvoke(new Action<NSNotification>(MulticastEnabledChanged), aNotification))
                return;

            iModel.Preferences.MulticastEnabled = GetBoolValue(kPreferenceMulticastEnabled);
        }
        
        [Export("multicastChannelChanged:")]
        private void MulticastChannelChanged(NSNotification aNotification)
        {
            if (iInvoker.TryBeginInvoke(new Action<NSNotification>(MulticastChannelChanged), aNotification))
                return;

            iModel.Preferences.MulticastChannel = GetIntegerValue(kPreferenceMulticastChannel);
        }

        [Export("musicLatencyMsChanged:")]
        private void MusicLatencyMsChanged(NSNotification aNotification)
        {
            if (iInvoker.TryBeginInvoke(new Action<NSNotification>(MusicLatencyMsChanged), aNotification))
                return;

            iModel.Preferences.MusicLatencyMs = GetIntegerValue(kPreferenceMusicLatencyMs);
        }

        [Export("videoLatencyMsChanged:")]
        private void VideoLatencyMsChanged(NSNotification aNotification)
        {
            if (iInvoker.TryBeginInvoke(new Action<NSNotification>(VideoLatencyMsChanged), aNotification))
                return;

            iModel.Preferences.VideoLatencyMs = GetIntegerValue(kPreferenceVideoLatencyMs);
        }

        [Export("volumeControlClicked:")]
        private void VolumeControlClicked(NSNotification aNotification)
        {
            if (iInvoker.TryBeginInvoke(new Action<NSNotification>(VolumeControlClicked), aNotification))
                return;

            iModel.Preferences.RotaryVolumeControl = GetBoolValue(kPreferenceRotaryVolumeControl);
        }

        [Export("autoUpdatesEnabledChanged:")]
        private void AutoUpdatesEnabledChanged(NSNotification aNotification)
        {
            if (iInvoker.TryBeginInvoke(new Action<NSNotification>(AutoUpdatesEnabledChanged), aNotification))
                return;

            iHelperAutoUpdate.OptionPageUpdates.AutoUpdate = GetBoolValue(kPreferenceAutoUpdatesEnabled);
        }

        [Export("betaUpdatesEnabledChanged:")]
        private void BetaUpdatesEnabledChanged(NSNotification aNotification)
        {
            if (iInvoker.TryBeginInvoke(new Action<NSNotification>(BetaUpdatesEnabledChanged), aNotification))
                return;

            iHelperAutoUpdate.OptionPageUpdates.BetaVersions = GetBoolValue(kPreferenceBetaUpdatesEnabled);
        }

        [Export("refreshReceiverListClicked:")]
        private void RefreshReceiverListClicked(NSNotification aNotification)
        {
            if (iInvoker.TryBeginInvoke(new Action<NSNotification>(RefreshReceiverListClicked), aNotification))
                return;

            iModel.RefreshReceiverList();
        }

        [Export("checkForUpdatesClicked:")]
        private void CheckForUpdatesClicked(NSNotification aNotification)
        {
            if (iInvoker.TryBeginInvoke(new Action<NSNotification>(CheckForUpdatesClicked), aNotification))
                return;

            iHelperAutoUpdate.CheckForUpdates();
        }
        
        #endregion

        #region Handlers for changes in the model/preferences

        private void OptionPageUpdatesChanged(object sender, EventArgs e)
        {
            // set the new values of the preferences
            NSUserDefaults.StandardUserDefaults.SetBool(iHelperAutoUpdate.OptionPageUpdates.AutoUpdate, kPreferenceAutoUpdatesEnabled);
            NSUserDefaults.StandardUserDefaults.Synchronize();

            // notify the preference pane
            NSDistributedNotificationCenter centre = NSDistributedNotificationCenter.DefaultCenter as NSDistributedNotificationCenter;
            centre.PostNotificationName("PreferenceAutoUpdatesEnabledChanged", iAppId, null, true);
        }
        
        private void ModelEnabledChanged(object sender, EventArgs e)
        {
            // This is not a true preference but the current value needs to be communicated to the preference pane
            NSUserDefaults.StandardUserDefaults.SetBool(iModel.Enabled, kPreferenceEnabled);
            NSUserDefaults.StandardUserDefaults.Synchronize();

            // notify the preference pane
            NSDistributedNotificationCenter centre = NSDistributedNotificationCenter.DefaultCenter as NSDistributedNotificationCenter;
            centre.PostNotificationName("PreferenceEnabledChanged", iAppId, null, true);
        }
        
        private void ModelReceiverListChanged(object sender, EventArgs e)
        {
            // This contains more info than the receiver list that is stored in the preferences - it also contains the
            // current status of each receiver
            NSUserDefaults.StandardUserDefaults[kPreferenceReceiverList] = BuildReceiverList();
            NSUserDefaults.StandardUserDefaults.Synchronize();

            // notify the preference pane
            NSDistributedNotificationCenter centre = NSDistributedNotificationCenter.DefaultCenter as NSDistributedNotificationCenter;
            centre.PostNotificationName("PreferenceReceiverListChanged", iAppId, null, true);
        }
        
        private void ModelSubnetListChanged(object sender, EventArgs e)
        {
            // This is not a true preference but the current value needs to be communicated to the preference pane
            NSUserDefaults.StandardUserDefaults[kPreferenceSubnetList] = BuildSubnetList();
            NSUserDefaults.StandardUserDefaults.Synchronize();

            // notify the preference pane
            NSDistributedNotificationCenter centre = NSDistributedNotificationCenter.DefaultCenter as NSDistributedNotificationCenter;
            centre.PostNotificationName("PreferenceSubnetListChanged", iAppId, null, true);
        }
        
        private void PreferencesSelectedReceiverChanged(object sender, EventArgs e)
        {
            // The app can auto-select the current receiver - this is to notify the preference pane
            NSUserDefaults.StandardUserDefaults.SetString(iModel.Preferences.SelectedReceiverUdn, kPreferenceSelectedReceiverUdn);
            NSUserDefaults.StandardUserDefaults.Synchronize();

            // notify the preference pane
            NSDistributedNotificationCenter centre = NSDistributedNotificationCenter.DefaultCenter as NSDistributedNotificationCenter;
            centre.PostNotificationName("PreferenceSelectedReceiverChanged", iAppId, null, true);
        }
        
        private void PreferencesSelectedSubnetChanged(object sender, EventArgs e)
        {
            // The app can auto-select the current subnet - this is to notify the preference pane
            NSUserDefaults.StandardUserDefaults.SetInt((int)iModel.Preferences.SelectedSubnetAddress, kPreferenceSelectedSubnetAddress);
            NSUserDefaults.StandardUserDefaults.Synchronize();

            // notify the preference pane
            NSDistributedNotificationCenter centre = NSDistributedNotificationCenter.DefaultCenter as NSDistributedNotificationCenter;
            centre.PostNotificationName("PreferenceSelectedSubnetChanged", iAppId, null, true);
        }

        
        #endregion

        // Helper methods for getting typed data from the NSUserDefaults
        
        private string GetStringValue(string aName)
        {
            NSUserDefaults.StandardUserDefaults.Synchronize();
            
            return NSUserDefaults.StandardUserDefaults.StringForKey(aName);
        }
        
        private bool GetBoolValue(string aName)
        {
            NSUserDefaults.StandardUserDefaults.Synchronize();
            
            return NSUserDefaults.StandardUserDefaults.BoolForKey(aName);
        }
        
        private uint GetIntegerValue(string aName)
        {
            NSUserDefaults.StandardUserDefaults.Synchronize();
            
            return Convert.ToUInt32(NSUserDefaults.StandardUserDefaults.IntForKey(aName));
        }

        private NSDictionary[] GetDictionaryListValue(string aName)
        {
            NSUserDefaults.StandardUserDefaults.Synchronize();

            NSObject[] list = NSUserDefaults.StandardUserDefaults.ArrayForKey(aName);
            List<NSDictionary> dictList = new List<NSDictionary>();
            
            foreach (NSObject obj in list)
            {
                NSDictionary item = obj as NSDictionary;
                if (item != null)
                {
                    dictList.Add(item);
                }
            }
            
            return dictList.ToArray();
        }
        
        private NSArray BuildReceiverList()
        {
            List<NSDictionary> dictList = new List<NSDictionary>();
            
            foreach (Receiver recv in iModel.ReceiverList)
            {
                // convert the receiver status so it can be stored in preferences
                uint status = 0;
                if (recv.IsOnline)
                {
                    switch (recv.Status)
                    {
                    case OpenHome.Songcast.EReceiverStatus.eDisconnected:
                    default:
                        status = 1;
                        break;
                    case OpenHome.Songcast.EReceiverStatus.eConnecting:
                        status = 2;
                        break;
                    case OpenHome.Songcast.EReceiverStatus.eConnected:
                        status = 3;
                        break;
                    }
                }

                // add dictionary to the list
                object[] keys = new object[] { "Udn", "Room", "Group", "Name", "Status" };
                object[] vals = new object[] { recv.Udn, recv.Room, recv.Group, recv.Name, status };
                dictList.Add(NSDictionary.FromObjectsAndKeys(vals, keys));
            }

            return NSArray.FromNSObjects(dictList.ToArray());
        }
        
        private NSArray BuildSubnetList()
        {
            List<NSDictionary> dictList = new List<NSDictionary>();
            
            foreach (ISubnet subnet in iModel.SubnetList)
            {
                object[] keys = new object[] { "Address", "Name" };
                object[] vals = new object[] { subnet.Address, subnet.AdapterName };
                dictList.Add(NSDictionary.FromObjectsAndKeys(vals, keys));
            }

            return NSArray.FromNSObjects(dictList.ToArray());
        }

        private IInvoker iInvoker;
        private string iAppId;
        private Model iModel;
        private HelperAutoUpdate iHelperAutoUpdate;
    }
}


