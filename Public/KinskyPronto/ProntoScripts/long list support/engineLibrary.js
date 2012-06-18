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
// PROPOSED
//
////

var engineLibrary = {};

#define engineLibrary_REQUEST_CHUNK_SIZE 30

/*
engineLibrary.folders = [];
engineLibrary.tracks = [];
engineLibrary.cache = {};
*/

engineLibrary.currentId = 0;
engineLibrary.index = 0; // '.index' contains the number of entries read so far.

engineLibrary.requestedIndex = 0;
engineLibrary.totalMatches = 0;


engineLibrary.entryIsAvailable = function(entryIndex)
{
  return entryIndex < engineLibrary.index;
};

engineLibrary.entryHasBeenRequested = function(entryIndex)
{
  return entryIndex < engineLibrary.requestedIndex;
};

// .idRequestCallback is used to allow uiLibrary to request that this module (engineLibrary)
// gets ms entries which the user is requesting.
// 'engineLibrary' does this when it need to display '..'s
// 
engineLibrary.idRequestCallback = function(entryIndex)
{
  // We update 'engineLibrary.requestedIndex' (done in '.getNextChunk')
  // to be the index we have requested up to (Which may not be the index we actually have - 
  // as there may be media server requests outstanding).
  // If we are requesting something we will (soon) have then just return. This stops the cascade
  // of requests which would otherwise happen when uiLibrary displays a page full of '..'s
  //
  LOG("engineLibrary.idRequestCallback("+entryIndex+") ["+engineLibrary.requestedIndex+"]");
	
  if (engineLibrary.entryHasBeenRequested(entryIndex))
  {
    LOG("engineLibrary.idRequestCallback("+entryIndex+") has already been requested.");
    return;
  };

  engineLibrary.requestedIndex = entryIndex;

  engineLibrary.getNextChunk();

};

engineLibrary.getNextChunk = function()
{
  LOG("engineLibrary.getNextChunk()"+engineLibrary.index+":"+engineLibrary.totalMatches+
	                           ":"+engineLibrary.requestedIndex);

  // Do we have all the elements we need?
  //
  if (engineLibrary.index >= engineLibrary.totalMatches)
  {
    LOG("engineLibrary.getNextChunk, got all entries, returning");
    return;
  };

  if (engineLibrary.index > engineLibrary.requestedIndex)
  {
    LOG("engineLibrary.getNextChunk, got enough entries, returning");
    return;
  };


  LOG("engineLibrary.getNextChunk, need ["+engineLibrary.index+".."+engineLibrary.totalMatches+"]");
  // rikrik engineLibrary.requestedIndex += engineLibrary.requestChunkSize;
 
#if 0
  GUI.alert("Need ["+engineLibrary.index+".."+engineLibrary.totalMatches+"]");
#endif
  
  // We're about to request another chunk of entries so update requestedIndex so any extra calls to
  // engineLibrary.idRequestCallback won't cause us unnecessary recursion / calls.
  //
  if (engineLibrary.requestedIndex < engineLibrary.index + engineLibrary_REQUEST_CHUNK_SIZE -1)
  {
    engineLibrary.requestedIndex = engineLibrary.index + engineLibrary_REQUEST_CHUNK_SIZE -1;
  };

  // Need to get some more elements.
  //
  function browse()
  {
    ms.browseDirectChildrenWithCallback(
      engineLibrary.objectId,
      engineLibrary.callback,
      engineLibrary.index,
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
  // TODO: This callback needs to know if to call 'uiLibrary.setLibraryNavigation'
  // or 'uiLibrary.updateLibraryNavigation', depending of if it has been called for
  // the first time per browse entry or not.
  //
  if (cacheEntry == null)
  {
    // Should we update uiLibrary with some form of error status?
    //
    return;
  };

  // TODO: Need work here to rebase entryCache.folders & .tracks to an array offset from .index
  // and merged
  //
  engineLibrary.currentId = cacheEntry.currentId;
  engineLibrary.totalMatches = cacheEntry.totalMatches;
  LOG("In engineLibrary.callback ["+engineLibrary.currentId+", got "+engineLibrary.index+"+"+cacheEntry.numberReturned+" of "+engineLibrary.totalMatches+"]");

  if (engineLibrary.index == 0)
  {
    uiLibrary.setLibraryNavigation(
        cacheEntry.parentId,
        cacheEntry.currentId,
        cacheEntry.title,
        cacheEntry.folders,
        cacheEntry.tracks,
	engineLibrary.totalMatches,
	engineLibrary.idRequestCallback);
  }
  else
  {
    uiLibrary.updateLibraryNavigation(
      cacheEntry.folders,
      cacheEntry.tracks);
  };

  engineLibrary.index += cacheEntry.numberReturned;
  engineLibrary.idRequestCallback(engineLibrary.requestedIndex);

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

  engineLibrary.index = 0;
  // rikrikr
  // should this be?
  //   engineLibrary.index = location
  //
  engineLibrary.requestedIndex = engineLibrary_REQUEST_CHUNK_SIZE -1;

  if (objectId == null) {
    objectId = engineLibrary.currentId;
  };

  engineLibrary.objectId = objectId;
  
  ms.browseDirectChildrenWithCallback(
      engineLibrary.objectId,
      engineLibrary.callback,
      engineLibrary.index,			// rikrik initially will be zero
      engineLibrary_REQUEST_CHUNK_SIZE);

};

elab.add("engineLibrary");
