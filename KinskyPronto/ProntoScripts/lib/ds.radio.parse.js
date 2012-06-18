////
//
// dsRadio.js
//
// Keep an eye on 'dsRadio.js' to see how this should be written.
//
////

/*
ACTION Ds/Radio 1 Read "1"
RESPONSE "&lt;DIDL-Lite xmlns:dc=&quot;http://purl.org/dc/elements/1.1/&quot; xm
lns:upnp=&quot;urn:schemas-upnp-org:metadata-1-0/upnp/&quot; xmlns=&quot;urn:sch
emas-upnp-org:metadata-1-0/DIDL-Lite/&quot;&gt;&lt;item&gt;&lt;dc:title&gt;AVRO
Radio Festival Classique&lt;/dc:title&gt;&lt;dc:description&gt;Butien. Gewoon. K
lassiek&lt;/dc:description&gt;&lt;upnp:genre&gt;Classical&lt;/upnp:genre&gt;&lt;
res bitrate=&quot;256000&quot;&gt;http://opml.radiotime.com/Tune.ashx?id=s44185&
amp;amp;formats=mp3,wma&amp;amp;username=linnproducts&amp;amp;partnerId=16&lt;/r
es&gt;&lt;upnp:albumArtURI&gt;http://radiotime-logos.s3.amazonaws.com/s44185q.pn
g&lt;/upnp:albumArtURI&gt;&lt;upnp:class&gt;object.item.audioItem&lt;/upnp:class
&gt;&lt;/item&gt;&lt;/DIDL-Lite&gt;"

http://radiotime-logos.s3.amazonaws.com/s44185q.png
http://opml.radiotime.com/Tune.ashx?id=s44185&formats=mp3,wma&username=linnproducts


<DIDL-Lite
  xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/"
  xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/">
 <item>
  <dc:title>AVRO Radio Festival Classique>/dc:title>
  <dc:description>Butien. Gewoon. Klassiek</dc:description>
  <upnp:genre>Classical</upnp:genre>
  <res bitrate="256000">http://opml.radiotime.com/Tune.ashx?id=s44185&formats=mp3,wma&username=linnproducts&partnerId=16</res>
  <upnp:albumArtURI>http://radiotime-logos.s3.amazonaws.com/s44185q.png</upnp:albumArtURI>
  <upnp:class>object.item.audioItem</upnp:class>
 </item>
</DIDL-Lite>
*/

    
// <Id>11</Id>
// <MetaData>
//  <DIDL-Lite xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/" xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/">
//      <item>
//         <dc:title>CNN Headline News: KLXX</dc:title>
//         <res bitrate="48000">http://opml.radiotime.com/Tune.ashx?id=s33929&amp;amp;sid=p213&amp;amp;formats=mp3,wma&amp;amp;username=linnproducts&amp;amp;partnerId=16</res>
//         <upnp:albumArtURI>http://radiotime-logos.s3.amazonaws.com/p213q.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>
//   </MetaData>

var dsRadio = {};
dsRadio.entryRegExp = new RegExp("<Entry>(.*?)</Entry>","mg");
dsRadio.entry2RegExp = new RegExp("&lt;Entry&gt;(.*?)&lt;/Entry&gt;","mg");
dsRadio.idRegExp = new RegExp("<Id>(.*?)</Id>","mg");
dsRadio.metadataRegExp = new RegExp("<MetaData>(.*?)</MetaData>","mg");
dsRadio.resRegExp = new RegExp("<res(.*?)>(.*?)</res>","mg");
dsRadio.titleRegExp = new RegExp('<dc:title>(.*?)</dc:title>',"m");
dsRadio.albumArtURIRegExp = new RegExp('<upnp:albumArtURI>(.*?)</upnp:albumArtURI>',"m");

dsRadio._parseEntry = function(xml){

  dsRadio.idRegExp.lastIndex = 0;
  dsRadio.metadataRegExp.lastIndex = 0;
  dsRadio.resRegExp.lastIndex = 0;
  dsRadio.titleRegExp.lastIndex = 0;
  dsRadio.albumArtURIRegExp.lastIndex = 0;

  // Looks like we only need .id, .title & .res
  // ToDo: Double check this & optimise away
  // Need AlbumArtURI as well !!!!!
  //
#if 0
  var entry = {id:0, metadata:"", title:"", albumArtURI:"", res:""};

  var id, metadata, title, albumArtURI, res;

  if (!dsRadio.idRegExp.test(xml)) {
    LOG("Couldn't find idRegexp");
  } else {
    entry.id = RegExp.$1;
  };

  if (!dsRadio.metadataRegExp.test(xml)) {
    LOG("Couldn't find metadataRegExp");
  } else {
    entry.metadata = RegExp.$1;
  };

  if (!dsRadio.titleRegExp.test(xml)) {
    LOG("Couldn't find titleRegExp");
  } else {
    entry.title = RegExp.$1;
  };

  if (!dsRadio.albumArtURIRegExp.test(xml)) {
    LOG("Couldn't find albumArtURIRegExp");
  } else {
    entry.albumArtURI = RegExp.$1;
  };

  if (!dsRadio.resRegExp.test(xml)) {
    LOG("Couldn't find resRegExp");
  } else {
//  entry.res = utils.htmlDecode(utils.htmlDecode(RegExp.$2));
//    entry.res = RegExp.$2;
    entry.res = utils.htmlDecode(RegExp.$2);
  };

  LOG("id["+entry.id+"], title["+entry.title+"], albumArtURI["+entry.albumArtURI+"]");
  LOG("  metadata["+entry.metadata+"]");
  LOG("  res["+entry.res+"]");

  return entry;
  
#endif

  var entry = {id:0, title:"", res:"", albumArtURI:""};

  var id, title, res, albumArtURI;

  if (dsRadio.idRegExp.test(xml)) {
    entry.id = RegExp.$1;
  };

  if (dsRadio.titleRegExp.test(xml)) {
    entry.title = RegExp.$1;
  };

  if (dsRadio.resRegExp.test(xml)) {

//  entry.res = utils.htmlDecode(utils.htmlDecode(RegExp.$2));
//    entry.res = RegExp.$2;
    entry.res = utils.htmlDecode(RegExp.$2);
  };
  
  if (dsRadio.albumArtURIRegExp.test(xml)) {
    entry.albumArtURI = RegExp.$1;
  };
  

  LOG("id["+entry.id+"], title["+entry.title+"], albumArtURI["+entry.albumArtURI+"]");
  LOG("  res["+entry.res+"]");

  return entry;
  
  
};

dsRadio.readlistNotDecoded = function(xml){
  
  // Use dsRadio.entry2RegExp not dsRadio.entryRegExp
  // and then decode the (Much smaller XML)
  //
  dsRadio.entry2RegExp.lastIndex = 0;

  var entries = xml.match(dsRadio.entry2RegExp);

  if (entries == null) {
    LOG("No entries");
    return [];
  };

  var result = [];

  LOG("entries.length = " + entries.length);
  for (var i = 0; i < entries.length; i++) {
    LOG("entries["+i+"]" + entries[i]);

    var entry = dsRadio._parseEntry(utils.htmlDecode(entries[i]));

    entry.title = utils.htmlDecode(utf8.decode(entry.title));
    result.push(entry);

  };

  return result;

};

#if 0

dsRadio.readlist = function(xml){

  dsRadio.entryRegExp.lastIndex = 0;

  var entries = xml.match(dsRadio.entryRegExp);

  var result = [];
  
  if (entries == null) {
    LOG("No entries");
  } else {
    LOG("entries.length = " + entries.length);
    for (var i = 0; i < entries.length; i++) {
      LOG("entries["+i+"]" + entries[i]);

      var entry = dsRadio._parseEntry(entries[i]);

      result.push(entry);

    };
  };

  return result;

};

#endif

elab.add("dsRadio");
