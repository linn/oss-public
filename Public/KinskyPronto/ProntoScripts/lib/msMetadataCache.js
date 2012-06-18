////
//
// msMetadataCache.js
//
////
var msMetadataCache = {};

msMetadataCache._cache = [];

msMetadataCache.add = function(objectId, parentId, title, upnpClass, albumArtURI){
  
  if (upnpClass == null){
    upnpClass = "";
  };

  if (albumArtURI == null){
    albumArtURI = "";
  };

  msMetadataCache._cache[objectId] = {parentId: parentId, title: title, upnpClass: upnpClass, albumArtURI: albumArtURI};

  LOG("msMetadataCache.add(" + objectId + ", " + parentId + ", '" + title + "', " + upnpClass + ", " + albumArtURI+")");

};

msMetadataCache.get = function(objectId){

  LOG("msMetadataCache.get("+objectId+")");

  var result;
  
  if (objectId in msMetadataCache._cache) {
    LOG("In cache get [" + objectId + "]");
    
    result = msMetadataCache._cache[objectId];
  } else {
  
    result = {parentId: 0, title: 'Unknown', upnpClass: "", albumArtURI: ""};
  };
  
  LOG("msMetadataCache.get("+objectId+"):" + result.parentId + ", '" + result.title + "', " + result.upnpClass + ", " + result.albumArtURI);
		       
  return result;
  
};

msMetadataCache._init = function(){

  msMetadataCache.add("-1", "-1", "Root's Parent", "", "");
  msMetadataCache.add("0", "-1", "Root", "", "");

};

elab.add("msMetadataCache", msMetadataCache._init);
