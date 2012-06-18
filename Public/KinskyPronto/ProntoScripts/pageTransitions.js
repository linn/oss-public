////
//
// pageTransitions.js
//
// This module contains the initialisation / transition code for each of the Pronto Kinsky pages.
// Each of these three module will be called immediatly prior to their associated page being displayed.
//
// These have been called transition, not initialisation, as each of these functions will be called every time the
// page is loaded not just on the first call.
//
// NOTE: These routines are executed in page scope, NOT activity scope.
//
///
//
// These three 'transition' procedures are called when each of the four main display pages are
// displayed either for the first time or when the page up / down button is pushed.
//
// Until one of these procedure are calls to write to graphics widgets will fail. This is a ProntoScript
// limitation, not KinskyPronto.
//
//
////

#if (VARIANT_DS)

  // ...

#elif (VARIANT_RADIO)

  // ...

#elif (VARIANT_PREAMP)

  // ...

#endif


var pageTransitions = {};

function doDisableAll(){

  log("doDisableAll+");

  try {

#if (VARIANT_DS)

	uiPlaylist.disable();
	uiLibrary.disable();

#elif (VARIANT_RADIO)

      ui_NowListening.disable();
      ui_Presets.disable();

#elif (VARIANT_PREAMP)

    ; // Nothing to disable - as we never change pages

#endif

  } catch (e) {
    log("doDisableAll:error");
    log(e);
  };

  log("doDisableAll-");

};

#if (VARIANT_DS)

function doTransitionNowPlaying(){

  try {

    TIMESTAMP("doTransitionNowPlaying+");
    //
    ui_paging.currentPage = ui_paging.PAGENAME_TYPE.NOW_PLAYING;
    uiHeader.disable();
    doDisableAll();

    if (diag.checkForFailure()) {return};

    uiNowPlaying.enable();
    handleFooter.updateAll();
    TIMESTAMP("doTransitionNowPlaying-");

  } catch (e) {
    TIMESTAMP("Error "+e+":doTransitionNowPlaying");
    diag.fatal(e+"\n\rin doTransitionNowPlaying");
  };



};

function doTransitionPlaylist(){

  try {

    TIMESTAMP("doTransitionPlaylist+");
    //
    ui_paging.currentPage = ui_paging.PAGENAME_TYPE.PLAYLIST;
    uiHeader.enable();
    doDisableAll();

    if (diag.checkForFailure()) {return};

    uiPlaylist.enable();
    handleFooter.updateAll();

    TIMESTAMP("doTransitionPlaylist-");

  } catch (e) {
    TIMESTAMP("Error "+e+":doTransitionPlaylist");
    diag.fatal(e+"\n\rin doTransitionPlaylist");
  };

  
};

function doTransitionLibrary(){

  try {

    TIMESTAMP("doTransitionLibrary+");
    //
    ui_paging.currentPage = ui_paging.PAGENAME_TYPE.LIBRARY;
    uiHeader.enable();
    doDisableAll();

    if (diag.checkForFailure()) {return};

    uiLibrary.enable();
    handleFooter.updateAll();
    TIMESTAMP("doTransitionLibrary-");

  } catch (e) {
    TIMESTAMP("Error "+e+":doTransitionLibrary");
    diag.fatal(e+"\n\rin doTransitionLibrary");
  };


}


#elif (VARIANT_RADIO)

function doTransitionNowListening(){

  try {

    TIMESTAMP("doTransitionNowListening+");
    //
    ui_paging.currentPage = ui_paging.PAGENAME_TYPE.NOW_LISTENING;
    uiHeader.disable();
    doDisableAll();

    if (diag.checkForFailure()) {return};

    ui_NowListening.enable();
    // ToDo: Why don't we call handleFooter.updateAll(); ??
  
    TIMESTAMP("doTransitionNowListening-");

  } catch (e) {
    TIMESTAMP("Error "+e+":doTransitionNowListening");
    diag.fatal(e+"\n\rin doTransitionNowListening");
  };


};

function doTransitionPresets(){

  try {

    TIMESTAMP("doTransitionPresets+");
    ui_paging.currentPage = ui_paging.PAGENAME_TYPE.PRESETS;
    uiHeader.enable();
    doDisableAll();

    if (diag.checkForFailure()) {return};

    ui_Presets.enable();
    TIMESTAMP("doTransitionPresets-");

  } catch (e) {
    TIMESTAMP("Error "+e+":doTransitionPresets");
    diag.fatal(e+"\n\rin doTransitionPresets");
  };

  
};

#elif (VARIANT_PREAMP)

// Note that we don't have a header (=uiHeader.redraw) for the preamp.
// This is because we already have all the information displayed on the (only)
// home page.
//
function doTransitionPreAmpOnly(){

  try {

    TIMESTAMP("doTransitionPreAmpOnly+");
    ui_paging.currentPage = ui_paging.PAGENAME_TYPE.PREAMP_ONLY;
    ui_paging.put();
    doDisableAll();

    if (diag.checkForFailure()) {return};

    uiPreAmpOnly.enable();
    TIMESTAMP("doTransitionPreAmpOnly-");

  } catch (e) {
    TIMESTAMP("Error "+e+":doTransitionPreAmpOnly");
    diag.fatal(e+"\n\rin doTransitionPreAmpOnly");
  };


};

#endif

elab.add("pageTransitions");
