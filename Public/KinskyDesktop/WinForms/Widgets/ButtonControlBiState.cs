using System;
using System.Drawing;
using System.Windows.Forms;

namespace KinskyDesktop.Widgets
{
    public partial class ButtonControlBiState : ButtonControl
    {
        private bool iState;
        private Image iImageStateOn;
        private Image iImageMouseOverOn;
        private Image iImageTouchedOn;

        public ButtonControlBiState()
        {
            InitializeComponent();
        }

        public bool State
        {
            get
            {
                return iState;
            }
            set
            {
                if (iState != value)
                {
                    iState = value;
                    if (iState)
                    {
                        iImageCurrentTouched = iImageTouchedOn;
                        iImageCurrentMouseOver = iImageMouseOverOn;
                        iImageCurrentState = iImageStateOn;
                    }
                    else
                    {
                        iImageCurrentTouched = iImageTouched;
                        iImageCurrentMouseOver = iImageMouseOver;
                        iImageCurrentState = iImageStateInitial;
                    }
                    Invalidate();
                }
            }
        }

        public Image ImageStateOn
        {
            get
            {
                return iImageStateOn;
            }
            set
            {
                iImageStateOn = value;
            }
        }

        public Image ImageMouseOverOn
        {
            get
            {
                return iImageMouseOverOn;
            }
            set
            {
                iImageMouseOverOn = value;
            }
        }

        public Image ImageTouchOn
        {
            get
            {
                return iImageTouchedOn;
            }
            set
            {
                iImageTouchedOn = value;
            }
        }
    }
}
