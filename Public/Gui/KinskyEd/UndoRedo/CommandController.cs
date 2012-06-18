using Linn.Gui.Resources;

namespace Linn {
namespace Gui {
namespace Editor {
namespace UndoRedo {
    
public class CommandPeriodChange : ICommand
{
    public CommandPeriodChange(Monostable aMonostable, float aNewPeriod) {
        iMonostable = aMonostable;
        iNewPeriod = aNewPeriod;
        iOldPeriod = aMonostable.Period;
    }
    
    public void Commit() {
        SetPeriod(iNewPeriod);
    }
    
    public void Undo() {
        SetPeriod(iOldPeriod);
    }
    
    public void Redo() {
        SetPeriod(iNewPeriod);
    }
    
    private void SetPeriod(float aPeriod) {
        iMonostable.Period = aPeriod;
    }
    
    Monostable iMonostable = null;
    float iNewPeriod;
    float iOldPeriod;
}

public class CommandStateChange : ICommand
{
    public CommandStateChange(Bistable aBistable, bool aNewState) {
        iBistable = aBistable;
        iNewState = aNewState;
        iOldState = aBistable.State;
    }
    
    public void Commit() {
        SetState(iNewState);
    }
    
    public void Undo() {
        SetState(iOldState);
    }
    
    public void Redo() {
        SetState(iNewState);
    }
    
    private void SetState(bool aState) {
        iBistable.State = aState;
    }
    
    Bistable iBistable = null;
    bool iNewState;
    bool iOldState;
}

public class CommandCountsPerSecondChange : ICommand
{
    public CommandCountsPerSecondChange(Counter aCounter, float aNewCountsPerSecond) {
        iCounter = aCounter;
        iNewCountsPerSecond = aNewCountsPerSecond;
        iOldCountsPerSecond = aCounter.CountsPerSecond;
    }
    
    public void Commit() {
        SetCountsPerSecond(iNewCountsPerSecond);
    }
    
    public void Undo() {
        SetCountsPerSecond(iOldCountsPerSecond);
    }
    
    public void Redo() {
        SetCountsPerSecond(iNewCountsPerSecond);
    }
    
    private void SetCountsPerSecond(float aCountsPerSecond) {
        iCounter.CountsPerSecond = aCountsPerSecond;
    }
    
    Counter iCounter = null;
    float iNewCountsPerSecond;
    float iOldCountsPerSecond;
}

public class CommandLoopChange : ICommand
{
    public CommandLoopChange(Counter aCounter, bool aNewLoop) {
        iCounter = aCounter;
        iNewLoop = aNewLoop;
        iOldLoop = aCounter.Loop;
    }
    
    public void Commit() {
        SetLoop(iNewLoop);
    }
    
    public void Undo() {
        SetLoop(iOldLoop);
    }
    
    public void Redo() {
        SetLoop(iNewLoop);
    }
    
    private void SetLoop(bool aLoop) {
        iCounter.Loop = aLoop;
    }
    
    Counter iCounter = null;
    bool iNewLoop;
    bool iOldLoop;
}

public class CommandMaxCountChange : ICommand
{
    public CommandMaxCountChange(Counter aCounter, int aNewMaxCount) {
        iCounter = aCounter;
        iNewMaxCount = aNewMaxCount;
        iOldMaxCount = aCounter.MaxCount;
    }
    
    public void Commit() {
        SetMaxCount(iNewMaxCount);
    }
    
    public void Undo() {
        SetMaxCount(iOldMaxCount);
    }
    
    public void Redo() {
        SetMaxCount(iNewMaxCount);
    }
    
    private void SetMaxCount(int aMaxCount) {
        iCounter.MaxCount = aMaxCount;
    }
    
    Counter iCounter = null;
    int iNewMaxCount;
    int iOldMaxCount;
}

public class CommandCountChange : ICommand
{
    public CommandCountChange(Counter aCounter, int aNewCount) {
        iCounter = aCounter;
        iNewCount = aNewCount;
        iOldCount = aCounter.Count;
    }
    
    public void Commit() {
        SetCount(iNewCount);
    }
    
    public void Undo() {
        SetCount(iOldCount);
    }
    
    public void Redo() {
        SetCount(iNewCount);
    }
    
    private void SetCount(int aCount) {
        iCounter.Count = aCount;
    }
    
    Counter iCounter = null;
    int iNewCount;
    int iOldCount;
}

public class CommandCountingChange : ICommand
{
    public CommandCountingChange(Counter aCounter, bool aNewCounting) {
        iCounter = aCounter;
        iNewCounting = aNewCounting;
        iOldCounting = aCounter.Counting;
    }
    
    public void Commit() {
        SetCounting(iNewCounting);
    }
    
    public void Undo() {
        SetCounting(iOldCounting);
    }
    
    public void Redo() {
        SetCounting(iNewCounting);
    }
    
    private void SetCounting(bool aCounting) {
        if(aCounting) {
            iCounter.Start();
        } else {
            iCounter.Stop();
        }
    }
    
    Counter iCounter = null;
    bool iNewCounting;
    bool iOldCounting;
}
    
} // UndoRedo
} // Editor
} // Gui
} // Linn
