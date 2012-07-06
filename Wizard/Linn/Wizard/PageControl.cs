using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using OpenHome.Xapp;
using Linn.ProductSupport;
using Linn.ProductSupport.Diagnostics;
using Linn;

namespace Linn.Wizard
{
    public class PageControl
    {
        public EventHandler<EventArgs> EventCloseApplicationRequested;

        public PageNavigation iPageNavigation;
        PageComponents iPageComponents;
        List<PageComponents> iPageDefaults;
        List<Constant> iConstantList;
        Network iNetwork;
        Diagnostics iDiagnostics;
        string iSelectedMacAddress = "";


        public string ProductModel
        {
            get;
            set;
        }

        public Diagnostics Diagnostics()
        {
            return(iDiagnostics);
        }

        public Network Network()
        {
            return (iNetwork);
        }

        public Box SelectedBox
        {
            get
            {
                return(Box(iSelectedMacAddress));   
            }
            
            set
            {
                iSelectedMacAddress = value.MacAddress;
            }
        }

        private Box Box(string aMacAddress)
        {
            if (aMacAddress == "")
            {
                return(null);
            }
            return(iNetwork.Box(aMacAddress));
        }

        public string BoxRoom()
        {
            if (SelectedBox == null)
            {
                return ("");
            }
            else
            {
                return SelectedBox.Room;
            }
        }

        

        public PageControl(Helper aHelper, Framework aXapp, string aResourcePath, string aXmlFileName)
        {
            iNetwork = new Network(aHelper);
            iDiagnostics = new Diagnostics();


            foreach (NetworkInterface netInterface in iNetwork.NetworkInterfaceList())
            {
                iDiagnostics.Run(ETest.eDhcp, netInterface.Info.IPAddress.ToString());
                iDiagnostics.Run(ETest.eInternet, netInterface.Info.IPAddress.ToString());
                iDiagnostics.Run(ETest.eUpnp, netInterface.Info.IPAddress.ToString());
            }

            iPageNavigation = new PageNavigation();

            StreamReader streamReader = new StreamReader(aResourcePath + "/../" + aXmlFileName);
            String description = streamReader.ReadToEnd();
            streamReader.Close();

            aXapp.AddCss("Linn.css");
            
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(description);


            iConstantList = new List<Constant>();
            XmlNodeList constants = xmlDoc.SelectNodes("/DSWizard/Pages/Constants/Const");

            foreach (XmlNode n in constants)
            {                
                iConstantList.Add(new Constant(n));
            }


            iPageDefaults = new List<PageComponents>();
            XmlNodeList defaultsList = xmlDoc.SelectNodes("/DSWizard/Pages/Default");

            foreach (XmlNode defaults in defaultsList)
            {
                UpdatePageComponents(defaults);
                iPageDefaults.Add(iPageComponents);
            }


            XmlNodeList pageList = xmlDoc.SelectNodes("/DSWizard/Pages/Page");
            bool firstPage = true;
            foreach (XmlNode page in pageList)
            {
                UpdatePageComponents(page);
                if (firstPage == true)
                {
                    firstPage = false;
                    iPageNavigation.Set(iPageComponents.PageName);
                }
                if (iPageComponents.PageTag != "")
                {
                    iPageNavigation.AddTag(iPageComponents.PageTag, iPageComponents.PageName);
                }
                switch (iPageComponents.PageType)
                {
                    case "Main":
                        aXapp.AddPage(new MainPage(this, "Main", iPageComponents, iPageNavigation));
                        break;
                    case "Standard":
                        aXapp.AddPage(new StandardPage(this, "Standard", iPageComponents, iPageNavigation));
                        break;
                    case "Discovery":
                        aXapp.AddPage(new DiscoveryPage(this, "Discovery", iPageComponents, iPageNavigation));
                        break;
                    case "Reprogram":
                        aXapp.AddPage(new ReprogramPage(this, "Reprogram", iPageComponents, iPageNavigation));
                        break;
                    case "Stream":
                        aXapp.AddPage(new StreamPage(this, "Stream", iPageComponents, iPageNavigation));
                        break;
                    case "Customise":
                        aXapp.AddPage(new HelpPage(this, "Customise", iPageComponents, iPageNavigation));
                        break;
                    case "Help":
                        aXapp.AddPage(new HelpPage(this, "Help", iPageComponents, iPageNavigation));
                        break;
                    case "Use":
                        aXapp.AddPage(new UsePage(this, "Use", iPageComponents, iPageNavigation));
                        break;
                }
            }
        }

        public void Close()
        {
            iDiagnostics.Shutdown();
            iNetwork.Close();
        }

        void UpdatePageComponents(XmlNode aPageNode)
        {
            iPageComponents = new PageComponents();

            foreach (PageComponents defaults in iPageDefaults)
            {
                if ((defaults.PageDefault == "All") || (defaults.PageDefault == ExtractData(aPageNode, "Defaults")))
                {
                    iPageComponents = new PageComponents(defaults); // replace page with defaults
                }
            }

            iPageComponents.PageName = ExtractData(aPageNode, "Name");
            iPageComponents.PageType = ExtractData(aPageNode, "Type");
            iPageComponents.PageTag = ExtractData(aPageNode, "Tag");
            iPageComponents.PageDefault = ExtractData(aPageNode, "Defaults");

            UpdateComponents(aPageNode, "Text");
            UpdateComponents(aPageNode, "Image");
            UpdateComponents(aPageNode, "Control");
            UpdateComponents(aPageNode, "Action");
            UpdateComponents(aPageNode, "Special");
        }

        private string ExtractData(XmlNode aPageNode, string aItem)
        {
            if (aPageNode.Attributes != null && aPageNode.Attributes.GetNamedItem(aItem) != null)
            {
                return aPageNode.Attributes.GetNamedItem(aItem).Value;
            }
            else
            {
                if (aPageNode[aItem] != null)
                {
                    return aPageNode[aItem].InnerText;
                }
            }

            return "";
        }

        private void UpdateComponents(XmlNode pageNode, string aComponentType)
        {
            XmlNodeList componentList = pageNode.SelectNodes(aComponentType);
            foreach (XmlNode node in componentList)
            {
                string name = "";

                XmlNode childNode = node.FirstChild;
                if (childNode != null)
                {
                    name = childNode.InnerText;
                }

                if ((childNode == null) || (childNode.Name == "#text")) // defined as attribute rather than new element
                {
                    iPageComponents.Add(aComponentType, new Component(name, node.Attributes, iConstantList));
                }
                else
                {
                    while (childNode != null)
                    {
                        iPageComponents.Add(aComponentType, new Component(childNode.Name, childNode.InnerText, childNode.Attributes, iConstantList));
                        childNode = childNode.NextSibling;
                    }
                }            

            }

        }
    }

        

    public interface IPageNavigation
    {
        void Next(Session aSession, string aPageName);
        void NextNoSave(Session aSession, string aPageName);
        void Previous(Session aSession);
        void RollBack();
        void GotoTag(Session aSession, string aPageTag);
        string PreviousPageName();
    }


    public class PageNavigation : IPageNavigation
    {
        private List<string> iPageHistory;
        List<PageTag> iTagList;

        public string PreviousPageName()
        {
            if (iPageHistory.Count > 1)
            {
                return (iPageHistory[iPageHistory.Count - 2]);
            }
            else
            {
                return("");
            }
        }
        
        
        
        public PageNavigation()
        {
            iPageHistory = new List<string>();
            iTagList = new List<PageTag>();
        }

        public void Set(string aPageName)
        {
            iPageHistory.Add(aPageName);    // initialise page history
        }

        public void Next(Session aSession, string aPageName)
        {
            iPageHistory.Add(aPageName);    // save page in history
            aSession.Navigate(aPageName);
        }

        public void NextNoSave(Session aSession, string aPageName) {
            aSession.Navigate(aPageName);
        }

        public void Previous(Session aSession)
        {
            if (iPageHistory.Count <= 1)
                return; // at first page

            iPageHistory.RemoveAt(iPageHistory.Count - 1);  // remove this page from history
            if (iPageHistory.Count > 0)
            {
                string pageName = iPageHistory[iPageHistory.Count - 1];    // go to last page in history
                aSession.Navigate(pageName);
            }
        }

        public void GotoTag(Session aSession, string aPageTag)
        {

            string pageName = PageTagToName(aPageTag);

            if (pageName != "")
            {
                iPageHistory.Add(pageName);    // save page in history - may need to do something different with history???!!!
                aSession.Navigate(pageName);
            }
        }

        public void RollBack()
        {
            if (iPageHistory.Count == 0)
                return; // no history

            string currentPageName = iPageHistory[iPageHistory.Count - 1];

            for (int i = iPageHistory.Count - 1; i > 0; i--)
            {
                if (iPageHistory[i - 1] == currentPageName)
                {
                    iPageHistory.RemoveRange(i - 1, iPageHistory.Count - i); // roll back to previous occurrence of this page
                }
            }
        }



        class PageTag
        {
            string iTag;
            string iName;

            public PageTag(string aTag, string aName)
            {
                iTag = aTag;
                iName = aName;
            }

            public string Tag
            {
                get { return iTag; }
            }
            public string Name
            {
                get { return iName; }
            }

        }


        public string PageTagToName(string aTag)
        {
            foreach (PageTag tag in iTagList)
            {
                if (tag.Tag == aTag)
                {
                    return tag.Name;
                }
            }
            return "";
        }
                
        public void AddTag(string aTag, string aName)
        {
            PageTag tag = new PageTag(aTag, aName);
            iTagList.Add(tag);
        }
    }


    public class Constant
    {
        string iId;
        string iValue;

        public Constant(XmlNode node)
        {
            XmlNode id = node.Attributes.GetNamedItem("id");
            if (id != null)
            {
                iId = "#" + id.Value;
                iValue = node.InnerXml;
            }
        }

        public string Id
        {
            get { return iId; }
        }
        public string Value
        {
            get { return iValue; }
        }
    }
}
