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
using Linn.ProductSupport.Diagnostics;

namespace Linn.Wizard
{
 

    public class DiscoveryPage : BasePage
    {

        string iMacAddress;
        BoxSelection iBoxSelection;  //only supports a single session
        int iListCount;
        bool iShowSelected;

        public DiscoveryPage(PageControl aPageControl, string aViewId, PageComponents aPageComponents, IPageNavigation aPageNavigation)
            : base(aPageControl, aViewId, aPageComponents, aPageNavigation)
        {
        }
        

        protected override void OnActivated(Session aSession)
        {
            iShowSelected = true;

            string modelSelected = "";
            if (iPageComponents.PageDefault == "KlimaxDsm") {
                modelSelected = "Klimax DSM";
            }
            else if (iPageComponents.PageDefault == "AkurateDsm") {
                modelSelected = "Akurate DSM";
            }
            else if (iPageComponents.PageDefault == "MajikDsm") {
                modelSelected = "Majik DSM";
            }
            else if (iPageComponents.PageDefault == "KikoDsm") {
                modelSelected = "Kiko DSM";
            }
            iBoxSelection = new BoxSelection(aSession, modelSelected);
            
            base.OnActivated(aSession);
            ShowDeviceList();
            SetSelected(aSession);
        }

        private void ShowDeviceList()
        {
            iBoxSelection.UpdateBoxList(iPageControl.Network().BoxList());
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

                        Box box = iPageControl.Network().Box(iMacAddress);
                        string deviceIpStr = box.IpAddress;
                        System.Net.IPAddress interfaceIp = box.NetworkInterfaceIpAddress();
                        iPageControl.Network().SetNetworkInterface(interfaceIp);
                        string interfaceIpStr = interfaceIp.ToString();
                        iPageControl.Diagnostics().Run(ETest.eMulticastFromDs, interfaceIpStr, deviceIpStr);
                        iPageControl.Diagnostics().Run(ETest.eMulticastToDs, interfaceIpStr, deviceIpStr);
                        iPageControl.Diagnostics().Run(ETest.eUdpEcho, interfaceIpStr, deviceIpStr);
                        iPageControl.Diagnostics().Run(ETest.eTcpEcho, interfaceIpStr, deviceIpStr);
                    }
                    break;

                case "Refresh":
                    iPageControl.Network().Refresh();

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

                case "NextPage":
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

                if ((iPageControl.SelectedBox != box) || (iShowSelected == true))
                {
                    iShowSelected = false;
                    iPageControl.SelectedBox = box;

                    // make 'next' button available
                    JsonObject jo = new JsonObject();
                    jo.Add("Type", new JsonValueString("Control"));
                    jo.Add("Id", new JsonValueString("NextButton"));
                    jo.Add("Visible", new JsonValueBool(true));
                    aSession.Send("Render", jo);

                    iBoxSelection.ShowButtons();    // update display
                }
            }
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
                    if (box.Model == iSelectedProduct && box.State == ProductSupport.Box.EState.eOn)
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

                                JsonObject jo = new JsonObject();
                                jo.Add("Button", new JsonValueString("DeviceButton" + button));
                                jo.Add("Id", new JsonValueString(box.MacAddress));
                                jo.Add("Room", new JsonValueString(box.Room));
                                jo.Add("Name", new JsonValueString(box.Name));
                                jo.Add("Selected", new JsonValueBool(selected));
                                jo.Add("New", new JsonValueBool(newbox));
                                jo.Add("Color", new JsonValueString(color));
                                jo.Add("BackgroundColor", new JsonValueString(backgroundcolor));

                                iSession.Send("DeviceButtonUpdate", jo);
                            }
                        }
                    }
                }
                while (button < iButtonsPerPage)    // clear out remaining buttons
                {
                    iMacAddressList[button % iButtonsPerPage] = "";     // use mac address to identify button
                    button++;

                    JsonObject jo = new JsonObject();
                    jo.Add("Button", new JsonValueString("DeviceButton" + button));
                    jo.Add("Id", new JsonValueString(""));
                    jo.Add("Room", new JsonValueString(""));
                    jo.Add("Name", new JsonValueString(""));
                    jo.Add("Selected", new JsonValueBool(false));
                    jo.Add("New", new JsonValueBool(false));
                    jo.Add("Color", new JsonValueString("gray"));
                    jo.Add("BackgroundColor", new JsonValueString("#c6bfb7"));

                    iSession.Send("DeviceButtonUpdate", jo);
                }

                bool visible = (morepages || iButtonPage > 0);
                
                // 'previous page' button availability
                JsonObject joUp = new JsonObject();
                joUp.Add("Type", new JsonValueString("Control"));
                joUp.Add("Id", new JsonValueString("DeviceListPageUp"));
                joUp.Add("Visible", new JsonValueBool(visible));
                joUp.Add("Enabled", new JsonValueBool(iButtonPage > 0));
                iSession.Send("Render", joUp);

                // 'next page' button availability
                JsonObject joDown = new JsonObject();
                joDown.Add("Type", new JsonValueString("Control"));
                joDown.Add("Id", new JsonValueString("DeviceListPageDown"));
                joDown.Add("Visible", new JsonValueBool(visible));
                joDown.Add("Enabled", new JsonValueBool(morepages));
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
