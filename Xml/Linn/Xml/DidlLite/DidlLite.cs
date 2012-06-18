using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;

using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Linn.Xml.DidlLite
{
    public class DidlLiteException : Exception
    {
        public DidlLiteException(string aMessage)
            : base(aMessage)
        {
        }
    }

    public class Property
    {
        public string name;
        public string type;
        public string xpath;
        public string element;
        public bool islist;

        internal Property(XPathNavigator navigator)
        {
            name = navigator.Value;
            type = navigator.SelectSingleNode("@type").Value;
            xpath = navigator.SelectSingleNode("@xpath").Value;
            
            if (type == "list")
            {
                islist = true;

                XPathNavigator e = navigator.SelectSingleNode("@element");

                if (e != null)
                {
                    element = e.Value;
                }
                else
                {
                    throw (new DidlLiteException("Property " + name + " is a list with no element specified."));
                }
            }
            else
            {
                islist = false;
            }
        }

        internal void Validate(List<Class> classes)
        {
        }
    }

    public class Class
    {
        public string name;
        public string extends;
        public string classType;
        public bool isbase;

        public List<Property> properties = new List<Property>();

        internal Class(XPathNavigator navigator)
        {
            name = navigator.SelectSingleNode("@name").Value;
            
            XPathNavigator e = navigator.SelectSingleNode("@extends");
            
            if (e != null)
            {
                isbase = false;
                extends = e.Value;
            }
            else
            {
                isbase = true;
            }

            e = navigator.SelectSingleNode("@classType");

            if (e != null)
            {
                classType = e.Value;
            }

            foreach (XPathNavigator p in navigator.Select("property"))
            {
                properties.Add(new Property(p));
            }
        }
        
        internal void Validate(List<Class> classes)
        {
            if (!isbase)
            {
                foreach (Class c in classes)
                {
                    if (extends == c.name)
                    {
                        return;
                    }
                }
                throw (new DidlLiteException("Class " + name + " extends unknown class " + extends + "."));
            }
        }
    }
    
    public class Namespace
    {
        public string prefix;
        public string uri;

        internal Namespace(XPathNavigator navigator)
        {
            prefix = navigator.SelectSingleNode("@prefix").Value;
            uri = navigator.Value;
        }
    }

    public class DidlLiteXml
    {
        public List<Class> classes = new List<Class>();
        public List<Namespace> namespaces = new List<Namespace>();

        private static void XmlValidationEventHandler(object sender, ValidationEventArgs e)
        {
            throw (e.Exception);
        }

        public DidlLiteXml(string uri)
        {
            ValidationEventHandler validator = new ValidationEventHandler(XmlValidationEventHandler);

            // Read the schema from the same directory as this library

            Assembly assembly = Assembly.GetExecutingAssembly();

            string path = System.IO.Path.GetDirectoryName(assembly.Location) + "/DidlLite.xsd";

            XmlSchema schema = XmlSchema.Read(new StreamReader(path), validator);

            XmlReaderSettings settings = new XmlReaderSettings();

            settings.CloseInput = true;
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += validator;
            settings.Schemas.Add(schema);

            XmlReader reader = XmlReader.Create(uri, settings);

            XPathDocument document = new XPathDocument(reader);

            XPathNavigator navigator = document.CreateNavigator();

            // Process the xml file

            foreach (XPathNavigator c in navigator.Select("/hierarchy/class"))
            {
                classes.Add(new Class(c));
            }
            
            foreach (XPathNavigator n in navigator.Select("/hierarchy/namespace"))
            {
                namespaces.Add(new Namespace(n));
            }

            foreach (Class c in classes)
            {
                c.Validate(classes);
                
                foreach (Property p in c.properties)
                {
                    p.Validate(classes);
                }
            }
        }
    }
}
