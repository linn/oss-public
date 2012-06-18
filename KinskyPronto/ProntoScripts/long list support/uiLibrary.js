////
//
// uiLibrary.js
//
// Support for Media Library UI Page
//
// PROPOSED
//
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
#if 0

a[0] = 0;
a[2] = 2;
a[3] = 3;
a[4] = 4;
a[5] = 5;

var b = [];

b[6] = 6;
b[7] = 7;
b[9] = 9;

a = a.concat(b);

document.writeln(a);

for (var i in a)
{
    document.writeln(a[i]);
};

document.writeln("---");

for (var i = 0; i < a.length; i++)
{
  if (a[i] !== undefined) // Note !==, not !=
  {
    document.writeln(a[i]);
  };
};

#endif


var uiLibrary = {};

uiLibrary.StatusWidget = null;
uiLibrary.parentId = 0;
uiLibrary.idRequestCallback = function(){};
uiLibrary.currentId = 0;
uiLibrary.initialSelectedEntries = [];

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
  
  // Is the selection a folder?
  //
  if (selectedId < uiLibrary.folders.length)
  {
    engineLibrary.browse(uiLibrary.folders[selectedId].id);
    return;
  };
 
  selectedId = selectedId - uiLibrary.folders.length;
  
  // Is the selection a track?
  //
  if (selectedId < uiLibrary.tracks.length) {
    uiLibrary.queueATrack(selectedId);
    return;
  };

  // Shouldn't really get here,
  // but if we do then do nothing as the user may have selected a '..' entry.
  //
};

#define TEST_SELECTION 0

#if TEST_SELECTION

/**
 * '.computeSelection'
 * 'selectedId' is indexed from 0.
 */
uiLibrary.computeSelectionJs = function(selectedId)
{

  // If the first entry isn't 'go up a level' then we will have displayed a line to go
  // 'up' a level, we need to ignore this line.
  //
  if (!uiLibrary.atRoot)
  {
    selectedId--;
  };


  if (selectedId < 0)
  {
    LOG("Selected up-a-level");
    return 'engineLibrary.browse("'+uiLibrary.parentId+'");';
  };
  
  // Is the selection a folder?
  //
  if (selectedId < uiLibrary.folders.length)
  {
    return 'engineLibrary.browse("'+uiLibrary.folders[selectedId].id+'");';
  };
 
  selectedId = selectedId - uiLibrary.folders.length;
  
  // Is the selection a track?
  //
  if (selectedId < uiLibrary.tracks.length)
  {
    return 'uiLibrary.queueATrack("' + selectedId + '");';
  };

  // Shouldn't really get here,
  // but if we do then just return a default of unknown.
  //
  return 'engineLibrary.browse(0);';
};
#endif

// `.folders[]` & `.tracks[]` are used to hold the id's etc so that `play.js`
// can do the correct thing if the hard buttons are pressed.
//
uiLibrary.folders = [];
uiLibrary.tracks = [];

uiLibrary.entries = []; // Sparse array

uiLibrary.entryCount = 0;
uiLibrary.atRoot = false; 
uiLibrary.title = "";

uiLibrary.queueATrack = function(index)
{
  if (uiLibrary.entries[i] == undefined)
  {
    return;
  };
  
  if (!uiLibrary.entries[i].isTrack)
  {
    return;
  };
  
  engineTracks.insertTrack(uiLibrary.entries[i].track);
};

uiLibrary.goBackCallback = function()
{
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
  LOG("Adding ["+entries.length+"] entries");
  uiLibrary.entries = uiLibrary.entries.concat(entries);

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

  uiLibrary.entries = entries;
  
  // If we aren't at the root then add one to the entry count to leave
  // space for the 'back' entry at the top.
  //
  uiLibrary.entryCount = entryCount + (uiLibrary.atRoot ? 0 : 1);

#if 0
    LOG("parentId = " + parentId + " currentId = " + currentId +" title = " + title);
    
    LOG("entries # = " + entries.length);
    for (var i = 0; i < folders.entries; i++)
    {
      if (folder.entries[i] !== undefined)
      {
        var entry = folder.entries[i];
        if (entry.isFolder)
        {
          LOG("["+i+"].id=" + entry.id + ", .title=" + entry.title);
        }
        
        if (entry.isTrack)
        {
          LOG("["+i+"].id=" + entry.id + ", .parentID=" + entry.parentID);
          LOG("["+i+"].title=" + entry.title);
        };
      };
    };

#endif
  
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
  play.setId(index);
  
  uiLibrary.StatusWidget.label = "Library " + utils.pageXofY(entry, entries, pageSize);

  // TODO: Remove this diag from production code.
  //
#if TEST_SELECTION
  uiLibrary.StatusWidget.label = uiLibrary.computeSelectionJs(index);
#endif

};

uiLibrary.entryText = function(index)
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
      return "< " + uiLibrary.title;
    };
  };

  if (uiLibrary.entries[index] == undefined)
  {
    uiLibrary.idRequestCallback(index);
    return "..";
  };

  if (uiLibrary.entries[index].isFolder)
  {
    return "> " + uiLibrary.entries[index].title;
  };

  if (uiLibrary.entries[index].isTrack)
  {
    return uiLibrary.entries[index].title;
  };

  // Should not get here
  //
  return "...";
  
};

uiLibrary.displayList = function()
{
  var initialSelectedEntries = 0;

#if 1
  // Remember where we've been
  //
  if (uiLibrary.initialSelectedEntries[uiLibrary.currentId] != null)
  {
    initialSelectedEntries = uiLibrary.initialSelectedEntries[uiLibrary.currentId];
  }
  else
  {
    initialSelectedEntries = 0;
  };
#endif

  sWM.go(
    uiLibrary.entryText,
    uiLibrary.entryCount, // uiLibrary.currentList.length,
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
  // Set the up, down, left, right & OK button to do something
  //
  uiLibrary.StatusWidget = CF.page("Library").widget("STATUS_LINE");
 
  uiNowPlaying.refreshTransportState();
  handleShuffleRepeat.update();

  uiLibrary.entryCount = 0;
  uiLibrary.displayList();
  engineLibrary.browse();
};

elab.add("uiLibrary");
