#include <Ascii.h>
#include <Parser.h>
#include <Http.h>
#include <Debug.h>

using namespace Zapp;

// Http Methods

const Brn Http::kMethodOptions("OPTIONS");
const Brn Http::kMethodGet("GET");
const Brn Http::kMethodHead("HEAD");
const Brn Http::kMethodPost("POST");
const Brn Http::kMethodPut("PUT");
const Brn Http::kMethodDelete("DELETE");
const Brn Http::kMethodTrace("TRACE");
const Brn Http::kMethodConnect("CONNECT");
const Brn Http::kMethodExtension("EXTENSION");

// Http Headers

const Brn Http::kHeaderCacheControl("Cache-Control");
const Brn Http::kHeaderConnection("Connection");
const Brn Http::kHeaderDate("Date");
const Brn Http::kHeaderPragma("Pragma");
const Brn Http::kHeaderTrailer("Trailer");
const Brn Http::kHeaderTransferEncoding("Transfer-Encoding");
const Brn Http::kHeaderUpgrade("Upgrade");
const Brn Http::kHeaderVia("Via");
const Brn Http::kHeaderWarning("Warning");
const Brn Http::kHeaderAccept("Accept");
const Brn Http::kHeaderAcceptCharset("Accept-Charset");
const Brn Http::kHeaderAcceptEncoding("Accept-Encoding");
const Brn Http::kHeaderAcceptLanguage("Accept-Language");
const Brn Http::kHeaderAuthorization("Authorization");
const Brn Http::kHeaderExpect("Expect");
const Brn Http::kHeaderFrom("From");
const Brn Http::kHeaderHost("Host");
const Brn Http::kHeaderIfMatch("If-Match");
const Brn Http::kHeaderIfModifiedSince("If-Modified-Since");
const Brn Http::kHeaderIfNoneMatch("If-None-Match");
const Brn Http::kHeaderIfRange("If-Range");
const Brn Http::kHeaderIfUnmodifiedSince("If-Unmodified-Since");
const Brn Http::kHeaderMaxForwards("Max-Forwards");
const Brn Http::kHeaderProxyAuthorization("Proxy-Authorization");
const Brn Http::kHeaderRange("Range");
const Brn Http::kHeaderReferer("Referer");
const Brn Http::kHeaderTe("Te");
const Brn Http::kHeaderUserAgent("User-Agent");
const Brn Http::kHeaderAcceptRanges("Accept-Ranges");
const Brn Http::kHeaderAge("Age");
const Brn Http::kHeaderETag("ETag");
const Brn Http::kHeaderLocation("Location");
const Brn Http::kHeaderProxyAuthenticate("Proxy-Authenticate");
const Brn Http::kHeaderRetryAfter("Retry-After");
const Brn Http::kHeaderServer("Server");
const Brn Http::kHeaderVary("Vary");
const Brn Http::kHeaderWwwAuthenticate("WWW-Authenticate");
const Brn Http::kHeaderAllow("Allow");
const Brn Http::kHeaderContentEncoding("Content-Encoding");
const Brn Http::kHeaderContentLanguage("Content-Language");
const Brn Http::kHeaderContentLength("Content-Length");
const Brn Http::kHeaderContentLocation("Content-Location");
const Brn Http::kHeaderContentMd5("Content-Md5");
const Brn Http::kHeaderContentRange("Content-Range");
const Brn Http::kHeaderContentType("Content-Type");
const Brn Http::kHeaderExpires("Expires");
const Brn Http::kHeaderLastModified("Last-Modified");

// Http Constants

const Brn Http::kUriPrefix("http://");
const Brn Http::kValueZero("0");
const Brn Http::kHeaderSeparator(": ");
const Brn Http::kHeaderTerminator("\r\n");
const Brn Http::kChunkedCountSeparator("\r\n");
const Brn Http::kConnectionClose("close");
const Brn Http::kContentLanguageEnglish("en");
const Brn Http::kRangeBytes("bytes=");
const Brn Http::kRangeSeparator("-");
const Brn Http::kExpect100Continue("100-continue");
const Brn Http::kTransferEncodingChunked("chunked");
const Brn Http::kTransferEncodingIdentity("identity");

// Http::EVersion

static const Brn kHttpVersion09("HTTP/0.9");
static const Brn kHttpVersion10("HTTP/1.0");
static const Brn kHttpVersion11("HTTP/1.1");
static const Brn kRtspVersion10("RTSP/1.0");

Http::EVersion Http::Version(const Brx& aBuffer)
{
    if (aBuffer == kHttpVersion11) {
        return (eHttp11);
    }
    if (aBuffer == kHttpVersion10) {
        return (eHttp10);
    }
    if (aBuffer == kHttpVersion09) {
        return (eHttp09);
    }
    if (aBuffer == kRtspVersion10) {
        return (eRtsp10);
    }
    THROW(HttpError);
}

const Brx& Http::Version(Http::EVersion aVersion)
{
    switch (aVersion) {
    case eHttp11:
        return (kHttpVersion11);
    case eHttp10:
        return (kHttpVersion10);
    case eHttp09:
        return (kHttpVersion09);
    case eRtsp10:
        return (kRtspVersion10);
    default:
        ASSERTS();
    }
    return Brx::Empty();
}

void Http::WriteHeaderRangeFirstOnly(WriterHttpHeader& aWriter, TUint64 aFirst)
{
    Bws<6+20+1> buf;
    buf.Append(Http::kRangeBytes);
    Ascii::AppendDec(buf, aFirst);
    buf.Append(Http::kRangeSeparator);

    aWriter.WriteHeader(Http::kHeaderRange, buf);
}

void Http::WriteHeaderRange(WriterHttpHeader& aWriter, TUint64 aFirst, TUint64 aLast)
{
    Bws<6+20+1+20> buf;
    buf.Append(Http::kRangeBytes);
    Ascii::AppendDec(buf, aFirst);
    buf.Append(Http::kRangeSeparator);
    Ascii::AppendDec(buf, aLast);

    aWriter.WriteHeader(Http::kHeaderRange, buf);
}

void Http::WriteHeaderHost(WriterHttpHeader& aWriter, const Uri& aUri)
{
    aWriter.WriteHeader(Http::kHeaderHost, aUri.Host());
}

void Http::WriteHeaderHostAndPort(WriterHttpHeader& aWriter, const Uri& aUri)
{
    IWriterAscii& writer = aWriter.WriteHeaderField(Http::kHeaderHost);
    writer.Write(aUri.Host());
    writer.Write(':');
    writer.WriteUint(aUri.Port());
    writer.WriteNewline();
}

void Http::WriteHeaderContentLength(WriterHttpHeader& aWriter, TUint aLength)
{
    Bws<Ascii::kMaxUintStringBytes> len;
    (void)Ascii::AppendDec(len, aLength);
    aWriter.WriteHeader(Http::kHeaderContentLength, len);
}

void Http::WriteHeaderContentType(WriterHttpHeader& aWriter, const Brx& aType)
{
    aWriter.WriteHeader(Http::kHeaderContentType, aType);
}

void Http::WriteHeaderConnectionClose(WriterHttpHeader& aWriter)
{
    aWriter.WriteHeader(Http::kHeaderConnection, Http::kConnectionClose);
}

// HttpStatus

TUint HttpStatus::Code() const
{
    return iCode;
}

const Brx& HttpStatus::Reason() const
{
    return iReason;
}

TBool HttpStatus::operator==(const HttpStatus& aStatus) const
{
    return (iCode == aStatus.iCode);
}

TBool HttpStatus::operator!=(const HttpStatus& aStatus) const
{
    return (iCode != aStatus.iCode);
}

HttpStatus::HttpStatus(TUint aCode, const Brn aReason) : iCode(aCode), iReason(aReason)
{
}

class HttpStatusMaker : public HttpStatus
{
public:
    HttpStatusMaker(TUint aCode, const Brn aReason) : HttpStatus(aCode, aReason) {}
};

// Informational codes
const HttpStatus HttpStatus::kContinue = HttpStatusMaker(100, Brn("Continue"));
const HttpStatus HttpStatus::kSwitchingProtocols = HttpStatusMaker(101, Brn("Switching Protocols"));
// Success codes
const HttpStatus HttpStatus::kOk = HttpStatusMaker(200, Brn("OK"));
const HttpStatus HttpStatus::kCreated = HttpStatusMaker(201, Brn("Created"));
const HttpStatus HttpStatus::kAccepted = HttpStatusMaker(202, Brn("Accepted"));
const HttpStatus HttpStatus::kNonAuthoritativeInformation = HttpStatusMaker(203, Brn("Non-Authoritative Information"));
const HttpStatus HttpStatus::kNoContent = HttpStatusMaker(204, Brn("No Content"));
const HttpStatus HttpStatus::kResetContent = HttpStatusMaker(205, Brn("Reset Content"));
const HttpStatus HttpStatus::kPartialContent = HttpStatusMaker(206, Brn("Partial Content"));
// Redirection codes
const HttpStatus HttpStatus::kMultipleChoices = HttpStatusMaker(300, Brn("Multiple Choices"));
const HttpStatus HttpStatus::kMovedPermanently = HttpStatusMaker(301, Brn("Moved Permanently"));
const HttpStatus HttpStatus::kFound = HttpStatusMaker(302, Brn("Found"));
const HttpStatus HttpStatus::kSeeOther = HttpStatusMaker(303, Brn("See Other"));
const HttpStatus HttpStatus::kNotModified = HttpStatusMaker(304, Brn("Not Modified"));
const HttpStatus HttpStatus::kUseProxy = HttpStatusMaker(305, Brn("Use Proxy"));
const HttpStatus HttpStatus::kTemporaryRedirect = HttpStatusMaker(307, Brn("Temporary Redirect"));
// Client error codes
const HttpStatus HttpStatus::kBadRequest = HttpStatusMaker(400, Brn("Bad Request"));
const HttpStatus HttpStatus::kUnauthorized = HttpStatusMaker(401, Brn("Unauthorized"));
const HttpStatus HttpStatus::kPaymentRequired = HttpStatusMaker(402, Brn("Payment Required"));
const HttpStatus HttpStatus::kForbidden = HttpStatusMaker(403, Brn("Forbidden"));
const HttpStatus HttpStatus::kNotFound = HttpStatusMaker(404, Brn("Not Found"));
const HttpStatus HttpStatus::kMethodNotAllowed = HttpStatusMaker(405, Brn("Method Not Allowed"));
const HttpStatus HttpStatus::kNotAcceptable = HttpStatusMaker(406, Brn("Not Acceptable"));
const HttpStatus HttpStatus::kProxyAuthenticationRequired = HttpStatusMaker(407, Brn("Proxy Authentication Required"));
const HttpStatus HttpStatus::kRequestTimeout = HttpStatusMaker(408, Brn("Request Timeout"));
const HttpStatus HttpStatus::kConflict = HttpStatusMaker(409, Brn("Conflict"));
const HttpStatus HttpStatus::kGone = HttpStatusMaker(410, Brn("Gone"));
const HttpStatus HttpStatus::kLengthRequired = HttpStatusMaker(411, Brn("Length Required"));
const HttpStatus HttpStatus::kPreconditionFailed = HttpStatusMaker(412, Brn("Precondition Failed"));
const HttpStatus HttpStatus::kRequestEntityTooLarge = HttpStatusMaker(413, Brn("Request Entity Too Large"));
const HttpStatus HttpStatus::kRequestUriTooLarge = HttpStatusMaker(414, Brn("Request URI Too Large"));
const HttpStatus HttpStatus::kUnsupportedMediaType = HttpStatusMaker(415, Brn("Unsupported Media Type"));
const HttpStatus HttpStatus::kRequestedRangeNotSatisfiable = HttpStatusMaker(416, Brn("Request Range Not Satisfiable"));
const HttpStatus HttpStatus::kExpectationFailure = HttpStatusMaker(417, Brn("Expectation Failure"));
// Server error codes
const HttpStatus HttpStatus::kInternalServerError = HttpStatusMaker(500, Brn("Internal Server Error"));
const HttpStatus HttpStatus::kNotImplemented = HttpStatusMaker(501, Brn("Not Implemented"));
const HttpStatus HttpStatus::kBadGateway = HttpStatusMaker(502, Brn("Bad Gateway"));
const HttpStatus HttpStatus::kServiceUnavailable = HttpStatusMaker(503, Brn("Service Unavailable"));
const HttpStatus HttpStatus::kGatewayTimeout = HttpStatusMaker(504, Brn("Gateway Timeout"));
const HttpStatus HttpStatus::kHttpVersionNotSupported = HttpStatusMaker(505, Brn("HTTP Version Not Supported"));

// ReaderHttpHeader

ReaderHttpHeader::ReaderHttpHeader()
{
}

void ReaderHttpHeader::AddHeader(IHttpHeader& aHeader)
{
    iHeaders.push_back(&aHeader);
}

IHttpHeader& ReaderHttpHeader::Header() const
{
    ASSERT(iHeader);
    return (*iHeader);
}

void ReaderHttpHeader::ResetHeaders()
{
    iHeader = 0;
    TUint count = iHeaders.size();
    for (TUint i = 0; i < count; i++) {
        iHeaders[i]->Reset();
    }
}

void ReaderHttpHeader::ProcessHeader(const Brx& aField, const Brx& aValue)
{
    TUint count = iHeaders.size();
    for (TUint i = 0; i < count; i++) {
        IHttpHeader* header = iHeaders[i];
        if (header->Recognise(aField)) {
            iHeader = header;
            header->Process(aValue);        
            return;
        }
    } 
}



// ReaderHttpRequest

ReaderHttpRequest::ReaderHttpRequest(IReader& aReader)
    : iReader(aReader)
{
}

void ReaderHttpRequest::Read()
{
    TUint count = 0;
    iMethod = 0;
    ResetHeaders();

    for (;;) {
        Brn line;
        line.Set(Ascii::Trim(iReader.ReadUntil(Ascii::kLf)));
//        LOG(kHttp, "HTTP Read Request   ");
//        LOG(kHttp, line);
//        LOG(kHttp, "\n");
        TUint bytes = line.Bytes();
        if (!bytes) {
            if (count == 0) {
                continue; // a blank line before first header - ignore (RFC 2616 section 4.1)
            }
            if (iMethod == 0) {
                THROW(HttpError);
            }
            return;
        }

        if (Ascii::IsWhitespace(line[0])) {
            continue; // a line starting with spaces is a continuation line
        }
    
        Parser parser(line);

        if (count == 0) { // method
            Brn method = parser.Next();
            Brn uri = parser.Next();
            Brn version = Ascii::Trim(parser.Remaining());
            ProcessMethod(method, uri, version);
        }
        else { // header
            Brn field = parser.Next(':');
            Brn value = Ascii::Trim(parser.Remaining());
            ProcessHeader(field, value);
        }
        count++;
    }
}

void ReaderHttpRequest::AddMethod(const Brx& aMethod)
{
    iMethods.push_back(&aMethod);
}

TBool ReaderHttpRequest::MethodNotAllowed() const
{
    return (iMethod == 0); 
}

const Brx& ReaderHttpRequest::Method() const
{
    ASSERT(iMethod != 0);
    return (*iMethod);
}

const Brx& ReaderHttpRequest::Uri() const
{
    return iUri;
}

Http::EVersion ReaderHttpRequest::Version() const
{
    return iVersion;
}
    
void ReaderHttpRequest::ProcessMethod(const Brx& aMethod, const Brx& aUri, const Brx& aVersion)
{
    TUint count = iMethods.size();
    for (TUint i = 0; i < count; i++) {
        if (*iMethods[i] == aMethod)
        {
            iMethod = iMethods[i];
            if (aUri.Bytes() > kMaxUriBytes) {
                THROW(HttpError);
            }
            iUri.Replace(aUri);
            //May throw HttpError
            iVersion = Http::Version(aVersion);
            return;
        }
    }

    LOG2(kHttp, kError, "Unsupported method - ");
    LOG2(kHttp, kError, aMethod);
    LOG2(kHttp, kError, "\n");
    THROW(HttpError);
}

void ReaderHttpRequest::Flush()
{
    iReader.ReadFlush();
}

void ReaderHttpRequest::Interrupt()
{
    iReader.ReadInterrupt();
}

// ReaderHttpResponse

ReaderHttpResponse::ReaderHttpResponse(IReader& aReader)
    : iReader(aReader)
{
}

Http::EVersion ReaderHttpResponse::Version() const
{
    return iVersion;
}

const HttpStatus& ReaderHttpResponse::Status() const
{
    return iStatus;
}

void ReaderHttpResponse::Read()
{   
    iReader.ReadFlush();
    ResetHeaders();
    TUint count = 0;
    for (;;) {
        Brn line;
        line.Set(Ascii::Trim(iReader.ReadUntil(Ascii::kLf)));
        LOG(kHttp, "HTTP Read Response  ");
        LOG(kHttp, line);
        LOG(kHttp, "\n");
        TUint bytes = line.Bytes();
        if (!bytes) {
            if (count == 0) {
                continue; // a blank line before first header - ignore (RFC 2616 section 4.1)
            }
            return;
        }

        if (Ascii::IsWhitespace(line[0])) {
            continue; // a line starting with spaces is a continuation line
        }

        Parser parser(line);

        if (count == 0) { // status
 //            LOG(kHttp, " status   ");
            Brn version = parser.Next(' ');
            Brn code = parser.Next(' ');
            Brn description = Ascii::Trim(parser.Remaining());
            ProcessStatus(version, code, description);
        }
        else { // header
//            LOG(kHttp, " header  ");
            Brn field = parser.Next(':');
            Brn value = Ascii::Trim(parser.Remaining());
            ProcessHeader(field, value);
        }
        count++;
    }
}

void ReaderHttpResponse::ProcessStatus(const Brx& aVersion, const Brx& aCode, const Brx& aDescription)
{
    TUint code;
    if (aVersion == Brn("ICY")) {
        iVersion = Http::eHttp11;
    }
    else {
        iVersion = Http::Version(aVersion);
    }

    try {
        code = Ascii::Uint(aCode);
    }
    catch (AsciiError&) {
        THROW(HttpError);
    }

    if (aDescription.Bytes() > kMaxDescriptionBytes) {
        THROW(HttpError);
    }

    iDescription.Replace(aDescription);
    iStatus.Set(code, iDescription);
}

void ReaderHttpResponse::Flush()
{
    iReader.ReadFlush();
}

void ReaderHttpResponse::Interrupt()
{
    iReader.ReadInterrupt();
}

// ReaderHttpResponse::StatusWritable

ReaderHttpResponse::StatusWritable::StatusWritable()
    : HttpStatus(0, Brx::Empty())
{
}

void ReaderHttpResponse::StatusWritable::Set(TUint aCode, const Brx& aDescription)
{
    iCode = aCode;
    const_cast<Brn*>(&iReason)->Set(aDescription);
}

// WriterHttpField

WriterHttpField::WriterHttpField(IWriter& aWriter)
    : WriterAscii(aWriter)
{
}

void WriterHttpField::Write(TByte aValue)
{
    LOG(kHttp, Brn(&aValue, 1));
    WriterAscii::Write(aValue);
}

void WriterHttpField::Write(const Brx& aBuffer)
{
    LOG(kHttp, aBuffer);
    WriterAscii::Write(aBuffer);
}

void WriterHttpField::WriteFlush()
{
    WriterAscii::WriteNewline();
}

void WriterHttpField::Flush()
{
    WriterAscii::WriteFlush();
}

// WriterHttpHeader

WriterHttpHeader::WriterHttpHeader(IWriter& aWriter)
    : iWriter(aWriter)
{
}

void WriterHttpHeader::Write(TByte /*aValue*/)
{
    ASSERTS();
}

void WriterHttpHeader::Write(const Brx& /*aBuffer*/)
{
    ASSERTS();
}

void WriterHttpHeader::WriteFlush()
{
    LOG(kHttp, "Http Write Header   ");
    iWriter.WriteNewline();
    iWriter.Flush();
}

void WriterHttpHeader::WriteHeader(const Brx& aField, const Brx& aValue)
{
    LOG(kHttp, "Http Write Header   ");

    iWriter.Write(aField);
    iWriter.Write(Http::kHeaderSeparator);
    iWriter.Write(aValue);
    iWriter.WriteNewline();
}

IWriterAscii& WriterHttpHeader::WriteHeaderField(const Brx& aField)
{
    LOG(kHttp, "Http Write Header   ");
    
    iWriter.Write(aField);
    iWriter.Write(Http::kHeaderSeparator);
    return (iWriter);
}

// WriterHttpRequest

WriterHttpRequest::WriterHttpRequest(IWriter& aWriter) : WriterHttpHeader(aWriter)
{
}

void WriterHttpRequest::WriteMethod(const Brx& aMethod, const Brx& aUri, Http::EVersion aVersion)
{
    LOG(kHttp, "Http Write Method   ");
    
    iWriter.Write(aMethod);
    iWriter.WriteSpace();
    iWriter.Write(aUri);
    iWriter.WriteSpace();
    iWriter.Write(Http::Version(aVersion));
    iWriter.WriteNewline();
}

// WriterHttpResponse

WriterHttpResponse::WriterHttpResponse(IWriter& aWriter) : WriterHttpHeader(aWriter)
{
}

void WriterHttpResponse::WriteStatus(const HttpStatus& aStatus, Http::EVersion aVersion)
{
    LOG(kHttp, "Http Write Status   ");

    iWriter.Write(Http::Version(aVersion));
    iWriter.WriteSpace();
    iWriter.WriteUint(aStatus.Code());
    iWriter.WriteSpace();
    iWriter.Write(aStatus.Reason());
    iWriter.WriteNewline();
}

void WriterHttpResponse::Write(const Brx& aBuffer)
{
    iWriter.Write(aBuffer);
}

// HttpHeader

HttpHeader::HttpHeader()
{
    iReceived = false;
}

TBool HttpHeader::Received() const
{
    return iReceived;
}

void HttpHeader::SetReceived()
{
    iReceived = true;
}

void HttpHeader::Reset()
{
    iReceived = false;
}

// class HttpHeaderHost

Zapp::Endpoint& HttpHeaderHost::Endpoint()
{
    if (!Received()) {
        Zapp::Endpoint defaultEndpoint;
        iEndpoint = defaultEndpoint;
    }
    return iEndpoint;
}

TBool HttpHeaderHost::Recognise(const Brx& aHeader)
{
    return (Ascii::CaseInsensitiveEquals(aHeader, Http::kHeaderHost));
}

void HttpHeaderHost::Process(const Brx& aValue)
{
    Zapp::Endpoint defaultEndpoint;
    iEndpoint = defaultEndpoint;
    SetReceived();
    try {
        TUint index = Ascii::IndexOf(aValue, ':');
        Brn address(aValue);
        TUint port = 0;
        if (index != aValue.Bytes()) {
            address.Set(address.Ptr(), index);
            Brn portBuf = aValue.Split(index+1, aValue.Bytes()-index-1);
            port = Ascii::Uint(portBuf);
        }
        iEndpoint = Zapp::Endpoint(port, address);
    }
    catch(AsciiError&) {
        THROW(HttpError);
    }
    catch(NetworkError&) {
        THROW(HttpError);
    }
}

// HttpHeaderLocation

const Brx& HttpHeaderLocation::Location() const
{
    return (iLocation);
}

TBool HttpHeaderLocation::Recognise(const Brx& aHeader)
{
    return Ascii::CaseInsensitiveEquals(aHeader, Http::kHeaderLocation);
}

void HttpHeaderLocation::Process(const Brx& aValue)
{
    try {
        iLocation.ReplaceThrow(aValue);
        SetReceived();
    }
    catch (BufferOverflow&) {
    }
}

// HttpHeaderContentType

const Brx& HttpHeaderContentType::Type() const
{
    return iType;
}

TBool HttpHeaderContentType::Recognise(const Brx& aHeader)
{
    return Ascii::CaseInsensitiveEquals(aHeader, Http::kHeaderContentType);
}

void HttpHeaderContentType::Process(const Brx& aValue)
{
    Parser parser(aValue);
    
    try {
        Brn type = parser.Next(';');
        Parser parser2(type);  // get first word of the type
        iType.Replace(parser2.Next());
        SetReceived();
    }
    catch (BufferOverflow&) {
        return;
    }
    
    Brn key;
    Brn value;
    
    for (;;) {
        key.Set(parser.Next('='));
        if (key.Bytes() == 0) {
            return;
        }
        value.Set(parser.Next());
        Process(key, value);
    }
}

void HttpHeaderContentType::Process(const Brx& /*aKey*/, const Brx& /*aValue*/)
{
}

// HttpHeaderContentLength

TUint HttpHeaderContentLength::ContentLength() const
{
    return (Received()? iContentLength : 0);
}

TBool HttpHeaderContentLength::Recognise(const Brx& aHeader)
{
    return (Ascii::CaseInsensitiveEquals(aHeader, Http::kHeaderContentLength));
}

void HttpHeaderContentLength::Process(const Brx& aValue)
{
    iContentLength = 0;
    SetReceived();
    try {
        iContentLength = Ascii::Uint(aValue);
    }
    catch (AsciiError&) {
        THROW(HttpError);
    }
}

// HttpHeaderTransferEncoding

TBool HttpHeaderTransferEncoding::IsChunked() const
{
    return (Received()? iChunked : false);
}

TBool HttpHeaderTransferEncoding::Recognise(const Brx& aHeader)
{
    return (Ascii::CaseInsensitiveEquals(aHeader, Http::kHeaderTransferEncoding));
}

void HttpHeaderTransferEncoding::Process(const Brx& aValue)
{
    SetReceived();
    iChunked = !Ascii::CaseInsensitiveEquals(aValue, Http::kTransferEncodingIdentity);
}

// HttpHeaderConnection

TBool HttpHeaderConnection::Close() const
{
    return (Received()? iClose : false);
}

TBool HttpHeaderConnection::Recognise(const Brx& aHeader)
{
    return Ascii::CaseInsensitiveEquals(aHeader, Http::kHeaderConnection);
}

void HttpHeaderConnection::Process(const Brx& aValue)
{
    iClose = false;
    if (aValue == Http::kConnectionClose) {
        iClose = true;
        SetReceived();
    }
}

// EndpointHttp

EndpointHttp::EndpointHttp(const Uri& aUri)
{
    SetAddress(aUri.Host());
    SetPort(aUri.Port() == -1 ? 80 : aUri.Port());
}

// ReaderHttpChunked

ReaderHttpChunked::ReaderHttpChunked(IReader& aReader)
    : iReader(aReader)
{
}

void ReaderHttpChunked::Read()
{
    for (;;) {
        Brn chunkSizeBuf = iReader.ReadUntil(Ascii::kLf);
        Parser parser(chunkSizeBuf);
        Brn trimmed = parser.Next(Ascii::kCr);
        if (trimmed.Bytes() == 0) {
            continue;
        }
        TUint chunkSize;
        try {
            chunkSize = Ascii::UintHex(trimmed);
        }
        catch (AsciiError&) {
            THROW(ReaderError);
        }
        if (chunkSize == 0) {
            break;
        }
        iEntity.Grow(iEntity.Bytes() + chunkSize);
        iEntity.Append(iReader.Read(chunkSize));
    }
}

void ReaderHttpChunked::TransferTo(Bwh& aBuf)
{
    iEntity.TransferTo(aBuf);
}

// WriterHttpChunked

WriterHttpChunked::WriterHttpChunked(IWriter& aWriter)
    : iBuffer(aWriter)
    , iChunked(false)
{
}

void WriterHttpChunked::SetChunked(TBool aValue)
{
    iChunked = aValue;
}

void WriterHttpChunked::Write(TByte aValue)
{
    if (iChunked) {
        iBuffer.Write('1');
        iBuffer.Write(Http::kHeaderTerminator);
        iBuffer.Write(aValue);
        iBuffer.Write(Http::kHeaderTerminator);
    }
    else {
        iBuffer.Write(aValue);
    }
}

void WriterHttpChunked::Write(const Brx& aBuffer)
{
    if (iChunked) {
        Bws<16> count;
        Ascii::AppendHex(count, aBuffer.Bytes());
        iBuffer.Write(count);
        iBuffer.Write(Http::kHeaderTerminator);
        iBuffer.Write(aBuffer);
        iBuffer.Write(Http::kHeaderTerminator);
    }
    else {
        iBuffer.Write(aBuffer);
    }
}

void WriterHttpChunked::WriteFlush()
{
    if (iChunked) {
        iBuffer.Write('0');
        iBuffer.Write(Http::kHeaderTerminator);
        iBuffer.Write(Http::kHeaderTerminator);
    }
    iBuffer.WriteFlush();
}
