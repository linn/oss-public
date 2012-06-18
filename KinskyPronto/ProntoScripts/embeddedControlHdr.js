////
//
// embeddedControlHdr.js
//
// Which is used to provide a minumal (But necessary) interface to a Linn DS
// via LPEC.
//
//
////

// Temporarily change scope to allow us to use lpecmessages.*.js
//
var lpec = lpec;

var utils = {};

utils.htmlDecode = function(textToDecode) {

  return textToDecode.replace(/&amp;/g, "&").
                      replace(/&quot;/g, '"').
		      replace(/&lt;/g, "<").
		      replace(/&gt;/g, ">").
		      replace(/&apos;/g, "'").
		      replace(/&nbsp;/g, " ");

};

function timestamp(str){
  LOG(str);
};

// Overide elab.add to keep lpecMessage.js happy.
// NOTE that (as of Davaar) lpecMessages needs initialised (done in embeddedControl.js)
//
var elab = {add: function(str){LOG("Elab("+str+")");}};
