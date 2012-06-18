using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

namespace KinskyJukebox
{
    public partial class FormWizard : Form
    {
        public FormWizard(HelperKinskyJukebox aHelper, FormKinskyJukebox aForm) {
            InitializeComponent();
            this.Icon = Icon.FromHandle(Properties.Resources.Wizard.GetHicon());
            iParentForm = aForm;
            wizardMessageBox.Text = "Click the Start button to begin the wizard";
            Step = Steps.eNotStarted;
            iHelper = aHelper;
        }

        private void buttonStart_Click(object sender, EventArgs e) {
            switch (iStep) {
                case Steps.eNotStarted: {
                    wizardConfigImage.BackgroundImage = null;
                    wizardImportImage.BackgroundImage = null;
                    wizardScanImage.BackgroundImage = null;
                    wizardSyncImage.BackgroundImage = null;
                    wizardDsImage.BackgroundImage = null;
                    wizardSaveImage.BackgroundImage = null;
                    wizardPrintImage.BackgroundImage = null;
                    this.ControlBox = false;
                    buttonCancel.Visible = true;
                    buttonCancel.DialogResult = DialogResult.None;
                    buttonCancel.Text = "Cancel";

                    // force config if scan directory not setup or invalid
                    wizardMessageBox.Text = "Scan Directory Configuration In Progress";
                    buttonStart.Visible = false;
                    buttonSkip.Visible = false;
                    Step = Steps.eScanDirectoryConfig;
                    if (!iParentForm.VerifyCollectionLocation()) {
                        WizardCancel();
                    }
                    break;
                }
                case Steps.eConfigOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.Config = OptionPageWizard.State.eAlways;
                    }
                    wizardMessageBox.Text = "Configuration In Progress";
                    buttonStart.Visible = false;
                    buttonSkip.Visible = false;
                    Step = Steps.eConfigInProgress;
                    iParentForm.optionsToolStripMenuItem_Click(this, EventArgs.Empty);
                    break;
                }
                case Steps.eImportOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.ImportSavedPresets = OptionPageWizard.State.eAlways;
                    }

                    if (iParentForm.PresetDataExists()) {
                        // option to clear existing preset data
                        wizardMessageBox.Text = "Do you want to clear the presets window before importing your saved presets?";
                        buttonStart.Text = "Yes";
                        buttonSkip.Text = "No";
                        buttonStart.Visible = true;
                        buttonSkip.Visible = true;
                        Step = Steps.eClearImportOption;

                        // look at user option for clear import
                        if (iHelper.OptionPageWizard.ClearImport == OptionPageWizard.State.eAlways) {
                            buttonStart_Click(this, null);
                        }
                        else if (iHelper.OptionPageWizard.ClearImport == OptionPageWizard.State.eNever) {
                            buttonSkip_Click(this, null);
                        }
                        else {
                            rememberBox.Checked = false;
                            rememberBox.Visible = true;
                        }
                    }
                    else {
                        ImportInProgress(false);
                    }
                    break;
                }
                case Steps.eClearImportOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.ClearImport = OptionPageWizard.State.eAlways;
                    }
                    ImportInProgress(true);
                    break;
                }
                case Steps.eUseAvaialbleScanDataOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.UseCurrentScan = OptionPageWizard.State.eAlways;
                    }
                    WizardScanComplete(true); // use info from last scan and skip to next step
                    break;
                }
                case Steps.eChooseScanType: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.ScanType = OptionPageWizard.State.eFullScan;
                    }
                    wizardMessageBox.Text = "Full Scan In Progress";
                    buttonStart.Visible = false;
                    buttonSkip.Visible = false;
                    wizardProgressBar.Value = 0;
                    wizardProgressBar.Visible = true;
                    Step = Steps.eScanInProgress;
                    iParentForm.toolStripButtonScan_Click(this, EventArgs.Empty);
                    break;
                }
                case Steps.eSyncWithDsOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.SyncDs = OptionPageWizard.State.eAlways;
                    }
                    wizardMessageBox.Text = "Sync with Linn DS In Progress";
                    buttonStart.Visible = false;
                    buttonSkip.Visible = false;
                    Step = Steps.eSynchDsInProgress;
                    iParentForm.exportToLinnDSToolStripMenuItem_Click(this, EventArgs.Empty);
                    break;
                }
                case Steps.ePrintOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.Print = OptionPageWizard.State.eAlways;
                    }
                    wizardMessageBox.Text = "Printing In Progress";
                    buttonStart.Visible = false;
                    buttonSkip.Visible = false;
                    wizardProgressBar.Value = 0;
                    wizardProgressBar.Visible = true;
                    Step = Steps.ePrintInProgress;
                    iParentForm.printToolStripMenuItem_Click(this, EventArgs.Empty);
                    break;
                }
                default: {
                    break;
                }
            }
        }

        private void buttonSkip_Click(object sender, EventArgs e) {
            switch (iStep) {
                case Steps.eConfigOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.Config = OptionPageWizard.State.eNever;
                    }
                    WizardConfigComplete(true); // no config required - continue wiazrd
                    break;
                }
                case Steps.eImportOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.ImportSavedPresets = OptionPageWizard.State.eNever;
                    }
                    WizardImportComplete(true); // no import required - continue wiazrd
                    break;
                }
                case Steps.eClearImportOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.ClearImport = OptionPageWizard.State.eNever;
                    }
                    ImportInProgress(false);
                    break;
                }
                case Steps.eUseAvaialbleScanDataOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.UseCurrentScan = OptionPageWizard.State.eNever;
                    }
                    ChooseScanType(); // override with new scan
                    break;
                }
                case Steps.eChooseScanType: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.ScanType = OptionPageWizard.State.eQuickScan;
                    }
                    wizardMessageBox.Text = "Quick Scan In Progress";
                    buttonStart.Visible = false;
                    buttonSkip.Visible = false;
                    wizardProgressBar.Value = 0;
                    wizardProgressBar.Visible = true;
                    Step = Steps.eScanInProgress;
                    iParentForm.toolStripButtonQuickScan_Click(this, EventArgs.Empty);
                    break;
                }
                case Steps.eSyncWithDsOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.SyncDs = OptionPageWizard.State.eNever;
                    }
                    WizardSyncWithDsComplete(true);
                    break;
                }
                case Steps.ePrintOption: {
                    rememberBox.Visible = false;
                    if (rememberBox.Checked) {
                        iHelper.OptionPageWizard.Print = OptionPageWizard.State.eNever;
                    }
                    WizardPrintComplete(true);
                    break;
                }
                default: {
                    break;
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e) {
            switch (iStep) {
                case Steps.eImportInProgress:
                case Steps.eSaveInProgress:
                case Steps.ePrintInProgress: {
                    iParentForm.SynchStopButton_Click(this, EventArgs.Empty);
                    break;
                }
                case Steps.eScanInProgress: {
                    iParentForm.ScanStopButton_Click(this, EventArgs.Empty);
                    break;
                }
                default: {
                    WizardCancel();
                    break;
                }
            }
        }

        public void WizardScanDirectoryConfigComplete() {
            // force config if directory url not setup or invalid
            if (iStep == Steps.eScanDirectoryConfig) {
                wizardMessageBox.Text = "URL of Scan Directory Configuration In Progress";
                buttonStart.Visible = false;
                buttonSkip.Visible = false;
                Step = Steps.eScanDirectoryUrlConfig;
                if (!iParentForm.VerifyHttpLocation()) {
                    WizardCancel();
                }
            }
        }

        public void WizardScanDirectoryUrlConfigComplete(bool aRequiredConfig) {
            if (iStep == Steps.eScanDirectoryUrlConfig) {
                if (!aRequiredConfig) { // only offer choice if no forced config occurred
                    wizardMessageBox.Text = "Would you like to change your configuration settings?";
                    buttonStart.Text = "Yes";
                    buttonSkip.Text = "No";
                    buttonStart.Visible = true;
                    buttonSkip.Visible = true;
                    Step = Steps.eConfigOption;

                    // look at user option for config step
                    if (iHelper.OptionPageWizard.Config == OptionPageWizard.State.eAlways) {
                        buttonStart_Click(this, null);
                    }
                    else if (iHelper.OptionPageWizard.Config == OptionPageWizard.State.eNever) {
                        buttonSkip_Click(this, null);
                    }
                    else {
                        rememberBox.Checked = false;
                        rememberBox.Visible = true;
                    }
                }
                else {
                    WizardConfigComplete(false);
                }
            }
        }

        public void WizardConfigComplete(bool aSkipped) {
            if (aSkipped) {
                wizardConfigImage.BackgroundImage = Properties.Resources.Skipped;
            }
            else {
                wizardConfigImage.BackgroundImage = Properties.Resources.Pass;
            }

            wizardMessageBox.Text = "Would you like to import your saved presets?";
            buttonStart.Text = "Yes";
            buttonSkip.Text = "No";
            buttonStart.Visible = true;
            buttonSkip.Visible = true;
            Step = Steps.eImportOption;

            if (iHelper.OptionPageWizard.ImportSavedPresets == OptionPageWizard.State.eAlways) {
                buttonStart_Click(this, null);
            }
            else if (iHelper.OptionPageWizard.ImportSavedPresets == OptionPageWizard.State.eNever) {
                buttonSkip_Click(this, null);
            }
            else {
                rememberBox.Checked = false;
                rememberBox.Visible = true;
            }
        }

        public void WizardImportComplete(bool aSkipped) {
            if (aSkipped) {
                wizardImportImage.BackgroundImage = Properties.Resources.Skipped;
            }
            else {
                wizardImportImage.BackgroundImage = Properties.Resources.Pass;
            }
            // check if scanned music already available
            if (iParentForm.ScanDataExists()) {
                wizardMessageBox.Text = "Would you like to use the available data from the last scan performed?";
                buttonStart.Text = "Yes";
                buttonSkip.Text = "No";
                buttonSkip.Visible = true;
                buttonStart.Visible = true;
                Step = Steps.eUseAvaialbleScanDataOption;

                if (iHelper.OptionPageWizard.UseCurrentScan == OptionPageWizard.State.eAlways) {
                    buttonStart_Click(this, null);
                }
                else if (iHelper.OptionPageWizard.UseCurrentScan == OptionPageWizard.State.eNever) {
                    buttonSkip_Click(this, null);
                }
                else {
                    rememberBox.Checked = false;
                    rememberBox.Visible = true;
                }
            }
            else {
                ChooseScanType();
            }
        }

        public void WizardScanComplete(bool aSkipped) {
            if (aSkipped) {
                wizardScanImage.BackgroundImage = Properties.Resources.Skipped;
            }
            else {
                wizardScanImage.BackgroundImage = Properties.Resources.Pass;
            }
            wizardMessageBox.Text = "Syncing Presets In Progress";
            buttonStart.Visible = false;
            buttonSkip.Visible = false;
            Step = Steps.eSyncInProgress;
            iParentForm.syncButton_Click(this, EventArgs.Empty);
        }

        public void WizardSyncComplete() {
            wizardSyncImage.BackgroundImage = Properties.Resources.Pass;
            wizardMessageBox.Text = "Saving Presets In Progress";
            buttonStart.Visible = false;
            buttonSkip.Visible = false;
            wizardProgressBar.Value = 0;
            wizardProgressBar.Visible = true;
            Step = Steps.eSaveInProgress;
            iParentForm.exportButton_Click(this, EventArgs.Empty);
        }

        public void WizardSaveComplete(string aPresetPrefix, int aPresetTotal) {
            wizardSaveImage.BackgroundImage = Properties.Resources.Pass;
            wizardMessageBox.Text = "Created " + aPresetTotal + " Presets at " + aPresetPrefix + Environment.NewLine + "Would you like to Sync with your Linn DS?";
            buttonStart.Text = "Yes";
            buttonSkip.Text = "No";
            buttonSkip.Visible = true;
            buttonStart.Visible = true;
            Step = Steps.eSyncWithDsOption;

            if (iHelper.OptionPageWizard.SyncDs == OptionPageWizard.State.eAlways) {
                buttonStart_Click(this, null);
            }
            else if (iHelper.OptionPageWizard.SyncDs == OptionPageWizard.State.eNever) {
                buttonSkip_Click(this, null);
            }
            else {
                rememberBox.Checked = false;
                rememberBox.Visible = true;
            }
        }

        public void WizardSyncWithDsComplete(bool aSkipped) {
            if (aSkipped) {
                wizardDsImage.BackgroundImage = Properties.Resources.Skipped;
            }
            else {
                wizardDsImage.BackgroundImage = Properties.Resources.Pass;
            }
            wizardMessageBox.Text = "Would you like to print a catalog of your presets to a file?";
            buttonStart.Text = "Yes";
            buttonSkip.Text = "No";
            buttonSkip.Visible = true;
            buttonStart.Visible = true;
            Step = Steps.ePrintOption;

            if (iHelper.OptionPageWizard.Print == OptionPageWizard.State.eAlways) {
                buttonStart_Click(this, null);
            }
            else if (iHelper.OptionPageWizard.Print == OptionPageWizard.State.eNever) {
                buttonSkip_Click(this, null);
            }
            else {
                rememberBox.Checked = false;
                rememberBox.Visible = true;
            }
        }

        public void WizardPrintComplete(bool aSkipped) {
            if (aSkipped) {
                wizardPrintImage.BackgroundImage = Properties.Resources.Skipped;
            }
            else {
                wizardPrintImage.BackgroundImage = Properties.Resources.Pass;
            }
            WizardComplete();
        }

        public void WizardComplete() {
            wizardMessageBox.Text = "Wizard Completed Successfully";
            buttonStart.Text = "Start";
            wizardProgressBar.Visible = false;
            rememberBox.Visible = false;
            buttonSkip.Visible = false;
            buttonStart.Visible = true;
            Step = Steps.eNotStarted;
            this.ControlBox = true;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Text = "Done";
        }

        public void WizardCancel() {
            UpdateAbortGraphic();
            wizardMessageBox.Text = "Wizard Cancelled";
            buttonStart.Text = "Start";
            wizardProgressBar.Visible = false;
            rememberBox.Visible = false;
            buttonSkip.Visible = false;
            buttonStart.Visible = true;
            Step = Steps.eNotStarted;
            this.ControlBox = true;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Text = "Done";
        }

        public void WizardFailed(string aFailMessage) {
            UpdateAbortGraphic();
            wizardMessageBox.Text = "Wizard Failed: " + aFailMessage;
            buttonStart.Text = "Start";
            wizardProgressBar.Visible = false;
            rememberBox.Visible = false;
            buttonSkip.Visible = false;
            buttonStart.Visible = true;
            Step = Steps.eNotStarted;
            this.ControlBox = true;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Text = "Done";
        }

        private void UpdateAbortGraphic() {
            switch (iStep) {
                case Steps.eScanDirectoryConfig:
                case Steps.eScanDirectoryUrlConfig:
                case Steps.eConfigOption:
                case Steps.eConfigInProgress: {
                    wizardConfigImage.BackgroundImage = Properties.Resources.Fail;
                    break;
                }
                case Steps.eImportOption:
                case Steps.eClearImportOption:
                case Steps.eImportInProgress: {
                    wizardImportImage.BackgroundImage = Properties.Resources.Fail;
                    break;
                }
                case Steps.eUseAvaialbleScanDataOption:
                case Steps.eChooseScanType:
                case Steps.eScanInProgress: {
                    wizardScanImage.BackgroundImage = Properties.Resources.Fail;
                    break;
                }
                case Steps.eSyncInProgress: {
                    wizardSyncImage.BackgroundImage = Properties.Resources.Fail;
                    break;
                }
                case Steps.eSaveInProgress: {
                    wizardSaveImage.BackgroundImage = Properties.Resources.Fail;
                    break;
                }
                case Steps.eSyncWithDsOption:
                case Steps.eSynchDsInProgress: {
                    wizardDsImage.BackgroundImage = Properties.Resources.Fail;
                    break;
                }
                case Steps.ePrintOption:
                case Steps.ePrintInProgress: {
                    wizardPrintImage.BackgroundImage = Properties.Resources.Fail;
                    break;
                }
                default: {
                    break;
                }
            }
        }

        private void ChooseScanType() {
            // allow user to choose full or quick scan
            wizardMessageBox.Text = "Would you like to scan your entire collection (Full) or just a folder within your collection (Quick)";
            buttonStart.Text = "Full";
            buttonSkip.Text = "Quick";
            buttonSkip.Visible = true;
            buttonStart.Visible = true;
            Step = Steps.eChooseScanType;

            if (iHelper.OptionPageWizard.ScanType == OptionPageWizard.State.eFullScan) {
                buttonStart_Click(this, null);
            }
            else if (iHelper.OptionPageWizard.ScanType == OptionPageWizard.State.eQuickScan) {
                buttonSkip_Click(this, null);
            }
            else {
                rememberBox.Checked = false;
                rememberBox.Visible = true;
            }
        }

        public void ScanProgress(int aPercent, string aMessage, Progress.State aProgressState) {
            if (aProgressState == Progress.State.eSuccess) {
                WizardScanComplete(false);
            }
            else if (aProgressState == Progress.State.eFail) {
                WizardFailed(aMessage);
            }
            else if (aProgressState == Progress.State.eComplete) {
                wizardProgressBar.Visible = false;
            }
            else if (aProgressState == Progress.State.eInProgress) {
                if (aMessage != null && wizardMessageBox.Text != aMessage) {
                    wizardMessageBox.Text = aMessage;
                }
                if ((aPercent >= 0) && (aPercent <= 100)) {
                    wizardProgressBar.Value = aPercent;
                }
            }
        }

        public void ExportProgress(int aPercent, string aMessage, Progress.State aProgressState) {
            if (aProgressState == Progress.State.eSuccess) {
                WizardSaveComplete(Presets.UriPath(iHelper), aPercent);
            }
            else if (aProgressState == Progress.State.eFail) {
                WizardFailed(aMessage);
            }
            else if (aProgressState == Progress.State.eComplete) {
                wizardProgressBar.Visible = false;
            }
            else if (aProgressState == Progress.State.eInProgress) {
                if (aMessage != null && wizardMessageBox.Text != aMessage) {
                    wizardMessageBox.Text = aMessage;
                }
                if ((aPercent >= 0) && (aPercent <= 100)) {
                    wizardProgressBar.Value = aPercent;
                }
            }
        }

        public void ImportProgress(int aPercent, string aMessage, Progress.State aProgressState) {
            if (aProgressState == Progress.State.eSuccess) {
                iParentForm.AssignNumbers();
                WizardImportComplete(false);
            }
            else if (aProgressState == Progress.State.eFail) {
                WizardFailed(aMessage);
            }
            else if (aProgressState == Progress.State.eComplete) {
                wizardProgressBar.Visible = false;
            }
            else if (aProgressState == Progress.State.eInProgress) {
                if (aMessage != null && wizardMessageBox.Text != aMessage) {
                    wizardMessageBox.Text = aMessage;
                }
                if ((aPercent >= 0) && (aPercent <= 100)) {
                    wizardProgressBar.Value = aPercent;
                }
            }
        }

        public void PrintingProgress(int aPercent, string aMessage, Progress.State aProgressState) {
            if (aProgressState == Progress.State.eSuccess) {
                WizardPrintComplete(false);
            }
            else if (aProgressState == Progress.State.eFail) {
                WizardFailed(aMessage);
            }
            else if (aProgressState == Progress.State.eComplete) {
                wizardProgressBar.Visible = false;
            }
            else if (aProgressState == Progress.State.eInProgress) {
                if (aMessage != null && wizardMessageBox.Text != aMessage) {
                    wizardMessageBox.Text = aMessage;
                }
                if ((aPercent >= 0) && (aPercent <= 100)) {
                    wizardProgressBar.Value = aPercent;
                }
            }
        }

        private void ImportInProgress(bool aClear) {
            wizardMessageBox.Text = "Import In Progress";
            buttonStart.Visible = false;
            buttonSkip.Visible = false;
            wizardProgressBar.Value = 0;
            wizardProgressBar.Visible = true;
            Step = Steps.eImportInProgress;
            iParentForm.importButton_Click((bool)aClear, EventArgs.Empty); // option to import current presets
        }

        private Steps Step {
            set {
                iStep = value;
                wizardConfigLabel.BackColor = SystemColors.Window;
                wizardImportLabel.BackColor = SystemColors.Window;
                wizardScanLabel.BackColor = SystemColors.Window;
                wizardSyncLabel.BackColor = SystemColors.Window;
                wizardSaveLabel.BackColor = SystemColors.Window;
                wizardDsLabel.BackColor = SystemColors.Window;
                wizardPrintLabel.BackColor = SystemColors.Window;

                switch (iStep) {
                    case Steps.eScanDirectoryConfig:
                    case Steps.eScanDirectoryUrlConfig:
                    case Steps.eConfigInProgress:
                    case Steps.eConfigOption: {
                        wizardConfigLabel.BackColor = Color.Yellow;
                        wizardConfigImage.BackgroundImage = Properties.Resources.Wizard;
                        break;
                    }
                    case Steps.eImportOption:
                    case Steps.eClearImportOption:
                    case Steps.eImportInProgress: {
                        wizardImportLabel.BackColor = Color.Yellow;
                        wizardImportImage.BackgroundImage = Properties.Resources.Wizard;
                        break;
                    }
                    case Steps.eUseAvaialbleScanDataOption:
                    case Steps.eChooseScanType:
                    case Steps.eScanInProgress: {
                        wizardScanLabel.BackColor = Color.Yellow;
                        wizardScanImage.BackgroundImage = Properties.Resources.Wizard;
                        break;
                    }
                    case Steps.eSyncInProgress: {
                        wizardSyncLabel.BackColor = Color.Yellow;
                        wizardSyncImage.BackgroundImage = Properties.Resources.Wizard;
                        break;
                    }
                    case Steps.eSaveInProgress: {
                        wizardSaveLabel.BackColor = Color.Yellow;
                        wizardSaveImage.BackgroundImage = Properties.Resources.Wizard;
                        break;
                    }
                    case Steps.eSyncWithDsOption:
                    case Steps.eSynchDsInProgress: {
                        wizardDsLabel.BackColor = Color.Yellow;
                        wizardDsImage.BackgroundImage = Properties.Resources.Wizard;
                        break;
                    }
                    case Steps.ePrintOption:
                    case Steps.ePrintInProgress: {
                        wizardPrintLabel.BackColor = Color.Yellow;
                        wizardPrintImage.BackgroundImage = Properties.Resources.Wizard;
                        break;
                    }
                    default: {
                        break;
                    }
                }
            }
        }

        private enum Steps
        {
            eNotStarted,
            eScanDirectoryConfig,
            eScanDirectoryUrlConfig,
            eConfigOption,
            eImportOption,
            eClearImportOption,
            eUseAvaialbleScanDataOption,
            eChooseScanType,
            eSyncWithDsOption,
            ePrintOption,
            eConfigInProgress,
            eImportInProgress,
            eScanInProgress,
            eSyncInProgress,
            eSaveInProgress,
            eSynchDsInProgress,
            ePrintInProgress,
        }

        private FormKinskyJukebox iParentForm;
        private Steps iStep;
        private HelperKinskyJukebox iHelper;
    }
}
