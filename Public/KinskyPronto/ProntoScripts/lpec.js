////
//
// lpec.js
//
// Linn Protocol for Eventing and Control (LPEC)
//
// For more details on LPEC, see http://docs.linn.uk.co/wiki/index.php/Developer:LPEC
// Note: Currently only five LPEC session is allowed per DS.
//
// Other packages used:
//  configuration.dsIPAddress, .preAmpType
//  utils.htmlDecode
//  elab.add
//  gotoHomePage();
//  macro: LOG, TIMESTAMP
////

/*
Play, Pause, Skip a DS player using the UPnP AVTransport service:

    ACTION MediaRenderer/AVTransport 1 Play "0" "1"
    ACTION MediaRenderer/AVTransport 1 Pause "0"
    ACTION MediaRenderer/AVTransport 1 Next "0"
    ACTION MediaRenderer/AVTransport 1 Previous "0"

Get the volume of a Linn pre-amplifier using the Linn Preamp service

    ACTION Preamp/Preamp 1 Volume
    RESPONSE "40"

Set mute on a Linn pre-amplifier using the Linn Preamp service

    ACTION Preamp/Preamp 1 SetMute "true"
    RESPONSE 
    
/*
ALIVE Ds 4c494e4e-0050-c221-7ccf-ec000003013f
ALIVE Preamp 4c494e4e-0050-c221-7ccf-ec0000030133
ALIVE MediaRenderer 4c494e4e-0050-c221-7ccf-ec0000030171

ALIVE Ds 4c494e4e-0050-c221-7ccf-ec000003013f
ALIVE Preamp 4c494e4e-0050-c221-7ccf-ec0000030133
ALIVE MediaRenderer 4c494e4e-0050-c221-7ccf-ec0000030171
SUBSCRIBE Ds/Ds
SUBSCRIBE 61
EVENT 61 0 SupportedProtocols "http-get:*:audio/x-flac:*,http-get:*:audio/wav:*,http-get:*:audio/wave:*,http-get:*:audio/x-wav:*,http-get:*:audio/mpeg:*,http-g"
SUBSCRIBE Ds/Playlist
SUBSCRIBE 62
EVENT 62 0 IdArray "AAA....."

Below here work with Cara

SUBSCRIBE Ds/Product
SUBSCRIBE Ds/Playlist
SUBSCRIBE Ds/MediaTime
SUBSCRIBE Preamp/Preamp
*/

var doNothing = function(){};
var lpec = {};


//------------------------------------------------------------------------------
// LPEC PUBLIC interface
//------------------------------------------------------------------------------

lpec.addEventCallback = function(callback, match, subDevice, service){
  // If subdevice & service are omitted (null) then we will match any service event.
  // This is advantageouse in the case of "Volume", "Mute" & "ProductSourceXml"
  // where we don't need to know which version (Preamp or DS) we've subscribed to.
  if (match == null)     {match = ""};
  if (subDevice == null) {subDevice = ""};
  if (service == null)   {service = ""};

  if (match == "") {
    LOG("Match is null, shouldn't happen, failing.");
    throw new Error("Match can't be empty string in lpec.addEventCallback");
    return;
  }

  var theService = subDevice + "/" + service;
  if (theService == '/') {
    theService = "";
  }

  LOG(match+" ("+theService+") LPEC Evt added");
  lpec.eventCallbacks.push({callback: callback, match: match, service: theService});

  for (var i in lpec.events) {
    var event = lpec.events[i];
    if (event.match == match) {
      if ((theService == "") ||
        (theService == event.service)) {
        LOG("Existing match found");
        callback(event.value);  
      }
    }
  }
}


lpec.action = function (request, callback, errorCallback, decodeXML){
  if (callback == null) {callback = doNothing;};
  if (errorCallback == null) {errorCallback = doNothing;};
  if (decodeXML == null) {decodeXML = true;}
  
  lpec.write("ACTION " + request, 
         lpec.REQUEST_TYPE.ACTION, decodeXML, callback, errorCallback);
}


//------------------------------------------------------------------------------
// LPEC PRIVATE methods and data - there should be no requirement to access any
// data or methods below this point from outside this module
//------------------------------------------------------------------------------

lpec.ipAddress = "{ip-address-undefined}";
lpec.telnetPort = 23;

lpec.initialised = false;
lpec.previouslyConnectedToDs = false;

lpec.actSocket = null;
lpec.currentRequest = null;
lpec.outstandingRequests = [];

lpec.evtSocket = null;
lpec.evtData = "";
lpec.evtErrorCount = 0;
lpec.events = [];
lpec.eventCallbacks = [];
lpec.subscriptions = [];

lpec.wifi = true;
lpec.sleep = false;

lpec.REQUEST_TYPE = {SUBSCRIBE: 0, UNSUBSCRIBE:1, ACTION: 2};


//------------------------------------------------------------------------------
// Socket initialisation
//
// This creates 2 sockets:
//	- evtSocket is an ASYNC socket to handle events and their subscriptions
//	- actSocket is SYNCHRONOUS socket to handle actions
//
// These are seperated to help with responsiveness to ACTIONS (where it is
// most sugnificant to end-user. Async sockets 'onData' callback appears to
// be restricted to approx. 2 calls/sec no matter hw fast data is Rx. Hence
// this severely limits responsiveness on sequences of messages (such as 
// adding tracks to a playlist). There does not seem to be any way to bypass
// this restriction as the 'onData' calls are generated even if the data was
// removed from input buffer before 'onData' is called. Using synchronous 
// socket removes this limitation, bit the call does BLOCK between the socket
// write and read
//------------------------------------------------------------------------------

lpec.initialiseActionSocket = function (ipAddress){
  TIMESTAMP("lpec.initialiseActionSocket("+ipAddress+")");
  lpec.ipAddress = ipAddress;
  try {
    lpec.actSocket = new TCPSocket(true); // true = sync socket
    lpec.actSocket.connect(ipAddress, lpec.telnetPort, 250);
  } catch (e) {
    lpec.actSocket = null;
    LOG("Unable to create LPEC action socket")
    return false;
  }
  return true;
}


lpec.initialiseEventSocket = function (ipAddress){
  TIMESTAMP("lpec.initialiseEventSocket("+ipAddress+")");
  lpec.ipAddress = ipAddress;
  try {
    lpec.evtSocket = new TCPSocket(false); // false = async socket
    lpec.evtSocket.onData = lpec.onEvtData;
    lpec.evtSocket.onClose = lpec.onEvtClose;
    lpec.evtSocket.onIOError = lpec.onEvtIOError;
    lpec.evtSocket.onTimeout = lpec.onEvtTimeout;
    lpec.evtSocket.onConnect = lpec.onEvtConnect;
    lpec.evtSocket.connect(ipAddress, lpec.telnetPort, 250);
  } catch (e) {
    lpec.evtSocket = null;
    LOG("Unable to create LPEC event socket");
    return false;
  }
  return true;
}


lpec.initialisationComplete = function(){	
  // Now called from evtSocket 'onConnect' callback	
  // Waits for evtSocket to be connected and communicating, and then 
  // clears out the LPEC startup data (which is ignored here as it is handled
  // by the LPEC startup data from the action socket
  var data;
  var started = false;
  var done    = false;
  while (done==false) {
    try {
      data = lpec.actSocket.read( 1024, 100 );
      started = true;
    } catch (e) {
      if (started==true) {
        done = true;
      }
    }
  }
  // set initialised flag and start backgroung write task
  lpec.initialised = true;
  lpec.writeTaskTask();
}


//------------------------------------------------------------------------------
// Write data to DS (and receive response from action socket)
//------------------------------------------------------------------------------

lpec.writeTask = function(){
  if (lpec.currentRequest != null) {	// single currentRequest across both sockets !!
    return;
  }

  if (!lpec.initialised) {
    return;
  }

  if (lpec.outstandingRequests.length == 0){
    return;
  }

  var request = lpec.outstandingRequests.shift();
  
  if (lpec.sleep){
    // throw away any requests whilst sleeping  
    return;
  }
    
  switch (request.requestType) {
    case lpec.REQUEST_TYPE.SUBSCRIBE:
    case lpec.REQUEST_TYPE.UNSUBSCRIBE:
      try {
        lpec.currentRequest = request;
        lpec.evtSocket.write(request.message+"\r\n"); 
        TIMESTAMP(" >>> LPEC EVT Tx: ["+request.message+"]");
      } catch (e) {
        LOG("LPEC event write error: "+e);
        lpec.evtSocket.close();
        lpec.evtSocket = null;
        lpec.currentRequest = null;
      }    	
      break;
      
    case lpec.REQUEST_TYPE.ACTION:
      try {
        lpec.currentRequest = request;
        lpec.actSocket.write(request.message+"\r\n"); 
        TIMESTAMP(" >>> LPEC ACT Tx: ["+request.message+"]");
      } catch (e) {
        LOG("LPEC action write error: "+e);
        lpec.actSocket.close();
        lpec.actSocket = null;
        lpec.currentRequest = null;
      }   

      // sync socket - need to do the read
      var data = ""
      while (data.lastIndexOf("\r\n") == -1) {
        try {
          data += lpec.actSocket.read( 4096, 20 );
        } catch (e) {}	// add a timeout here ????
      }
      data = data.slice(0,-2);
      TIMESTAMP(" <<< LPEC ACT Rx: " + data.slice(0,5000));
      lpec.handleDsResponse(data);
      break;
      
    default:
      LOG("LPEC Tx invalid request type: "+request.requestType);
  }	
}
     

lpec.writeTaskTask = function(){
  try {
    lpec.writeTask();
  } catch (e) {
    LOG('lpec.writeTask() error: '+e);
  };
  
  try
  {
    Activity.scheduleAfter(10, lpec.writeTaskTask);
  } catch (e) {
    GUI.alert("Activity.scheduleAfter[2] " + e);
    LOG("Activity.scheduleAfter[2] " + e);
  };  

}


lpec.write = function(str, requestType, decodeXML, callback, errorCallback){
  if (errorCallback == null) {errorCallback = doNothing;};
  if (callback == null) {callback = doNothing;};

  var request = {message: str, requestType: requestType, decodeXML: decodeXML, callback: callback, errorCallback: errorCallback};
  lpec.outstandingRequests.push(request);
  lpec.writeTask();
}


//------------------------------------------------------------------------------
// Process incoming data on action socket
// NOTE that for legacy reasons, solicited data on event socket is also handled
// within this code (passed in from event socket handler)
//------------------------------------------------------------------------------

lpec.handleDsResponse = function (str){
  // Because of the way we handle sockets we often get null data passed, not a
  // bug but a feature. For simplicity's sake simply ignore this.
  if (str == "") {return;};

  // The response we get back from the DS can be one of the following:
  //
  // Action Response
  //	RESPONSE "[outarg1]" "[outarg2]" ... "[outargn]"
  // Subscribe Response
  //	SUBSCRIBE [subscription-id]
  // Initial Event
  //	EVENT [subscription-id] 0 [var1-name] "[var1]" [var2-name] "[var2]" ... [varn-name] "[varn]"
  // Event
  //	EVENT [subscription-id] [sequence-no] [var1-name] "[var1]" [var2-name] "[var2]" ... [varn-name] "[varn]"
  // Unsubscribe Response
  //	UNSUBSCRIBE [subscription-id]
  // Sub-Device Enabled
  //	ALIVE [sub-device] [udn]
  // Sub-Device Disabled
  //	BYEBYE [sub-device] [udn]
  // Error
  //	ERROR [code] "[description]"
  //
  // These split into two classes in-bound and out-of-bound (oob)
  //
  // ib: RESPONSE, SUBSCRIBE, UNSUBSCRIBE, ERROR
  // oob: EVENT, ALIVE, BYEBYE
  //
  // RESPONSEs, SUBCRIBEs, UNSUBSCRIBEs, ERRORs are Rx on actSocket SYNChronously
  // EVENTs are Rx on evtSocket ASYNChronously
  // ALIVEs, BYEBYEs will be Rx by both sockets
  //
  var index = str.indexOf(" ");
  var response = []
  if (index == -1) {
    response = [str, ""]
  } else {
    response = [str.slice(0, index), str.slice(index+1)]
  }

  // Send next message (if available) BEFORE handling incoming response
  var request = lpec.currentRequest;
  if (response[0]=="RESPONSE" || response[0]=="SUBSCRIBE" || response[0]=="ERROR" ) {
    lpec.currentRequest = null
  }
  lpec.writeTask();  

  switch (response[0]) {
    case "RESPONSE":
      lpec.handleLpecResponse(response[1], request);
      break;
    case "SUBSCRIBE":
      lpec.handleLpecSubscribe(response[1], request);
      break;
    case "EVENT":
      lpec.handleLpecEvent(response[1]);
      break;
    case "UNSUBSCRIBE":
      lpec.handleLpecUnSubscribe (response[1]);
      break;
    case "ALIVE":
      lpec.handleLpecAlive(response[1]);
      break;
    case "BYEBYE":
      lpec.handleLpecByebye(response[1]);
      break;
    case "ERROR":
      lpec.handleLpecError(response[1], request);
      break;
    default: 
      LOG("LPEC bad response: ["+response[0]+"]");
  }
}


lpec.handleLpecResponse = function (str, req){
  // We should only get a RESPONSE (or ERROR) from an LPEC ACTION request
  // So package up the RESPONSE items into a (possibly null) array and return that.
  //	
  // This has been changed so that the automatic (& slow) decoding of the
  // returned XML is optional. Note that the XML decode can only be enabled and 
  // disabled for ALL the paramters of the message.
  var result = [];
  while (str.length > 0) {
    var value;
    var index_1 = str.indexOf('"');
    var index_2 = str.indexOf('"', index_1+1);
    if (req.decodeXML) {
      value = utils.htmlDecode(str.slice(index_1+1, index_2));
    } else {
      value = str.slice(index_1+1, index_2);
    }
    str = str.slice(index_2+2); // Skip space
    result.push(value);
  }

#if 0
  LOG("Res to:" + req.message);
  for (var i = 0; i < result.length; i++){
    LOG("Res["+i+"]" +result[i]);
  }
#endif

  try {
    req.callback(result);
  } catch (e) {
    LOG('Err in lpec callback: '+e);
  }
}


lpec.handleLpecError = function (str, req){
  LOG("lpec:ERROR["+str+"]");
  try {
    req.errorCallback(str);
  } catch (e) {
    LOG('Err in lpec callback.3'+e)
  }
}


lpec.handleLpecAlive = doNothing;
lpec.handleLpecByebye = doNothing;


//------------------------------------------------------------------------------
// Process incoming data on event socket
//------------------------------------------------------------------------------

lpec.onEvtData = function() {
  TIMESTAMP("lpec.onEvtData+")
  // If we get here then the socket must be up, so reset the error count.
  lpec.evtErrorCount = 0;
  var crLf = "\r\n";

  try {
    lpec.evtData += lpec.evtSocket.read();
  } catch (e) {
    lpec.evtData = "";
  }
    
  lpec.previouslyConnectedToDs = true;	  
  var index = lpec.evtData.lastIndexOf(crLf);
  LOG("{lastIndexOf:"+index+":.data.length"+lpec.evtData.length+"}");
      
  if ((index + 2) == lpec.evtData.length) {
    var splitData = lpec.evtData.split(crLf);
    lpec.evtData = "";
  
    for(var i = 0; i < splitData.length; i++){
      // Logger hangs if message length > 13033 so truncate logged message
      TIMESTAMP(" <<< LPEC EVT Rx: " + splitData[i].slice(0,5000));
      lpec.handleDsResponse(splitData[i]);
    }
  }
  TIMESTAMP("lpec.onEvtData-");
}


lpec.onEvtConnect = function () {
  TIMESTAMP("lpec.onEvtConnect+OK- ("+lpec.evtSocket.connected+")*******");
  if(!lpec.previouslyConnectedToDs){
    lpec.previouslyConnectedToDs = true;
    lpec.initialisationComplete();
  }
}


lpec.onEvtIOError = function(e) {
  LOG("LPEC event socket error [" + e + "]");
  lpec.closeEvtSocket();
  if (lpec.previouslyConnectedToDs) {
    gotoHomePage()
  }
}


lpec.onEvtClose = function () {
  LOG("LPEC event socket closed");
  lpec.closeEvtSocket();
}


lpec.onEvtTimeout = function(e) {
  LOG("LPEC event Timeout error [" + e + "]");
  lpec.closeEvtSocket();
  if (lpec.previouslyConnectedToDs) {
    gotoHomePage();
  }
}

lpec.closeEvtSocket = function() {
  lpec.evtSocket.close();
  lpec.evtSocket = null;
  lpec.currentRequest = null;
}


//------------------------------------------------------------------------------
// Handle incoming events from DS
//------------------------------------------------------------------------------

lpec.handleLpecEvent = function (str){
  var subId = lpec.parseLpecEventSubId(str);
  var result = lpec.parseLpecEvent(str);
  var service = lpec.subscriptions[subId].service;  
  LOG("Event for subscriptionId[" + subId + "] = ["+service+"]" );

  if (result.length == 0){
    LOG("lpec: Null Event");
    return;
  }
    
  for (var i = 0; i < result.length; i++){    
    var event = {service: service, match: result[i].name, value: result[i].value};    
    lpec.events[result[i].name] = event; // result[i].value;
    try {
      LOG("Event ["+result[i].name+":"+service+"] = '" + result[i].value + "'");
      lpec.doSingleEventCallback(event); // result[i].name, result[i].value, service)
    } catch (e) {
      LOG('Err in lpec callback.3: '+e)
    }
    LOG("Evt:"+result[i].name+":"+result[i].value);
  } 
}


lpec.doSingleEventCallback = function(event){
  //  event = {service: service, match: result[i].name, value: result[i].value};
  TIMESTAMP("doSingleEventCallback("+event.match+","+event.service+","+event.value+")");

  for (var i=0; i<lpec.eventCallbacks.length; i++){
    // {callback: callback, match: match, service: theService}
    var theCallback = lpec.eventCallbacks[i];
    if (theCallback.match == event.match) {
      try {
        if (theCallback.service == "") {
          theCallback.callback(event.value);
        } else if (theCallback.service == event.service) {
          LOG("Matched; services equal ["+theCallback.service+"]");
          TIMESTAMP("doSingleEventCallback(callback)+");
          theCallback.callback(event.value);
          TIMESTAMP("doSingleEventCallback(callback)-");
        } else {
          LOG("Didn't match with ["+theCallback.service+"]");
        }
      } catch(e) {
        // Do nothing
        LOG("Error in callback:");
        LOG(e);
      }
    }
  }
  TIMESTAMP("doSingleEventCallback-");
}


lpec.parseLpecEventSubId = function (str){
  var index_1 = str.indexOf(" ");
  var subscriptionId = str.slice(0, index_1);
  return subscriptionId;
}


lpec.parseLpecEvent = function (str){
  var result = [];
  var name = "";
  var value = "";
  var index_1 = 0;
  var index_2 = 0;
    
  index_1 = str.indexOf(" ");
  index_2 = str.indexOf(" ", index_1+1);  
  var subscriptionId = str.slice(0, index_1);  
      
  str = str.slice(index_2+1);  
  LOG("Event["+str+"], sub Id ["+subscriptionId+"]");
      
  while (str.length > 0) {
    index_1 = str.indexOf(" ");
    if (index_1 == -1) {
      return result
    } else {
      name = str.slice(0, index_1);
      index_1 = str.indexOf('"');
      index_2 = str.indexOf('"', index_1+1);
      value = utils.htmlDecode(str.slice(index_1+1, index_2)); // New code
      str = str.slice(index_2+2); // Skip space
      result.push({name: name, value: value});
    } 
  }  
  return result;
}


//------------------------------------------------------------------------------
// Event subscription / unsubscription
//------------------------------------------------------------------------------

lpec.subscribeAll = function()
{
	TIMESTAMP("lpec.subscribeAll+");

  // We should always have the following 3 'base' subscriptions available.
  // Even though there are 5!
  //
  // NOTE: Ok to fail for Ds/Radio & Ds/Time don't exist.
  //
#ifdef DAVAAR
  lpec.subscribe("Ds", "Info");
#else
  lpec.subscribe("Ds", "Ds");
  lpec.subscribe("Ds", "Info");
#endif

#ifdef DAVAAR
#else
#endif

  if (pronto.variant == pronto.VARIANT_TYPE.DS)
  {
#ifdef DAVAAR
    lpec.subscribe("Ds", "Time");
#else
    lpec.subscribe("Ds", "MediaTime");
#endif
    lpec.subscribe("Ds", "Playlist");
  };

  if (pronto.variant == pronto.VARIANT_TYPE.RADIO)
  {
    lpec.subscribe("Ds", "Radio");
    lpec.subscribe("Ds", "Time");
  };

  // Need to subscribe to Ds/Product or Preamp/Product - but not both.
  // This will give us the correct ProductSourceXml for later use.
  // Either will give us the correct StandBy notification.
  //
  /*
   * How do we do subscriptions with error handling if the service
   * isn't available? i.e. Cara Radio, preamp etc
   */
  lpec.subscribe("Ds", "Product"); // Standby notification

  if (configuration.preAmpType == 2) // 2 = External
  {
#ifdef DAVAAR
#else
#endif
    lpec.subscribe("Preamp", "Product"); // Standby notification & Source XML
  };

  // Volume, Mute
  // -------------
  //
  // Would it be the same if we subscribed to Ds/Preamp?
  // I would think so - except that there isn't a Ds/Preamp service!
  // ToDo: investigate the above.
  //
  if (configuration.preAmpType == 1) // 1 = Internal
  {
#ifdef DAVAAR
    lpec.subscribe("Ds", "Volume");
#else
    lpec.subscribe("Ds", "Preamp");
#endif
  }
  else if (configuration.preAmpType == 2) // 2 = External
  {
#ifdef DAVAAR
    lpec.subscribe("Preamp", "Volume");
#else
    lpec.subscribe("Preamp", "Preamp");
#endif
  };
  TIMESTAMP("lpec.subscribeAll-");
};


lpec.subscribe = function (subDevice, service){
  var serviceName = subDevice + '/' + service;
  LOG("Subscribing to " + serviceName);

  // Inner functions to the rescue.
  //
  function success(res){  
    var result = {};
    result.service = serviceName;
    result.subscription = res;
    lpec.subscriptions[res] = result;    
    LOG("Subscribed to ["+serviceName+"] as ["+res+"]");
  }
    
  function failure(res){
    LOG("Failed to subscribe to ["+serviceName+"] err ["+res+"]");
  }

  lpec.write("SUBSCRIBE " + subDevice + "/" +service,
    lpec.REQUEST_TYPE.SUBSCRIBE, true, success, failure);
}


lpec.unSubscribeAll = function(){
  if( lpec.subscriptions.length > 0 ){
    LOG("Unsubscribing from ALL events");
    lpec.write("UNSUBSCRIBE",lpec.REQUEST_TYPE.UNSUBSCRIBE);
    lpec.subscriptions = [];
  }
}


lpec.handleLpecSubscribe = function (str, req){
  LOG("Subscribed Id = ["+str+"]");
  try {
    req.callback(str);
  } catch (e) {
    LOG('Err in lpec callback for SUBSCRIBE: '+e);
  } 
}


lpec.handleLpecUnSubscribe = function( str ){
  lpec.currentRequest = null;
}


//------------------------------------------------------------------------------
// Handle sleep and netlink (wifi) events
//------------------------------------------------------------------------------

lpec.onSleep = function(){
  // When sleeping unsubscribe from events
  TIMESTAMP("Sleeping");
  if( lpec.evtSocket != null){
    lpec.unSubscribeAll();
  };
  lpec.sleep = true;
}


lpec.onWake = function(){
  // On wake re-subscribe to events
  // Note that WiFi will drop once sleeping, but the relevant netlink event is
  // lost (probably because WiFi is dropped), hence need to check wifi status
  // here and act accordingly
  var status = System.getNetlinkStatus();
  switch( status ){
    case "wifi-level1":
    case "wifi-level2":
    case "wifi-level3":
    case "wifi-level4":
    case "eth-ok":
      TIMESTAMP("Waking up / WiFi live -> subscribe to events");
      if( lpec.evtSocket != null){
        lpec.closeEvtSocket();
      }
      lpec.initialiseEventSocket(configuration.dsIPAddress);
      try
      {
        Activity.scheduleAfter(350, lpec.subscribeAll);
      } catch (e) {
        GUI.alert("Activity.scheduleAfter[3] " + e);
        LOG("Activity.scheduleAfter[3] " + e);
      };  
      
      lpec.wifi = true;
      break;
    default:
      TIMESTAMP("Waking up / WiFi dead -> wait for WiFi");
      lpec.wifi = false;
  }
  lpec.sleep = false;
}


lpec.onNetlinkEvent = function(event){
  // Handle netlink events
  // If WiFi drops, unsubscribe from events
  // If WiFi recovers, re-start event socket and re-subscribe to events
  if( !lpec.sleep ){	  
    switch( event ){
    case "disabled":
    case "sleeping":
    case "wifi-disconnected":
    case "wifi-noip":
    case "wifi-standalone":
    case "ethdisconnected":
    case "eth-noip":
      if( lpec.wifi ){	
      TIMESTAMP("Wifi dropped")
          if( lpec.evtSocket != null){
            lpec.unSubscribeAll();
          }  
          lpec.wifi = false;
      }	
      break;
    case "wifi-level1":
    case "wifi-level2":
    case "wifi-level3":
    case "wifi-level4":
    case "eth-ok":
      if( !lpec.wifi ){
        lpec.wifi = true;
          TIMESTAMP("WiFi reconnected - subscribe to events");
          if( lpec.evtSocket != null){
            lpec.closeEvtSocket();
          }
          lpec.initialiseEventSocket(configuration.dsIPAddress);
          
          try
          {
            Activity.scheduleAfter(350, lpec.subscribeAll);
          } catch (e) {
            GUI.alert("Activity.scheduleAfter[4] " + e);
            LOG("Activity.scheduleAfter[4] " + e);
          };
      
      }  
      break;
    default:
      LOG("Unhandled Netlink Event: "+event);
    }
  }
}


//------------------------------------------------------------------------------
// Startup - create sockets / setup handlers for LPEC, netlink and sleep events
//------------------------------------------------------------------------------

lpec.handleSleep = function(){
  LOG("lpec.handleSleep");
  currActivity = CF.activity("");
  currActivity.onSleep = lpec.onSleep;
  currActivity.onWake = lpec.onWake;
}


lpec.handleNetlinkEvent = function(){
  System.addEventListener("netlink",lpec.onNetlinkEvent);
}


lpec._start = function(){
  lpec.initialiseActionSocket(configuration.dsIPAddress);
  lpec.initialiseEventSocket(configuration.dsIPAddress);
  LOG("LPEC Subscribing, in 350ms");

  try
  {
    Activity.scheduleAfter(350, lpec.subscribeAll);
    Activity.scheduleAfter(2990, lpec.handleNetlinkEvent);
    Activity.scheduleAfter(3000, lpec.handleSleep);
  } catch (e) {
    GUI.alert("Activity.scheduleAfter[5] " + e);
    LOG("Activity.scheduleAfter[5] " + e);
  };

};

lpec.byebye = function()
{
};


elab.add("lpec", null, lpec._start);
