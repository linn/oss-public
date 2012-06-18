////
//
// engine_Radio.js
//
// Previously engine.Presets.js
//
////

var engine_Radio = {};

engine_Radio.currentPlaylist = [];
engine_Radio.displayedPresetArray = [];
engine_Radio.trackListString = "";
engine_Radio.trackId = 0;
engine_Radio.trackIdIndex = -1;

engine_Radio.presetCallbacks = [];
engine_Radio.statusCallbacks = [];

engine_Radio.status = {title:"", albumArtURI:"", TransportState: 'Stopped', seconds: '0'};

// Set the current preset, id is index into engine_Radio.displayedPresetArray
//
engine_Radio.setPreset = function (id) {

  // var entry = {id:0, metadata:"", title:"", albumArtURI:"", res:""};
  
  for (var i = 0; i < engine_Radio.displayedPresetArray.length; i++) {
    var entry = engine_Radio.displayedPresetArray[i];
  
    if (entry.id == id) {
      LOG('engine_Radio.setPreset('+entry.id+','+entry.res+')');
      lpecMessages.Ds.Radio.setId(entry.id, entry.res);
      engine_Radio.play();
      return;
    };
  };
  
  LOG("Didn't find preset id ["+id+"]");
};

engine_Radio.stop = function()
{
  LOG("engine_Radio.stop()");
  lpecMessages.Ds.Radio.Stop();
};



engine_Radio.playPrevious = function()
{
  // Can't play preset prior to 0 (Array offset)
  
  LOG("engine_Radio.playPrevious()");
  if (engine_Radio.trackIdIndex < 1) {
    return;
  };

  engine_Radio.setPreset(engine_Radio.displayedPresetArray[engine_Radio.trackIdIndex-1].id);
};

engine_Radio.playNext = function(){

  LOG("engine_Radio.playNext()");
  if ((engine_Radio.trackIdIndex == -1) || (engine_Radio.trackIdIndex >= engine_Radio.displayedPresetArray.length))
  {
    return;
  };

  engine_Radio.setPreset(engine_Radio.displayedPresetArray[engine_Radio.trackIdIndex+1].id);

};

engine_Radio.play = function()
{
  LOG("engine_Radio.play()");
  lpecMessages.Ds.Radio.Play();

};

engine_Radio.toggleStopPlay = function()
{
  LOG("engine_Radio.toggleStopPlay: " + engine_Radio.status.TransportState);

  if (engine_Radio.status.TransportState == 'Stopped') {
    engine_Radio.play();
  } else {
    engine_Radio.stop();
  };

};

engine_Radio.registerPresetsCallback = function(callback){

  engine_Radio.presetCallbacks.push(callback);

  try {
    callback(engine_Radio.displayedPresetArray);
  } catch (e) {
    Diagnostics.log("callback error primary");
    Diagnostics.log(e);
  } finally {
        ; // null;
  };

};

engine_Radio.invokePresetsCallback = function(){

  LOG("engine_Radio.invokePresetsCallback, " + engine_Radio.presetCallbacks.length);

  for (var i = 0; i < engine_Radio.presetCallbacks.length; i++){

    try {
      engine_Radio.presetCallbacks[i](engine_Radio.displayedPresetArray);
    } catch (e) {
      LOG("Callback error");
      LOG(e);
    } finally {
          ; // null;
    };
  };
};


engine_Radio.registerStatusCallback = function(callback){

  engine_Radio.statusCallbacks.push(callback);

  try {
    callback(engine_Radio.status);
  } catch (e) {
    Diagnostics.log("callback error status");
    Diagnostics.log(e);
  } finally {
        ; // null;
  };

};

engine_Radio.invokeStatusCallback = function(){

  LOG("engine_Radio.invokeStatusCallback, " + engine_Radio.presetCallbacks.length);

  for (var i = 0; i < engine_Radio.statusCallbacks.length; i++){

    try {
      engine_Radio.statusCallbacks[i](engine_Radio.status);
    } catch (e) {
      LOG("Callback error [2]");
      LOG(e);
    } finally {
          ; // null;
    };
  };
};


// decodes the response to an LPEC DS/Playlist request
//
// <MetaDataList><Entry>...</Entry></MetaDataList>
//

engine_Radio.lpecCallbackPresetId = function(value){

  engine_Radio.trackIdIndex = -1;
  engine_Radio.trackId = parseInt(value);
  
  LOG("engine_Radio.callbackPresetId("+value+")")

  for (var i = 0; i < engine_Radio.displayedPresetArray.length; i++){
    var entry = engine_Radio.displayedPresetArray[i];
    
    if (entry.id == value) {
      engine_Radio.trackIdIndex = i;
      LOG("Listening to " + entry.title);
    };
  };

  // engine_Radio.playlistUpdated();
  // Should we update the display to refelect the new playing Track?
  //
  engine_Radio.calculateCurrentPreset();
  engine_Radio.invokePresetsCallback();
};

engine_Radio.calculateCurrentPreset = function()
{
  engine_Radio.trackIdIndex = -1;

  for (var i = 0; i < engine_Radio.displayedPresetArray.length; i++){
    var entry = engine_Radio.displayedPresetArray[i];
    
    engine_Radio.displayedPresetArray[i].currentPreset = (entry.id == engine_Radio.trackId);
    if (engine_Radio.displayedPresetArray[i].currentPreset) {
      engine_Radio.trackIdIndex = i;
      LOG("We are listening to " + entry.title);
    };
  };
	
};


// The basic idea is that a call to playlistUpdated() will update the complete playlist of upto 100
// presets. We don't have enough RAM to store all the track data for this so just get the 10 or
// so tracks which we will be displaying.
// This is done by an async callback (via LPEC) which in turn calls this.setTracks() with the full
// details of the 10 tracks we are interested in displaying.
// This callback to setTracks will cause the display to be updated as appropriate.
//
engine_Radio.callbackLpecReadList = function(result)
{
  LOG("In engine_Radio.callbackLpecReadList");
  LOG("decodeMetaDataList["+result+"]");

  var displayedPresetArray = dsRadio.readlistNotDecoded(result[0]);
  engine_Radio.setPresets(displayedPresetArray);
};



engine_Radio.setPresets = function(displayedPresetArray)
{
  engine_Radio.displayedPresetArray = displayedPresetArray;
  engine_Radio.calculateCurrentPreset();
  engine_Radio.invokePresetsCallback();
};

engine_Radio.getPresets = function(arrayOfIds)
{

  // Normally this would be done as a callback some time later (after the DS
  // has responded)
  //
  // get the subset tracks we are intersted in the call
  // engine_Radio.setPresets(displayedPresetArray);
  //
  var displayedPresetArray = [];
  var trackListString = "";
#ifdef DAVAAR
  var seperator = " ";
#else
  var seperator = ",";
#endif


  for (var i = 0; i < arrayOfIds.length; i++)
  {
		trackListString += arrayOfIds[i]+seperator;
  };
  
  LOG("Tracks to get = {"+trackListString+"}");

  engine_Radio.trackListString = trackListString;  
  lpecMessages.Ds.Radio.ReadList(trackListString, engine_Radio.callbackLpecReadList);
};


engine_Radio.display = function(){

  // Ok we should have a list of tracks etc..
  // What do we do?
  //
  if (engine_Radio.currentPlaylist == null) {
  
    engine_Radio.setPresets([]);
    return;
  };
  
  if (engine_Radio.currentPlaylist.length == 0) {
  
    engine_Radio.setPresets([]);
    return;
  };

  engine_Radio.somethingToDisplay = true;

  LOG("Numbers ["+engine_Radio.currentPlaylist.length+"]");
  

  var arrayOfIds = [];
  
  for (var i = 0; i < engine_Radio.currentPlaylist.length; i++){

//      LOG("Adding "+engine_Radio.currentPlaylist[i]+" to arrayOfIds");
      arrayOfIds[arrayOfIds.length] = engine_Radio.currentPlaylist[i];

  };
  
  engine_Radio.getPresets(arrayOfIds);

};

engine_Radio.presetsUpdated = function(){

  LOG("engine_Radio.playlistUpdated();");

  if (engine_Radio.currentPlaylist == null){
    engine_Radio.currentPlaylist = [];
  };

  LOG("engine_Radio.currentPlaylist.length = " + engine_Radio.currentPlaylist.length);

  // Presets may be zero extended so delete any excess.
  //
  while ((engine_Radio.currentPlaylist.length > 0 ) && engine_Radio.currentPlaylist[engine_Radio.currentPlaylist.length-1] == 0) {
    engine_Radio.currentPlaylist.length--;
  };

#if 0
  var diagStr = "";
  
  for (var i=0; i < engine_Radio.currentPlaylist.length; i++)
  {
    diagStr = diagStr + engine_Radio.currentPlaylist[i] + " ";
  };
  
  LOG("engine_Radio.currentPlaylist[] = " + diagStr;
#endif

  // Special case for the empty playlist.
  //
  if (engine_Radio.currentPlaylist.length == 0) {
    LOG("No Presets");
    engine_Radio.somethingToDisplay = false;
    engine_Radio.display();
    return;
  };

  engine_Radio.somethingToDisplay = true;
  engine_Radio.display();
};

engine_Radio.lpecCallbackIdArray = function(value){

  LOG("lpecCallbackIdArray called ["+value+"]");
  engine_Radio.currentPlaylist = Base64.toIntegerArray(value); 
  engine_Radio.presetsUpdated();

};


engine_Radio.lpecCallbackChannelMetadata = function(value){

  // The ChannelMetadata will be of the form:
  // &lt;DIDL-Lite xmlns:dc=&quot;http://purl.org/dc/elements/1.1/&quot; xmlns:upnp=&quot;urn:schemas-upnp-org:metadata-1-0/upnp/&quot; xmlns=&quot;urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/&quot;&gt;&lt;item&gt;&lt;dc:title&gt;Al Jazeera TV (TV News) - Not Supported&lt;/dc:title&gt;&lt;res bitrate=&quot;000&quot;&gt;http://opml.radiotime.com/Tune.ashx?error=true&amp;amp;formats=mp3,wma&amp;amp;username=linnproducts&amp;amp;partnerId=16&lt;/res&gt;&lt;upnp:albumArtURI&gt;http://radiotime-logos.s3.amazonaws.com/s105647q.png&lt;/upnp:albumArtURI&gt;&lt;upnp:class&gt;object.item.audioItem&lt;/upnp:class&gt;&lt;/item&gt;&lt;/DIDL-Lite&gt;
  //
  // So need code to parse this.
  //
  LOG("lpecCallbackChannelMetadata()");
//  LOG("lpecCallbackChannelMetadata called ["+value+"]");
  var xml = utils.htmlDecode(value);
//  LOG("lpecCallbackChannelMetadata xml ["+xml+"]");
  
  // Only interested in `.title` & `.albumArtURI`
  //
  var entry = dsRadio._parseEntry(xml);

  // metadata is double escaped
  engine_Radio.status.title = utf8.decode(utils.htmlDecode(utils.htmlDecode(entry.title)));
  engine_Radio.status.albumArtURI = entry.albumArtURI;

  engine_Radio.invokeStatusCallback();
  
};

engine_Radio.lpecCallbackTransportState = function(value){

  LOG("lpecCallbackTransportState("+value+")");

  engine_Radio.status.TransportState = value;
  engine_Radio.invokeStatusCallback();

};


engine_Radio.lpecCallbackSeconds = function(value){

  LOG("lpecCallbackSeconds("+value+")");

  engine_Radio.status.seconds = value;
  engine_Radio.invokeStatusCallback();

};

engine_Radio._start = function(){
  lpec.addEventCallback(engine_Radio.lpecCallbackIdArray,         "IdArray",         "Ds", "Radio");
  lpec.addEventCallback(engine_Radio.lpecCallbackPresetId,        "Id",              "Ds", "Radio"); // Not trackId

#if 0
  lpec.addEventCallback(engine_Radio.lpecCallbackChannelUri,      "ChannelUri",      "Ds", "Radio"); 
#endif

#ifdef DAVAAR
  lpec.addEventCallback(engine_Radio.lpecCallbackChannelMetadata, "Metadata", "Ds", "Info");
#else
  lpec.addEventCallback(engine_Radio.lpecCallbackChannelMetadata, "ChannelMetadata", "Ds", "Radio");
#endif
  lpec.addEventCallback(engine_Radio.lpecCallbackTransportState,  "TransportState",  "Ds", "Radio");
  lpec.addEventCallback(engine_Radio.lpecCallbackSeconds,         "Seconds",         "Ds", "Time");

/*
ChannelUri "http://opml.radiotime.com/Tune.ashx?id=s13606&amp;amp;formats=mp3,wma&amp;amp;username=linnproducts&amp;amp;partnerId=16"
ChannelMetadata "&lt;DIDL-Lite xmlns:dc=&quot;http://purl.org/dc/elements/1.1/&quot; xmlns:upnp=&quot;urn:schemas-upnp-org:metadata-1-0/upnp/&quot; xmlns=&quot;urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/&quot;&gt;&lt;item&gt;&lt;dc:title&gt;Al Jazeera TV (TV News) - Not Supported&lt;/dc:title&gt;&lt;res bitrate=&quot;000&quot;&gt;http://opml.radiotime.com/Tune.ashx?error=true&amp;amp;formats=mp3,wma&amp;amp;username=linnproducts&amp;amp;partnerId=16&lt;/res&gt;&lt;upnp:albumArtURI&gt;http://radiotime-logos.s3.amazonaws.com/s105647q.png&lt;/upnp:albumArtURI&gt;&lt;upnp:class&gt;object.item.audioItem&lt;/upnp:class&gt;&lt;/item&gt;&lt;/DIDL-Lite&gt;"
TransportState "Stopped"
*/
};


elab.add("engine_Radio", null, engine_Radio._start);
