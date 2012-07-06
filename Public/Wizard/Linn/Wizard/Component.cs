using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace Linn.Wizard
{

    public class Component
    {
        // defines information for a page component, eg text, image or control
        private string iId;
        private string iParameter;
        private string iEnabled;
        private string iDisplayed;
        private string iVisible;
        private string iTop;
        private string iLeft;
        private string iHeight;
        private string iImage;
        private string iBackgroundImage;
        private string iColor;
        private string iBackgroundColor;
        private string iCustomAction;
        private string iVar;        // probably need a DeviceText and a DeviceImage variable for substitution in Render()
        List<Constant> iConstants;

        public Component(string aParameter, XmlAttributeCollection aAttributes, List<Constant> aConstants)
        {
            iConstants = aConstants;
            if (aAttributes != null && aAttributes.Count > 0)
            {
                XmlNode state = aAttributes.GetNamedItem("id");
                if (state != null)
                {
                    Set(state.Value, aParameter, aAttributes);
                }
            }
        }

        public Component(string aId, string aParameter, XmlAttributeCollection aAttributes, List<Constant> aConstants)
        {
            iConstants = aConstants;
            Set(aId, aParameter, aAttributes);
        }

        public void Merge(Component c)
        {
            if (iParameter == "")
            {
                iParameter = c.iParameter;
            }

            if (iEnabled == "")
            {
                iEnabled = c.iEnabled;
            }

            if (iDisplayed == "") {
                iDisplayed = c.iDisplayed;
            }

            if (iVisible == "")
            {
                iVisible = c.iVisible;
            }

            if (iTop == "")
            {
                iTop = c.iTop;
            }

            if (iLeft == "")
            {
                iLeft = c.iLeft;
            }

            if (iHeight == "")
            {
                iHeight = c.iHeight;
            }

            if (iImage == "")
            {
                iImage = c.iImage;
            }

            if (iBackgroundImage == "")
            {
                iBackgroundImage = c.iBackgroundImage;
            }

            if (iColor == "")
            {
                iColor = c.iColor;
            }

            if (iBackgroundColor == "")
            {
                iBackgroundColor = c.iBackgroundColor;
            }

            if (iCustomAction == "")
            {
                iCustomAction = c.iCustomAction;
            }

            if (iVar == "")
            {
                iVar = c.iVar;
            }
            
        }

        private void Set(string aId, string aParameter, XmlAttributeCollection aAttributes)
        {
            Id = aId;
            string text = aParameter.Replace("\\n", "<br>");      //cannot define control characters in xml so convert here
            text = text.Replace("[ul]", "<ul id=\"TextList\">");
            text = text.Replace("[/ul]", "</ul>");
            text = text.Replace("[ol]", "<ol id=\"TextList\">");
            text = text.Replace("[/ol]", "</ol>");
            text = text.Replace("[li]", "<li>");
            text = text.Replace("[/li]", "</li>");

            //must be able to do better than this with Regex! (note - .net 1.0 Regex split different to 2.0)
            if (text.IndexOf('[') >= 0)
            {
                string[] split = text.Split('[');
                text = "";
                foreach (string s in split)
                {
                    bool valid = false;

                    // string must begin with "[id, width,height,image url]"
                    int delim = s.IndexOf(']');
                    if (delim > 0)
                    {
                        string[] extract = s.Split(']');
                        string[] field = extract[0].Split(',');
                        int width = 0;
                        int height = 0;
                        string filename = "";
                        string id = "";
                        try
                        {
                            width = Convert.ToInt16(field[1]);
                            height = Convert.ToInt16(field[2]);
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }

                        if (width > 0 &&  height > 0)
                        {
                            id = field[0].Trim();
                            filename = field[3].Trim();

                            valid = true;
                            string img = "<img id=\"" + id + "\" src=\"" + filename + "\" width=\"" + width +"\" height=\"" + height + "\" />";

                            text += img;
                            if (extract.Length > 0)
                            {
                                text += extract[1];
                            }
                        }

                    }
                    if (valid == false)  // use original text
                    {
                        text += s;
                    }
                }
            }

            if (text.IndexOf('{') >= 0) {
                string[] split = text.Split('{');
                text = "";
                foreach (string s in split) {
                    bool valid = false;

                    // string must begin with "{url,link text}"
                    int delim = s.IndexOf('}');
                    if (delim > 0) {
                        string[] extract = s.Split('}');
                        string[] field = extract[0].Split(',');
                        string url = field[0].Trim();
                        string linktext = field[1].Trim();

                        valid = true;
                        string link = "<a target=\"_self\" href=\"#\" onClick=\"linkClicked('" + url + "')\">" + linktext + "</a>";
                        text += link;
                        if (extract.Length > 0) {
                            text += extract[1];
                        }

                    }
                    if (valid == false) {  // use original text
                        text += s;
                    }
                }
            }
            
            iParameter = text;
            iEnabled = "";
            iDisplayed = "";
            iVisible = "";
            iTop = "";
            iLeft = "";
            iHeight = "";
            iImage = "";
            iBackgroundImage = "";
            iColor = "";
            iBackgroundColor = "";
            iCustomAction = "";
            iVar = "";

            if (aAttributes != null && aAttributes.Count > 0)
            {
                XmlNode state = aAttributes.GetNamedItem("state");
                if (state != null)
                {
                    if (state.Value.ToLower() == "hide")
                    {
                        iVisible = "false";
                    }
                    else if (state.Value.ToLower() == "unhide")
                    {
                        iVisible = "true";
                    }
                    else if (state.Value.ToLower() == "enable")
                    {
                        iEnabled = "true";
                    }
                    else if (state.Value.ToLower() == "disable")
                    {
                        iEnabled = "false";
                    }
                    else if (state.Value.ToLower() == "display") {
                        iDisplayed = "true";
                    }
                    else if (state.Value.ToLower() == "nodisplay") {
                        iDisplayed = "false";
                    }
                }
                XmlNode top = aAttributes.GetNamedItem("top");
                iTop = getValue(top, iTop);

                XmlNode left = aAttributes.GetNamedItem("left");
                iLeft = getValue(left, iLeft);

                XmlNode height = aAttributes.GetNamedItem("height");
                iHeight = getValue(height, iHeight);

                XmlNode action = aAttributes.GetNamedItem("action");
                iCustomAction = getValue(action, iCustomAction);

                XmlNode image = aAttributes.GetNamedItem("image");
                iImage = getValue(image, iImage);

                XmlNode backgroundimage = aAttributes.GetNamedItem("backgroundimage");
                iBackgroundImage = getValue(backgroundimage, iBackgroundImage);

                XmlNode color = aAttributes.GetNamedItem("color");
                iColor = getValue(color, iColor);

                XmlNode backgroundcolor = aAttributes.GetNamedItem("backgroundcolor");
                iBackgroundColor = getValue(backgroundcolor, iBackgroundColor);

                XmlNode var = aAttributes.GetNamedItem("class");
                iVar = getValue(var, iVar);

            }
        }

        private string getValue(XmlNode aNode, string aValue)
        {
            string value = aValue;
            if (aNode != null)
            {
                value = aNode.Value;
                if (value.StartsWith("#"))
                {
                    foreach (Constant c in iConstants)
                    {
                        if (value == c.Id)
                        {
                            value = c.Value;
                        }
                    }
                }
            }
             
            return value;
        }

        public string Id
        {
            get { return iId; }
            set { iId = value; }
        }
        public string Parameter
        {
            get { return iParameter; }
        }
        public bool Enabled
        {
            get
            {
                if (iEnabled == "" || iEnabled == "true") 
                    return true;
                return false; 
            }
        }
        public bool Displayed {
            get {
                if (iDisplayed == "" || iDisplayed == "true")
                    return true;
                return false;
            }
        }
        public bool Visible
        {
            get {
                if (iVisible == "" || iVisible == "true")
                    return true;
                return false;                
            }
        }
        public string Top
        {
            get { return iTop; }
        }
        public string Left
        {
            get { return iLeft; }
        }
        public bool HeightSet
        {
            get { return (iHeight != ""); }
        }
        public string Height
        {
            get { return iHeight; }
        }
        public string Image
        {
            get { return iImage; }
        }
        public string BackgroundImage
        {
            get { return iBackgroundImage; }
        }
        public string Color
        {
            get { return iColor; }
        }
        public string BackgroundColor
        {
            get { return iBackgroundColor; }
        }
        public string CustomAction
        {
            get { return iCustomAction; }
        }
        public string Var
        {
            get { return iVar; }
        }
    }

    public class PageComponents
    {
        string pageName;
        string pageType;
        string pageTag;
        string pageProduct;
        List<Component> text;
        List<Component> image;
        List<Component> control;
        List<Component> action;
        List<Component> special;

        public PageComponents()
        {
            text = new List<Component>();
            image = new List<Component>();
            control = new List<Component>();
            action = new List<Component>();
            special = new List<Component>();
        }

        public PageComponents(PageComponents aPageComponents)
        {
            pageName = aPageComponents.PageName;
            pageType = aPageComponents.pageType;
            pageTag = aPageComponents.pageTag;
            pageProduct = aPageComponents.pageProduct;
            text = new List<Component>(aPageComponents.text);
            image = new List<Component>(aPageComponents.image);
            control = new List<Component>(aPageComponents.control);
            action = new List<Component>(aPageComponents.action);
            special = new List<Component>(aPageComponents.special);
        }
 
        public void Add(string aComponentType, Component aComponent)
        {
            List<Component> list;
            switch (aComponentType)
            {
                case "Text":
                    list = text;
                    break;
                case "Image":
                    list = image;
                    break;
                case "Control":
                    list = control;
                    break;
                case "Action":
                    list = action;
                    break;
                case "Special":
                    list = special;
                    break;
                default:
                    Console.WriteLine("Unsupported component type " + aComponentType + "\n");
                    return;
            }

            foreach (Component c in list) 
            {
                if (c.Id == aComponent.Id)
                {
                    aComponent.Merge(c);        // merge in orifginal data
                    int index = list.IndexOf(c);
                    list.RemoveAt(index);       // remove original definition
                    break;
                }
            }
            list.Add(aComponent);

        }

        public List<Component> GetList(string aComponentType)
        {
            switch (aComponentType)
            {
                case "Text":
                    return (text);
                case "Image":
                    return (image);
                case "Control":
                    return (control);
                case "Action":
                    return (action);
                case "Special":
                    return (special);
                default:
                    Console.WriteLine("Unsupported component type " + aComponentType + "\n");
                    return (null);
            }
        }

        public string PageType
        {
            get { return pageType; }
            set { pageType = value; }
        }
        public string PageTag
        {
            get { return pageTag; }
            set { pageTag = value; }
        }
        public string PageDefault
        {
            get { return pageProduct; }
            set { pageProduct = value; }
        }

        public string PageName
        {
            get { return pageName; }
            set { pageName = value; }
        }

        public string Next
        {
            get
            {
                foreach (Component a in action)
                {
                    if (a.Id == "Next")
                    {
                        return a.Parameter;
                    }
                }
                return "";
            }
        }

        public string Diagnostics
        {
            get
            {
                foreach (Component a in action)
                {
                    if (a.Id == "Diagnostics")
                    {
                        return a.Parameter;
                    }
                }
                return "";
            }
        }

        // MenuItem could be more generic!
        public string MenuItem1
        {
            get
            {
                foreach (Component a in action)
                {
                    if (a.Id == "MenuItem1")
                    {
                        return a.Parameter;
                    }
                }
                return "";
            }
        }

        public string MenuItem2
        {
            get
            {
                foreach (Component a in action)
                {
                    if (a.Id == "MenuItem2")
                    {
                        return a.Parameter;
                    }
                }
                return "";
            }
        }

        public string MenuItem3
        {
            get
            {
                foreach (Component a in action)
                {
                    if (a.Id == "MenuItem3")
                    {
                        return a.Parameter;
                    }
                }
                return "";
            }
        }

        public string MenuItem4
        {
            get
            {
                foreach (Component a in action)
                {
                    if (a.Id == "MenuItem4")
                    {
                        return a.Parameter;
                    }
                }
                return "";
            }
        }

        public string MenuItem5
        {
            get
            {
                foreach (Component a in action)
                {
                    if (a.Id == "MenuItem5")
                    {
                        return a.Parameter;
                    }
                }
                return "";
            }
        }

        public string MenuItem6
        {
            get
            {
                foreach (Component a in action)
                {
                    if (a.Id == "MenuItem6")
                    {
                        return a.Parameter;
                    }
                }
                return "";
            }
        }

        public string MenuItem7 {
            get {
                foreach (Component a in action) {
                    if (a.Id == "MenuItem7") {
                        return a.Parameter;
                    }
                }
                return "";
            }
        }

        public string CustomAction(string aAttribute, out string aValue)
        {
            foreach (Component a in action)
            {
                if (a.Id == aAttribute)
                {
                    aValue = a.Parameter;
                    return a.CustomAction;
                }
            }
            aValue = "";
            return "";
        }

    }

}
