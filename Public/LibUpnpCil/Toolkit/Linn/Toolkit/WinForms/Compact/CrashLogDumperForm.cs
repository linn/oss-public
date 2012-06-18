using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Linn.Toolkit.WinForms
{
    public class CrashLogDumperForm : ICrashLogDumper
    {
        //do not display on pocket pc
        public CrashLogDumperForm(string aTitle, string aProduct, string aVersion, Icon aIcon)
        {
        }

        public void Dump(CrashLog aCrashLog)
        {
        }
    }
}
