using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

namespace Linn.Doktor
{
    public class ParameterDs : ParameterUpnp
    {
        public ParameterDs(string aName, string aDescription)
            : base(aName, "Ds", aDescription)
        {
        }

        protected override IList<string> Recognise(INode aNode)
        {
            IList<string> list = base.Recognise(aNode);
            
            if (list != null) {
                if (Device != null)
                {
                    foreach (XmlNode n in Device.SelectNodes("u:serviceList/u:service/u:serviceType", NsManager))
                    {
                        if (n.FirstChild.Value.StartsWith("urn:linn-co-uk:service:Product:"))
                        {
                            return (list);
                        }
                    }
                }
            }
            
            return (null);
        }
    }
}
