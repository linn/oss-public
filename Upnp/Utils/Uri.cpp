#include <Uri.h>
#include <Parser.h>
#include <Ascii.h>


using namespace Zapp;


Uri::Uri()
{
    Clear();
}

Uri::Uri(const Brx& aUri)
{
    Replace(aUri);
}

Uri::Uri(const Brx& aBaseUri, const Brx& aRelativeUri)
{
    Replace(aBaseUri, aRelativeUri);
}

void Uri::Clear()
{
    iAbsoluteUri.Replace(Brx::Empty());
    iScheme.Set(Brx::Empty());
    iHost.Set(Brx::Empty());
    iPort = -1;
    iAuthority.Set(Brx::Empty());
    iPath.Set(Brx::Empty());
    iQuery.Set(Brx::Empty());
    iPathAndQuery.Set(Brx::Empty());
    iFragment.Set(Brx::Empty());
}

void Uri::Replace(const Brx& aUri)
{
    Clear();
    Parse(aUri);
}

void Uri::Replace(const Brx& aBaseUri, const Brx& aRelativeUri)
{
    // merge base and relative path into an absolute uri
    iAbsoluteUri.Replace(Brx::Empty());
    iBase.Replace(Brx::Empty());
    if (aRelativeUri.Equals(Brx::Empty())) {
        THROW(UriError);
    }
    iRelative.Replace(aRelativeUri);
    Ascii::Substitute(iRelative, '\\', '/'); // convert path to use only back slashes
    
    // if no ':' in the path, then it is a relative path, otherwise already an absolute path
    if (!Ascii::Contains(aRelativeUri, ':')) {
        if (aBaseUri.Equals(Brx::Empty())) {
            THROW(UriError);
        }
        iBase.Replace(aBaseUri);
        Ascii::Substitute(iBase, '\\', '/'); // convert path to use only back slashes
    
        while (iRelative.At(0) == '/') { // insure relative path starts without back slash
            iRelative.Replace(iRelative.Split(1,iRelative.Bytes()-1));
        }
        if (iBase.At(iBase.Bytes()-1) != '/') { // insure base path ends with back slash
            iBase.Append('/');
        }
    
        Parser parser(iRelative);
        Brn section;
        TUint relCount = 0, relIndex = 0;
        while (!parser.Finished()) {
            section.Set(parser.Next('/'));
            if (section.Bytes() > 0) {
                if (section.At(0) == '.') {
                    relCount++;
                    relIndex = parser.Index();
                }
                else {
                    break;
                }
            }
        }
        if (relCount > kMaxDirLevels) {
            THROW(UriError);
        }
        iRelative.Replace(iRelative.Split(relIndex, iRelative.Bytes()-relIndex));
        
        if (relCount > 0) {
            parser.Set(iBase);
            TUint baseCount = 0;
            TUint baseIndex[kMaxDirLevels];
            while (!parser.Finished()) {
                section.Set(parser.Next('/'));
                baseIndex[baseCount] = parser.Index();
                baseCount++;
            }
            if (relCount >= baseCount) {
                THROW(UriError);
            }
            iBase.Replace(iBase.Split(0, baseIndex[(baseCount-1)-relCount]));
        }
        iAbsoluteUri.Append(iBase);
    }
    iAbsoluteUri.Append(iRelative);
    iBase.Replace(iAbsoluteUri);
    Replace(iBase);
}

void Uri::Parse(const Brx& aUri)
{
    // See RFC 2396 for details (Appendix A gives a summary of the rules)
    //
    Brn scheme;
    Brn host;
    TInt port;
    Brn portBuff;
    Brn path;
    Brn query;
    Brn fragment;

    // Parse the fragment first
    Parser parser(aUri);
    Brn currUri = parser.NextNoTrim('#'); // currUri = "scheme://host[:port]/path?query"
    fragment.Set(parser.Remaining());

    // Parse the scheme - everything up to the first ":"
    parser.Set(currUri);
    scheme.Set(parser.Next(':'));   // scheme = "scheme" remaining = "//host[:port]/path?query"
    if (parser.Finished()) {
        THROW(UriError); // No ":" scheme delimiter (or URI == scheme:)
    }

    // Parse for the query
    currUri.Set(parser.NextNoTrim('?'));  // currUri = "//host[:port]/path" remaining = "query"
    query.Set(parser.Remaining());

    // The next character must be a '/'
    parser.Set(currUri);    // remaining = "//host[:port]/path"
    if (parser.Finished() || parser.At(0) != '/') {
        THROW(UriError);
    }
    parser.Next('/');   // remaining = "/host[:port]/path"

    // Check for an authority
    if (parser.Finished()) {
        THROW(UriError); // URI is scheme:/
    }
    if (parser.At(0) != '/') {
        THROW(UriError); // No authority unsupported
    }

    // An authority is specified in the URI
    parser.Next('/');   // skip over the leading '/' - remaining = "host[:port]/path"
    if (parser.Finished()) {
        THROW(UriError); // URI is scheme://
    }

    Brn authority = parser.Next('/');   // authority = "host[:port]" remaining = "path"
    if (Ascii::Contains(authority, '@')) {
        THROW(UriError); // URI is scheme://userinfo@host - unsupported
    }

    // Parse the path - set the local stack 'path' buffer to be the path
    // without the leading '/'
    path.Set(parser.Remaining());

    // Parse the host and port
    if (Ascii::Contains(authority, ':')) {
        // host:port
        parser.Set(authority);  // remaining = "host:port"
        host.Set(parser.Next(':'));
        try {
            portBuff.Set(parser.Remaining());
            port = Ascii::Uint(parser.Remaining());
        }
        catch (AsciiError&) {
            THROW(UriError); // Port is not a number
        }
    }
    else {
        host.Set(authority);
        portBuff.Set(Brx::Empty());
        port = -1;
    }


    // Reconstruct the iAbsoluteUri - scheme://host[:port]/path[?query][#fragment]

    // check length of resulting URI
    TUint length = scheme.Bytes() + 3 + host.Bytes();   // = len("scheme://host")
    if (port != -1) {
        length += portBuff.Bytes() + 1;   // += len(":port")
    }
    length += EscapedBytes(path) + 1; // += len("/path")
    if (query.Bytes() != 0) {
        length += EscapedBytes(query) + 1;    // += len("?query");
    }
    if (fragment.Bytes() != 0) {
        length += EscapedBytes(fragment) + 3;    // += len("#fragment"); (# is escaped to %23)
    }
    if (length > kMaxUriBytes) {
        THROW(UriError);
    }

    // scheme
    iAbsoluteUri.Append(scheme);
    iScheme.Set(iAbsoluteUri);
    iAbsoluteUri.Append("://");

    // host
    TUint start = iAbsoluteUri.Bytes();
    iAbsoluteUri.Append(host);
    iHost.Set(iAbsoluteUri.Split(start));

    // port
    if (port != -1) {
        iAbsoluteUri.Append(":");
        Ascii::AppendDec(iAbsoluteUri, port);
        iPort = port;
    }
    iAuthority.Set(iAbsoluteUri.Split(start));

    // path - include leading '/'
    TUint startPath = iAbsoluteUri.Bytes();
    iAbsoluteUri.Append("/");
    Escape(iAbsoluteUri, path);
    iPath.Set(iAbsoluteUri.Split(startPath));

    // query - including the leading '?'
    if (query.Bytes() != 0) {
        start = iAbsoluteUri.Bytes();
        iAbsoluteUri.Append("?");
        Escape(iAbsoluteUri, query);
        iQuery.Set(iAbsoluteUri.Split(start));
    }
    iPathAndQuery.Set(iAbsoluteUri.Split(startPath));

    // fragment - include leading '#'
    if (fragment.Bytes() != 0) {
        start = iAbsoluteUri.Bytes();
        Escape(iAbsoluteUri, Brn("#"));
        Escape(iAbsoluteUri, fragment);
        iFragment.Set(iAbsoluteUri.Split(start));
    }

    ValidateScheme();
    ValidateHost();
    ValidatePath();
}

void Uri::ValidateScheme()
{
    if (iScheme.Bytes() == 0) {
        THROW(UriError); // Zero length scheme
    }
    if (Ascii::IsAlphabetic(iScheme[0]) == false) {
        THROW(UriError); // First character must be letter
    }
    // Subsequent characters must be letter, number, +, - or .
    TUint i;
    for(i=1 ; i<iScheme.Bytes() ; i++) {
        TChar c = iScheme[i];
        if (!(Ascii::IsAlphabetic(c) || Ascii::IsDigit(c) || c == '+' || c == '-' || c == '.')) {
            THROW(UriError);
        }
    }
}

void Uri::ValidateHost()
{
    if (iHost.Bytes() == 0) {
        THROW(UriError); // Zero length host
    }
    // Host characters must be letter, number, - or .
    TUint i;
    for(i=0 ; i<iHost.Bytes() ; i++) {
        TChar c = iHost[i];
        if (!(Ascii::IsAlphabetic(c) || Ascii::IsDigit(c) || c == '-' || c == '.')) {
            THROW(UriError);
        }
    }
}

void Uri::ValidatePath()
{
    if (iPath[0] != '/') {
        THROW(UriError);
    }
    TUint i;
    for(i=0 ; i<iPath.Bytes() ; i++) {
        TChar c = iPath[i];
        if (c == '%') {
            // escaped character
            if ( (iPath.Bytes() - i <= 2) ||
                !(Ascii::IsHex(iPath[i+1]) && Ascii::IsHex(iPath[i+2])) ) {
                THROW(UriError);
            }
        }
        else if (!(Uri::IsUnreserved(c) || c == '/' ||
              c == ';' || c == ':' || c == '@' || c == '&' ||
              c == '=' || c == '+' || c == '$' || c == ',')) {
            THROW(UriError);
        }
    }
}

TUint Uri::EscapedBytes(const Brx& aBuffer)
{
    TUint escapedBytes = 0;
    TUint i=0;
    while (i<aBuffer.Bytes()) {
        // Is this an escaped substring?
        if (IsEscaped(aBuffer, i)) {
            escapedBytes += 3;
            i += 3;
            continue;
        }

        // Is it an excluded character?
        if (Uri::IsExcluded(aBuffer[i])) {
            escapedBytes += 3;
            i++;
            continue;
        }

        // This character should be ok
        escapedBytes++;
        i++;
    }
    return escapedBytes;
}

void Uri::Escape(Bwx& aDst, const Brx& aSrc)
{
    TUint i=0;
    while (i<aSrc.Bytes()) {
        if (IsEscaped(aSrc, i)) {
            // An already escaped substring - copy as is
            aDst.Append(aSrc[i]);
            aDst.Append(aSrc[i+1]);
            aDst.Append(aSrc[i+2]);
            i += 3;
            continue;
        }

        if (Uri::IsExcluded(aSrc[i])) {
            // excluded characters must be escaped
            aDst.Append('%');
            Ascii::AppendHex(aDst, aSrc[i]);
            i++;
            continue;
        }

        // No escaping required
        aDst.Append(aSrc[i]);
        i++;
    }
}

void Uri::Unescape(Bwx& aDst, const Brx& aSrc)
{
    TUint i=0;
    while (i<aSrc.Bytes()) {
        if (IsEscaped(aSrc, i)) {
            // An escaped substring - unescape it
            TByte hexDig1 = (TByte)Ascii::HexValue(aSrc.At(i+1));
            TByte hexDig2 = (TByte)Ascii::HexValue(aSrc.At(i+2));
            TByte originalChar = hexDig1*16 + hexDig2;
            aDst.Append(originalChar);
            i += 3;
            continue;
        }

        // No escaping required
        aDst.Append(aSrc[i]);
        i++;
    }
}

TBool Uri::IsEscaped(const Brx& aBuffer, TUint aIndex)
{
    if (aIndex+3 > aBuffer.Bytes()) {
        return false;
    }
    return aBuffer[aIndex] == '%' && Ascii::IsHex(aBuffer[aIndex+1]) && Ascii::IsHex(aBuffer[aIndex+2]);
}

TBool Uri::IsUnreserved(TChar aValue)
{
    return (Ascii::IsAlphabetic(aValue) || Ascii::IsDigit(aValue) ||
            aValue == '-' || aValue == '_' || aValue == '.' || aValue == '!' ||
            aValue == '~' || aValue == '*' || aValue == '\'' || aValue == '(' || aValue == ')');
}

TBool Uri::IsExcluded(TChar aValue)
{
    return (aValue<=0x20 || aValue>=0x7f ||
            aValue=='<' || aValue=='>' || aValue=='#' || aValue=='%' || aValue=='"' ||
            aValue=='{' || aValue=='}' || aValue=='|' || aValue=='\\' ||
            aValue=='^' || aValue=='[' || aValue==']' || aValue=='`');
}
