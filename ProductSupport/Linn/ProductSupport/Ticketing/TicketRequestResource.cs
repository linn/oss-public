// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Linn Products Limited">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.ProductSupport.Ticketing
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;


    // added by Eamonn...
    using System.IO;
    using System.Text;

    using Linn.Tickets.Resources;

    [XmlRoot(Namespace = "http://www.linn.co.uk/2012/tickets")]
    [XmlType(TypeName = "ticketRequest")]
    public class TicketRequestResource : IValidatableObject
    {
        bool iValidated = false;

        public TicketRequestResource()
        {
            this.InstallerReport = new InstallerReportResource();
        }

        [Required(ErrorMessage = "FirstName:FirstName is required.")]
        [StringLength(64, ErrorMessage = "FirstName must not exceed 64 characters.")]
        [XmlElement("firstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName:LastName is required.")]
        [StringLength(64, ErrorMessage = "LastName must not exceed 64 characters.")]
        [XmlElement("lastName")]
        public string LastName { get; set; }

        [StringLength(256, ErrorMessage = "Email:Email must not exceed 256 characters.")]
        [XmlElement("email")]
        public string Email { get; set; }

        [XmlElement("phoneNumber")]
        [StringLength(64, ErrorMessage = "PhoneNumber:PhoneNumber must not exceed 64 characters.")]
        public string PhoneNumber { get; set; }

        [StringLength(512, ErrorMessage = "FaultDescription:FaultDescription must not exceed 512 characters.")]
        [XmlElement("faultDescription")]
        public string FaultDescription { get; set; }

        [StringLength(512, ErrorMessage = "ContactNotes:ContactNotes must not exceed 512 characters.")]
        [XmlElement("contactNotes")]
        public string ContactNotes { get; set; }

        [StringLength(256, ErrorMessage = "TimeZoneId must not exceed 256 characters.")]
        [XmlElement("timeZoneId")]
        public string TimeZoneId { get; set; }

        [StringLength(64, ErrorMessage = "ProductId must not exceed 64 characters.")]
        [XmlElement("productId")]
        public string ProductId { get; set; }

        [StringLength(64, ErrorMessage = "ProductName must not exceed 64 characters.")]
        [XmlElement("productName")]
        public string ProductName { get; set; }

        [StringLength(64, ErrorMessage = "ProductFirmware must not exceed 64 characters.")]
        [XmlElement("productFirmware")]
        public string ProductFirmware { get; set; }

        [StringLength(64, ErrorMessage = "ProductMacAddress must not exceed 64 characters.")]
        [XmlElement("productMacAddress")]
        public string ProductMacAddress { get; set; }

        [StringLength(256, ErrorMessage = "OperatingSystem must not exceed 256 characters.")]
        [XmlElement("operatingSystem")]
        public string OperatingSystem { get; set; }

        [StringLength(16, ErrorMessage = "InstallerVersion must not exceed 16 characters.")]
        [Required(ErrorMessage = "InstallerVersion is required.")]
        [XmlElement("installerVersion")]
        public string InstallerVersion { get; set; }

        [Required(ErrorMessage = "InstallerReport is required.")]
        [XmlElement("installerReport")]
        public InstallerReportResource InstallerReport { get; set; }



        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (!iValidated)   // to combat the strange quirk of the Validator (it may or may not call this method at construction)
                               // so I call this manually but if it's already been called then the report would get duplicated. 
                               // this flag ensures it doesn't get run twice
            {
                iValidated = true;

                if (string.IsNullOrWhiteSpace(this.Email) && string.IsNullOrWhiteSpace(this.PhoneNumber))
                {
                    results.Add(new ValidationResult("Email:PhoneNumber:At least one contact method is required."));
                }

                Validator.TryValidateObject(
                    this.InstallerReport,
                    new ValidationContext(this.InstallerReport, null, null),
                    results,
                    true);

                
            }
            return results;
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
