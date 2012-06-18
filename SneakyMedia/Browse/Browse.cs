using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;


using SneakyMedia.Database;
using SneakyMedia.Query;
using SneakyMedia.Scanner;

namespace SneakyMedia.Browse
{
    public class SchemeNotFoundError : Exception
    {
        public SchemeNotFoundError()
        {
        }
    }

    public class SchemeInvalidError : Exception
    {
        public SchemeInvalidError()
        {
        }
    }

    public interface IInput
    {
        string Id { get; }
        string Name { get; }
    }

    public interface IOutput : IInput
    {
    }

    public interface IElement
    {
        string Id { get; }
        string Name { get; }
        string Child { get; }
        IList<IInput> Inputs { get; }
        IList<IOutput> Outputs { get; }
    }

    public interface IScheme
    {
        string Id { get; }
        string Name { get; }
        IList<IElement> Elements { get; }
    }

    [XmlRoot("scheme")]

    public class Scheme : IScheme
    {
        public Scheme()
        {
            iElements = new List<IElement>();
        }

        public static IScheme LoadScheme(string aId)
        {
            Scheme scheme;

            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(Scheme));

                TextReader reader = new StreamReader(aId + ".xml");

                scheme = (Scheme)xml.Deserialize(reader);

                reader.Close();
            }
            catch (Exception e)
            {
                throw (new SchemeNotFoundError());
            }

            if (scheme.Id != aId)
            {
                throw (new SchemeInvalidError());
            }

            return (scheme);
        }

        [XmlIgnore]

        public string Id
        {
            get
            {
                return (iId);
            }
        }

        [XmlIgnore]

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        [XmlIgnore]

        public IList<IElement> Elements
        {
            get
            {
                return (iElements.AsReadOnly());
            }
        }

        [XmlElement("id")]

        public string DeserialiseId
        {
            get
            {
                throw (new ApplicationException());
            }
            set
            {
                iId = value;
            }
        }

        [XmlElement("name")]

        public string DeserialiseName
        {
            get
            {
                throw (new ApplicationException());
            }
            set
            {
                iName = value;
            }
        }

        [XmlElement("element")]

        public Element[] DeserialiseElements
        {
            get
            {
                throw (new ApplicationException());
            }
            set
            {
                if (value != null)
                {
                    foreach (Element p in value)
                    {
                        iElements.Add(p);
                    }
                }
            }
        }

        private string iId;
        private string iName;
        private List<IElement> iElements;
    }

    public class Element : IElement
    {
        public Element()
        {
            iInputs = new List<IInput>();
            iOutputs = new List<IOutput>();
        }

        [XmlIgnore]

        public string Id
        {
            get
            {
                return (iId);
            }
        }

        [XmlIgnore]

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        [XmlIgnore]

        public string Child
        {
            get
            {
                return (iChild);
            }
        }

        [XmlIgnore]

        public IList<IInput> Inputs
        {
            get
            {
                return (iInputs.AsReadOnly());
            }
        }

        [XmlIgnore]

        public IList<IOutput> Outputs
        {
            get
            {
                return (iOutputs.AsReadOnly());
            }
        }


        [XmlElement("id")]

        public string DeserialiseId
        {
            get
            {
                throw (new ApplicationException());
            }
            set
            {
                iId = value;
            }
        }

        [XmlElement("name")]

        public string DeserialiseName
        {
            get
            {
                throw (new ApplicationException());
            }
            set
            {
                iName = value;
            }
        }

        [XmlElement("child")]

        public string DeserialiseChild
        {
            get
            {
                throw (new ApplicationException());
            }
            set
            {
                iChild = value;
            }
        }

        [XmlElement("input")]

        public Input[] DeserialiseInputs
        {
            get
            {
                throw (new ApplicationException());
            }
            set
            {
                if (value != null)
                {
                    foreach (Input i in value)
                    {
                        iInputs.Add(i);
                    }
                }
            }
        }

        [XmlElement("output")]

        public Output[] DeserialiseOutputs
        {
            get
            {
                throw (new ApplicationException());
            }
            set
            {
                if (value != null)
                {
                    foreach (Output o in value)
                    {
                        iOutputs.Add(o);
                    }
                }
            }
        }

        private string iId;
        private string iName;
        private List<IInput> iInputs;
        private List<IOutput> iOutputs;
        private string iChild;
    }

    public class Input : IInput
    {
        public Input()
        {
        }

        [XmlIgnore]

        public string Id
        {
            get
            {
                return (iId);
            }
        }

        [XmlIgnore]

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        [XmlElement("id")]

        public string DeserialiseId
        {
            get
            {
                throw (new ApplicationException());
            }
            set
            {
                iId = value;
            }
        }

        [XmlElement("name")]

        public string DeserialiseName
        {
            get
            {
                throw (new ApplicationException());
            }
            set
            {
                iName = value;
            }
        }

        private string iId;
        private string iName;
    }

    public class Output : Input, IOutput
    {
        public Output()
        {
        }
    }

    public interface IPageInput
    {
        string Name { get; }
        string Value { get; }
    }

    public interface IPageOutput
    {
        string Name { get; }
    }

    public interface ILocation
    {
        string Id { get; }
        IList<string> Values { get; }
    }

    public interface IPage
    {
        string Name { get; }
        ILocation Location { get; }
        IList<IPageInput> Inputs { get; }
        IList<IPageOutput> Outputs { get; }
        IList<IList<string>> Items { get; }
        ILocation ItemLocation(uint aItem);
    }

    public interface IBrowser : IPage
    {
        ILocation Home { get; }
        void Goto(ILocation aLocation);
    }

    internal class PageInput : IPageInput
    {
        public PageInput(string aName, string aValue)
        {
            iName = aName;
            iValue = aValue;
        }

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        public string Value
        {
            get
            {
                return (iValue);
            }
        }

        private string iName;
        private string iValue;
    }

    internal class PageOutput : IPageOutput
    {
        public PageOutput(string aName)
        {
            iName = aName;
        }

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        private string iName;
    }

    public class Location : ILocation
    {
        public Location(string aId)
        {
            iId = aId;
            iValues = new List<string>();
        }

        public string Id
        {
            get
            {
                return (iId);
            }
        }

        public void AddValue(string aValue)
        {
            iValues.Add(aValue);
        }

        public IList<string> Values
        {
            get
            {
                return (iValues.AsReadOnly());
            }
        }

        private string iId;
        private List<string> iValues;
    }

    public abstract class Page : IPage
    {
        public Page()
        {
            iInputs = new List<IPageInput>();
            iOutputs = new List<IPageOutput>();
        }

        public void SetName(string aName)
        {
            iName = aName;
        }

        public void AddInput(IPageInput aInput)
        {
            iInputs.Add(aInput);
        }

        public void AddOutput(IPageOutput aOutput)
        {
            iOutputs.Add(aOutput);
        }

        public void SetItems(IList<IList<string>> aItems)
        {
            iItems = aItems;
        }

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        public IList<IPageInput> Inputs
        {
            get
            {
                return (iInputs);
            }
        }

        public IList<IPageOutput> Outputs
        {
            get
            {
                return (iOutputs);
            }
        }

        public IList<IList<string>> Items
        {
            get
            {
                return (iItems);
            }
        }

        public abstract ILocation Location { get; }
        public abstract ILocation ItemLocation(uint aItem);

        private string iName;
        private List<IPageInput> iInputs;
        private List<IPageOutput> iOutputs;
        private IList<IList<string>> iItems;
    }

    internal class PageHome : Page
    {
        public PageHome(IScheme aScheme)
        {
            iScheme = aScheme;

            iLocation =  new Location(iScheme.Id);

            SetName(iScheme.Name);

            AddOutput(new PageOutput("Browse"));

            iItemLocations = new List<ILocation>();

            // Find all the elements that require no input values
            // These represent the top level elements

            List<IList<string>> items = new List<IList<string>>();

            foreach (IElement element in iScheme.Elements)
            {
                if (element.Inputs.Count == 0)
                {
                    List<string> row = new List<string>();
                    iItemLocations.Add(new Location(element.Id));
                    row.Add(element.Name);
                    items.Add(row);
                }
            }

            SetItems(items);
        }

        public override ILocation Location
        {
            get
            {
                return (iLocation);
            }
        }

        public override ILocation ItemLocation(uint aItem)
        {
            if (aItem < iItemLocations.Count)
            {
                return (iItemLocations[(int)aItem]);
            }

            return (null);
        }

        private IScheme iScheme;
        private ILocation iLocation;
        private List<ILocation> iItemLocations;
    }

    internal class PageElement : Page
    {
        public PageElement(IEngine aEngine, IElement aElement, IElement aChild, IList<string> aValues)
        {
            iEngine = aEngine;
            iElement = aElement;
            iChild = aChild;

            iLocation = new Location(iElement.Id);

            foreach (string value in aValues)
            {
                iLocation.AddValue(value);
            }

            SetName(iElement.Name);

            // add inputs

            IEnumerator<string> values = aValues.GetEnumerator();

            iValues = new Dictionary<string, string>();

            List<IMetadatum> metadata = new List<IMetadatum>();

            foreach (IInput input in iElement.Inputs)
            {
                values.MoveNext();

                iLocation.AddValue(values.Current);

                IPageInput i = new PageInput(input.Name, values.Current);

                iValues.Add(input.Id, values.Current);

                Metadatum m = new Metadatum(new TagGeneral(input.Id), values.Current);

                metadata.Add(m);

                AddInput(i);
            }

            // add outputs

            List<ITag> tags = new List<ITag>();

            foreach (IOutput output in iElement.Outputs)
            {
                IPageOutput o = new PageOutput(output.Name);

                TagGeneral t = new TagGeneral(output.Id);

                tags.Add(t);

                AddOutput(o);
            }

            // collect items

            SetItems(iEngine.QueryItems(metadata, 0, tags));
        }

        public override ILocation Location
        {
            get
            {
                return (iLocation);
            }
        }

        public override ILocation ItemLocation(uint aItem)
        {
            if (iChild != null)
            {
                if (aItem < Items.Count)
                {
                    Location location = new Location(iChild.Id);

                    IList<string> row = Items[(int)aItem];

                    foreach (IInput input in iChild.Inputs)
                    {
                        location.AddValue(FindValue(input.Id, row));
                    }

                    return (location);
                }
            }

            return (null);
        }

        private string FindValue(string aId, IList<string> aItems)
        {
            // find in input values

            string value;

            try
            {
                value = iValues[aId];
            }
            catch (KeyNotFoundException)
            {
                // find in output items

                int i = 0;

                foreach (IOutput output in iElement.Outputs)
                {
                    if (output.Id == aId)
                    {
                        return (aItems[i]);
                    }

                    i++;
                }

                return (String.Empty);
            }

            return (value);
        }

        private IEngine iEngine;
        private IElement iElement;
        private IElement iChild;
        private Location iLocation;
        private Dictionary<string, string> iValues;
    }

    public class Browser : IBrowser
    {
        internal Browser(IEngine aEngine, string aSchemeId)
        {
            iEngine = aEngine;
            iScheme = iEngine.LoadScheme(aSchemeId);
            iHome = new PageHome(iScheme);
            iPage = iHome;
        }

        // IPage

        public string Name
        {
            get
            {
                return (iPage.Name);
            }
        }

        public ILocation Location
        {
            get
            {
                return (iPage.Location);
            }
        }

        public IList<IPageInput> Inputs
        {
            get
            {
                return (iPage.Inputs);
            }
        }

        public IList<IPageOutput> Outputs
        {
            get
            {
                return (iPage.Outputs);
            }
        }

        public IList<IList<string>> Items
        {
            get
            {
                return (iPage.Items);
            }
        }

        public ILocation ItemLocation(uint aItem)
        {
            return (iPage.ItemLocation(aItem));
        }

        // IBrowser

        public ILocation Home
        {
            get
            {
                return (iHome.Location);
            }
        }

        public void Goto(ILocation aLocation)
        {
            // is it the home location?

            if (aLocation.Id == iScheme.Id)
            {
                iPage = iHome;
            }
            else
            {
                foreach (IElement element in iScheme.Elements)
                {
                    if (element.Id == aLocation.Id)
                    {
                        IElement child = null;

                        if (element.Child != null)
                        {
                            foreach (IElement c in iScheme.Elements)
                            {
                                if (c.Id == element.Child)
                                {
                                    child = c;
                                    break;
                                }
                            }
                        }
                        iPage = new PageElement(iEngine, element, child, aLocation.Values);
                    }
                }
            }
        }

        IEngine iEngine;
        IScheme iScheme;
        PageHome iHome;
        Page iPage;
    }

    public abstract class ModuleBrowse : Module
    {
        public const string kModuleType = "Browse";

        protected ModuleBrowse(string aName, Version aVersion)
            : base(aName, aVersion, kModuleType)
        {
        }
    }

}

