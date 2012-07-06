
using System;
using System.Windows;


namespace Linn.Toolkit
{
    public partial class HelperAutoUpdate
    {
        partial void UpdateStarted(object sender, EventArgs e)
        {
            Application.Current.Shutdown(2);
        }
    }
}

