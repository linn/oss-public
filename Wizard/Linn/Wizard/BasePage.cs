using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using Linn.ProductSupport.Diagnostics;
using Linn.ProductSupport;


namespace Linn.Wizard
{
    public class BasePage : Page
    {
        protected PageDefinitions.Page iPageDefinition;
        protected PageControl iPageControl;

        protected BasePage(PageControl aPageControl, PageDefinitions.Page aPageDefinition)
            : base(aPageDefinition)
        {
            iPageDefinition = aPageDefinition;
            iPageControl = aPageControl;

            ModelInstance.Instance.Network.GetNetworkChangeWatcher().EventNetworkChanged += NetworkChangedHandler;
        }

        private void NetworkChangedHandler(object sender, EventArgs e)
        {
            Send("NetworkChangeExit", "");
        }
        
        private void RenderPage(Session aSession)
        {
            RenderControls(aSession, "Text");
            RenderControls(aSession, "Image");
            RenderControls(aSession, "Control");

            aSession.Send("EnableBreadcrumb", "");
        }

        private void RenderControls(Session aSession, string aType)
        {
            Component[] list = iPageDefinition.GetComponents(aType);

            foreach (Component c in list)
            {
                OpenHome.Xapp.JsonObject jo = new OpenHome.Xapp.JsonObject();
                jo.Add("Type", new OpenHome.Xapp.JsonValueString(aType));
                jo.Add("Id", new OpenHome.Xapp.JsonValueString(c.Id));
                jo.Add("Visible", new OpenHome.Xapp.JsonValueBool(c.Visible));
                jo.Add("Enabled", new OpenHome.Xapp.JsonValueBool(c.Enabled));
                jo.Add("Displayed", new OpenHome.Xapp.JsonValueBool(c.Displayed));
                jo.Add("Top", new OpenHome.Xapp.JsonValueString(c.Top));
                jo.Add("Left", new OpenHome.Xapp.JsonValueString(c.Left));
                jo.Add("HeightSet", new OpenHome.Xapp.JsonValueBool(c.HeightSet));
                jo.Add("Height", new OpenHome.Xapp.JsonValueString(c.Height));
                jo.Add("WidthSet", new OpenHome.Xapp.JsonValueBool(c.WidthSet));
                jo.Add("Width", new OpenHome.Xapp.JsonValueString(c.Width));
                jo.Add("Image", new OpenHome.Xapp.JsonValueString(c.Image));
                jo.Add("BackgroundImage", new OpenHome.Xapp.JsonValueString(c.BackgroundImage));
                jo.Add("Color", new OpenHome.Xapp.JsonValueString(c.Color));
                jo.Add("BackgroundColor", new OpenHome.Xapp.JsonValueString(c.BackgroundColor));

                // set the text to add to the DOM element
                if (!string.IsNullOrEmpty(c.Parameter))
                {
                    jo.Add("Text", new OpenHome.Xapp.JsonValueString(c.Parameter));
                }

                // set the 'class' attribute of the DOM element
                if (!string.IsNullOrEmpty(c.Class))
                {
                    jo.Add("Class", new OpenHome.Xapp.JsonValueString(c.Class));
                }

                aSession.Send("Render", jo);
                
                Console.WriteLine("{0} id [{1}]:  message: {2}", aType, c.Id, c.Parameter);
            }
        }
        

        protected override void OnActivated(Session aSession)
        {
            RenderPage(aSession);

            base.OnActivated(aSession);
        }


        protected override void OnDeactivated(Session aSession)
        {
            base.OnDeactivated(aSession);
        }
        

        protected string GetActionValue(PageDefinitions.Page aPage, string aId)
        {
            // this code only needs to use the old style actions i.e. ActionBasic
            foreach (PageDefinitions.Action action in aPage.Actions)
            {
                if (action.Id == aId)
                {
                    PageDefinitions.ActionBasic actionBasic = action as PageDefinitions.ActionBasic;

                    if (actionBasic != null)
                    {
                        return actionBasic.Parameter;
                    }
                }
            }
            return string.Empty;
        }


        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            Console.WriteLine(string.Format("Name: {0} Value: {1}", aName, aValue));

            switch (aName)
            {
                case "CloseApplication":
                    if (iPageControl.EventCloseApplicationRequested != null) {
                        iPageControl.EventCloseApplicationRequested(this, EventArgs.Empty);
                    }
                    break;
                default:
                    base.OnReceive(aSession, aName, aValue);
                    break;
            }
        }

    }
}
