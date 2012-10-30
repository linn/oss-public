// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Linn Products Limited">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.ProductSupport.Ticketing
{
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using System.IO;
    using System.Text;

    using Linn.Tickets.Resources;

    [XmlRoot(Namespace = "http://www.linn.co.uk/2012/tickets")]
    [XmlType(TypeName = "ticketRequest")]
    public class TicketRequestResource 
    {
        ////[Required(ErrorMessage = "FirstName:FirstName is required.")]
        //[StringLength(64, ErrorMessage = "FirstName must not exceed 64 characters.")]
        [XmlElement("firstName")]
        public string FirstName { get; set; }

        //[Required(ErrorMessage = "LastName:LastName is required.")]
        //[StringLength(64, ErrorMessage = "LastName must not exceed 64 characters.")]
        [XmlElement("lastName")]
        public string LastName { get; set; }

        //[StringLength(256, ErrorMessage = "Email:Email must not exceed 256 characters.")]
        [XmlElement("email")]
        public string Email { get; set; }

        [XmlElement("phoneNumber")]
        //[StringLength(64, ErrorMessage = "PhoneNumber:PhoneNumber must not exceed 64 characters.")]
        public string PhoneNumber { get; set; }

        //[StringLength(512, ErrorMessage = "FaultDescription:FaultDescription must not exceed 512 characters.")]
        [XmlElement("faultDescription")]
        public string FaultDescription { get; set; }

        //[StringLength(512, ErrorMessage = "ContactNotes:ContactNotes must not exceed 512 characters.")]
        [XmlElement("contactNotes")]
        public string ContactNotes { get; set; }

        //[StringLength(256, ErrorMessage = "TimeZoneId must not exceed 256 characters.")]
        [XmlElement("timeZoneId")]
        public string TimeZoneId { get; set; }

        //[StringLength(64, ErrorMessage = "ProductId must not exceed 64 characters.")]
        [XmlElement("productId")]
        public string ProductId { get; set; }

        //[StringLength(64, ErrorMessage = "ProductName must not exceed 64 characters.")]
        [XmlElement("productName")]
        public string ProductName { get; set; }

        //[StringLength(64, ErrorMessage = "ProductFirmware must not exceed 64 characters.")]
        [XmlElement("productFirmware")]
        public string ProductFirmware { get; set; }

        //[StringLength(64, ErrorMessage = "ProductMacAddress must not exceed 64 characters.")]
        [XmlElement("productMacAddress")]
        public string ProductMacAddress { get; set; }

        //[StringLength(256, ErrorMessage = "OperatingSystem must not exceed 256 characters.")]
        [XmlElement("operatingSystem")]
        public string OperatingSystem { get; set; }

        //[StringLength(16, ErrorMessage = "InstallerVersion must not exceed 16 characters.")]
        //[Required(ErrorMessage = "InstallerVersion is required.")]
        [XmlElement("installerVersion")]
        public string InstallerVersion { get; set; }

        //[Required(ErrorMessage = "InstallerReport is required.")]
        [XmlElement("installerReport")]
        public InstallerReportResource InstallerReport { get; set; }


        public bool Valid()
        {
            if ((FirstName.Length == 0) || (FirstName.Length > 64))
            {
               return(false);
            }
            else if ((LastName.Length == 0) || (LastName.Length > 64))
            {
                return (false);
            }
            else if ((Email.Length == 0) && (PhoneNumber.Length == 0))
            {
                return (false);
            }
            else if (Email.Length > 256)
            {
                return (false);
            }
            else if (PhoneNumber.Length > 64)
            {
                return (false);
            }
            else if (FaultDescription.Length > 512)
            {
                return (false);
            }
            else if (ContactNotes.Length > 512)
            {
                return (false);
            }
            else if (TimeZoneId.Length > 256)
            {
                return (false);
            }
            else if (ProductId.Length > 64)
            {
                return (false);
            }
            else if (ProductName.Length > 64)
            {
                return (false);
            }
            else if (ProductFirmware.Length > 64)
            {
                return (false);
            }
            else if (ProductMacAddress.Length > 64)
            {
                return (false);
            }
            else if (OperatingSystem.Length > 256)
            {
                return (false);
            }
            else if ((InstallerVersion.Length == 0) || (InstallerVersion.Length > 16))
            {
                return (false);
            }
            else if (InstallerReport == null)
            {
                return (false);
            }
            
            return(true);
        }

        public string ToXmlString()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, "http://www.linn.co.uk/2012/tickets");
            var serializer = new XmlSerializer(typeof(TicketRequestResource));
            var writer = new Utf8StringWriter();
            serializer.Serialize(writer, this, ns);

            string xmlDataString = writer.ToString();
            xmlDataString = xmlDataString.Replace("\0", ""); // debug - remove any null terminators
            return (xmlDataString);
        }


        public class Utf8StringWriter : StringWriter
        {
            public override System.Text.Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }
    }



}
