using System.Windows;

namespace Viewer
{
    public partial class ProgressBarWindow : Window
    {
        public ProgressBarWindow()
        {
            InitializeComponent();
            this.progressBar.Value = this.progressBar.Minimum = 0;
            this.progressBar.Maximum = 100;
        }

        public double Maximum
        {
            get
            {
                return progressBar.Maximum;
            }
            set
            {
                progressBar.Maximum = value;
            }
        }

        public double Minimum
        {
            get
            {
                return progressBar.Minimum;
            }
            set
            {
                progressBar.Minimum = value;
            }
        }

        public double Value
        {
            get
            {
                return progressBar.Value;
            }
            set
            {
                this.progressBar.Value = value;
            }
        }

    }
}
