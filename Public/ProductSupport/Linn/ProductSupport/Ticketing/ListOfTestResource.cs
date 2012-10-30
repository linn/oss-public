// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListOfTestResource.cs" company="Linn Products Ltd.">
//   Copyright © 2012 Linn Products Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Linn.Tickets.Resources
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class ListOfTestResource 
    {
        public ListOfTestResource()
        {
            this.Tests = new List<TestResource>();
        }

        [XmlElement("test", typeof(TestResource))]
        public List<TestResource> Tests { get; set; }

    }
}