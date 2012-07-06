// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemResource.cs" company="Linn Products Ltd.">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.Tickets.Resources
{
    using System.ComponentModel.DataAnnotations;
    using System.Xml;
    using System.Xml.Serialization;

    //using Linn.Core.Resources;

    //[UriTemplate("tickets/{ticketId}/installer-reports/{activityId}/report/items/{itemId}")]
    public class ItemResource
    {
        //[XmlElement("id")]
        //public int Id { get; set; }

        [XmlElement("content")]
        public XmlCDataSection Content { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(256, ErrorMessage = "Title must not exceed 256 characters.")]
        [XmlAttribute("title")]
        public string Title { get; set; }
    }
}