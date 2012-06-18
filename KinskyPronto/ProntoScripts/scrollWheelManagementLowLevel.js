////
//
// sWMLowLevel.js
//
// TODO: Rename to sWM_Low.
//
// sWMLowLevel.go(..); is the only external function.
//
// We take a very simplistic, but effecient philosophy to manage the scroll wheel and it's onRotary function
//
// Each Pronto page can register an array of objects which have a text element & a javascript string to be executed if the
// OK or NEWS buttons are pressed. Also callbacks need to be provided to the text to be displayed for each entry.
//
////

// TODO: we ALWAYS redraw the widgetList text - not exactly effcient.
//

// TODO: Add support for initialSelectedEntry

// Despite our best efforts at OO programming you looks all context in the onRotary call back function.
// so we keep all the state external. Not really a problem (but a bit ugly) as there can be only one visible
// scroll wheel.
//

dummyScrollWheelConfiguration = {
  pageSize: 13, // How many lines of text do we get per page. (17 = 18px, 13 = 24px)
  characterHeight:30,  // High of widgetList text in px. (24 = 18px, 32 = 24px)
  y: 60, // y offset in px to avoid headers etc
  widgetList: "",
  widgetListWidget: null,
  widgetSelector: "",
  widgetSelectorWidget: null,
  updateSelectionCallback: function(){},
  highlightWidgetName: null,
  highlightEntry: -1,
  highlightWidget: null,
  entryCount: 0, // Number of rows to display (In total, not per-page), 0 == nothing to display == disable scrolling.
  entryText: function (){return "";}, // Callback to return text per visible row.
  doSelected: function (){},
  doNext: function (){},
  doPrevious: function (){},
  initialSelectedEntry: 0, // Item initially selected. (Need to do some magic with currentSelection)
};

// Based on sWMLowLevel
//

sWMLowLevel = {};

sWMLowLevel.disabled = true;

// After quite a bit of though the config & state have been separated, hopfully this will make life
// a little easier futher up the food chain.
//
sWMLowLevel.config = dummyScrollWheelConfiguration;
sWMLowLevel.initialState = {
  currentPage: 0, // offset from 0
  offset:0, // == currentPage * config.pageSize
  maxPage: 0,
  currentSelection: 0, // 0 <= currentSelection < this.config.entryCount
  currentSelectionInPage: 0, // 0 <= currentSelection < this.config.pageSize
  list:[] // Of strings to display 0 <= length < this.config.pageSize
};

sWMLowLevel.state = sWMLowLevel.initialState;

#if 0

sWMLowLevel.putConfig = function(){

  LOG("swLL.config = {");
  LOG('.pageSize ['+this.config.pageSize+']');
  LOG('.characterHeight ['+this.config.characterHeight+']');
  LOG('.y ['+this.config.y+']');
  LOG('.widgetList ['+this.config.widgetList+']');
  LOG('.widgetSelector ['+this.config.widgetSelector+']');
  LOG('.updateSelectionCallback ['+this.config.updateSelectionCallback+']');
  LOG('.entryCount ['+this.config.entryCount+']');
  LOG('.entryText ['+this.config.entryText+']');
  LOG('.doSelected ['+this.config.doSelected+']');
  LOG('.doNext ['+this.config.doNext+']');
  LOG('.doPrevious ['+this.config.doPrevious+']');
  LOG("}");

};

sWMLowLevel.putState = function(){

  LOG('swLL.state = {');
  LOG('.currentPage ['+sWMLowLevel.state.currentPage+']');
  LOG('.offset ['+sWMLowLevel.state.offset+']');
  LOG('.maxPage ['+sWMLowLevel.state.maxPage+']');
  LOG('.currentSelection ['+sWMLowLevel.state.currentSelection+']');
  LOG('.currentSelectionInPage ['+sWMLowLevel.state.currentSelectionInPage+']');
  LOG('.list ['+sWMLowLevel.state.list+']');
  LOG("}");

};

#endif

sWMLowLevel.put = function(){

#if 0
  
  sWMLowLevel.putState();
  sWMLowLevel.putConfig();

#endif

};

sWMLowLevel.initialiseState = function()
{

  sWMLowLevel.state = {
    currentPage: 0, // offset from 0
    offset:0, // == currentPage * config.pageSize
    maxPage: 0,
    currentSelection: 0, // 0 <= currentSelection < this.config.entryCount
    currentSelectionInPage: 0, // 0 <= currentSelection < this.config.pageSize
    list:[] // Of strings to display 0 <= length < this.config.pageSize
  };
  
  if (this.config.initialSelectedEntry < 0)
  {
    this.config.initialSelectedEntry = 0;
  };

  this.state.offset = this.config.initialSelectedEntry;
  
  var initialSelectedEntry = this.config.initialSelectedEntry;
  
  while (initialSelectedEntry >= this.config.pageSize)
  {
    initialSelectedEntry -= this.config.pageSize;
    this.state.currentPage++;
  };
  this.state.currentSelectionInPage = initialSelectedEntry;


  sWMLowLevel.config.widgetSelectorWidget = CF.widget(sWMLowLevel.config.widgetSelector);
  sWMLowLevel.config.widgetListWidget     = CF.widget(sWMLowLevel.config.widgetList);
  
  // TODO: Comment out the following line when we have recalibratedscreen font size.
#if 0
  sWMLowLevel.config.widgetListWidget.font = pronto.font;
#endif
  
  if (sWMLowLevel.config.highlightWidgetName != null)
  {
    sWMLowLevel.config.highlightWidget      = CF.widget(sWMLowLevel.config.highlightWidgetName);
  }
  else
  {
    sWMLowLevel.config.highlightWidget      = null;
  };
  

}


// Redraw the current page state.
//
sWMLowLevel.redraw = function(){

  if (sWMLowLevel.disabled) {return};
  
  sWMLowLevel.put();
  
  //LOG("mod: " + mod + " rem: " + rem)
    
  // CF.widget(this.widgetSelector).label = this.currentSelection + ":" + mod + ":" + rem;
  var offset = (sWMLowLevel.state.currentSelectionInPage * sWMLowLevel.config.characterHeight);
  // LOG('Offset is ['+offset+']');
  this.config.widgetSelectorWidget.top = (sWMLowLevel.config.y + offset);
  
  var foundHighlight = -1;
  var text = "";
  for (var i = 0; i < this.state.list.length; i++)
  {
    text += this.state.list[i] + "\n";
    if (this.state.offset + i == this.config.highlightEntry)
    {
      foundHighlight = i;
    };
  };

  this.config.widgetListWidget.label = text;

  if (sWMLowLevel.config.highlightWidget != null)
  {
    if (foundHighlight >= 0)
    {
       sWMLowLevel.config.highlightWidget.visible = true;
       sWMLowLevel.config.highlightWidget.top = (sWMLowLevel.config.y +
	                                         (foundHighlight * sWMLowLevel.config.characterHeight));
    }
    else
    {
      sWMLowLevel.config.highlightWidget.visible = false;
    };
  };

  sWMLowLevel.config.updateSelectionCallback(sWMLowLevel.state.currentSelection);
  
};


// The page we want to display has changed so we need to get the lines to display
//
sWMLowLevel.updateEntries = function() {

  // Throw away the old lines.
  //
  this.state.list = [];
  
  this.state.maxPage = parseInt(this.config.entryCount / this.config.pageSize);
  this.state.offset = this.state.currentPage * this.config.pageSize;
  
  // How may lines do we need to display - I suppose this should be entryCount REM pageSize
  // In theory shouldn't be zero unless we are in the paradoxical case of having nothing at all to display.
  //
  var numberOfLines = this.config.entryCount  % this.config.pageSize;
  
  for (var i = 0; i < this.config.pageSize; i++) {
    if (i+this.state.offset >= this.config.entryCount) {return};
    this.state.list[i] = this.config.entryText(i+this.state.offset);
    
    /**
     * Put test for current selection display here.
    
    if (i+this.state.offset == this.config.currentActive) {
    
    };
    */
  }

};


sWMLowLevel.pageUp = function(){

  LOG('N');
    
  if (sWMLowLevel.disabled) {return};
  

  // If we are already at the first page, do nothing.
  //
  if (sWMLowLevel.state.currentPage == 0) {return};
  
  sWMLowLevel.state.currentPage--;
  sWMLowLevel.updateEntries();

  // We call onRotary() because it will check that the value of currentSelectionInPage currentSelection are OK.
  //
  sWMLowLevel.onRotary(0); // Implicit call to 'sWMLowLevel.redraw();'
  
};
  
sWMLowLevel.pageDown = function(){
  LOG('S');

  if (sWMLowLevel.disabled) {return};


  // If we are already at the last page, do nothing.
  //
  if (sWMLowLevel.state.currentPage == sWMLowLevel.state.maxPage) {return};
  
  sWMLowLevel.state.currentPage++;
  
  sWMLowLevel.updateEntries();

  // We call onRotary() because it will check that the value of currentSelectionInPage currentSelection are OK.
  //
  sWMLowLevel.onRotary(0); // Implicit call to 'sWMLowLevel.redraw();'
  
};


// Depending on the current highlit / selected line invoke the Javascript.
//
sWMLowLevel.select = function ()
{
  if (sWMLowLevel.disabled)
  {
    return;
  };

  LOG('Ok');    
  sWMLowLevel.config.doSelected(sWMLowLevel.state.currentSelection);
};

// Depending on the current highlit / selected line invoke the Javascript.
//
sWMLowLevel.next = function()
{
  if (sWMLowLevel.disabled){
    return;
  };
    
  LOG('E');
  sWMLowLevel.config.doNext(sWMLowLevel.state.currentSelection);
};

sWMLowLevel.previous = function()
{
  if (sWMLowLevel.disabled)
  {
    return;
  };

  LOG('W');
  sWMLowLevel.config.doPrevious(sWMLowLevel.state.currentSelection);
};


// Previously there was a second, resetScroll, boolean parameter which determined if we
// were to reset the scrolling selection. Because we now update the text on the fly this is
// no longer necessary and we ALWAYS reset the selection.
//
// Should we even use this procedure - I guess so as it gives us a simple way to reset our view
// of the world.
//


/*
  onRotary = this.onRotary;
  prontoHardKeys.Ok = this.select;
  prontoHardKeys.N = this.pageUp;
  prontoHardKeys.S = this.pageDown;
  prontoHardKeys.E = this.next;
  prontoHardKeys.W = this.previous;
*/
sWMLowLevel.disable = function()
{
  sWMLowLevel.disabled = true;
  onRotary = doNothing;
  prontoHardKeys.setCompassKeys();
};

sWMLowLevel.enable = function()
{
  sWMLowLevel.disabled = false;
  onRotary = sWMLowLevel.onRotary;
  prontoHardKeys.setCompassKeys(this.select, this.pageUp, this.pageDown, this.next, this.previous);
};

sWMLowLevel.go = function(configuration)
{
  if (configuration == null)
  {
    sWMLowLevel.disable();
    LOG("Scroll wheel disabled.")
    return;
  };
  
  if (configuration.entryCount <= 0)
  {
    sWMLowLevel.disable();
    LOG("Scroll wheel disabled because entryCount <= 0.")
    return;
  };

  LOG("Scroll wheel enabled.");
  
  sWMLowLevel.config = configuration;
  sWMLowLevel.initialiseState();
  
  sWMLowLevel.enable();
  
  sWMLowLevel.updateEntries();
  sWMLowLevel.onRotary(0);  // Implicit call to 'sWMLowLevel.redraw();'

  LOG("Scroll wheel enabled - done.");
};

sWMLowLevel.onRotary = function(clicks)
{
  if (sWMLowLevel.disabled) {return;}

  // LOG('scroll('+clicks+')');
  
  if (clicks < 0) {
    clicks = -1;
  } else if (clicks > 0) {
    clicks = 1;
  };
    
  // Have tried to reduce the scroll wheet sensitivity but the Pronto still 'clicks' for each rotation which
  // makes the interface non-intuative as you hear two clicks but the list only moves by one.
  //
  sWMLowLevel.state.currentSelectionInPage += clicks // parseInt(clicks/2); // Way too sensitive ...

  // Make sure we haven't jumped off either end of our list.
  //
  if (sWMLowLevel.state.currentSelectionInPage < 0)
  {
    sWMLowLevel.state.currentSelectionInPage = 0;
  };

  if (sWMLowLevel.state.currentSelectionInPage >= sWMLowLevel.state.list.length)
  {
    sWMLowLevel.state.currentSelectionInPage = sWMLowLevel.state.list.length - 1;
  };
   
  sWMLowLevel.state.currentSelection = sWMLowLevel.state.currentSelectionInPage +
					sWMLowLevel.state.offset;

  sWMLowLevel.redraw();
};

sWMLowLevel._init = function()
{  
  // Start by disabling the scroll wheel - we can enable when needed.
  //
  sWMLowLevel.go(null);

};


elab.add("sWMLowLevel", sWMLowLevel._init, null);

#if 0
function sWMLowLevelTest()
{
  var ourConfig = {
    pageSize: 8, // was 13, // How many lines of text do we get per page. (17 = 18px, 13 = 24px)
    characterHeight:30,  // High of widgetList text in px. (24 = 18px, 32 = 24px)
    y: 60, // y offset in px to avoid headers etc
    widgetList: 'List',
    widgetSelector: 'Selector',
    updateSelectionCallback: function(index){LOG('Current Selection is ' + index)},
    entryCount: 23, // Number of rows to display (In total, not per-page)
    entryText: function (){return "";}, // Callback to return text per visible row.
    doSelected: function (){},
    doNext: function (){},
    doPrevious: function (){}
  };
  
  LOG('sWMLowLevelTest()');
  LOG("widgetSelector1:" + ourConfig.widgetSelector)
  sWMLowLevel.go(ourConfig);

};

#endif
