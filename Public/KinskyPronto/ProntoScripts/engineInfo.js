////
//
// engineInfo.js
//
// Manages the Ds/Info subscription service.
//
// BUG: Currently only on callback can be registered to receive
// Metadata events.
//
////
var engineInfo = {};

engineInfo.status = {tbd: {}};

engineInfo.callback = null;

engineInfo.setCallback = function(callBack){

  engineInfo.callback = callBack;

  callBack(engineInfo.status);
};


engineInfo.callbackMetadata = function(value)
{
  // GUI.alert(value);
  return;
  engineInfo.status.volume = parseInt(value);

  if (engineInfo.callback != null) {
    engineInfo.callback(engineInfo.status);
  };

};


engineInfo._start = function()
{
  lpec.addEventCallback(engineInfo.callbackMetadata, "Metadata", "Ds", "Info");
};

elab.add("engineInfo", null, engineInfo._start);
