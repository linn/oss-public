////
//
// ui_paging.js
//
//
// We have the following pages:
//
// DS:
//   'NowPlaying'
//   'Playlist'
//   'Library'
//
// Radio:
//   'Presets' // Cara Radio
//   'Radio'   // Equiv of 'NowPlaying' for Radio == NOW_LISTENING
//
// PreAmp Only:
//   'PreAmp'  // 'NowPlaying' for source only e.g. LP-12
//
// Unused:
//   'Source'  // [No longer used]
//
////

#if (VARIANT_DS)

  // ...

#elif (VARIANT_RADIO)

  // ...

#elif (VARIANT_PREAMP)

  // ...

#endif

var ui_paging = {};

// '.MAX_PAGE' is used as an upper bound on the number of pages that a user
// can page through. This is the actual no of pages of range is [1..MAX_PAGE]
//

#if (VARIANT_DS)

ui_paging.MAXPAGE = 3;
ui_paging.PAGENAME_TYPE = {NOW_PLAYING:0, PLAYLIST:1, LIBRARY:2};
ui_paging.currentPage = ui_paging.PAGENAME_TYPE.NOW_PLAYING;

#elif (VARIANT_RADIO)

ui_paging.MAXPAGE = 2;
ui_paging.PAGENAME_TYPE = {NOW_LISTENING:0, PRESETS:1};
ui_paging.currentPage = ui_paging.PAGENAME_TYPE.NOW_LISTENING;

#elif (VARIANT_PREAMP)

ui_paging.MAXPAGE = 1;
ui_paging.PAGENAME_TYPE = {PREAMP_ONLY:0};
ui_paging.currentPage = ui_paging.PAGENAME_TYPE.PREAMP_ONLY;

#endif

ui_paging.gotoPageExec = function(pageName){

  LOG("gotoPage:"+pageName);
  CF.page("ACTIONS").widget(pageName).executeActions();
};

// TODO: Some modular arithmetic would help here.
//
ui_paging.nextPageNo = function (currentPageNo){

  currentPageNo++;
  if (currentPageNo >= ui_paging.MAXPAGE) {
    return 0;
  };

  return currentPageNo;
};

ui_paging.previousPageNo = function (currentPageNo){

  currentPageNo--;
  if (currentPageNo < 0) {
    return ui_paging.MAXPAGE - 1;
  };

  return currentPageNo;
};

ui_paging.put = function(){
#ifdef LOGGING
  LOG("Current page = " + ui_paging.currentPage);
#endif
};

#if (VARIANT_DS)

ui_paging.gotoPage = function(newPage){

  if (newPage == ui_paging.PAGENAME_TYPE.LIBRARY) {
    ui_paging.gotoPageExec("LIBRARY");
  } else if (newPage == ui_paging.PAGENAME_TYPE.PLAYLIST) {
    ui_paging.gotoPageExec("PLAYLIST");
  } else if (newPage == ui_paging.PAGENAME_TYPE.NOW_PLAYING) {
    ui_paging.gotoPageExec("NOW_PLAYING");
  } else {
    LOG("Next page unknown:"+ newPage);
  };

};

ui_paging.nextPage = function(){

  ui_paging.gotoPage(ui_paging.nextPageNo(ui_paging.currentPage));

};


ui_paging.previousPage = function(){

  ui_paging.gotoPage(ui_paging.previousPageNo(ui_paging.currentPage));

};

#elif (VARIANT_RADIO)

// There are only two pages for radio so simply swap the page.
// Also .nextPage & .previousPage do the same thing.
//
ui_paging.gotoPage = function(newPage){

  if (newPage == ui_paging.PAGENAME_TYPE.NOW_LISTENING) {
    ui_paging.gotoPageExec("NOW_LISTENING");
  } else if (newPage == ui_paging.PAGENAME_TYPE.PRESETS) {
    ui_paging.gotoPageExec("PRESETS");
  } else {
  
    LOG("Next page unknown:"+ newPage);
  };
};

ui_paging.nextPage = function(){

  ui_paging.gotoPage(ui_paging.nextPageNo(ui_paging.currentPage));

};

ui_paging.previousPage = ui_paging.nextPage;

#elif (VARIANT_PREAMP)

// Do little as this is the only page displayed for the preamp personality.
//
ui_paging.nextPage = function(){};
ui_paging.previousPage = function(){};

#endif

doPageUp = function(){
  LOG("doPageUp");
  ui_paging.put();
  ui_paging.previousPage();
};

doPageDown = function(){
  LOG("doPageDown");
  ui_paging.put();
  ui_paging.nextPage();
};


elab.add("ui_paging");
ui_paging.put();
