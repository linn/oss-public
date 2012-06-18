////
//
// diag.js (ProntoScript variant)
//
//
////
var diag = {};

#ifdef LOGGING

diag.logLowLevel = function(msg){

  logLocal(msg);
};

diag.log = diag.logLowLevel;

//
// Logging for long (> 80 character) strings.
//
// uses: log
//
diag.logMessage = function(msg){

  var width = 80;
  
  for (var i=0; i < msg.length; i += width) {
    diag.log(msg.substr(i, width));
  };
};

#else

diag.log = function(msg){};
diag.logLowLevel = function(msg){};
diag.logMessage = function(msg){};

#endif
// fatal error handing.
//
// usage: diag.fatal("Error message");
//
// You can assume that you will not return from this error.
//
// uses: useragent.js
//
// TODO: Take a look at diag.html.js for a nicer way to do this in Firefox...
//
// We can't log errors until the program has finished initialisation and we have
// displayed the opening screen.
//

// Set Pronto specific debug mask.
//
#if 0
System.setDebugMask(9);
#endif
System.setDebugMask(1);

diag.failed = false;

diag.fatal = function(fatalErrorText) {

  // TODO: This routine should be changed to use the newer GUI.alert(string) facility.
  //
  Diagnostics.log("Fatal Error:");
  Diagnostics.log(fatalErrorText);

  // Only allow one fatal error per session.
  // [But we do always log them via Diagnostics.log, so could be some cascading]
  //
  if (diag.failed) {
    return;
  };

  diag.failed = true;

  GUI.alert(fatalErrorText);
};


diag.checkForFailure = function(){
  
  return diag.failed;

};

#if 0
// TODO: Only enable me when testing on simulator as disables prontoDebug.
//
log_helper.callback = logLocal;
#endif