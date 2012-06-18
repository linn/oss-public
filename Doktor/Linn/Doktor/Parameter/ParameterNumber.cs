using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Linn.Doktor
{
    public class ParameterNumber : ParameterNormal
    {
        public const string kMin = "Min";
        public const string kMax = "Max";
        
        public ParameterNumber(string aName, string aDescription, int aMin, int aMax)
            : this(aName, aDescription, aMin, aMax, aMin)
        {
        }

        public ParameterNumber(string aName, string aDescription, int aMin, int aMax, int aDefault)
            : base(aName, "Number", aDescription, aDefault.ToString())
        {
            iMin = aMin;
            iMax = aMax;
            iNumber = aDefault;
            
            Add(kMin, iMin.ToString());
            Add(kMax, iMax.ToString());
        }
        
        protected override bool ValueChanged(string aValue)
        {
            if (int.TryParse(aValue, out iNumber))
            {
                if (iNumber >= iMin && iNumber <= iMax)
                {
                    return (true);
                }
            }
            
            return (false);
        }
        
        public int Number
        {
            get
            {
                return (iNumber);
            }
        }
        
        private int iMin;
        private int iMax;
        private int iNumber;
    }
}
