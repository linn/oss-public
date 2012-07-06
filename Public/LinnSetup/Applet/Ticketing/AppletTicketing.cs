using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Linn;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;
using Linn.ProductSupport.Diagnostics;

namespace LinnSetup
{
    public class AppletFactoryTicketing : AppletFactory
    {
        public AppletFactoryTicketing(Helper aHelper, Diagnostics aDiagnostics) : base("Ticketing") {
            iHelper = aHelper;
            iDiagnostics = aDiagnostics;
        }

        public override Applet Create(Target aTarget) {
            return new AppletTicketing(aTarget, iHelper, iDiagnostics);
        }

        private Helper iHelper;
        private Diagnostics iDiagnostics;
    }

    public class AppletTicketing : Applet
    {
        public AppletTicketing(Target aTarget, Helper aHelper, Diagnostics aDiagnostics) : base(aTarget) {
            iHelper = aHelper;
            iDiagnostics = aDiagnostics;
        }

        public override Control Ui {
            get {
                return iUi;
            }
        }

        public override void Activate() {
            if (iUi == null) {
                iUi = new Ticketing(base.Target, iHelper, iDiagnostics);
                base.Target.EventBoxChanged += BoxChangedHandler;
            }
        }

        public override void Deactivate() {
        }

        protected override void BoxChangedHandler(object obj, EventArgsBox e) {
            if (e.BoxArg.State == Box.EState.eOn && !e.BoxArg.IsProxy) {
                iUi.Enable();
            }
            else {
                iUi.Disable();
            }
        }

        protected Ticketing iUi = null;
        private Helper iHelper;
        private Diagnostics iDiagnostics;
    }
}
