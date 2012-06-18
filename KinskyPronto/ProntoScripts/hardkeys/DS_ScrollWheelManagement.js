////
//
// ScrollWheelManagement.js
//
//
// We take a very simplistic, but effecient philosophy to manage the scroll wheel and it's onRotary function
//
// Each page can register an array of objects which have a text element & a javascript string to be executed if the
// OK button is pressed.
//
////

// TODO: we ALWAYS redraw the widgetList text - not exactly effcient.
//

var Kinsky;
if (!Kinsky) Kinsky = {};

// Despite our best efforts at OO programming you looks all context in the onRotary call back function.
// so we keep all the state external. Not really a problem (but a bit ugly) as there can be only one ring
// \\\ scroll wheel.
//
//

Kinsky.ScrollWheelManagement = {

  pageSize: 13, // How many lines of text do we get per page. (17 = 18px, 13 = 24px)
  charHeight: 30, // High of widgetList text in px. (24 = 18px, 32 = 24px)
  off: true,
  from:0,
  to:0,
  currentSelection: 0,
  widgetList: "",
  widgetSelector: "",
  list: new Array(),
  
  redraw: function(){

    //Sys.print(".redraw");
    
    // Now we have a valid currentSelection [0 <= currentSelection < completeStringArray.length]
    // let's work out what page we are on and how far down it.
    //
    var rem = this.currentSelection % this.pageSize;
    var mod = parseInt(this.currentSelection / this.pageSize);
  
    //Sys.print("mod: " + mod + " rem: " + rem)
    
    // CF.widget(this.widgetSelector).label = this.currentSelection + ":" + mod + ":" + rem;
    CF.widget(this.widgetSelector).top = this.y + (rem * this.charHeight); //getCharacterHeight(18))
  
    this.from = this.pageSize * mod
    this.to = this.pageSize * (mod+1) -1
    //System.print("From: " + from + " to:" + to)
    if (this.to >= this.list.length ) {this.to = this.list.length-1}
    //System.print("From: " + from + " to:" + to)
  
    var text = ""
    for (var i = this.from; i <= this.to; i++) {
      text += this.list[i].text + "\n";
    
    };

    CF.widget(this.widgetList).label = text;
    
    this.selectionUpdateCallback(this.currentSelection);
  
  },
  
  pageUp: function(){
    _onRotary(-this.pageSize);
  },
  
  pageDown: function(){
    _onRotary(this.pageSize);
  },

  // Depending on the current highlit / selected line invoke the Javascript.
  //
  select: function () {
    if (this.off) {return;}
    
    // System.print(".select");
    
    eval(this.list[this.currentSelection].select);
  },

  // Depending on the current highlit / selected line invoke the Javascript.
  //
  next: function () {
    if (this.off) {return;}
    
    // System.print(".next");
    
    eval(this.list[this.currentSelection].next);
  },

  previous: function () {
    if (this.off) {return;}
    
    // System.print(".previous");
    
    eval(this.list[this.currentSelection].previous);
  },

  onRotary: function (clicks) {

  },

  update: function (list, resetScroll) {

    if (resetScroll) {this.currentSelection = 0;};
    
    this.list = new Array();
    
    for (var i=0; i < list.length; i++) {
      this.list[i] = {text: list[i].text, select: list[i].select};
      
      this.list[i].next = ((list[i].next) ? list[i].next: list[i].select)
      this.list[i].previous = ((list[i].previous) ? list[i].previous : list[i].select)
    };
    
    if (this.currentSelection >= this.list.length) {
      this.currentSelection = 0;
    };
    
    this.redraw();

  },

  initialise: function (list, widgetList, widgetSelector, y, charHeight, pageSize, selectionUpdateCallback) {

    if (y == null) {y = 60};
    if (charHeight == null) {charHeight = 30};
    if (pageSize == null) {pageSize = 13};
    if (selectionUpdateCallback == null) {selectionUpdateCallback = function(){}};

    this.pageSize = pageSize;
    this.charHeight = charHeight;
    this.y = y;
    
    this.off = false;
    this.currentSelection = 0;
    this.widgetList = widgetList;
    this.widgetSelector = widgetSelector;
    this.selectionUpdateCallback = selectionUpdateCallback;
    onRotary = _onRotary;
    
    this.update(list, true);
    
  },
  
  clear : function () {
    // Disable everything
    //
    // Sys.print(".clear");

    onRotary = function(clicks){};
    this.list = [];
    this.off = true;
    this.label = "";
    
  }
}

function _onRotary(clicks){
    if (Kinsky.ScrollWheelManagement.off) {return;}

    //System.print(".onRotary")
    //System.print(Kinsky.ScrollWheelManagement.list);
    
    // Have tried to reduce the scroll wheet sensitivity but the Pronto still 'clicks' for each rotation which
    // makes the interface non-intuative as you hear two clicks but the list only moves by one.
    //
    Kinsky.ScrollWheelManagement.currentSelection += clicks // parseInt(clicks/2); // Way too sensitive ...
  
    // Make sure we haven't jumped off either end of our list.
    //
    if (Kinsky.ScrollWheelManagement.currentSelection < 0) {
      Kinsky.ScrollWheelManagement.currentSelection = 0;
    };
  
    if (Kinsky.ScrollWheelManagement.currentSelection >= Kinsky.ScrollWheelManagement.list.length ) {
      Kinsky.ScrollWheelManagement.currentSelection = Kinsky.ScrollWheelManagement.list.length - 1;
    };
  
    Kinsky.ScrollWheelManagement.redraw();
};

//
////