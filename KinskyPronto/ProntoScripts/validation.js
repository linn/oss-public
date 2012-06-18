////
//
// validation.js
//
// TODO: Move to common code
//
// change `.validIP` to return a string with error code.
// The we can write code like
//
// errorTxt = ..validIP(192.186..);
//
// if (errorTxt.length > 0) {...failure code...};
//
// It would save on a pointless callback routine.
//
////
validation = {};

validation.validIP = function (ipAddress, errorCallback) {
  
  var ipPattern = /^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$/;
  var ipArray = ipAddress.match(ipPattern);

  if (ipAddress == "0.0.0.0") {
    errorCallback ("The IP address 0.0.0.0 is the default network IP address and can't be used.");
    return;
  };

  if (ipAddress == "255.255.255.255") {
    errorCallback ("The IP address 255.255.255.255 is the broadcast IP address and can't be used.");
    return;
  };

  if (ipAddress == "127.0.0.1") {
    errorCallback ("The IP address 127.0.0.1 is the loopback IP address and can't be used.");
    return;
  };

  if (ipArray == null) {
    errorCallback ('"' + ipAddress+'" is not a valid IP address.');
    return;
  };
  
  // doAlert (ipArray[1] +":" +ipArray[2] +":" +ipArray[3]+":" +ipArray[4])
  
  for (i = 1; i < 5; i++) {
    thisSegment = ipArray[i];
    if (thisSegment > 255) {
      errorCallback ('"' + ipAddress + '" is not a valid IP address (byte > 255).');
      return;
    };

    if ((i == 1) && (thisSegment >= 224)) {
      errorCallback ('"' + ipAddress + '" is not a valid IP address (Experimental / reserved).');
      return;
    };
  };

};

#if 0

validation.test = function(){

  validation.validIP("192.168.1.23");
  validation.validIP("0.0.0.0");
  validation.validIP("255.255.255.255");
  validation.validIP("fred");
  validation.validIP("");
  validation.validIP("127.234.12.25a");
  validation.validIP("127.333.34.45");
  validation.validIP("127.33.34.256");
  validation.validIP("224.15.34.45");
  
};

// validation.test();

#endif
