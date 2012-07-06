using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Linn;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;

namespace LinnSetup
{
    public class AppletFactoryDiagnostics : AppletFactory
    {
        public AppletFactoryDiagnostics() : base("Diagnostics") {
        }

        public override Applet Create(Target aTarget) {
            return new AppletDiagnostics(aTarget);
        }
    }

    public class AppletDiagnostics : Applet
    {
        public AppletDiagnostics(Target aTarget) : base(aTarget) {
        }

        public override Control Ui {
            get {
                return iUi;
            }
        }

        public override void Activate() {
            if (iUi == null) {
                iUi = new Diagnostic(base.Target);

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
    }
}
