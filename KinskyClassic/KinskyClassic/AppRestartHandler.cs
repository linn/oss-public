using System.Windows.Forms;

using Linn.Kinsky;

namespace KinskyClassic
{
	internal class AppRestartHandler : IAppRestartHandler
	{
		public void Restart()
		{
			DialogResult result = MessageBox.Show("The application must be restarted to complete the plugin installation.\nIs it okay to restart now?", "Install warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
			if (result == DialogResult.Yes)
			{
				Application.Restart();
			}
		}
	}
}