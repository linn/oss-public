////
//
// splash.js
//
// ToDo: Suboptimal. At the moment the splash screen is removed after
// the Pronto has elaborated - but before it has reached a steady state.
// i.e. it can still be dealing with subscription events from the DS.
//
////

#if 0

var splash = {};

splash.remove = function(){

  CF.page("NowPlaying").widget("SPLASH").visible = false;
  CF.page("NowListening").widget("SPLASH").visible = false;
  CF.page("PreAmpOnly").widget("SPLASH").visible = false;

};

elab.add("splash");

#endif
