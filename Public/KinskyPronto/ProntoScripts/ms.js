////
//
// ms.js
//
// This sub module of the engine object deals exclusivly with the interface to the Media Server (MS)*
//
// *Currently assumed to be Twonky, but with a little work any UPnP compliant server
//
// We maintain a simple cache of MS responces this is for performance reasons as the MS is not very likely to change it's state
//
// This cache is automatically cleared whenever the Pronto 'Home' page is revisited.
//
// For each object ID we maintain the DirectChildren & Metadata
//
//
// uses: elab, doProntoScriptHttp() [in ProntoScriptHttp.js], now replaced by engineTcpSocket.doSOAP(...)
//
// All Media Server comms via UPnP, via doProntoScriptHttp()
//
//
// Modified to use lib/msParse.js, which is substantially faster & more memory effecient.
//
////

// Cache should remain disabled, othe code now makes assumtions
// about calling which would not be true with caching enabled.
//
#define USE_CACHE 0

var ms = {};

ms.currentId = null;


ms.callback = function(){};

ms.browseDirectChildrenWithCallback = function (objectId, callback, startingIndex, requestedCount)
{

  if (startingIndex == null) {
    startingIndex = 0;
  };

  if (requestedCount == null) {
    requestedCount = 250;
  };

  ms.callback = callback;

  activity.start();

  if (objectId == null) {
    callback(null);
    return;
  };

  LOG(".browse(" + objectId + ")");

  // We need to make a SOAP call for the DirectChildren & Metadata.
  // Because of this we take a note of the id we've been asked to look at - because most of our
  // work will be done in callbacks which will not have access to this function's scope.
  //
  ms.currentId = objectId;
  ms.startingIndex = startingIndex;
  
  engineTcpSocket.doSOAP(
    configuration.msIPAddress,
    configuration.msPort,
    soapMessages.contentDirectoryBrowseDirectChildren(configuration.msIPAddress,
                                                      configuration.msPort,
                                                      ms.currentId,
                                                      startingIndex,
                                                      requestedCount),
    ms.callbackBrowseDirectChildren,
    ms.errorCallback); 

};


ms.callbackBrowseDirectChildren = function (status, header, body) {
  // LOG("in ms.callbackBrowseDirectChildren");
  
  // If the SOAP call failed then simply return `null` to the callback.
  //
  if (status != 200) {
    ms.callback(null);
    return;
  };
  
  activity.tick();

  var result = msParse.contentDirectory_1_Browse_response(body);
  //
  //  result.numberReturned  = NumberReturned;
  //  result.totalMatches = TotalMatches;
  //  result.updateID = UpdateID;
  //  result.folders = [...];
  //  result.tracks = [...];
  //  result.entries = [...]; index from ms.startingIndex
  //
  var entries = [];
  var offset = ms.startingIndex;

#if 0  
  GUI.alert("ms.startingIndex:"+ms.startingIndex);
#endif  

  for (var i = 0; i < result.folders.length; i++){
      
      // Populate the id to parentid (& title) mapping
      //
      msMetadataCache.add(result.folders[i].id, ms.currentId, result.folders[i].title);

      // LOG("folders["+i+"] " + result.folders[i].id + ":" + result.folders[i].title);
      entries[i+offset] = result.folders[i];
      entries[i+offset].isFolder = true;
  };

  offset += (result.folders.length);
  
  for (var i = 0; i < result.tracks.length; i++){
      entries[i+offset] = result.tracks[i];
      entries[i+offset].isFolder =  false;      
  };

  var metadata = msMetadataCache.get(ms.currentId);

  LOG (".browse2(" + ms.currentId + " ["+metadata.title+"] " + metadata.parentId + ")");

  
  var result = {parentId:       metadata.parentId, // content_info.parentID,
	        currentId:      ms.currentId,
		title:          metadata.title, // content_info.title,
		folders:        result.folders, // TODO: No longer necessary
		tracks:         result.tracks,  // TODO: No longer necessary
		entries:	entries,
	        numberReturned: result.numberReturned,
	        totalMatches:   result.totalMatches};

  
#if USE_CACHE
  ms.cache[ms.currentId] = result;
#endif
		    
  // LOG("About to 'ms.callback(cacheEntry);'");
    
  activity.stop();
  ms.callback(result);  

};


ms.errorCallback = function() {
  activity.stop();
}