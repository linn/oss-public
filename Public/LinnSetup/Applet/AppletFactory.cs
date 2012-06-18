using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Linn.ControlPoint.Upnp;
using Linn.Topology.Boxes;

namespace LinnSetup
{
    public abstract class AppletFactory
    {
        public abstract Applet Create(Target aTarget, EventServerUpnp aEventServer);

        public AppletFactory(string aName) {
            iName = aName;
        }

        public string Name {
            get {
                return iName;
            }
        }

        private string iName;
    }
}