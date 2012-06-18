////
//
// uiLibrary.js
//
// Support for Media Library UI Page
//
// CURRENT
//
// Usage:
// Called in engineLibray.js & play.js
//
// uiLibrary.enable()
// uiL
////

/*
ToDo:

// Currently we ignore location, but we shouldn't as it makes library browsing
// less efficent.
//
// Commented out as shows a bug (== crash) in uiLibrary's caching algorithm.
//

Look at uiLibrary.initialSelectedEntries[] & it's usage

*/

#define engineLibrary_REQUEST_CHUNK_SIZE 30

var uiLibrary = {};

uiLibrary.StatusWidget = null;
uiLibrary.parentId = 0;
uiLibrary.idRequestCallback = function(){};
uiLibrary.currentId = 0;
uiLibrary.initialSelectedEntries = [];
uiLibrary.selectedId = "";

uiLibrary.selectItemCallback = function(selectedId)
{
  // If the first entry isn't 'go up a level' then we will have displayed a line to go
  // 'up' a level, we need to ignore this line.
  //
  if (!uiLibrary.atRoot) {
    selectedId--;
  };

  if (selectedId < 0) {
    LOG("Selected up-a-level");
    engineLibrary.browse(uiLibrary.parentId);
    return;
  };
  
	var selection = asyncBlockLoadCache.read(selectedId);
  
	if (selection == "..")
	{
		LOG("Selected blank entry, do nothing");
	}
	else if (selection.isFolder)
	{
		engineLibrary.browse(selection.id);
	}
	else
	{
		engineTracks.insertTrack(selection);
	};
};

uiLibrary.computeSelection = function(){

  var result = {
	  selectionIsFolder: false,
          selectedFolder: null,
	  selectionIsTrack: false,
	  selectedTrack: null};

  // selectedId is indexed from 0.
  //
  var selectedId = uiLibrary.selectedId;
  
  // If the first entry isn't 'go up a level' then we will have displayed a line to go
  // 'up' a level, we need to ignore this line.
  //
  if (!uiLibrary.atRoot) {
    selectedId--;
  };

  if (selectedId < 0) {
    LOG("Selected up-a-level");
    return result;
  };

	if (!asyncBlockLoadCache.entryAvailable(selectedId))
	{
		LOG("Selection not in cache");
		return result;
	};
  
	var selection = asyncBlockLoadCache.read(selectedId);
  
	if (selection.isFolder)
	{
		result.selectionIsFolder = true;
		result.selectedFolder = selection;
		return result;
	};

	// Selection must be a track.
	//
	result.selectionIsTrack = true;
	result.selectedTrack = selection;
	return result;
  
};



// `.folders[]` & `.tracks[]` are used to hold the id's etc so that `play.js`
// can do the correct thing when the hard buttons are pressed.
//
uiLibrary.folders = [];
uiLibrary.tracks = [];
uiLibrary.entryCount = 0;
uiLibrary.atRoot = false; 
uiLibrary.title = "";
uiLibrary.enabled = false;

uiLibrary.queueATrack = function(index)
{
  if ((index < 0) || (index >= uiLibrary.tracks.length))
  {
    return;
  };
  engineTracks.insertTrack(uiLibrary.tracks[index]);
};

uiLibrary.goBackCallback = function()
{
	if (!uiLibrary.enabled)
	{
		return;
	};

  // If we aren't at the root already then go up a level.
  //
  if (!uiLibrary.atRoot)
  {
    // Set the initialSelectedEntries for this level back to zero
    // so that if we come back up to this level we will start at the
    // beginning, not half way through the list.
    //
    uiLibrary.initialSelectedEntries[uiLibrary.currentId] = 0;
    engineLibrary.browse(uiLibrary.parentId);
  };
};

// Called from engineLibrary.engineLibrary.callback
//
uiLibrary.updateLibraryNavigation = function(
	entries)
{
	//GUI.alert("Adding ["+entries.length+"] entries");
	asyncBlockLoadCache.insertEntries(entries);
	sWM.redraw();
};

// Called from engineLibrary.engineLibrary.callback
//
uiLibrary.setLibraryNavigation = function (
                parentId,
                currentId,
                title,
		entries,
                entryCount,
                idRequestCallback)
{
  asyncBlockLoadCache.resetCache();

  uiLibrary.atRoot = false;
  uiLibrary.title = title;
  uiLibrary.idRequestCallback = idRequestCallback;
  uiLibrary.currentId = currentId;

  LOG("parentId["+parentId+"] currentId["+currentId+"]");

  if (parentId == "-1")
  {
    uiLibrary.atRoot = true;
  };
  
  if (currentId == "0") {
    uiLibrary.atRoot = true;
  };
  
  if (!uiLibrary.atRoot) {
    uiLibrary.parentId = parentId;
  };
  
  asyncBlockLoadCache.insertEntries(entries);
  /*
  uiLibrary.folders = folders;
  uiLibrary.tracks = tracks;
  */
  
  // If we aren't at the root then add one to the entry count to leave
  // space for the 'back' entry at the top.
  //
  uiLibrary.entryCount = entryCount + (uiLibrary.atRoot ? 0 : 1);

  uiLibrary.displayList();

}
//
////
//
// .updateSelectionCallback is called by sWM to indicated that a different entry has been
// selected (but no necessarily chosen i.e. the line has been greyed out).
//
uiLibrary.updateSelectionCallback = function(index)
{
  var pageSize = pronto.uiLibraryPageSize;
  var entries = uiLibrary.entryCount;
  var entry = index;
  
  uiLibrary.initialSelectedEntries[uiLibrary.currentId] = entry;
	
  // TODO: BUG: This is wrong as index into array, not an id.
  //
  uiLibrary.selectedId = index;
  play.setId(uiLibrary.selectedId);

  uiLibrary.StatusWidget.label = "Library " + utils.pageXofY(entry, entries, pageSize);
};

uiLibrary.entryTextCallback = function(index)
{
  var originalIndex = index;

  if (index < 0)
  {
    return "..";
  };

  if (!uiLibrary.atRoot)
  {
    index--;
    if (index < 0)
    {
      return "< " + utf8.decode(uiLibrary.title);
    };
  };

	var entry = asyncBlockLoadCache.read(index);

	if (entry == "..")
	{
		return "..";
	};

	if (entry.isFolder)
	{
		return "> " + utf8.decode(entry.title);
	}
	return utf8.decode(entry.title);
  
};

uiLibrary.asyncReadCallback = function(index)
{
	uiLibrary.idRequestCallback(index);
};

uiLibrary.displayList = function()
{
	var initialSelectedEntries = 0;

	// Remember where we've been
	//
	if (uiLibrary.initialSelectedEntries[uiLibrary.currentId] != null)
	{
		initialSelectedEntries = uiLibrary.initialSelectedEntries[uiLibrary.currentId];
	};

	sWM.go(
		uiLibrary.entryTextCallback,
		uiLibrary.entryCount,
		"List",
		"Selector",
		pronto.yOffset, // Y offset
		pronto.characterHeight, // character height
		pronto.uiLibraryPageSize, // page size
		uiLibrary.updateSelectionCallback,
		uiLibrary.selectItemCallback,
		uiLibrary.goBackCallback,
		null,  // No highlight selected
		null,  //  - " -
		initialSelectedEntries);    // Select first entry [test: Math.floor(uiLibrary.entryCount/2)]
};

// Enable the display of UI
//
uiLibrary.enable = function()
{
	uiLibrary.enabled = true;

  asyncBlockLoadCache.init(
		engineLibrary_REQUEST_CHUNK_SIZE,
		uiLibrary.asyncReadCallback,
		".."); // unknownResult

  // Set the up, down, left, right & OK button to do something
  //
  uiLibrary.StatusWidget = CF.page("Library").widget("STATUS_LINE");
 
  uiNowPlaying.refreshTransportState();
  handleShuffleRepeat.update();

  uiLibrary.entryCount = 0;
  uiLibrary.displayList();
  engineLibrary.browse();
};

uiLibrary.disable = function()
{
	uiLibrary.enabled = false;
};

elab.add("uiLibrary");
