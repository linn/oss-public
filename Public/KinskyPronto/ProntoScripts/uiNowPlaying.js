////
//
// uiNowPlaying.js
//
////
var uiNowPlaying = {};

// Variables to hold what we display with suitable defaults
//
uiNowPlaying.seconds = 0;
uiNowPlaying.trackId = 0; // = no track selected
uiNowPlaying.trackDuration = 0;

uiNowPlaying.track = {album:"", artist:"", title:"", coverArt: ""}
uiNowPlaying.transportState = "";
uiNowPlaying.page = CF.page("NowPlaying");

uiNowPlaying.toggleMute = function()
{
  engineVolumeEtc.toggleMute();
};

uiNowPlaying.callbackSeconds = function(value)
{
  uiNowPlaying.seconds = parseInt(value);
  uiNowPlaying.refreshProgress();
};

uiNowPlaying.callbackLpecRead = function(result)
{
  // We should get 2 items back, [1] the uri & [2] the metadata.
  //
#if 0
    for (var i = 0; i < result.length; i++){
      LOG("Res["+i+"]" +result[i]);
    };
#endif

  var metadata = result[1];
  
  uiNowPlaying.track = {album:"", artist:"", title:"", coverArt: ""};
  
  // TODO This should be xmlExtractTextSpace, but it doesn't work.
  //
  LOG("Metadata["+metadata+"]");
  uiNowPlaying.track.title = utf8.decode(utils.htmlDecode(utils.xmlExtractText(metadata, "dc:title"))); // Trailing space in string is important
  uiNowPlaying.track.album = utf8.decode(utils.htmlDecode(utils.xmlExtractText(metadata, "upnp:album"))); // Trailing space in string is important
  uiNowPlaying.track.artist = utf8.decode(utils.htmlDecode(utils.xmlExtractText(metadata, "upnp:artist"))); // Trailing space in string is important
  uiNowPlaying.track.coverArt = utils.xmlExtractText(metadata, "upnp:albumArtURI"); // Trailing space in string is important
  
  coverArt.fromURL(uiNowPlaying.track.coverArt);
  uiNowPlaying.refreshTrack();
}

uiNowPlaying.callbackTrackId = function(value)
{
  uiNowPlaying.trackId = parseInt(value);
  
  LOG("uiNowPlaying.callbackTrackId("+value+")")
  
  if (uiNowPlaying.trackId == 0)
  {
    // No track is playing so null out values & return.
    //
    uiNowPlaying.track = {album:"", artist:"", title:"", coverArt: ""};
    uiNowPlaying.refreshTrack();
    coverArt.clear();
    return;
  };
  
  lpecMessages.playlistRead(uiNowPlaying.trackId, uiNowPlaying.callbackLpecRead);

};

uiNowPlaying.callbackTrackDuration = function(value)
{
  uiNowPlaying.trackDuration = parseInt(value);
  uiNowPlaying.refreshProgress();
};


uiNowPlaying.callbackTransportState = function(value)
{
  LOG("callbackTransportState:"+value);
  uiNowPlaying.transportState = value;
  uiNowPlaying.refreshTransportState();
}
//
////
//
// refresh the on screen graphics.
//

uiNowPlaying.refreshTransportState = function()
{
  handleFooter.updateAll();
};


uiNowPlaying.refreshVolume = function()
{
  uiVolumeEtc.update();
};

uiNowPlaying.refreshProgress = function()
{
  var seconds = uiNowPlaying.seconds;
  
  uiNowPlaying.page.widget("relTime").label = utils.mm_ss(seconds);
  
  // LOG("Sec:"+seconds+":Dur:" +  uiNowPlaying.trackDuration);
  
  // Sanity incase LPEC events out of order.
  //
  if (seconds > uiNowPlaying.trackDuration){
    seconds = 0;
  };
  
  // Special case if we don't know the track's duration, we simply switch the spinner off.
  //
  if (uiNowPlaying.trackDuration < 1)
  {
    uiNowPlaying.page.widget("progressImage").visible = false;
    if (pronto.is9400)
    {
      uiNowPlaying.page.widget("progressImage9400").visible = false;
      uiNowPlaying.page.widget("remainingTime9400").visible = false;
    };
    return;
  };
  
  if (pronto.is9400) {
    var barWidth = 166;
    var barXOffset = 37;
    var percentage166 = parseInt((uiNowPlaying.seconds / uiNowPlaying.trackDuration) * 166);
    
    var secondsRemaining = parseInt(uiNowPlaying.trackDuration - uiNowPlaying.seconds);
    
    uiNowPlaying.page.widget("progressImage9400").stretchImage = true;  
    uiNowPlaying.page.widget("progressImage9400").visible = true;
    uiNowPlaying.page.widget("progressImage9400").left = percentage166 + barXOffset;
    uiNowPlaying.page.widget("remainingTime9400").visible = true;
    uiNowPlaying.page.widget("remainingTime9400").label = utils.mm_ss(secondsRemaining);
    return;
  };
  
  var percentage = parseInt((uiNowPlaying.seconds / uiNowPlaying.trackDuration) * 100)
  
  // percentage is currently a %age [0..100] we need to change this to [0..48]
  //
  var per48 = parseInt(percentage*48/100)

  // LOG("%:"+percentage+":%48:"+per48);
  
  if (per48 > 48) {per48 = 48};
  if (per48 < 0) {per48 = 0};
  
  if (per48 == 0) {
    uiNowPlaying.page.widget("progressImage").visible = false;
    return;
  };
  
  uiNowPlaying.page.widget("progressImage").stretchImage = true;  
  uiNowPlaying.page.widget("progressImage").visible = true;
  
  if (per48 >= 37) { 
    uiNowPlaying.page.widget("progressImage").setImage(CF.widget("seq"+per48, "SEQ4").getImage());
    return;
  };

  if (per48 >= 25) { 
    uiNowPlaying.page.widget("progressImage").setImage(CF.widget("seq"+per48, "SEQ3").getImage());
    return;
  };

  if (per48 >= 13) { 
    uiNowPlaying.page.widget("progressImage").setImage(CF.widget("seq"+per48, "SEQ2").getImage());
    return;
  };

  uiNowPlaying.page.widget("progressImage").setImage(CF.widget("seq"+per48, "SEQ1").getImage());

}


uiNowPlaying.refreshTrack = function(){
  
  uiNowPlaying.page.widget("Artist").label = uiNowPlaying.track.artist;
  uiNowPlaying.page.widget("Track").label = uiNowPlaying.track.title;
  uiNowPlaying.page.widget("Album").label = uiNowPlaying.track.album;

  coverArt.refresh();
  
};

uiNowPlaying.refresh = function()
{
  uiNowPlaying.refreshVolume();
  uiNowPlaying.refreshProgress();
  uiNowPlaying.refreshTrack();
  uiNowPlaying.refreshTransportState();
  handleShuffleRepeat.update();
};

uiNowPlaying._start = function(){

  uiNowPlaying.page.widget("GOTO_HOME_PAGE").visible = false;
  uiNowPlaying.page.widget("GOTO_NOW_PLAYING").visible = false;
#ifdef DAVAAR
  lpec.addEventCallback(uiNowPlaying.callbackSeconds, "Seconds", "Ds", "Time");
  lpec.addEventCallback(uiNowPlaying.callbackTrackId, "Id", "Ds", "Playlist");
  lpec.addEventCallback(uiNowPlaying.callbackTrackDuration, "Duration", "Ds", "Time"); // Was TrackDuration
  lpec.addEventCallback(uiNowPlaying.callbackTransportState, "TransportState", "Ds", "Playlist");
#else
  lpec.addEventCallback(uiNowPlaying.callbackSeconds, "Seconds", "Ds", "MediaTime");
  lpec.addEventCallback(uiNowPlaying.callbackTrackId, "TrackId", "Ds", "Ds");
  lpec.addEventCallback(uiNowPlaying.callbackTrackDuration, "TrackDuration", "Ds", "Ds");
  lpec.addEventCallback(uiNowPlaying.callbackTransportState, "TransportState", "Ds", "Ds");
#endif
  
};

// Enable the display of UI
//
uiNowPlaying.enable = function()
{
  // NOTE: .internalDsSourceSelect implicitly calls engineSources.selectDs();
  //
  engineSources.internalDsSourceSelect(engineSources.INTERNAL_SOURCES.PLAYLIST);

  uiNowPlaying.refresh();
  sWMLowLevel.go(null);
};


elab.add('uiNowPlaying', null, uiNowPlaying._start);
