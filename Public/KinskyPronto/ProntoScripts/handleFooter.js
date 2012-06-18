////
//
// handleFooter.js
//
//
// ToDo: This package needs an overhall to take into account the differing footers
// which each page has.
//
////
var handleFooter = {};

  // F1 = prev
  // F2 = next
  // F3 = play
  // F4 = Repeat
  // F5 = Shuffle
  //

handleFooter.doNext = function(){
  engineSources.selectDs();
  lpecMessages.next();
};

handleFooter.doPrevious = function(){
  engineSources.selectDs();
  lpecMessages.previous();
};

handleFooter.doPlay = function(){
  engineSources.selectDs();
  lpecMessages.play();
};

handleFooter.doPause = function(){
  engineSources.selectDs();
  lpecMessages.pause();
};

handleFooter.setFnImage2 = function(pageName, fn, image){

  var widget = CF.page(pageName).widget(fn);
  widget.stretchImage = true;  
  widget.visible = true;
  widget.setImage(CF.page("BUTTONS").widget(image).getImage());

};

handleFooter.setFnImage = function(fn, image){

#if (VARIANT_DS)

  handleFooter.setFnImage2("NowPlaying", fn, image);
  handleFooter.setFnImage2("Library",    fn, image);
  handleFooter.setFnImage2("Playlist",   fn, image);

#elif (VARIANT_RADIO)

  handleFooter.setFnImage2("NowListening", fn, image);
  handleFooter.setFnImage2("Presets",      fn, image);
  
#elif (VARIANT_PREAMP)

  // Do nothing

#endif

}

// handleFooter.updateAll is the only external function.
//
handleFooter.updateAll = function(){

#if (VARIANT_DS)

    var paused, menuActive, shuffle, repeat;
  
    LOG("uiNowPlaying.transportState:" + uiNowPlaying.transportState);

    if (uiNowPlaying.transportState == "Playing") {
      paused = false;
    } else if (uiNowPlaying.transportState == "Buffering") {
      paused = false;
    } else if (uiNowPlaying.transportState == "Paused") {
      paused = true;
    } else if (uiNowPlaying.transportState == "Stopped") {
      paused = true;
    };
  
    menuActive = uiPlaylist.firmMenuVisible;
    shuffle = handleShuffleRepeat.shuffle;
    repeat = handleShuffleRepeat.repeat;
  
    handleFooter.update(paused, menuActive, shuffle, repeat, ui_paging.currentPage == ui_paging.PAGENAME_TYPE.LIBRARY);

#elif (VARIANT_RADIO)

  // Do nothing
  
#elif (VARIANT_PREAMP)

  // Do nothing

#endif

};

handleFooter.updateLibraryPage = function(){

  var libraryPage = CF.page("Library");

  libraryPage.widget("F1").stretchImage = true;  
  libraryPage.widget("F1").visible = true;
  libraryPage.widget("F1").setImage(CF.page("BUTTONS").widget("PLAY_NOW").getImage()); // 86px wide !
  
  libraryPage.widget("F2").stretchImage = true;  
  libraryPage.widget("F2").visible = true;
  libraryPage.widget("F2").setImage(CF.page("BUTTONS").widget("PLAY_NEXT").getImage());
  
  libraryPage.widget("F3").stretchImage = true;  
  libraryPage.widget("F3").visible = true;
  libraryPage.widget("F3").setImage(CF.page("BUTTONS").widget("PLAY_LATER").getImage());
  
  libraryPage.widget("F4").visible = false;
  
  libraryPage.widget("F5").visible = false;
  
  setFunctionKeys(play.now, play.next, play.later, doNothing, doNothing);
};

handleFooter.update = function (paused, menuActive, shuffle, repeat, libraryPage){

  if (libraryPage) {
    handleFooter.updateLibraryPage();
    return;
  };

  var f1, f2, f3, f4, f5;
  var f1Img, f2Img, f3Img, f4Img, f5Img;
  
  f1Img = "SKIP_BACK";
  f2Img = "SKIP_FORWARD";
  f3Img = "PLAY";
  f4Img = "REPEAT";
  f5Img = "SHUFFLE";
  
  f1 = handleFooter.doPrevious;
  f2 = handleFooter.doNext;
  f3 = handleFooter.doPlay;
  f4 = lpecMessages.playlistRepeatOn;
  f5 = lpecMessages.playlistShuffleOn;
  
  // Are we paused or stopped?
  //
  if (!paused) {
    f3Img = "PAUSE"
    f3 = handleFooter.doPause;
  };
  
  if (repeat) {
    f4Img = "REPEAT_ON"
    f4 = lpecMessages.playlistRepeatOff;
  };

  if (shuffle) {
    f5Img = "SHUFFLE_ON"
    f5 = lpecMessages.playlistShuffleOff;  
  };
  
  if (menuActive) {
    f1Img = "DELETE_PLAYLIST"
    f2Img = "DELETE_SELECTED"
    f3Img = "GOTO_TRACK"
  
    f1 = lpecMessages.playlistDeleteAll;
    f2 = uiPlaylist_deleteTrack;
    f3 = uiPlaylist_displayCurrentTrack;
  
  };
  
  setFunctionKeys(f1, f2, f3, f4, f5);

  handleFooter.setFnImage("F1", f1Img);
  handleFooter.setFnImage("F2", f2Img);
  handleFooter.setFnImage("F3", f3Img);
  handleFooter.setFnImage("F4", f4Img);
  handleFooter.setFnImage("F5", f5Img);

}

elab.add("handleFooter");
