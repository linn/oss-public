using System;

namespace Linn.Konfig
{
    public class Preferences
    {
        private const string kTrackerAccountDev = "UA-35641967-1";
        private const string kTrackerAccountBeta = "UA-35652300-1";
        private const string kTrackerAccountRelease = "UA-35645916-1";
        public Preferences(Helper aHelper)
        {
            iOptionPagePrivacy = new OptionPagePrivacy(aHelper);
            aHelper.AddOptionPage(iOptionPagePrivacy);

            iOptionFirmwareBeta = new OptionBool("konfig.firmwarebeta", "Participate in firmware beta programme", "Include beta firmware when checking for available updates", false);
            iOptionNetwork = new OptionUint("konfig.network", "Network", "Network to use", 0);
            iOptionSelectedProductUdn = new OptionString("konfig.selectedproductudn", "Selected product UDN", "UDN of the product to automatically select for configuration", string.Empty);
            iOptionSendDsCrashData = new OptionBool("konfig.senddscrashdata", "Send DS crash data to Linn", "Automatically send DS crash data to Linn", false);

            aHelper.AddOption(iOptionFirmwareBeta);
            aHelper.AddOption(iOptionNetwork);
            aHelper.AddOption(iOptionSelectedProductUdn);
            aHelper.AddOption(iOptionSendDsCrashData);
            iTrackerAccount = kTrackerAccountDev;
            if (aHelper.BuildType == EBuildType.Release)
            {
                iTrackerAccount = kTrackerAccountRelease;
            }
            else if (aHelper.BuildType == EBuildType.Beta)
            {
                iTrackerAccount = kTrackerAccountBeta;
            }
        }

        public string TrackerAccount
        {
            get
            {
                return iTrackerAccount;
            }
        }

        public bool FirmwareBeta
        {
            get
            {
                return iOptionFirmwareBeta.Native;
            }
            set
            {
                iOptionFirmwareBeta.Native = value;
            }
        }

        public event EventHandler<EventArgs> EventFirmwareBetaChanged
        {
            add
            {
                iOptionFirmwareBeta.EventValueChanged += value;
            }
            remove
            {
                iOptionFirmwareBeta.EventValueChanged -= value;
            }
        }

        public uint Network
        {
            get
            {
                return iOptionNetwork.Native;
            }
            set
            {
                iOptionNetwork.Native = value;
            }
        }

        public event EventHandler<EventArgs> EventNetworkChanged
        {
            add
            {
                iOptionNetwork.EventValueChanged += value;
            }
            remove
            {
                iOptionNetwork.EventValueChanged -= value;
            }
        }

        public string SelectedProductUdn
        {
            get
            {
                return iOptionSelectedProductUdn.Native;
            }
            set
            {
                iOptionSelectedProductUdn.Native = value;
            }
        }

        public event EventHandler<EventArgs> EventSelectedProductUdnChanged
        {
            add
            {
                iOptionSelectedProductUdn.EventValueChanged += value;
            }
            remove
            {
                iOptionSelectedProductUdn.EventValueChanged -= value;
            }
        }

        public bool SendDsCrashData
        {
            get
            {
                return iOptionSendDsCrashData.Native;
            }
            set
            {
                iOptionSendDsCrashData.Native = value;
            }
        }

        public event EventHandler<EventArgs> EventSendDsCrashDataChanged
        {
            add
            {
                iOptionSendDsCrashData.EventValueChanged += value;
            }
            remove
            {
                iOptionSendDsCrashData.EventValueChanged -= value;
            }
        }

        public bool UsageData
        {
            get { return iOptionPagePrivacy.UsageData; }
            set { iOptionPagePrivacy.UsageData = value; }
        }

        public event EventHandler<EventArgs> EventUsageDataChanged
        {
            add { iOptionPagePrivacy.EventUsageDataChanged += value; }
            remove { iOptionPagePrivacy.EventUsageDataChanged -= value; }
        }

        private OptionBool iOptionFirmwareBeta;
        private OptionUint iOptionNetwork;
        private OptionString iOptionSelectedProductUdn;
        private OptionBool iOptionSendDsCrashData;
        private OptionPagePrivacy iOptionPagePrivacy;
        private string iTrackerAccount;
    }
}

