﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Linn;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;

namespace LinnSetup
{
    public class AppletFactoryBasicSetup : AppletFactory
    {
        public AppletFactoryBasicSetup() : base("BasicSetup") {
        }

        public override Applet Create(Target aTarget) {
            return new AppletBasicSetup(aTarget);
        }
    }

    public class AppletBasicSetup : Applet
    {
        public AppletBasicSetup(Target aTarget) : base(aTarget) {
        }

        public override Control Ui {
            get {
                return iUi;
            }
        }

        public override void Activate() {
            if (iUi == null) {
                iUi = new BasicSetup(base.Target);
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

        protected BasicSetup iUi = null;
    }
}