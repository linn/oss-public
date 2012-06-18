using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Linn;

namespace KinskyDesktop.Widgets
{
    public partial class ButtonControlTriState : ButtonControl
    {
        private uint iState;
        private Image iImageStateTwo;
        private Image iImageMouseOverStateTwo;
        private Image iImageTouchedStateTwo;
        private Image iImageStateThree;
        private Image iImageMouseOverStateThree;
        private Image iImageTouchedStateThree;

        public ButtonControlTriState()
        {
            InitializeComponent();
        }

        public uint State
        {
            get
            {
                return iState;
            }
            set
            {
                uint newState = value % 3;
                if (newState != iState)
                {
                    iState = newState;
                    switch (iState)
                    {
                        case 0:
                            iImageCurrentState = iImageStateInitial;
                            iImageCurrentMouseOver = iImageMouseOver;
                            iImageCurrentTouched = iImageTouched;
                            break;
                        case 1:
                            iImageCurrentState = iImageStateTwo;
                            iImageCurrentMouseOver = iImageMouseOverStateTwo;
                            iImageCurrentTouched = iImageTouchedStateTwo;
                            break;
                        case 2:
                            iImageCurrentState = iImageStateThree;
                            iImageCurrentMouseOver = iImageMouseOverStateThree;
                            iImageCurrentTouched = iImageTouchedStateThree;
                            break;
                        default:
                            Assert.Check(false);
                            break;
                    }
                    Invalidate();
                }
            }
        }

        public Image ImageStateTwo
        {
            get
            {
                return iImageStateTwo;
            }
            set
            {
                iImageStateTwo = value;
            }
        }

        public Image ImageMouseOverStateTwo
        {
            get
            {
                return iImageMouseOverStateTwo;
            }
            set
            {
                iImageMouseOverStateTwo = value;
            }
        }

        public Image ImageTouchedStateTwo
        {
            get
            {
                return iImageTouchedStateTwo;
            }
            set
            {
                iImageTouchedStateTwo = value;
            }
        }

        public Image ImageStateThree
        {
            get
            {
                return iImageStateThree;
            }
            set
            {
                iImageStateThree = value;
            }
        }

        public Image ImageMouseOverStateThree
        {
            get
            {
                return iImageMouseOverStateThree;
            }
            set
            {
                iImageMouseOverStateThree = value;
            }
        }

        public Image ImageTouchedStateThree
        {
            get
            {
                return iImageTouchedStateThree;
            }
            set
            {
                iImageTouchedStateThree = value;
            }
        }
    }
}
