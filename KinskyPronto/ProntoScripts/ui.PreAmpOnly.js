////
//
// uiPreAmpOnly.js
//
// Removed calls to engineSources, as it over complicated things dramatically.
//
////

var uiPreAmpOnly = {};

uiPreAmpOnly.enable = function(){

  // parameters are:
  // configuration.preAmpInput = 0;
  // configuration.preAmpInputName = 'LP 12';
  //  configuration.preAmpType = [0|1|2]
  //
  LOG('preamp only ['+configuration.preAmpInputName+','+configuration.preAmpInput+']');

  CF.page("PreAmpOnly").widget("STATUS_LINE").label = configuration.preAmpInputName;

  if (configuration.preAmpType == 0) {
    LOG("No preamp!");
  } else if (configuration.preAmpType == 1) {
    lpecMessages.dsSetSourceIndex(configuration.preAmpInput);
  } else {
    lpecMessages.preampSetSourceIndex(configuration.preAmpInput);
  };
  
  uiVolumeEtc.update();

};

elab.add("uiPreAmpOnly");
