using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Linn.ControlPoint.Upnp;
using Linn.Topology.Boxes;

namespace LinnSetup
{
    public class AppletFactoryDiagnostics : AppletFactory
    {
        public AppletFactoryDiagnostics() : base("Diagnostics") {
        }

        public override Applet Create(Target aTarget, EventServerUpnp aEventServer) {
            return new AppletDiagnostics(aTarget, aEventServer);
        }
    }

    public class AppletDiagnostics : Applet
    {
        public AppletDiagnostics(Target aTarget, EventServerUpnp aEventServer) : base(aTarget) {
            iEventServer = aEventServer;
        }

        public override Control Ui {
            get {
                return iUi;
            }
        }

        public override void Activate() {
            if (iUi == null) {
                iUi = new Diagnostic(base.Target, iEventServer);

                //when an applet is active it should be notified when the Target is changed
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

        protected Diagnostic iUi = null;
        private EventServerUpnp iEventServer;
    }
}
