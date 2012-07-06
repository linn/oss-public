using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using Linn.ProductSupport.Diagnostics;

using OpenHome.Xapp;
using Linn.ProductSupport;

namespace Linn.Wizard
{
    public class BasePage : Page
    {
        protected PageControl iPageControl; 
        protected PageComponents iPageComponents;
        protected IPageNavigation iPageNavigation;
        List<string> iDeviceUpdates;

        protected BasePage(PageControl aPageControl, string aViewId, PageComponents aPageComponents, IPageNavigation aPageNavigation)
            : base(aPageComponents.PageName, aViewId)
        {
            iPageControl = aPageControl;
            iPageComponents = aPageComponents;
            iPageNavigation = aPageNavigation;

            iDeviceUpdates = new List<string>();
            iPageControl.Network().GetNetworkChangeWatcher().EventNetworkChanged += NetworkChangedHandler;
        }

        private void NetworkChangedHandler(object sender, EventArgs e)
        {
            Send("NetworkChangeExit", "");
        }
        

        protected void ResetDeviceParameters(Session aSession) 
        {
            iDeviceUpdates.Clear();        
        }

        protected void RenderPage(Session aSession)
        {
            RenderControls(aSession, "Text");
            RenderControls(aSession, "Image");
            RenderControls(aSession, "Control");
            RenderControls(aSession, "Special");


            aSession.Send("EnableBreadcrumb", "");
        }

        protected void RenderControls(Session aSession, string aType)
        {
            List<Component> iList;

            iList = iPageComponents.GetList(aType);
            foreach (Component c in iList)
            {
                JsonObject jo = new JsonObject();
                jo.Add("Type", new JsonValueString(aType));
                jo.Add("Id", new JsonValueString(c.Id));
                jo.Add("Visible", new JsonValueBool(c.Visible));
                jo.Add("Enabled", new JsonValueBool(c.Enabled));
                jo.Add("Displayed", new JsonValueBool(c.Displayed));
                jo.Add("Top", new JsonValueString(c.Top));
                jo.Add("Left", new JsonValueString(c.Left));
                jo.Add("HeightSet", new JsonValueBool(c.HeightSet));
                jo.Add("Height", new JsonValueString(c.Height));
                jo.Add("Image", new JsonValueString(c.Image));
                jo.Add("BackgroundImage", new JsonValueString(c.BackgroundImage));
                jo.Add("Color", new JsonValueString(c.Color));
                jo.Add("BackgroundColor", new JsonValueString(c.BackgroundColor));
                jo.Add("CustomAction", new JsonValueString(c.CustomAction));


                string text;
                if (c.Var == "")
                {
                    text = c.Parameter;
                }
                else if (iPageControl.SelectedBox == null)
                {
                    text = "****";
                }
                else
                {
                    jo.Add("Class", new JsonValueString(c.Var));    // use class to identify service for parameter update
                    string[] target = c.Var.Split('-');      // defined in xml
                    switch (target[0])
                    {
                        case "RoomName":
                            text = iPageControl.SelectedBox.Room;
                            break;
                        case "SourceName":
                            SourceInfo nameInfo = iPageControl.SelectedBox.BasicSetup.SourceInfoAt(target[1]);
                            if (nameInfo == null)
                            {
                                text = "unknown";
                            }
                            else
                            {
                                text = nameInfo.Name;
                            }
                            break;
                        case "SourceIcon":
                            SourceInfo iconInfo = iPageControl.SelectedBox.BasicSetup.SourceInfoAt(target[1]);
                            text = "";
                            if (iconInfo != null)
                            {
                           //     jo.Add("BackgroundImage", new JsonValueString(iconInfo.Icon.ImageUri));
                            }
                            break;
                        default:
                            text = "unknown"; //iVar == DS service name to get string from
                            break;
                    }
                }
                jo.Add("Text", new JsonValueString(text));


                aSession.Send("Render", jo);
                
                Console.WriteLine("{0} id [{1}]:  message: {2}", aType, c.Id, c.Parameter);
            }
        }
        

        protected override void OnActivated(Session aSession)
        {
            RenderPage(aSession);
            iPageNavigation.RollBack();
        }


        protected override void OnDeactivated(Session aSession)
        {           
        }
        

        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            Console.WriteLine(string.Format("Name: {0} Value: {1}", aName, aValue));

            switch (aName)
            {
                case "NextPage":
                    // save any changes to device parameters
                               
                    Box box = null;
                    if (iPageControl.SelectedBox != null)
                    {
                        box = iPageControl.SelectedBox;
                    }
                    if (box != null) // ignore if no device selected
                    {
                        foreach (string s in iDeviceUpdates)
                        {
                            string[] parameters = s.Split(',');
                            string[] service = null;

                                        // xaml framework adds + when it replaces space when constructing uri, so split on + 
                            string[] htmlclass = parameters[0].Split(' '); 
                            for (int i = 0; i < htmlclass.Length; i++)
                            {
                                if (htmlclass[i].StartsWith("Service_"))
                                {
                                    if (i < htmlclass.Length-1)
                                    {
                                        htmlclass[i] += " ";
                                        htmlclass[i] += htmlclass[i+1]; //need to do this hack because there can be spaces in the source id!!
                                    }
                                    service = htmlclass[i].Split('_');
                                    break;
                                }
                            }

                            if (service != null)
                            {
                                string[] target = service[2].Split('-');      // defined in xml
                                string p1 = parameters[1];
                                switch (target[0])
                                {
                                    case "RoomName":
                                        box.BasicSetup.SetRoom(p1);
                                        break;
                                    case "SourceName":
                                        box.BasicSetup.SetSourceName(target[1], p1);  //source, name
                                        break;
                                    case "SourceIcon":
                                        box.BasicSetup.SetSourceIcon(target[1], p1);  //source, icon url
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    iDeviceUpdates.Clear();

                    if (iPageComponents.Next != "")
                    {
                        Console.WriteLine(string.Format("Name: {0}  current id {1}, next id {2}", aName, aSession.Id, iPageComponents.Next));
                        iPageNavigation.Next(aSession, iPageComponents.Next);
                    }
                    break;
                case "PreviousPage":
                        Console.WriteLine(string.Format("Name: {0}  current id {1} -> previous", aName, aSession.Id));
                        iPageNavigation.Previous(aSession);
                    break;
                case "CustomAction":
                    string customValue;
                    switch (iPageComponents.CustomAction(aValue, out customValue))
                    {
                        case "goto":
                            iPageNavigation.Next(aSession, customValue);
                            break;
                    }
                    break;
                case "TroubleshootButton":
                    if (iPageComponents.Diagnostics != "")
                    {
                        iPageNavigation.Next(aSession, iPageComponents.Diagnostics);
                    }
                    break;
                case "HelpMenuItem1":
                    if (iPageComponents.MenuItem1 != "")
                    {
                        iPageNavigation.NextNoSave(aSession, iPageComponents.MenuItem1);
                    }
                    break;
                case "HelpMenuItem2":
                    if (iPageComponents.MenuItem2 != "")
                    {
                        iPageNavigation.NextNoSave(aSession, iPageComponents.MenuItem2);
                    }
                    break;
                case "HelpMenuItem3":
                    if (iPageComponents.MenuItem3 != "")
                    {
                        iPageNavigation.NextNoSave(aSession, iPageComponents.MenuItem3);
                    }
                    break;
                case "HelpMenuItem4":
                    if (iPageComponents.MenuItem4 != "")
                    {
                        iPageNavigation.NextNoSave(aSession, iPageComponents.MenuItem4);
                    }
                    break;
                case "HelpMenuItem5":
                    if (iPageComponents.MenuItem5 != "")
                    {
                        iPageNavigation.NextNoSave(aSession, iPageComponents.MenuItem5);
                    }
                    break;
                case "HelpMenuItem6":
                    if (iPageComponents.MenuItem6 != "")
                    {
                        iPageNavigation.NextNoSave(aSession, iPageComponents.MenuItem6);
                    }
                    break;
                case "HelpMenuItem7":
                    if (iPageComponents.MenuItem7 != "") {
                        iPageNavigation.NextNoSave(aSession, iPageComponents.MenuItem7);
                    }
                    break;
                case "SelectProductModel":
                    iPageControl.ProductModel = aValue ;
                    break;
                case "GotoTag":
                    iPageNavigation.GotoTag(aSession, aValue);
                    break;
                case "GotoPage":
                    iPageNavigation.Next(aSession, aValue);
                    break;
                case "ResetDeviceParameter":                   
                    iDeviceUpdates.Clear();         // clear stored updates (e.g. when different device selected)
                    break;
                case "UpdateDeviceParameter":
                    iDeviceUpdates.Add(aValue);     // store updates to be applied on close (should check for duplicates)
                    break;
                case "CloseApplication":
                    if (iPageControl.EventCloseApplicationRequested != null) {
                        iPageControl.EventCloseApplicationRequested(this, EventArgs.Empty);
                    }
                    break;
                case "LinkClicked":
                    System.Diagnostics.Process.Start(aValue);
                    break;
                case "Special":
                    switch (aValue)
                    {
                        case "HistoryReset":
 //always do this!                           iPageNavigation.RollBack();
                            break;
                    }
                    break;
            }
        }

    }
}
