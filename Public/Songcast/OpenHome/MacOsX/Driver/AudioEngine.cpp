#include "AudioEngine.h"
#include <IOKit/audio/IOAudioStream.h>
#include <IOKit/IOLib.h>



static const uint32_t BLOCKS = 200;


// implementation of the AudioEngine class
OSDefineMetaClassAndStructors(AudioEngine, IOAudioEngine);


bool AudioEngine::init(OSDictionary* aProperties)
{
    IOLog("Songcast AudioEngine[%p]::init(%p) ...\n", this, aProperties);

    if (!IOAudioEngine::init(aProperties)) {
        IOLog("Songcast AudioEngine[%p]::init(%p) base class init failed\n", this, aProperties);
        return false;
    }

    iCurrentFormat = &(Songcast::SupportedFormats[0]);

    iCurrentBlock = 0;
    
    iTimer = 0;
    iTimeZero = 0;
    iTimerFiredCount = 0;
    iTimestamp = 0;
    iAudioStopping = false;
    iTimerIntervalNs = iCurrentFormat->TimeNs();

    // allocate the output buffers
    iBuffer = new BlockBuffer(BLOCKS, iCurrentFormat->SampleCount, iCurrentFormat->Channels, iCurrentFormat->BitDepth);

    if (!iBuffer || !iBuffer->Ptr()) {
        IOLog("Songcast AudioEngine[%p]::init(%p) buffer alloc failed\n", this, aProperties);
        if (iBuffer) {
            delete iBuffer;
            iBuffer = 0;
        }
        return false;
    }

    IOLog("Songcast AudioEngine[%p]::init(%p) ok\n", this, aProperties);
    return true;
}


bool AudioEngine::initHardware(IOService* aProvider)
{
    IOLog("Songcast AudioEngine[%p]::initHardware(%p) ...\n", this, aProvider);

    // base class initialisation
    if (!IOAudioEngine::initHardware(aProvider)) {
        IOLog("Songcast AudioEngine[%p]::initHardware(%p) base class init failed\n", this, aProvider);
        return false;
    }
    
    setNumSampleFramesPerBuffer(BLOCKS * iCurrentFormat->SampleCount);

    IOAudioSampleRate sampleRate;
    sampleRate.whole = iCurrentFormat->SampleRate;
    sampleRate.fraction = 0;
    setSampleRate(&sampleRate);

    // create output stream
    IOAudioStream* outStream = new IOAudioStream;
    if (!outStream) {
        IOLog("Songcast AudioEngine[%p]::initHardware(%p) failed to alloc stream\n", this, aProvider);
        return false;
    }

    if (!outStream->initWithAudioEngine(this, kIOAudioStreamDirectionOutput, 1)) {
        IOLog("Songcast AudioEngine[%p]::initHardware(%p) failed to init stream\n", this, aProvider);
        outStream->release();
        return false;
    }


    // initialise audio format for the stream
    IOAudioStreamFormat format;
    format.fNumChannels = iCurrentFormat->Channels;
    format.fSampleFormat = kIOAudioStreamSampleFormatLinearPCM;
    format.fNumericRepresentation = kIOAudioStreamNumericRepresentationSignedInt;
    format.fBitDepth = iCurrentFormat->BitDepth;
    format.fBitWidth = iCurrentFormat->BitDepth;
    format.fAlignment = kIOAudioStreamAlignmentHighByte;
    format.fByteOrder = kIOAudioStreamByteOrderBigEndian;
    format.fIsMixable = 1;
    format.fDriverTag = 0;

    outStream->addAvailableFormat(&format, &sampleRate, &sampleRate);
    outStream->setSampleBuffer(iBuffer->Ptr(), iBuffer->Bytes());

    if (outStream->setFormat(&format) != kIOReturnSuccess) {
        IOLog("Songcast AudioEngine[%p]::initHardware(%p) failed to set stream format\n", this, aProvider);
        outStream->release();
        return false;
    }

    if (addAudioStream(outStream) != kIOReturnSuccess) {
        IOLog("Songcast AudioEngine[%p]::initHardware(%p) failed to add stream\n", this, aProvider);
        outStream->release();
        return false;
    }

    // stream can be released as the addAudioStream will retain it
    outStream->release();


    // create the timer
    IOWorkLoop* workLoop = getWorkLoop();
    if (!workLoop) {
        IOLog("Songcast AudioEngine[%p]::initHardware(%p) failed to get work loop\n", this, aProvider);
        return false;
    }

    iTimer = IOTimerEventSource::timerEventSource(this, TimerFired);
    if (!iTimer) {
        IOLog("Songcast AudioEngine[%p]::initHardware(%p) failed to create timer\n", this, aProvider);
        return false;
    }

    if (workLoop->addEventSource(iTimer) != kIOReturnSuccess) {
        IOLog("Songcast AudioEngine[%p]::initHardware(%p) failed to add timer\n", this, aProvider);
        return false;
    }

    IOLog("Songcast AudioEngine[%p]::initHardware(%p) ok\n", this, aProvider);
    return true;
}


void AudioEngine::free()
{
    IOLog("Songcast AudioEngine[%p]::free()\n", this);

    if (iBuffer) {
        delete iBuffer;
        iBuffer = 0;
    }

    IOAudioEngine::free();
}


void AudioEngine::SetSongcast(Songcast& aSongcast)
{
    iSongcast = &aSongcast;
}


void AudioEngine::SetDescription(const char* aDescription)
{
    setDescription(aDescription);
}


void AudioEngine::stop(IOService* aProvider)
{
    IOLog("Songcast AudioEngine[%p]::stop(%p)\n", this, aProvider);
    IOAudioEngine::stop(aProvider);
    
    // remove timer
    iTimer->cancelTimeout();
    IOWorkLoop* workLoop = getWorkLoop();
    if (workLoop) {
        workLoop->removeEventSource(iTimer);
    }
    iTimer->release();
    iTimer = 0;
}


IOReturn AudioEngine::performAudioEngineStart()
{
    takeTimeStamp(false);
    iCurrentBlock = 0;

    uint64_t currTime;
    clock_get_uptime(&currTime);
    absolutetime_to_nanoseconds(currTime, &iTimestamp);

    iTimeZero = iTimestamp;
    iTimerFiredCount = 0;
    iAudioStopping = false;

    if (iTimer->setTimeout(iTimerIntervalNs) != kIOReturnSuccess) {
        IOLog("Songcast AudioEngine[%p]::performAudioEngineStart() failed to start timer\n", this);
        return kIOReturnError;
    }
    
    IOLog("Songcast AudioEngine[%p]::performAudioEngineStart() ok\n", this);
    return kIOReturnSuccess;
}


IOReturn AudioEngine::performAudioEngineStop()
{
    // set flag that audio is stopping - let the timer method handle cleanup
    iAudioStopping = true;

    IOLog("Songcast AudioEngine[%p]::performAudioEngineStop()\n", this);
    return kIOReturnSuccess;
}


UInt32 AudioEngine::getCurrentSampleFrame()
{
    return iCurrentBlock * iCurrentFormat->SampleCount;
}


IOReturn AudioEngine::performFormatChange(IOAudioStream* aAudioStream, const IOAudioStreamFormat* aNewFormat, const IOAudioSampleRate* aNewSampleRate)
{
    IOLog("Songcast AudioEngine[%p]::performFormatChange()", this);

    if (aNewFormat) {
        IOLog(" Format(%u, %u, %u, %u, %u, %u, %u, %u, %u)", (uint32_t)aNewFormat->fNumChannels, (uint32_t)aNewFormat->fSampleFormat, (uint32_t)aNewFormat->fNumericRepresentation,
                                                             (uint32_t)aNewFormat->fBitDepth, (uint32_t)aNewFormat->fBitWidth, (uint32_t)aNewFormat->fAlignment,
                                                             (uint32_t)aNewFormat->fByteOrder, (uint32_t)aNewFormat->fIsMixable, (uint32_t)aNewFormat->fDriverTag);
    }
    else {
        IOLog(" Format()");
    }

    if (aNewSampleRate) {
        IOLog(" SampleRate(%u, %u)\n", (uint32_t)aNewSampleRate->whole, (uint32_t)aNewSampleRate->fraction);
    }
    else {
        IOLog(" SampleRate()\n");
    }

    return kIOReturnSuccess;
}


extern IOReturn AudioEngineClipOutputSamples(const void* aMixBuffer, void* aSampleBuffer, UInt32 aFirstSampleFrame, UInt32 aNumSampleFrames, UInt32 aNumChannels, UInt32 aBitDepth);


IOReturn AudioEngine::clipOutputSamples(const void* aMixBuffer, void* aSampleBuffer, UInt32 aFirstSampleFrame, UInt32 aNumSampleFrames, const IOAudioStreamFormat* aFormat, IOAudioStream* aStream)
{
    return AudioEngineClipOutputSamples(aMixBuffer, aSampleBuffer, aFirstSampleFrame, aNumSampleFrames, aFormat->fNumChannels, aFormat->fBitWidth);
}


void AudioEngine::TimerFired(OSObject* aOwner, IOTimerEventSource* aSender)
{
    if (aOwner)
    {
        AudioEngine* engine = OSDynamicCast(AudioEngine, aOwner);
        if (engine) {
            engine->TimerFired();
        }
    }
}


void AudioEngine::TimerFired()
{
    // increment the timer fired count - this is used to calculate accurate timer
    // intervals for when the timer is next scheduled
    iTimerFiredCount++;

    if (!iAudioStopping)
    {
        // calculate the absolute time when the next timer should fire - we calculate
        // this based on the following:
        //  - the absolute iTimeZero (which is the origin of time for this session)
        //  - the expected timer interval (based on the sample rate of the audio and the number of samples to send)
        //  - the number of times that the timer has currently fired
        uint64_t timeOfNextFire = iTimeZero + (iTimerIntervalNs * (iTimerFiredCount + 1));

        // calculate the interval for the next timer and schedule it - this is simply based on the
        // difference between the expected time of the next fire and the current time
        uint64_t currTimeAbs, currTimeNs;
        clock_get_uptime(&currTimeAbs);
        absolutetime_to_nanoseconds(currTimeAbs, &currTimeNs);

        uint32_t interval = (timeOfNextFire > currTimeNs) ? (uint32_t)(timeOfNextFire - currTimeNs) : 0;
        iTimer->setTimeout(interval);
    }    

    // gather the audio data to send
    uint64_t timestamp = iTimestamp;
    bool halt = iAudioStopping;
    void* data = iBuffer->BlockPtr(iCurrentBlock);
    uint32_t bytes = iBuffer->BlockBytes();

    // increment counters and send timestamp to the upper audio layers if the buffer
    // wraps
    iCurrentBlock++;
    if (iCurrentBlock >= iBuffer->Blocks()) {
        iCurrentBlock = 0;
        takeTimeStamp();
    }

    // get the timestamp to use for the next audio packet
    uint64_t currTimeAbs;
    clock_get_uptime(&currTimeAbs);
    absolutetime_to_nanoseconds(currTimeAbs, &iTimestamp);

    // send the data
    iSongcast->Send(*iCurrentFormat, timestamp, halt, data, bytes);
}



// implementation of BlockBuffer
BlockBuffer::BlockBuffer(uint32_t aBlocks, uint32_t aBlockSamples, uint32_t aChannels, uint32_t aBitDepth)
: iPtr(0)
, iBytes(aBlocks * aBlockSamples * aChannels * aBitDepth / 8)
, iBlockBytes(aBlockSamples * aChannels * aBitDepth / 8)
, iBlocks(aBlocks)
{
    iPtr = IOMalloc(iBytes);
}


BlockBuffer::~BlockBuffer()
{
    if (iPtr) {
        IOFree(iPtr, iBytes);
    }
}


