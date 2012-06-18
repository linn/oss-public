////
//
// lpecMessages.js
//
////
var lpecMessages = {};

#ifdef DAVAAR
#else
#endif
	

////
//
// Preamp Service
//
// Need to Change to Ds/Volume or Preamp/Volume
//	
////
lpecMessages.mute = function(){
  lpec.action(lpecMessages.volume + ' 1 SetMute "1"');
};

lpecMessages.unMute = function(){
  lpec.action(lpecMessages.volume + ' 1 SetMute "0"');
};

lpecMessages.volumeDec = function(){
  lpec.action(lpecMessages.volume + ' 1 VolumeDec');
};

lpecMessages.volumeInc = function(){
  lpec.action(lpecMessages.volume + ' 1 VolumeInc');
};


lpecMessages.preampSetSourceIndex = function(aSourceIndex){
  lpec.action('Preamp/Product 1 SetSourceIndex "'+ aSourceIndex + '"')
};



lpecMessages.preampSetStandby = function(aStandby) {
  lpec.action('Preamp/Product 1 SetStandby "'+ aStandby + '"')
};
////
//
// Ds/Ds Service
//
////
lpecMessages.play = function(){
  lpec.action(lpecMessages.ds + ' 1 Play');
};

lpecMessages.pause = function(){
  lpec.action(lpecMessages.ds + ' 1 Pause');
};

lpecMessages.next = function()
{
#ifdef DAVAAR
  lpec.action(lpecMessages.ds + ' 1 Next');
#else
  lpec.action(lpecMessages.ds + ' 1 SeekTrackRelative "1"');
#endif
};

lpecMessages.previous = function()
{
#ifdef DAVAAR
  lpec.action(lpecMessages.ds + ' 1 Previous');
#else
  lpec.action(lpecMessages.ds + ' 1 SeekTrackRelative "-1"');
#endif
};

lpecMessages.stop = function(){
  lpec.action(lpecMessages.ds + ' 1 Stop');
};


lpecMessages.playlistRead = function(trackId, callback, errorCallback)
{
	lpec.action('Ds/Playlist 1 Read "'+trackId+'"', callback, errorCallback)
};

lpecMessages.playlistReadList = function(trackIdArray, callback, errorCallback)
{
	lpec.action('Ds/Playlist 1 ReadList "'+trackIdArray+'"', callback, errorCallback)
};

lpecMessages.dsSetSourceIndexByName = function(aSourceName)
{
	lpec.action('Ds/Product 1 SetSourceIndexByName "'+ aSourceName + '"')
};

lpecMessages.dsSetSourceIndex = function(aSourceIndex)
{
	lpec.action('Ds/Product 1 SetSourceIndex "'+ aSourceIndex + '"')
};


lpecMessages.dsSetStandby = function(aStandby)
{
  lpec.action('Ds/Product 1 SetStandby "'+ aStandby + '"')
};

//
lpecMessages.seekTrackId = function(aTrackId)
{
#ifdef DAVAAR
  lpec.action(lpecMessages.ds + ' 1 SeekId "'+ aTrackId + '"')
#else
  lpec.action(lpecMessages.ds + ' 1 SeekTrackId "'+ aTrackId + '"')
#endif
}
//
// TODO: Not implemented
//
lpecMessages.playlistDelete = function(aId) {
  lpec.action('Ds/Playlist 1 DeleteId "'+ aId + '"')
};

lpecMessages.playlistDeleteAll = function() {
  lpec.action('Ds/Playlist 1 DeleteAll')
};

lpecMessages.playlistRepeatOn = function() {
  lpec.action('Ds/Playlist 1 SetRepeat "1"')
};

lpecMessages.playlistRepeatOff = function() {
  lpec.action('Ds/Playlist 1 SetRepeat "0"')
};

lpecMessages.playlistShuffleOn = function() {
  lpec.action('Ds/Playlist 1 SetShuffle "1"')
};

lpecMessages.playlistShuffleOff = function() {
  lpec.action('Ds/Playlist 1 SetShuffle "0"')
};
//
// TODO: Not implemented
//
lpecMessages.playlistInsert = function(callback, errorcallback) {
  lpec.action('Ds/Media 2 PlaylistInsert "'+ track + '" "Absolute"', callback, errorCallback)
};
////
//
// Ds/Radio Service
//
////
lpecMessages.Ds = {};

lpecMessages.Ds.Radio = {};

/*
 * Unused:
 *
lpecMessages.Ds.Radio.Read = function(aId, callback, errorCallback) {
  lpec.action('Ds/Radio 1 Read "'+aId+'"', callback, errorCallback);
};
*/

lpecMessages.Ds.Radio.Stop = function() {
  lpec.action('Ds/Radio 1 Stop');
};

lpecMessages.Ds.Radio.Play = function() {
  lpec.action('Ds/Radio 1 Play');
};


lpecMessages.Ds.Radio.setId = function(aId, aUrl) {
  lpec.action('Ds/Radio 1 SetId "'+aId+'" "'+aUrl+'"');
};

lpecMessages.Ds.Radio.ReadList = function(trackIdArray, callback) {

  var decodeXML = false;
  
  lpec.action('Ds/Radio 1 ReadList "'+trackIdArray+'"', callback, null, decodeXML);
};

////
//
// Ds/Delay Service
//
// Currently incomplete
//
////
lpecMessages.delaySetPresetIndex = function(aIndex) {
  lpec.action('Ds/Delay 1 SetPresetIndex "'+ aIndex + '"')
};

lpecMessages.delaySetDelay = function(aDelay) {
  lpec.action('Ds/Delay 1 SetDelay "' + aDelay + '"')
};

lpecMessages.delayDelay = function(callback) {
  lpec.action('Ds/Delay 1 Delay', callback, null, false);
};

lpecMessages._start = function()
{
#ifdef DAVAAR
	
	lpecMessages.ds = "Ds/Product"; // rikrik
	lpecMessages.ds = "Ds/Playlist";

	if (configuration.preAmpType == 1) // 1 = Internal
	{
		lpecMessages.volume = 'Ds/Volume';
	}
	else
	{
		lpecMessages.volume = 'Preamp/Volume';
	};
	
#else
	lpecMessages.ds = 'Ds/Ds';
	lpecMessages.volume = 'Preamp/Product';
#endif

};

elab.add("lpecMessages", null, lpecMessages._start);
