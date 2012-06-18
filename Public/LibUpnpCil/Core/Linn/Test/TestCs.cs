
using System;
using System.Xml;
using System.IO;
using Linn;
using Linn.TestFramework;


class SuiteXmlEscaping : Suite
{
    public SuiteXmlEscaping() : base("Tests for C# XML escaping") {
    }

    public override void Test() {
        //
        // Test creating XML
        //
        XmlDocument doc = new XmlDocument();
        XmlElement elem1 = doc.CreateElement("Elem1");
        XmlElement elem2 = doc.CreateElement("Elem2");
        XmlElement elem3 = doc.CreateElement("Elem3");
        XmlElement elem4 = doc.CreateElement("Elem4");
        XmlElement elem5 = doc.CreateElement("Elem5");
        XmlElement elem6 = doc.CreateElement("Elem6");
        XmlElement elem7 = doc.CreateElement("Elem7");
        XmlAttribute attr2 = doc.CreateAttribute("Attr2");
        XmlAttribute attr3 = doc.CreateAttribute("Attr3");
        XmlAttribute attr4 = doc.CreateAttribute("Attr4");
        XmlAttribute attr5 = doc.CreateAttribute("Attr5");
        XmlAttribute attr6 = doc.CreateAttribute("Attr6");
        XmlAttribute attr7 = doc.CreateAttribute("Attr7");

        doc.AppendChild(elem1);
        elem1.AppendChild(elem2);
        elem1.AppendChild(elem3);
        elem1.AppendChild(elem4);
        elem1.AppendChild(elem5);
        elem1.AppendChild(elem6);
        elem1.AppendChild(elem7);
        elem2.SetAttributeNode(attr2);
        elem3.SetAttributeNode(attr3);
        elem4.SetAttributeNode(attr4);
        elem5.SetAttributeNode(attr5);
        elem6.SetAttributeNode(attr6);
        elem7.SetAttributeNode(attr7);

        //
        // Set element and attribute values using InnerText
        //
        // For XmlElement (setting value via InnerText):
        //     escaped: < > &
        //     not escaped: " '
        // For XmlAttribute  (setting value via InnerText):
        //     escaped: < > & "
        //     not escaped: '
        elem2.InnerText = "value 2";
        elem3.InnerText = "value <3";
        elem4.InnerText = "value >4";
        elem5.InnerText = "value &5";
        elem6.InnerText = "value \"6";
        elem7.InnerText = "value \'7";
        attr2.InnerText = "attr value 2";
        attr3.InnerText = "attr value <3";
        attr4.InnerText = "attr value >4";
        attr5.InnerText = "attr value &5";
        attr6.InnerText = "attr value \"6";
        attr7.InnerText = "attr value \'7";

        TEST(elem2.OuterXml == "<Elem2 Attr2=\"attr value 2\">value 2</Elem2>");
        TEST(elem2.InnerXml == "value 2");
        TEST(elem2.InnerText == "value 2");
        TEST(attr2.InnerXml == "attr value 2");
        TEST(attr2.InnerText == "attr value 2");

        TEST(elem3.OuterXml == "<Elem3 Attr3=\"attr value &lt;3\">value &lt;3</Elem3>");
        TEST(elem3.InnerXml == "value &lt;3");
        TEST(elem3.InnerText == "value <3");
        TEST(attr3.InnerXml == "attr value &lt;3");
        TEST(attr3.InnerText == "attr value <3");

        TEST(elem4.OuterXml == "<Elem4 Attr4=\"attr value &gt;4\">value &gt;4</Elem4>");
        TEST(elem4.InnerXml == "value &gt;4");
        TEST(elem4.InnerText == "value >4");
        TEST(attr4.InnerXml == "attr value &gt;4");
        TEST(attr4.InnerText == "attr value >4");

        TEST(elem5.OuterXml == "<Elem5 Attr5=\"attr value &amp;5\">value &amp;5</Elem5>");
        TEST(elem5.InnerXml == "value &amp;5");
        TEST(elem5.InnerText == "value &5");
        TEST(attr5.InnerXml == "attr value &amp;5");
        TEST(attr5.InnerText == "attr value &5");

        TEST(elem6.OuterXml == "<Elem6 Attr6=\"attr value &quot;6\">value \"6</Elem6>");
        TEST(elem6.InnerXml == "value \"6");
        TEST(elem6.InnerText == "value \"6");
        TEST(attr6.InnerXml == "attr value \"6");
        TEST(attr6.InnerText == "attr value \"6");

        TEST(elem7.OuterXml == "<Elem7 Attr7=\"attr value \'7\">value \'7</Elem7>");
        TEST(elem7.InnerXml == "value \'7");
        TEST(elem7.InnerText == "value \'7");
        TEST(attr7.InnerXml == "attr value \'7");
        TEST(attr7.InnerText == "attr value \'7");

        //
        // Set element and attribute values using InnerXml
        //
        try {
            elem3.InnerXml = "value <3";
            TEST(false);
        }
        catch (XmlException) {
            TEST(true);
        }
        try {
            elem5.InnerXml = "value &5";
            TEST(false);
        }
        catch (XmlException) {
            TEST(true);
        }
        elem4.InnerXml = "value >4";
        elem6.InnerXml = "value \"6";
        elem7.InnerXml = "value \'7";
        TEST(elem4.InnerXml == "value &gt;4");
        TEST(elem6.InnerXml == "value \"6");
        TEST(elem7.InnerXml == "value \'7");
        TEST(elem4.InnerText == "value >4");
        TEST(elem6.InnerText == "value \"6");
        TEST(elem7.InnerText == "value \'7");

        try {
            attr3.InnerXml = "attr value <3";
            TEST(false);
        }
        catch (XmlException) {
            TEST(true);
        }
        try {
            attr5.InnerXml = "attr value &5";
            TEST(false);
        }
        catch (XmlException) {
            TEST(true);
        }
        attr4.InnerXml = "attr value >4";
        attr7.InnerXml = "attr value \'7";
        TEST(attr4.InnerXml == "attr value &gt;4");
        TEST(attr7.InnerXml == "attr value \'7");
        TEST(attr4.InnerText == "attr value >4");
        TEST(attr7.InnerText == "attr value \'7");

        // Oh dear
        attr6.InnerXml = "attr value \"6";
        switch(Environment.OSVersion.Platform)
        {
        case PlatformID.Unix:
            TEST(attr6.InnerXml == "attr value \"6");
            TEST(attr6.InnerText == "attr value \"6");
            break;
        default:
            TEST(attr6.InnerXml == "attr value ");
            TEST(attr6.InnerText == "attr value ");
            break;
        }

        //
        // XmlTextWriter
        //
        StringWriter stringWriter = new StringWriter();
        XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
        xmlWriter.WriteStartElement("Elem1");
        xmlWriter.WriteStartElement("Elem2");
        xmlWriter.WriteAttributeString("Attr2", "attr value 2");
        xmlWriter.WriteString("value 2");
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("Elem3");
        xmlWriter.WriteAttributeString("Attr3", "attr value <3");
        xmlWriter.WriteString("value <3");
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("Elem4");
        xmlWriter.WriteAttributeString("Attr4", "attr value >4");
        xmlWriter.WriteString("value >4");
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("Elem5");
        xmlWriter.WriteAttributeString("Attr5", "attr value &5");
        xmlWriter.WriteString("value &5");
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("Elem6");
        xmlWriter.WriteAttributeString("Attr6", "attr value \"6");
        xmlWriter.WriteString("value \"6");
        xmlWriter.WriteEndElement();
        xmlWriter.WriteStartElement("Elem7");
        xmlWriter.WriteAttributeString("Attr7", "attr value \'7");
        xmlWriter.WriteString("value \'7");
        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndElement();

        doc = new XmlDocument();
        doc.LoadXml(stringWriter.ToString());
        XmlNodeList nodeList;
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "value 2");
        TEST(nodeList[0].InnerText == "value 2");
        nodeList = nodeList[0].SelectNodes("@Attr2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "attr value 2");
        TEST(nodeList[0].InnerText == "attr value 2");

        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem3");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "value &lt;3");
        TEST(nodeList[0].InnerText == "value <3");
        nodeList = nodeList[0].SelectNodes("@Attr3");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "attr value &lt;3");
        TEST(nodeList[0].InnerText == "attr value <3");

        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem4");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "value &gt;4");
        TEST(nodeList[0].InnerText == "value >4");
        nodeList = nodeList[0].SelectNodes("@Attr4");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "attr value &gt;4");
        TEST(nodeList[0].InnerText == "attr value >4");

        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem5");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "value &amp;5");
        TEST(nodeList[0].InnerText == "value &5");
        nodeList = nodeList[0].SelectNodes("@Attr5");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "attr value &amp;5");
        TEST(nodeList[0].InnerText == "attr value &5");

        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem6");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "value \"6");
        TEST(nodeList[0].InnerText == "value \"6");
        nodeList = nodeList[0].SelectNodes("@Attr6");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "attr value \"6");
        TEST(nodeList[0].InnerText == "attr value \"6");

        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem7");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "value \'7");
        TEST(nodeList[0].InnerText == "value \'7");
        nodeList = nodeList[0].SelectNodes("@Attr7");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "attr value \'7");
        TEST(nodeList[0].InnerText == "attr value \'7");

        //
        // Parsing elements
        //
        // &lt;
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2>&lt;</Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "&lt;");
        TEST(nodeList[0].InnerText == "<");
        // &gt;
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2>&gt;</Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "&gt;");
        TEST(nodeList[0].InnerText == ">");
        // &amp;
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2>&amp;</Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "&amp;");
        TEST(nodeList[0].InnerText == "&");
        // &quot;
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2>&quot;</Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "\"");
        TEST(nodeList[0].InnerText == "\"");
        // &apos;
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2>&apos;</Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "\'");
        TEST(nodeList[0].InnerText == "\'");
        // <
        try {
            doc = new XmlDocument();
            doc.LoadXml("<Elem1><Elem2><</Elem2></Elem1>");
            TEST(false);
        }
        catch (XmlException) {
            TEST(true);
        }
        // >
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2>></Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "&gt;");
        TEST(nodeList[0].InnerText == ">");
        // &
        try {
            doc = new XmlDocument();
            doc.LoadXml("<Elem1><Elem2>&</Elem2></Elem1>");
            TEST(false);
        }
        catch (XmlException) {
            TEST(true);
        }
        // "
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2>\"</Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "\"");
        TEST(nodeList[0].InnerText == "\"");
        // '
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2>\'</Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "\'");
        TEST(nodeList[0].InnerText == "\'");

        //
        // Parsing attributes
        //
        // &lt;
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2 Attr2=\"&lt;\"></Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2/@Attr2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "&lt;");
        TEST(nodeList[0].InnerText == "<");
        // &gt;
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2 Attr2=\"&gt;\"></Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2/@Attr2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "&gt;");
        TEST(nodeList[0].InnerText == ">");
        // &amp;
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2 Attr2=\"&amp;\"></Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2/@Attr2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "&amp;");
        TEST(nodeList[0].InnerText == "&");
        // &quot;
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2 Attr2=\"&quot;\"></Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2/@Attr2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "\"");
        TEST(nodeList[0].InnerText == "\"");
        // &apos;
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2 Attr2=\"&apos;\"></Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2/@Attr2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "\'");
        TEST(nodeList[0].InnerText == "\'");
        // <
        try {
            doc = new XmlDocument();
            doc.LoadXml("<Elem1><Elem2 Attr2=\"<\"></Elem2></Elem1>");
            TEST(false);
        }
        catch (XmlException) {
            TEST(true);
        }
        // >
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2 Attr2=\">\"></Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2/@Attr2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "&gt;");
        TEST(nodeList[0].InnerText == ">");
        // &
        try {
            doc = new XmlDocument();
            doc.LoadXml("<Elem1><Elem2 Attr2=\"&\"></Elem2></Elem1>");
            TEST(false);
        }
        catch (XmlException) {
            TEST(true);
        }
        // "
        try {
            doc = new XmlDocument();
            doc.LoadXml("<Elem1><Elem2 Attr2=\"\"\"></Elem2></Elem1>");
            TEST(false);
        }
        catch (XmlException) {
            TEST(true);
        }
        // '
        doc = new XmlDocument();
        doc.LoadXml("<Elem1><Elem2 Attr2=\"\'\"></Elem2></Elem1>");
        nodeList = doc.DocumentElement.SelectNodes("/Elem1/Elem2/@Attr2");
        TEST(nodeList.Count == 1);
        TEST(nodeList[0].InnerXml == "\'");
        TEST(nodeList[0].InnerText == "\'");
    }
}

class TestCs
{
    static void Main(string[] aArgs) {
        Helper helper = new Helper(aArgs);
        helper.ProcessCommandLine();

        Runner runner = new Runner("Some C# tests");
        runner.Add( new SuiteXmlEscaping() );
        runner.Run();

        helper.Dispose();
    }
}


