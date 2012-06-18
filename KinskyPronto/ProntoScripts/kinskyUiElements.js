////
//
// kinskyUiElements.js
//
// We are trying to move KinskyPronto towards a touch oriented UI.
// To this goal it will become necessary to create graphics on-the-fly.
// This package represents a first pass at this capability.
//
////

var kinskyUiElements = {};

// getVolumeImage returns a graphics image representing a volume
// between 0 and 100. Any values outside this range will return
// Zero or FSD as appropriate.
//
kinskyUiElements.getVolumeImage = function (volumeLevel) {

  var volStr = "VOL";

  if (volumeLevel == null) {
    volumeLevel = 0;
  };

  if (volumeLevel < 13) {return CF.widget("Vol1", volStr).getImage()};
  if (volumeLevel < 26) {return CF.widget("Vol2", volStr).getImage()};
  if (volumeLevel < 38) {return CF.widget("Vol3", volStr).getImage()};
  if (volumeLevel < 51) {return CF.widget("Vol4", volStr).getImage()};
  if (volumeLevel < 63) {return CF.widget("Vol5", volStr).getImage()};
  if (volumeLevel < 76) {return CF.widget("Vol6", volStr).getImage()};
  if (volumeLevel < 88) {return CF.widget("Vol7", volStr).getImage()};
  
  return CF.widget("Vol8", volStr).getImage();

};

kinskyUiElements.getVolumeImage9400 = function (volumeLevel) {

  var volStr = "VOL9400";

  if (volumeLevel == null) {
    volumeLevel = 0;
  };

  if (volumeLevel < 6) {return CF.widget("Vol0", volStr).getImage()};
  if (volumeLevel < 12) {return CF.widget("Vol1", volStr).getImage()};
  if (volumeLevel < 18) {return CF.widget("Vol2", volStr).getImage()};
  if (volumeLevel < 25) {return CF.widget("Vol3", volStr).getImage()};
  if (volumeLevel < 31) {return CF.widget("Vol4", volStr).getImage()};
  if (volumeLevel < 37) {return CF.widget("Vol5", volStr).getImage()};
  if (volumeLevel < 44) {return CF.widget("Vol6", volStr).getImage()};
  if (volumeLevel < 50) {return CF.widget("Vol7", volStr).getImage()};
  if (volumeLevel < 56) {return CF.widget("Vol8", volStr).getImage()};
  if (volumeLevel < 62) {return CF.widget("Vol9", volStr).getImage()};
  if (volumeLevel < 69) {return CF.widget("Vol10", volStr).getImage()};
  if (volumeLevel < 75) {return CF.widget("Vol11", volStr).getImage()};
  if (volumeLevel < 81) {return CF.widget("Vol12", volStr).getImage()};
  if (volumeLevel < 87) {return CF.widget("Vol13", volStr).getImage()};
  if (volumeLevel < 94) {return CF.widget("Vol14", volStr).getImage()};

  return CF.widget("Vol15", volStr).getImage();

};

kinskyUiElements.getVolumeImageAux9400 = function (volumeLevel) {

  var volStr = "VOLAUX9400";

  if (volumeLevel == null) {
    volumeLevel = 0;
  };

  if (volumeLevel < 6) {return CF.widget("Vol0", volStr).getImage()};
  if (volumeLevel < 12) {return CF.widget("Vol1", volStr).getImage()};
  if (volumeLevel < 18) {return CF.widget("Vol2", volStr).getImage()};
  if (volumeLevel < 25) {return CF.widget("Vol3", volStr).getImage()};
  if (volumeLevel < 31) {return CF.widget("Vol4", volStr).getImage()};
  if (volumeLevel < 37) {return CF.widget("Vol5", volStr).getImage()};
  if (volumeLevel < 44) {return CF.widget("Vol6", volStr).getImage()};
  if (volumeLevel < 50) {return CF.widget("Vol7", volStr).getImage()};
  if (volumeLevel < 56) {return CF.widget("Vol8", volStr).getImage()};
  if (volumeLevel < 62) {return CF.widget("Vol9", volStr).getImage()};
  if (volumeLevel < 69) {return CF.widget("Vol10", volStr).getImage()};
  if (volumeLevel < 75) {return CF.widget("Vol11", volStr).getImage()};
  if (volumeLevel < 81) {return CF.widget("Vol12", volStr).getImage()};
  if (volumeLevel < 87) {return CF.widget("Vol13", volStr).getImage()};
  if (volumeLevel < 94) {return CF.widget("Vol14", volStr).getImage()};

  return CF.widget("Vol15", volStr).getImage();

};

// The progress image is currently 9600 specific, though this will change when
// we add the dynamic header bar to the top of all the pages.
//
// All graphics are available in 9400.
//
// For ease of editing we have split the graphics onto 4 pages [SEQ1..4]
// Each page has 12 elements, except for the first which has an extra blank
// entry ("seq0').
//
kinskyUiElements.getProgressImage = function (percentage) {


  // percentage is currently a %age [0..100] we need to change this to [0..48]
  //
  var per48 = parseInt(percentage*48/100)

  // LOG("%:"+percentage+":%48:"+per48);
  
  if (per48 > 48) {per48 = 48};
  if (per48 < 0) {per48 = 0};
  
  if (per48 == 0) {
    return CF.widget("seq0", "SEQ1").getImage();;
  };

  if (per48 >= 37) { 
    return CF.widget("seq"+per48, "SEQ4").getImage();
  };

  if (per48 >= 25) { 
    return CF.widget("seq"+per48, "SEQ3").getImage();
  };

  if (per48 >= 13) { 
    return CF.widget("seq"+per48, "SEQ2").getImage();
  };

  return CF.widget("seq"+per48, "SEQ1").getImage();

};

elab.add("kinskyUiElements.js");
