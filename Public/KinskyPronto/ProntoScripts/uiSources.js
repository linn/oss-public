////
//
// uiSources.js (Pronto variant)
//
// Support for Source Selection UI Page
//
////
var uiSources = {};

uiSources.currentList = [{
      text: "Loading Source List..",
      select: 'doNothing();'}];


uiSources.selectInternalSource = function(index){

  engineSources.selectInternalSource(index);

};

uiSources.selectExternalSource = function(index){

  engineSources.selectExternalSource(index);

};

uiSources.selectSource = function(index){

  LOG("Would like to select source [" + index + "]");
  
  if (configuration.preAmpType == 0) {
    LOG("But there is no preamp");
  } else if (configuration.preAmpType == 1) {
      uiSources.selectInternalSource(index);
  } else {
      uiSources.selectExternalSource(index);
  };

};


uiSources.updateSelectionCallback = function(index){

  var pageSize = pronto.uiLibraryPageSize;
  var entries = uiSources.currentList.length;
  var entry = index;

  LOG("uiSources.updateSelectionCallback [" + index + "]");
  
  CF.page("Library").widget("STATUS_LINE").label = "Sources " + utils.pageXofY(entry, entries, pageSize);
  
};

uiSources.displayList = function(){

  // TODO: This will no longer work as the parameters to sWM have changed.
  // No longet pass a list.
  //
  sWM.go(
    uiSources.currentList,
    "List",
    "Selector",
    pronto.yOffset, // Y offset
    pronto.characterHeight, // character height
    pronto.uiLibraryPageSize, // page size
    uiSources.updateSelectionCallback);
    
};

// Enable the display of UI
//
uiSources.enable = function(){

  CF.page("Sources").widget("LOADING").visible = false;

  handleShuffleRepeat.update();
  
  uiSources.displayList();

};

// Disable the display of the UI
//
uiSources.disable = function(){};

elab.add("uiSources");