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
        private string iWidth;
        private string iImage;
        private string iBackgroundImage;
        private string iColor;
        private string iBackgroundColor;
        private string iClass;

        public Component(string aParameter, XmlAttributeCollection aAttributes, PageDefinitions.Constant[] aConstants)
        {
            if (aAttributes != null && aAttributes.Count > 0)
            {
                XmlNode state = aAttributes.GetNamedItem("id");
                if (state != null)
                {
                    Set(state.Value, aParameter, aAttributes, aConstants);
                }
            }
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

            if (iWidth == "")
            {
                iWidth = c.iWidth;
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

            if (iClass == "")
            {
                iClass = c.iClass;
            }
        }

        private void Set(string aId, string aParameter, XmlAttributeCollection aAttributes, PageDefinitions.Constant[] aConstants)
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
            iWidth = "";
            iImage = "";
            iBackgroundImage = "";
            iColor = "";
            iBackgroundColor = "";
            iClass = "";

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
                iTop = getValue(top, iTop, aConstants);

                XmlNode left = aAttributes.GetNamedItem("left");
                iLeft = getValue(left, iLeft, aConstants);

                XmlNode height = aAttributes.GetNamedItem("height");
                iHeight = getValue(height, iHeight, aConstants);

                XmlNode width = aAttributes.GetNamedItem("width");
                iWidth = getValue(width, iWidth, aConstants);

                XmlNode image = aAttributes.GetNamedItem("image");
                iImage = getValue(image, iImage, aConstants);

                XmlNode backgroundimage = aAttributes.GetNamedItem("backgroundimage");
                iBackgroundImage = getValue(backgroundimage, iBackgroundImage, aConstants);

                XmlNode color = aAttributes.GetNamedItem("color");
                iColor = getValue(color, iColor, aConstants);

                XmlNode backgroundcolor = aAttributes.GetNamedItem("backgroundcolor");
                iBackgroundColor = getValue(backgroundcolor, iBackgroundColor, aConstants);

                XmlNode cls = aAttributes.GetNamedItem("class");
                iClass = getValue(cls, iClass, aConstants);
            }
        }

        private string getValue(XmlNode aNode, string aCurrentValue, PageDefinitions.Constant[] aConstants)
        {
            if (aNode == null)
            {
                return aCurrentValue;
            }

            string value = aNode.Value;

            if (value.StartsWith("#"))
            {
                string constId = value.Substring(1);

                foreach (PageDefinitions.Constant c in aConstants)
                {
                    if (c.Id == constId)
                    {
                        value = c.Value;
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
        public bool WidthSet
        {
            get { return (iWidth != ""); }
        }
        public string Width
        {
            get { return iWidth; }
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
        public string Class
        {
            get { return iClass; }
        }
    }
}
