using System;
using Linn.Gui;
using System.Xml;
using System.Globalization;

namespace Linn {
namespace Gui {
namespace Scenegraph {

public class Vector3d : ISerialiseObject, ICloneable
{
    public Vector3d() {
        iX = 0;
        iY = 0;
        iZ = 0;
    }
    
    public Vector3d(Vector3d aVector) {
        iX = aVector.iX;
        iY = aVector.iY;
        iZ = aVector.iZ;
    }
    
    public Vector3d(float aX, float aY, float aZ) {
        iX = aX;
        iY = aY;
        iZ = aZ;
    }
    
    public Object Clone() {
        Vector3d v = new Vector3d();
        v.iX = iX;
        v.iY = iY;
        v.iZ = iZ;
        return v;
    }
    
    public void Load(XmlNode aXmlNode) {
        NumberFormatInfo nfi = new NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("X");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iX = float.Parse(list[0].FirstChild.Value, nfi);
        }
        
        list = aXmlNode.SelectNodes("Y");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iY = float.Parse(list[0].FirstChild.Value, nfi);
        }
        
        list = aXmlNode.SelectNodes("Z");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iZ = float.Parse(list[0].FirstChild.Value, nfi);
        }
    }
    
    public void Link() {
    }
    
    public void Save(XmlTextWriter aWriter) {
        NumberFormatInfo nfi = new NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";
        aWriter.WriteStartElement("X");
        aWriter.WriteString(iX.ToString(nfi));
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Y");
        aWriter.WriteString(iY.ToString(nfi));
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Z");
        aWriter.WriteString(iZ.ToString(nfi));
        aWriter.WriteEndElement();
    }
    
    public float X {
        get {
            return iX;
        }
        set {
            iX = value;
        }
    }
    
    public float Y {
        get {
            return iY;
        }
        set {
            iY = value;
        }
    }
    
    public float Z {
        get {
            return iZ;
        }
        set {
            iZ = value;
        }
    }
    
    public float Size() {
        float squareSum = X*X + Y*Y + Z*Z;
        if(squareSum > 0.001f) {
            return (float)Math.Sqrt(squareSum);
        }
        return 0;
    }
        
    
    public bool Normalise() {
        float squareSum = X*X + Y*Y + Z*Z;
        if(squareSum > 0.001f) {
            float scale = 1.0f / (float)Math.Sqrt(squareSum);
            iX = iX * scale;
            iY = iY * scale;
            iZ = iZ * scale;
            return true;
        }
        return false;
    }
    
    public static Vector3d operator -(Vector3d lhs, Vector3d rhs) {
        Vector3d result = new Vector3d(lhs);
        result.iX -= rhs.iX;
        result.iY -= rhs.iY;
        result.iZ -= rhs.iZ;
        return result;
    }
    
    public static Vector3d operator +(Vector3d lhs, Vector3d rhs) {
        Vector3d result = new Vector3d(lhs);
        result.iX += rhs.iX;
        result.iY += rhs.iY;
        result.iZ += rhs.iZ;
        return result;
    }
    
    public static Vector3d operator ^(Vector3d lhs, Vector3d rhs) {
        return new Vector3d(lhs.iY*rhs.iZ - lhs.iZ*rhs.iY, lhs.iZ*rhs.iX - lhs.iX*rhs.iZ, lhs.iX*rhs.iY - lhs.iY*rhs.iX);
    }
    
    public static float operator *(Vector3d lhs, Vector3d rhs) {
        return lhs.iX*rhs.iX + lhs.iY*rhs.iY + lhs.iZ*rhs.iZ;
    }
    
    public static Vector3d operator *(Vector3d lhs, float value) {
        Vector3d result = new Vector3d(lhs);
        result.iX *= value;
        result.iY *= value;
        result.iZ *= value;
        return result;
    }
    
    public static Vector3d operator /(Vector3d lhs, float value) {
        Vector3d result = new Vector3d(lhs);
        float scale = 1.0f / value;
        result.iX *= scale;
        result.iY *= scale;
        result.iZ *= scale;
        return result;
    }
    
    public override bool Equals(Object obj) {
        return((obj is Vector3d) && (this == (Vector3d)obj));
    }
    
    public static bool operator ==(Vector3d lhs, Vector3d rhs) {
        return(Math.Abs(lhs.iX - rhs.iX) < 0.0001f && Math.Abs(lhs.iY - rhs.iY) < 0.0001f && Math.Abs(lhs.iZ - rhs.iZ) < 0.0001f);
    }
    
    public static bool operator !=(Vector3d lhs, Vector3d rhs) {
        return(!(lhs == rhs));
    }
    
    public override string ToString() {
        return iX + "," + iY + "," + iZ;
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    
    private float iX, iY, iZ;
}

public class Srt : ISerialiseObject, ICloneable
{
    public Srt() {
    }
    
    public Srt(Vector3d aScale, Vector3d aTranslation) {
        iScale = aScale;
        iTranslation = aTranslation;
    }
    
    public Object Clone() {
        Srt srt = new Srt();
        srt.iScale = (Vector3d)iScale.Clone();
        srt.iTranslation = (Vector3d)iTranslation.Clone();
        return srt;
    }
    
    public void Load(XmlNode aXmlNode) {
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Scale");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iScale.Load(list[0]);
        }
        
        list = aXmlNode.SelectNodes("Translation");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iTranslation.Load(list[0]);
        }
    }
    
    public void Link() {
    }
    
    public void Save(XmlTextWriter aWriter) {
        aWriter.WriteStartElement("Srt");
        aWriter.WriteStartElement("Scale");
        iScale.Save(aWriter);
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Translation");
        iTranslation.Save(aWriter);
        aWriter.WriteEndElement();
        aWriter.WriteEndElement();
    }
    
    public Vector3d Translation {
        get {
            return iTranslation;
        }
        set {
            iTranslation = value;
        }
    }
    
    public Vector3d Scale {
        get {
            return iScale;
        }
        set {
            iScale = value;
        }
    }
    
    private Vector3d iScale = new Vector3d(1, 1, 1);
    private Vector3d iTranslation = new Vector3d();
//  private Quaternion iRotation;
}

} // Scenegraph
} // Gui
} // Linn
