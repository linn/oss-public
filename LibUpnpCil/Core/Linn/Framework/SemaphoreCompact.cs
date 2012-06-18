using System.Threading;

namespace Linn
{
    public class Semaphore
    {
        public Semaphore(uint aCount, uint aMaxCount)
        {
            Assert.Check(aCount <= aMaxCount);

            iMaxCount = aMaxCount;
            iCount = aCount;
            iWaiters = 0;

            iMutex = new Mutex(false);
            iEvent = new ManualResetEvent(true);
        }

        public void WaitOne()
        {
            while (true)
            {
                iMutex.WaitOne();

                if (iCount > 0)
                {
                    iCount--;

                    iMutex.ReleaseMutex();
                    
                    return;
                }

                if (iWaiters == 0)
                {
                    iEvent.Reset();
                }

                iWaiters++;

                iMutex.ReleaseMutex();

                iEvent.WaitOne();
            }
        }

        public void Release()
        {
            iMutex.WaitOne();

            iCount++;

            Assert.Check(iCount <= iMaxCount);

            if (iWaiters > 0)
            {
                iEvent.Set();
                iWaiters = 0;
            }

            iMutex.ReleaseMutex();
        }

        private Mutex iMutex;
        private ManualResetEvent iEvent;

        private uint iMaxCount;
        private uint iWaiters;
        private uint iCount;
    }
}