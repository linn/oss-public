////
//
// ui_Presets.js
//
// Support for Radio Presets UI Page
//
////

/*

ALIVE Ds 4c494e4e-0050-c221-729d-d1000003013f
ALIVE Preamp 4c494e4e-0050-c221-729d-d10000030133
ALIVE MediaRenderer 4c494e4e-0050-c221-729d-d10000030171


ACTION Preamp/Preamp 1 Volume
RESPONSE "38"

ACTION Ds/Radio 1 Read "1"
RESPONSE "&lt;DIDL-Lite xmlns:dc=&quot;http://purl.org/dc/elements/1.1/&quot; xm
lns:upnp=&quot;urn:schemas-upnp-org:metadata-1-0/upnp/&quot; xmlns=&quot;urn:sch
emas-upnp-org:metadata-1-0/DIDL-Lite/&quot;&gt;&lt;item&gt;&lt;dc:title&gt;AVRO
Radio Festival Classique&lt;/dc:title&gt;&lt;dc:description&gt;Butien. Gewoon. K
lassiek&lt;/dc:description&gt;&lt;upnp:genre&gt;Classical&lt;/upnp:genre&gt;&lt;
res bitrate=&quot;256000&quot;&gt;http://opml.radiotime.com/Tune.ashx?id=s44185&
amp;amp;formats=mp3,wma&amp;amp;username=linnproducts&amp;amp;partnerId=16&lt;/r
es&gt;&lt;upnp:albumArtURI&gt;http://radiotime-logos.s3.amazonaws.com/s44185q.pn
g&lt;/upnp:albumArtURI&gt;&lt;upnp:class&gt;object.item.audioItem&lt;/upnp:class
&gt;&lt;/item&gt;&lt;/DIDL-Lite&gt;"

http://radiotime-logos.s3.amazonaws.com/s44185q.png
http://opml.radiotime.com/Tune.ashx?id=s44185&formats=mp3,wma&username=linnproducts


<DIDL-Lite
  xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/"
  xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/">
 <item>
  <dc:title>AVRO Radio Festival Classique>/dc:title>
  <dc:description>Butien. Gewoon. Klassiek</dc:description>
  <upnp:genre>Classical</upnp:genre>
  <res bitrate="256000">http://opml.radiotime.com/Tune.ashx?id=s44185&formats=mp3,wma&username=linnproducts&partnerId=16</res>
  <upnp:albumArtURI>http://radiotime-logos.s3.amazonaws.com/s44185q.png</upnp:albumArtURI>
  <upnp:class>object.item.audioItem</upnp:class>
 </item>
</DIDL-Lite>

*/

var ui_Presets = {};

ui_Presets.enabled = false;
ui_Presets.tracks = []; // TODO: we only use ui_Presets.tracks[*].id so could store less info.

ui_Presets.currentList = ["Loading Radio Presets.."];

ui_Presets.currentIndex = -1;

      
ui_Presets.selectItemCallback = function(selectedId){
  // 
  //
  if (selectedId < 0)
  {
    return;
  };

  if (ui_Presets.tracks.length <= 0)
  {
    return;
  };

  if (selectedId >= ui_Presets.tracks.length)
  {
    return;
  };

  ui_Presets.selectPreset(ui_Presets.tracks[selectedId].id);

};

// ...
//
ui_Presets.selectPreset = function(i)
{
  LOG("ui_Presets.selectPreset "+i);
  engine_Radio.setPreset(i);
};

// .updatePresets is called as a callback (set in engine_Radio.registerPresetsCallback(..))
//
ui_Presets.updatePresets = function (tracks)
{

  LOG("ui_Presets.updatePresets:" + tracks.length);

  ui_Presets.tracks = tracks;

  if (tracks.length == 0) {
    ui_Presets.currentList =  ["No radio presets found."];
  
    ui_Presets.currentIndex = -1;
    ui_Presets.displayList();
    return;
  };
  
#ifdef LOGGING
    for (var i = 0; i < tracks.length; i++){
    };
#endif

  ui_Presets.currentList = [];
  ui_Presets.currentIndex = -1;
  LOG("preset = ");
  for (var i = 0; i < tracks.length; i++)
  {

    LOG("["+i+"]={" + tracks[i].id + "," + tracks[i].title);
    //LOG("["+i+"]={" + tracks[i].id + "," + tracks[i].title + (tracks[i].currentPreset ? "}***" : "}"));
	  
    // Have we found the currently playing preset?
    //
    if (tracks[i].currentPreset)
    {
      ui_Presets.currentIndex = i;
    };
    
//    ui_Presets.currentList.push(tracks[i].title);
    //ui_Presets.currentList.push(utils.htmlDecode(UTF8(tracks[i].title)));
    // ui_Presets.currentList.push(UTF8(tracks[i].title));
    ui_Presets.currentList.push(tracks[i].title);
    
  };

  ui_Presets.displayList();

};

ui_Presets.updateSelectionCallback = function(index)
{
  var pageSize = pronto.uiLibraryPageSize;    // Same basic screen as uiLibrary (NOT uiPlaylist);, // page size
  var entries = ui_Presets.currentList.length;
  var entry = index;
  
  ui_Presets.statusLineWidget.label = "Radio Presets " + utils.pageXofY(entry, entries, pageSize);
};

ui_Presets.entryText = function(index){

  if ((index < 0) || (index >= ui_Presets.currentList.length))
  {
    return "..";
  };

  return ui_Presets.currentList[index];
};

ui_Presets.displayList = function()
{
  if (!ui_Presets.enabled)
  {
    return;
  };

  sWM.go(
    ui_Presets.entryText,
    ui_Presets.currentList.length,
    "List",
    "Selector",
    pronto.yOffset, // Y offset
    pronto.characterHeight, // character height
    pronto.uiLibraryPageSize,    // Same basic screen as uiLibrary (NOT uiPlaylist);, // page size
    ui_Presets.updateSelectionCallback,
    ui_Presets.selectItemCallback,
    null, // No goBackCallback
    "MARKER",
    ui_Presets.currentIndex,
    ui_Presets.currentIndex);    // Select first entry
};

// Enable the display of UI
//
ui_Presets.enable = function()
{
  ui_Presets.statusLineWidget = CF.page("Presets").widget("STATUS_LINE");

  engineSources.selectDs();
  engineSources.internalDsSourceSelect(engineSources.INTERNAL_SOURCES.RADIO);

  ui_Presets.enabled = true;
  doFirmMenu = doNothing;

  setFunctionKeys(engine_Radio.playPrevious, engine_Radio.playNext, engine_Radio.toggleStopPlay, doNothing, doNothing);

  ui_Presets.displayList();
};

// Disable the display of the UI
//
ui_Presets.disable = function()
{
  // Stop the up, down, left, right & OK buttons doing anything.
  //
  ui_Presets.enabled = false;
  LOG("ui_Presets.disabled");
};


ui_Presets._start = function()
{
  engine_Radio.registerPresetsCallback(ui_Presets.updatePresets);
};

elab.add("ui_Presets", null, ui_Presets._start);
