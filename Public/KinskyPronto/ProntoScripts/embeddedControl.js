////
//
// Embedded control - Start of code proper.
//
////

var linnControl = {};

linnControl.PREAMP_TYPE = {NONE:0, INTERNAL:1, EXTERNAL:2};

linnControl.LIPSYNC_TYPE        = {NONE: 0, BASIC: 1, ADVANCED: 2};
linnControl.currentLipsyncState = {type:linnControl.LIPSYNC_TYPE.NONE, index:0, delay:0};
linnControl.lipsyncDelta        = 10;
linnControl.lipsyncDelayMax     = 2000;
linnControl.lipsyncDelayMin     = 50;

linnControl.initialise = function(ipAddress, preAmpType, inputNumber){

  LOG("linnControl.initialise("+ipAddress+","+preAmpType+","+inputNumber+")");
  LOG("1");
  
  try {
  
    lpec.initialiseActionSocket(ipAddress);
    lpec.initialiseEventSocket(ipAddress);
    
  } catch (e) {
  
    LOG("lpec.initialiseSocket error");
    LOG(e);
  };
  LOG("2");
  
  lpecMessages.ds = "Ds/Product";
  
  if (preAmpType == linnControl.PREAMP_TYPE.NONE) {
    return;
  } else if (preAmpType == linnControl.PREAMP_TYPE.INTERNAL) {
    LOG("Internal Preamp");
    linnControl.setInternalSource(inputNumber);
	lpecMessages.volume = 'Ds/Volume';

  } else if (preAmpType == linnControl.PREAMP_TYPE.EXTERNAL) {
    LOG("External PreAmp");
    linnControl.setExternalSource(inputNumber);
	lpecMessages.volume = 'Preamp/Volume';

  } else {
    return;
  };

};

linnControl.mute = function(){

  LOG("mute()");
  lpecMessages.mute();

};

linnControl.unMute = function(){
  LOG("unMute()");
  lpecMessages.unMute();
}

linnControl.volumeInc = function(){
  LOG("volumeInc()");
  lpecMessages.volumeInc();

};

linnControl.volumeDec = function(){

  LOG("volumeDec()");
  lpecMessages.volumeDec();
};

linnControl.powerOn = function(){

  LOG("powerOn()");
  lpecMessages.dsSetStandby('0');

};

linnControl.powerOff = function(){

  LOG("powerOff()");
  lpecMessages.dsSetStandby(1);

};


linnControl.setInternalSource = function(inputNumber){

  LOG("setInternalSource("+inputNumber+")");
  lpecMessages.dsSetSourceIndex(inputNumber);
  
};

linnControl.setExternalSource = function(inputNumber){

  LOG("setExternalSource("+inputNumber+")");
  lpecMessages.preampSetSourceIndex(inputNumber);
  
};


linnControl.enableBasicLipsync = function(index){

  // presets numbered from 1, but indexed from 0	
  index -= 1;	
  linnControl.currentLipsyncState.type = linnControl.LIPSYNC_TYPE.BASIC;
  linnControl.currentLipsyncState.index = index;
  lpecMessages.delaySetPresetIndex(index);
  lpecMessages.delayDelay(linnControl.callbackDelay);  
};

linnControl.callbackDelay = function(info){
 
  linnControl.currentLipsyncState.delay = parseInt(info);
}

linnControl.lipsyncInc = function(){

  // Only basic lip sync is supported at the moment.
  //
  if (linnControl.currentLipsyncState.type != linnControl.LIPSYNC_TYPE.BASIC){
    return;
  };

  linnControl.currentLipsyncState.delay += linnControl.lipsyncDelta;
  if (linnControl.currentLipsyncState.delay > linnControl.lipsyncDelayMax) {
	  linnControl.currentLipsyncState.delay = linnControl.lipsyncDelayMax;  
  }
  lpecMessages.delaySetDelay(linnControl.currentLipsyncState.delay);
  
};

linnControl.lipsyncDec = function(){

  // Only basic lip sync is supported at the moment.
  //
  if (linnControl.currentLipsyncState.type != linnControl.LIPSYNC_TYPE.BASIC){
    return;
  };

  linnControl.currentLipsyncState.delay -= linnControl.lipsyncDelta;
  if (linnControl.currentLipsyncState.delay < linnControl.lipsyncDelayMin) {
	  linnControl.currentLipsyncState.delay = linnControl.lipsyncDelayMin;  
  }
  lpecMessages.delaySetDelay(linnControl.currentLipsyncState.delay);

};

/*

linnControl.callback = function(sourceId, volume, muted, poweredOn){
};

linnControl.status = {sourceId : 0, volume: 0, muted: true, poweredOn: true, connected: true};

//
// parameters
//   ipAddress      : The IP Address of the DS
//   internalPreamp : Is the DS's preamp internal? (Majik DS-I or Sekrit DS-I)
//   source         : Input source number to select
//   callback       : A function to be called when the status of the DS changes
//                  : Will be called with a record containing current source, volume, mute, power and connection status
//                  : See the example below for an sample usage.
//                  : If no callback function is required then simple pass `null`.
//
linnControl.connect = function(ipAddress, preAmpType, source, callback){

};




var linn = {};

linn.ds = {};

linnControl = {};


// Example usage

function myCallback(status){

  if (!status.connected) {
    LOG("The Pronto is not connected to the DS.");
    return;
  };

  if (!status.poweredOn){
    LOG("The DS is powered off");
    return;
  };

  if(status.muted){
    LOG("The volume is muted");
    return;
  };
  
  LOG("Volume [" + status.sourceId + "], source [" + status.volume + "]");
};

linnControl.connect("192.168.1.64", true, 2, myCallback);

*/
