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

        public PageControl(Helper aHelper, Framework aXapp, string aResourcePath, string aXmlFileName)
        {
            // create the model class for this setup wizard
            ModelInstance.Create(aHelper);

            // load the page definitions xml
            PageDefinitions pageDef = new PageDefinitions(Path.Combine(aResourcePath, aXmlFileName));

            // create the page manager
            PageManager.Create(pageDef, this);

            aXapp.AddCss("Linn.css");

            // add all created pages to the xapp framework
            foreach (PageManager.Entry pageEntry in PageManager.Instance.Entries)
            {
                aXapp.AddPage(pageEntry.Page);
            }

            // create xapp views
            foreach (PageDefinitions.Page page in pageDef.Pages)
            {
                WizardView view = new WizardView(page, aResourcePath);
                aXapp.AddView(view);
            }
        }

        public void Close()
        {
            ModelInstance.Instance.Close();
        }
    }
}
