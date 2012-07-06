// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestResource.cs" company="Linn Products Ltd.">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.Tickets.Resources
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Xml;
    using System.Xml.Serialization;


    public class TestResource
    {
        [XmlElement("content")]
        public XmlCDataSection Content { get; set; }

        [XmlAttribute("result")]
        public TestResourceResult Result { get; set; }

        [StringLength(512, ErrorMessage = "ResultDescription must not exceed 512 characters.")]
        [XmlElement("resultDescription")]
        public string ResultDescription { get; set; }

        [XmlElement("startedUtc")]
        public DateTime? StartedUtc { get; set; }

        [XmlElement("finishedUtc")]
        public DateTime? FinishedUtc { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(256, ErrorMessage = "Title must not exceed 256 characters.")]
        [XmlAttribute("title")]
        public string Title { get; set; }
    }
}