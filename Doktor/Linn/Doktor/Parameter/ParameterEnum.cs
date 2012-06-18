using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Linn.Doktor
{
    public class ParameterEnum : ParameterEnumerated
    {
        public ParameterEnum(string aName, string aDescription)
            : base(aName, "Enum", aDescription)
        {
        }
    }
}
