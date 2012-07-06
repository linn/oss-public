// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListOfCategoryResource.cs" company="Linn Products Ltd.">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.Tickets.Resources
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;

    public class ListOfCategoryResource : IValidatableObject
    {
        public ListOfCategoryResource()
        {
            this.Categories = new List<CategoryResource>();
        }

        [XmlElement("category", typeof(CategoryResource))]
        public List<CategoryResource> Categories { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            foreach (var category in this.Categories)
            {
                Validator.TryValidateObject(category, new ValidationContext(category, null, null), results, true);
            }

            return results;
        }
    }
}