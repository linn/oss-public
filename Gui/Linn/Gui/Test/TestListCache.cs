using Linn.TestFramework;
using Linn.Gui.Scenegraph;
using System;
using System.Collections.Generic;

namespace Linn {
namespace Gui {

internal class ListableTest : IListable
{
    public ListableTest(uint aIndex) {
        iIndex = aIndex;
    }
    
    public void Dispose() {}
    
    public uint Index {
        get {
            return iIndex;
        }
    }
    
    public void Highlight() {}
    public void UnHighlight() {}
    public void Select() {}
    public void UnSelect() {}
    public int State {
        get { return 0; }
        set {}
    }
    public NodeHit NodeHit {
        get { return null; }
        set {}
    }
    
    private uint iIndex = 0;
}

internal class ListEntryProviderTest : IListEntryProvider
{
    public void Dispose() {}
    public void SetList(IList aList) {}
    
    public uint Count {
        get {
            return iCount;
        }
        set {
            iCount = value;
        }
    }
    
    public List<IListable> Entries(uint aStartIndex, uint aCount) {
        iEntriesCallCount++;
        iReadStartIndex = aStartIndex;
        iReadCount = aCount;
        List<IListable> result = new List<IListable>();
        for(uint i = 0; i < aCount; ++i) {
            result.Add(new ListableTest(aStartIndex + i));
        }
        return result;
    }
    
    public uint EntryCallCount {
        get {
            return iEntryCallCount;
        }
    }
    
    public uint EntriesCallCount {
        get {
            return iEntriesCallCount;
        }
    }
    
    public uint ReadStartIndex {
        get {
            return iReadStartIndex;
        }
    }
    
    public uint ReadCount {
        get {
            return iReadCount;
        }
    }
    
    private uint iCount = 0;
    private uint iEntryCallCount = 0;
    private uint iEntriesCallCount = 0;
    private uint iReadStartIndex = 0;
    private uint iReadCount = 0;
}
    
internal class SuiteEntryTests : Suite
{
    public SuiteEntryTests() : base("Entry tests") {
    }
    
    public override void Test() {
        ListEntryProviderTest provider = new ListEntryProviderTest();
        ListCache cache = new ListCache(provider, 6);
        
        // test that when we have nothing in a list cache does not call into the list
        TEST_THROWS(typeof(AssertionError), cache, "Entry", (uint)0);
        TEST(provider.EntryCallCount == 0);
        TEST(provider.EntriesCallCount == 0);
        
        provider.Count = 1;
        List<IListable> listables = cache.Entries(0, 1);
        ListableTest listable = listables[0] as ListableTest;
        TEST(listable.Index == 0);
        TEST(provider.EntryCallCount == 0);
        TEST(provider.EntriesCallCount == 1);   // we have 1 cache miss
        TEST(provider.ReadStartIndex == 0);
        TEST(provider.ReadCount == 1);
        
        provider.Count = 10;
        List<IListable> result = cache.Entries(2, 6);
        TEST(provider.EntryCallCount == 0);
        TEST(provider.EntriesCallCount == 4);   // we have 3 cache misses
        TEST(provider.ReadStartIndex == 2);
        TEST(provider.ReadCount == 6);
        for(int i = 0; i < 6; ++i) {
            listable = result[i] as ListableTest;
            TEST(listable.Index == 2+i);
        }
        result = cache.Entries(0, 10);
        TEST(provider.EntryCallCount == 0);
        TEST(provider.EntriesCallCount == 9);   // we have 5 cache misses
    }
}

class Program {
    public static void Main(string[] aArgs) {
        Helper helper = new Helper(aArgs);
        helper.ProcessCommandLine();
        
        Runner runner = new Runner("ListCache tests");
        runner.Add(new SuiteEntryTests());
        runner.Run();
        
        helper.Dispose();
    }
}

} // Gui
} // Linn
