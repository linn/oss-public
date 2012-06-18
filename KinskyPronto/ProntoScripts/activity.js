////
//
// activity.js
//
// There are a few places in the Pronto where we display
// 'Loading..' or 'Queuing..' text to indicate a background
// activity.
//
// This is usually a bit of a mess as we have UI within
// engine - NOT good.
//
// If we uses this then at least we've hidden it!
//
// Grab some nice graphics from
// http://www.ajaxload.info/
//
// Should we have a simple activity.start & handle everything else from there
// on in ourselves. So we always show {tbd} seconds of activity?
//
// Need some `throbber` graphics or could use the progress indicator circle
// suitably scaled.
//
////

var activity = {};

activity.queuingIndex = 0;
activity.queuingWording = ["       ", ".      ", "..     ", "...    ", "....   ", ".....  ", "...... ", "......."];
//activity.queuingWording = ["|", "/", "-", "\\"];

activity.widget = CF.page("Library").widget("LOADING");

activity.start = function(){
  activity.tick();
};

activity.tick = function(){
  activity.queuingIndex = 0;
  activity.ticking = true;
  activity.update();
};
		
activity.update = function(){	
  if (activity.ticking == true) {	
    activity.widget.visible = true;
    activity.widget.label = "Loading"+activity.queuingWording[activity.queuingIndex];
    GUI.updateScreen();
    activity.queuingIndex++;
    if (activity.queuingIndex == activity.queuingWording.length) {
      activity.queuingIndex = 0;
    };
    Activity.scheduleAfter(200, activity.update);
  };
};

activity.stop = function(){
  activity.ticking = false;
  activity.queuingIndex = 0;
  activity.widget.visible = false;
  GUI.updateScreen();
};

activity._init = function(){
  activity.widget.visible = false;
  activity.queuingIndex = 0;
};

elab.add("activity", activity._init, null);
