
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using Linn;


namespace Linn.ProductSupport
{

public enum ETagType {
    eString,
    eBinary,
    eTUint,
    eTInt
}

public partial class Tags
{
    public class XmlError : Exception {
        public XmlError(string aMsg) : base("Error parsing tags XML: " + aMsg) {
        }
    }
    public class UndefinedTag : Exception {
        public UndefinedTag(string aTag) : base("Undefined Tag: " + aTag) {}
    }
    public class UndefinedKey : Exception {
        public UndefinedKey(uint aKey) : base("Undefined Key: " + aKey) {}
    }

    private class Entry
    {
        public Entry(string aTag, ETagType aType, uint aKey, string aDesc, uint aCount) {
            if ((aTag[0] != 'u') || aTag.Substring(1,1) != aTag.Substring(1,1).ToUpper()) {
                throw new XmlError("bad tag name " + aTag);
            }
            for (int i=0 ; i<aTag.Length ; i++) {
                if (Char.IsLetterOrDigit(aTag[i]) == false) {
                    throw new XmlError("bad tag name " + aTag);
                }
            }
            iTag = aTag;
            iKey = aKey;
            iType = aType;
            iDesc = aDesc;
            iCount = aCount;
        }
        public string iTag;
        public uint iKey;
        public ETagType iType;
        public string iDesc;
        public uint iCount;
    }

    public Tags(string aXml) {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(aXml);

        XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/Tags");
        if (nodeList.Count != 1) {
            throw new XmlError("bad xml");
        }

        try {
            nodeList = xmlDoc.DocumentElement.SelectNodes("/Tags/Entry");
            foreach (XmlNode entryNode in nodeList) {
                string tag = ParseXml(entryNode, "Tag");
                uint key = uint.Parse(ParseXml(entryNode, "Key"));
                string desc = ParseXml(entryNode, "Desc");
                ETagType type = StringToTagType(ParseXml(entryNode, "Type"));

                Entry entry = new Entry(tag, type, key, desc, 1);
                iTagToEntry[entry.iTag] = entry;
                iKeyToEntry[entry.iKey] = entry;
            }

            nodeList = xmlDoc.DocumentElement.SelectNodes("/Tags/Group");
            foreach (XmlNode groupNode in nodeList) {
                string tag = ParseXml(groupNode, "Tag");
                uint key = uint.Parse(ParseXml(groupNode, "Key"));
                string desc = ParseXml(groupNode, "Desc");
                ETagType type = StringToTagType(ParseXml(groupNode, "Type"));
                uint count = uint.Parse(ParseXml(groupNode, "Count"));

                Entry entry = new Entry(tag, type, key, desc, count);
                iGroupList.Add(entry);
            }
        }
        catch (Exception) {
            throw new XmlError("bad xml");
        }
    }

    // public instance functions
    public string Tag(uint aKey) {
        // check single entries
        try {
            return iKeyToEntry[aKey].iTag;
        }
        catch (KeyNotFoundException) {
        }
        // check groups
        Entry group = Group(aKey);
        return group.iTag + (aKey-group.iKey).ToString();
    }
    public uint Key(string aTag) {
        // check single entries
        try {
            return iTagToEntry[aTag].iKey;
        }
        catch (KeyNotFoundException) {
        }

        // check groups
        int index = aTag.Length-1;
        while (Char.IsDigit(aTag[index])) {
            index -= 1;
        }
        if (index == aTag.Length-1) {
            // no digits on the end - not a valid group tag name
            throw new UndefinedTag(aTag);
        }
        string baseTag = aTag.Substring(0, index+1);
        uint num = uint.Parse(aTag.Substring(index+1));
        foreach (Entry group in iGroupList) {
            if (baseTag == group.iTag) {
                if (num < group.iCount) {
                    return group.iKey + num;
                }
            }
        }
        throw new UndefinedTag(aTag);
    }
    public ETagType Type(uint aKey) {
        // check single entries
        try {
            return iKeyToEntry[aKey].iType;
        }
        catch (KeyNotFoundException) {
        }
        // check groups
        Entry group = Group(aKey);
        return group.iType;
    }
    public string Desc(uint aKey) {
        // check single entries
        try {
            return iKeyToEntry[aKey].iDesc;
        }
        catch (KeyNotFoundException) {
        }
        // check groups
        Entry group = Group(aKey);
        return group.iDesc;
    }

    private string ParseXml(XmlNode aNode, string aElement) {
        // parse the node for the element named aElement
        XmlNodeList nodeList = aNode.SelectNodes(aElement);
        if (nodeList.Count == 1) {
            return nodeList[0].InnerText;
        }
        else {
            throw new XmlError("undefined element " + aElement);
        }
    }
    private ETagType StringToTagType(string aType) {
        switch (aType) {
        case "String":
            return ETagType.eString;
        case "Binary":
            return ETagType.eBinary;
        case "TUint":
            return ETagType.eTUint;
        case "TInt":
            return ETagType.eTInt;
        default:
            throw new XmlError("bad tag type " + aType);
        }
    }
    private Entry Group(uint aKey) {
        foreach (Entry group in iGroupList) {
            if (aKey >= group.iKey && aKey < group.iKey+group.iCount) {
                return group;
            }
        }
        throw new UndefinedKey(aKey);
    }

    private Dictionary<string, Entry> iTagToEntry = new Dictionary<string, Entry>();
    private Dictionary<uint, Entry> iKeyToEntry = new Dictionary<uint, Entry>();
    private List<Entry> iGroupList = new List<Entry>();
}

}   // namespace Linn


