#include <Converter.h>
#include <ZappTypes.h>
#include <Buffer.h>
#include <Stream.h>

using namespace Zapp;

void Converter::ToXmlEscaped(IWriter& aWriter, TByte aValue)
{
    switch (aValue) {
    case '<':
        aWriter.Write(Brn("&lt;"));
        break;
    case '>':
        aWriter.Write(Brn("&gt;"));
        break;
    case '&':
        aWriter.Write(Brn("&amp;"));
        break;
    case '\'':
        aWriter.Write(Brn("&apos;"));
        break;
    case '\"':
        aWriter.Write(Brn("&quot;"));
        break;
    default:
        aWriter.Write(aValue);
        break;
    }
}

void Converter::ToXmlEscaped(IWriter& aWriter, const Brx& aValue)
{
    for(TUint i = 0; i < aValue.Bytes(); ++i) {
        ToXmlEscaped(aWriter, aValue[i]);
    }
}

static const TByte kBase64[64] = {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
        'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
        'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X',
        'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f',
        'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
        'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
        'w', 'x', 'y', 'z', '0', '1', '2', '3',
        '4', '5', '6', '7', '8', '9', '+', '/'
};

static const TByte kDecode64[256] = {
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x3e, 0xff, 0xff, 0xff, 0x3f, // + /
        0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, // 0 - 9 =
        0xff, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, // A - O
        0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0xff, 0xff, 0xff, 0xff, 0xff, // P - Z
        0xff, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, // a - o
        0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30, 0x31, 0x32, 0x33, 0xff, 0xff, 0xff, 0xff, 0xff, // p - z
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
};

void Converter::ToBase64(IWriter& aWriter, const Brx& aValue)
{
    TUint b = 0;
    TByte block[3];
    
    for(TUint i = 0; i < aValue.Bytes(); ++i) {
        
        block[b++] = aValue[i];
        
        if (b >= 3) {
            TByte index0 = block[0] >> 2;
            TByte index1 = (block[0] & 0x03) << 4 | block[1] >> 4;
            TByte index2 = (block[1] & 0x0f) << 2 | block[2] >> 6;
            TByte index3 = (block[2] & 0x3f);
            
            aWriter.Write(kBase64[index0]);
            aWriter.Write(kBase64[index1]);
            aWriter.Write(kBase64[index2]);
            aWriter.Write(kBase64[index3]);
            
            b = 0;
        }
    }
    
    if (b == 1) {
        TByte index0 = block[0] >> 2;
        TByte index1 = (block[0] & 0x03) << 4;
        aWriter.Write(kBase64[index0]);
        aWriter.Write(kBase64[index1]);
        aWriter.Write('=');
        aWriter.Write('=');
    }
    else if (b == 2) {
        TByte index0 = block[0] >> 2;
        TByte index1 = (block[0] & 0x03) << 4 | block[1] >> 4;
        TByte index2 = (block[1] & 0x0f) << 2;
        aWriter.Write(kBase64[index0]);
        aWriter.Write(kBase64[index1]);
        aWriter.Write(kBase64[index2]);
        aWriter.Write('=');
    }
}

void Converter::FromBase64(Bwx& aValue)
{
    TUint bytes = aValue.Bytes();

    TUint j = 0;
    TUint b = 0;
    TByte block[4];

    for (TUint i = 0; i < bytes; i++) {
        
        TByte d = kDecode64[aValue[i]];
        if (d > 64) {
            continue;
        }
        block[b++] = d;
        if (b >= 4) {
            aValue[j++] = block[0] << 2 | block[1] >> 4;
            aValue[j++] = block[1] << 4 | block[2] >> 2;
            aValue[j++] = block[2] << 6 | block[3];
            b = 0;
        }
    }
    
    if (b > 1) {
        aValue[j++] = block[0] << 2 | block[1] >> 4;
    }
    if (b > 2) {
        aValue[j++] = block[1] << 4 | block[2] >> 2;
    }
    
    aValue.SetBytes(j);
}

void Converter::FromXmlEscaped(Bwx& aValue)
{
    TUint j = 0;
    TUint bytes = aValue.Bytes();

    for (TUint i = 0; i < bytes; i++) {
        if (aValue[i] != '&') {
            aValue[j++] = aValue[i];
            continue;
        }
        if (++i < bytes) {
            if (aValue[i] == 'l') {
                if (++i < bytes) {
                    if (aValue[i] == 't') {
                        if (++i < bytes) {
                            if (aValue[i] == ';') {
                                aValue[j++] = '<';
                                continue;
                            }
                        }
                    }
                }
            }
            else if (aValue[i] == 'g') {
                if (++i < bytes) {
                    if (aValue[i] == 't') {
                        if (++i < bytes) {
                            if (aValue[i] == ';') {
                                aValue[j++] = '>';
                                continue;
                            }
                        }
                    }
                }
            }
            else if (aValue[i] == 'a') {
                if (++i < bytes) {
                    if (aValue[i] == 'm') {
                        if (++i < bytes) {
                            if (aValue[i] == 'p') {
                                if (++i < bytes) {
                                    if (aValue[i] == ';') {
                                        aValue[j++] = '&';
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    else if (aValue[i] == 'p') {
                        if (++i < bytes) {
                            if (aValue[i] == 'o') {
                                if (++i < bytes) {
                                    if (aValue[i] == 's') {
                                        if (++i < bytes) {
                                            if (aValue[i] == ';') {
                                                aValue[j++] = '\'';
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (aValue[i] == 'q') {
                if (++i < bytes) {
                    if (aValue[i] == 'u') {
                        if (++i < bytes) {
                            if (aValue[i] == 'o') {
                                if (++i < bytes) {
                                    if (aValue[i] == 't') {
                                        if (++i < bytes) {
                                            if (aValue[i] == ';') {
                                                aValue[j++] = '\"';
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    aValue.SetBytes(j);
}
