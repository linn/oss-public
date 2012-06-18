////
//
// sWMLowestLevel.js
//
// sWMLowestLevel.go(..) is the only external interface.
//
// TODO: Rename to sWM_Base.
//
// We take a very simplistic, but effecient philosophy to manage the scroll wheel and it's onRotary function
//
// Each Pronto page can register an array of objects which have a text element & a javascript string to be executed if the
// OK or NEWS buttons are pressed. Also callbacks need to be provided to the text to be displayed for each entry.
//
////


/*
lowest level interface, so no access to the NSEW+Go keys, only the outside rotary
control.

Parameters:

pageSize,
characterHeight,
y,
widgetList,
widgetSelector,
list[].
updateselectionCallback(index), 0..list.length-1

No no page up / down, next, previous as these all handled by higher level procedures

*/


// Despite our best efforts at OO programming you looks all context in the onRotary call back function.
// so we keep all the state external. Not really a problem (but a bit ugly) as there can be only one visible
// scroll wheel.
//

dummyScrollWheelConfigurationLowestLevel = {
  characterHeight:30,  // High of widgetList text in px. (24 = 18px, 32 = 24px)
  y: 60, // y offset in px to avoid headers etc
  widgetList: "",
  widgetSelector: "",
  highlightSelector: "",
  highlightEntry: -1,
  updateSelectionCallback: function(){},
  list:[], // Of strings to display 0 <= length < this.config.pageSize
  selection: 0, // 0 <= selection < list.length-1
};

// Based on sWMLowestLevel
//

sWMLowestLevel = {};

sWMLowestLevel.disabled = true;

// After quite a bit of though the config & state have been separated, hopfully this will make life
// a little easier futher up the food chain.
//
sWMLowestLevel.config = dummyScrollWheelConfigurationLowestLevel;


sWMLowestLevel.lines = 0; // 0 <= config.selection < list.length-1
sWMLowestLevel.text = "";

sWMLowestLevel.put = function(){

#if 0
  
  LOG("swLeL.lines = " + sWMLowestLevel.lines);
  LOG("swLeL.config = {");
  LOG(".selection = " + sWMLowestLevel.config.selection + ", .highlightEntry = " + sWMLowestLevel.config.highlightEntry);
  LOG('.characterHeight ['+sWMLowestLevel.config.characterHeight+'], .y ['+sWMLowestLevel.config.y+']');
  LOG('.highlightSelector['+sWMLowestLevel.config.highlightSelector+'], .widgetList ['+sWMLowestLevel.config.widgetList+'], .widgetSelector ['+sWMLowestLevel.config.widgetSelector+']');
  LOG('.updateSelectionCallback ['+sWMLowestLevel.config.updateSelectionCallback+']}');

#endif

};

// Redraw the current page state.
//
sWMLowestLevel.redraw = function(){

  if (sWMLowestLevel.disabled) {return};
  
  sWMLowestLevel.put();
  
  //LOG("mod: " + mod + " rem: " + rem)
    
  // CF.widget(this.widgetSelector).label = this.config.selection + ":" + mod + ":" + rem;
  var offset = (sWMLowestLevel.config.selection * sWMLowestLevel.config.characterHeight); //getCharacterHeight(18))
  // LOG('Offset is ['+offset+']');
  CF.widget(sWMLowestLevel.config.widgetSelector).top = (sWMLowestLevel.config.y + offset);

  if (sWMLowestLevel.config.highlightEntry >= 0) {
    offset = (sWMLowestLevel.config.highlightEntry * sWMLowestLevel.config.characterHeight); //getCharacterHeight(18))
    // LOG('Offset is ['+offset+']');
    CF.widget(sWMLowestLevel.config.highlightSelector).visible = true;
    CF.widget(sWMLowestLevel.config.highlightSelector).top = (sWMLowestLevel.config.y + offset);

  } else {
    CF.widget(sWMLowestLevel.config.highlightSelector).visible = false;
  };
  
  CF.widget(this.config.widgetList).label = this.text;
    
  sWMLowestLevel.config.updateSelectionCallback(sWMLowestLevel.config.selection);
  
};

sWMLowestLevel.go = function (configuration) {

  if (configuration == null) {
    sWMLowestLevel.disabled = true;
    LOG("Scroll wheel disabled.")
    return;
  };
  
  if (configuration.list.length <= 0) {
    sWMLowestLevel.disabled = true;
    LOG("Scroll wheel disabled because list is null.")
    return;
  };

  LOG("Scroll wheel enabled.");
  onRotary = sWMLowestLevel.onRotary;  
  sWMLowestLevel.disabled = false;
  sWMLowestLevel.config = configuration;
  sWMLowestLevel.text = "";
  sWMLowestLevel.lines = sWMLowestLevel.config.list.length;
  
  for (var i = 0; i < sWMLowestLevel.config.list.length; i++) {
    sWMLowestLevel.text += sWMLowestLevel.config.list[i] + "\n";
  };

  sWMLowestLevel.onRotary(0);
  sWMLowestLevel.redraw();
  
};

// Make sure we haven't jumped off either end of our list.
//
// range == 0 <= selcction < lines
//
sWMLowestLevel.normalizeSelection = function(selection){

  if (selection < 0) {
    selection = 0;
  };

  if (selection >= sWMLowestLevel.lines) {
    selection = sWMLowestLevel.lines - 1;
 };

  return selection;
}

sWMLowestLevel.onRotary = function(clicks){
  if (sWMLowestLevel.disabled) {return;}

  // LOG('scroll('+clicks+')');
  
  if (clicks < 0) {
    clicks = -1;
  } else if (clicks > 0) {
    clicks = 1;
  };

  // Have tried to reduce the scroll wheet sensitivity but the Pronto still 'clicks' for each rotation which
  // makes the interface non-intuative as you hear two clicks but the list only moves by one.
  //
  sWMLowestLevel.config.selection += clicks // parseInt(clicks/2); // Way too sensitive ...


  sWMLowestLevel.config.selection = sWMLowestLevel.normalizeSelection(sWMLowestLevel.config.selection);
  sWMLowestLevel.redraw();
};

// TODO: Is the ._init function strictly necessary?
//
sWMLowestLevel._init = function(){

  onRotary = sWMLowestLevel.onRotary;
  
  // Start by disabling the scroll wheel - we can enable when needed.
  //
  sWMLowestLevel.go(null);

};

elab.add("sWMLowestLevel", sWMLowestLevel._init, null);

#if 0
function sWMLowestLevelTest() {

  var ourConfig = {
    characterHeight:30,  // High of widgetList text in px. (24 = 18px, 32 = 24px)
    y: 60, // y offset in px to avoid headers etc
    widgetList: 'List',
    widgetSelector: 'Selector',
    updateSelectionCallback: function(index){LOG('Current Selection is ' + index)},
    list: ["a","b","c","d","e", "f", "g", "h", "i", "j", "k"],
    selection: 2,
   };
  
  sWMLowestLevel.go(ourConfig);

};

#endif
