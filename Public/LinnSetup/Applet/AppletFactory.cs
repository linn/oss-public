using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Linn;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;

namespace LinnSetup
{
    public abstract class AppletFactory
    {
        public abstract Applet Create(Target aTarget);

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