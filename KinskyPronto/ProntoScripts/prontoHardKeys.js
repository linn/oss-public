////
//
// prontoHardKeys.js
//
//
// Also look at prontoCallbacks.js, which should probably be removed.
//
////
var prontoHardKeys = {};

// Scroll wheel inner navigation keys.
//
prontoHardKeys.Ok = doNothing;
prontoHardKeys.N = doNothing;
prontoHardKeys.S = doNothing;
prontoHardKeys.E = doNothing;
prontoHardKeys.W = doNothing;

prontoHardKeys.mute =  doNothing;
prontoHardKeys.volPlus =  doNothing; // Special case for repeat
prontoHardKeys.volMinus =  doNothing; // Special case for repeat
prontoHardKeys.menu =  doNothing;

prontoHardKeys.setCompassKeys = function (ok,n,s,e,w){

  if (ok == null) {ok = doNothing};
  if (n == null) {n = doNothing};
  if (s == null) {s = doNothing};
  if (e == null) {e = doNothing};
  if (w == null) {w = doNothing};
  
  doFirmOk = ok;
  doFirmUp = n;
  doFirmDown = s;
  doFirmLeft = w;
  doFirmRight = e;
  
};

prontoHardKeys.disableCompassKeys = function(){

  prontoHardKeys.setCompassKeys();
}

// The doFirm*() functions are defined in ProntoEdit, so we have to map these to work as appropriate.
//
// Firm keys along the bottom edge of the screen.
//
// lpecMessages is forward referenced so has to be a function, not assignment.
//

doFirm1 = doNothing;
doFirm2 = doNothing;
doFirm3 = doNothing;
doFirm4 = doNothing;
doFirm5 = doNothing;

function setFunctionKeys(f1, f2, f3, f4, f5){

  doFirm1 = f1;
  doFirm2 = f2;
  doFirm3 = f3;
  doFirm4 = f4;
  doFirm5 = f5;
  
};

function doVolMute(){engineVolumeEtc.toggleMute();}

function doFirmPower(){powerManagement.switchOff();}

function doVolDelta(){};  // Never called, all done in ProntoScript because of repeating scope.

// 9400 Specific key
//
function doBack()
{
#if (VARIANT_DS)
	uiLibrary.goBackCallback();
#endif
};

// 9400 Specific key
//
function doInfo()
{
};

var doFirmMenu = doNothing;

// Define compass keys.
//
var doFirmOk = doNothing;
var doFirmUp = doNothing;
var doFirmDown = doNothing;
var doFirmLeft = doNothing;
var doFirmRight = doNothing;

function doChannelUp(){onRotary(-1);};
function doChannelDown(){onRotary(1);};

elab.add('prontoHardKeys');
