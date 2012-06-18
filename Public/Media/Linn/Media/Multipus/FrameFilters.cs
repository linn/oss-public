using System;

namespace Linn.Media.Multipus
{
    public interface IFrameErrorHandler
    {
        void NotMpus(Frame aFrame);
        void LateOrDuplicate(Frame aFrame);
        void Missing(long aLastGoodTimestampNs, long aNextGoodTimestampns, uint aMissingFrames);
    }

    public class FrameErrorHandlerDummy : IFrameErrorHandler
    {
        public void NotMpus(Frame aFrame)
        {
        }
        public void LateOrDuplicate(Frame aFrame)
        {
        }
        public void Missing(long aLastGoodTimestampNs, long aNextGoodTimestampns, uint aMissingFrames)
        {
        }
    }
    
public class FrameFilter
{
    public FrameFilter(IFrameReader aReader)
        : this(aReader, null)
    {
    }
     
	public FrameFilter(IFrameReader aReader, IFrameErrorHandler aError)
	{
        iReader = aReader;
        if (aError == null)
        {
            aError = new FrameErrorHandlerDummy();
        }
        iError = aError;
	}

    public Frame Read()
    {
        while (true)
        {
            Frame frame = iReader.Read();
            if (!frame.IsMpus)
            {
                iError.NotMpus(frame);
            }
            else if (iFrame == null)
            {
                Console.WriteLine("Inited at frame: {0}", frame.FrameNumber);
                iFrame = frame;
                return frame;
            }
            else if (frame.FrameNumber <= iFrame.FrameNumber)
            {
                iError.LateOrDuplicate(frame);
            }
            else if (frame.FrameNumber > iFrame.FrameNumber + 1)
            {
                uint missed = (frame.FrameNumber - iFrame.FrameNumber + 1);
                iError.Missing(iFrame.TimeStampNs, frame.TimeStampNs, missed);
                iFrame = frame;
                return frame;
            }
            else
            {
                Assert.Check(frame.FrameNumber == iFrame.FrameNumber + 1);
                iFrame = frame;
                return frame;
            }

        }
    }

    private IFrameReader iReader;
    private Frame iFrame = null;
    private IFrameErrorHandler iError;
}

}