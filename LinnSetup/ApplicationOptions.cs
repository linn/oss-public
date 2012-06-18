using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using Linn;

namespace LinnSetup
{
    public class ApplicationOptions
    {
        public ApplicationOptions(IHelper aHelper) {
            iLastBoxSelected = new OptionString("lastboxselected", "Last Box Selected", "Store last device selected by the user", "");
            aHelper.AddOption(iLastBoxSelected);

            iViewDetails = new OptionBool("viewdetails", "View Details", "Show device list in detail rather than by Icons", false);
            aHelper.AddOption(iViewDetails);
        }

        public string LastBoxSelected {
            get {
                return iLastBoxSelected.Native;
            }
            set {
                iLastBoxSelected.Native = value;
            }
        }

        public bool ViewDetails {
            get {
                return iViewDetails.Native;
            }
            set {
                iViewDetails.Native = value;
            }
        }

        public void ResetToDefaults() {
            iLastBoxSelected.ResetToDefault();
            iViewDetails.ResetToDefault();
        }

        private OptionString iLastBoxSelected;
        private OptionBool iViewDetails;
    }
}