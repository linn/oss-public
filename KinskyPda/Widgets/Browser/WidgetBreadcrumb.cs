
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Linn.Kinsky;


namespace KinskyPda.Widgets
{
    public class WidgetBreadcrumb : ComboBox, IViewWidgetLocation
    {
        public WidgetBreadcrumb()
        {
            this.BackColor = ViewSupport.Instance.BackColour;
            this.Font = ViewSupport.Instance.FontLarge;
            this.ForeColor = ViewSupport.Instance.ForeColourBright;
            this.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
        }

        public IControllerLocation Controller
        {
            set { iController = value; }
        }

        public bool IsHome
        {
            get { return iIsHome; }
        }

        public event EventHandler<EventArgs> EventLocationChanged;

        public void Open()
        {
        }

        public void Close()
        {
        }

        private delegate void DSetLocation(IList<string> aLocation);
        public void SetLocation(IList<string> aLocation)
        {
            if (aLocation.Count == 0)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new DSetLocation(SetLocation), new object[] { aLocation });
            }
            else
            {
                // stop receiving events while the combo box is updated
                this.SelectedIndexChanged -= OnSelectedIndexChanged;

                // add the new location to the combo box items
                this.Items.Clear();
                foreach (string location in aLocation)
                {
                    ComboBoxItem item = new ComboBoxItem(null, location, null);
                    this.Items.Add(item);
                }

                // last item in list is current location
                this.SelectedIndex = this.Items.Count - 1;
                iIsHome = (this.SelectedIndex == 0);

                if (EventLocationChanged != null)
                    EventLocationChanged(this, EventArgs.Empty);

                // ok to start receiving events from the combo box now
                this.SelectedIndexChanged += OnSelectedIndexChanged;
            }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            IControllerLocation controller = iController;
            if (controller != null)
            {
                int upLevels = this.Items.Count - this.SelectedIndex - 1;
                controller.Up((uint)upLevels);
            }
        }

        private bool iIsHome;
        private IControllerLocation iController;
    }
}
