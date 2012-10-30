// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Linn Products Ltd." file="InstallerReportResource.cs">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.Tickets.Resources
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot(Namespace = "http://www.linn.co.uk/2012/tickets")]
    [XmlType(TypeName = "installerReport")]

    public class InstallerReportResource 
    {
        public bool Valid()
        {
            if (EntryPoint.Length > 256)
            {
                return(false);
            }
            return(true);
        }
            
    
        //[StringLength(256, ErrorMessage = "EntryPoint must not exceed 256 characters.")]
        [XmlElement("entryPoint")]
        public string EntryPoint { get; set; }

        [XmlElement("information")]
        public ListOfCategoryResource Information { get; set; }

        [XmlElement("tests")]
        public ListOfTestResource Tests { get; set; } 
    }

}

