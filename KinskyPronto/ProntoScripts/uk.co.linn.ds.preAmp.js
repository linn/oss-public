////
//
// uk.co.linn.ds.preAmp.js
//
//
// This module assumes that the DS being controlled either has an integrated
// preamp (currently Majik DS-I, Sekrit DS-I & Sneaky DS-I) or is attached
// to a supported Linn preamp via the proxy service. There is no difference
// in functionality between these two operating modes as the difference are
// handled internaly.
//
// If this module is used to try and control a DS without preamp capability
// (For example a standalone Klimax DS) then it will fail silently. That is
// all functions and callbacks will contine to operate but will respond as
// though they were controlling an unmuted DS with the volume at 0 playing
// source 0 which is called "DS".
//
// Usage:
//
// For a more complete example of using this module please see
// the Linn OSS site at http://oss.linn.uk.co/todo
//
/*


The interface is 

  
  uk.co.linn.ds.preAmp.initialise(ipAddress, preampType, inputNumber);
  
*/
////

// First define the uk.co.uk.co.linn.ds.preAmp object & ensure that its
// correctly formed.
//
// Raise execption and fail if this isn't the case.
//
var uk;

if (!uk) {
  uk = {};
} else if (typeof uk != "object") {
  throw new Error("uk already exists and is not an object");
};

if (!uk.co) {
  uk.co = {};
} else if (typeof uk.co != "object") {
  throw new Error("uk.co already exists and is not an object");
};

if (!uk.co.linn) {
  uk.co.linn = {};
} else if (typeof uk.co.linn != "object") {
  throw new Error("uk.co.linn already exists and is not an object");
};

if (!uk.co.linn.ds) {
  uk.co.linn.ds = {};
} else if (typeof uk.co.linn.ds != "object") {
  throw new Error("uk.co.linn.ds already exists and is not an object");
};

if (!lpec) {
  throw new Error("Required module lpec does not exist");
};

if (typeof lpec != "object") {
  throw new Error("Required module lpec is not an object");
};


if (uk.co.linn.ds.preAmp) {
    throw new Error("uk.co.linn.ds.preAmp module already exists");
};
//
////



var elab = {add: function(str){System.print("Elab("+str+")");}}; // Very bad indeed.

function log(str){

  Diagnostics.log(str);

};

uk.co.linn.ds.preAmp = {};

uk.co.linn.ds.preAmp.PREAMP_TYPE = {NONE:0, INTERNAL:1, EXTERNAL:2};

uk.co.linn.ds.preAmp.preAmpType = uk.co.linn.ds.preAmp.PREAMP_TYPE.NONE;


uk.co.linn.ds.preAmp.initialise = function (ipAddress, preAmpType, inputNumber){

};


uk.co.linn.ds.preAmp.setInternalSource = function(inputNumber){

};

uk.co.linn.ds.preAmp.setExternalSource = function(inputNumber){


    
};

uk.co.linn.ds.preAmp.volumeInc = function(){

};

uk.co.linn.ds.preAmp.volumeDec = function(){

};

uk.co.linn.ds.preAmp.volumeMute = function(){

};

uk.co.linn.ds.preAmp.volumeUnMute = function(){

};

uk.co.linn.ds.preAmp.powerOn = function(){

};

uk.co.linn.ds.preAmp.powerOff = function(){

};
