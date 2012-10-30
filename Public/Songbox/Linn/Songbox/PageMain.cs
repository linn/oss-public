using Linn.Toolkit;

namespace Linn.Songbox
{
    
    public interface IStartAtLoginOption
    {
        bool StartAtLogin { get; set; }
    }

    public class PageMain : OpenHome.MediaServer.PageMain, OpenHome.Xapp.ITrackerSender
    {
        private OpenHome.Xapp.Tracker iTracker;
        private OptionPagePrivacy iOptionPagePrivacy;
        private HelperAutoUpdate iHelperAutoUpdate;
        private IStartAtLoginOption iStartAtLoginOption;

        public PageMain(Helper aHelper, OptionPagePrivacy aOptionPagePrivacy, HelperAutoUpdate aHelperAutoUpdate, IStartAtLoginOption aStartAtLoginOption)
            : base()
        {
            iTracker = new OpenHome.Xapp.Tracker(TrackerConfiguration.TrackerAccount(aHelper), this);
            iOptionPagePrivacy = aOptionPagePrivacy;
            iHelperAutoUpdate = aHelperAutoUpdate;
            iStartAtLoginOption = aStartAtLoginOption;
            iOptionPagePrivacy.EventUsageDataChanged += HandleEventUsageDataChanged;
        }

        private void HandleEventUsageDataChanged(object sender, System.EventArgs e)
        {
            UpdateUsageDataValue();
        }

        public void TrackPageVisibilityChange(bool aVisibility)
        {
            if (iTracker != null)
            {
                iTracker.TrackVariable(OpenHome.Xapp.Tracker.EVariableIndex.Index1, "PageVisibility", aVisibility ? "Visible" : "Hidden", OpenHome.Xapp.Tracker.EVariableScope.Page);
                iTracker.TrackPageView(this.ViewId);
            }
        }

        public bool SendUsageData
        {
            get
            {
                return iOptionPagePrivacy.UsageData;
            }
            set
            {
                iOptionPagePrivacy.UsageData = value;
                iTracker.SetTracking(value);
            }
        }

        public void CheckForUpdates()
        {
            iHelperAutoUpdate.CheckForUpdates();
        }

        protected override void OnActivated(OpenHome.Xapp.Session aSession)
        {
            base.OnActivated(aSession);
            UpdateStartAtLoginValue();
            UpdateUsageDataValue();
            iTracker.SetTracking(iOptionPagePrivacy.UsageData);
            TrackPageVisibilityChange(false);
        }

        private void UpdateStartAtLoginValue()
        {
            Send("SetStartAtLogin", iStartAtLoginOption.StartAtLogin);
        }

        private void UpdateUsageDataValue()
        {
            Send("SetUsageData", iOptionPagePrivacy.UsageData);
        }

        protected override void OnReceive(OpenHome.Xapp.Session aSession, string aName, string aValue)
        {
            if (aName == "checkforupdates")
            {

                iHelperAutoUpdate.CheckForUpdates();
            }
            else if (aName == "startatlogin")
            {
                iStartAtLoginOption.StartAtLogin = bool.Parse(aValue);
                UpdateStartAtLoginValue();
            }
            else if (aName == "usagedata")
            {
                iOptionPagePrivacy.UsageData = bool.Parse(aValue);
                UpdateUsageDataValue();
            }
            base.OnReceive(aSession, aName, aValue);
        }

        void OpenHome.Xapp.ITrackerSender.Send(string aName, OpenHome.Xapp.JsonObject aValue)
        {
            Send(aName, aValue);
        }
    }
}