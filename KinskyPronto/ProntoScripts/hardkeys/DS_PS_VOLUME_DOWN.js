doVolDelta(-1);

// Unfortunatly the onHold function must be declared within the button
// scope.
//
onHold = function (){
  doVolDelta(-3);
}

onHoldInterval = 250; // msec