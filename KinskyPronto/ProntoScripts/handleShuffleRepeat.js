////
//
// handleShuffleRepeat.js
//
////
var handleShuffleRepeat = {};

handleShuffleRepeat.repeat = false;
handleShuffleRepeat.shuffle = false;

handleShuffleRepeat.lpecCallbackRepeat = function(value){

  // If we aren't controlling a DS then we can ignore these events.
  //
  handleShuffleRepeat.repeat = (value == "true"); 

  handleShuffleRepeat.update();
  
};

handleShuffleRepeat.lpecCallbackShuffle = function(value){

  handleShuffleRepeat.shuffle = (value == "true"); 

  handleShuffleRepeat.update();

};

handleShuffleRepeat.update = function (){

  handleFooter.updateAll();

};


handleShuffleRepeat._start = function(){

  // If we aren't controlling a DS then we can ignore these events.
  //
  if (pronto.variant != pronto.VARIANT_TYPE.DS) {return;};

  // Note don't ignore setting up callback in startup as we don't know if we are
  // to handle a ds, opreamp or radio by then we only get that information
  // during initial page startup which happens after elaboration.
  //
  lpec.addEventCallback(handleShuffleRepeat.lpecCallbackRepeat, "Repeat", "Ds", "Playlist");
  lpec.addEventCallback(handleShuffleRepeat.lpecCallbackShuffle, "Shuffle", "Ds", "Playlist");
  
};

elab.add("handleShuffleRepeat", null, handleShuffleRepeat._start);
