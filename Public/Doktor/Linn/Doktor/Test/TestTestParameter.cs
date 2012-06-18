using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Linn.Doktor
{
    public class TestTestParameter : Test
    {
        public TestTestParameter()
            : base("Parameter", "Test", "Exercise the parameter passing mechanism")
        {
            iParameter1 = new ParameterNumber("1", "Number", -1000, 1234, -17);
            iParameter2 = new ParameterString("2", "String");
            iParameter3 = new ParameterUri("3", "Uri");

            iParameter4 = new ParameterEnum("4", "Enum");
            iParameter4.Add("Apple");
            iParameter4.Add("Orange");
            iParameter4.AddDefault("Lemon");
            iParameter4.Add("Pear");

            iParameter5 = new ParameterUpnp("5", "Upnp");
            iParameter6 = new ParameterMediaRenderer("6", "Media Renderer");
            iParameter7 = new ParameterMediaServer("7", "Media Server");
            iParameter8 = new ParameterDs("8", "Ds");
            
            Add(iParameter1); 
            Add(iParameter2); 
            Add(iParameter3);
            Add(iParameter4);
            Add(iParameter5);
            Add(iParameter6);
            Add(iParameter7);
            Add(iParameter8);
        }
        
        protected override void Execute()
        {
        }
        
        ParameterNumber iParameter1;
        ParameterString iParameter2;
        ParameterUri iParameter3;
        ParameterEnum iParameter4;
        ParameterUpnp iParameter5;
        ParameterMediaRenderer iParameter6;
        ParameterMediaServer iParameter7;
        ParameterDs iParameter8;
    }
}
