using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;
using Linn.ProductSupport.Diagnostics;

namespace Linn.Wizard
{
    public class DiscoveryPage : BasePage
    {

        string iMacAddress;
        BoxSelection iBoxSelection;  //only supports a single session
        int iListCount;
        bool iShowSelected;

        public DiscoveryPage(PageControl aPageControl, PageDefinitions.Page aPageDefinition)
            : base(aPageControl, aPageDefinition)
        {
        }
        

        protected override void OnActivated(Session aSession)
        {
            iShowSelected = true;

            string selectedProduct = aSession.Model.SelectedProduct;
            string modelSelected = "";

            if (selectedProduct == "KlimaxDsm") {
                modelSelected = "Klimax DSM";
            }
            else if (selectedProduct == "AkurateDsm") {
                modelSelected = "Akurate DSM";
            }
            else if (selectedProduct == "MajikDsm") {
                modelSelected = "Majik DSM";
            }
            else if (selectedProduct == "KikoDsm") {
                modelSelected = "Kiko DSM";
            }
            iBoxSelection = new BoxSelection(aSession, modelSelected);
            
            base.OnActivated(aSession);
            ShowDeviceList();
            SetSelected(aSession);
            if (aSession.Tracking)
            {
                aSession.Send("BoxTrackingData", GetBoxTracking(ModelInstance.Instance.Network.BoxList()));
            }
        }

        private void ShowDeviceList()
        {
            iBoxSelection.UpdateBoxList(ModelInstance.Instance.Network.BoxList());
            iBoxSelection.SetSelectedBox(iMacAddress);
            iBoxSelection.ShowButtons();
        }

        protected override void OnDeactivated(Session aSession)
        {
            Console.WriteLine("ReprogramPage OnDeactivated");

            iBoxSelection.Hide();
            base.OnDeactivated(aSession);
        }
            
        private static Mutex iRxMutex = new Mutex();

        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            switch (aName)
            {                    
                case "DeviceButton1":
                case "DeviceButton2":
                case "DeviceButton3":
                case "DeviceButton4":
                case "DeviceButton5":
                    string s = aName.Replace("DeviceButton", "");
                    int button = Convert.ToInt32(s);
                    iMacAddress = iBoxSelection.MacAddressOfButton(button);
                    if (iMacAddress.Length > 0) {
                        SetSelected(aSession);
                        iBoxSelection.SetSelectedBox(iMacAddress);
                        iBoxSelection.ShowButtons();

                        Box box = ModelInstance.Instance.Network.Box(iMacAddress);
                        string deviceIpStr = box.IpAddress;
                        System.Net.IPAddress interfaceIp = box.NetworkInterfaceIpAddress();
                        ModelInstance.Instance.Network.SetNetworkInterface(interfaceIp);
                        string interfaceIpStr = interfaceIp.ToString();
                        ModelInstance.Instance.Diagnostics.Run(ETest.eMulticastFromDs, interfaceIpStr, deviceIpStr);
                        ModelInstance.Instance.Diagnostics.Run(ETest.eMulticastToDs, interfaceIpStr, deviceIpStr);
                        ModelInstance.Instance.Diagnostics.Run(ETest.eUdpEcho, interfaceIpStr, deviceIpStr);
                        ModelInstance.Instance.Diagnostics.Run(ETest.eTcpEcho, interfaceIpStr, deviceIpStr);
                    }
                    break;

                case "Refresh":
                    ModelInstance.Instance.Network.Refresh();

                    Send("Disable", "TroubleshootButton");
                    Send("Disable", "PreviousButton");
                    Send("Disable", "NavTab1");

                    Send("Hide", "RefreshButton");
                    Send("Unhide", "DiscoveryRefreshSpinner");
                    Thread.Sleep(4000);
                    ShowDeviceList();
                    Send("Hide", "DiscoveryRefreshSpinner");
                    Send("Unhide", "RefreshButton");

                    Send("Enable", "TroubleshootButton");
                    Send("Enable", "PreviousButton");
                    Send("Enable", "NavTab1");
                    
                    break;

                case "PageUp":
                    iBoxSelection.PreviousButtonPage();
                    iBoxSelection.ShowButtons();
                    break;

                case "PageDown":
                    iBoxSelection.NextButtonPage();
                    iBoxSelection.ShowButtons();
                    break;

                case "ListChange":
                    iRxMutex.WaitOne();
                    switch (aValue)
                    {
                        case "Added":
                            iListCount++;
                            break;

                        case "Removed":
                            iListCount--;
                            break;

                        case "Cleared":
                            Console.WriteLine("SelectionChange {0} count {1}", aValue, iListCount);
                            iListCount = 0;
                            break;
                    }
                    iRxMutex.ReleaseMutex();
                    break;

                default:
                    
                
                    base.OnReceive(aSession, aName, aValue);
                    break;
            }
        }

        private void SetSelected(Session aSession)
        {
            if ((iMacAddress != null) && (iMacAddress != ""))
            {
                Box box = iBoxSelection.Box(iMacAddress);

                if ((aSession.Model.SelectedBoxMacAddress != box.MacAddress) || (iShowSelected == true))
                {
                    iShowSelected = false;
                    aSession.Model.SelectedBoxMacAddress = box.MacAddress;

                    // make 'next' button available
                    OpenHome.Xapp.JsonObject jo = new OpenHome.Xapp.JsonObject();
                    jo.Add("Type", new OpenHome.Xapp.JsonValueString("Control"));
                    jo.Add("Id", new OpenHome.Xapp.JsonValueString("NextButton"));
                    jo.Add("Visible", new OpenHome.Xapp.JsonValueBool(true));
                    aSession.Send("Render", jo);

                    iBoxSelection.ShowButtons();    // update display
                }
            }
        }


        private OpenHome.Xapp.JsonObject GetBoxTracking(List<Box> aBoxList)
        {
            Dictionary<string, int> modelCount = new Dictionary<string, int>();
            Dictionary<string, int> softwareVersionCount = new Dictionary<string, int>();
            foreach (Box b in aBoxList)
            {
                if (!modelCount.ContainsKey(b.Model))
                {
                    modelCount.Add(b.Model, 1);
                }
                else
                {
                    modelCount[b.Model] += 1;
                }

                string softwareVersion = b.SoftwareVersion;
                // log nulls and blanks as Unknown
                if (softwareVersion == null || softwareVersion == String.Empty)
                {
                    softwareVersion = "Unknown";
                }
                if (!softwareVersionCount.ContainsKey(softwareVersion))
                {
                    softwareVersionCount.Add(softwareVersion, 1);
                }
                else
                {
                    softwareVersionCount[softwareVersion] += 1;
                }
            }
            var resultJson = new OpenHome.Xapp.JsonObject();

            var modelsJson = new OpenHome.Xapp.JsonArray<OpenHome.Xapp.JsonObject>();
            foreach (string modelName in modelCount.Keys)
            {
                OpenHome.Xapp.JsonObject modelJson = new OpenHome.Xapp.JsonObject();
                modelJson.Add("Name", new OpenHome.Xapp.JsonValueString(modelName));
                modelJson.Add("Value", new OpenHome.Xapp.JsonValueInt(modelCount[modelName]));
                modelsJson.Add(modelJson);
            }
            resultJson.Add("Models", modelsJson);

            var versionsJson = new OpenHome.Xapp.JsonArray<OpenHome.Xapp.JsonObject>();
            foreach (string version in softwareVersionCount.Keys)
            {
                OpenHome.Xapp.JsonObject versionJson = new OpenHome.Xapp.JsonObject();
                versionJson.Add("Name", new OpenHome.Xapp.JsonValueString(version));
                versionJson.Add("Value", new OpenHome.Xapp.JsonValueInt(softwareVersionCount[version]));
                versionsJson.Add(versionJson);
            }
            resultJson.Add("Versions", versionsJson);

            return resultJson;
        }



        private static int CompareByRoom(Box r1, Box r2)
        {
            return r1.Room.CompareTo(r2.Room);
        }

        private class BoxSelection
        {
            List<Box> iBoxListDisplayed;  // may be able to use contents of listbox rather than keeping a seperate copy
            string[] iMacAddressList;
            Session iSession;
            bool iActive;
            int iButtonPage;
            int iButtonsPerPage = 5;
            string iSelectedMacAddress;
            string iSelectedProduct;

            public BoxSelection(Session aSession, string aSelectedProduct)
            {
                iActive = false;
                iSession = aSession;
                iBoxListDisplayed = new List<Box>();
                iMacAddressList = new string[5];
                iButtonPage = 0;
                iSelectedMacAddress = "";
                iSelectedProduct = aSelectedProduct;
            }

            public void UpdateBoxList(List<Box> aBoxList)
            {
                iButtonPage = 0;
                iBoxListDisplayed = new List<Box>(aBoxList);   // take a snapshot of the discovered devices
            }

            public void SetSelectedBox(string aMacAddress)
            {
                iSelectedMacAddress = aMacAddress;
            }

            public void NextButtonPage()
            {
                if (((iButtonPage+1) * iButtonsPerPage) < iBoxListDisplayed.Count)
                {
                    iButtonPage++;
                }
            }
            public void PreviousButtonPage()
            {
                if (iButtonPage > 0)
                {
                    iButtonPage--;
                }
            }
            public void ShowButtons()
            {
                iBoxListDisplayed.Sort(CompareByRoom);
                int index = 0;
                int button = 0;
                bool morepages = false;
                foreach (Box box in iBoxListDisplayed)
                {
                    if (box.Model == iSelectedProduct && box.State == Linn.ProductSupport.Box.EState.eOn)
                    //if (box.Room.Contains("Jim") || box.Room.Contains("EPB"))
                    {
                        // filter out fake Majik DSM devices (non-HDMI)
                        if (box.Model == "Majik DSM" && box.BoardDescription.Length > 0) {
                            if (box.BoardDescription[0].ToLowerInvariant() == "2011 variant") {
                                continue;
                            }
                        }
                        index++;
                        //limit to current page of 5
                        if (index > (iButtonPage * iButtonsPerPage))
                        {
                            if (button >= iButtonsPerPage)
                            {
                                morepages = true;
                                break;
                            }
                            else
                            {
                                bool selected = (iSelectedMacAddress == box.MacAddress) ? true : false;
                                bool newbox = (box.Room == "Main Room") ? true : false;
                                string color = selected ? "white" : "black";
                                string backgroundcolor = selected ? "rgb(223, 80, 92)" : "#c6bfb7";
                                iMacAddressList[button] = box.MacAddress;     // use mac address to identify button
                                button++;

                                OpenHome.Xapp.JsonObject jo = new OpenHome.Xapp.JsonObject();
                                jo.Add("Button", new OpenHome.Xapp.JsonValueString("DeviceButton" + button));
                                jo.Add("Id", new OpenHome.Xapp.JsonValueString(box.MacAddress));
                                jo.Add("Room", new OpenHome.Xapp.JsonValueString(box.Room));
                                jo.Add("Name", new OpenHome.Xapp.JsonValueString(box.Name));
                                jo.Add("Selected", new OpenHome.Xapp.JsonValueBool(selected));
                                jo.Add("New", new OpenHome.Xapp.JsonValueBool(newbox));
                                jo.Add("Color", new OpenHome.Xapp.JsonValueString(color));
                                jo.Add("BackgroundColor", new OpenHome.Xapp.JsonValueString(backgroundcolor));

                                iSession.Send("DeviceButtonUpdate", jo);
                            }
                        }
                    }
                }
                while (button < iButtonsPerPage)    // clear out remaining buttons
                {
                    iMacAddressList[button % iButtonsPerPage] = "";     // use mac address to identify button
                    button++;

                    OpenHome.Xapp.JsonObject jo = new OpenHome.Xapp.JsonObject();
                    jo.Add("Button", new OpenHome.Xapp.JsonValueString("DeviceButton" + button));
                    jo.Add("Id", new OpenHome.Xapp.JsonValueString(""));
                    jo.Add("Room", new OpenHome.Xapp.JsonValueString(""));
                    jo.Add("Name", new OpenHome.Xapp.JsonValueString(""));
                    jo.Add("Selected", new OpenHome.Xapp.JsonValueBool(false));
                    jo.Add("New", new OpenHome.Xapp.JsonValueBool(false));
                    jo.Add("Color", new OpenHome.Xapp.JsonValueString("gray"));
                    jo.Add("BackgroundColor", new OpenHome.Xapp.JsonValueString("#c6bfb7"));

                    iSession.Send("DeviceButtonUpdate", jo);
                }

                bool visible = (morepages || iButtonPage > 0);
                
                // 'previous page' button availability
                OpenHome.Xapp.JsonObject joUp = new OpenHome.Xapp.JsonObject();
                joUp.Add("Type", new OpenHome.Xapp.JsonValueString("Control"));
                joUp.Add("Id", new OpenHome.Xapp.JsonValueString("DeviceListPageUp"));
                joUp.Add("Visible", new OpenHome.Xapp.JsonValueBool(visible));
                joUp.Add("Enabled", new OpenHome.Xapp.JsonValueBool(iButtonPage > 0));
                iSession.Send("Render", joUp);

                // 'next page' button availability
                OpenHome.Xapp.JsonObject joDown = new OpenHome.Xapp.JsonObject();
                joDown.Add("Type", new OpenHome.Xapp.JsonValueString("Control"));
                joDown.Add("Id", new OpenHome.Xapp.JsonValueString("DeviceListPageDown"));
                joDown.Add("Visible", new OpenHome.Xapp.JsonValueBool(visible));
                joDown.Add("Enabled", new OpenHome.Xapp.JsonValueBool(morepages));
                iSession.Send("Render", joDown);

            }


            public string MacAddressOfButton(int aButton)
            {
                return(iMacAddressList[aButton-1]);
            }

            public void Hide()
            {
                iActive = false;
                iBoxListDisplayed.Clear();
            }

            public bool Active
            {
                get
                {
                    return iActive;
                }  
            }


            public Box Box(string aMacAddress)
            {
                foreach (Box b in iBoxListDisplayed)
                {
                    if (b.MacAddress == aMacAddress)
                    {
                        return b;
                    }
                }
                return null;
            }

            
        
        }
        
    }
 
}
