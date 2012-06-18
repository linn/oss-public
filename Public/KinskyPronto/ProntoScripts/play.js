////
//
// play.js
//
// Handle to play now, next or later button used on the library page
//
// Calls trackQueuing.*
//
// This module could also be used by the center / home button
// when we decide what behaviour we want - but only for tracks,
// not folders.
//
// This procedure does none of the recursion for folders etc just the basic stuff to start
// `trackQueuing` off.
//
// Given a `.selectedId` we need to know if we have selected
// a folder or track. This is done via:
//
//   uiLibrary.folders = [];
//   uiLibrary.tracks = [];
//   uiLibrary.atRoot = false; (Do we display a "<Back" as first entry?)
//
////

var play = {};

play.PLAYING_MODE_TYPE = {NOW:0, NEXT:1, LATER:2};

play.selectedId = "";

play.selectionIsFolder = false;
play.selectedFolder = null;
play.selectionIsTrack = false;
play.selectedTrack = null;

play.playing_mode = play.PLAYING_MODE_TYPE.LATER;

play.computeSelection = function(){

  var result = uiLibrary.computeSelection();
  play.selectionIsFolder = result.selectionIsFolder;
  play.selectedFolder =  result.selectedFolder;
  play.selectionIsTrack =  result.selectionIsTrack;
  play.selectedTrack =  result.selectedTrack;
 
  return;
/*
  // Reset all values.
  //
  play.selectionIsFolder = false;
  play.selectedFolder = null;
  play.selectionIsTrack = false;
  play.selectedTrack = null;
  
  // selectedId is indexed from 0.
  //
  var selectedId = play.selectedId;
  
  // If the first entry isn't 'go up a level' then we will have displayed a line to go
  // 'up' a level, we need to ignore this line.
  //
  if (!uiLibrary.atRoot) {
    selectedId--;
  };

  if (selectedId < 0) {
    LOG("Selected up-a-level");
    return;
  };
  
  // Is the selection a folder?
  //
  if (selectedId < uiLibrary.folders.length) {
    play.selectionIsFolder = true;
    play.selectedFolder = uiLibrary.folders[selectedId];
    return;
  };
 
  selectedId = selectedId - uiLibrary.folders.length;
  
  // Is the selection a track?
  //
  if (selectedId < uiLibrary.tracks.length) {
    play.selectionIsTrack = true;
    play.selectedTrack = uiLibrary.tracks[selectedId];
    return;
  };

  // Shouldn't really get here,
  // but if we do then just fail silently.
  //
*/
};


play.putSelected = function()
{
#if 0
  var prefix = "";
  
  if (play.playNow) {
    prefix = 'Now';
  } else if (play.playNext) {
    prefix = 'Next';
  } else if (play.playLater) {
    prefix = 'Later';
  };

  if (play.selectionIsTrack) {
    LOG(prefix + ", track ["+play.selectedTrack.title+"]");
  } else if (play.selectionIsFolder) {
    LOG(prefix + ", folder ["+play.selectedFolder.title+"]");
  } else {
    LOG(", no selection");
  };
#endif
};

play.playTrack = function(track)
{
  LOG("Request to play track ["+track.title+"]");
  trackQueuing.queueTrack(track);
};

play.playFolder = function(folder)
{
  LOG("Request to play folder ["+folder.title+":"+folder.id+"]");
  //
  // We call queueFolderId as we haven't read the folder, just it's name
  // and Id.
  //
  trackQueuing.queueFolderId(folder.id);  
};

play.playGeneral = function()
{
  play.computeSelection();
  play.putSelected();

  // There are three queuing modes:
  //
  //   -1 = play Now
  //    0 = play next
  //   +1 = queue to end.
  //
  // TODO: Should this be done in play.now, .next, .later ?
  //
  if (play.playing_mode == play.PLAYING_MODE_TYPE.NOW) {
    trackQueuing.setMode(-1);
  } else if (play.playing_mode == play.PLAYING_MODE_TYPE.NEXT) {
    trackQueuing.setMode(0);
  } else if (play.playing_mode == play.PLAYING_MODE_TYPE.LATER) {
    trackQueuing.setMode(+1);
  };
  
  if (play.selectionIsTrack) {
    play.playTrack(play.selectedTrack);
  } else if (play.selectionIsFolder) {
    play.playFolder(play.selectedFolder);
  };
  
};

play.now = function(){
  LOG("play.now("+play.selectedId+")");
  
  play.playing_mode = play.PLAYING_MODE_TYPE.NOW;
  play.playGeneral();

};

play.next = function(){
  LOG("play.next("+play.selectedId+")");
  
  play.playing_mode = play.PLAYING_MODE_TYPE.NEXT;
  play.playGeneral();

};

play.later = function(){
  LOG("play.later("+play.selectedId+")");

  play.playing_mode = play.PLAYING_MODE_TYPE.LATER;
  play.playGeneral();

};

// Called from uiLibrary.js to get the MS ID currently selected (From
// the scroll wheel callback).
//
play.setId = function(selectedId){

  // LOG("play.setId("+selectedId+")");

  play.selectedId = selectedId;
};


elab.add("play");
