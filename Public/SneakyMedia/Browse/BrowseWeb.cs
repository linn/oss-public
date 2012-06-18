using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using SneakyMedia.Database;

namespace SneakyMedia.Browse
{
    public class ModuleBrowseWeb : ModuleBrowse
    {
        public const int kPort = 50003;
        public const int kSlots = 10;

        public ModuleBrowseWeb()
            : base("Web", new Version(1, 0, 0, 0))
        {
        }

        protected override void OnCreate()
        {
            new Thread(Listen).Start();
        }

        protected override void OnLoad()
        {
        }

        private void Listen()
        {
            HttpListener server = new HttpListener();
            server.Prefixes.Add("http://*:" + kPort + "/");
            server.Start();

            while (true)
            {
                HttpListenerContext context = server.GetContext();
                new Session(Engine, context);
            }
        }

        internal class Session
        {
            private IEngine iEngine;
            private IBrowser iBrowser;
            private HttpListenerContext iContext;

            public Session(IEngine aEngine, HttpListenerContext aContext)
            {
                iEngine = aEngine;
                iContext = aContext;
                iBrowser = new Browser(iEngine, "Telnet");
                new Thread(Process).Start();
            }

            private void Process()
            {
                StringBuilder sb = new StringBuilder();

                ILocation location = UnpackLocation(iContext.Request.Url.PathAndQuery);

                iBrowser.Goto(location);

                sb.Append("<html><body>");

                // display title

                sb.Append("<h1>" + iBrowser.Name + "</h1>");

                // display blank line

                //sb.Append("<b>");

                // display input values

                sb.Append("<table border=\"1\">");

                foreach (IPageInput input in iBrowser.Inputs)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>");
                    sb.Append(input.Name);
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append(input.Value);
                    sb.Append("</td>");
                    sb.Append("</tr>");
                }

                sb.Append("</table");

                // display blank line

                //sb.Append("<b>");

                // display items

                sb.Append("<table border=\"1\">");

                sb.Append("<tr>");

                sb.Append("<td><b>#</b></td>");

                foreach (IPageOutput output in iBrowser.Outputs)
                {
                    sb.Append("<td><b>");
                    sb.Append(output.Name);
                    sb.Append("</b></td>");
                }

                sb.Append("</tr>");

                uint line = 1;

                foreach (IList<string> row in iBrowser.Items)
                {
                    sb.Append("<tr>");

                    sb.Append("<td>");
                    sb.Append("<a href=\"");
                    sb.Append(PackLocation(iBrowser.ItemLocation(line - 1)));
                    sb.Append("\">");
                    sb.Append(line++);
                    sb.Append("</a>");
                    sb.Append("</td>");

                    foreach (string item in row)
                    {
                        sb.Append("<td>");
                        sb.Append(item);
                        sb.Append("</td>");
                    }

                    sb.Append("</tr>");
                }

                sb.Append("</table");
                /*
                    // display items

                    for (int l = 0; l < kItemLines; l++)
                    {
                        if (l + top > iBrowser.Items.Count)
                        {
                            iWriter.WriteLine(String.Empty);
                            continue;
                        }

                        string num = (top + l).ToString();

                        iWriter.Write(num);
                        iWriter.Write(Blanks(kNumberWidth - num.Length + 1));

                        index = 0;

                        IList<string> row = iBrowser.Items[top + l - 1];

                        foreach (string item in row)
                        {
                            iWriter.Write(item);
                            iWriter.Write(Blanks(widths[index++] - item.Length + 1));
                        }

                        iWriter.WriteLine(String.Empty);
                    }

                    // display blank line

                    iWriter.WriteLine(String.Empty);

                    // display item count

                    iWriter.Write(iBrowser.Items.Count);
                    iWriter.Write(" item");

                    if (iBrowser.Items.Count != 1)
                    {
                        iWriter.Write("s");
                    }

                    iWriter.WriteLine(String.Empty);

                    // display blank line

                    iWriter.WriteLine(String.Empty);

                    // display help line

                    iWriter.Write(kVt100AttributesReverseVideo);
                    iWriter.Write(kHelpLine);
                    iWriter.WriteLine(Blanks(totalwidth - kHelpLine.Length));
                    iWriter.Write(kVt100AttributesOff);
                    iWriter.Write(kVt100CursorUp);
                    iWriter.Write(kVt100CursorRight);

                    // read command

                    string line = iReader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    line = line.ToUpperInvariant();

                    if (line.StartsWith("H") || line.StartsWith("*"))
                    {
                        iStack.Clear();

                        iBrowser.Goto(iBrowser.Home);

                        top = 1;
                    }
                    if (line.StartsWith("N") || line.StartsWith("+"))
                    {
                        if (top + kItemLines - 1 < iBrowser.Items.Count)
                        {
                            top += kItemLines;
                        }
                    }
                    if (line.StartsWith("P") || line.StartsWith("-"))
                    {
                        top -= kItemLines;

                        if (top < 1)
                        {
                            top = 1;
                        }
                    }
                    if (line.StartsWith("B") || line.StartsWith("/"))
                    {
                        int count = iStack.Count;

                        if (count > 0)
                        {
                            Position back = iStack[count - 1];

                            iStack.RemoveAt(count - 1);

                            iBrowser.Goto(back.Location);

                            top = back.Top;
                        }
                    }
                    else
                    {

                        uint value;

                        try
                        {
                            value = uint.Parse(line);
                        }
                        catch (FormatException)
                        {
                            continue;
                        }
                        catch (OverflowException)
                        {
                            continue;
                        }

                        if (value == 0)
                        {
                            continue;
                        }

                        if (value >= top && value < top + kItemLines)
                        {

                            ILocation next = iBrowser.ItemLocation(value - 1);

                            if (next != null)
                            {
                                iStack.Add(new Position(top, iBrowser.Location));

                                iBrowser.Goto(next);

                                top = 1;
                            }
                        }
                        else
                        {
                            ILocation next = iBrowser.ItemLocation(value - 1);

                            if (next != null)
                            {
                                uint nexttop = value - ((value - 1) % kItemLines);
                                top = (int)nexttop;
                            }
                        }
                    }
                }
                */

                sb.Append("</body></html>");

                byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
                iContext.Response.ContentLength64 = b.Length;
                iContext.Response.OutputStream.Write(b, 0, b.Length);
                iContext.Response.OutputStream.Close();
            }

            private string PackLocation(ILocation aLocation)
            {
                string result = aLocation.Id;

                foreach (string value in aLocation.Values)
                {
                    result += "+";
                    result += value;
                }
                return (result);
            }

            private ILocation UnpackLocation(string aLocation)
            {
                string trimmed = String.Empty;

                if (aLocation.Length > 0)
                {
                    trimmed = aLocation.Substring(1, aLocation.Length - 1);
                }

                string[] split = trimmed.Split(new char[] { '+' });

                if (split.Length == 0)
                {
                    return (iBrowser.Home);
                }

                Location location = new Location(split[0]);

                if (split.Length == 1)
                {
                    return (location);
                }

                for (int i = 1; i < split.Length; i++)
                {
                    location.AddValue(split[i]);
                }

                return (location);
            }
        }
    }
}

