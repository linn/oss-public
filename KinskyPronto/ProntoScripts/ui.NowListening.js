////
//
// ui_NowListening.js
//
////

var ui_NowListening = {};

ui_NowListening.coverArtUrl = "";

ui_NowListening.status = {seconds:0, title:"", albumArtURI:"", TransportState: 'Stopped'};

ui_NowListening.enabled = false;

ui_NowListening.enable = function(){

  ui_NowListening.enabled = true;
  
  // Reset the cover art Url so we will reload it.
  //
  ui_NowListening.coverArtUrl = "";

  engineSources.selectDs();
  engineSources.internalDsSourceSelect(engineSources.INTERNAL_SOURCES.RADIO);

  setFunctionKeys(engine_Radio.playPrevious, engine_Radio.playNext, engine_Radio.toggleStopPlay, doNothing, doNothing);

  uiVolumeEtc.update();
  ui_NowListening.update();
  //TODO: doesn't have .. sWMLowLevel.go(null);

};

// Disable the display of the UI
//
ui_NowListening.disable = function()
{
  // Stop the up, down, left, right & OK buttons doing anything.
  //
  ui_NowListening.enabled = false;
};

ui_NowListening.callback = function(status)
{
  LOG('NowListening status (' + status.title + ',' + status.albumArtURI + ', ' + status.TransportState  + ', ' + status.seconds + ')');

  ui_NowListening.status = status; // {title:"", albumArtURI:"", TransportState: 'Stopped'};
  ui_NowListening.update();

};

ui_NowListening.coverArt = function(aUrl){

  // When playing this fn wil be called once a second (As each second causes the status to change)
  // So don't reload the image unless we have to.
  //
  if (ui_NowListening.coverArtUrl == aUrl) {
    return;
  };

  ui_NowListening.coverArtUrl = aUrl;

  coverArt.setCoverArtFromUrl(aUrl);

};


ui_NowListening.update = function(){

  ui_NowListening.coverArt(ui_NowListening.status.albumArtURI);
  
  if (ui_NowListening.status.TransportState == 'Stopped') {
    handleFooter.setFnImage("F3", "PLAY");
  } else {
    handleFooter.setFnImage("F3", "STOP");
  };

  var seconds = (ui_NowListening.status.seconds % 86400); // Seconds in a day 23:59 -> 00:00
  
  page("NowListening").widget("relTime").label = utils.mm_ss(seconds);

  //CF.page("NowListening").widget("Artist").label = uiNowPlaying.track.artist;
  CF.page("NowListening").widget("Track").label = ui_NowListening.status.title;
  //CF.page("NowListening").widget("Album").label = uiNowPlaying.track.album;
};

ui_NowListening._start = function(){

  engine_Radio.registerStatusCallback(ui_NowListening.callback);

};

elab.add("ui_NowListening", null, ui_NowListening._start);
