////
//
// uiPageView.js
//
// Note that some of these functions are virtual and will need
// to be replaces by appropriate GUI components.
//
////

/**
 * uiPageView Philosophy
 *
 * The aim of uiPageView is to provide an efective & efficient
 * abstraction to the UI idiom of a paged display of
 * a (potentially large) set of items.
 *
 * The user of this package should have no (or at worse
 * very little) concept of the actual UI being used.
 * In particular withing the context of the Pronto
 * the user should not know (or care) if they are using
 * a 9600, 
 */
var uiPageView = {};

uiPageView.xxx = function(){};

uiPageView.defaultWidgetObject = null;

uiPageView.state = {
 itemCount = 0,
 currentPage = 0,
 noOfPages = 0};

uiPageView.setDefaultImage = function(widgetObject){

  uiPageView.defaultWidgetObject = widgetObject;

};

uiPageView.requestItemsCallback = function(){};

uiPageView.setRequestItemsCallback = function(callback){

 uiPageView.requestItemsCallback = callback;
};

uiPageView.selectedItemCallback = function(){};

uiPageView.setSelectedItemCallback = function(callback){

 uiPageView.selectedItemCallback = callback;
};


uiPageView.displayItems = function(itemCount){

  // Initialise the status data structure, clear the UI &
  // set off first callback to get the first page of items.
  //
  // If itemCount <= 0 then we have nothing to display so display
  // 'empty' text.
  //
};

elab.add("uiPageView");

/*
Usage:

uiPageView is VERY callback intensive. It does some caching
of information but it is probably best to do caching yourself
as well.

To start the display of a new list of data the following call flow will be
executed.

1. Call uiPageView.displayItems(itemCount) where itemCount is the total number
of items to be displayed (potentially 1,000's).

2. Callback defined in setRequestItemsCallback will be called with parameters
 as follows:
 
   yourCallback(offset, count, onwardCallback)
 
 This your callback routing should the ASYNCRONOUSLY obtain 'count' items from 'offset'
 [zero based] and call 'onwardCallback' passing these items as the single array parameter.
 
 Each item should be defined as a JSON object as follows:
 
 item = {description:"I'm item 27", imageUrl:"http://16.12.34.56/image.jpg", isCurrent: boolean};
 
 If there is no imageUrl then null can be passed & the image from the widget define by
 setDefaultImage(widgetObject) will be used in it's place.
 
3. At various times times 'yourCallback(offset, count, onwardCallback)' will be called
   as the end user pages up & down the list.

4. When the use selects an item then the callback defined in setSelectedItemCallback will be called
  with a single integer parameter which will be the zero based index into the vitual items which
  the user has selected.
*/

uiPageView.displayItems(itemCount);

///////////////////////////////////////////////////////////////////////////////////////////////
//

/**
 * Misc design notes:
 
 Book / Chapter / Page / Paragraph / Line Analogy

Callback interface / Design Pattern

Should we start with the first page or the page which has the selected item.

Each item has an index (0..*), title, graphic & detailedText(+).

Should we divorce the paging / cache from the (currently scrolling) UI, this would certainly make sense in the long term, not sure about the short  term.

To take the analogy further.

Book = Everything
Chapter = held in cache
Page = ? Read from source
Paragraph = Displayed by UI
Line = Single item

(+) detailedText is CR/LF delimited and would be extra information displayed in the UI when the item is selected - we could possible do this via a callback (getDetailedInformation(index).

So we need something like this:

uiBookChapterPage.js - good a name as any

.loadBook(....)
.clearBook(...) - close the UI down, we're moving pages (Or should we just 'loadBook(null) ?)

need to pass parameters to loadBook

  getLines(from:to):[pages]
  displayPage(someParams)
  setSelectedLine(index)
  
Also need to consider a page display object & associated UI. I guess the right metaphor would be the Java 'implements'
Are the display & UI separate (My guess is no - especially as (in the future) we may be creating GUI object on the fly).

We need to be able to capture both a scrollWheel API & a touch API. Both these will have methods like this:

selected, nextPage, previousPage (cf isFirstPage, isLastPage,)
a method of expressing %age completion (Page x or y)

*/

