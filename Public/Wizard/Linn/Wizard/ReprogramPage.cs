using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;

namespace Linn.Wizard
{
    public class ReprogramPage : BasePage, IUpdateFirmwareHandler
    {
        public ReprogramPage(PageControl aPageControl, PageDefinitions.Page aPageDefinition)
            : base(aPageControl, aPageDefinition)
        {
            
        }


        protected override void OnActivated(Session aSession)
        {
            // this page stores the selected box - this will fail when multiple sessions are being used
            // since the data needs to be per session, not per page. This is done as a temporary measure
            // to get around some problems with the ProductSupport api
            iSelectedBox = aSession.Model.SelectedBox;

            iSelectedBox.UpdateCheckCompleteEvent += UpdateCheckComplete;
            iSelectedBox.UpdateCheckErrorEvent += UpdateCheckError;

            base.OnActivated(aSession);

            SetInitialUpdateState(iSelectedBox);
        }

        private void SetInitialUpdateState(Box aBox) {
            bool nextButtonVisible = true;
            bool updateButtonVisible = false;
            string updateButtonText = "";

            if (aBox == null)
            {
                Send("UpdateBoxTitle", "Update Not Possible");
                Send("UpdateBoxText", "Your device is no longer available.");
            }
            else if (aBox.State == Box.EState.eOff)
            {
                Send("UpdateBoxTitle", "Update Not Possible");
                Send("UpdateBoxText", "Your device is turned off.");
            }
            else if (aBox.IsProxy)
            {
                Send("UpdateBoxTitle", "Update Not Possible");
                Send("UpdateBoxText", "Your device can not be updated because it is a network proxy.");
            }
            else if (aBox.UpdateCheckInProgress)
            {
                Send("UpdateBoxTitle", "Update Check In Progress");
                Send("UpdateBoxText", "Update information is currently downloading.<br>Please wait.");
            }
            else if (aBox.UpdateCheckFailed)
            {
                Send("UpdateBoxTitle", "Update Check Failed");
                Send("UpdateBoxText", "Unable to download update information. Please check your internet connection.");

                updateButtonVisible = true;
                updateButtonText = kUpdateButtonCheckForUpdatesAgain;
            }
            else if (aBox.UpdateCheckDeviceNotFound)
            {
                Send("UpdateBoxTitle", "Update Not Possible");
                Send("UpdateBoxText", "No update information could be found for '" + aBox.Model + "'.");
            }
            else if (!aBox.SoftwareUpdateAvailable)
            {
                Send("UpdateBoxTitle", "No Update Is Required");
                Send("UpdateBoxText", "Your device is already running the latest software.");
            }
            else if (aBox.SoftwareUpdateAvailable)
            {
                Send("UpdateBoxTitle", "Update Available");
                Send("UpdateBoxText", "New software, " + aBox.SoftwareUpdateVersion + ", is available for your device. Click below to start the update. Please do not close the application while the update is in progress.");
                
                updateButtonVisible = true;
                updateButtonText = kUpdateButtonTextStartUpdate;
            }

            SetButtonState(nextButtonVisible, updateButtonVisible, updateButtonText);
        }

        private void SetButtonState(bool aNextPrevButtonsVisible, bool aUpdateButtonVisible) {
            SetButtonState(aNextPrevButtonsVisible, aUpdateButtonVisible, "");
        }

        private void SetButtonState(bool aNextPrevButtonsVisible, bool aUpdateButtonVisible, string aUpdateButtonText) {
            OpenHome.Xapp.JsonObject joNew = new OpenHome.Xapp.JsonObject();
            joNew.Add("Type", new OpenHome.Xapp.JsonValueString("Control"));
            joNew.Add("Id", new OpenHome.Xapp.JsonValueString("NextButton"));
            joNew.Add("Visible", new OpenHome.Xapp.JsonValueBool(aNextPrevButtonsVisible));
            Send("Render", joNew);

            OpenHome.Xapp.JsonObject joPrev = new OpenHome.Xapp.JsonObject();
            joPrev.Add("Type", new OpenHome.Xapp.JsonValueString("Control"));
            joPrev.Add("Id", new OpenHome.Xapp.JsonValueString("PreviousButton"));
            joPrev.Add("Visible", new OpenHome.Xapp.JsonValueBool(aNextPrevButtonsVisible));
            Send("Render", joPrev);

            OpenHome.Xapp.JsonObject joUpdate = new OpenHome.Xapp.JsonObject();
            joUpdate.Add("Type", new OpenHome.Xapp.JsonValueString("Control"));
            joUpdate.Add("Id", new OpenHome.Xapp.JsonValueString("UpdateButton"));
            joUpdate.Add("Text", new OpenHome.Xapp.JsonValueString(aUpdateButtonText));
            joUpdate.Add("Visible", new OpenHome.Xapp.JsonValueBool(aUpdateButtonVisible));
            Send("Render", joUpdate);

            iUpdateButtonText = aUpdateButtonText;

            OpenHome.Xapp.JsonObject joUpdateBgd = new OpenHome.Xapp.JsonObject();
            joUpdateBgd.Add("Type", new OpenHome.Xapp.JsonValueString("Control"));
            joUpdateBgd.Add("Id", new OpenHome.Xapp.JsonValueString("UpdateBoxFooter"));
            joUpdateBgd.Add("Visible", new OpenHome.Xapp.JsonValueBool(aUpdateButtonVisible));
            Send("Render", joUpdateBgd);
        }

        private void UpdateCheckComplete(object obj, EventArgs e) {
            SetInitialUpdateState(iSelectedBox);
        }

        private void UpdateCheckError(object obj, EventArgsUpdateError e) {
            SetInitialUpdateState(iSelectedBox);
        }

        protected override void OnDeactivated(Session aSession)
        {
            iSelectedBox.UpdateCheckCompleteEvent -= UpdateCheckComplete;
            iSelectedBox.UpdateCheckErrorEvent -= UpdateCheckError;
            iSelectedBox = null;

            base.OnDeactivated(aSession);
        }
            

        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            
            switch (aName)
            {
                case "UpdateButton":
                    if (iUpdateButtonText == kUpdateButtonTextStartUpdate || iUpdateButtonText == kUpdateButtonTextTryUpdateAgain) {
                        ReprogramDevice(aSession);
                    }
                    else if (iUpdateButtonText == kUpdateButtonCheckForUpdatesAgain) {
                        Box selectedBox = aSession.Model.SelectedBox;
                        selectedBox.CheckForUpdates();
                        SetInitialUpdateState(selectedBox);
                    }
                    break;
                
                default:
                    base.OnReceive(aSession, aName, aValue);
                    break;
            }
        }



        private void ReprogramDevice(Session aSession)
        {
            Box selectedBox = aSession.Model.SelectedBox;
            selectedBox.UpdateFirmware(this, false);
        }

        // IUpdateFirmwareHandler interface
        public void Started() {
            Send("UpdateBoxTitle", "Update In Progress");
            Send("UpdateBoxText", "");
            SetButtonState(false, false);

            // Set progress bar visible
            Send("Unhide", "UpdateProgressBar");
            Send("Unhide", "UpdateProgressBarFilled");
            Send("Unhide", "UpdateProgressText");
            Send("UpdateProgressText", "0%");

            // Set progress bar value
            OpenHome.Xapp.JsonObject joUpdateProg = new OpenHome.Xapp.JsonObject();
            joUpdateProg.Add("Id", new OpenHome.Xapp.JsonValueString("UpdateProgressBarFilled"));
            joUpdateProg.Add("Value", new OpenHome.Xapp.JsonValueString("0%"));
            Send("UpdateProgress", joUpdateProg);
        }
        public void OverallProgress(int aValue) {
            Send("UpdateProgressText", aValue.ToString() + "%");

            // Set progress bar value
            OpenHome.Xapp.JsonObject joUpdateProg = new OpenHome.Xapp.JsonObject();
            joUpdateProg.Add("Id", new OpenHome.Xapp.JsonValueString("UpdateProgressBarFilled"));
            joUpdateProg.Add("Value", new OpenHome.Xapp.JsonValueString(aValue.ToString() + "%"));
            Send("UpdateProgress", joUpdateProg);
        }
        public void Status(string aMessage) {
        }
        public void Completed() {
            Send("UpdateBoxTitle", "Update Complete");
            Send("UpdateBoxText", "Your device is now up-to-date.");
            SetButtonState(true, false);

            // Set progress bar hidden
            Send("Hide", "UpdateProgressBar");
            Send("Hide", "UpdateProgressBarFilled");
            Send("Hide", "UpdateProgressText");
        }
        public void Error(string aMessage) {
            UserLog.WriteLine("Update failed: " + aMessage);
            Send("UpdateBoxTitle", "Update Failed");
            Send("UpdateBoxText", "No need to panic. Click on the Need Help? link above for information on what to do next.");
            SetButtonState(false, true, kUpdateButtonTextTryUpdateAgain);

            // Set progress bar hidden
            Send("Hide", "UpdateProgressBar");
            Send("Hide", "UpdateProgressBarFilled");
            Send("Hide", "UpdateProgressText");
        }

        private const string kUpdateButtonTextStartUpdate = "Start Update";
        private const string kUpdateButtonTextTryUpdateAgain = "Try Again";
        private const string kUpdateButtonCheckForUpdatesAgain = "Check Again";
        private string iUpdateButtonText = "";
        private Box iSelectedBox;
    }
}
