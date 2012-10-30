// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListOfCategoryResource.cs" company="Linn Products Ltd.">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.Tickets.Resources
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class ListOfCategoryResource 
    {
        public ListOfCategoryResource()
        {
            this.Categories = new List<CategoryResource>();
        }

        [XmlElement("category", typeof(CategoryResource))]
        public List<CategoryResource> Categories { get; set; }

   }
}