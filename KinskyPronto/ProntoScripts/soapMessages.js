////
//
// soapMessages.js
//
// Currently only has support for ContentDirectory, will probably stay this way as all other Pronto comms will be via LPEC.
//
// urn:schemas-upnp-org:service:ContentDirectory:1#Browse /ContentDirectory/Control
//
////

var soapMessages = {};

soapMessages.header = '<?xml version="1.0" encoding="utf-8"?><s:Envelope s:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/" xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body>'
soapMessages.footer = '</s:Body></s:Envelope>'


////
//
//
// -> u:BrowseResponse
/*

POST /ContentDirectory/172824d8-83c1-5e7f-a16b-b3520877e6cb/control.xml HTTP/1.1 -- for Asset UPnP --

POST /ContentDirectory/Control HTTP/1.1
SOAPACTION: "urn:schemas-upnp-org:service:ContentDirectory:1#Browse"
CONTENT-TYPE: text/xml ; charset="utf-8"
HOST: 192.168.1.25:9000
Content-Length: 535

<?xml version="1.0" encoding="utf-8"?>
<s:Envelope s:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/" xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
   <s:Body>
      <u:Browse xmlns:u="urn:schemas-upnp-org:service:ContentDirectory:1">
         <ObjectID>32</ObjectID>
         <BrowseFlag>BrowseDirectChildren</BrowseFlag>
         <Filter>*</Filter>
         <StartingIndex>0</StartingIndex>
         <RequestedCount>0</RequestedCount>
         <SortCriteria />
      </u:Browse>
   </s:Body>
</s:Envelope>

*/
////

soapMessages.contentDirectoryBrowse = function(msIPAddress, msPort, objectId, browseFlag, startingIndex, requestedCount) {

  var crLf = "\r\n";
  var result = "";
  var body = "";
	
  if (startingIndex == null) {
    startingIndex = 0;
  };

  if (requestedCount == null) {
    requestedCount = 250;
  };

  // TODO: RESTRICTION:  Limited to 250 elements
  // Asset returns much more data that Twonky so we have to limit out return count to 100 messages.
  // Asset port numbers are always > 10,000 and Twonky < 10,000 - which is useful.
  //
  if (requestedCount > 250) {
      requestedCount = 250;
  };
    
  if (msPort > 9999) {
    if (requestedCount > 100) {
      requestedCount = 100;
    };
  };

  body = soapMessages.header;
  body += '<u:Browse xmlns:u="urn:schemas-upnp-org:service:ContentDirectory:1">';
  body += '<ObjectID>' + objectId + '</ObjectID>';
  body += '<BrowseFlag>'+browseFlag+'</BrowseFlag>';
  body += '<Filter>*</Filter>';
  body += '<StartingIndex>'+startingIndex+'</StartingIndex>';
  body += '<RequestedCount>'+requestedCount+'</RequestedCount>';
  body += '<SortCriteria />';
  body += '</u:Browse>';
  body += soapMessages.footer;
  
  result = 'POST '+configuration.msUrl +' HTTP/1.1' + crLf;
  result += 'SOAPACTION: "urn:schemas-upnp-org:service:ContentDirectory:1#Browse"' + crLf;
  result += 'CONTENT-TYPE: text/xml ; charset="utf-8"' + crLf;
  result += 'HOST: ' + msIPAddress + ':' + msPort + crLf;
  result += 'Content-Length: ' + body.length + crLf;
  result += "" + crLf;
  
  return result + body

}

soapMessages.contentDirectoryBrowseDirectChildren = function(msIPAddress, msPort, objectId, startingIndex, requestedCount) {

  return soapMessages.contentDirectoryBrowse(msIPAddress, msPort, objectId, "BrowseDirectChildren", startingIndex, requestedCount) 

}

#if 0
soapMessages.contentDirectoryBrowseMetadata = function(msIPAddress, msPort, objectId) {

  return soapMessages.contentDirectoryBrowse(msIPAddress, msPort, objectId, "BrowseMetadata") 

}
#endif

elab.add("soapMessages");
