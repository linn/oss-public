////
//
// engineTracks.js
//
//
// TODO: What about play.js & trackQueuing.js ???
//
// This whole module pertains to the old days when we had a 'Play All' at the
// top of any list of tracks - now obsolete.
//
////
var engineTracks = {};

engineTracks.trackId = 0;
engineTracks.lastIdInQueue = 0;

engineTracks.tracksQueue = [];


// `.playTrack` should we play the track after it has been queued?
//
engineTracks.playTrack = false;

engineTracks.passTime = function(){

  activity.tick();
  
};

engineTracks.timePassed = function(){

  activity.stop();

};


engineTracks.handleTrackQueue = function(){

  if (engineTracks.tracksQueue.length == 0){
    engineTracks.timePassed(); // i.e. We've finished
    return;
  };
  
  var track = engineTracks.tracksQueue.shift();
  var aUri = track.aUri;
  var aMetaData = track.aMetaData;

  lpec.action('Ds/Playlist 1 Insert "'+engineTracks.trackId+'" "'+aUri+'" "'+aMetaData+'"', engineTracks.callback);

}

engineTracks.insertTrackInQueue = function(aUri, aMetaData){

  var track;
  
  LOG("aUri:"+aUri);
  LOG("aMetaData:"+aMetaData);
  LOG("engineTracks.insertTrack engineTracks.trackId:1:"+engineTracks.trackId);
  
#if 0
  //  aUri = 'http://192.168.1.40:9000/disk/music/O1$13$73688182$744778304$2758094404.flac';
  //  aMetaData = '<DIDL-Lite xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/" xmlns:dlna="urn:schemas-dlna-org:metadata-1-0/" xmlns:pv="http://www.pv.com/pvns/" xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/"><item id="1$13$73688182$744778304$2758094404" refID="1$268435466$1744840755" parentID="1$13$73688182$744778304" restricted="1"><dc:title>Jewel</dc:title><dc:creator>Unknown</dc:creator><upnp:artist>Marcella Detroit</upnp:artist><upnp:album>Jewel</upnp:album><upnp:genre>General Pop</upnp:genre><dc:date>0000-01-01</dc:date><upnp:originalTrackNumber>1</upnp:originalTrackNumber><upnp:albumArtURI>http://192.168.1.40:9000/cgi-bin/O1$13$73688182$744778304$2758094404/W160/H160/S1/L1/Xjpeg-jpeg.desc.jpg</upnp:albumArtURI><res size="27611885"   protocolInfo="http-get:*:audio/x-flac:*" >http://192.168.1.40:9000/disk/music/O1$13$73688182$744778304$2758094404.flac</res><upnp:class>object.item.audioItem.musicTrack</upnp:class></item></DIDL-Lite>'

  //  aUri = utils.htmlEncode(aUri);
  //  aMetaData = utils.htmlEncode(aMetaData);
#endif

  track = {aUri: aUri, aMetaData:aMetaData};
  
  engineTracks.tracksQueue.push(track);
  //?? engineTracks.timePassed();

};


engineTracks.callback = function(result){

  LOG("engineTracks.callback engineTracks.trackId:2:"+result[0]);
  var result = parseInt(result[0]);
  
  // Should we play the track we've just queued?
  //
  if (engineTracks.playTrack) {
    engineTracks.playTrack = false;
    lpecMessages.seekTrackId(result);
  };

  if (result > engineTracks.trackId) {
     engineTracks.trackId = result;
  };
  LOG("engineTracks.callback engineTracks.trackId:3:"+engineTracks.trackId);

  engineTracks.handleTrackQueue();

};

// called from 'trackQueuing.queueTrack'
//
engineTracks.insertTrack = function(track){

  var uri         = "";
  var index_left  = "";
  var index_mid   = "";
  var index_right = "";
  var quot        = new RegExp ('"', 'g');
  
  engineTracks.passTime();

  index_left  = track.self.indexOf('&lt;res');
  index_mid   = track.self.indexOf('&gt;', index_left);
  index_right = track.self.indexOf('&lt;/res&gt;', index_mid);
  uri = track.self.substring(index_mid + 4, index_right);

  // SOAP message may not have double quotes escaped whilst LPEC message
  // MUST have these escaped, hence escape any existing double quotese here
  engineTracks.insertTrackInQueue(uri, track.self.replace(quot,'&quot;'));

  engineTracks.handleTrackQueue();
};

engineTracks.insertTracks = function(tracks){

  for (var i = 0; i < tracks.length; i++) {
    engineTracks.insertTrack(tracks[i]);
  };

};

// id < 0 = use existing
// otherwise override.
//
engineTracks.queueAfter = function(id){

  if (id < 0) {
    engineTracks.trackId = engineTracks.lastIdInQueue;
  } else {
    engineTracks.trackId = id;
  };
  
};

engineTracks.lpecCallback = function(value){

  // This callback is regulated to every 300ms so don't necessarily take what it says as gosple.
  //
  var currentPlaylist = Base64.toIntegerArray(value); // base64._arrayAsString(...)

  uiPlaylist.engineTracksCallbackIdArray(currentPlaylist);

  if (currentPlaylist.length == 0) {

    engineTracks.trackId = 0;
    engineTracks.lastIdInQueue = 0;

  } else {
    var result = currentPlaylist[currentPlaylist.length-1];
    
    engineTracks.lastIdInQueue = result;
    
    if (result > engineTracks.trackId) {
      engineTracks.trackId = result;
    };
  };
  
  LOG("engineTracks.lpecCallback trackId:" + engineTracks.trackId );
  
  engineTracks.handleTrackQueue();
  
};

engineTracks._start = function(){

  // Register callback to keep trackId up to date
  //
  lpec.addEventCallback(engineTracks.lpecCallback, "IdArray", "Ds", "Playlist");
}

elab.add('engineTracks', null, engineTracks._start);
