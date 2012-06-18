////
//
// engineTcpSocket.js
//
////
var engineTcpSocket = {};
engineTcpSocket.socket = null; 
engineTcpSocket.outstandingRequests = [];


engineTcpSocket.processRequests = function() {

  var data = "";
  var request;
  var callback;
  var parsed;
  var tries;
  
  while (engineTcpSocket.outstandingRequests.length > 0) {
    
  request = engineTcpSocket.outstandingRequests.shift();
  
  tries = 0;
  while (tries<3) {
      try {
        engineTcpSocket.socket = new TCPSocket(true); // true = sync socket
        engineTcpSocket.socket.connect(request.ipAddress, request.port, 500);
        tries = 3;
    } catch (e) {
      LOG('Try #'+tries+': Unable to create MS socket ['+e+']');
      tries += 1;
      engineTcpSocket.socket.close();
      engineTcpSocket.socket = null;
    }  
  }

    if (engineTcpSocket.socket == null) {
    request.errorCallback();
    return;
  }

    TIMESTAMP(" >>> TCP Tx: "+request.message );    
    try {
      engineTcpSocket.socket.write(request.message);  
    } catch (e) {
      LOG('Error writing to MS socket ['+e+']');
      engineTcpSocket.socket.close();
      engineTcpSocket.socket = null;
      request.errorCallback();
      return;
    }

    data = "";    
    while (data.lastIndexOf(":Envelope>") == -1) {		// assume SOAP message
      try {
        data += engineTcpSocket.socket.read( 16384, 20 );
      } catch (e) {}
    }
    data = data.slice(0,-2);
    TIMESTAMP(" <<< TCP Rx: " + data.slice(0,10000));

    engineTcpSocket.socket.close();
    engineTcpSocket.socket = null;
    
    parsed = engineTcpSocket.parseHttpResponse(data);
    request.callback(parsed.status, parsed.header, parsed.body);
  }	
}


engineTcpSocket.parseHttpResponse = function(response) {

  var dummyResult = {status:0, header:"", body:""};
  var crLf = "\r\n"
  var header = ""
  var status = 0
  var body = ""

  if (response.length == 0) {
    LOG("response is null");
    return dummyResult;
  }

  var header_body = response.split(crLf+crLf,2); // find the blank line between HTTP header & body
    
  if (header_body.length != 2) {
    LOG("header_body.length != 2, is " + header_body.length);
    LOG('Received: ' + response);
    return dummyResult;
  }
  header = header_body[0]
  body = header_body[1]

  try {
      var component = header.split(crLf);    
      status = parseInt(component[0].match(/\d{3,}/)[0],10);
  } catch (e) {
      LOG('Error trying to get HTTP status ' + e);
      LOG('HTTP Header is: ' + header);
      return dummyResult;
  }

  if (status != 200) { // 200 = HTTP Sucsess
    LOG("Status is ["+status+"]");
    for (var cpt = 0; cpt < component.length; cpt ++){
      LOG(cpt + ":" + component[cpt]);
    }
  }
  return {status: status, header: header, body: body};
}


engineTcpSocket.doSOAP = function (ipAddress, port, message, callback, errCallback) {

  var request = {
    ipAddress: ipAddress,
    port: port,
    message: message,
    callback: callback,
    errorCallback: errCallback}; 

  engineTcpSocket.outstandingRequests.push(request); 
  engineTcpSocket.processRequests();
}


elab.add("engineTcpSocket");
