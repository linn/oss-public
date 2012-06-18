using System;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

class Xslt
{
    [STAThread]

    static void Main(string[] aArgs)
    {
        if (aArgs.Length < 3)
        {
            ShowUsage();
        }

        try
        {
            XsltArgumentList arguments = new XsltArgumentList();

            for (uint i = 3; i < aArgs.Length; i += 2)
            {
                if (i + 1 >= aArgs.Length)
                {
                    ShowUsage();
                }

                string key = aArgs[i];

                if (key.Length < 2)
                {
                    ShowUsage();
                }

                if (key.StartsWith("-"))
                {
                    key = key.Substring(1);

                    string value = aArgs[i + 1];

                    arguments.AddParam(key, String.Empty, value);
                }
                else
                {
                    ShowUsage();
                }
            }

            System.Xml.Xsl.XslCompiledTransform transform = new XslCompiledTransform();

            transform.Load(aArgs[1]);
            transform.OutputSettings.Indent = true;
            transform.OutputSettings.IndentChars = "  ";
            transform.OutputSettings.CloseOutput = true;
            transform.OutputSettings.ConformanceLevel = ConformanceLevel.Auto;
            transform.OutputSettings.Encoding = Encoding.UTF8;
            transform.OutputSettings.NewLineChars = "\n";
            transform.OutputSettings.NewLineHandling = NewLineHandling.Replace;

            XmlReader input = XmlReader.Create(aArgs[0]);
            XmlWriter output = XmlWriter.Create(aArgs[2], transform.OutputSettings);

            transform.Transform(input, arguments, output, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(String.Empty);
            ShowUsage();
        }

        System.Environment.Exit(0);
    }

    static void ShowUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(String.Empty);
        Console.WriteLine("    Xslt [xml file] [xslt file] [output file] [parameter-list]");
        Console.WriteLine("    where [parameter-list] is a list of -key value pairs");
        Console.WriteLine(String.Empty);
        System.Environment.Exit(1);
    }
}
