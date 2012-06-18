using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Linn.Doktor
{
    public class ParameterString : ParameterNormal
    {
        public ParameterString(string aName, string aDescription)
            : base(aName, "String", aDescription)
        {
        }

        public ParameterString(string aName, string aDescription, string aDefault)
            : base(aName, "String", aDescription, aDefault)
        {
        }

        protected override bool ValueChanged(string aValue)
        {
            return (aValue.Length > 0) ;
        }
    }
}
