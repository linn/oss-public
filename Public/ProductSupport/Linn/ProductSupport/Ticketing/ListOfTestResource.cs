// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListOfTestResource.cs" company="Linn Products Ltd.">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.Tickets.Resources
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;

    public class ListOfTestResource : IValidatableObject
    {
        public ListOfTestResource()
        {
            this.Tests = new List<TestResource>();
        }

        [XmlElement("test", typeof(TestResource))]
        public List<TestResource> Tests { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            foreach (var test in this.Tests)
            {
                Validator.TryValidateObject(test, new ValidationContext(test, null, null), results, true);
            }

            return results;
        }
    }
}