/**
*
*  Base64 encode / decode
*  http://www.webtoolkit.info/
*
* Only function used by KinskyPronto are Base64.toIntegerArray
* and  Base64.toIntegerArray0
*
* 'Base64.toIntegerArray0' is a special case performance optimisation
* to handle radio prosets.
*
**/

var base64Chars1 = [];
var reverseBase64Chars1 = [];

var Base64 = {
 
	// public method for decoding
	decode : function (input) {
		var output = "";
		var chr1, chr2, chr3;
		var enc1, enc2, enc3, enc4;
		var i = 0;
 
		// Remove any none Base 64 characters
		//
		// TODO: Do we trust the DS on this ?
		// If we do then we can save quite a bit of time
		// by removing this line of code.
		//
		// CHECK: Need a test case!
		//
#if 1
		var input2 = input.replace(/[^A-Za-z0-9\+\/\=]/g, "");
		
		if (input2 != input){
		  GUI.alert("input2 != input\r\n" + input + "\r\n" + input2);
		};
#endif

		while (i < input.length) {

			enc1 = reverseBase64Chars1[input.charAt(i++)];
			enc2 = reverseBase64Chars1[input.charAt(i++)];
			enc3 = reverseBase64Chars1[input.charAt(i++)];
			enc4 = reverseBase64Chars1[input.charAt(i++)];

                        chr1 = (enc1 << 2) | (enc2 >> 4);

			output += String.fromCharCode(chr1);
 
			if (enc3 != 64) {
				chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
				output += String.fromCharCode(chr2);
			};
			
			if (enc4 != 64) {
				chr3 = ((enc3 & 3) << 6) | enc4;
				output += String.fromCharCode(chr3);
			};
 
		};
 
		return output;
 
	}
 
};

Base64._stringToIntegerArray = function(str){
  var i = 0;
  var result = []
  var res

  while (i < str.length) {
    res = str.charCodeAt(i+3) + (str.charCodeAt(i+2) << 8) + (str.charCodeAt(i+1) << 16) + (str.charCodeAt(i) << 24);
    i += 4;
    result.push(res);
  }
  return result
}

Base64._stringToIntegerArray0 = function(str){
  var i = 0;
  var result = [];
  var res;

  while (i < str.length) {
    res = str.charCodeAt(i+3) + (str.charCodeAt(i+2) << 8) + (str.charCodeAt(i+1) << 16) + (str.charCodeAt(i) << 24);
    i += 4;
    result.push(res);
    if (res == 0) {
      return result;
    };
  }
  return result
}


Base64.toIntegerArray = function(base64String) {

  if (base64String == "") {return []}; // Special case.

  return Base64._stringToIntegerArray(Base64.decode(base64String))
};

// Array can be zero terminated (Not in the 'C' sence)
// If so terminate early.
//
Base64.toIntegerArray0 = function(base64String) {

  if (base64String == "") {return []}; // Special case.

  return Base64._stringToIntegerArray0(Base64.decode(base64String))
};

Base64._init = function(){

  base64Chars1 = [
    'A','B','C','D','E','F','G','H',
    'I','J','K','L','M','N','O','P',
    'Q','R','S','T','U','V','W','X',
    'Y','Z','a','b','c','d','e','f',
    'g','h','i','j','k','l','m','n',
    'o','p','q','r','s','t','u','v',
    'w','x','y','z','0','1','2','3',
    '4','5','6','7','8','9','+','/',
    '='
  ];
 
  for (var i=0; i < base64Chars1.length; i++){
    reverseBase64Chars1[base64Chars1[i]] = i;
  };

};

elab.add("Base64", Base64._init, null);
