using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Linn.ControlPoint.Upnp;
using Linn.Topology.Boxes;

namespace LinnSetup
{
    public class AppletFactoryConfig : AppletFactory
    {
        public AppletFactoryConfig() : base("Configuration") {
        }

        public override Applet Create(Target aTarget, EventServerUpnp aEventServer) {
            return new AppletConfig(aTarget);
        }
    }

    public class AppletConfig : Applet
    {
        public AppletConfig(Target aTarget) : base(aTarget) {
        }

        public override Control Ui {
            get {
                return iUi;
            }
        }

        public override void Activate() {
            if (iUi == null) {
                iUi = new Config(base.Target);
                base.Target.EventBoxChanged += BoxChangedHandler;
            }
        }

        public override void Deactivate() {
        }

        protected override void BoxChangedHandler(object obj, EventArgsBox e) {
            if (e.BoxArg.State == Box.EState.eOn) {
                iUi.Enable();
            }
            else {
                iUi.Disable();
            }
        }

        protected Config iUi = null;
    }
}
