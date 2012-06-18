using System;
using System.Collections.Generic;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using SneakyMedia.Database;

namespace SneakyMedia.Browse
{
    public class ModuleBrowseTelnet : ModuleBrowse
    {
        public const int kPort = 50001;
        public const int kSlots = 10;

        public ModuleBrowseTelnet()
            : base("Telnet", new Version(1, 0, 0, 0))
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
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, kPort));
            server.Listen(kSlots);

            while (true)
            {
                Socket session = server.Accept();
                new Session(Engine, session);
            }
        }

        internal class Session
        {
            private IEngine iEngine;
            private Socket iSocket;
            private StreamReader iReader;
            private StreamWriter iWriter;
            private IBrowser iBrowser;
            private List<Position> iStack;

            private const int kInputLines = 5;
            private const int kItemLines = 20;
            private const int kNumberWidth = 6;

            private const string kVt100ClearScreen = "\x001b[2J";
            private const string kVt100CursorHome = "\x001b[H";
            private const string kVt100AttributesOff = "\x001b[m";
            private const string kVt100AttributesReverseVideo = "\x001b[7m";
            private const string kVt100CursorUp = "\x001b[A";
            private const string kVt100CursorRight = "\x001b[C";

            private const string kHelpLine = "[     ] Enter Number or H = Home, B = Back, N = Next, P = Previous";

            class Position
            {
                public Position(int aTop, ILocation aLocation)
                {
                    iTop = aTop;
                    iLocation = aLocation;
                }

                public int Top
                {
                    get
                    {
                        return (iTop);
                    }
                }

                public ILocation Location
                {
                    get
                    {
                        return (iLocation);
                    }
                }

                private int iTop;
                private ILocation iLocation;
            }

            public Session(IEngine aEngine, Socket aSocket)
            {
                iEngine = aEngine;
                iSocket = aSocket;
                iReader = new StreamReader(new NetworkStream(iSocket, false));
                iWriter = new StreamWriter(new NetworkStream(iSocket, true));
                iWriter.AutoFlush = true;
                iBrowser = new Browser(iEngine, "Telnet");
                iStack = new List<Position>();
                new Thread(Process).Start();
            }

            private void SetMaximum(ref int aTarget, int aValue)
            {
                if (aTarget < aValue)
                {
                    aTarget = aValue;
                }
            }

            private string Blanks(int aValue)
            {
                return (new string(' ', aValue));
            }

            private void Process()
            {
                try
                {
                    int index;

                    int top = 1;

                    while (true)
                    {
                        iWriter.Write(kVt100ClearScreen);
                        iWriter.Write(kVt100CursorHome);

                        int totalwidth = kNumberWidth;

                        // analyse inputs

                        int inputwidth = 0;

                        foreach (IPageInput input in iBrowser.Inputs)
                        {
                            SetMaximum(ref inputwidth, input.Name.Length);
                        }

                        foreach (IPageInput input in iBrowser.Inputs)
                        {
                            SetMaximum(ref totalwidth, input.Value.Length + inputwidth + 3);
                        }

                        // analyse output - find width of each column

                        int[] widths = new int[iBrowser.Outputs.Count];

                        // clear widths

                        for (uint i = 0; i < widths.Length; i++)
                        {
                            widths[i] = 0;
                        }

                        // find width of each column from item data

                        foreach (IList<string> row in iBrowser.Items)
                        {
                            index = 0;

                            foreach (string item in row)
                            {
                                SetMaximum(ref widths[index++], item.Length);
                            }
                        }

                        // increase width of any column that is currently narrower than its column heading

                        index = 0;

                        foreach (IPageOutput output in iBrowser.Outputs)
                        {
                            SetMaximum(ref widths[index++], output.Name.Length);
                        }

                        // find width of an item line

                        int itemswidth = 0;

                        for (uint i = 0; i < widths.Length; i++)
                        {
                            itemswidth += widths[i] + 1;
                        }

                        // adjust total width to items width

                        SetMaximum(ref totalwidth, itemswidth);

                        // adjust total width to title width

                        SetMaximum(ref totalwidth, iBrowser.Name.Length + 1);

                        // adjust total width to help line width

                        SetMaximum(ref totalwidth, kHelpLine.Length + 1);

                        // create heading

                        string heading = "#";

                        string numbergap = new string(' ', kNumberWidth);

                        heading += Blanks(kNumberWidth);

                        index = 0;

                        foreach (IPageOutput output in iBrowser.Outputs)
                        {
                            heading += output.Name;
                            heading += Blanks(widths[index++] - output.Name.Length + 1);
                        }

                        // adjust total width to heading width

                        SetMaximum(ref totalwidth, heading.Length);

                        // display title

                        int titleremaining = totalwidth - iBrowser.Name.Length;

                        if (titleremaining % 2 > 0)
                        {
                            totalwidth++;
                            titleremaining++;
                        }

                        iWriter.Write(kVt100AttributesReverseVideo);
                        iWriter.WriteLine(Blanks(totalwidth));
                        iWriter.Write(Blanks(titleremaining / 2));
                        iWriter.Write(iBrowser.Name);
                        iWriter.WriteLine(Blanks(titleremaining / 2));
                        iWriter.WriteLine(Blanks(totalwidth));
                        iWriter.Write(kVt100AttributesOff);

                        // display blank line

                        iWriter.WriteLine(String.Empty);

                        // display input values

                        int lines = kInputLines;

                        foreach (IPageInput input in iBrowser.Inputs)
                        {
                            iWriter.Write(input.Name);
                            iWriter.Write(Blanks(inputwidth - input.Name.Length));
                            iWriter.WriteLine(": " + input.Value);

                            lines--;
                        }

                        while (lines-- > 0)
                        {
                            iWriter.WriteLine(String.Empty);
                        }

                        // display blank line

                        iWriter.WriteLine(String.Empty);

                        // display heading

                        iWriter.Write(kVt100AttributesReverseVideo);
                        iWriter.Write(heading);
                        iWriter.WriteLine(Blanks(totalwidth - heading.Length));
                        iWriter.Write(kVt100AttributesOff);

                        // display blank line

                        iWriter.WriteLine(String.Empty);

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
                }
                catch (IOException)
                {
                }

                iSocket.Close();
            }
        }
    }
}

