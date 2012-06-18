////
//
// engineLibrary.js
//
// This sub module of the engine object deals exclusivly with the interface to the Media Server (MS).
//
// For each object ID we maintain the DirectChildren & Metadata
//
// Define a engineLibrary sub-object
//
// uses: elab, doProntoScriptHttp() [in ProntoScriptHttp.js], now replaced by engineTcpSocket.doSOAP(...)
//
// All Media Server comms via UPnP, via doProntoScriptHttp()
//
// Modified to use lib/msParse.js, which is substantially faster & more memory effecient.
//
// TODO: Bad - relies on the internal state of uiLibrary - should be optimised away.
//
// CURRENT
//
////

var engineLibrary = {};

engineLibrary.currentId = 0;
engineLibrary.callSetLibraryNavigation = true;
engineLibrary.totalMatches = 0;

// .idRequestCallback is used to allow uiLibrary to request that this module (engineLibrary)
// gets ms entries which the user is requesting.
// 'engineLibrary' does this when it need to display '..'s
// 
engineLibrary.idRequestCallback = function(entryIndex)
{
  LOG("engineLibrary.idRequestCallback():"+entryIndex);

#if 0
  GUI.alert("Need ["+entryIndex+":"+engineLibrary.totalMatches+"]");
#endif
  
  // Need to get some more elements.
  //
  function browse()
  {
    ms.browseDirectChildrenWithCallback(
      engineLibrary.objectId,
      engineLibrary.callback,
      entryIndex,
      engineLibrary_REQUEST_CHUNK_SIZE);
  };

  try
  {
    Activity.scheduleAfter(250, browse);  // TODO: This value need tweeking
  } catch (e) {
    GUI.alert("Activity.scheduleAfter[1] " + e);
    LOG("Activity.scheduleAfter[1] " + e);
  };
};

engineLibrary.callback = function(cacheEntry)
{
  // This callback needs to know if to call 'uiLibrary.setLibraryNavigation'
  // or 'uiLibrary.updateLibraryNavigation', depending of if it has been called for
  // the first time per browse entry or not (Hence .callSetLibraryNavigation).
  //
  if (cacheEntry == null)
  {
    // Should we update uiLibrary with some form of error status?
    //
    return;
  };

  engineLibrary.currentId = cacheEntry.currentId;
  engineLibrary.totalMatches = cacheEntry.totalMatches;
  LOG("In engineLibrary.callback ["+engineLibrary.currentId+", got "+cacheEntry.numberReturned+" of "+engineLibrary.totalMatches+"]");

  if (engineLibrary.callSetLibraryNavigation)
  {
	engineLibrary.callSetLibraryNavigation = false;
	uiLibrary.setLibraryNavigation(
		cacheEntry.parentId,
		cacheEntry.currentId,
		cacheEntry.title,
		cacheEntry.entries,
		engineLibrary.totalMatches,
		engineLibrary.idRequestCallback);
  }
  else
  {
	uiLibrary.updateLibraryNavigation(
		cacheEntry.entries);
  };

}

// TODO: all the hard work needs to happen here (or in it's callback)
//
// 'location' is the index of where we want to start browsing from.
// If it's null then start at the first entry, otherwise start browsing
// from the location specified.
//
// BUT: & its a big but we will (necesseraly) have to read items [0..location]
// first - which may not be optimal.
//
engineLibrary.browse = function (objectId, location)
{
  if (objectId == null) {
    objectId = engineLibrary.currentId;
  };

  engineLibrary.objectId = objectId;
  
  engineLibrary.callSetLibraryNavigation = true;

  ms.browseDirectChildrenWithCallback(
      engineLibrary.objectId,
      engineLibrary.callback,
      0, // index
      engineLibrary_REQUEST_CHUNK_SIZE);

};

elab.add("engineLibrary");
