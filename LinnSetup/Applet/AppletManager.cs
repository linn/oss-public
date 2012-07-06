using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Linn;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;
using Linn.ProductSupport.Diagnostics;

namespace LinnSetup
{
    public class AppletManager
    {
        public AppletManager(Helper aHelper, Diagnostics aDiagnostics) {
            Add(new AppletFactoryConfig());
            Add(new AppletFactoryPresentation());
            Add(new AppletFactoryDiagnostics());
            Add(new AppletFactorySysLog());
            Add(new AppletFactoryReflash());
            Add(new AppletFactoryPlaybackTest());
            Add(new AppletFactoryBasicSetup());
            Add(new AppletFactoryTicketing(aHelper, aDiagnostics));
        }

        public List<Applet> CreateApplets(Target aTarget) {
            List<Applet> applets = new List<Applet>();

            foreach (AppletFactory factory in iFactories) {
                Applet applet = factory.Create(aTarget);
                applets.Add(applet);
            }

            return applets;
        }

        public List<string> AppletNames {
            get {
                return iAppletNames;
            }
        }

        private void Add(AppletFactory aFactory) {
            iFactories.Add(aFactory);
            iAppletNames.Add(aFactory.Name);
        }

        private List<AppletFactory> iFactories = new List<AppletFactory>();
        private List<string> iAppletNames = new List<string>();
    }
}
