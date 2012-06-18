using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

namespace Linn.Doktor
{
    public class ParameterMediaRenderer : ParameterUpnp
    {
        public ParameterMediaRenderer(string aName, string aDescription)
            : base(aName, "Mr", aDescription)
        {
        }

        protected override IList<string> Recognise(INode aNode)
        {
            IList<string> list = base.Recognise(aNode);
            
            if (list != null) {
                if (Device != null)
                {
                    XmlNode type = Device.SelectSingleNode("u:deviceType", NsManager);

                    if (type != null)
                    {
                        if (type.FirstChild.Value.StartsWith("urn:schemas-upnp-org:device:MediaRenderer:"))
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
