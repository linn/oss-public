// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryResource.cs" company="Linn Products Ltd.">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.Tickets.Resources
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class CategoryResource 
    {
        public CategoryResource()
        {
            this.Items = new List<ItemResource>();
        }

        public bool Valid()
        {
            if ((Title.Length == 0) || (Title.Length > 256))
            {
                return(false);
            }
            return(true);
        }

        [XmlElement("item", typeof(ItemResource))]
        public List<ItemResource> Items { get; set; }

        //[Required(ErrorMessage = "Title is required.")]
        //[StringLength(256, ErrorMessage = "Title must not exceed 256 characters.")]
        [XmlAttribute("title")]
        public string Title { get; set; }

 
    }
}