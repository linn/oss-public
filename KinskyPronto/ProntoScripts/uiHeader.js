////
//
// uiHeader.js
//
// Add Pronto independed header.
// This header display key information about the state of the DS.
// Information displayed is:
//
//   * Title (Of playlist track, Radio Station or even source name)
//   * cover Art
//   * volume level (If PreAmp available)
//   * Progress through track
//
// Use http://radiotime-logos.s3.amazonaws.com/s9067q.png for dummy cover art
//
// For some pages (Now Playing & Now Listening) we don't want to display the
// header (As all the information is already on the screen), however we still
// need to track changes to volume etc. So we have the concept of turning the
// header on [uiHeader.on()] & off [uiHeader.off()] which only effects to
// diplaying of the header, not the tracking of updates.
//
// API Usage
//
// 'uiHeader.on/.off()' controls if the header is displayed or not.
// changes to the stored volume level etc will be still be maintained even in the
// '.off' state. Default is '.off' (i.e no header displayed).
//
// 'uiHeader.set*(value)' (*=Volume, Progress, CoverArtData)
// For setVolume -ve values represent mute.
// for setProgess value is an integer percentage
////
var uiHeader = {};

/**
  * Define all the UI widgets to initially be null.
  */
uiHeader.titleWidget = null;
uiHeader.coverArtWidget = null;
uiHeader.volumeWidget = null;
uiHeader.progressWidget = null;

uiHeader.state = {volume: null, imageData: null, progress: null, title: null};

#if 0
  uiHeader.state = {volume: 55, imageData: null, progress: 45, title: "Title Here"};
#endif


uiHeader.widgets = {TITLE:0, COVERART:1, VOLUME:2, PROGRESS:3};

/**
 * Track the display state.
 */
uiHeader.enabled = false;

uiHeader.enable = function()
{
  uiHeader.enabled = true;
  uiHeader.redrawAll();
};

uiHeader.disable = function()
{
  uiHeader.enabled = false;
};

/**
  * Redraw the header with the current (saved) values.
  * The idea is that this procedure should be called during page transitions.
  *
  */
uiHeader.redrawAll = function()
{
  // Disable the header on the 9400, it's too big for it's screen.
  //
  if (pronto.is9400)
  {
    return;
  };

  if (!uiHeader.enabled)
  {
    return;
  };

  // Remove all the existing widgets.
  //
  if (uiHeader.volumeWidget != null)
  {
    uiHeader.volumeWidget.remove();
    uiHeader.volumeWidget = null;
  };

  if (uiHeader.coverArtWidget != null) {
    uiHeader.coverArtWidget.remove();
    uiHeader.coverArtWidget = null;
  };

  if (uiHeader.progressWidget != null) {
    uiHeader.progressWidget.remove();
    uiHeader.progressWidget = null;
  };

  if (uiHeader.titleWidget != null) {
    uiHeader.titleidget.remove();
    uiHeader.titleWidget = null;
  };

  // Draw progress indicator
  //
  if (uiHeader.state.progress != null)
  {
    uiHeader.progressWidget = uiHeader.createWidget(uiHeader.widgets.PROGRESS);
    uiHeader.progressWidget.setImage(kinskyUiElements.getProgressImage(uiHeader.state.progress));
    uiHeader.progressWidget.label = "12:34";  // TODO Remove this, just displat the dial.
  };

  // Draw volume indicator
  //
  
  if (uiHeader.state.volume != null)
  {
    uiHeader.volumeWidget = uiHeader.createWidget(uiHeader.widgets.VOLUME);
  
    if (uiHeader.state.volume < 0)
    {
      uiHeader.volumeWidget.setImage(kinskyUiElements.getVolumeImage(-uiHeader.state.volume));
      uiHeader.volumeWidget.label = "*" + (-uiHeader.state.volume) + "*";
      uiHeader.volumeWidget.color = 0x0000ff; // Red
    }
    else
    {
      uiHeader.volumeWidget.setImage(kinskyUiElements.getVolumeImage(uiHeader.state.volume));
      uiHeader.volumeWidget.label = uiHeader.state.volume;
    }
  };
 
  // Draw title text
  //
  if (uiHeader.state.title != null)
  {
    uiHeader.titleWidget = uiHeader.createWidget(uiHeader.widgets.TITLE);
    uiHeader.titleWidget.label = uiHeader.state.title;
  };

  // Draw image data
  //  
  if (uiHeader.state.imageData != null)
  {
    uiHeader.coverArtWidget = uiHeader.createWidget(uiHeader.widgets.COVERART);
    uiHeader.coverArtWidget.stretchImage = true;
    uiHeader.coverArtWidget.setImage(uiHeader.state.imageData);
  };
};


uiHeader.redraw = function()
{
  uiHeader.redrawAll();
};



/**
 * Sets the volume level, if -ve the volume is assumed to be muted.
 * If not called (or called with null) then no volume control assumed
 * (or displayed).
 */
uiHeader.setVolume = function(volume)
{    
  uiHeader.state.volume = volume;
  uiHeader.redrawAll();
};

/**
 * Sets (or clears) the cover art (or radio station logo) to display.
 * pass url = null to clear.
 */
uiHeader.setCoverArtData = function(data)
{
  uiHeader.state.imageData = data;
  uiHeader.redrawAll();
};


uiHeader.setProgress = function(progress)
{
  uiHeader.state.progress = progress;
  uiHeader.redrawAll();
};


uiHeader.setTitle = function(title)
{
  uiHeader.state.title = title;
  uiHeader.redrawAll();
};


uiHeader.createWidget = function(widgetType){

  var result = GUI.addPanel();

  // All widgets align to the top of the screen.
  //
  result.top = 0;
  result.halign = "center";
  result.valign = "center";
  result.stretchImage = true;

  // Set height & width for the widget - this is usually the height of
  // the header - except in the case of the 9400 Cover Art which is
  // enlarged to 45px (Otherwise it is just too small).
  //
  if (pronto.is9600)
  {
    result.height = 60;
    result.width = 60;
  };

  if (pronto.is9400)
  {
    result.height = 30;
    result.width = 30;
    
    if (widgetType == uiHeader.widgets.COVERART)
    {
        result.height = 45;
        result.width = 45;
    };
  };

  if (widgetType == uiHeader.widgets.TITLE)
  {
    result.left = (pronto.is9600 ? 115 : 34);
    result.width = (pronto.is9600 ? 237 : 115);
  }
  else if (widgetType == uiHeader.widgets.COVERART)
  {
    result.left = (pronto.is9600 ? 56 : 196);
  }
  else if (widgetType == uiHeader.widgets.VOLUME)
  {
    result.left = (pronto.is9600 ? 429 : 161); // 364 : 127
  }
  else if (widgetType == uiHeader.widgets.PROGRESS)
  {
    result.left = (pronto.is9600 ? 429 : 161);
  };

  result.color = 0xdddddd;
 // result.font = "ProntoMaestro";  // ToDo: Change to Droid Sana when decission made.
  result.fontSize = 18;
  result.transparent = true;
  result.label = "";
  result.visible = true;

  return result;    
};

elab.add("uiHeader");
