////
//
// sWM.js [scroll Wheel Management]
//
// TODO:  Rename to sWM.js
//
// This module allows the newer sWMLowLevel module to interact
// with the older sWM style of interface. Eventually this module
// will become obsolete & be removed.
//
//
// uses sWMLowLevel
//
// Used in 2 places:
// 1: Radio for preset selection - in which case only 'select' is used.
// 2: DS for media library
//
// In these 
// NOTE: DS playlist display is via sWMLowestLevel
//
//////

var sWM = {};

sWM.goBack = null;
sWM.config = null;

// Depending on the current highlit / selected line invoke the Javascript.
//
sWM.doSelect = function (index)
{
   LOG("About to call ["+index+"] in select");
   sWM.selectItemCallback(index);
  
};

// Depending on the current highlit / selected line invoke the Javascript.
//
#if 0
sWM.doNext = function (index)
{
  sWM.doSelect(index);
};
#endif


sWM.doPrevious = function (index)
{    
  if (sWM.goBackCallback != null) {
    sWM.goBackCallback();
  };
};

#if 0

sWM.list = [];

sWM.entryText = function (index)
{
  return sWM.list[index];
};


sWM.updateList = function (list)
{
    sWM.list = [];

    for (var i=0; i < list.length; i++) {
      sWM.list[i] = list[i]; // {text: list[i].text};
    };
    
};

#endif
//
////
//
sWM.go = function (entryText, entryCount, widgetList, widgetSelector, y, charHeight, pageSize,
                   selectionUpdateCallback,
                   selectItemCallback,
                   goBackCallback,
                   highlightWidgetName,
                   highlightEntry,
                   initialSelectedEntry)
{

  sWM.selectItemCallback = selectItemCallback;
  sWM.goBackCallback = goBackCallback;

  // If we don't want any entry's highlit then default out.
  //
  if (highlightEntry == null)
  {
    highlightEntry = -1;
  };
  
  // If we don't specify any entry to select then select the first
  // one.
  //
  if (initialSelectedEntry == null)
  {
    initialSelectedEntry = 0;
  };

  if (initialSelectedEntry < 0)
  {
    initialSelectedEntry = 0;
  };

  // sWM.updateList(list);
    
  sWM.config = {
    pageSize: pageSize,
    characterHeight: charHeight,
    y: y,
    widgetList: widgetList,
    widgetSelector: widgetSelector,
    updateSelectionCallback: selectionUpdateCallback,
    entryCount: entryCount,
    entryText:  entryText,
    doSelected: sWM.doSelect,
    doNext:     sWM.doSelect, // Yes, call Selected in all cases.
    doPrevious: sWM.doPrevious, // AKA goBack
    highlightWidgetName: highlightWidgetName,  // Note, null is valid.
    highlightEntry: highlightEntry,
    initialSelectedEntry: initialSelectedEntry
  };
    
  sWMLowLevel.go(sWM.config);
    
};

/**
 * sWM.redraw causes the 'screen' to be redrawn.
 * It would usually be called if the text to be displayed had changed but not the
 * number of elements.
 * TODO: Test this function.
 */
sWM.redraw = function()
{
  sWMLowLevel.updateEntries();
  sWMLowLevel.redraw();
};

elab.add("sWM");
