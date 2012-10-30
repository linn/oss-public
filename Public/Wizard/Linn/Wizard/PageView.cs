
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;

using OpenHome.Xapp;


namespace Linn.Wizard
{
    public class WizardView : IView
    {
        public WizardView(PageDefinitions.Page aPageDef, string aResourcePath)
        {
            iPageDef = aPageDef;
            iResourcePath = aResourcePath;
        }

        public string Id
        {
            get { return iPageDef.Name; }
        }

        public bool WriteResource(string aUri, int aIndex, List<string> aLanguageList, IResourceWriter aResourceWriter)
        {
            if (aUri == "/index.xapp")
            {
                // load the xapp file for parsing
                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(iResourcePath, iPageDef.Xapp, "index.xapp"));

                // get the list of fragment nodes and replace the fragments
                XmlNodeList nodes = doc.SelectNodes("//page-frag");

                // on the Mac, removing a node from the doc while iterating over the XmlNodeList causes an exception
                // so create a separate normal list of the nodes to remove and do that after the page-frag nodes have
                // been added
                List<XmlNode> toRemove = new List<XmlNode>();

                foreach (XmlNode node in nodes)
                {
                    // read in the fragment file
                    string fragFile = Path.Combine(iResourcePath, node.Attributes["frag"].InnerText);
                    StreamReader fragReader = new StreamReader(fragFile);
                    string xmlFrag = fragReader.ReadToEnd();
                    fragReader.Close();

                    // create a xml fragment to insert
                    XmlDocumentFragment docFrag = doc.CreateDocumentFragment();
                    docFrag.InnerXml = xmlFrag;

                    // replace the node with the fragment
                    XmlNode parent = node.ParentNode;
                    parent.InsertAfter(docFrag, node);
                    toRemove.Add(node);
                }

                // now remove the actual <page-frag> nodes
                foreach (XmlNode node in toRemove)
                {
                    node.ParentNode.RemoveChild(node);
                }

                // render some of the controls into the dom
                RenderControls(doc, "Text");
                RenderControls(doc, "Image");
                RenderControls(doc, "Control");

                // convert the xapp xml to a byte array
                byte[] buffer = System.Text.UTF8Encoding.UTF8.GetBytes(doc.OuterXml);

                aResourceWriter.WriteResourceBegin(buffer.Length, string.Empty);
                aResourceWriter.WriteResource(buffer, buffer.Length);
                aResourceWriter.WriteResourceEnd();

                return true;
            }
            else
            {
                // the view Id is different from the name of the folder in which the index.xapp resides so, in order to pick up
                // other files in that folder (such as .js files), we have to handle it here
                string uri = aUri.Substring(aIndex);
                uri = uri.TrimStart('/');
                string[] split = uri.Split(new char[] { '?' }, 2);
                string uriWithoutQuery = split[0];

                try
                {
                    FileInfo f = new FileInfo(Path.Combine(iResourcePath, iPageDef.Xapp, uriWithoutQuery));

                    string mimetype = string.Empty;
                    switch (f.Extension)
                    {
                        case ".css":
                            mimetype = "text/css";
                            break;
                        case ".js":
                            mimetype = "application/javascript";
                            break;
                    }

                    FileStream stream = File.OpenRead(f.FullName);
                    aResourceWriter.WriteResourceBegin((int)stream.Length, mimetype);

                    byte[] buffer = new byte[4 * 1024];
                    while (true)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                            break;

                        aResourceWriter.WriteResource(buffer, bytesRead);
                    }

                    aResourceWriter.WriteResourceEnd();
                    stream.Close();
                    return true;
                }
                // these exceptions are equivalent to 404 Not Found so silently handle and let the Xapp framework deal with it
                catch (FileNotFoundException) { }
                catch (DirectoryNotFoundException) { }
            }

            return false;
        }

        private void RenderControls(XmlDocument aDoc, string aType)
        {
            Component[] comps = iPageDef.GetComponents(aType);

            foreach (Component comp in comps)
            {
                // find the XML node for this component
                XmlNode compNode = aDoc.SelectSingleNode("//*[@id='" + comp.Id + "']");
                if (compNode == null)
                {
                    continue;
                }

                // get the current style attribute
                XmlNode styleNode = compNode.Attributes.GetNamedItem("style");

                CssStyle style = new CssStyle((styleNode != null) ? styleNode.Value : string.Empty);

                // set some style values
                if (!string.IsNullOrEmpty(comp.BackgroundImage))
                {
                    style.Set("background-image", "url(Resources/" + comp.BackgroundImage + ")");
                }
                if (!string.IsNullOrEmpty(comp.Top))
                {
                    style.Set("top", comp.Top);
                }
                if (!string.IsNullOrEmpty(comp.Left))
                {
                    style.Set("left", comp.Left);
                }

                // set the style attribute of the node
                string styleValue = style.ToString();
                if (!string.IsNullOrEmpty(styleValue))
                {
                    if (styleNode == null)
                    {
                        XmlAttribute styleAttr = aDoc.CreateAttribute("style");
                        compNode.Attributes.Append(styleAttr);
                        styleNode = styleAttr;
                    }

                    styleNode.Value = styleValue;
                }


                // get the current class attribute
                XmlNode classNode = compNode.Attributes.GetNamedItem("class");

                CssClass cls = new CssClass((classNode != null) ? classNode.Value : string.Empty);

                if (!string.IsNullOrEmpty(comp.Class))
                {
                    cls.Add(comp.Class);
                }

                // set the class attribute of the node
                string classValue = cls.ToString();
                if (!string.IsNullOrEmpty(classValue))
                {
                    if (classNode == null)
                    {
                        XmlAttribute classAttr = aDoc.CreateAttribute("class");
                        compNode.Attributes.Append(classAttr);
                        classNode = classAttr;
                    }

                    classNode.Value = classValue;
                }
            }
        }

        private PageDefinitions.Page iPageDef;
        private string iResourcePath;
    }


    public class CssStyle
    {
        public CssStyle(string aStyle)
        {
            iDict = new Dictionary<string, string>();

            string[] items = aStyle.Split(';');

            foreach (string item in items)
            {
                string[] keyval = item.Split(new char[] { ':' }, 2);

                if (keyval.Length == 2)
                {
                    iDict.Add(keyval[0].Trim().ToLower(), keyval[1].Trim());
                }
            }
        }

        public void Set(string aKey, string aValue)
        {
            aKey = aKey.Trim().ToLower();
            aValue = aValue.Trim();

            if (iDict.ContainsKey(aKey))
            {
                iDict[aKey] = aValue;
            }
            else
            {
                iDict.Add(aKey, aValue);
            }
        }

        public override string ToString()
        {
            string s = string.Empty;

            foreach (string key in iDict.Keys)
            {
                s += key + ": " + iDict[key] + "; ";
            }
            s = s.Trim();

            return s;
        }

        private Dictionary<string, string> iDict;
    }


    public class CssClass
    {
        public CssClass(string aClass)
        {
            iList = new List<string>();

            string[] items = aClass.Split();

            foreach (string item in items)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    iList.Add(item);
                }
            }
        }

        public void Add(string aClass)
        {
            CssClass c = new CssClass(aClass);

            foreach (string cls in c.iList)
            {
                if (!iList.Contains(cls))
                {
                    iList.Add(cls);
                }
            }
        }

        public override string ToString()
        {
            string s = string.Empty;

            foreach (string cls in iList)
            {
                s += cls + " ";
            }
            s = s.Trim();

            return s;
        }

        private List<string> iList;
    }
}

