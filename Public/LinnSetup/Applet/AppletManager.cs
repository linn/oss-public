using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Linn.ControlPoint.Upnp;
using Linn.Topology.Boxes;

namespace LinnSetup
{
    public class AppletManager
    {
        public AppletManager() {
            Add(new AppletFactoryConfig());
            Add(new AppletFactoryPresentation());
            Add(new AppletFactoryDiagnostics());
            Add(new AppletFactoryReflash());
        }

        public List<Applet> CreateApplets(Target aTarget, EventServerUpnp aEventServer) {
            List<Applet> applets = new List<Applet>();

            foreach (AppletFactory factory in iFactories) {
                Applet applet = factory.Create(aTarget, aEventServer);
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
