#ifndef HEADER_STREAM
#define HEADER_STREAM

#include <Standard.h>
#include <ZappTypes.h>
#include <Buffer.h>

EXCEPTION(ReaderError);
EXCEPTION(WriterError);

namespace Zapp {

class IReader
{
public:
    virtual Brn Read(TUint aBytes) = 0;
    virtual Brn ReadUntil(TByte aSeparator) = 0;
    virtual void ReadFlush() = 0;
    virtual void ReadInterrupt() = 0;
    virtual ~IReader() {};
};

class IReaderSource
{
public:
    virtual void Read(Bwx& aBuffer) = 0;
    virtual void ReadFlush() = 0;
    virtual void ReadInterrupt() = 0;
    virtual ~IReaderSource() {};
};

class IWriter
{
public:
    virtual void Write(TByte aValue) = 0;
    virtual void Write(const Brx& aBuffer) = 0;
    virtual void WriteFlush() = 0;
    virtual ~IWriter() {};
};

class Sxx : public INonCopyable
{
    friend class Swp;
protected:
    Sxx(TUint aMaxBytes);
    virtual ~Sxx();
protected:
    virtual TByte* Ptr() = 0;
protected:
    TUint iMaxBytes;
    TUint iBytes;
};

class Srx : public Sxx, public IReader
{
public:
    virtual Brn Read(TUint aBytes);
    virtual Brn ReadUntil(TByte aSeparator);
    virtual void ReadFlush();
    virtual void ReadInterrupt();
    Brn Snaffle();
protected:
    Srx(TUint aMaxBytes, IReaderSource& aSource);
protected:
    IReaderSource& iSource;
    TUint iOffset;
};

template <TUint S> class Srs : public Srx
{
public:
    Srs(IReaderSource& aSource) : Srx(S, aSource) {}
private:
    virtual TByte* Ptr() { return (iBuf); }
private:
    TByte iBuf[S];
};

class Srd : public Srx
{
public:
    Srd(TUint aMaxBytes, IReaderSource& aSource);
    virtual ~Srd();
private:
    virtual TByte* Ptr();
private:
    TByte* iPtr;
};

class Swx : public Sxx, public IWriter
{
public:
    virtual void Write(TByte aValue);
    virtual void Write(const Brx& aBuffer);
    virtual void WriteFlush();
private:
    void WriteDrain();
protected:
    Swx(TUint aMaxBytes, IWriter& aWriter);
private:
    void Error();
    virtual TByte* Ptr() = 0;
protected:
    IWriter& iWriter;
};

template <TUint S> class Sws : public Swx
{
public:
    Sws(IWriter& aWriter) : Swx(S, aWriter) {}
private:
    virtual TByte* Ptr() { return (iBuf); }
private:
    TByte iBuf[S];
};

class Swd : public Swx
{
public:
    Swd(TUint aMaxBytes, IWriter& aWriter);
    virtual ~Swd();
private:
    virtual TByte* Ptr();
private:
    TByte* iPtr;
};

class Swp : public Swx
{
public:
    Swp(Srx& aHost, IWriter& aWriter);
    virtual ~Swp();
private:
    virtual TByte* Ptr();
private:
    Srx& iHost;
};

class ReaderBuffer : public IReader
{
public:
    ReaderBuffer();
    void Set(const Brx& aBuffer);
    TUint Bytes() const;
    Brn ReadRemaining();
    Brn ReadPartial(TUint aBytes);
    // IReader
    virtual Brn Read(TUint aBytes);
    virtual Brn ReadUntil(TByte aSeparator);
    virtual void ReadFlush();
    virtual void ReadInterrupt();
private:
    Brn iBuffer;
    TUint iOffset;
};

class WriterBuffer : public IWriter, public INonCopyable
{
public:
    WriterBuffer(Bwx& aBuffer);
    void Flush();
    // IWriter
    virtual void Write(TByte aValue);
    virtual void Write(const Brx& aBuffer);
    virtual void WriteFlush();
private:
    Bwx& iBuffer;
};

class WriterBwh : public IWriter
{
public:
    WriterBwh(TInt aGranularity);
    void TransferTo(Bwh& aDest);
    void TransferTo(Brh& aDest);
    void Write(const TChar* aBuffer);
    // IWriter
    void Write(TByte aValue);
    void Write(const Brx& aBuffer);
    void WriteFlush();
private:
    Bwh iBuf;
    TInt iGranularity;
};

} //namespace Zapp

#endif // HEADER_STREAM
