////
//
// engineVolumeEtc.js
//
////
var engineVolumeEtc = {};

engineVolumeEtc.status = {volume:0, mute: false};

engineVolumeEtc.callback = null;

engineVolumeEtc.setCallback = function(callBack){

  engineVolumeEtc.callback = callBack;

  callBack(engineVolumeEtc.status);
};


engineVolumeEtc.callbackVolume = function(value){

  engineVolumeEtc.status.volume = parseInt(value);

  if (engineVolumeEtc.callback != null) {
    engineVolumeEtc.callback(engineVolumeEtc.status);
  };

};

engineVolumeEtc.callbackMute = function(value){

  engineVolumeEtc.status.mute = (value == "true");

  if (engineVolumeEtc.callback != null) {
    engineVolumeEtc.callback(engineVolumeEtc.status);
  };

};

engineVolumeEtc.toggleMute = function(){

  if (engineVolumeEtc.status.mute) {
    lpecMessages.unMute();
  } else {
    lpecMessages.mute();
  };
};

engineVolumeEtc._start = function()
{
	if (configuration.preAmpType == 0) // 0 = no preamp
	{
		return;
	};
  
	if (configuration.preAmpType == 2) // 2 = External
	{
#ifdef DAVAAR
		lpec.addEventCallback(engineVolumeEtc.callbackVolume, "Volume", "Preamp", "Volume");
		lpec.addEventCallback(engineVolumeEtc.callbackMute, "Mute", "Preamp", "Volume");
#else
		lpec.addEventCallback(engineVolumeEtc.callbackVolume, "Volume");
		lpec.addEventCallback(engineVolumeEtc.callbackMute, "Mute");
#endif
	}
	else // Must be internal
	{
#ifdef DAVAAR
		lpec.addEventCallback(engineVolumeEtc.callbackVolume, "Volume", "Ds", "Volume");
		lpec.addEventCallback(engineVolumeEtc.callbackMute, "Mute", "Ds", "Volume");
#else
		lpec.addEventCallback(engineVolumeEtc.callbackVolume, "Volume");
		lpec.addEventCallback(engineVolumeEtc.callbackMute, "Mute");
#endif
	};


  

};

elab.add("engineVolumeEtc", null, engineVolumeEtc._start);
