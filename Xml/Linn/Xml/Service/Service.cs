using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;

using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Linn.Xml.Service
{
    public class Argument
    {
        public string name;
        public string type;
        public string related;
        public string get;
        public string kind;

        internal Argument(XPathNavigator navigator, Model model)
        {
            type = navigator.Name;
            name = navigator.Value;

            if (type == "related")
            {
                related = navigator.SelectSingleNode("@name").Value;

                foreach (Property p in model.properties)
                {
                    if (p.name == related)
                    {
                        kind = p.kind;
                        get = p.get;
                        break;
                    }
                }
            }
            else
            {
                switch (type)
                {
                    case "string":
                        kind = "String";
                        get = "const Brx&";
                        break;
                    case "unsigned":
                        kind = "Uint";
                        get = "TUint";
                        break;
                    case "signed":
                        kind = "Int";
                        get = "TInt";
                        break;
                    case "bool":
                        kind = "Bool";
                        get = "TBool";
                        break;
                    case "binary":
                        kind = "Binary";
                        get = "const Brx&";
                        break;
                    case "enum":
                        kind = "Int";
                        get = "TInt";
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public class Method
    {
        public string name;
        public string invoke;

        public List<Argument> inargs = new List<Argument>();
        public List<Argument> outargs = new List<Argument>();

        internal Method(XPathNavigator navigator, Model model)
        {
            name = navigator.SelectSingleNode("name").Value;
            invoke = "INVOKE";

            foreach (XPathNavigator i in navigator.Select("in/*"))
            {
                inargs.Add(new Argument(i, model));
            }

            foreach (XPathNavigator o in navigator.Select("out/*"))
            {
                outargs.Add(new Argument(o, model));
            }
        }
    }

    public class Property
    {
        public string type;
        public string name;
        public string kind;
        public string get;
        public string instance;
        public string model;
        public uint bytes = 0;
        public string access = "rw";
        public bool evented = false;

        public Dictionary<string, uint> values = new Dictionary<string, uint>();

        internal Property(XPathNavigator navigator)
        {
            type = navigator.Name;

            name = navigator.SelectSingleNode("name").Value;

            XPathNavigator a = navigator.SelectSingleNode("access");

            if (a != null)
            {
                access = a.Value;
            }

            if (access == "rw" || access == "rv")
            {
                evented = true;
            }

            switch (type)
            {
                case "string":
                    bytes = Convert.ToUInt32(navigator.SelectSingleNode("bytes").Value);
                    kind = "String";
                    get = "const Brx&";
                    if (bytes > 0)
                    {
                        instance = "Bws<" + bytes + ">";
                    }
                    else
                    {
                        instance = "Bwd";
                    }
                    break;
                case "unsigned":
                    kind = "Uint";
                    get = "TUint";
                    instance = "TUint";
                    break;
                case "signed":
                    kind = "Int";
                    get = "TInt";
                    instance = "TInt";
                    break;
                case "bool":
                    kind = "Bool";
                    get = "TBool";
                    instance = "TBool";
                    break;
                case "binary":
                    bytes = Convert.ToUInt32(navigator.SelectSingleNode("bytes").Value);
                    kind = "Binary";
                    get = "const Brx&";
                    if (bytes > 0)
                    {
                        instance = "Bws<" + bytes + ">";
                    }
                    else
                    {
                        instance = "Bwd";
                    }
                    break;
                case "enum":
                    uint num = 0;
                    foreach (XPathNavigator v in navigator.Select("value"))
                    {
                        XPathNavigator n = v.SelectSingleNode("@n");
                        if (n != null)
                        {
                            num = Convert.ToUInt32(n.Value);
                        }
                        values.Add(v.Value, num);
                        num++;
                    }
                    kind = "Int";
                    get = "TInt";
                    instance = "TInt";
                    break;
                case "record":
                    if (access == "ro")
                    {
                        access = "no";
                    }
                    model = name;
                    name += "Index";
                    kind = "Uint";
                    get = "TUint";
                    instance = "TUint";
                    break;
                default:
                    break;
            }
        }
    }

    public class Model
    {
        public string name;
        public Model parent;

        public List<Property> properties = new List<Property>();
        public List<Method> methods = new List<Method>();

        public List<Model> models = new List<Model>();

        internal Model(XPathNavigator navigator, string name, Model parent)
        {
            this.name = name;
            this.parent = parent;

            foreach (XPathNavigator n in navigator.Select("*"))
            {
                if (n.Name == "interface")
                {
                    foreach (XPathNavigator m in n.Select("method"))
                    {
                        methods.Add(new Method(m, this));
                    }
                    break;
                }
                properties.Add(new Property(n));
            }

            XPathNodeIterator iterator = navigator.Select("record/model");

            foreach (Property p in properties)
            {
                if (p.type == "record")
                {
                    iterator.MoveNext();
                    models.Add(new Model(iterator.Current, p.model, this));
                }
            }
        }
    }

    public class ServiceXml
    {
        // Description

        public string name;
        public string guard;
        public string include;
        public string ns;

        public List<string> namespaces = new List<string>();

        public Model model;

        public List<Model> models = new List<Model>();

        private static void XmlValidationEventHandler(object sender, ValidationEventArgs e)
        {
            throw (e.Exception);
        }

        public ServiceXml(string uri)
        {
            ValidationEventHandler validator = new ValidationEventHandler(XmlValidationEventHandler);

            // Read the schema from the same directory as this library

            Assembly assembly = Assembly.GetExecutingAssembly();

            string path = System.IO.Path.GetDirectoryName(assembly.Location) + "/Service.xsd";

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

            name = navigator.SelectSingleNode("/service/description/name").Value;

            guard = "DEFINE_";

            include = null;

            ns = null;

            foreach (XPathNavigator n in navigator.Select("/service/description/namespace"))
            {
                string s = n.Value;

                namespaces.Add(s);

                guard += s.ToUpper();
                guard += "_";

                include += s;
                include += "/";

                if (ns != null)
                {
                    ns += "::";
                }

                ns += s;
            }

            guard += name.ToUpper();

            include += name + ".h";

            XPathNavigator m = navigator.SelectSingleNode("/service/model");

            model = new Model(m, name, null);

            GetModels(model);
        }

        internal void GetModels(Model model)
        {
            models.Add(model);

            foreach (Model m in model.models)
            {
                GetModels(m);
            }
        }

        internal void Add(Model model, List<Model> list)
        {
            list.Add(model);

            foreach (Model m in model.models)
            {
                Add(m, list);
            }
        }

        public List<Model> Related(Model model)
        {
            List<Model> list = new List<Model>();

            Add(model, list);

            return (list);
        }

        public List<Model> Chain(Model model, Model ancestor)
        {
            List<Model> chain = new List<Model>(); ;

            if (ancestor == null)
            {
                return (chain);
            }

            Model m = model;

            while (m != ancestor && m != null)
            {
                chain.Insert(0, m);
                m = m.parent;
            }

            return (chain);
        }

        public string Prefix(Model model, Model ancestor)
        {
            string prefix = null;

            if (ancestor == null)
            {
                return (prefix);
            }

            Model m = model;

            while (m != ancestor && m != null)
            {
                prefix = m.name + prefix;
                m = m.parent;
            }

            return (prefix);
        }

        public Model Child(Model model, Model ancestor)
        {
            Model child = null;

            Model m = model;

            while (m != ancestor)
            {
                child = m;
                m = m.parent;
            }

            return (child);
        }
    }
}
