// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryResource.cs" company="Linn Products Ltd.">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.Tickets.Resources
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;

    public class CategoryResource : IValidatableObject
    {
        public CategoryResource()
        {
            this.Items = new List<ItemResource>();
        }

        [XmlElement("item", typeof(ItemResource))]
        public List<ItemResource> Items { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(256, ErrorMessage = "Title must not exceed 256 characters.")]
        [XmlAttribute("title")]
        public string Title { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            foreach (var item in this.Items)
            {
                Validator.TryValidateObject(item, new ValidationContext(item, null, null), results, true);
            }

            return results;
        }
    }
}