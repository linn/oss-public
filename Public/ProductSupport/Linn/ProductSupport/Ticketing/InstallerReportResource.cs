// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Linn Products Ltd." file="InstallerReportResource.cs">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.Tickets.Resources
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;

    [XmlRoot(Namespace = "http://www.linn.co.uk/2012/tickets")]
    [XmlType(TypeName = "installerReport")]

    public class InstallerReportResource : IValidatableObject
    {
        public InstallerReportResource()
        {
            this.Tests = new ListOfTestResource();
            this.Information = new ListOfCategoryResource();
        }

        [StringLength(256, ErrorMessage = "EntryPoint must not exceed 256 characters.")]
        [XmlElement("entryPoint")]
        public string EntryPoint { get; set; }

        [XmlElement("information")]
        public ListOfCategoryResource Information { get; set; }

        [XmlElement("tests")]
        public ListOfTestResource Tests { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (this.Information != null)
            {
                Validator.TryValidateObject(
                    this.Information, new ValidationContext(this.Information, null, null), results, true);
            }

            if (this.Tests != null)
            {
                Validator.TryValidateObject(
                    this.Tests, new ValidationContext(this.Tests, null, null), results, true);
            }

            return results;
        }
    }
}

