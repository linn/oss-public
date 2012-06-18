using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

using Linn.ControlPoint.Upnp;
using Linn.Topology.Boxes;

namespace LinnSetup
{
    public class AppletFactoryPresentation : AppletFactory
    {
        public AppletFactoryPresentation() : base("Portal") {
        }

        public override Applet Create(Target aTarget, EventServerUpnp aEventServer) {
            return new AppletPresentation(aTarget);
        }
    }

    public class AppletPresentation : Applet
    {
        public AppletPresentation(Target aTarget) : base(aTarget) {
        }

        public override Control Ui {
            get {
                return iUi;
            }
        }

        public override void Activate() {
            if (iUi == null) {
                iUi = new Presentation(base.Target);
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

        protected Presentation iUi = null;
    }
}
