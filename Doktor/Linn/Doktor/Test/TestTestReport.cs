using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Linn.Doktor
{
    public class TestTestReport : Test
    {
        public TestTestReport()
            : base("Report", "Test", "Exercise the reporting mechanism")
        {
        }
        
        protected override void Execute()
        {
            Context("Case", "3");
            Low("Failure description");
        }
    }
}
