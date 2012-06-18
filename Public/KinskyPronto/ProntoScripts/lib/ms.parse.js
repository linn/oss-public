////
//
// ms.parse.js
//
// Parses the XML returned from calls to the media server (ms).
//
// Platform independent XML (response) parsing for urn:schemas-upnp-org:service:ContentDirectory:1.
//
// The basic idea is that these routine simple extract the pertinent values from the XML / SOAP response.
// They apply no inteligence to the result, that is
// handled elsewhere - still with in engine*.js module(s)
//
////
var msParse = {};

/**
 * For performance reasons pre-compute the HTML encoding.
 */
#if 0
msParse.didlHead = utils.htmlEncode('<DIDL-Lite xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/" xmlns:dlna="urn:schemas-dlna-org:metadata-1-0/" xmlns:pv="http://www.pv.com/pvns/" xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/">');
msParse.didlFoot = utils.htmlEncode('</DIDL-Lite>');
#endif

msParse.didlHead = '&lt;DIDL-Lite xmlns:dc=&quot;http://purl.org/dc/elements/1.1/&quot; xmlns:upnp=&quot;urn:schemas-upnp-org:metadata-1-0/upnp/&quot; xmlns:dlna=&quot;urn:schemas-dlna-org:metadata-1-0/&quot; xmlns:pv=&quot;http://www.pv.com/pvns/&quot; xmlns=&quot;urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/&quot;&gt;';
msParse.didlFoot = '&lt;/DIDL-Lite&gt;';

msParse.idRegexp = new RegExp('id="(.*?)" ',"m");
msParse.parentIDRegexp = new RegExp('parentID="(.*?)" ',"m");
msParse.titleRegexp = new RegExp('&lt;dc:title&gt;(.*?)&lt;/dc:title&gt;',"m");
msParse.albumArtURIRegexp = new RegExp('&lt;upnp:albumArtURI&gt;(.*?)&lt;/upnp:albumArtURI&gt;',"m");
msParse.resultRegexp = new RegExp("<Result>(.*?)</Result>","m");
msParse.numberReturnedRegexp = new RegExp("<NumberReturned>(.*?)</NumberReturned>","m");
msParse.totalMatchesRegexp = new RegExp("<TotalMatches>(.*?)</TotalMatches>","m");
msParse.updateIDRegexp = new RegExp("<UpdateID>(.*?)</UpdateID>","m");
msParse.containerRegExp = new RegExp("&lt;container(.*?)&lt;/container&gt;","mg");
msParse.itemRegExp = new RegExp("&lt;item(.*?)&lt;/item&gt;","mg");


msParse.getIdParentidAndTitle = function (str) {
  
  // We need to reconstruct the XML which we stripped off elsewhere.
  //
  // .self is used by engineTracks.insertTracks
  // We should rationalise this as that routine does a pile of encoding as well.
  //
  var self = msParse.didlHead + str + msParse.didlFoot;

  // var result = {self: self, id: 0, parentID: 0, title: "title", albumArtURI: ""};

  // 
  //
  var id, parentID, title, albumArtURI;

  if (!msParse.idRegexp.test(str)) {
    LOG("Couldn't find idRegexp");
    id = "";
  } else {
    id = RegExp.$1;
  };

  if (!msParse.parentIDRegexp.test(str)) {
    LOG("Couldn't find parentIDRegexp");
    parentID = "";
  } else {
    parentID = RegExp.$1;
  };

  if (!msParse.titleRegexp.test(str)) {
    LOG("Couldn't find title");
    title = 'Title';
  } else {
    title = utils.htmlDecode(utils.htmlDecode(RegExp.$1));
  };


  
  if (!msParse.albumArtURIRegexp.test(str)) {
    // LOG("Couldn't find albumArtURI");
    albumArtURI = "";
  } else {
    albumArtURI = RegExp.$1;
    LOG("AA = ["+albumArtURI+"]");
  };

  return {self: self, id: id, parentID: parentID, title: title, albumArtURI: albumArtURI};
};


msParse._logEntry = function(obj){
  LOG(obj.id + ":" + obj.parentID + ":" + obj.title + ":" + obj.albumArtURI);// + ":" + obj.self);
};

msParse.strEntry = function(obj){
  return (obj.id + ":" + obj.parentID + ":" + obj.title + ":" + obj.albumArtURI);// + ":" + obj.self);
};

msParse.contentDirectory_1_Browse_response = function (xmlResponse) {

  // Returns a record result of the format:
  //
  //  result.numberReturned  = NumberReturned;
  //  result.totalMatches = TotalMatches;
  //  result.updateID = UpdateID;
  //  result.folders = [...];
  //  result.tracks = [...];
  //
  LOG("+msParse.contentDirectory_1_Browse_response" );

  var result = {};

#if 0
    LOG("engine_Parse_XML.browseDirectChildrenResponse:xmlResponse");
    LOG(xmlResponse);
#endif

  var Result, NumberReturned, TotalMatches, UpdateID;

  msParse.resultRegexp.lastIndex = 0;

  if (!msParse.resultRegexp.test(xmlResponse)) {
    LOG("Couldn't find resultRegexp");
    Result = "";
  } else {
    Result = RegExp.$1;
  };
  
  msParse.numberReturnedRegexp.lastIndex = 0;
  
  if (!msParse.numberReturnedRegexp.test(xmlResponse)) {
    LOG("Couldn't find numberReturnedRegexp");
    NumberReturned = 0;
  } else {
    try
    {
      NumberReturned = parseInt(RegExp.$1);
    } catch (e) {
      LOG("Couldn't parseInt numberReturned " + e);
      NumberReturned = 0;
    };
  };

  msParse.totalMatchesRegexp.lastIndex = 0;

  if (!msParse.totalMatchesRegexp.test(xmlResponse))
  {
    LOG("Couldn't find totalMatchesRegexp");
    TotalMatches = 0;
  } else {
    try
    {
      TotalMatches = parseInt(RegExp.$1);
    } catch (e) {
      LOG("Couldn't parseInt TotalMatches " + e);
      TotalMatches = 0;
    };
  };

  msParse.updateIDRegexp.lastIndex = 0;

  if (!msParse.updateIDRegexp.test(xmlResponse)) {
    LOG("Couldn't find updateIDRegexp");
    UpdateID = "";
  } else {
    UpdateID = RegExp.$1;
  };
  
  // var self = Result;

  result.numberReturned  = NumberReturned;
  result.totalMatches = TotalMatches;
  result.updateID = UpdateID;

  var folders = Result.match(msParse.containerRegExp);
  
  if (folders == null) {
    LOG("No folders");
    folders = [];
  } else {
    LOG("folders.length = " + folders.length);
  };

  // If you want to keep track of the original XML string then do it outside of this
  // function as it is a waste of memory.
  //
  // result.self = Result;

#if 0
  LOG('Result ['+Result.split(0,10000)+']');
#endif

  var tracks = Result.match(msParse.itemRegExp);
  if (tracks == null) {
    LOG("No tracks");
    tracks = [];
  } else {
    LOG("tracks.length = " + tracks.length);
  };

  result.folders = [];
  result.tracks = [];
  
  for (var i = 0; i < folders.length; i++){
      var obj = msParse.getIdParentidAndTitle(folders[i]);
      result.folders[i] = obj;
  };

  for (var i = 0; i < tracks.length; i++){
      var obj = msParse.getIdParentidAndTitle(tracks[i]);
      result.tracks[i] = obj;
  };


  /*
   * For browseMetadataResponse the # of children should always be 1.
   *
  // There should only be one element returned. If not then we have come from somewhere very odd.
  // This single element
  //    
  if (contents.length != 1) { // TODO Trace this fault 
    throw "<container> with > 1 entry in engineParseBrowseMetadataResponse";
  };
  */
  LOG("-BrowseDirectChildren");

#if 0
  GUI.alert("result.numberReturned: ["  + result.numberReturned + "]\r\n" +
              "result.totalMatches: [" + result.totalMatches  + "]\r\n" +
              "result.updateID: [" + result.updateID + "]\r\n" +
              "result.folders: [" + result.folders.length + "]\r\n" +
              "result.tracks: [" + result.tracks.length + "]");
#endif

  return result;

};


elab.add("msParse");