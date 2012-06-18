////
//
// uiPageViewCache
//
////
var uiPageViewCache = {};

uiPageViewCache.items = [];

uiPageViewCache.clearCache = function(){
  self.items = [];
};

/**
 * .addItems is used to insert items into the cache. If too many items are added then excess items will be set to null. 
 */
uiPageViewCache.addItems = function(offset, items){};

/**
 * Reading of items is implicit
 * If .items[x] /= null then the cache is
 * valid & you can read the entry. 

*/

/**
 * The updateCache function
 * wanders the cache & deletes any items which are a certain distance from the currently displayed page. 
 * 
 */
uiPageViewCache.updateCache = function (){

};

uiPageViewCache.xx = function(){};
uiPageViewCache.xx = function(){};
uiPageViewCache.xx = function(){};
uiPageViewCache.xx = function(){};
