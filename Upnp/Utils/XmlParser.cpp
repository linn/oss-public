#include <XmlParser.h>
#include <ZappTypes.h>
#include <Buffer.h>
#include <Ascii.h>
#include <Parser.h>

using namespace Zapp;

Brn XmlParserBasic::Find(const TChar* aTag, const Brx& aDocument)
{
    Brn ignore;
    return XmlParserBasic::Find(aTag, aDocument, ignore);
}

Brn XmlParserBasic::Find(const Brx& aTag, const Brx& aDocument)
{
    Brn ignore;
    return XmlParserBasic::Find(aTag, aDocument, ignore);
}

Brn XmlParserBasic::Find(const TChar* aTag, const Brx& aDocument, Brn& aRemaining)
{
    Brn tag(aTag);
    return XmlParserBasic::Find(tag, aDocument, aRemaining);
}

Brn XmlParserBasic::Find(const Brx& aTag, const Brx& aDocument, Brn& aRemaining)
{
    EParserState state = eSearchOpen;
    TInt ignoreClose = 0;
    Brn namesp;
    Brn name;
    Brn attributes;
    Brn ns;
    TUint index;
    Brn doc(Ascii::Trim(aDocument));
    Brn remaining(Ascii::Trim(aDocument));
    Brn retStart;
    ETagType tagType;
    for (;;) {
        NextTag(doc, name, attributes, ns, index, remaining, tagType);
        if (name == aTag) {
            if (state == eSearchOpen) {
                if (tagType == eTagClose) {
                    if (--ignoreClose < 0)
                        THROW(XmlError);
                }
                else if (tagType == eTagOpenClose) {
                    return Brn(Brx::Empty());
                }
                namesp.Set(ns);
                retStart.Set(remaining);
                state = eSearchClose;
            }
            else { // eSearchClose
                if (tagType == eTagOpen) {
                    ++ignoreClose;
                }
                else if (tagType == eTagClose && namesp == ns) {
                    if (ignoreClose == 0) {
                        aRemaining.Set(remaining);
                        const TUint retBytes = doc.Ptr() - retStart.Ptr() + index;
                        Brn ret(retStart.Ptr(), retBytes);
                        return ret;
                    }
                    ignoreClose--;
                }
            }
        }
        if (remaining.Bytes() == 0) {
            THROW(XmlError);
        }
        doc.Set(remaining);
    }
}

void XmlParserBasic::NextTag(const Brx& aDocument, Brn& aName, Brn& aAttributes, Brn& aNamespace, TUint& aIndex, Brn& aRemaining, ETagType& aType)
{
    aName.Set(Brx::Empty());
    aAttributes.Set(Brx::Empty());
    aNamespace.Set(Brx::Empty());
    aRemaining.Set(Brx::Empty());
    Parser parser(aDocument);
    for (;;) {
        Brn item = parser.Next('>');
        TUint bytes = item.Bytes();
        if (bytes > 0 && item[0] != '<') {
            Parser valueParser(item);
            Brn value = valueParser.Next('<');
            bytes -= value.Bytes();
            item.Set(item.Split(value.Bytes(), bytes));
            item.Set(Ascii::Trim(item));
            bytes = item.Bytes();
        }
        if (bytes < 2 || item[0] != '<') {
            THROW(XmlError);
        }
        aIndex = item.Ptr() - aDocument.Ptr();
        if (item[1] == '?') {
            if (bytes < 3) { // catch special case of <?>
                THROW(XmlError);
            }
            if (item[bytes - 1] == '?') { // processing instruction
                continue;
            }
            THROW(XmlError);
        }

        aRemaining.Set(parser.Remaining());

        TUint start = 1; // skip opening '<'
        TUint len = bytes-1;
        if (item[1] == '/') {
            aType = eTagClose;
            start++;
            len--;
        }
        else if (item[bytes-1] == '/') {
            aType = eTagOpenClose;
            len--;
        }
        else {
            aType = eTagOpen;
        }

        parser.Set(item.Split(start, len));
        aName.Set(parser.Next(' '));
        aAttributes.Set(parser.Remaining());
        
        if (Ascii::Contains(aName, ':')) { // collect the namespace
            parser.Set(aName);
            aNamespace.Set(parser.Next(':'));
            if (!aNamespace.Bytes()) {
                THROW(XmlError);
            }
            aName.Set(parser.Remaining());
        }
        else {
            aNamespace.Set(Brx::Empty());
        }

        if (!aName.Bytes()) {
            THROW(XmlError);
        }

        return;
    }
}

Brn XmlParserBasic::FindAttribute(const TChar* aTag, const TChar* aAttribute, const Brx& aDocument)
{
    Brn tag(aTag);
    Brn attribute(aAttribute);
    return (XmlParserBasic::FindAttribute(tag, attribute, aDocument));
}

Brn XmlParserBasic::FindAttribute(const Brx& aTag, const Brx& aAttribute, const Brx& aDocument)
{
    Brn namesp;
    Brn name;
    Brn attributes;
    Brn ns;
    TUint index;
    Brn doc(Ascii::Trim(aDocument));
    Brn remaining(Ascii::Trim(aDocument));
    Brn retStart;
    ETagType tagType;
    for (;;) {
        NextTag(doc, name, attributes, ns, index, remaining, tagType);
        if (name == aTag) {
            if (tagType != eTagClose) {
            	Parser parser(attributes);
            	while (!parser.Finished()) {
            		Brn att = parser.Next('=');
            		Brn ws = parser.Next('\"');
            		Brn value = parser.Next('\"');
            		if (att == aAttribute) {
            			return (value);
            		}
            	}
            }
        }
        if (remaining.Bytes() == 0) {
            THROW(XmlError);
        }
        doc.Set(remaining);
    }
}
