#include <TestFramework.h>
#include <Thread.h>
#include <Os.h>

using namespace Zapp;
using namespace Zapp::TestFramework;

const TUint32 kSleepMs = 25; // short sleep used as a lazy way of avoiding too many dependencies on thread priorities

class TestPipe
{
public:
    TestPipe();
    void Write(TChar aChar);
    TBool Read(TChar aChar);
    TBool IsEmpty() const;
private:
    static const TUint kMaxPipeEntries = 2;
    TChar iPipe[kMaxPipeEntries];
    TUint iCount;
    Mutex iLock;
};

TestPipe::TestPipe()
: iCount(0), iLock("")
{}

void TestPipe::Write(TChar aChar)
{
    iLock.Wait();
    ASSERT(iCount < kMaxPipeEntries);
    iPipe[iCount++] = aChar;
    iLock.Signal();
}

TBool TestPipe::Read(TChar aChar)
{
    TBool matches;
    iLock.Wait();
    ASSERT(iCount != 0);
    iCount--;
    matches = (aChar==iPipe[iCount]);
    iLock.Signal();
    return matches;
}

TBool TestPipe::IsEmpty() const
{
    TBool empty;
    const_cast<Mutex&>(iLock).Wait();
    empty = (iCount==0);
    const_cast<Mutex&>(iLock).Signal();
    return empty;
}


TestPipe gPipe;

class TestThread : public Thread
{
public:
    TestThread(const TChar* aName, TChar aChar, TUint aPriority = kPriorityNormal);
    void Run();
    THandle Handle() const { return iHandle; }
private:
    TChar iChar;
};

TestThread::TestThread(const TChar* aName, TChar aChar, TUint aPriority)
: Thread(aName, aPriority), iChar(aChar)
{
}

void TestThread::Run()
{
    while(1) {
        Wait();
        gPipe.Write(iChar);
    }
}

class SuiteStartStop : public Suite
{
public:
    SuiteStartStop() : Suite("Starting and Stopping threads") {}
    void Test();
};

void SuiteStartStop::Test()
{
    Thread* threadA = new TestThread("THDA", 'A');
    Thread* threadB = new TestThread("THDB", 'B');

    TEST( ((Thread&) *threadA) == ((Thread&) *threadA) );
    TEST( ((Thread&) *threadA) != ((Thread&) *threadB) );

    threadA->Start();
    threadB->Start();

    TUint i=0;
    for(; i< 100; i++) {
        TEST(gPipe.IsEmpty());
        threadA->Signal();
        // use sleeps to avoid dependency on thread priorities being supported
        Thread::Sleep(kSleepMs);
        TEST(gPipe.Read('A'));

        threadB->Signal();
        Thread::Sleep(kSleepMs);
        TEST(gPipe.Read('B'));

        threadA->Signal();
        Thread::Sleep(kSleepMs);
        TEST(gPipe.Read('A'));
    }
    TEST(gPipe.IsEmpty());

    delete threadA;
    delete threadB;
}

class SuitePriority : public Suite
{
public:
    SuitePriority() : Suite("Thread Priority") {}
    void Test();
};

void SuitePriority::Test()
{
    Thread* threadA = new TestThread("THDA", 'A', kPriorityLow);
    Thread* threadB = new TestThread("THDB", 'B', kPriorityLow + kPriorityLess);
    TEST(gPipe.IsEmpty());

    threadA->Start();
    threadB->Start();
    threadA->Signal();
    threadB->Signal();
    TEST(gPipe.IsEmpty());
    Thread::Sleep(kSleepMs);
    TEST(gPipe.Read('A')); //thread A ran first
    TEST(gPipe.Read('B')); //thread B ran second
    TEST(gPipe.IsEmpty());
    delete threadA;
    delete threadB;

    threadA = new TestThread("THDA", 'A', kPriorityLowest);
    threadB = new TestThread("THDB", 'B', kPriorityLowest+1);
    TEST(gPipe.IsEmpty());

    threadA->Signal();
    threadB->Signal();
    Thread::Sleep(kSleepMs);
    TEST(gPipe.Read('B')); //thread B ran first
    TEST(gPipe.Read('A')); //thread A ran second
    TEST(gPipe.IsEmpty());
    delete threadA;
    delete threadB;
}


class SuiteTimeslice: public Suite
{
public:
    SuiteTimeslice() : Suite("Thread Timeslicing") {}
    void Test();
};

// Sorry for the bulk, I went a bit overboard on the Timeslicing stuff.
// Basically, the test suite launches a monitor thread, which in turn
// launches counter threads and monitors their progress.
//
// There are two test cases - one tests for equal progress made by both
// counter threads, and the other creates a higher priority thread to
// preempt the counters. The monitor thread then tests for zero-progress
// in the second counter thread.

const TUint kThreadCounterMaxCount      = 20000000;
const TUint kThreadDisruptorPeriodMs    = 100;
const TUint kThreadMonitorPeriodMs      = 1000;
const TUint kSuitePriority              = 150;

// <ThreadCounter>
class ThreadCounter : public Thread
{
public:
    ThreadCounter( TUint aPriority, TUint aCeiling )
        : Thread( "CounterThread", aPriority ),
        iHasFinished(false),
        iCount(0),
        iOldCount(0),
        iCeiling(aCeiling)
        {};

    void Run();
    TBool HasMadeProgress();
    TBool HasFinished()          { return iHasFinished; };
private:
    TBool   iHasFinished;
    TUint   iCount;
    TUint   iOldCount;
    TUint   iCeiling;
};

void ThreadCounter::Run()
{
    for ( iCount = 0 ; iCount < iCeiling ; iCount++ )
    {
        if ( iCount == 0 )              // Forces optimisation to make iCount available.
        {
            Print( "" );
        }
    }

    iHasFinished = true;
    Wait();
}

TBool ThreadCounter::HasMadeProgress()
{
    TBool hasMadeProgress = ( iCount > iOldCount );
    iOldCount = iCount;
    return hasMadeProgress;
}
// </ThreadCounter>

// <ThreadDisruptor> This Thread periodcally preempts all lower priority threads
class ThreadDisruptor : public Thread
{
public:
    ThreadDisruptor( TUint aPriority, TUint aPeriodMs )
        : Thread( "DisruptorThread", aPriority ),
        iPeriodMs( aPeriodMs )
        {}
    void Run();
private:
    TUint iPeriodMs;
};

void ThreadDisruptor::Run()
{
    while(1)
    {
        Sleep( iPeriodMs );
        CheckForKill();
    }
}
// </ThreadDisruptor>


// <ThreadMonitor> This class creates two ThreadCounters and monitors their progress.
class ThreadMonitor : public Thread
{
public:
    ThreadMonitor( TUint aPriority, TUint aPeriodMs, TBool aUseDisruptor, Thread& aCaller )
        : Thread( "MonitorThread", aPriority ),
        iUseDisruptor( aUseDisruptor ),
        iPeriodMs( aPeriodMs ),
        iCaller( aCaller ),
        iPriority(aPriority)
        {}

    void Run();
private:
    TBool               iUseDisruptor;
    TUint               iPeriodMs;
    Thread&             iCaller;
    TUint               iPriority;
};

void ThreadMonitor::Run()
{
    TUint testCounter = 0;

    ThreadDisruptor *threadDisruptor = NULL;
    ThreadCounter   *threadCounter1 = new ThreadCounter( iPriority-2, kThreadCounterMaxCount );        // Create counters
    ThreadCounter   *threadCounter2 = new ThreadCounter( iPriority-2, kThreadCounterMaxCount );

    if ( iUseDisruptor )
    {
        threadDisruptor = new ThreadDisruptor( iPriority-1, kThreadDisruptorPeriodMs );
        threadDisruptor->Start();                              // Start disruptor thread if required.
    }

    threadCounter1->Start();                                   // Start counters
    threadCounter2->Start();

    Sleep( iPeriodMs );                                         // Do initial sleep()

    while( (testCounter++ < 4) && (!threadCounter1->HasFinished()) )
    {
        if ( ! iUseDisruptor )
        {
            TEST( threadCounter1->HasMadeProgress() );         // If normal mode, test for progress in both threads
            TEST( threadCounter2->HasMadeProgress() );
        }
        else
        {
            TEST(   threadCounter1->HasMadeProgress() );       // else test for zero-progress in second thread
            TEST( ! threadCounter2->HasMadeProgress() );
        }

        Sleep( iPeriodMs );
    }

    if ( iUseDisruptor )
    {
        delete threadDisruptor;
    }

    threadCounter1->Signal();                                  // Allow conter threads to terminate normally.
    threadCounter2->Signal();

    delete threadCounter1;
    delete threadCounter2;

    iCaller.Signal();
}


class TimesliceTestThread : public Thread
{
public:
    TimesliceTestThread(Semaphore& aSem) : Thread("TSLT", kSuitePriority), iSem(aSem) {}
    void Run();
private:
    Semaphore& iSem;
};

void TimesliceTestThread::Run()
{
    // Starting Normal Timeslicing Conditions ...
    ThreadMonitor *tm1 = new ThreadMonitor( (kSuitePriority-1), kThreadMonitorPeriodMs, false, *Thread::Current() );
    tm1->Start();
    Thread::Current()->Wait();

    // Start Heavy Preempt Conidtions ...
    Print( "\nTesting for the reset of quantum on thread preemption - see TRAC #257\n" );
    ThreadMonitor *tm2 = new ThreadMonitor( (kSuitePriority-1), kThreadMonitorPeriodMs, true, *Thread::Current() );
    tm2->Start();
    Thread::Current()->Wait();
    iSem.Signal();
}

void SuiteTimeslice::Test()
{
    Semaphore sem("TSLS", 0);
    TimesliceTestThread* th = new TimesliceTestThread(sem);
    th->Start();
    sem.Wait();
    delete th;
}

class SuiteSemaphore : public Suite
{
public:
    SuiteSemaphore() : Suite("Semaphore") {}
    void Test();
};

class SemaphoreThread : public Thread
{
public:
    SemaphoreThread(Semaphore& aSem) : Thread("SEMT", kPriorityVeryLow), iSem(aSem), iWaits(0) {}
    void Run();
    TInt Waits() const { return iWaits; }
private:
    Semaphore& iSem;
    TInt iWaits;
};

void SemaphoreThread::Run()
{
    iSem.Wait();
    iWaits++;
    iSem.Wait();
    iWaits++;
    iSem.Wait();
    iWaits++;
    iSem.Wait();
    iWaits++;
    iSem.Wait();
}

class SignalThread : public Thread
{
public:
    SignalThread(Semaphore& aOwner) : Thread("SEM2"), iOwner(aOwner), iSelf("STHS", 0) {}
    Semaphore& Sem() { return iSelf; }
    void Run();
private:
    Semaphore& iOwner;
    Semaphore  iSelf;
};

void SignalThread::Run()
{
    while (1) {
        iSelf.Wait();
        iOwner.Signal();
        CheckForKill();
    }
}

void SuiteSemaphore::Test()
{
    // basic test - stack Semaphore
    Semaphore sem1("TMP1", 1);
    sem1.Wait();
    TEST_THROWS(sem1.Wait(1), Timeout);
    TEST(sem1.Clear() == false);
    sem1.Signal();
    TEST(sem1.Clear() == true);
    sem1.Signal();

    // basic test - heap Semaphore
    Semaphore* sem2 = new Semaphore("TMP2", 1);
    sem2->Wait();
    TEST(sem2->Clear() == false);
    sem2->Signal();
    TEST(sem2->Clear() == true);
    delete sem2;

    // check that another thread will wait on signal
    // ...and that multiple calls to Signal() complete multiple Wait()s
    Semaphore sem3("TMP3", 0);
    SemaphoreThread* semTh = new SemaphoreThread(sem3);
    semTh->Start();
    TEST(semTh->Waits() == 0);
    sem3.Signal();
    Thread::Current()->Sleep(kSleepMs);
    TEST(semTh->Waits() == 1);
    sem3.Signal();
    sem3.Signal();
    sem3.Signal();
    Thread::Current()->Sleep(kSleepMs);
    TEST(semTh->Waits() == 4);
    sem3.Signal();
    delete semTh;

    // test thread safety of Semaphore
    Semaphore sem4("TMP4", 0);
    static const int kNumSignalThreads = 10;
    SignalThread* th[kNumSignalThreads];
    int i, j;
    for (i=0; i<kNumSignalThreads; i++) {
        th[i] = new SignalThread(sem4);
        th[i]->Start();
    }
    for (i=0; i<1000; i++) {
        for (j=0; j<kNumSignalThreads; j++) {
            th[j]->Sem().Signal();
        }
        for (j=0; j<kNumSignalThreads; j++) {
            try {
                sem4.Wait(500);
            }
            catch(Timeout&) {
                TEST(1==0);
            }
        }
        TEST(!sem4.Clear());
    }
    for (i=0; i<kNumSignalThreads; i++) {
        th[i]->Kill();
        th[i]->Sem().Signal();
        delete th[i];
    }
}


class SuiteMutex : public Suite
{
public:
    SuiteMutex() : Suite("Mutexes") {}
    void Test();
};

class MutexThread : public Thread
{
public:
    MutexThread(Mutex& aMutex, TInt& aCount) : Thread("MUTT"), iMutex(aMutex), iCount(aCount) {}
    void Run();
private:
    Mutex& iMutex;
    TInt& iCount;
};

void MutexThread::Run()
{
    iCount++;
    iMutex.Wait();
    iCount++;
    iMutex.Signal();
}

void SuiteMutex::Test()
{
    Mutex mutex("MUT1");
    mutex.Wait();
    mutex.Signal();

    // check we can delete a locked mutex
    Mutex* mutex2 = new Mutex("MUT2");
    mutex2->Wait();
    delete mutex2;

    // check that another thread will block waiting on this mutex
    TInt count = 0;
    mutex.Wait();
    MutexThread* mutexTh = new MutexThread(mutex, count);
    mutexTh->Start();
    Thread::Current()->Sleep(kSleepMs);
    TEST(count == 1);
    mutex.Signal();
    Thread::Current()->Sleep(kSleepMs);
    TEST(count == 2);
    delete mutexTh;

    // check we can't nest locks within a thread
    Mutex* mutex3 = new Mutex("MUT3");
    mutex3->Wait();
    TEST_THROWS(mutex3->Wait(), AssertionFailed);
    delete mutex3;
}

class SuitePerformance : public Suite
{
public:
    SuitePerformance() : Suite("Performance") {}
    void Test();
};

class ThreadPerformance : public Thread
{
public:
    ThreadPerformance(TUint aPriority=kPriorityNormal);
    void Run();
    void Signal() { iSem.Signal(); }
private:
    Semaphore iSem;
};

ThreadPerformance::ThreadPerformance(TUint aPriority)
    : Thread("PERF", aPriority), iSem("", 0)
{
}

void ThreadPerformance::Run()
{
    iSem.Wait();
}

void SuitePerformance::Test()
{
    const TUint kNumThreads = 60;
    ThreadPerformance* threads[kNumThreads];
    TUint i=0;

    TUint start = TimeStart();
    for(i = 0; i<kNumThreads; i++) {
        threads[i] = new ThreadPerformance(kPriorityLow);
    }
    TUint time = TimeStop(start);
    TEST(time < 50);

    start = TimeStart();
    for(i=0; i<kNumThreads; i++) {
        threads[i]->Start();
    }
    time = TimeStop(start);
    TEST(time < 21);

    // Let the threads die
    for(i = 0; i<kNumThreads; i++) {
        threads[i]->Signal();
    }
    Zapp::Thread::Sleep(50);
}


class SuiteAutoMutex : public Suite
{
public:
    SuiteAutoMutex() : Suite("AutoMutex Functionality") {}
    void Test();
    void Hold(Mutex& aMutex);
};

void SuiteAutoMutex::Hold(Mutex& aMutex)
{
    AutoMutex a(aMutex);
}

void SuiteAutoMutex::Test()
{
    Mutex mA("muta");
    Mutex mB("mutb");

    Hold(mA);
    mA.Wait();

    {
        AutoMutex amB(mB);
    }
    mB.Wait();
}


class ThreadKillable : public Thread
{
public:
    ThreadKillable(Semaphore& aSem) : Thread("KILL"), iSem(aSem) {}
    void Run();
private:
    Semaphore& iSem;
};

void ThreadKillable::Run()
{
    try {
        TEST(TryWait() == false);
        Signal();
        TEST(TryWait() == true);
        CheckForKill();
        Kill();
        TEST_THROWS(CheckForKill(), ThreadKill);
        TEST_THROWS(Wait(), ThreadKill);
        TEST_THROWS(TryWait(), ThreadKill);
    }
    catch(ThreadKill&) {
        ASSERT(0);
    }
    iSem.Signal();
}

class SuiteThreadKill : public Suite
{
public:
    SuiteThreadKill() : Suite("Thread kill") {}
    void Test();
};

void SuiteThreadKill::Test()
{
    Semaphore sem("", 0);
    Thread* th = new ThreadKillable(sem);
    th->Start();
    sem.Wait();
    delete th;
}


class MainTestThread : public Thread
{
public:
    MainTestThread() : Thread("MAIN") {}
    void Run();
};

void MainTestThread::Run()
{
    Runner runner("Threading System");
    runner.Add(new SuiteSemaphore());
    runner.Add(new SuiteMutex());
    runner.Add(new SuiteAutoMutex());
    runner.Add(new SuiteStartStop());
    runner.Add(new SuitePerformance());
    runner.Add(new SuiteThreadKill());
    if (Zapp::Thread::SupportsPriorities())
    {
        runner.Add(new SuitePriority());
        runner.Add(new SuiteTimeslice());
    }
    runner.Run();

    Thread::Current()->Signal();
}

void Zapp::TestFramework::Runner::Main(TInt /*aArgc*/, TChar* /*aArgv*/[], InitialisationParams* aInitParams)
{
    UpnpLibrary::InitialiseMinimal(aInitParams);

    // many tests rely on Thread::Current() so run all tests in a thread we create
    MainTestThread th;
    th.Start();
    th.Wait();

    UpnpLibrary::Close();
}
