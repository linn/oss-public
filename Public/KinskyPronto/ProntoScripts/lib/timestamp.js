#ifdef LOGGING

////
//
//timestamp.js
//
//uses: log();
//
////

var timestamp_helper = {startTime: new Date().getTime(),
                      wallClock: 0};

timestamp_helper.wallClock = timestamp_helper.startTime;


timestamp_tick = function(){
var newTime = new Date();
var result = "@"+(newTime-elab.timestamp);
timestamp_starttime = newTime;
return result;
};


function timestamp(str){

var wallClock = new Date().getTime();

var diff1 = wallClock - timestamp_helper.wallClock;
var diff2 = wallClock - timestamp_helper.startTime;

LOG(diff1 + ':' + diff2 + '@' + str);

timestamp_helper.wallClock = wallClock;

};

timestamp('Timestamp startup.');

/*

var watchdog_startTimestamp = new Date().getTime();
var watchdog_timestamp = watchdog_startTimestamp;
LOG(watchdog_timestamp + "ms since 1970/01/01");

*/

#endif
