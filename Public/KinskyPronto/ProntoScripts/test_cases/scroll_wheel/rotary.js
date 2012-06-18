

var rotary = {};
	
rotary.calcAcceleration = function(clicks){

  var accClicks,
  absClicks,
  temp;

  absClicks = Math.abs(clicks);

  if (absClicks > 2) {
    temp = absClicks - 1;
    accClicks = parseInt((Math.exp(temp)) / temp);

    if (clicks < 0) {
      return -accClicks;
    };
   
   return accClicks;
  };

  if (clicks > 0) {
      return 1;
    } else if (clicks < 0 ) {
      return -1;
    } else {
      return 0;
  };

};

rotary.calcAcceleration2 = function(clicks){

  if (clicks > 0) {
      return 1;
    } else if (clicks < 0 ) {
      return -1;
    } else {
      return 0;
  };

}

rotary.calcAcceleration3 = function(clicks){

  return clicks;
}

// .. & ..

var displaySlider = {
};

displaySlider.widget = null;

/**
 * Pre-set the page size & number of entries as these are invariant across
 * each individual page displayed,
 */
displaySlider.initialise = function(pageSize, entries){
};

displaySlider.doDisplay = function(entry){
  
};

displaySlider.remove = function(){
};

//
// Start of code proper
//

var value = 0;
var mode = 1;

onRotary = function(clicks){

  if (mode == 1) {
    clicks = rotary.calcAcceleration(clicks);
  } else if (mode == 2){
    clicks = rotary.calcAcceleration2(clicks);
  } else if (mode == 3){
    clicks = rotary.calcAcceleration3(clicks);
  };
  
  value = doScaling(value + clicks);
  
  GUI.alert(value);

}

doFirm1 = function(){

  GUI.alert("Mode is now 1 [Acceleration]");
  mode = 1;

};

doFirm2 = function(){

  GUI.alert("Mode is now 2 [Linear]");
  mode = 2;

};

doFirm3 = function(){

  GUI.alert("Mode is now 3 [+/- 1]");
  mode = 3;

};

doScaling = function(value){

  if (value > 1000) {
    return 1000;
  };
  
  if (value < 0) {
    return 0;
  };
  
  return value;
  
}