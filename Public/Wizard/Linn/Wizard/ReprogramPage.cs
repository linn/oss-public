using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using OpenHome.Xapp;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;

namespace Linn.Wizard
{


    public class ReprogramPage : BasePage, IUpdateFirmwareHandler
    {
        public ReprogramPage(PageControl aPageControl, string aViewId, PageComponents aPageComponents, IPageNavigation aPageNavigation)
            : base(aPageControl, aViewId, aPageComponents, aPageNavigation)
        {
            
        }


        protected override void OnActivated(Session aSession)
        {
            Box box = iPageControl.SelectedBox;
            box.UpdateCheckCompleteEvent += UpdateCheckComplete;
            box.UpdateCheckErrorEvent += UpdateCheckError;

            base.OnActivated(aSession);

            SetInitialUpdateState();
        }

        private void SetInitialUpdateState() {
            bool nextButtonVisible = true;
            bool updateButtonVisible = false;
            string updateButtonText = "";

            Box box = iPageControl.SelectedBox;

            if (box == null)
            {
                Send("UpdateBoxTitle", "Update Not Possible");
                Send("UpdateBoxText", "Your device is no longer available.");
            }
            else if (box.State == Box.EState.eOff)
            {
                Send("UpdateBoxTitle", "Update Not Possible");
                Send("UpdateBoxText", "Your device is turned off.");
            }
            else if (box.IsProxy)
            {
                Send("UpdateBoxTitle", "Update Not Possible");
                Send("UpdateBoxText", "Your device can not be updated because it is a network proxy.");
            }
            else if (box.UpdateCheckInProgress)
            {
                Send("UpdateBoxTitle", "Update Check In Progress");
                Send("UpdateBoxText", "Update information is currently downloading.<br>Please wait.");
            }
            else if (box.UpdateCheckFailed)
            {
                Send("UpdateBoxTitle", "Update Check Failed");
                Send("UpdateBoxText", "Unable to download update information. Please check your internet connection.");

                updateButtonVisible = true;
                updateButtonText = kUpdateButtonCheckForUpdatesAgain;
            }
            else if (box.UpdateCheckDeviceNotFound)
            {
                Send("UpdateBoxTitle", "Update Not Possible");
                Send("UpdateBoxText", "No update information could be found for '" + box.Model + "'.");
            }
            else if (!box.SoftwareUpdateAvailable)
            {
                Send("UpdateBoxTitle", "No Update Is Required");
                Send("UpdateBoxText", "Your device is already running the latest software.");
            }
            else if (box.SoftwareUpdateAvailable)
            {
                Send("UpdateBoxTitle", "Update Available");
                Send("UpdateBoxText", "New software, " + box.SoftwareUpdateVersion + ", is available for your device. Click below to start the update. Please do not close the application while the update is in progress.");
                
                updateButtonVisible = true;
                updateButtonText = kUpdateButtonTextStartUpdate;
            }

            SetButtonState(nextButtonVisible, updateButtonVisible, updateButtonText);
        }

        private void SetButtonState(bool aNextPrevButtonsVisible, bool aUpdateButtonVisible) {
            SetButtonState(aNextPrevButtonsVisible, aUpdateButtonVisible, "");
        }

        private void SetButtonState(bool aNextPrevButtonsVisible, bool aUpdateButtonVisible, string aUpdateButtonText) {
            JsonObject joNew = new JsonObject();
            joNew.Add("Type", new JsonValueString("Control"));
            joNew.Add("Id", new JsonValueString("NextButton"));
            joNew.Add("Visible", new JsonValueBool(aNextPrevButtonsVisible));
            Send("Render", joNew);

            JsonObject joPrev = new JsonObject();
            joPrev.Add("Type", new JsonValueString("Control"));
            joPrev.Add("Id", new JsonValueString("PreviousButton"));
            joPrev.Add("Visible", new JsonValueBool(aNextPrevButtonsVisible));
            Send("Render", joPrev);

            JsonObject joUpdate = new JsonObject();
            joUpdate.Add("Type", new JsonValueString("Control"));
            joUpdate.Add("Id", new JsonValueString("UpdateButton"));
            joUpdate.Add("Text", new JsonValueString(aUpdateButtonText));
            joUpdate.Add("Visible", new JsonValueBool(aUpdateButtonVisible));
            Send("Render", joUpdate);

            iUpdateButtonText = aUpdateButtonText;

            JsonObject joUpdateBgd = new JsonObject();
            joUpdateBgd.Add("Type", new JsonValueString("Control"));
            joUpdateBgd.Add("Id", new JsonValueString("UpdateBoxFooter"));
            joUpdateBgd.Add("Visible", new JsonValueBool(aUpdateButtonVisible));
            Send("Render", joUpdateBgd);
        }

        private void UpdateCheckComplete(object obj, EventArgs e) {
            SetInitialUpdateState();
        }

        private void UpdateCheckError(object obj, EventArgsUpdateError e) {
            SetInitialUpdateState();
        }

        protected override void OnDeactivated(Session aSession)
        {
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
                        iPageControl.SelectedBox.CheckForUpdates();
                        SetInitialUpdateState();
                    }
                    break;
                
                default:
                    base.OnReceive(aSession, aName, aValue);
                    break;
            }
        }



        private void ReprogramDevice(Session aSession)
        {
            iPageControl.SelectedBox.UpdateFirmware(this, false);
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
            JsonObject joUpdateProg = new JsonObject();
            joUpdateProg.Add("Id", new JsonValueString("UpdateProgressBarFilled"));
            joUpdateProg.Add("Value", new JsonValueString("0%"));
            Send("UpdateProgress", joUpdateProg);
        }
        public void OverallProgress(int aValue) {
            Send("UpdateProgressText", aValue.ToString() + "%");

            // Set progress bar value
            JsonObject joUpdateProg = new JsonObject();
            joUpdateProg.Add("Id", new JsonValueString("UpdateProgressBarFilled"));
            joUpdateProg.Add("Value", new JsonValueString(aValue.ToString() + "%"));
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
    }
}
