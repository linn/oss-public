////
//
// trackQueuing.js
//
//
// Common code to do DS playlist and track queuing manipulation
//
// msId: A media server Id.
// If this Id is a single track then only that track will be queued
// If it is a folder the all the tracks within that folder will be
// (recursivly) added.
//
// Used in play.js
//
// API is trackQueuing.queueTrack, .setMode, .queueFolderId
//
////

/*

The design decission is wether to queue by msId or by the actual retreived
data. Asset 2.1 isn't that great at doing browseMetadata but is good at
browseDirectChildren. But are we going to break some design beauty by going that way?

Konductor uses `dsPlaylist.addTracksInfo` to add a track by Id.

*/

var trackQueuing = {};

// we need to keep track of the current playing track so that we
// can implement playAfter.
//
trackQueuing.currentPlayingTrackId = 0;

// `.foldersQueue` is a queue of folders to be added to the DS's playlist
// (Via `.tracksQueue`).
//
// The main issue with this is what happens when we need to traverse a large
// folder with many entries - it will cause the Pronto the fail with memory
// full.
//
// So instead we need to be able to chunk call to get the folder, now we need
// a way to represent this in the queue.
//
// In general we can fairly safely assume that any one folder will hold less
// subfolders than tracks (at a maximum).
// But will there be too many folders?
// Perhaps instead of a queue of folders, we need to maintain a queue of
// folder chunks, with each chunk representing 20-30 entries within the folder.
//
//
trackQueuing.foldersQueue = [];

// There are three queuing modes:
//
//   -1 = play Now
//    0 = play next
//   +1 = queue to end.
//
// NOTE: Play Now will queue the track at the END of the DS's playlist &
// play the track from there.
//
trackQueuing.mode = 1;

// External Function
// =================
//
trackQueuing.setMode = function(mode){

  trackQueuing.mode = mode;

  // Should we play now?
  //
  if (mode == -1) {
    engineTracks.playTrack = true;
    trackQueuing.mode = +1; // Set mode to queue at end.
  };

  if (mode == 0) {
    if (uiNowPlaying.trackId != 0) {
      engineTracks.queueAfter(uiNowPlaying.trackId);
    } else {
      engineTracks.queueAfter(0);
    };
  } else {
    engineTracks.queueAfter(-1);
  };
};

// It is possible to queue either an individual track or the (recursive)
// contents of a folder.
//
// The normal calling convention would be:
//
// trackQueuing.setMode(theModeWanted);
// trackQueuing.queueTrack(msId); or trackQueuing.queueFolder(msId);
//
//
// Should we not pass the pre-parsed track or folder, not the msId?
//
// External Function
// =================
//
trackQueuing.queueTrack = function(track){

  engineTracks.insertTrack(track);

};

trackQueuing.queueTracks = function(tracks){

  engineTracks.insertTracks(tracks);

};

// Queue Folder is used as a callback fn in the call to
// .queueFolderId
//
trackQueuing.queueFolder = function(folder){
  
  LOG("trackQueuing.queueFolder()");
  // Stop needless recusion
  //
  if (folder == null) {
    LOG("trackQueuing.queueFolder(null), returning");
    trackQueuing.processAFoldersQueueEntry();
    return;
  };

  // What we need to do is..
  //
  // 1. Queue any track which are in the folder.
  // 2. For each subfolder - queue the Id to start the folder grabbing
  //    process off.
  //
  LOG(folder.tracks.length + " tracks to queue");
  if (folder.tracks.length > 0) {
    
    trackQueuing.queueTracks(folder.tracks);
  };
  
  // We use the engineLibrary.browse code here - or something very close
  // and callback'able.
  //
  // ms.browseDirectChildrenWithCallback();
  //
  // What we need it do is queue the sub-folders for later processing.
  //
  LOG(folder.folders.length + " folders to queue");

  if (folder.folders.length < 1) {
  
    // No subfolder, return.
    //
    trackQueuing.processAFoldersQueueEntry();
    return;
  };
  
  for (var i = 0; i < folder.folders.length; i++) {
    LOG("Queuing folder ["+folder.folders[i].id+"], ["+folder.folders[i].title+"]");
    
    // Actually queue the folder
    //
    trackQueuing.foldersQueue.push(folder.folders[i].id);
  };

  // We've added at least one folder so lets get started on processing them.
  //
  trackQueuing.processAFoldersQueueEntry();

};


// External Function
// =================
//
trackQueuing.queueFolderId = function(msId)
{
  LOG("Queuing folder ["+msId+"]");
  ms.browseDirectChildrenWithCallback(msId, trackQueuing.queueFolder);
};


trackQueuing.processAFoldersQueueEntry = function(){

  LOG("trackQueuing.processAFoldersQueueEntry()");
  
  if (trackQueuing.foldersQueue.length < 1) {
    LOG("no folders to process");
    return;
  };
  
  var entry = trackQueuing.foldersQueue.shift();
  
  LOG("Process folder id ["+entry+"]");
  // Now do something with `entry`.
  //
  trackQueuing.queueFolderId(entry);
};


trackQueuing.callbackTrackId = function(value){

  trackQueuing.currentPlayingTrackId = parseInt(value);

  LOG("trackQueuing.callbackTrackId("+value+")")

};

trackQueuing._start = function()
{
#ifdef DAVAAR
	lpec.addEventCallback(trackQueuing.callbackTrackId, "Id", "Ds", "Playlist");
#else
	lpec.addEventCallback(trackQueuing.callbackTrackId, "TrackId", "Ds", "Ds");
#endif
};

elab.add("trackQueuing", null, trackQueuing._start);
