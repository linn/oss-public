using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using OpenHome.Xapp;


namespace Linn.Wizard
{

    public class CustomisePage : BasePage
    {

        public CustomisePage(PageControl aPageControl, string aViewId, PageComponents aPageComponents, IPageNavigation aPageNavigation)
            : base(aPageControl, aViewId, aPageComponents, aPageNavigation)
        {

        }

        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            switch(aName)
            {
                case "Title":
                    aSession.Send(aName, aValue); //replace title text
                    break;
                case "EnableNext":
                    aSession.Send(aName, aValue); //enable/disable next button
                    break;
                case "PreviousPage":
                    base.ResetDeviceParameters(aSession);  //reset any customise changes 
                    base.OnReceive(aSession, aName, aValue);
                    break;
                default:
                    base.OnReceive(aSession, aName, aValue);
                    break;
            }
        }

    }
    
}
