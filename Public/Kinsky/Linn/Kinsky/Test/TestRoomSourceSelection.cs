using Linn.TestFramework;
using Linn.Kinsky;
using Linn.Topology;
using System.Collections.Generic;
using System.Threading;
using Linn;

internal class SuiteRoomSourceSubscriptionTests : Suite
{   
    public SuiteRoomSourceSubscriptionTests() : base("Room/Source subscription tests") {
    }
    
    public override void Test() {
        RoomSourceSelection selection = new RoomSourceSelection();
        selection.EEventSelectionChanged += EventSelectionChanged;

        // test that no subscription occurs for room/source after 200ms
        selection.SetPendingRoom(-1, null);
        Thread.Sleep(100);
        TEST(iNumEventsReceived == 0);    // after 100ms no event should have occured
        Thread.Sleep(150);
        TEST(iNumEventsReceived == 0);    // after 250ms no event should have occured

        selection.SetPendingSource(-1, null, true);
        Thread.Sleep(100);
        TEST(iNumEventsReceived == 0);    // after 100ms no event should have occured
        Thread.Sleep(150);
        TEST(iNumEventsReceived == 0);    // after 250ms no event should have occured

        ModelRoom room = new ModelRoom("TestRoom");
        // test that a subscription occurs for room/source after 200ms
        selection.SetPendingRoom(0, room);
        Thread.Sleep(100);
        TEST(iNumEventsReceived == 0);    // after 100ms no event should have occured
        Thread.Sleep(200);
        TEST(iNumEventsReceived == 1);    // after 250ms 1 event should have occured
        iNumEventsReceived = 0;
        int roomIndex = -1;
        ModelRoom modelRoom = null;
        int sourceIndex = -1;
        ModelRoomSource modelRoomSource = null;
        selection.GetRoomAndSource(ref roomIndex, ref modelRoom, ref sourceIndex, ref modelRoomSource);
        TEST(roomIndex == 0);
        TEST(modelRoom == room);
        TEST(sourceIndex == -1);
        TEST(modelRoomSource == null);
        ModelRoomSource source = new ModelRoomSource();
        ModelDevice modelDevice = new ModelDevice("TestRoom", "TestSource", "Auxiliary");
        source.ModelAuxiliarySource = new ModelDeviceSourceAuxiliary(modelDevice);
        source.ModelAuxiliarySource.ModelRoom = room;
        selection.SetPendingSource(1, source, true);
        Thread.Sleep(100);
        TEST(iNumEventsReceived == 0);    // after 100ms no event should have occured
        Thread.Sleep(200);
        TEST(iNumEventsReceived == 1);    // after 250ms 1 event should have occured
        iNumEventsReceived = 0;
        selection.GetRoomAndSource(ref roomIndex, ref modelRoom, ref sourceIndex, ref modelRoomSource);
        TEST(roomIndex == 0);
        TEST(modelRoom == room);
        TEST(sourceIndex == 1);
        TEST(modelRoomSource == source);

        // unsubscribe from source
        selection.SetPendingSource(-1, null, true);
        Thread.Sleep(250);
        TEST(iNumEventsReceived == 1);    // after 250ms 1 event should have occured
        iNumEventsReceived = 0;
        selection.GetRoomAndSource(ref roomIndex, ref modelRoom, ref sourceIndex, ref modelRoomSource);
        TEST(roomIndex == 0);
        TEST(modelRoom == room);
        TEST(sourceIndex == -1);
        TEST(modelRoomSource == null);

        // unsubscribe from room when subscribed to room and source
        selection.SetPendingSource(1, source, true);
        Thread.Sleep(250);
        selection.SetPendingRoom(-1, null);
        Thread.Sleep(250);
        TEST(iNumEventsReceived == 2);    // after 500ms 2 events should have occured
        iNumEventsReceived = 0;
        selection.GetRoomAndSource(ref roomIndex, ref modelRoom, ref sourceIndex, ref modelRoomSource);
        TEST(roomIndex == -1);
        TEST(modelRoom == null);
        TEST(sourceIndex == -1);
        TEST(modelRoomSource == null);
    }

    private void EventSelectionChanged() {
        ++iNumEventsReceived;
    }

    private uint iNumEventsReceived = 0;
}

class TestProgram {
    public static void Main(string[] aArgs) {
        App app = new App(aArgs);
        app.Start();

        Runner runner = new Runner("RoomSourceSelection tests");
        runner.Add(new SuiteRoomSourceSubscriptionTests());
        runner.Run();
    }
}