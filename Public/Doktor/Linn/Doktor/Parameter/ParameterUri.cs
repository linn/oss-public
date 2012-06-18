using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Linn.Doktor
{
    public class ParameterUri : ParameterNormal
    {
        public ParameterUri(string aName, string aDescription)
            : base(aName, "Uri", aDescription)
        {
        }

        protected override bool ValueChanged(string aValue)
        {
            try
            {
                Uri uri = new Uri(aValue);
            }
            catch (UriFormatException)
            {
                return (false);
            }

            return (true);
        }
    }
}
