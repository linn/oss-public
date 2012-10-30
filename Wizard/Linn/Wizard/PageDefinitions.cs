
using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;


namespace Linn.Wizard
{
    public class PageDefinitions
    {
        public PageDefinitions(string aFilename)
        {
            // load the file
            XmlDocument doc = new XmlDocument();
            doc.Load(aFilename);

            // parse constants
            XmlNodeList nodes = doc.SelectNodes("/DSWizard/Pages/Constants/Const");

            List<Constant> constants = new List<Constant>();
            foreach (XmlNode node in nodes)
            {
                constants.Add(new Constant(node));
            }

            // parse defaults
            nodes = doc.SelectNodes("/DSWizard/Pages/Default");

            List<Default> defaults = new List<Default>();
            foreach (XmlNode node in nodes)
            {
                defaults.Add(new Default(node, constants.ToArray()));
            }

            // parse pages
            nodes = doc.SelectNodes("/DSWizard/Pages/Page");

            iPages = new List<Page>();
            foreach (XmlNode node in nodes)
            {
                iPages.Add(new Page(node, constants.ToArray(), defaults.ToArray()));
            }
        }

        public Page[] Pages
        {
            get { return iPages.ToArray(); }
        }

        private List<Page> iPages;


        // definitions of <Const> element
        public class Constant
        {
            public Constant(XmlNode aNode)
            {
                XmlNode idNode = aNode.Attributes.GetNamedItem("id");
                Assert.Check(idNode != null);

                iId = idNode.Value;
                iValue = aNode.InnerText;
            }

            public string Id
            {
                get { return iId; }
            }

            public string Value
            {
                get { return iValue; }
            }

            private string iId;
            private string iValue;
        }


        // definition of <Default> element
        public class Default : ComponentList
        {
            public Default(XmlNode aNode, Constant[] aConstants)
            {
                // parse the defaults name
                XmlNode nameNode = aNode.Attributes.GetNamedItem("Defaults");
                Assert.Check(nameNode != null);
                iName = nameNode.Value;

                // parse the child component elements
                iTexts = ParseComponents(TypeText, aNode, aConstants);
                iImages = ParseComponents(TypeImage, aNode, aConstants);
                iControls = ParseComponents(TypeControl, aNode, aConstants);
                iActions = ParseActions(aNode);
                iWidgets = ParseWidgets(aNode);
            }

            public string Name
            {
                get { return iName; }
            }

            private string iName;
        }


        // definition of <Page> element
        public class Page : ComponentList
        {
            public Page(XmlNode aNode, Constant[] aConstants, Default[] aDefaults)
            {
                // parse attributes for this page
                XmlNode attrNode = aNode.Attributes.GetNamedItem("Name");
                Assert.Check(attrNode != null);
                iName = attrNode.Value;

                attrNode = aNode.Attributes.GetNamedItem("Type");
                Assert.Check(attrNode != null);
                iType = attrNode.Value;

                attrNode = aNode.Attributes.GetNamedItem("Xapp");
                Assert.Check(attrNode != null);
                iXapp = attrNode.Value;

                attrNode = aNode.Attributes.GetNamedItem("Model");
                iModel = (attrNode != null) ? attrNode.Value : null;

                attrNode = aNode.Attributes.GetNamedItem("Defaults");
                string defsString = (attrNode != null) ? attrNode.Value : "";
                iDefaults = defsString.Split();

                // parse child component elements and merge with defaults
                iTexts = MergeComponents(TypeText, aNode, aConstants, aDefaults);
                iImages = MergeComponents(TypeImage, aNode, aConstants, aDefaults);
                iControls = MergeComponents(TypeControl, aNode, aConstants, aDefaults);
                iActions = MergeActions(aNode, aDefaults);
                iWidgets = ParseWidgets(aNode);
            }

            public string Name
            {
                get { return iName; }
            }

            public string Type
            {
                get { return iType; }
            }

            public string Xapp
            {
                get { return iXapp; }
            }

            public string Model
            {
                get { return iModel; }
            }

            public string[] Defaults
            {
                get { return iDefaults; }
            }

            private Component[] MergeComponents(string aType, XmlNode aNode, Constant[] aConstants, Default[] aDefaults)
            {
                // start with an empty list
                Component[] list = new Component[0];

                // merge in all default components with the "All" defaults
                foreach (Default defs in aDefaults)
                {
                    if (defs.Name == "All")
                    {
                        list = MergeComponents(list, defs.GetComponents(aType));
                    }
                }

                // merge in all default components for this page
                foreach (string defName in iDefaults)
                {
                    foreach (Default defs in aDefaults)
                    {
                        if (defs.Name == defName)
                        {
                            list = MergeComponents(list, defs.GetComponents(aType));
                        }
                    }
                }

                // finally add the components defined in the page
                list = MergeComponents(list, ParseComponents(aType, aNode, aConstants));

                return list;
            }

            private Component[] MergeComponents(Component[] aOriginalComponents, Component[] aNewComponents)
            {
                // copy the original list
                List<Component> list = new List<Component>(aOriginalComponents);

                // merge in each component in the new list
                foreach (Component newComp in aNewComponents)
                {
                    Component origComp = Find(list, newComp.Id);

                    if (origComp != null)
                    {
                        // this new component is also in the original list - remove the original from the list and re-add the merged component
                        list.Remove(origComp);
                        newComp.Merge(origComp);
                        list.Add(newComp);
                    }
                    else
                    {
                        // this new component is not in the original list - just add it
                        list.Add(newComp);
                    }
                }

                return list.ToArray();
            }

            private Component Find(List<Component> aList, string aId)
            {
                foreach (Component comp in aList)
                {
                    if (comp.Id == aId)
                    {
                        return comp;
                    }
                }

                return null;
            }

            private Action[] MergeActions(XmlNode aNode, Default[] aDefaults)
            {
                // start with an empty list
                Action[] list = new Action[0];

                // merge in all default actions with the "All" defaults
                foreach (Default defs in aDefaults)
                {
                    if (defs.Name == "All")
                    {
                        list = MergeActions(list, defs.Actions);
                    }
                }

                // merge in all default components for this page
                foreach (string defName in iDefaults)
                {
                    foreach (Default defs in aDefaults)
                    {
                        if (defs.Name == defName)
                        {
                            list = MergeActions(list, defs.Actions);
                        }
                    }
                }

                // finally add the components defined in the page
                list = MergeActions(list, ParseActions(aNode));

                return list;
            }

            private Action[] MergeActions(Action[] aOriginalActions, Action[] aNewActions)
            {
                // copy the original list
                List<Action> list = new List<Action>(aOriginalActions);

                // simply append the new list
                list.AddRange(aNewActions);

                return list.ToArray();
            }

            private Action Find(List<Action> aList, string aId)
            {
                foreach (Action action in aList)
                {
                    if (action.Id == aId)
                    {
                        return action;
                    }
                }

                return null;
            }

            private string iName;
            private string iType;
            private string iXapp;
            private string iModel;
            private string[] iDefaults;
        }


        // <Default> and <Page> elements have the same child element structure
        public class ComponentList
        {
            public const string TypeText = "Text";
            public const string TypeImage = "Image";
            public const string TypeControl = "Control";

            protected Component[] ParseComponents(string aType, XmlNode aNode, Constant[] aConstants)
            {
                // basic parsing of the components represented by <aType> elements
                XmlNodeList nodes = aNode.SelectNodes(aType);

                List<Component> comps = new List<Component>();
                foreach (XmlNode node in nodes)
                {
                    string value = node.InnerText;

                    comps.Add(new Component(value, node.Attributes, aConstants));
                }

                return comps.ToArray();
            }

            protected Action[] ParseActions(XmlNode aNode)
            {
                List<Action> actions = new List<Action>();

                XmlNodeList nodes = aNode.SelectNodes("Action");
                foreach (XmlNode node in nodes)
                {
                    XmlNode attr = node.Attributes.GetNamedItem("type");

                    if (attr == null)
                    {
                        actions.Add(new ActionBasic(node));
                    }
                    else if (attr.Value == "navigate")
                    {
                        actions.Add(new ActionNavigate(node));
                    }
                    else if (attr.Value == "data")
                    {
                        actions.Add(new ActionData(node));
                    }
                    else if (attr.Value == "method")
                    {
                        actions.Add(new ActionMethod(node));
                    }
                }

                return actions.ToArray();
            }

            protected Widget[] ParseWidgets(XmlNode aNode)
            {
                List<Widget> widgets = new List<Widget>();

                XmlNodeList nodes = aNode.SelectNodes("Widget");
                foreach (XmlNode node in nodes)
                {
                    widgets.Add(new Widget(node));
                }

                return widgets.ToArray();
            }

            public Component[] Texts
            {
                get { return iTexts; }
            }

            public Component[] Images
            {
                get { return iImages; }
            }

            public Component[] Controls
            {
                get { return iControls; }
            }

            public Action[] Actions
            {
                get { return iActions; }
            }

            public Widget[] Widgets
            {
                get { return iWidgets; }
            }

            public Component[] GetComponents(string aType)
            {
                switch (aType)
                {
                case TypeText:
                    return iTexts;
                case TypeImage:
                    return iImages;
                case TypeControl:
                    return iControls;
                default:
                    Assert.Check(false);
                    return null;
                }
            }

            protected Component[] iTexts;
            protected Component[] iImages;
            protected Component[] iControls;
            protected Action[] iActions;
            protected Widget[] iWidgets;
        }


        // Base class for all <ActionXXX> elements
        public abstract class Action
        {
            protected Action(XmlNode aNode)
            {
                XmlNode attr = aNode.Attributes.GetNamedItem("id");
                Id = attr.Value;
            }

            public string Id
            {
                get;
                private set;
            }
        }

        public class ActionBasic : Action
        {
            public ActionBasic(XmlNode aNode)
                : base(aNode)
            {
                Parameter = aNode.InnerText;
            }

            public string Parameter
            {
                get;
                private set;
            }
        }

        public class ActionNavigate : Action
        {
            public ActionNavigate(XmlNode aNode)
                : base(aNode)
            {
                PageId = aNode.InnerText;

                XmlNode attr = aNode.Attributes.GetNamedItem("source");
                Source = (attr != null) ? attr.Value : null;
            }

            public string PageId
            {
                get;
                private set;
            }

            public string Source
            {
                get;
                private set;
            }
        }

        public class ActionData : Action
        {
            public ActionData(XmlNode aNode)
                : base(aNode)
            {
                XmlNode attr = aNode.Attributes.GetNamedItem("dataId");
                DataId = attr.Value;

                attr = aNode.Attributes.GetNamedItem("dataValue");
                DataValue = (attr != null) ? attr.Value : null;
            }

            public string DataId
            {
                get;
                private set;
            }

            public string DataValue
            {
                get;
                private set;
            }
        }

        public class ActionMethod : Action
        {
            public ActionMethod(XmlNode aNode)
                : base(aNode)
            {
                MethodId = aNode.InnerText;
            }

            public string MethodId
            {
                get;
                private set;
            }
        }


        // Widget class
        public class Widget
        {
            public Widget(XmlNode aNode)
            {
                XmlNode attr = aNode.Attributes.GetNamedItem("id");
                Id = attr.Value;

                attr = aNode.Attributes.GetNamedItem("xappEvent");
                XappEvent = attr.Value;

                XmlNode node = aNode.SelectSingleNode("DataId");
                DataId = node.InnerText;

                node = aNode.SelectSingleNode("AllowedValues");
                if (node != null)
                {
                    attr = node.Attributes.GetNamedItem("source");

                    if (attr != null)
                    {
                        // allowed values specified through a data source
                        AllowedValues = null;
                        AllowedValuesSource = attr.Value;
                    }
                    else
                    {
                        // allowed values specified in the XML
                        XmlNodeList nodes = node.SelectNodes("Value");

                        List<string> allowedValues = new List<string>();

                        foreach (XmlNode node2 in nodes)
                        {
                            allowedValues.Add(node2.InnerText);
                        }

                        AllowedValues = allowedValues.ToArray();
                        AllowedValuesSource = null;
                    }
                }
                else
                {
                    // no allowed values specified
                    AllowedValues = null;
                    AllowedValuesSource = null;
                }
            }

            public string Id
            {
                get;
                private set;
            }

            public string XappEvent
            {
                get;
                private set;
            }

            public string DataId
            {
                get;
                private set;
            }

            public string[] AllowedValues
            {
                get;
                private set;
            }

            public string AllowedValuesSource
            {
                get;
                private set;
            }
        }
    }
}



