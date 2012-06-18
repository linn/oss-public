////
//
// decode.js
//
// Uses:
//  utils.htmlDecode
//  utils.extractBetweenTagsStringArray
//  utils.extractBetweenTagsString
//
////

var decode = {};

// utils.htmlDecode = Given "&lthello&gt' returns '<hello>'.
//

// The xml strings passsed to be decode should start '<..', however sometimes we are passed '&lt...'
// Returns true, if we have been passed '&lt...' so should decode further.
//
decode.needsFurtherDecoding = function(xml){
  
  return (xml.slice(0,3) == '&lt');
};

#ifdef konductor

decode.ProductSourceXml = function(xml){
  
  if (decode.needsFurtherDecoding(xml)) {
    xml = utils.htmlDecode(xml);
  };
  
  // alert(xml);
  
  // var sourceList = utils.xmlExtractTextArray(xml, "Source");
  var sourceList = utils.extractBetweenTagsStringArray(xml, "<Source>", "</Source>")
  var result = [];
  
  // Each entry in sourceList will be of the form <name>..</name><type>..</type><visible>..</visible> so extract those.
  for(var i = 0; i < sourceList.length; i++){
    var source = sourceList[i] ;
    var entry = {};
    entry.name = utils.extractBetweenTagsString(source, "<Name>", "</Name>");
    entry.type = utils.extractBetweenTagsString(source, "<Type>", "</Type>");
    entry.visible = utils.extractBetweenTagsString(source, "<Visible>", "</Visible>");
    result[result.length] = entry;
    LOG("decode.ProductSourceXml : Name:" + entry.name + " Type:" + entry.type + " Visible:" + entry.visible);
  };
    
  return result;
    
};

#endif

/**
 * Instead of returing an array of records, .ProductSourceXmlLite returns an array
 * or strings where each string corresponds to the input name.
 */
decode.ProductSourceXmlLite = function(xml){
  
  try {

    if (decode.needsFurtherDecoding(xml)) {
      xml = utils.htmlDecode(xml);
    };
  
  // alert(xml);
  
  // var sourceList = utils.xmlExtractTextArray(xml, "Source");
  var sourceList = utils.extractBetweenTagsStringArray(xml, "<Source>", "</Source>")
  var result = [];

  LOG("Found " + sourceList.length + " source(s)");

  // Each entry in sourceList will be of the form <name>..</name><type>..</type><visible>..</visible> so extract those.
  for(var i = 0; i < sourceList.length; i++){
      var source = sourceList[i];
      result[i] = utils.extractBetweenTagsString(source, "<Name>", "</Name>");
      LOG("decode.ProductSourceXml []" + result[i]);
    };
    
    return result;

  } catch (e) {
        
    TIMESTAMP("Error, "+e+", in decode.ProductSourceXmlLite");
    TIMESTAMP("XML ["+xml+"]");
    return [];
  };
  
};


elab.add("decode");


/*


  var decode_testProductSourceXml1 = "&lt;SourceList&gt;&lt;Source&gt;&lt;Name&gt;DS-12&lt;/Name&gt;&lt;Type&gt;Analog&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;TV&lt;/Name&gt;&lt;Type&gt;Analog&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;Sonos&lt;/Name&gt;&lt;Type&gt;Analog&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;Analog 4&lt;/Name&gt;&lt;Type&gt;Analog&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;Tuner&lt;/Name&gt;&lt;Type&gt;Analog&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;/SourceList&gt;"
  var decode_testProductSourceXml2 = "&lt;SourceList&gt;&lt;Source&gt;&lt;Name&gt;Analog1&lt;/Name&gt;&lt;Type&gt;Analog&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;Digital1&lt;/Name&gt;&lt;Type&gt;Spdif&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;Digital2&lt;/Name&gt;&lt;Type&gt;Toslink&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;Playlist&lt;/Name&gt;&lt;Type&gt;Playlist&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;UpnpAv&lt;/Name&gt;&lt;Type&gt;UpnpAv&lt;/Type&gt;&lt;Visible&gt;0&lt;/Visible&gt;&lt;/Source&gt;&lt;/SourceList&gt;"


SUBSCRIBE Preamp/Product
SUBSCRIBE 30
EVENT 30 0 ProductType "Preamp" ProductModel "Klimax Kontrol" ProductName "Klima
x Kontrol" ProductRoom "Lounge" ProductStandby "true" ProductSourceIndex "1" Pro
ductSourceCount "5" ProductSourceXml "&lt;SourceList&gt;&lt;Source&gt;&lt;Name&g
t;DS-12&lt;/Name&gt;&lt;Type&gt;Analog&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&
gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;TV&lt;/Name&gt;&lt;Type&gt;Analog&lt
;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&
gt;Sonos&lt;/Name&gt;&lt;Type&gt;Analog&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible
&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;Analog 4&lt;/Name&gt;&lt;Type&gt;An
alog&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&l
t;Name&gt;Tuner&lt;/Name&gt;&lt;Type&gt;Analog&lt;/Type&gt;&lt;Visible&gt;1&lt;/
Visible&gt;&lt;/Source&gt;&lt;/SourceList&gt;" StartupSourceIndex "0" StartupSou
rceEnabled "false" ProductAnySourceName "0" ProductAnySourceVisible "0" ProductA
nySourceType "0"
SUBSCRIBE Preamp/Preamp
SUBSCRIBE 31
EVENT 31 0 Volume "41" Mute "false" Balance "0" VolumeLimit "100" StartupVolume
"40" StartupVolumeEnabled "true"


or ...

var ProductSourceXml = "&lt;SourceList&gt;&lt;Source&gt;&lt;Name&gt;Analog1&lt;/Name&gt;&lt;Type&gt;Analog&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;Digital1&lt;/Name&gt;&lt;Type&gt;Spdif&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;Digital2&lt;/Name&gt;&lt;Type&gt;Toslink&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;Playlist&lt;/Name&gt;&lt;Type&gt;Playlist&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;UpnpAv&lt;/Name&gt;&lt;Type&gt;UpnpAv&lt;/Type&gt;&lt;Visible&gt;0&lt;/Visible&gt;&lt;/Source&gt;&lt;/SourceList&gt;"

ACTION Ds/Product 1 SourceXml
RESPONSE "&lt;SourceList&gt;&lt;Source&gt;&lt;Name&gt;Analog1&lt;/Name&gt;&lt;Ty
pe&gt;Analog&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Sour
ce&gt;&lt;Name&gt;Digital1&lt;/Name&gt;&lt;Type&gt;Spdif&lt;/Type&gt;&lt;Visible
&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;Digital2&lt;/Name&
gt;&lt;Type&gt;Toslink&lt;/Type&gt;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&g
t;&lt;Source&gt;&lt;Name&gt;Playlist&lt;/Name&gt;&lt;Type&gt;Playlist&lt;/Type&g
t;&lt;Visible&gt;1&lt;/Visible&gt;&lt;/Source&gt;&lt;Source&gt;&lt;Name&gt;UpnpA
v&lt;/Name&gt;&lt;Type&gt;UpnpAv&lt;/Type&gt;&lt;Visible&gt;0&lt;/Visible&gt;&lt
;/Source&gt;&lt;/SourceList&gt;"
ACTION Ds/Pro
ERROR 103 "Service not found"
ACTION Preamp/Product 1 SourceXml
ERROR 103 "Service not found"

SUBSCRIBE Preamp/Preamp
SUBSCRIBE 7
EVENT 7 0 Volume "70" Mute "false" Balance "0" VolumeLimit "100" StartupVolume "
70" StartupVolumeEnabled "true"

*/

