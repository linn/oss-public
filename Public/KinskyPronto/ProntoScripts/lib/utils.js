////
//
// utils.js
//
////





var utils= {};

/**
 * entry: Entry we are interested in, starts at 0, -1 = no entry
 * totalEntries: Total no of entrys ( == .length)
 * pagseSize: No. of entries per page
 */
utils.pageXofY = function (entry, totalEntries, pageSize) {

  if (entry < 0) {
    return "";
  };

  // return "page 1 of 3"
  //
  var div = parseInt(entry / pageSize);
  var noPages = (parseInt((totalEntries-1)/pageSize)) + 1; // 0 offset & all that.

  return "[ " + (div+1) + " : " + noPages + " ]";

};

utils.mm_ss = function(seconds) {
  var min = Math.floor(seconds/60);
  var sec = seconds % 60
  
  if (sec < 10) {sec = "0" + sec;};
  return min + ":" + sec;
};

#if 0
utils.hh_mm_ss_to_seconds = function (hh_mm_ss) {

  var hh_mm_ss_array = hh_mm_ss.split(':');
    
  // hh_mm_ss_array should have 3 elements
  //
  if (hh_mm_ss_array.length != 3) {
    LOG("Error hh_mm_ss entries = " + hh_mm_ss_array.length);
    return 0;
  };
    
  return parseInt(hh_mm_ss_array[0] * 60 * 60) + // HH
      parseInt(hh_mm_ss_array[1] * 60) + // MM
      parseInt(hh_mm_ss_array[2]) // SS
};
#endif

#if 0
utils.padNum = function(number, numDigits) {
    var str = number.toString();
    while (str.length < numDigits) {str = '0' + str};
    return(str);
};
#endif


#if 0
utils.htmlEncode = function(textToEncode)  {
  var result = textToEncode;

  var amp = /&/gi;
  var gt = />/gi;
  var lt = /</gi;
  var quot = new RegExp ('"', 'gi'); //   /"/gi;
  var apos = new RegExp ("'", 'gi'); //   /'/gi;

  var html_gt = "&gt;";
  var html_lt = "&lt;";
  var html_amp = "&amp;";
  var html_quot = "&quot;";
  var html_apos = "&apos;";

  result = result.replace(amp, html_amp);
  result = result.replace(quot, html_quot);
  result = result.replace(lt, html_lt);
  result = result.replace(gt, html_gt);
  result = result.replace(apos, html_apos);

  return result;
};
#endif

utils.htmlDecode = function(textToDecode) {

  return textToDecode.replace(/&amp;/g, "&").
                      replace(/&quot;/g, '"').
          replace(/&lt;/g, "<").
          replace(/&gt;/g, ">").
          replace(/&apos;/g, "'").
          replace(/&nbsp;/g, " ");

};

#if 0
utils.xmlEncode = function(textToEncode) {
  var result = textToEncode;

  var amp = /&/gi;
  var gt = />/gi;
  var lt = /</gi;
  var quot = new RegExp ('"', 'gi'); //   /"/gi;
  var apos = new RegExp ("'", 'gi'); //   /'/gi;

  var xml_gt = "&#62;";
  var xml_lt = "&#38;#60;";
  var xml_amp = "&#38;#38;";
  var xml_quot = "&#34;";
  var xml_apos = "&#39;";

  result = result.replace(amp, xml_amp);
  result = result.replace(quot, xml_quot);
  result = result.replace(lt, xml_lt);
  result = result.replace(gt, xml_gt);
  result = result.replace(apos, xml_apos);
        
  return result;
};
#endif

#if 0
utils.xmlDecode = function(textToDecode)  {
  var result = textToDecode;

// 'quot' => chr(34),
// 'amp' => chr(38),
// 'apos' => chr(39),
// 'lt' => chr(60),
// 'gt' => chr(62),

  var gt = /&#62;/gi;       // &>
  var lt = /&#38;#60;/gi;  // &<
  var amp = /&#38;#38;/gi;   // &&
  var quot = /&#34;/gi;  // &
  var apos = /&#39;/gi;

  var xml_gt = ">";
  var xml_lt = "<";
  var xml_amp = "&";
  var xml_quot = "\"";
  var xml_apos = "'";

  result = result.replace(amp, xml_amp);
  result = result.replace(quot, xml_quot);
  result = result.replace(lt, xml_lt);
  result = result.replace(gt, xml_gt);
  result = result.replace(apos, xml_apos);
        
  return result;
};
#endif

utils.extractBetweenTagsStringArray = function(str, lhs, rhs){

  var index_left
  var index_right
  var local_str = str
  var result = [];
  
  while (true) {
    index_left = local_str.indexOf(lhs)
    index_right = local_str.indexOf(rhs)

#if 0
      LOG("..Strings substring is " + local_str);
      LOG("  length is " + local_str.length);
      LOG("  lhs " + lhs + " : rhs " + rhs);
      LOG("  left " + index_left + " : right " + index_right);
#endif
    
    if (index_left == -1) {return result;};
    if (index_right == -1) {return result;};
    if (index_right < index_left) {return result;};
  
    result[result.length] = local_str.substring(index_left + lhs.length,index_right)
    
    local_str = local_str.substring(index_right+rhs.length, local_str.length);
  };
};

#if 0
utils.extractBetweenTagsStrings = utils.extractBetweenTagsStringArray;
#endif

/**
 * Used in decode.js
 */
utils.extractBetweenTagsString = function(str, lhs, rhs, default_value){
  
  var result = utils.extractBetweenTagsStringArray(str, lhs, rhs);
  
  if (result.length != 1) {return (default_value == null) ? "!" : default_value};
  
  return result[0];
  
};

/**
 * Used in decode.js
 */
utils.xmlExtractTextArray = function(xml, tag) {
  
    var str_left = "<" + tag
    var str_right = "</"  + tag + ">"
    var index_left
    var index_mid
    var index_right
    var local_xml = xml
    var result = [];
  
    while (true) { // Forever .. and a day

      index_left = local_xml.indexOf(str_left)
      index_mid = local_xml.indexOf('>', index_left)
      index_right = local_xml.indexOf(str_right, index_mid)

      // Loop and 1/2 exit case here
      //
      if (index_left == -1) {return result;};
      if (index_mid == -1) {return result;};
      if (index_right == -1) {return result;};
      if (index_right < index_left)  {return result;};
      if (index_right < index_mid)  {return result;};
      if (index_right < index_left) {return result;};
 
      result[result.length] = local_xml.substring(index_mid + 1, index_right);
      
      local_xml = local_xml.substring(index_right+str_right.length, local_xml.length);
  };  
  
};

#if 0
utils.xmlExtractTextContents = utils.xmlExtractTextArray;
#endif

utils.xmlExtractText = function(xml, tag) {

  var str_left = "<" + tag;
  var str_right = "</"  + tag + ">";
  var index_left;
  var index_mid;
  var index_right;
  

  //alert ("substring is " + str);
  index_left = xml.indexOf(str_left)
  index_mid = xml.indexOf('>', index_left)
  index_right = xml.indexOf(str_right, index_mid)

  //LOG("left " + index_left + " mid = " + index_mid + " right " + index_right);

  if (index_left == -1) {LOG("Error lhs looking for " + str_left + " in " + xml); return "";};
  if (index_mid == -1) {LOG("Error mid looking for >"); return "";};
  if (index_right == -1) {LOG("Error rhs looking for " + str_right); return "";};
  if (index_right < index_left)  {LOG("Error < looking for " + xml); return "";};
  if (index_right < index_mid)  {LOG("Error < looking for " + xml); return "";};
  
  
  //LOG("result = " + xml.substring(index_mid + 1, index_right)); 
  
  return xml.substring(index_mid + 1, index_right); // '+1' == '>'.length/
};

#if 0

utils.extract_xml_result_string = function(xml, tag, default_value){

  var str_left = "<" + tag;
  var str_right = "</"  + tag + ">";
  var index_left;
  var index_mid;
  var index_right;

  if (default_value == null) {default_value = "!"};
  
  //alert ("substring is " + str);
  index_left = xml.indexOf(str_left)
  index_mid = xml.indexOf('>', index_left)
  index_right = xml.indexOf(str_right, index_mid)

  //LOG("left " + index_left + " right " + index_right);
    
  if (index_left == -1) {LOG("Error lhs looking for " + str_left + " in " + xml); return default_value;};
  if (index_mid == -1) {LOG("Error mid looking for >"); return default_value;};
  if (index_right == -1) {LOG("Error rhs looking for " + str_right); return default_value;};
  if (index_right < index_left)  {LOG("Error < looking for " + xml); return default_value;};
  if (index_right < index_mid)  {LOG("Error < looking for " + xml); return default_value;};
  if (index_right < index_left)  {LOG("Error < looking for " + xml); return default_value;};
  //alert("result = " + str.substring(index_left + lhs.length,index_right));  
  return xml.substring(index_mid + 1, index_right); // '+1' == '>'.length
    
};
#endif

#if 0

utils._test = function(){

  LOG(utils.extractBetweenTagsStrings ("<a>next blank</a> bill <a></a>time <a>three</a>joghn <a>four</a>willy<a>end</a>","<a>","</a>"));
  LOG("---------")
  LOG(utils.extractBetweenTagsStrings ('&lt;DIDL-Lite xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/" xmlns:dlna="urn:schemas-dlna-org:metadata-1-0/" xmlns:pv="http://www.pv.com/pvns/" xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/"&gt;&lt;container id="0" parentID="-1" childCount="1" restricted="1" searchable="1"&gt;&lt;upnp:searchClass includeDerived="1"&gt;object&lt;/upnp:searchClass&gt;&lt;dc:title&gt;root&lt;/dc:title&gt;&lt;res protocolInfo="http-get:*:audio/x-mpegurl:*"&gt;http://192.168.1.103:9000/m3u/0.m3u&lt;/res&gt;&lt;upnp:class&gt;object.container&lt;/upnp:class&gt;&lt;/container&gt;&lt;/DIDL-Lite&gt;',"&lt;container","&lt;/container&gt;"));
  LOG("---------")

};

#endif

elab.add("utils");
