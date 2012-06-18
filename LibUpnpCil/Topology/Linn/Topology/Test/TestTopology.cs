using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

using Linn.Topology;
using Linn.Topology.Layer1;

public class TestTopology
{
  //[STAThread]
    static void Main(string[] aArgs)
    {
        IHelper helper = new Helper(aArgs);

        OptionParser optParser = helper.OptionParser;

        optParser.Usage =  "usage: TestTopology [options]\n";

        //OptionString optType = new OptionString("-t", "--type", "", "Type of test", "TYPE");

        //optParser.AddOption(optType);

        helper.ProcessCommandLine();

        if (!optParser.HelpSpecified())
        {
            //string type = optType.Value;

            try
            {
                Test();
            }
            catch (ApplicationException)
            {
            }
        }

        helper.Dispose();
    }

    private static void Test()
    {
        // Create test environment

        Linn.Topology.Layer1.TestStack stack = new Linn.Topology.Layer1.TestStack();
        House house = new House(stack);

        house.EventRoomAdded += RoomAdded;
        house.EventRoomRemoved += RoomRemoved;

        TestPreamp preamp = new TestPreamp("Preamp");

        List<TestSource> list1 = new List<TestSource>();
        list1.Add(new TestSource("Source0", "Aux", true));
        list1.Add(new TestSource("Source1", "Aux", true));
        list1.Add(new TestSource("Source2", "Aux", true));
        TestGroup group1 = new TestGroup("Kitchen", "One", false, null, list1);

        Test("Create Kitchen");

        stack.Add(group1);
        CheckEvent("R+ Kitchen");
        CheckEvent("S+ Kitchen One (Source0)");
        CheckEvent("S+ Kitchen One (Source1)");
        CheckEvent("S+ Kitchen One (Source2)");
        CheckEvent("C* Kitchen One (Source0)");
        CheckComplete();

        Test("Switch Source");

        group1.UpdateCurrentSource(1);
        CheckEvent("C* Kitchen One (Source1)");
        CheckComplete();

        group1.UpdateCurrentSource(2);
        CheckEvent("C* Kitchen One (Source2)");
        CheckComplete();

        group1.UpdateCurrentSource(0);
        CheckEvent("C* Kitchen One (Source0)");
        CheckComplete();

        Test("Change to not visible");

        group1.UpdateSourceVisible(0, false);
        CheckEvent("S- Kitchen One (Source0)");
        CheckComplete();

        Test("Switch source while one source is not visible");

        group1.UpdateCurrentSource(1);
        CheckEvent("C* Kitchen One (Source1)");
        CheckComplete();

        group1.UpdateCurrentSource(2);
        CheckEvent("C* Kitchen One (Source2)");
        CheckComplete();

        group1.UpdateCurrentSource(0);
        CheckEvent("C* Kitchen One (Source0)");
        CheckComplete();

        Test("Change to visible");

        group1.UpdateSourceVisible(0, true);
        CheckEvent("S+ Kitchen One (Source0)");
        CheckComplete();

        Test("Further visibility changes");
        group1.UpdateSourceVisible(1, false);
        group1.UpdateSourceVisible(2, false);
        group1.UpdateSourceVisible(1, true);
        group1.UpdateSourceVisible(2, true);
        CheckEvent("S- Kitchen One (Source1)");
        CheckEvent("S- Kitchen One (Source2)");
        CheckEvent("S+ Kitchen One (Source1)");
        CheckEvent("S+ Kitchen One (Source2)");
        CheckComplete();

        Test("Change name");
        group1.UpdateSourceName(0, "SourceZero");
        CheckEvent("S- Kitchen One (Source0)");
        CheckEvent("S+ Kitchen One (SourceZero)");
        CheckComplete();

        Test("Change name while not visible");
        group1.UpdateSourceVisible(1, false);
        group1.UpdateSourceName(1, "SourceOne");
        CheckEvent("S- Kitchen One (Source1)");
        CheckComplete();
        group1.UpdateSourceVisible(1, true);
        CheckEvent("S+ Kitchen One (SourceOne)");
        CheckComplete();

        Test("Remove Kitchen");
        stack.Remove(group1);
        CheckEvent("C* Kitchen NONE");
        CheckEvent("S- Kitchen One (SourceZero)");
        CheckEvent("S- Kitchen One (SourceOne)");
        CheckEvent("S- Kitchen One (Source2)");
        CheckEvent("R- Kitchen");
        CheckComplete();

        group1.UpdateSourceName(0, "Source0");
        group1.UpdateSourceName(1, "Source1");

        Test("Re-add Kitchen");

        stack.Add(group1);
        CheckEvent("R+ Kitchen");
        CheckEvent("S+ Kitchen One (Source0)");
        CheckEvent("S+ Kitchen One (Source1)");
        CheckEvent("S+ Kitchen One (Source2)");
        CheckEvent("C* Kitchen One (Source0)");
        CheckComplete();

        List<TestSource> list2 = new List<TestSource>();
        list2.Add(new TestSource("Preamp0", "Aux", true));
        list2.Add(new TestSource("Preamp1", "Aux", true));
        list2.Add(new TestSource("Preamp2", "Aux", true));
        TestGroup group2 = new TestGroup("Kitchen", "Two", false, preamp, list2);

        Test("Add second group to Kitchen");

        stack.Add(group2);

        CheckEvent("S+ Kitchen Two (Preamp0)");
        CheckEvent("S+ Kitchen Two (Preamp1)");
        CheckEvent("S+ Kitchen Two (Preamp2)");
        CheckComplete();

        Test("Switch Source with two groups");

        group2.UpdateCurrentSource(1);
        CheckEvent("P* Kitchen Preamp");
        CheckEvent("C* Kitchen Two (Preamp1)");
        CheckComplete();

        group2.UpdateCurrentSource(2);
        CheckEvent("C* Kitchen Two (Preamp2)");
        CheckComplete();

        group2.UpdateCurrentSource(0);
        CheckEvent("C* Kitchen Two (Preamp0)");
        CheckComplete();

        group1.UpdateCurrentSource(1);
        CheckEvent("P* Kitchen NONE");
        CheckEvent("C* Kitchen One (Source1)");
        CheckComplete();

        group1.UpdateCurrentSource(2);
        CheckEvent("C* Kitchen One (Source2)");
        CheckComplete();

        group1.UpdateCurrentSource(0);
        CheckEvent("C* Kitchen One (Source0)");
        CheckComplete();

        Room kitchen = house.RoomList[0];
        Source source0 = kitchen.SourceList[0];
        Source source1 = kitchen.SourceList[1];
        Source source2 = kitchen.SourceList[2];
        Source source3 = kitchen.SourceList[3];
        Source source4 = kitchen.SourceList[4];
        Source source5 = kitchen.SourceList[5];

        Check("Source 0 Name", source0.FullName == "One (Source0)");
        Check("Source 1 Name", source1.FullName == "One (Source1)");
        Check("Source 2 Name", source2.FullName == "One (Source2)");
        Check("Source 3 Name", source3.FullName == "Two (Preamp0)");
        Check("Source 4 Name", source4.FullName == "Two (Preamp1)");
        Check("Source 5 Name", source5.FullName == "Two (Preamp2)");

        Test("Check selection of two independent groups");

        source3.Select();
        CheckEvent("P* Kitchen Preamp");
        CheckEvent("C* Kitchen Two (Preamp0)");
        CheckComplete();

        source1.Select();
        CheckEvent("P* Kitchen NONE");
        CheckEvent("C* Kitchen One (Source1)");
        CheckComplete();

        source4.Select();
        CheckEvent("P* Kitchen Preamp");
        CheckEvent("C* Kitchen Two (Preamp1)");
        CheckComplete();

        source2.Select();
        CheckEvent("P* Kitchen NONE");
        CheckEvent("C* Kitchen One (Source2)");
        CheckComplete();

        source5.Select();
        CheckEvent("P* Kitchen Preamp");
        CheckEvent("C* Kitchen Two (Preamp2)");
        CheckComplete();

        source0.Select();
        CheckEvent("P* Kitchen NONE");
        CheckEvent("C* Kitchen One (Source0)");
        CheckComplete();

        Test("Create link between groups through name change");
        group2.UpdateSourceName(0, "One");

        CheckEvent("S- Kitchen Two (Preamp0)");
        CheckEvent("P* Kitchen Preamp");
        CheckComplete();

        Test("Break link between groups through name change");
        group2.UpdateSourceName(0, "Preamp0");

        CheckEvent("P* Kitchen NONE");
        CheckEvent("S+ Kitchen Two (Preamp0)");
        CheckComplete();

        Test("Create link between groups through name change onto non-current source");
        group2.UpdateSourceName(1, "One");

        CheckEvent("S- Kitchen Two (Preamp1)");
        CheckEvent("P* Kitchen Preamp");
        CheckEvent("C* Kitchen Two (Preamp0)");
        CheckComplete();

        Test("Break link again");
        group2.UpdateSourceName(1, "Preamp1");

        CheckEvent("S+ Kitchen Two (Preamp1)");
        CheckComplete();

        Test("Make new link");
        group2.UpdateSourceName(2, "One");

        CheckEvent("S- Kitchen Two (Preamp2)");
        CheckComplete();

        Test("And break link yet again");
        group2.UpdateSourceName(2, "Preamp2");

        CheckEvent("S+ Kitchen Two (Preamp2)");
        CheckComplete();

        Test("Remove both groups");
        stack.Remove(group1);
        stack.Remove(group2);
        CheckEvent("S- Kitchen One (Source0)");
        CheckEvent("S- Kitchen One (Source1)");
        CheckEvent("S- Kitchen One (Source2)");
        CheckEvent("P* Kitchen NONE");
        CheckEvent("C* Kitchen NONE");
        CheckEvent("S- Kitchen Two (Preamp0)");
        CheckEvent("S- Kitchen Two (Preamp1)");
        CheckEvent("S- Kitchen Two (Preamp2)");
        CheckEvent("R- Kitchen");
        CheckComplete();

        // Create Cara 1 Sneaky Music DS

        List<TestSource> sneakylist1 = new List<TestSource>();
        sneakylist1.Add(new TestSource("Playlist", "Playlist", true));
        sneakylist1.Add(new TestSource("UpnpAv", "UpnpAv", true));
        TestGroup sneakygroup1 = new TestGroup("Main", "Sneaky Music DS", false, null, sneakylist1);

        List<TestSource> sneakylist2 = new List<TestSource>();
        sneakylist2.Add(new TestSource("Sneaky Music DS", "Aux", false));
        TestGroup sneakygroup2 = new TestGroup("Main", "Sneaky Music DS", false, preamp, sneakylist2);

        Test("Create Main with Sneaky Music DS");

        stack.Add(sneakygroup1);
        stack.Add(sneakygroup2);
        CheckEvent("R+ Main");
        CheckEvent("S+ Main Sneaky Music DS (Playlist)");
        CheckEvent("S+ Main Sneaky Music DS (UpnpAv)");
        CheckEvent("C* Main Sneaky Music DS (Playlist)");
        CheckEvent("P* Main Preamp");
        CheckComplete();

        Test("Remove Main with Sneaky Music DS");

        stack.Remove(sneakygroup1);
        stack.Remove(sneakygroup2);
        CheckEvent("C* Main Sneaky Music DS (Sneaky Music DS)");
        CheckEvent("S- Main Sneaky Music DS (Playlist)");
        CheckEvent("S- Main Sneaky Music DS (UpnpAv)");
        CheckEvent("P* Main NONE");
        CheckEvent("C* Main NONE");
        CheckEvent("R- Main");
        CheckComplete();

        Test("Create Main with Sneaky Music DS (reverse order)");

        stack.Add(sneakygroup2);
        stack.Add(sneakygroup1);
        CheckEvent("R+ Main");
        CheckEvent("P* Main Preamp");
        CheckEvent("C* Main Sneaky Music DS (Sneaky Music DS)");
        CheckEvent("S+ Main Sneaky Music DS (Playlist)");
        CheckEvent("S+ Main Sneaky Music DS (UpnpAv)");
        CheckEvent("C* Main Sneaky Music DS (Playlist)");
        CheckComplete();

        Test("Remove Main with Sneaky Music DS (reverse order)");

        stack.Remove(sneakygroup2);
        stack.Remove(sneakygroup1);
        CheckEvent("P* Main NONE");
        CheckEvent("C* Main NONE");
        CheckEvent("S- Main Sneaky Music DS (Playlist)");
        CheckEvent("S- Main Sneaky Music DS (UpnpAv)");
        CheckEvent("R- Main");
        CheckComplete();


        // Create Cara 1 Majik System

        List<TestSource> majiklist1 = new List<TestSource>();
        majiklist1.Add(new TestSource("Playlist", "Playlist", true));
        majiklist1.Add(new TestSource("UpnpAv", "UpnpAv", true));
        TestGroup majikgroup1 = new TestGroup("Main", "Majik DS", false, null, majiklist1);

        List<TestSource> majiklist2 = new List<TestSource>();
        majiklist2.Add(new TestSource("Analog 1", "Analog", true));
        majiklist2.Add(new TestSource("Majik DS", "Analog", true));
        majiklist2.Add(new TestSource("Analog 3", "Analog", true));
        majiklist2.Add(new TestSource("Analog 4", "Analog", true));
        majiklist2.Add(new TestSource("Analog 5", "Analog", true));
        majiklist2.Add(new TestSource("Analog 6", "Analog", true));
        majiklist2.Add(new TestSource("Knekt", "Knekt", true));
        TestGroup majikgroup2 = new TestGroup("Main", "Majik Kontrol", false, preamp, majiklist2);

        Test("Create Main with Majik System on Analog 3");

        majikgroup2.UpdateCurrentSource(2);

        stack.Add(majikgroup1);

        CheckEvent("R+ Main");
        CheckEvent("S+ Main Majik DS (Playlist)");
        CheckEvent("S+ Main Majik DS (UpnpAv)");
        CheckEvent("C* Main Majik DS (Playlist)");

        stack.Add(majikgroup2);
        CheckEvent("S+ Main Majik Kontrol (Analog 1)");
        CheckEvent("P* Main Preamp");
        CheckEvent("C* Main Majik Kontrol (Analog 3)");
        CheckEvent("S+ Main Majik Kontrol (Analog 3)");
        CheckEvent("S+ Main Majik Kontrol (Analog 4)");
        CheckEvent("S+ Main Majik Kontrol (Analog 5)");
        CheckEvent("S+ Main Majik Kontrol (Analog 6)");
        CheckEvent("S+ Main Majik Kontrol (Knekt)");
        CheckComplete();

        Test("Remove Main with Majik System on Analog 3");

        stack.Remove(majikgroup1);
        stack.Remove(majikgroup2);
        CheckEvent("S+ Main Majik Kontrol (Majik DS)");
        CheckEvent("S- Main Majik DS (Playlist)");
        CheckEvent("S- Main Majik DS (UpnpAv)");
        CheckEvent("P* Main NONE");
        CheckEvent("C* Main NONE");
        CheckEvent("S- Main Majik Kontrol (Analog 1)");
        CheckEvent("S- Main Majik Kontrol (Majik DS)");
        CheckEvent("S- Main Majik Kontrol (Analog 3)");
        CheckEvent("S- Main Majik Kontrol (Analog 4)");
        CheckEvent("S- Main Majik Kontrol (Analog 5)");
        CheckEvent("S- Main Majik Kontrol (Analog 6)");
        CheckEvent("S- Main Majik Kontrol (Knekt)");
        CheckEvent("R- Main");
        CheckComplete();

        Test("Create Main with Majik System on Analog 2");

        majikgroup2.UpdateCurrentSource(1);

        stack.Add(majikgroup1);
        stack.Add(majikgroup2);
        CheckEvent("R+ Main");
        CheckEvent("S+ Main Majik DS (Playlist)");
        CheckEvent("S+ Main Majik DS (UpnpAv)");
        CheckEvent("C* Main Majik DS (Playlist)");
        CheckEvent("S+ Main Majik Kontrol (Analog 1)");
        CheckEvent("P* Main Preamp");
        CheckEvent("S+ Main Majik Kontrol (Analog 3)");
        CheckEvent("S+ Main Majik Kontrol (Analog 4)");
        CheckEvent("S+ Main Majik Kontrol (Analog 5)");
        CheckEvent("S+ Main Majik Kontrol (Analog 6)");
        CheckEvent("S+ Main Majik Kontrol (Knekt)");
        CheckComplete();

        Test("Remove Main with Majik System on Analog 2");

        stack.Remove(majikgroup1);
        stack.Remove(majikgroup2);
        CheckEvent("C* Main Majik Kontrol (Majik DS)");
        CheckEvent("S+ Main Majik Kontrol (Majik DS)");
        CheckEvent("S- Main Majik DS (Playlist)");
        CheckEvent("S- Main Majik DS (UpnpAv)");
        CheckEvent("P* Main NONE");
        CheckEvent("C* Main NONE");
        CheckEvent("S- Main Majik Kontrol (Analog 1)");
        CheckEvent("S- Main Majik Kontrol (Majik DS)");
        CheckEvent("S- Main Majik Kontrol (Analog 3)");
        CheckEvent("S- Main Majik Kontrol (Analog 4)");
        CheckEvent("S- Main Majik Kontrol (Analog 5)");
        CheckEvent("S- Main Majik Kontrol (Analog 6)");
        CheckEvent("S- Main Majik Kontrol (Knekt)");
        CheckEvent("R- Main");
        CheckComplete();
    }

    private static void RoomAdded(object obj, EventArgsRoom e)
    {
        e.Room.EventSourceAdded += SourceAdded;
        e.Room.EventSourceRemoved += SourceRemoved;
        e.Room.EventCurrentChanged += CurrentChanged;
        e.Room.EventPreampChanged += PreampChanged;
        AddEvent("R+ " + e.Room.Name);
    }

    private static void RoomRemoved(object obj, EventArgsRoom e)
    {
        e.Room.EventSourceAdded -= SourceAdded;
        e.Room.EventSourceRemoved -= SourceRemoved;
        e.Room.EventCurrentChanged -= CurrentChanged;
        e.Room.EventPreampChanged -= PreampChanged;
        AddEvent("R- " + e.Room.Name);
    }

    private static void SourceAdded(object obj, Linn.Topology.EventArgsSource e)
    {
		Source source = e.Source as Source;
        AddEvent("S+ " + source.Room.Name + " " + source.FullName);
    }

    private static void SourceRemoved(object obj, Linn.Topology.EventArgsSource e)
    {
		Source source = e.Source as Source;
        AddEvent("S- " + source.Room.Name + " " + source.FullName);
    }

    private static void CurrentChanged(object obj, EventArgs e)
    {
        Room room = obj as Room;

        string source = "NONE";

        if (room.Current != null)
        {
            source = room.Current.FullName;
        }

        AddEvent("C* " + room.Name + " " + source);
    }

    private static void PreampChanged(object obj, EventArgs e)
    {
        Room room = obj as Room;

        string preamp = "NONE";

        if (room.Preamp != null)
        {
            preamp = room.Preamp.Type;
        }

        AddEvent("P* " + room.Name + " " + preamp);
    }

    private static void AddEvent(string aEvent)
    {
        Events.Add(aEvent);
    }

    private static void Test(string aDescription)
    {
        Console.WriteLine("TEST : " + aDescription);
    }

    private static void Check(string aDescription, bool aCheck)
    {
        Console.WriteLine("CHECK: " + aDescription);

        if (!aCheck)
        {
            Console.WriteLine("FAILS: Check failed");
            throw (new ApplicationException());
        }
    }

    private static void CheckEvent(string aEvent)
    {
        Console.WriteLine("CHECK: " + aEvent);

        if (Events.Count == 0)
        {
            Console.WriteLine("FAILS: No events found");
            throw (new ApplicationException());
        }

        string check = Events[0];
        Events.RemoveAt(0);

        if (check != aEvent)
        {
            Console.WriteLine("EVENT: " + check);
            Console.WriteLine("FAILS: Event does not match");
            throw (new ApplicationException());
        }
    }

    private static void CheckComplete()
    {
        Console.WriteLine("CHECK: Complete");

        if (Events.Count != 0)
        {
            string check = Events[0];
            Events.RemoveAt(0);
            Console.WriteLine("EVENT: " + check);
            Console.WriteLine("FAILS: Pending events");
            throw (new ApplicationException());
        }
    }


    private static List<string> Events = new List<string>();
}

namespace Linn.Topology.Layer1
{
    public class TestGroup : IGroup
    {
        public TestGroup(string aRoom, string aName, bool aStandby, IPreamp aPreamp, List<TestSource> aSourceList)
        {
            iRoom = aRoom;
            iName = aName;
            iStandby = aStandby;
            iPreamp = aPreamp;
            iSourceList = aSourceList;

            iMutex = new Mutex();
        }
		
		public bool Standby
		{
			get
			{
				return iStandby;
			}
		}

        public string Room
        {
            get
            {
                return (iRoom);
            }
        }

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        public IPreamp Preamp
        {
            get
            {
                return (iPreamp);
            }
        }

        public uint CurrentSource
        {
            get
            {
                return (iCurrentSource);
            }
        }

        private void Lock()
        {
            iMutex.WaitOne();
        }

        private void Unlock()
        {
            iMutex.ReleaseMutex();
        }

        public void UpdateCurrentSource(uint aIndex)
        {
            Lock();

            if (iCurrentSource != aIndex)
            {
                iCurrentSource = aIndex;

                Unlock();

                if (EventCurrentSourceChanged != null)
                {
                    EventCurrentSourceChanged(this, EventArgs.Empty);
                }
            }
            else
            {
                Unlock();
            }
        }

        public void UpdateStandby(bool aStandby)
        {
            Lock();

            if (iStandby != aStandby)
            {
                iStandby = aStandby;

                Unlock();

                if (EventStandbyChanged != null)
                {
                    EventStandbyChanged(this, EventArgs.Empty);
                }
            }
            else
            {
                Unlock();
            }
        }

        public void UpdateSourceName(uint aIndex, string aName)
        {
            TestSource source = iSourceList[(int)aIndex];
            TestSource update = new TestSource(aName, source.Type, source.Visible);
            iSourceList[(int)aIndex] = update;
            UpdateSource(aIndex);
        }

        public void UpdateSourceVisible(uint aIndex, bool aVisible)
        {
            TestSource source = iSourceList[(int)aIndex];
            TestSource update = new TestSource(source.Name, source.Type, aVisible);
            iSourceList[(int)aIndex] = update;
            UpdateSource(aIndex);
        }

        private void UpdateSource(uint aIndex)
        {
            if (EventSourceChanged != null)
            {
                EventSourceChanged(this, new EventArgsSource(aIndex));
            }
        }

        public void SetStandby(bool aValue)
        {
        }

        public void Select(uint aIndex)
        {
        }

        public uint SourceCount
        {
            get
            {
                return ((uint)iSourceList.Count);
            }
        }

        public ISource Source(uint aIndex)
        {
            return (iSourceList[(int)aIndex]);
        }

        private string iRoom;
        private string iName;
        private bool iStandby;

        private uint iCurrentSource;

        private IPreamp iPreamp;
        private List<TestSource> iSourceList;

        private Mutex iMutex;

        public event EventHandler<EventArgs> EventPreampChanged;
        public event EventHandler<EventArgs> EventCurrentSourceChanged;
        public event EventHandler<EventArgs> EventStandbyChanged;
        public event EventHandler<EventArgsSource> EventSourceChanged;
    }

    public class TestPreamp : IPreamp
    {
        public TestPreamp(string aType)
        {
            iType = aType;
        }

        public string Type
        {
            get
            {
                return (iType);
            }
        }

        public Device Device
        {
            get
            {
                return (null);
            }
        }

        private string iType;
    }

    public class TestSource : ISource
    {
        public TestSource(string aName, string aType, bool aVisible)
        {
            iName = aName;
            iType = aType;
            iVisible = aVisible;
        }

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        public string Type
        {
            get
            {
                return (iType);
            }
        }

        public bool Visible
        {
            get
            {
                return (iVisible);
            }
        }

        public Device Device
        {
            get
            {
                return (null);
            }
        }

        internal void SetName(string aName)
        {
            iName = aName;
        }

        internal void SetVisible(bool aVisible)
        {
            iVisible = aVisible;
        }

        private string iName;
        private string iType;
        private bool iVisible;
    }

    public class TestStack : Linn.Topology.Layer1.IStack
    {
            public event EventHandler<EventArgsGroup> EventGroupAdded;
            public event EventHandler<EventArgsGroup> EventGroupRemoved;

            public TestStack()
            {
            }

            public void Start(IPAddress aInterface)
            {
            }

            public void Stop()
            {
            }

            public void Rescan()
            {
            }

            public void Add(TestGroup aGroup)
            {
                if (EventGroupAdded != null)
                {
                    EventGroupAdded(this, new EventArgsGroup(aGroup));
                }
            }

            public void Remove(TestGroup aGroup)
            {
                if (EventGroupRemoved != null)
                {
                    EventGroupRemoved(this, new EventArgsGroup(aGroup));
                }
            }

            public IEventUpnpProvider EventServer
            {
                get
                {
                    return (null);
                }
            }
    }
}