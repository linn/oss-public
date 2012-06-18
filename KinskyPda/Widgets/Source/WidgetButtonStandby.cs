
using System;
using System.Drawing;
using System.Windows.Forms;

using Linn.Kinsky;

namespace KinskyPda.Widgets
{
    public class WidgetButtonStandby : IViewWidgetButton
    {
        public event EventHandler<EventArgs> EventClick;

        public WidgetButtonStandby(ImageButton aButton)
        {
            iButton = aButton;
            iButton.Click += ButtonClick;

            SetEnabled(false);
        }

        public void Open()
        {
            SetEnabled(true);
        }

        public void Close()
        {
            SetEnabled(false);
        }

        private delegate void SetEnabledDelegate(bool aEnabled);
        private void SetEnabled(bool aEnabled)
        {
            if (iButton.InvokeRequired)
            {
                iButton.BeginInvoke(new SetEnabledDelegate(SetEnabled), new object[] { aEnabled });
            }
            else
            {
                iButton.Enabled = aEnabled;
            }
        }
        
        private void ButtonClick(object sender, EventArgs e)
        {        
            if (EventClick != null)
            {
                EventClick(this, EventArgs.Empty);
            }
        }

        private ImageButton iButton;
    }
}
