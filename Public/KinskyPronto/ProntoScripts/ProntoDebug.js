#ifdef LOGGING

////
//
// ProntoDebug.js
//
// Any errors in this routine will be reported via Diagnostics.log
//
////


var prontoDebug = {}

prontoDebug.socket = null;
prontoDebug.messages = [];
prontoDebug.ok = false;

prontoDebug.onIOError = function(e) {
  Diagnostics.log("prontoDebug socket error [" + e + "]");
  this.close();
  prontoDebug.socket = null;
};


prontoDebug.onClose = function () {
  Diagnostics.log ("prontoDebug socket closed");
  prontoDebug.socket.close();
  prontoDebug.socket = null;
};

prontoDebug.onConnect = function () {

  // If we have connected OK then we can redirect the o/p of diag.log to prontoDebug
  //
  diag.log = prontoDebug.write;
  log_helper.callback = prontoDebug.write;
  timestamp("Logging via prontoDebug.write (now connected)");
};
  
prontoDebug.onTimeout = function(e) {
    Diagnostics.log ("prontoDebug Timeout error [" + e + "]");
    this.close();
    prontoDebug.socket = null;
};

prontoDebug.onData = function(str) {

  var data = this.read();   // .. and discard
  
};

prontoDebug.writeBuffer = "";

prontoDebug.write = function(str){

#if 0
  logLocal(str);
  return;
#endif

  if ((str.length + prontoDebug.writeBuffer.length) > 1000) {
    prontoDebug.write2(prontoDebug.writeBuffer);
    prontoDebug.write2(str);
    prontoDebug.writeBuffer = "";
    return;
  };

  if (prontoDebug.writeBuffer.length <= 0) {
    prontoDebug.writeBuffer = str;
  } else {
    prontoDebug.writeBuffer += ("\r\n" + str);
  };
  
};

prontoDebug.flush = function(){
  if (prontoDebug.writeBuffer.length > 0){
    prontoDebug.write2(prontoDebug.writeBuffer);
    prontoDebug.writeBuffer = "";
  };
};

prontoDebug.write2 = function(str) {

  // diag.logLowLevel('prontoDebug.write('+str+');'); // TODO: - bad infinate recursion as we are diag.log
  
  // The messages[] buffer only hold the last 250 diagnostic strings, if we are going to overflow that
  // then simple throw the head of the array away.
  //
  if (prontoDebug.messages.length > 250) {
    var discardedEntry = prontoDebug.messages.shift();
    Diagnostics.log("Discarding error:");
    Diagnostics.log(discardedEntry);
  };
  
  prontoDebug.messages.push(str);

};

prontoDebug.socketWrite = function() {

  if (!prontoDebug.ok) {return};

  // diag.logLowLevel('prontoDebug.socketWrite('+prontoDebug.messages[0]+');');

  if (prontoDebug.socket == null){diag.logLowLevel("prontoDebug.socket == null"); return;};

  // Try & write as many messages as possible.
  //
  while (true) {
  
    // If we've sent all the messages then return.
    //
    if (prontoDebug.messages.length == 0){return;};

    // Grab the first entry off the message list (Note: non destructive read)
    //
    var str = prontoDebug.messages[0];
  
    if (!prontoDebug.socket.connected) {
      diag.log("!prontoDebug.socket.connected");
      Diagnostics.log(str);
      Diagnostics.log('prontoDebug write error (not connected.) :');
      return;
    };


    try {
        prontoDebug.socket.write(str+"\r\n");
    } catch (e) {
        Diagnostics.log(e);
        Diagnostics.log('prontoDebug write error :');
        prontoDebug.socket = null;
	return;
    };
  
    // if we get here then we've sucessfully sent the first entry so take it off the front of the array & discard.
    //
    var discardedEntry = prontoDebug.messages.shift();
  
  };
}

prontoDebug.initialiseSocket = function (ipAddress){

  if (prontoDebug.socket != null) {return}; // Already done

  diag.log("prontoDebug.initSocket("+ipAddress+")");
  
  try {
    prontoDebug.socket = new TCPSocket(false); // false == async socket
  } catch (e) {
    prontoDebug.socket = null;
    Diagnostics.log("Unable to create debug socket.");
    prontoDebug.write2 = logLocal;
    return;
  };

  prontoDebug.socket.onData = prontoDebug.onData;
  prontoDebug.socket.onClose = prontoDebug.onClose;
  prontoDebug.socket.onIOError = prontoDebug.onIOError;
  prontoDebug.socket.onTimeout = prontoDebug.onTimeout;
  prontoDebug.socket.onConnect = prontoDebug.onConnect;
  
  try {
    prontoDebug.socket.connect(ipAddress, 23, 1000);
  } catch (e) {
    Diagnostics.log('prontoDebug Error trying to connect :' + e);
    prontoDebug.socket.close();
    prontoDebug.socket = null;
    prontoDebug.write2 = logLocal;
  };
};

prontoDebug.setDebug = doNothing;

var doDebug = doNothing;

// tick() & tock() act together to allow a very low priority background task to execute.
// tock() can be called by tick() or any other procedure to see if any debug messages
// need printing & if so to try & print them.
//
prontoDebug.tock = function(){

  // diag.log("prontoDebug.tock()");
  prontoDebug.flush();

  // If nothing to do then return
  //
  if (prontoDebug.messages.length == 0) {return;};
  
  prontoDebug.socketWrite();
  
};

prontoDebug.tick = function(){

  // diag.log("prontoDebug.tick()");

  try {
    prontoDebug.tock();
  } catch (e) {
    diag.log('prontoDebug.tick e:');
    diag.log(e);
  };

  try
  {
    Activity.scheduleAfter(1000, prontoDebug.tick); // check every second.
  } catch (e) {
    GUI.alert("Activity.scheduleAfter[6] " + e);
    LOG("Activity.scheduleAfter[6] " + e);
  };
  

};


//prontoDebug.okToWrite = function(){
//
//  timestamp('OK To Write Now');
//  prontoDebug.ok = true;
//
//};

prontoDebug._start = function(){

#if 0
  diag.log = prontoDebug.write;
  return;
#endif

  prontoDebug.initialiseSocket(QUOTEME(DEBUG_IP));

  prontoDebug.write("prontoDebug Startup");
  prontoDebug.tick();

  // disable logging hold-off as it causes messages to get lost and has no
  // noticable affects on performance (still takes ~2s to startup due to
  // socket opening delay)
  //
  // scheduleAfter(20000, prontoDebug.okToWrite); // Do nothing for first 20 seconds
  prontoDebug.ok = true;
};


elab.add("prontoDebug", null, prontoDebug._start);

#endif

