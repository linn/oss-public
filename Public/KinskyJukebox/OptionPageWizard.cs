using System;
using Linn;

namespace KinskyJukebox
{
    public class OptionPageWizard : OptionPage
    {
        public enum State
        {
            eAlways,
            eNever,
            ePrompt,
            eQuickScan,
            eFullScan,
        }

        public OptionPageWizard()
            : base("Wizard") {

            iConfig = new OptionEnum("wizardchangeconfig", "Change Configuration Settings", "Determines if the wizard will execute this step each time it is run. If 'Prompt' is selected, you will be prompted each time the wizard is run.");
            AddBasic(iConfig);

            iImportSavedPresets = new OptionEnum("wizardimport", "Import Saved Presets", "Determines if the wizard will execute this step each time it is run. If 'Prompt' is selected, you will be prompted each time the wizard is run.");
            AddBasic(iImportSavedPresets);

            iClearImport = new OptionEnum("wizardclear", "Clear Presets Window before Import", "Determines if the wizard will clear the presets window before importing your saved presets each time it is run. If 'Prompt' is selected, you will be prompted each time the wizard is run.");
            AddBasic(iClearImport);

            iCorrectIp = new OptionEnum("wizardipcorrect", "Correct IP Address Mismatches on Import", "Determines if the wizard will correct any IP address matches found when importing your presets each time it is run. If 'Prompt' is selected, you will be prompted each time the wizard is run.");
            AddBasic(iCorrectIp);

            iUseCurrentScan = new OptionEnum("wizardusescan", "Use Available Scan Data", "Determines if the wizard will use any existing scan data rather than re-scanning each time it is run. If 'Prompt' is selected, you will be prompted each time the wizard is run.");
            AddBasic(iUseCurrentScan);

            iScanType = new OptionEnum("wizardscantype", "Scan Type", "Determines if the wizard will perform a full scan (entire collection) or a quick scan (ie single album) each time it is run. If 'Prompt' is selected, you will be prompted each time the wizard is run.");
            iScanType.AddDefault(kPrompt);
            iScanType.Add(kFullScan);
            iScanType.Add(kQuickScan);
            Add(iScanType);

            iSyncDs = new OptionEnum("wizardsync", "Sync Location with Linn DS", "Determines if the wizard will execute this step each time it is run. If 'Prompt' is selected, you will be prompted each time the wizard is run.");
            AddBasic(iSyncDs);

            iPrint = new OptionEnum("wizardprint", "Print Catalog", "Determines if the wizard will execute this step each time it is run. If 'Prompt' is selected, you will be prompted each time the wizard is run.");
            AddBasic(iPrint);
        }

        private void AddBasic(OptionEnum aOption) {
            aOption.AddDefault(kPrompt);
            aOption.Add(kAlways);
            aOption.Add(kNever);
            Add(aOption);
        }

        public State Config {
            get { return ConvertStateStringToEnum(iConfig.Value); }
            set { iConfig.Set(ConvertStateEnumToString(value)); }
        }

        public State ImportSavedPresets {
            get { return ConvertStateStringToEnum(iImportSavedPresets.Value); }
            set { iImportSavedPresets.Set(ConvertStateEnumToString(value)); }
        }

        public State ClearImport {
            get { return ConvertStateStringToEnum(iClearImport.Value); }
            set { iClearImport.Set(ConvertStateEnumToString(value)); }
        }

        public State CorrectIp {
            get { return ConvertStateStringToEnum(iCorrectIp.Value); }
            set { iCorrectIp.Set(ConvertStateEnumToString(value)); }
        }

        public State UseCurrentScan {
            get { return ConvertStateStringToEnum(iUseCurrentScan.Value); }
            set { iUseCurrentScan.Set(ConvertStateEnumToString(value)); }
        }

        public State ScanType {
            get { return ConvertStateStringToEnum(iScanType.Value); }
            set { iScanType.Set(ConvertStateEnumToString(value)); }
        }

        public State SyncDs {
            get { return ConvertStateStringToEnum(iSyncDs.Value); }
            set { iSyncDs.Set(ConvertStateEnumToString(value)); }
        }

        public State Print {
            get { return ConvertStateStringToEnum(iPrint.Value); }
            set { iPrint.Set(ConvertStateEnumToString(value)); }
        }

        public event EventHandler<EventArgs> EventConfigChanged {
            add { iConfig.EventValueChanged += value; }
            remove { iConfig.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventImportSavedPresetsChanged {
            add { iImportSavedPresets.EventValueChanged += value; }
            remove { iImportSavedPresets.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventClearImportChanged {
            add { iClearImport.EventValueChanged += value; }
            remove { iClearImport.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventCorrectIpChanged {
            add { iCorrectIp.EventValueChanged += value; }
            remove { iCorrectIp.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventUseCurrentScanChanged {
            add { iUseCurrentScan.EventValueChanged += value; }
            remove { iUseCurrentScan.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventScanTypeChanged {
            add { iScanType.EventValueChanged += value; }
            remove { iScanType.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSyncDsChanged {
            add { iSyncDs.EventValueChanged += value; }
            remove { iSyncDs.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventPrintChanged {
            add { iPrint.EventValueChanged += value; }
            remove { iPrint.EventValueChanged -= value; }
        }

        private State ConvertStateStringToEnum(string aString) {
            if (aString == kAlways) {
                return State.eAlways;
            }
            else if (aString == kNever) {
                return State.eNever;
            }
            else if (aString == kQuickScan) {
                return State.eQuickScan;
            }
            else if (aString == kFullScan) {
                return State.eFullScan;
            }
            else {
                return State.ePrompt;
            }
        }

        private string ConvertStateEnumToString(State aEnum) {
            if (aEnum == State.eAlways) {
                return kAlways;
            }
            else if (aEnum == State.eNever) {
                return kNever;
            }
            else if (aEnum == State.eQuickScan) {
                return kQuickScan;
            }
            else if (aEnum == State.eFullScan) {
                return kFullScan;
            }
            else {
                return kPrompt;
            }
        }

        private static string kPrompt = "Prompt";
        private static string kAlways = "Always";
        private static string kNever = "Never";
        private static string kQuickScan = "Quick";
        private static string kFullScan = "Full";

        private OptionEnum iConfig;
        private OptionEnum iImportSavedPresets;
        private OptionEnum iClearImport;
        private OptionEnum iCorrectIp;
        private OptionEnum iUseCurrentScan;
        private OptionEnum iScanType;
        private OptionEnum iSyncDs;
        private OptionEnum iPrint;
    }
}