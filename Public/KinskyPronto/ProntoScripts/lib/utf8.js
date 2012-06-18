////
//
// UTF-8 Encode & Decode functions.
//
// For a nice overview of UTF-8 see the Wikipedia article here:
//
//  http://en.wikipedia.org/wiki/UTF-8
//
////

/*
function utf8_encode(s){
        for(var c, i = -1, l = (s = s.split("")).length, o = String.fromCharCode; ++i < l;
            s[i] = (c = s[i].charCodeAt(0)) >= 127 ? o(0xc0 | (c >>> 6)) + o(0x80 | (c & 0x3f)) : s[i]
        );
        return s.join("");
};

function utf8_decode(s){
        for(var a, b, i = -1, l = (s = s.split("")).length, o = String.fromCharCode, c = "charCodeAt"; ++i < l;
            ((a = s[i][c](0)) & 0x80) &&
            (s[i] = (a & 0xfc) == 0xc0 && ((b = s[i + 1][c](0)) & 0xc0) == 0x80 ?
            o(((a & 0x03) << 6) + (b & 0x3f)) : o(128), s[++i] = "")
        );
        return s.join("");
};

function utf8_test(){
  var s = "aáéíóúe";

  LOG('\uF087' + String.fromCharCode(0xF087))
  LOG('utf8_encode("' + s + '") = ', utf8_encode(s));
  LOG('utf8_decode(UTF8.encode("' + s + '"))) = ' + utf8_decode(utf8_encode(s)));
  LOG('utf8_decode(UTF8.encode("' + s + '"))) = ' + utf8_decode(utf8_encode(s)));
};


*/

var utf8 = {};

utf8.charCode = function(ch) {
  return String.fromCharCode(ch);

  if (ch < 128) {
    return String.fromCharCode(ch);
  };
  
  return '*';
  
};

utf8.decode = function (utftext) {
        var string = "";
        var i = 0;
        var c = c1 = c2 = 0;
	var ch = 0;
	
        while ( i < utftext.length ) {

            c = utftext.charCodeAt(i);

            if (c < 128) {
                string += utf8.charCode(c);
                i++;
            }
            else if((c > 191) && (c < 224)) {
                c2 = utftext.charCodeAt(i+1);
		ch = ((c & 31) << 6) | (c2 & 63);
                string += utf8.charCode(ch);
                i += 2;
            }
            else {
                c2 = utftext.charCodeAt(i+1);
                c3 = utftext.charCodeAt(i+2);
		ch = ((c & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63);
                string += utf8.charCode(ch);
                i += 3;
            }

        }

        return string;
};

elab.add('utf8');

var REGEXP_UTF8 = /[\x80-\xFF]/;

function decodeUTF8(aBinary)
{
    var result,
        plainStart,
        i,
        found,
        charCode;
    trace("decodeUTF8", arguments);
    REGEXP_UTF8.lastIndex = 0;
    found = aBinary.search(REGEXP_UTF8);
    if (found < 0) {
      return aBinary;
    }
    result = "";
    plainStart = 0;
    i = 0;
    for (; found >= 0; found = aBinary.substr(i).search(REGEXP_UTF8)) {
      result += aBinary.substr(plainStart, found);
      i += found;
      charCode = aBinary.charCodeAt(i);
      if ((charCode >= 0xC0) && (charCode < 0xE0)) {
        charCode = ((charCode & 0x1f) << 6) | (aBinary.charCodeAt(i + 1) & 0x3f);
        i += 2;
      } else if (charCode < 0xF0) {
        charCode = ((charCode & 0x0f) << 12) |
                   ((aBinary.charCodeAt(i + 1) & 0x3f) << 6) |
                   (aBinary.charCodeAt(i + 2) & 0x3f);
        i += 3;
      } else if (charCode < 0xF8) {
        charCode = ((charCode & 0x07) << 18) |
                   ((aBinary.charCodeAt(i + 1) & 0x3f) << 12) |
                   ((aBinary.charCodeAt(i + 2) & 0x3f) << 6) |
                   (aBinary.charCodeAt(i + 3) & 0x3f);
        i += 4;
      } else {
        throw new HttpError("Invalid UTF-8 sequence");
      }
      if (charCode < 0x10000) {
        result += String.fromCharCode(charCode);
      } else {
        // use surrogate pairs to represent high code points
        result += String.fromCharCode((charCode >> 10) | 0xD800) +
                  String.fromCharCode((charCode & 0x3FF) | 0xDC00);
      }
      plainStart = i;
    }
    result += aBinary.substring(plainStart);
    return result;
};

var UTF8 = function(a){

  var b="";
  var i=0;

  for(i=0; i < a.length; i++){
    
    if(a.charCodeAt(i)>127){
      System.print("a.charCodeAt("+i+") ["+a.charCodeAt(i)+"]");
      if((i < a.length-1) && (a.charCodeAt(i)>=194 && a.charCodeAt(i)<=223) && ((a.charCodeAt(i+1) & 192)==128) ) {
        b += String.fromCharCode((64*(a.charCodeAt(i) & 31) + (a.charCodeAt(i+1) & 63)) & 255); i++;
      } else {
        System.print("No mapping ["+a.charCodeAt(i)+"]");
        b += a[i];
      }
    } else {
      b += a[i];
    }
  };
  return b;
}

/*

var utf8 = {

    // public method for url encoding
    encode : function (string) {
        string = string.replace(/\r\n/g,"\n");
        var utftext = "";

        for (var n = 0; n < string.length; n++) {

            var c = string.charCodeAt(n);

            if (c < 128) {
                utftext += String.fromCharCode(c);
            }
            else if((c > 127) && (c < 2048)) {
                utftext += String.fromCharCode((c >> 6) | 192);
                utftext += String.fromCharCode((c & 63) | 128);
            }
            else {
                utftext += String.fromCharCode((c >> 12) | 224);
                utftext += String.fromCharCode(((c >> 6) & 63) | 128);
                utftext += String.fromCharCode((c & 63) | 128);
            }

        }

        return utftext;
    },

    // public method for url decoding
    decode : function (utftext) {
        var string = "";
        var i = 0;
        var c = c1 = c2 = 0;

        while ( i < utftext.length ) {

            c = utftext.charCodeAt(i);

            if (c < 128) {
                string += String.fromCharCode(c);
                i++;
            } else if((c > 191) && (c < 224)) {
                c2 = utftext.charCodeAt(i+1);
                string += String.fromCharCode(((c & 31) << 6) | (c2 & 63));
                i += 2;
            } else {
                c2 = utftext.charCodeAt(i+1);
                c3 = utftext.charCodeAt(i+2);
                string += String.fromCharCode(((c & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63));
                i += 3;
            };
        };
        return string;
    };
};

*/
