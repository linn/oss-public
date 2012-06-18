using Linn;
using System.Xml.XPath;
using System.Xml;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System;
using System.Drawing;
using System.Globalization;

namespace Linn {
namespace Gui {
namespace Scenegraph {
      
public class NodeFont : NodeHit
{
    public enum EJustification {
        EJ_Left = 0,
        EJ_Centre = 1,
        EJ_Right = 2
    }
    
    public enum EAlignment {
        EA_Top = 0,
        EA_Centre = 1,
        EA_Bottom = 2
    }
    
    public enum ETrimming {
        ET_None = 0,
        ET_Character = 1,
        ET_EllipsisCharacter = 2,
        ET_Word = 3,
        ET_EllipsisWord = 4,
        ET_EllipsisPath = 5
    }
    
    public NodeFont() : base(UniqueName("NodeFont")) {
    }

    protected NodeFont(string aName) : base(aName) {
    }
    
    public override Object Clone() {
        NodeFont f = new NodeFont();
        Clone(f);
        return f;
    }
    
    protected void Clone(NodeFont aNodeFont) {
        Clone((NodeHit)aNodeFont);
        aNodeFont.iJustification = iJustification;
        aNodeFont.iAlignment = iAlignment;
        aNodeFont.iTrimming = iTrimming;
        aNodeFont.iColour = iColour;
        aNodeFont.iFaceName = iFaceName;
        aNodeFont.iPointSize = iPointSize;
        aNodeFont.iBold = iBold;
        aNodeFont.iItalic = iItalic;
        aNodeFont.iUnderline = iUnderline;
        aNodeFont.iFont = iFont;
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Justification");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iJustification = (EJustification)Enum.Parse(typeof(EJustification), list[0].FirstChild.Value, true);
        }
        
        list = aXmlNode.SelectNodes("Alignment");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iAlignment = (EAlignment)Enum.Parse(typeof(EAlignment), list[0].FirstChild.Value, true);
        }
        
        list = aXmlNode.SelectNodes("Trimming");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iTrimming = (ETrimming)Enum.Parse(typeof(ETrimming), list[0].FirstChild.Value, true);
        }
        
        list = aXmlNode.SelectNodes("Colour");
        if(list != null) {
            XmlNodeList element;
            int red = 0, green = 0, blue = 0, alpha = 0;
            Assert.Check(list.Count == 1);
            element = list[0].SelectNodes("R");
            if(element != null) {
                Assert.Check(element.Count == 1);
                red = int.Parse(element[0].FirstChild.Value);
            }
            element = list[0].SelectNodes("G");
            if(element != null) {
                Assert.Check(element.Count == 1);
                green = int.Parse(element[0].FirstChild.Value);
            }
            element = list[0].SelectNodes("B");
            if(list != null) {
                Assert.Check(element.Count == 1);
                blue = int.Parse(element[0].FirstChild.Value);
            }
            element = list[0].SelectNodes("A");
            if(list != null) {
                Assert.Check(list.Count == 1);
                alpha = int.Parse(element[0].FirstChild.Value);
            }
            iColour = new Colour(alpha, red, green, blue);
        }
        
        // NOTE: GDI specific code
        list = aXmlNode.SelectNodes("Font");
        if(list != null) {
            Assert.Check(list.Count == 1);
            if(list[0].FirstChild != null) {
                XmlNodeList element;
                element = list[0].SelectNodes("FaceName");
                if(element != null) {
                    Assert.Check(element.Count == 1);
                    iFaceName = element[0].FirstChild.Value;
                }
                element = list[0].SelectNodes("PointSize");
                if(element != null) {
                    Assert.Check(element.Count == 1);
                    NumberFormatInfo nfi = new NumberFormatInfo();
                    nfi.NumberDecimalSeparator = ".";
                    iPointSize = float.Parse(element[0].FirstChild.Value, nfi);
                }
                FontStyle style = FontStyle.Regular;
                element = list[0].SelectNodes("Bold");
                if(element != null) {
                    Assert.Check(element.Count == 1);
                    if(bool.Parse(element[0].FirstChild.Value)) {
                        style |= FontStyle.Bold;
                        iBold = true;
                    }
                }
                element = list[0].SelectNodes("Italic");
                if(element != null) {
                    Assert.Check(element.Count == 1);
                    if(bool.Parse(element[0].FirstChild.Value)) {
                        iItalic = true;
                        style |= FontStyle.Italic;
                    }
                }
                element = list[0].SelectNodes("Underlined");
                if(element != null) {
                    Assert.Check(element.Count == 1);
                    if(bool.Parse(element[0].FirstChild.Value)) {
                        iUnderline = true;
                        style |= FontStyle.Underline;
                    }
                }
                iFont = new Font(iFaceName, iPointSize, style);
            }
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("Justification");
        aWriter.WriteString(((int)iJustification).ToString());
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Alignment");
        aWriter.WriteString(((int)iAlignment).ToString());
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Trimming");
        aWriter.WriteString(((int)iTrimming).ToString());
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Colour");
        aWriter.WriteStartElement("R");
        aWriter.WriteString(iColour.R.ToString());
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("G");
        aWriter.WriteString(iColour.G.ToString());
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("B");
        aWriter.WriteString(iColour.B.ToString());
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("A");
        aWriter.WriteString(iColour.A.ToString());
        aWriter.WriteEndElement();
        aWriter.WriteEndElement();
        
        // NOTE: GDI specific code
        aWriter.WriteStartElement("Font");
        if(iFont != null) {
            aWriter.WriteStartElement("FaceName");
            aWriter.WriteString(iFont.Name);
            aWriter.WriteEndElement();
            aWriter.WriteStartElement("PointSize");
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            aWriter.WriteString(iFont.Size.ToString(nfi));
            aWriter.WriteEndElement();
            aWriter.WriteStartElement("Bold");
            aWriter.WriteString(((iFont.Style & FontStyle.Bold) == FontStyle.Bold).ToString());
            aWriter.WriteEndElement();
            aWriter.WriteStartElement("Italic");
            aWriter.WriteString(((iFont.Style & FontStyle.Italic) == FontStyle.Italic).ToString());
            aWriter.WriteEndElement();
            aWriter.WriteStartElement("Underlined");
            aWriter.WriteString(((iFont.Style & FontStyle.Underline) == FontStyle.Underline).ToString());
            aWriter.WriteEndElement();
        }
        aWriter.WriteEndElement();
    }
    
    public string FaceName {
        get {
            return iFaceName;
        }
        set {
            if(value != iFaceName) {
                iFaceName = value;
                CreateFont();
                ObserverUpdate();
            }
        }
    }
    
    public float PointSize {
        get {
            return iPointSize;
        }
        set {
            if(value != iPointSize) {
                iPointSize = value;
                CreateFont();
                ObserverUpdate();
            }
        }
    }
    
    public bool Bold {
        get {
            return iBold;
        }
        set {
            if(value != iBold) {
                iBold = value;
                CreateFont();
                ObserverUpdate();
            }
        }
    }
    
    public bool Italic {
        get {
            return iItalic;
        }
        set {
            if(value != iItalic) {
                iItalic = value;
                CreateFont();
                ObserverUpdate();
            }
        }
    }
    
    public bool Underline {
        get {
            return iUnderline;
        }
        set {
            if(value != iUnderline) {
                iUnderline = value;
                CreateFont();
                ObserverUpdate();
            }
        }
    }
    
    public EJustification Justification {
        get {
            return iJustification;
        }
        set {
            if(value != iJustification) {
                iJustification = value;
                ObserverUpdate();
            }
        }
    }
    
    public EAlignment Alignment {
        get {
            return iAlignment;
        }
        set {
            if(value != iAlignment) {
                iAlignment = value;
                ObserverUpdate();
            }
        }
    }
    
    public ETrimming Trimming {
        get {
            return iTrimming;
        }
        set {
            if(value != iTrimming) {
                iTrimming = value;
                ObserverUpdate();
            }
        }
    }
    
    public virtual Colour Colour {
        get {
            return iColour;
        }
        set {
            if(value != iColour) {
                iColour = value;
                ObserverUpdate();
            }
        }
    }
    
    public Font CurrFont {
        get {
            return iFont;
        }
        set {
            if(value != iFont) {
                iFont = value;
                ObserverUpdate();
            }
        }
    }
    
    private void CreateFont() {
        FontStyle style = FontStyle.Regular;
        if(iBold) {
            style |= FontStyle.Bold;
        }
        if(iItalic) {
            style |= FontStyle.Italic;
        }
        if(iUnderline) {
            style |= FontStyle.Underline;
        }
        CurrFont = new Font(iFaceName, iPointSize, style);
    }
    
    protected EJustification iJustification = EJustification.EJ_Left;
    protected EAlignment iAlignment = EAlignment.EA_Top;
    protected ETrimming iTrimming = ETrimming.ET_None;
    protected Colour iColour = new Colour(255, 0, 0, 0);
    protected string iFaceName = "Arial";
    protected float iPointSize = 10;
    protected bool iBold = false;
    protected bool iItalic = false;
    protected bool iUnderline = false;
    protected Font iFont = null;
}

} // Scenegraph
} // Gui
} // Linn
