#ifdef LOGGING

////
//
// log.js
//
////

var log_helper = {callback: null,
                  messages: []};

function log(str){

  // Do we now have a callback routine?
  //
  if (log_helper.callback != null) {
  
    // Are there any outstanding messages to send?
    //
    if (log_helper.messages.length > 0) {
      for (var i = 0; i < log_helper.messages.length; i++){
        log_helper.callback(log_helper.messages[i]);
      };
      log_helper.messages = [];
    };

    log_helper.callback(str.slice(0,10000));
    return;
  };
  
  // No callback routine, so save the message for later.
  //
  log_helper.messages.push(str);

};

// logLocal logs messages local to the Pronto, no buffering or writing to ProntoMonitor etc
// will take place.
//
function logLocal(str){

  Diagnostics.log(str);

#if 1
  System.print(str);
#endif

};

#endif
