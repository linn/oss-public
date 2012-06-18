////
//
// coverArtCache.js
//
// NOTE: Now unsed
//
////
#if 0
var coverArtCache = {};

coverArtCache.cache = [];
coverArtCache.maxSize = 5;

coverArtCache.read = function(key){ // key is albumName

  // Ignore empty or null keys.
  //
  if (key == null){return null};
  if (key == ""){return null};
  
  if (key in coverArtCache.cache) {
    LOG("Album ["+key+"] is in cache");
    return coverArtCache.cache[key];
  };
  
  return null;
  
};

coverArtCache.write = function(key, value){

  // Ignore empty or null keys.
  //
  if (key == null) {return;};
  if (key == ""){return;};
  
  // If the album is already in the cache then bin it.
  //
  if (coverArtCache.read(key) != null) {
    LOG("Album already in  ["+key+"] cache");
    
  };
  if (coverArtCache.cache.length > coverArtCache.maxSize) {
    var dummy = coverArtCache.cache.shift();
  };
  
  LOG("Adding album ["+key+"] to cache");
  coverArtCache.cache[key] = value;
  
}

elab.add("coverArtCache");
#endif