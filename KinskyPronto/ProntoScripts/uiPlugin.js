////
//
// uiPlugin..js
//
////
var uiPlugin. = {};

/*

uiPlugin.attributes

pageSize
scrollable
graphical 

What else do we need?

maxCachedItems use -1 to imply no limit. Otherwise meander round setting 'entry[x] = null' every so often for items which are a given 'distance' from the current page. 

maxCachedPages is another calculated value (at least = 3) derived from maxCachedItems. 

Should we have a separate page caching package? Would that make sense, I think so. 

Ok let's implement it.

*/