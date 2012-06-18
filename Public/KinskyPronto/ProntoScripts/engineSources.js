////
//
// engineSources.js
//
//Preamp service
//Ok, so let's get this straight.
//
//The key to all this is the PRODUCT service.
//
//The PRODUCT service has a NAME, a ROOM, a TYPE, and a collection of SOURCES (and some other stuff).
//
//A SOURCE has a NAME, a SYSTEMNAME, a TYPE, and a VISIBILITY (and some other stuff)
//
//There may be more than one PRODUCT in the same ROOM (and maybe even in the same box)
//
//If a PRODUCT NAME is the same as a SOURCE NAME in the same ROOM, then they are connected.
//
//There are two PRODUCT TYPES:
//
//If the PRODUCT TYPE is "Source", then the UPNP device that bears the PRODUCT service DOES NOT bear the PREAMP service
//If the PRODUCT TYPE is "Preamp", then the UPNP device that bears the PRODUCT service DOES bear the PREAMP service
//
//There are a number of different SOURCE TYPES:
//
//If a SOURCE has type "Playlist", then the UPNP device that bears its PRODUCT service also bears the Ds, Playlist, and Jukebox services
//If a SOURCE has type "Aux", "Spdif", "Toslink", "Analog", then it is an external input of the appropriate type
//If a SOURCE has type "Internal", then it is an internal connection between two PRODUCT services in the same box
//If a SOURCE has type "UpnpAv", then there is a UpnpAv device on the same Ip Address as its PRODUCT service (this is the wierd one)
//
//Clear as mud?
//
//Graham Darnel
//
////


var engineSources = {};

engineSources.dsProductSourceXml = [];
engineSources.preampProductSourceXml = [];

// engineSources.dsProductName is only used in the case of an external preamp
// for matching.
//
engineSources.dsProductName = "";


engineSources.INTERNAL_SOURCES = {PLAYLIST: 0, RADIO:1};

engineSources.selectInternalSourceByName = function(name){

  LOG("Select internal source called [" + name + "]");
  lpecMessages.dsSetSourceIndexByName(name);

};

engineSources.selectInternalSource = function(index){

  LOG("Select internal source [" + index + "]");
  lpecMessages.dsSetSourceIndex(index);

};

engineSources.selectExternalSource = function(index){

  LOG("Select external source [" + index + "]");
  lpecMessages.preampSetSourceIndex(index);

};

//
// decode.ProductSourceXml : Name:Playlist Type:Playlist Visible:1
// decode.ProductSourceXml : Name:Radio Type:Radio Visible:1
// decode.ProductSourceXml : Name:UpnpAv Type:UpnpAv Visible:0
//

engineSources.selectInternalDsSourcePlaylist = function(){

  LOG("In .selectInternalDsSourcePlaylist");
  engineSources.selectInternalSourceByName("Playlist");

};

engineSources.selectInternalDsSourceRadio = function(){

  LOG("In .selectInternalDsSourceRadio");
  engineSources.selectInternalSourceByName("Radio");

};

// Usage:
//
// engineSources.internalDsSourceSelect(engineSources.INTERNAL_SOURCES.PLAYLIST);
// engineSources.internalDsSourceSelect(engineSources.INTERNAL_SOURCES.RADIO);
//
engineSources.internalDsSourceSelect = function(source){

  LOG("engineSources.internalDsSourceSelect("+source+")");
  engineSources.selectDs();

  if (source == engineSources.INTERNAL_SOURCES.PLAYLIST) {
    engineSources.selectInternalDsSourcePlaylist();
  } else if (source == engineSources.INTERNAL_SOURCES.RADIO) {
    engineSources.selectInternalDsSourceRadio();
  } else {
  
    // unknown internal source, fail silently
    //
    ;
  };
};

engineSources.selectSource = function(index){

  LOG("Would like to select source [" + index + "]");
  
  if (configuration.preAmpType == 0) {
    LOG("But there is no preamp");
  } else if (configuration.preAmpType == 1) {
      engineSources.selectInternalSource(index);
  } else {
      engineSources.selectExternalSource(index);
  };

};


// Bug: Oh dear this routine is being called before the .preampProductSourceXml
// has been populated - so we don't know which source to select.
// How do we fix this?
//
engineSources.selectDs = function(){

  // We only have to handle automatic DS source selection if the preamp is external
  // otherwise either the DS will do it for us or there is no preamp.
  //
  if (configuration.preAmpType != 2) {
    LOG("engineSources.selectDs:No external preamp.");
    return;
  };

  TIMESTAMP("engineSources.selectDs:External DS Source.");

  // Have we woken up yet?
  // If not then wait 1/10 second & try again
  //
  if (engineSources.preampProductSourceXml.length == 0) {
    LOG("Waiting: 0 = engineSources.preampProductSourceXml.length");
    Activity.scheduleAfter(100, engineSources.selectDs);
    return;
  };

  if (engineSources.dsProductName.length == 0) {
    LOG("Waiting: 0 = engineSources.dsProductName.length");
    Activity.scheduleAfter(100, engineSources.selectDs);
    return;
  };

  LOG("DS is called ["+engineSources.dsProductName+"]");

  // To quote from Linn (Graham Douglas):
  //
  // In external preamp case, the source to which the DS playlist is connected can be determined
  // by matching the SourceName from the Preamp device (From SourceXml) with the name in the
  // Source device Product service. The defaults for the Name is 'Klimax DS' etc
  //
  //
  // So to do this we need to know what the Preamp thinks the current source & it's name is
  // And what the DS thinks it's Source Product is.
  //
  for(var i = 0; i < engineSources.preampProductSourceXml.length; i++){
    var entry = engineSources.preampProductSourceXml[i];
    LOG("["+i+"] Name:" + entry); //  + " Type:" + entry.type + " Visible:" + entry.visible);

    if (entry == engineSources.dsProductName) {
      LOG("DS is on preamp input ["+i+"]");
      engineSources.selectExternalSource(i);
      return;
    };    
  };

  LOG("No DS input found:Error")
};


engineSources.lpecActionCallback = function(result)
{
#ifdef DAVAAR
  LOG("dsProductName is " + result);
  engineSources.dsProductName = result;
#else
  LOG("dsProductName is " + result[0]);
  engineSources.dsProductName = result[0];
#endif

};


engineSources.lpecCallbackDsProduct = function(result)
{
  LOG("ProductSourceXml(Ds) is " + result);
  engineSources.dsProductSourceXml = decode.ProductSourceXmlLite(result);
};

engineSources.lpecCallbackPreampProduct = function(result)
{
  LOG("ProductSourceXml(Pre) is " + result);
  engineSources.preampProductSourceXml = decode.ProductSourceXmlLite(result);
};


engineSources._start = function()
{
  // TODO: What to do about this.
  // We need to handle a different event depending on if we are a preamp or not.
  //
#ifdef DAVAAR
  lpec.addEventCallback(engineSources.lpecCallbackPreampProduct, "SourceXml", "Preamp", "Product");
  lpec.addEventCallback(engineSources.lpecCallbackDsProduct,     "SourceXml", "Ds",     "Product");
  lpec.addEventCallback(engineSources.lpecActionCallback,        "ProductName", "Ds",     "Product");
#else
  lpec.addEventCallback(engineSources.lpecCallbackPreampProduct, "ProductSourceXml", "Preamp", "Product");
  lpec.addEventCallback(engineSources.lpecCallbackDsProduct,     "ProductSourceXml", "Ds",     "Product");

  lpec.action('Ds/Product 1 Name', engineSources.lpecActionCallback);
#endif

	// If there is no external pre-amp then there is no need to find out the product name,
	// so no need for the callback.
	//
	if (configuration.preAmpType != 2) {
		LOG("engineSources.selectDs:No external preamp.");
		return;
	};

	return; // rikrikrik

#ifdef DAVAAR
	lpec.action('Ds/Product 1 ProductName', engineSources.lpecActionCallback);
#else
	lpec.action('Ds/Product 1 Name', engineSources.lpecActionCallback);
#endif

};

elab.add("engineSources", null, engineSources._start);
