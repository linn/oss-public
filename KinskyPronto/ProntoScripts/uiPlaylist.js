////
//
// uiPlaylist.js (Pronto variant)
//
// TODO: Time for a rewrite:
//
// TODO: Make sure that - if there is a playlist - then the first entry
// on every page is '[Clear Playlist]'
//
// The Pronto can display 8 lines of text (+ the album / artist / title of
// the currently selected track). 
// only display 8 tracks worth of information.
//
////
var uiPlaylist = {};


// Interface into DS Playlist display:
//
uiPlaylist.emptyPlaylist = true;
uiPlaylist.firstTrackToDisplay = 0; // Track Id's start at 1.
uiPlaylist.indexOfFirstTrackToDisplay = 0;
uiPlaylist.currentPlayingTrack = 0; // Track Id's start at 1.
uiPlaylist.currentPlaylist = null; // null == no value yet. [];
uiPlaylist.pageSize = pronto.uiPlaylistPageSize;
uiPlaylist.somethingToDisplay = false;
uiPlaylist.trackListString = "";
uiPlaylist.displayedTrackArray = [];
uiPlaylist.index = 0;
uiPlaylist.enabled = false;
uiPlaylist.currentScrollWheel = null;
uiPlaylist.trackId = 0;
uiPlaylist.firmMenuVisible = false; // Alternate Firm Key Menu

uiPlaylist.loadingScrollWheel = {
    characterHeight:pronto.characterHeight,  // High of widgetList text in px. (24 = 18px, 32 = 24px)
    y: pronto.yOffset, // y offset in px to avoid headers etc
    widgetList: 'List',
    widgetSelector: 'Selector',
    highlightEntry: -1,
    highlightSelector: 'MARKER',
    updateSelectionCallback: doNothing,
    list: ["[Loading playlist...]"],
    selection: 0};

uiPlaylist.emptyScrollWheel = {
    characterHeight:pronto.characterHeight,  // High of widgetList text in px. (24 = 18px, 32 = 24px)
    y: pronto.yOffset, // y offset in px to avoid headers etc
    widgetList: 'List',
    widgetSelector: 'Selector',
    highlightEntry: -1,
    highlightSelector: 'MARKER',
    updateSelectionCallback: doNothing,
    list: ["[Empty playlist]"],
    selection: 0};

uiPlaylist.doEast = doNothing;
uiPlaylist.doWest = doNothing;

// Enable the display of UI
//
uiPlaylist.enable = function()
{
  uiPlaylist.firmMenuVisible = false;
  doFirmMenu = uiPlaylist_firmMenu;
  
  uiPlaylist.enabled = true;

  uiNowPlaying.refreshTransportState();
  
  handleShuffleRepeat.update();
  
  // Set the up, down, left, right & OK button to do something
  //
  prontoHardKeys.setCompassKeys(uiPlaylist.select, uiPlaylist.pageUp, uiPlaylist.pageDown, uiPlaylist.doEast, uiPlaylist.doWest);
  
  uiPlaylist.playlistUpdated();
};

// Disable the display of the UI
//
uiPlaylist.disable = function()
{
  uiPlaylist.firmMenuVisible = false;
  doFirmMenu = doNothing;
  
  uiPlaylist.enabled = false;
  // Stop the up, down, left, right & OK buttons doing anything.
  //
  prontoHardKeys.disableCompassKeys();
};

function uiPlaylist_displayCurrentTrack()
{
  LOG("uiPlaylist_displayCurrentTrack")
  if (!uiPlaylist.somethingToDisplay) {return};
  LOG("display Current Track ["+uiPlaylist.trackId+"]")

  if (uiPlaylist.trackId == 0) {return};
  
  var index = 0
  
  for (var i = 0; i < uiPlaylist.currentPlaylist.length; i++) {
//    LOG("I:"+i);
    if (uiPlaylist.trackId == uiPlaylist.currentPlaylist[i]) {
      index = i;
      LOG("uiPlaylist.indexOfFirstTrackToDisplay = " + i);
    };
  };

  index = parseInt(index / uiPlaylist.pageSize) * uiPlaylist.pageSize;


  uiPlaylist.indexOfFirstTrackToDisplay = index;
  
  uiPlaylist.firstTrackToDisplay = uiPlaylist.currentPlaylist[uiPlaylist.indexOfFirstTrackToDisplay];
  uiPlaylist.display();

};

function uiPlaylist_firstPage(){

  if (!uiPlaylist.somethingToDisplay) {return};

  uiPlaylist.indexOfFirstTrackToDisplay = 0;
  uiPlaylist.firstTrackToDisplay = uiPlaylist.currentPlaylist[uiPlaylist.indexOfFirstTrackToDisplay];
  uiPlaylist.display();

};

function uiPlaylist_lastPage(){

  if (!uiPlaylist.somethingToDisplay) {return};

  // Goto last page.
  //
  uiPlaylist.indexOfFirstTrackToDisplay = uiPlaylist.currentPlaylist.length - uiPlaylist.pageSize + 1;

  if (uiPlaylist.indexOfFirstTrackToDisplay < 0) {
    uiPlaylist.indexOfFirstTrackToDisplay = 0;
  };

  uiPlaylist.firstTrackToDisplay = uiPlaylist.currentPlaylist[uiPlaylist.indexOfFirstTrackToDisplay];
  uiPlaylist.display();

};


// Display the previous page of tracks
//
uiPlaylist.pageUp = function(){

  if (!uiPlaylist.somethingToDisplay) {return};
  
  // Display the previous page of tracks
  //
  uiPlaylist.indexOfFirstTrackToDisplay -= uiPlaylist.pageSize;
  
  if (uiPlaylist.indexOfFirstTrackToDisplay < 0) {
    uiPlaylist.indexOfFirstTrackToDisplay = 0;
  };
  
  uiPlaylist.firstTrackToDisplay = uiPlaylist.currentPlaylist[uiPlaylist.indexOfFirstTrackToDisplay]

  uiPlaylist.display();
};

// Display the next page of tracks
//
uiPlaylist.pageDown = function(){
  if (!uiPlaylist.somethingToDisplay) {return};

  if ((uiPlaylist.indexOfFirstTrackToDisplay + uiPlaylist.pageSize) >= uiPlaylist.currentPlaylist.length) {
    LOG("Already on last page.");
    return;
  };

  uiPlaylist.indexOfFirstTrackToDisplay += uiPlaylist.pageSize;

  uiPlaylist.firstTrackToDisplay = uiPlaylist.currentPlaylist[uiPlaylist.indexOfFirstTrackToDisplay]

  uiPlaylist.display();
};

uiPlaylist.select = function(){

  LOG("Play track ["+uiPlaylist.index+"]");

  if(!uiPlaylist.somethingToDisplay){
    LOG("Nothing to do.");
    return;
  };

  LOG("Play track ["+uiPlaylist.displayedTrackArray[uiPlaylist.index].trackId+"]");
  lpecMessages.seekTrackId(uiPlaylist.displayedTrackArray[uiPlaylist.index].trackId);

};

uiPlaylist.setCurrentPlayingTrack = function(trackId){
  uiPlaylist.currentPlayingTrack = trackId;
};
  
uiPlaylist.playlistUpdated = function()
{
  if (uiPlaylist.currentPlaylist == null){
    uiPlaylist.currentPlaylist = []
  };

  // Special case for the empty playlist.
  //
  if (uiPlaylist.currentPlaylist.length == 0) {
    uiPlaylist.somethingToDisplay = false;
    uiPlaylist.display();
    return;
  };

  uiPlaylist.somethingToDisplay = true;

  uiPlaylist.setFirstTrackToDisplay();
  uiPlaylist.display();
  // Now uiPlaylist.firstTrackToDisplay is 0 of the first track we want to display.
  //
};

uiPlaylist.display = function()
{
  if (!uiPlaylist.enabled) {return};
  
  // Ok we should have a list of tracks etc..
  // What do we do?
  //
  if (uiPlaylist.currentPlaylist == null) {
  
    CF.page("Playlist").widget("STATUS_LINE").label = "Playlist [Loading]";

    uiPlaylist.somethingToDisplay = false;
    uiPlaylist.currentScrollWheel = uiPlaylist.loadingScrollWheel;
    sWMLowestLevel.go(uiPlaylist.currentScrollWheel);
    return;
  };
  
  if (uiPlaylist.currentPlaylist.length == 0) {
  
    CF.page("Playlist").widget("STATUS_LINE").label = "Playlist [Empty]";

    uiPlaylist.somethingToDisplay = false;
    uiPlaylist.currentScrollWheel = uiPlaylist.emptyScrollWheel
    sWMLowestLevel.go(uiPlaylist.currentScrollWheel);
    return;
  };

  uiPlaylist.somethingToDisplay = true;

  CF.page("Playlist").widget("STATUS_LINE").label = "Playlist " + utils.pageXofY(uiPlaylist.indexOfFirstTrackToDisplay, uiPlaylist.currentPlaylist.length, uiPlaylist.pageSize);

  var arrayOfIds = [];
  
  for (var i = uiPlaylist.indexOfFirstTrackToDisplay; i < uiPlaylist.currentPlaylist.length; i++){
    if (arrayOfIds.length < uiPlaylist.pageSize ) {
      arrayOfIds[arrayOfIds.length] = uiPlaylist.currentPlaylist[i];
    }
  };
  
  uiPlaylist.getTracks(arrayOfIds);

};

// decodes the responce to an LPEC DS/Playlist request
//
// <MetaDataList><Entry>...</Entry></MetaDataList>
//

uiPlaylist.lpecCallbackTrackId = function(value)
{
	uiPlaylist.trackId = parseInt(value);
	LOG("uiPlaylist.callbackTrackId("+value+")")
	uiPlaylist.playlistUpdated();
};


uiPlaylist.callbackLpecReadList = function(result)
{
  var unencodedEntrys     = utils.xmlExtractTextArray(result[0], "Entry");
  var displayedTrackArray = [];
  var id                  = "";
  var meta                = "";
  
  LOG("decodeMetaDataList ["+unencodedEntrys.length+"]")
       
  for (var i = 0; i < unencodedEntrys.length; i++) {
    displayedTrackArray[i] = {};
    
    id = utils.xmlExtractText(unencodedEntrys[i], "Id",  ""); 
    displayedTrackArray[i].trackId = id;
    LOG("id:"+id);

    // metadata is double-escaped
#ifdef DAVAAR
    meta = utils.xmlExtractText(unencodedEntrys[i], "Metadata",  "");
#else
    meta = utils.xmlExtractText(unencodedEntrys[i], "MetaData",  "");
#endif
    data = utils.xmlExtractText(meta, "upnp:artist", "unknown artist");
    displayedTrackArray[i].artist = utf8.decode(utils.htmlDecode(utils.htmlDecode(data)));
    data = utils.xmlExtractText(meta, "upnp:album", "unknown album");
    displayedTrackArray[i].album = utf8.decode(utils.htmlDecode(utils.htmlDecode(data)));
    data = utils.xmlExtractText(meta, "dc:title", "unknown title");
    displayedTrackArray[i].track = utf8.decode(utils.htmlDecode(utils.htmlDecode(data)));
    LOG("Title:"+ displayedTrackArray[i].track);
  };
  uiPlaylist.setTracks(displayedTrackArray);
}


uiPlaylist.getTracks = function(arrayOfIds)
{
  // Normally this would be done as a callback some time later (after the DS
  // has responded)
  //
  // get the subset tracks we are intersted in the call
  // uiPlaylist.setTracks(displayedTrackArray);
  
  var displayedTrackArray = [];
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
  
  LOG("Tracks to get = {"+trackListString+"}")

  if (trackListString == uiPlaylist.trackListString) {
    // No need to get more tracks we already have them.
    //
    // We should return as in cache
    //
    uiPlaylist.setTracks(uiPlaylist.displayedTrackArray);
    return;
  };
  
  uiPlaylist.trackListString = trackListString;
  
  lpecMessages.playlistReadList(trackListString, uiPlaylist.callbackLpecReadList);
  
};

// Idea = should we display the current playing track, not the first track
// Sounds like a nice idea to me.
//
uiPlaylist.setFirstTrackToDisplay = function(){

  for (var i = 0; i < uiPlaylist.currentPlaylist.length; i++){
    if (uiPlaylist.currentPlaylist[i] == uiPlaylist.firstTrackToDisplay){
      uiPlaylist.indexOfFirstTrackToDisplay = i;
      return;
    };
  };
  
  // We can't find the track to display, id there is one display the first track, otherwise none.
  //
  if (uiPlaylist.currentPlaylist.length > 0) {
    uiPlaylist.firstTrackToDisplay = uiPlaylist.currentPlaylist[0];
    uiPlaylist.indexOfFirstTrackToDisplay = 0;
    return;
  };
  
  uiPlaylist.indexOfFirstTrackToDisplay = 0;
  uiPlaylist.firstTrackToDisplay = 0;
};

// The basic idea is that a call to playlistUpdated() will update the complete playlist of upto 1000
// tracks. We don't have enough RAM to store all the track data for this so just get the 10 or
// so tracks which we will be displaying.
// This is done by an async callback (via LPEC) which in turn calls this.setTracks() with the full
// details of the 10 tracks we are interested in displaying.
// This callback to setTracks will cause the display to be updated as appropriate.
//
uiPlaylist.setTracks = function(displayedTrackArray){

  uiPlaylist.displayedTrackArray = displayedTrackArray;
  
  var highlightEntry = -1;
  var list = []
  
  for (var i = 0; i < displayedTrackArray.length; i++){
    list[i] = displayedTrackArray[i].track;
    if (displayedTrackArray[i].trackId == uiPlaylist.trackId) {
      highlightEntry = i;
    };
  };
  
  // Just create a 'config' structure for sWMLowestLevel and we are done
  //
  uiPlaylist.currentScrollWheel = {
    characterHeight:pronto.characterHeight,  // High of widgetList text in px. (24 = 18px, 32 = 24px)
    y: pronto.yOffset, // y offset in px to avoid headers etc
    widgetList: 'List',
    widgetSelector: 'Selector',
    highlightEntry: highlightEntry,
    highlightSelector: 'MARKER',
    updateSelectionCallback: uiPlaylist.selectionPlaylistUpdateCallback,
    list: list, // Of strings to display 0 <= length < this.config.pageSize
    selection: 0}; // 0 <= selection < list.length-1
  
  sWMLowestLevel.go(uiPlaylist.currentScrollWheel);
};


uiPlaylist.selectionPlaylistUpdateCallback = function (i)
{
  uiPlaylist.index = i;
  
  // LOG("selectionPlaylistUpdateCallback("+i+")");
  // TODO: Check for handling of empty playlist in ScrollWhellManagement
  //
  page("Playlist").widget("Artist").label = "";
  page("Playlist").widget("Track").label = "";
  page("Playlist").widget("Album").label = "";

  // Do nothing for the first entry "[Clear playlist]" and optional last entry.
  //
  if (i > uiPlaylist.displayedTrackArray.length) {
    return;
  };
  
  try {
    CF.page("Playlist").widget("Artist").label = uiPlaylist.displayedTrackArray[i].artist;
    CF.page("Playlist").widget("Track").label = uiPlaylist.displayedTrackArray[i].track;
    CF.page("Playlist").widget("Album").label = uiPlaylist.displayedTrackArray[i].album;
  } catch (e) {
    CF.page("Playlist").widget("Track").label = "**Error**";
  };
};

// engineTracksCallbackIdArray is called from engineTracks.
//
uiPlaylist.engineTracksCallbackIdArray = function(currentPlaylist)
{
  uiPlaylist.currentPlaylist = currentPlaylist; 
  uiPlaylist.playlistUpdated();
};
//
///
//
function uiPlaylist_firmMenuN()
{
  uiPlaylist_firmMenu();
  lpecMessages.playlistDeleteAll();
};

function uiPlaylist_firmMenuS()
{
  uiPlaylist_firmMenu();
  uiPlaylist_deleteTrack();
};

function uiPlaylist_deleteTrack()
{
  if(!uiPlaylist.somethingToDisplay){
    LOG("No track to delete.");
    return;
  };
  
  LOG("Menu: Delete track index " + uiPlaylist.index + " which is " + uiPlaylist.displayedTrackArray[uiPlaylist.index].trackId);

  lpecMessages.playlistDelete(uiPlaylist.displayedTrackArray[uiPlaylist.index].trackId);
};


function uiPlaylist_firmMenuW()
{
  uiPlaylist_firmMenu();
  
  if (handleShuffleRepeat.repeat)
  {
    lpecMessages.playlistRepeatOff()
  }
  else
  {
    lpecMessages.playlistRepeatOn()
  }
};

function uiPlaylist_firmMenuE()
{
  uiPlaylist_firmMenu();
  
  if (handleShuffleRepeat.shuffle)
  {
    lpecMessages.playlistShuffleOff()
  }
  else
  {
    lpecMessages.playlistShuffleOn()
  };
};

function uiPlaylist_firmMenuOk()
{
	uiPlaylist_firmMenu();
	uiPlaylist_displayCurrentTrack();
};

function uiPlaylist_firmMenu(){

  uiPlaylist.firmMenuVisible = !uiPlaylist.firmMenuVisible;

  handleFooter.updateAll();

  prontoHardKeys.setCompassKeys(uiPlaylist.select,uiPlaylist.pageUp,uiPlaylist.pageDown,uiPlaylist.doEast,uiPlaylist.doWest);

};
//
///
//
uiPlaylist._start = function()
{
	handleShuffleRepeat.update();
#ifdef DAVAAR
	lpec.addEventCallback(uiPlaylist.lpecCallbackTrackId, "Id", "Ds", "Playlist");
#else
	lpec.addEventCallback(uiPlaylist.lpecCallbackTrackId, "TrackId", "Ds", "Ds");
#endif
};


elab.add("uiPlaylist", null, uiPlaylist._start);
