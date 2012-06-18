////
//
// asyncBlockLoadCache
//
// Probably not the best name. What we're implementing is a cache where
// individual entries are read synchronously* with cache misses causing a
// read (by the calling procedure). 
//
// *with a default value if no entry found. 
//
// External Interface
// 
// init(blockSize,
//   asyncRead(fromIndex),
//   unknownResult (eg '..')
//
// resetCache()
//
// insertEntries(entries) // any NOT necessarily zero based array. 
//
// read(entry):result
//  Where result is either the entry we want,
//     null (= waiting asynchronous response)
//     undefined (= not known)
//
////

var asyncBlockLoadCache =
{
	cache: [],
	blockSize: 1,
	asyncReadCallback: function(){},
	unknownResult: null,
};

// init(blockSize,
//   asyncRead(fromIndex),
//   unknownResult (eg '..')
//
asyncBlockLoadCache.init = function(
		blockSize,
		asyncReadCallback,
		unknownResult)
{
	asyncBlockLoadCache.blockSize = blockSize;
	asyncBlockLoadCache.asyncReadCallback = asyncReadCallback;
	asyncBlockLoadCache.unknownResult = unknownResult;
	asyncBlockLoadCache.cache = [];
};

//
// resetCache()
// 
asyncBlockLoadCache.resetCache = function()
{
	asyncBlockLoadCache.cache = [];
};

//
// insertEntries(entries) // any NOT necessarily zero based array. 
//
asyncBlockLoadCache.insertEntries = function(entries)
{
	for (var i = 0; i < entries.length; i++)
	{
		if (entries[i] !== undefined) // Note !==, not !=
	  	{
			asyncBlockLoadCache.cache[i] = entries[i];
		};
	};
};

// read(entry):result
//  Where result is either the entry we want,
//     null (= waiting asynchronous response)
//     undefined (= not knownasyncBlockLoadCache.xxx = function()

asyncBlockLoadCache.entryAvailable = function(index)
{
	if (typeof(asyncBlockLoadCache.cache[index]) == "undefined")
	{
		return false;
	}
	else
	{
		return (asyncBlockLoadCache.cache[index] != null);
	};
};

asyncBlockLoadCache.entryRequested = function(index)
{
	if (typeof(asyncBlockLoadCache.cache[index]) == "undefined")
	{
		return false;
	}
	else
	{
		return (asyncBlockLoadCache.cache[index] == null);
	};
};


asyncBlockLoadCache.read = function(index)
{
	if (typeof(asyncBlockLoadCache.cache[index]) == "undefined")
	{
		LOG("Entry "+index+" not in cache or requested");
		// We neither have the entry in our cache or have requested it.
		//
		for (var i = 0; i < asyncBlockLoadCache.blockSize; i++)
		{
			if (typeof(asyncBlockLoadCache.cache[index+i]) == "undefined")
			{
				asyncBlockLoadCache.cache[index+i] = null;
			};
		};
		asyncBlockLoadCache.asyncReadCallback(index);
		return asyncBlockLoadCache.unknownResult;
	};

	if (asyncBlockLoadCache.cache[index] == null)
	{
		LOG("Entry "+index+" requested, returning unknownResult");
		return asyncBlockLoadCache.unknownResult;
	};

	return asyncBlockLoadCache.cache[index];
};

asyncBlockLoadCache.dump = function()
{
	LOG("Cache dump+");
	for (var i = 0; i < asyncBlockLoadCache.cache.length; i++)
	{
		if (typeof(asyncBlockLoadCache.cache[i]) == "undefined")
		{
			LOG("["+i+"] is empty");
		}
		else if (asyncBlockLoadCache.cache[i] == null)
		{
			LOG("["+i+"] is waiting read callback");
		}
		else
		{
			LOG("["+i+"] = " + asyncBlockLoadCache.cache[i]);
		};
	};
	LOG("Cache dump-");
};

var asyncBlockLoadCache_Test =
{
};

asyncBlockLoadCache_Test.callback = function(index)
{
	LOG("Requested index = " + index);
	
	var entries = [];
	
	for (var i = index; i < index + 20; i++)
	{
		entries[i] = "<" + i + ">";
	};

	asyncBlockLoadCache.dump();
	asyncBlockLoadCache.insertEntries(entries);
	asyncBlockLoadCache.dump();
};

asyncBlockLoadCache_Test.go = function()
{
	var blockSize = 20;
	var asyncReadCallback = asyncBlockLoadCache_Test.callback;
	var unknownResult = '..';

	var entries = [];
	
	asyncBlockLoadCache.init(blockSize,asyncReadCallback,unknownResult);

	asyncBlockLoadCache.dump();
	
	var dummy;

	dummy = asyncBlockLoadCache.read(20);
	dummy = asyncBlockLoadCache.read(21);
	dummy = asyncBlockLoadCache.read(22);
	dummy = asyncBlockLoadCache.read(23);
	
	entries[4] = "four";
	entries[5] = "5";
	entries[6] = "6";
	entries[7] = "7";
	
	asyncBlockLoadCache.insertEntries(entries);
	
	asyncBlockLoadCache.dump();


	entries[7] = "Sse7en";
	entries[8] = "Eight";
	entries[11] = "Apollo"
	
	asyncBlockLoadCache.insertEntries(entries);
	
	asyncBlockLoadCache.dump();
	dummy = asyncBlockLoadCache.read(3);	
	asyncBlockLoadCache.dump();
/*	
	LOG()
	asyncBlockLoadCache.dump();
*/
};


/*

uiPageViewCache.js

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

//
// addItems is used to insert items into the cache. If too many items are added then excess items
// will be set to null. 
//
uiPageViewCache.addItems = function(offset, items)
{
};

//
// Reading of items is implicit
// If .items[x] /= null then the cache is
// valid & you can read the entry. 
//


// The updateCache function
// wanders the cache & deletes any items which are a certain distance from the currently displayed page. 
// 
uiPageViewCache.updateCache = function ()
{
};

*/


