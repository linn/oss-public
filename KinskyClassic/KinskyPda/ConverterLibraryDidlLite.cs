using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Linn.Topology
{
    public class ConverterLibraryDidlLite : ConverterLibrary
    {
        public override List<Dictionary<string, string>> Objects(ModelMediaServer aModelMediaServer, Dictionary<string, string> aParent, uint aStartIndex, uint aCount)
        {
            return null;
        }

        /*public static readonly string kRoot = "Root";
        public static readonly string kName = "Name";
        public static readonly string kCount = "Count";*/
    }
}
