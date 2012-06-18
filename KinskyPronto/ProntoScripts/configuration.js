////
//
// configuration.js (Pronto variant), previously know as DS_Configuration.js
//
// ToDo: Sould really be renamed settings.
//
// Define the IP addresses & ports for the DS & Twonky that we are going to control via the pronto.
//
// uses: diag
//
// settings = {
//     ds:    {IPAddress: "192.168.1.88",
//             dsPort: 55178},
//     ms:    {},
//     debug: {}
// };
//
////


var configuration = {};

configuration.dsIPAddress = "192.168.1.88"; // or .67
configuration.dsPort = 55178; // Fixed for PreAmp & DS class devices

configuration.preAmp = true;
configuration.preAmpType = 0; // 0 = none, 1 = internal, 2 = external

configuration.preAmpInput = 0; // Used for non DS sources (pronto.variant == pronto.VARIANT_TYPE.PREAMP)
configuration.preAmpInputName = 'LP 12';

configuration.msIPAddress = "192.168.1.106";

// For Asset ... 53877  /ContentDirectory/172824d8-83c1-5e7f-a16b-b3520877e6cb/control.xml
// For Twonky 9000 /ContentDirectory/Control
configuration.msPort = 9000;
configuration.msUrl = "/ContentDirectory/Control";

configuration.get = function(name){
  
  var global = System.getGlobal("LINN_" + name);

  if (global) {
    return global;
  };
  
  return CF.widget(name, "PARAMETERS").label;
};


configuration.commonParameters = function(){

  // Read the common parameters.
  //
  // These are the DS IP Address, PreAmp type & debugging
  //
  configuration.dsIPAddress = configuration.get("DS_IP_ADDRESS");

  var preAmpType = configuration.get("PREAMP_TYPE");
  
  try {
    configuration.preAmpType = parseInt(preAmpType);
  } catch (e) {
      configuration.preAmpType = 0;
  } finally {

        ; // null;
  };
  
  configuration.preAmp = (configuration.preAmpType != 0);
  
};


configuration.dsParameters = function(){

  configuration.msIPAddress = configuration.get("MS_IP_ADDRESS");
  configuration.msPort = parseInt(configuration.get("MS_IP_PORT")); 
  configuration.msUrl = configuration.get("MS_URL");

};

configuration.preampParameters = function(){
    configuration.preAmpInput = parseInt(configuration.get("PREAMP_INPUT"));
    configuration.preAmpInputName = configuration.get("PREAMP_INPUT_NAME");
};

configuration._init = function(){

  configuration.commonParameters();

  if (pronto.variant == pronto.VARIANT_TYPE.PREAMP) {
    configuration.preampParameters();
  } else if (pronto.variant == pronto.VARIANT_TYPE.DS) {
    configuration.dsParameters();
  } else if (pronto.variant == pronto.VARIANT_TYPE.RADIO) {
    // Do nothing, no extra parameters
  };

};

configuration._start = function(){

  LOG("Pronto Configuration is")
  LOG(" Build: "+buildVersion + "[" + buildRevision+ "] " + buildTimestamp);
  LOG(" Media Server: "+configuration.msIPAddress+':'+configuration.msPort + configuration.msUrl);
  LOG(" DS: "+configuration.dsIPAddress+':'+configuration.dsPort);
  LOG(" PreAmp: "+configuration.preAmpType);
  LOG(configuration.debugEnabled ? (" DebugServer: "+configuration.debugServer) : " No Debug");

  validation.validIP(configuration.dsIPAddress, diag.fatal);
  validation.validIP(configuration.msIPAddress, diag.fatal);

};

elab.add("configuration", configuration._init, configuration._start);
