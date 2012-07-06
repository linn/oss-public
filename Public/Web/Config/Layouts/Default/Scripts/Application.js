(function($) {

Array.prototype.contains = function(aValue){
    return $.inArray(aValue, this) != -1;
}

var containers = {};
var accordionOptions =  {
    autoHeight:false, 
    collapsible:true,
    active:false,
    change: function(){
        $(".Invalid").each(function(aIdx, aElem){
            if ($(aElem).attr('tagName').toUpperCase() == "INPUT"){
                $(aElem).attr('value', $(aElem).attr('initialValue'));
            }
            Validate($(aElem));
        });
    }
};
var isReleaseBuild = (kBuildVariant == "release");
var isSerivceSpecified = false;
var showTopLevelTabs = true;
                        
$(document).ready(function () {
    if (!isReleaseBuild){
        var options = { 
            beforeSubmit: function(){
            	var fileName = "" + $("#inputUpload")[0].value;
            	if (fileName == ""){
                    ShowUploadMessage("No web application file specified.");
                    return false;
                }
                else if (!fileName.endsWith(".tar"))
                {
                    ShowUploadMessage("Web application file must be in TAR format.");
                    return false;
                }
                return true;
            },  
            success: function() { 
                ShowUploadMessage("Application Submitted"); 
            },
            clearform: true        // clear all form fields after successful submit 
        }; 
        $("#frmUpload").ajaxForm(options);
    }else{
        $("#lnkTabUpload").remove();
        $("#tabUpload").remove();
    }
    $("#tabHeaders").tabs("#tabs > div");
    
    var width = (isIPad ? '1000px' : '800px')
    $("#container").css('max-width', width);
    
    var qs = GetQueryString();
    var service = "Ds";
    if (qs.service){
        service = qs.service;
        isSerivceSpecified = true;
    }
    if (qs.device == "Preamp") {
    	service = "Preamp"; // allow Preamp devices without service having to be explicitly defined
    }
    
    if (isIPad && isSerivceSpecified) {
    	showTopLevelTabs = false;
    	$("#header").remove();
    	$("#tabUpload").remove();
    }

    containers[service] = AppendContainer(service, true);
});

function ShowUploadMessage(aMessage) {
	var uploadDialog = $('<div></div>')
        .html(aMessage)
        .dialog({
        	buttons: { "OK": function() { $(this).dialog('close'); } },
            width: 400,
            resizable: false,
            modal: true,
            position: 'center',
            autoOpen: false,
            title: "Upload Application"
        });
    uploadDialog.dialog('open');
}

function RemoveContainer(aService){
    containers[aService].Stop();
    delete containers[aService];
    if (showTopLevelTabs) {
        $("#lnkTab" + aService).remove();
    }
    $("#tab" + aService).remove();
}

function RemoveEmptyContainers(){
	$.each(containers, function(name, value) { 
		var headerElems = $("#header" + name + " > li");
		var bodyElems = $("#body" + name + " > div > div");
		if (headerElems.length == 0 && bodyElems.length == 0) {
		    RemoveContainer(name);
		}
	});
}

function AppendContainer(aService, aIsMainTab){    
    var services = [
        new ServiceProduct(aService),
        new ServiceConfiguration(aService),
        new ServiceVolkano(aService),
        new ServiceProxy(aService)
    ];
    
    var container = new ServiceCollection();
    
    container.ServiceName = aService;
    
    for (var i=0;i<services.length;i++){
        container.AddService(services[i]);
    }
    
    container.Services().Configuration.Variables().ParameterXml.AddListener(function() {Reload(container);});
    container.Services().Product.Variables().SourceXml.AddListener(function() {Reload(container);});

    container.Services().Product.Variables().ProductUrl.AddListener(function() {RefreshDeviceInfo(container, aService);}); 
    container.Services().Product.Variables().ManufacturerInfo.AddListener(function() {RefreshDeviceInfo(container, aService);}); 
    container.Services().Product.Variables().ManufacturerName.AddListener(function() {RefreshDeviceInfo(container, aService);});      
    container.Services().Product.Variables().ManufacturerUrl.AddListener(function() {RefreshDeviceInfo(container, aService);});
    container.Services().Product.Variables().ManufacturerImageUri.AddListener(function() {RefreshDeviceInfo(container, aService);});
    container.Services().Product.Variables().ModelName.AddListener(function() {RefreshDeviceInfo(container, aService);});   
    container.Services().Product.Variables().ModelImageUri.AddListener(function() {RefreshDeviceInfo(container, aService);});
    container.Services().Product.Variables().ModelUrl.AddListener(function() {RefreshDeviceInfo(container, aService);});
    
    container.Start();   
    
    if (aIsMainTab) {
        CreateContainerHtml(aService);
    }
    
    return container;
}

function CreateContainerHtml(aService) {
	var lnkTabServiceLength = 0;
	if (showTopLevelTabs) {
		lnkTabServiceLength = $("lnkTab" + aService).length;
	}
	
	if (lnkTabServiceLength == 0 && $("#tab" + aService).length == 0) {
		var tabName = aService;
	    if (aService == "Cd") {
	        tabName = "Disc Player";
	    }
	    var link = $("<li id='lnkTab" + aService + "'><a href='#tab" + aService + "'><span id='lnkTabSpan" + aService + "'>" + tabName + "</span></a></li>");
	    var tab = $("<div id='tab" + aService + "'><ul id='header" + aService + "' class='header'/><div id='body" + aService + "' class='body'/></div>");        
	
	    if (isReleaseBuild){
	    	if (showTopLevelTabs) {
	           $("#tabHeaders").append(link);
	    	}
	        $("#tabs").append(tab);
	    }else{
	    	if (showTopLevelTabs) {
	           $("#lnkTabUpload").before(link);
	           $("#tabUpload").before(tab);
	    	}
	    	else {
	    	   $("#tabs").append(tab);
	    	}
	    }
	    if (showTopLevelTabs) {
		    $("#tabHeaders").data("tabs").destroy();
		    $("#tabHeaders").tabs("#tabs > div");
	    }
	}
}

function RefreshDeviceInfo(aContainer, aService){
	// load logo image from device (MaufacturerImageUri), create image link to ManufacturerUrl, tooltip set to ManufacturerInfo
    if (showTopLevelTabs) {
    	// show logo top right - clickable link
    	$("<a><img src='" + aContainer.Services().Product.Variables().ManufacturerImageUri.Value() + "'/></a>")
	        .attr({href: aContainer.Services().Product.Variables().ManufacturerUrl.Value(),
	               title: aContainer.Services().Product.Variables().ManufacturerInfo.Value(),
	               target: '_blank'})
	        .appendTo($("#logo").empty());
	    
	    // top level tabs labeled as ModelName
        ($("#lnkTabSpan" + aService).empty()).append(aContainer.Services().Product.Variables().ModelName.Value());
    }
    
    // create About Tab
    Refresh(aContainer, true);
}

function Reload(aContainer){
    aContainer.ParameterJson = $.xml2json(aContainer.Services().Configuration.Variables().ParameterXml.Value());
    Refresh(aContainer, false);
}

function Refresh(aContainer, aRefreshAbout)
{
	// clear reboot dialog if there
	if ($("#dialogReboot").dialog('isOpen')) {
        $("#dialogReboot").dialog('close');
    }
	
    var createdNew = false;
    var validHeaderIds = [];
    var validParameterIds = [];
    var validCollectionIds = [];
    
    // create optional Proxy device tabs
    if (!isSerivceSpecified) { // no tabs for proxy devices if a particular service is requested
        CreateContainerHtml(aContainer.ServiceName);
    }
    
    // create About Tab
    var kAboutTabName = "About";
    var headerIdAbout = kHeaderPrefix + StripIllegalCharacters(aContainer.ServiceName) + kAboutTabName;
    var bodyIdAbout = kBodyPrefix + StripIllegalCharacters(aContainer.ServiceName) + kAboutTabName;
    validHeaderIds[validHeaderIds.length] = headerIdAbout;
    
    if ($("#" + headerIdAbout).length == 0){
    	var newHeaderAbout = $("<li id='" + headerIdAbout + "'><a href='#'>" + kAboutTabName + "</a></li>");
    	var newBodyAbout = $("<div id='" + bodyIdAbout + "'/>");
        $("#header" + aContainer.ServiceName).append(newHeaderAbout);
        $("#body" + aContainer.ServiceName).append(newBodyAbout);
    }
    
    var idModelIcon = headerIdAbout + "_" + "ModelIcon";
    var idModelName = headerIdAbout + "_" + "ModelName";
    var idManufacturerName = headerIdAbout + "_" + "ManufacturerName";
    var idSwVersion = headerIdAbout + "_" + "SwVersion";
    var idSwUpdate = headerIdAbout + "_" + "SwUpdate";
    var idIpAddress = headerIdAbout + "_" + "IpAddress";
    var idMacAddress = headerIdAbout + "_" + "MacAddress";
    var idProductId = headerIdAbout + "_" + "ProductId";
    var idBoardInfoTitle = headerIdAbout + "_" + "BoardInfoTitle";
    var idBoardInfo = headerIdAbout + "_" + "BoardInfo";
    var idModelInfo = headerIdAbout + "_" + "ModelInfo";
    
    validParameterIds[validParameterIds.length] = idModelName;
    validParameterIds[validParameterIds.length] = idManufacturerName;
    validParameterIds[validParameterIds.length] = idSwVersion;
    validParameterIds[validParameterIds.length] = idModelInfo;
	validParameterIds[validParameterIds.length] = idModelIcon;
    validParameterIds[validParameterIds.length] = idSwUpdate;
    validParameterIds[validParameterIds.length] = idIpAddress;
    validParameterIds[validParameterIds.length] = idMacAddress;
    validParameterIds[validParameterIds.length] = idProductId;
    validParameterIds[validParameterIds.length] = idBoardInfoTitle;
    validParameterIds[validParameterIds.length] = idBoardInfo;

    if (aRefreshAbout) {
    	var kModelNameDisplay = "Model Name";
    	var kManufacturerName = "Manufacturer";
    	var kSwVersionDisplay = "Software Version";
    	var kSwUpdateDisplay = "Software Update Available";
    	var kIpAddressDisplay = "IP Address";
    	var kMacAddressDisplay = "MAC Address";
    	var kProductIdDisplay = "Product ID";
    	var kBoardTypeDisplay = "Board Type";
    	var kBoardIdDisplay = "Board ID";
    	var kModelInfoDisplay = "Model Information";
    	
        $("#" + bodyIdAbout).empty();
        var title = "";
        var value = "";
        // ModelIcon
        if (aContainer.Services().Product.Variables().ModelImageUri.Value()) {
        	title = $("<div class='ModelIcon' id='" + idModelIcon +"'></div>");
        	if (aContainer.Services().Product.Variables().ProductUrl.Value()) { // only rebootable (volkano) devices have product urls
        		$("<a href='#' title='Reboot'><span></span><img src='" + aContainer.Services().Product.Variables().ModelImageUri.Value() + "'/></a>")
	               .click(function(){
	                    Reboot(aContainer.ServiceName);
	                    return false; // prevent the default action, e.g., following a link
	                }).appendTo(title); 
        	}   
        	else {
        		$("<img src='" + aContainer.Services().Product.Variables().ModelImageUri.Value() + "'/>").appendTo(title);
        	}
	        $("#" + bodyIdAbout).append(title);
        }
        else {
        	$("#" + bodyIdAbout).append("<br/>");
        }
        // ModelName
        title = $("<div id='" + idModelName +"'><span class='AboutDescription'>" + kModelNameDisplay + "</span></div>");
        if (aContainer.Services().Product.Variables().ModelUrl.Value()) {
            value = $("<a class='ElementLink'>" + aContainer.Services().Product.Variables().ModelName.Value() + "</a>").attr({
        	   href: aContainer.Services().Product.Variables().ModelUrl.Value(),
        	   target: "_blank",
               title: "Product Information"});
        }
        else {
            value = $("<span class='ElementInfo'>" + aContainer.Services().Product.Variables().ModelName.Value() + "</span>");
        }
        title.append(value);
        $("#" + bodyIdAbout).append(title);
        // ManufacturerName
        title = $("<div id='" + idManufacturerName +"'><span class='AboutDescription'>" + kManufacturerName + "</span></div>");
        value = $("<a class='ElementLink'>" + aContainer.Services().Product.Variables().ManufacturerInfo.Value() + "</a>").attr({
           href: aContainer.Services().Product.Variables().ManufacturerUrl.Value(),
           target: "_blank",
           title: aContainer.Services().Product.Variables().ManufacturerName.Value() + " Website"}).appendTo(value);
        title.append(value);
        $("#" + bodyIdAbout).append(title);
        // SwVersion
        title = $("<div id='" + idSwVersion +"'></div>");
        aContainer.Services().Volkano.SoftwareVersion(function(result){
            $("#" + idSwVersion).empty();
            $("#" + idSwVersion).append("<span class='AboutDescription'>" + kSwVersionDisplay + "</span>");
            $("#" + idSwVersion).append("<span class='ElementInfo'>" + SoftwareVersionPretty(result.aSoftwareVersion) + "</span>");
        }, function(message, transport){
            aContainer.Services().Proxy.SoftwareVersion(function(result){
                var rs232Info = "RS232 connection not active";
                if (result.aSoftwareVersion) {
                    rs232Info = "RS232 Controlled";
                    $("#" + idSwVersion).empty();
                    $("#" + idSwVersion).append("<span class='AboutDescription'>" + kSwVersionDisplay + "</span>");
                    $("#" + idSwVersion).append("<span class='ElementInfo'>" + result.aSoftwareVersion + "</span>");
                }
                $("#" + idModelInfo).empty();
                $("#" + idModelInfo).append("<span class='AboutDescription'>" + kModelInfoDisplay + "</span>");
                $("#" + idModelInfo).append("<span class='ElementInfo'>" + rs232Info + "</span>");
            });
        }); 
        $("#" + bodyIdAbout).append(title);
        $("#" + bodyIdAbout).append(title);
        // SwUpdate
        title = $("<div id='" + idSwUpdate +"'></div>");
        aContainer.Services().Volkano.SoftwareUpdate(function(result){
            $("#" + idSwUpdate).empty();
            $("#" + idSwUpdate).append("<span class='AboutDescription'>" + kSwUpdateDisplay + "</span>");
            if (result.aAvailable == true) {
	            $("<a class='ElementLink'>" + SoftwareVersionPretty(result.aSoftwareVersion) + "</a>").attr({
	               href: "http://products.linn.co.uk/VersionInfo/ReleaseVersionInfo.xml",
	               target: "_blank",
	               title: "Release Notes"}).appendTo($("#" + idSwUpdate));
	        }
	        else {
	            $("#" + idSwUpdate).append("<span class='ElementInfo'>" + SoftwareVersionPretty(result.aSoftwareVersion) + "</span>");
	        }
        }); 
        $("#" + bodyIdAbout).append(title);
        // IpAddress
        if (aContainer.Services().Product.Variables().ProductUrl.Value()) {
            title = $("<div id='" + idIpAddress +"'><span class='AboutDescription'>" + kIpAddressDisplay + "</span></div>");
            var deviceUrl = document.createElement("a");
            deviceUrl.href = aContainer.Services().Product.Variables().ProductUrl.Value();
            value = $("<span class='ElementInfo'>" + deviceUrl.hostname + "</span>");
            title.append(value);
            $("#" + bodyIdAbout).append(title);
        }
        // MacAddress
        title = $("<div id='" + idMacAddress +"'></div>");
        aContainer.Services().Volkano.MacAddress(function(result){
            $("#" + idMacAddress).empty();
            $("#" + idMacAddress).append("<span class='AboutDescription'>" + kMacAddressDisplay + "</span>");
            $("#" + idMacAddress).append("<span class='ElementInfo'>" + result.aMacAddress + "</span>");
        });
        $("#" + bodyIdAbout).append(title);
        // ProductId
        title = $("<div id='" + idProductId +"'></div>");
        aContainer.Services().Volkano.ProductId(function(result){
            $("#" + idProductId).empty();
            $("#" + idProductId).append("<span class='AboutDescription'>" + kProductIdDisplay + "</span>");
            $("#" + idProductId).append("<span class='ElementInfo'>" + result.aProductNumber + "</span>");
        });
        $("#" + bodyIdAbout).append(title);
        // ModelInfo
        title = $("<div id='" + idModelInfo +"'></div>");
        $("#" + bodyIdAbout).append(title);
        // BoardInfoTitle
        title = $("<div id='" + idBoardInfoTitle +"'><span class='AboutDescriptionTitle'>" + kBoardTypeDisplay + "</span></div>");
        value = $("<span class='ElementInfoTitle'>" + kBoardIdDisplay + "</span>");
        title.append(value);
        $("#" + bodyIdAbout).append(title);
        // BoardInfo
        title = $("<div id='" + idBoardInfo +"'></div>");
        
        aContainer.Services().Volkano.DeviceInfo(function(resultBoardInfo){
            var allowedValues = [];
            var deviceInfoJson = $.xml2json(resultBoardInfo.aDeviceInfoXml);
            var extraInfo = "";
            $(deviceInfoJson.BoardList.Board).each(function(aIdx, aValue){
                if (extraInfo.length > 0 && aValue.Description.length > 0) {
                    extraInfo += ", ";
                }
                extraInfo += aValue.Description;
                
                var idBoardIndex = idBoardInfo + "_Board" + aIdx;
                if ($("#" + idBoardIndex).length == 0) {
                    $("#" + idBoardInfo).append("<div id='" + idBoardIndex +"'></div>");
                    $("#" + idBoardIndex).append("<span class='AboutInfoLeft'>" + aValue.TypePretty + "</span>");
                    $("#" + idBoardIndex).append("<span class='ElementInfo'>" + aValue.Id + "</span>");
                }
            });
            if (extraInfo.length > 0) {
                $("#" + idModelInfo).empty();
                $("#" + idModelInfo).append("<span class='AboutDescription'>" + kModelInfoDisplay + "</span>");
                $("#" + idModelInfo).append("<span class='ElementInfo'>" + extraInfo + "</span>");
            }
        }, function(message, transport){
	        	aContainer.Services().Proxy.HardwareVersion(function(result){
	                var idBoardIndex = idBoardInfo + "_ProxyBoard0";
	                if ($("#" + idBoardIndex).length == 0) {
	                    $("#" + idBoardInfo).append("<div id='" + idBoardIndex +"'></div>");
	                    $("#" + idBoardIndex).append("<span class='AboutInfoLeft'>" + (result.aHardwareVersion == null ? "Unknown" : result.aHardwareVersion) + "</span>");
	                    $("#" + idBoardIndex).append("<span class='ElementInfo'>" + (result.aHardwareVersion == null ? "Unknown" : "Main") + "</span>");
	                }
	        });
        });

        $("#" + bodyIdAbout).append(title);
	    return;
    }
    
    gProxyPreamp = Linn.ProductSupport.kModelProxyNone;
    gProxyCd = Linn.ProductSupport.kModelProxyNone;
    $(aContainer.ParameterJson.Parameter).each(function(aIdx, aParameter){
        var parameter = AppendAttributes(aParameter, aContainer, validHeaderIds, validParameterIds, validCollectionIds, aContainer.ServiceName);    
        if (parameter.IsVisible){
            var element = GetElement(parameter)
            if (element.length == 0){
                createdNew = true;
                element = CreateElement(parameter);
            }else{
                UpdateElement(parameter, element);
            }
        }
    });
    var unused = RemoveUnusedElements(validHeaderIds, validParameterIds, validCollectionIds, aContainer.ServiceName);    
    if (createdNew || unused > 0){
        var header = $("#header" + aContainer.ServiceName);
        if (header.data("tabs")){
            header.data("tabs").destroy();
        }
        $("#header" + aContainer.ServiceName).tabs("#body" + aContainer.ServiceName + " > div");
    }
}

var kHeaderPrefix = "Header_";
var kBodyPrefix = "Body_";
var kSelectSelectedAttribute = " selected='selected'";
var kComboBoxToggleOption = "User-defined...";

var gProxyPreamp;
var gProxyCd;

function AppendAttributes(aParameter, aContainer, aValidHeaderIds, aValidParameterIds, aValidCollectionIds, aServiceName){
    aParameter.Grouping = "" + aParameter.Target;
    if (aParameter.Collection) {
        aParameter.Grouping = aParameter.Collection;
    }
    
    // parameter order can be altered here - otherwise alphabetical
    aParameter.PriorityIndex = 0;
    aParameter.PriorityCollectionIndex = 0;
    aParameter.TrailingBreak = false;
    switch (aParameter.Grouping + aParameter.Name){    
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameRoom):
        {
            aParameter.PriorityIndex = 1;
            aParameter.TrailingBreak = true;
            break;
        }
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameEnableInternalVolumeControl):
        {
            aParameter.PriorityIndex = 1;
            aParameter.TrailingBreak = true;
            break;
        }
        case (Linn.Parameter.kTargetTuneIn + Linn.Parameter.kNameTuneInUsername):
        {
        	aParameter.PriorityIndex = 1;
            aParameter.TrailingBreak = true;
            break;
        }
        case (Linn.Parameter.kTargetRs232 + Linn.Parameter.kNameKontrolProductConnected):
        {
            aParameter.PriorityIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetRs232 + Linn.Parameter.kNameKontrolProductComPort):
        {
            aParameter.PriorityIndex = 2;
            aParameter.TrailingBreak = true;
            break;
        }
        case (Linn.Parameter.kTargetHdmi + Linn.Parameter.kNameHdmiOffInSleep):
        {
            aParameter.PriorityIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetHdmi + Linn.Parameter.kNameHdmiMode):
        {
            aParameter.PriorityIndex = 2;
            aParameter.TrailingBreak = true;
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceName):
        {
        	aParameter.PriorityIndex = 1;
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceIconName):
        {
            aParameter.PriorityIndex = 2;
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceVisible):
        {
        	aParameter.PriorityIndex = 3;
        	aParameter.TrailingBreak = true;
            break;
        }
        default : { break; }
    }
    
    switch (aParameter.Grouping + aParameter.Target){
    	case (Linn.Parameter.kCollectionSources + Linn.ProductSupport.kSourceNamePlaylist):
        {
            aParameter.PriorityCollectionIndex = 1;
            aParameter.TrailingBreak = false;
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.ProductSupport.kSourceNameRadio):
        {
            aParameter.PriorityCollectionIndex = 2;
            aParameter.TrailingBreak = false;
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.ProductSupport.kSourceNameSongcast):
        {
            aParameter.PriorityCollectionIndex = 3;
            aParameter.TrailingBreak = false;
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.ProductSupport.kSourceNameAirplay):
        {
            aParameter.PriorityCollectionIndex = 4;
            aParameter.TrailingBreak = false;
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.ProductSupport.kSourceNameUpnpAv):
        {
            aParameter.PriorityCollectionIndex = 5;
            aParameter.TrailingBreak = false;
            break;
        }
        default : { break; }
    }
    
    aParameter.HeaderId = kHeaderPrefix + StripIllegalCharacters(aServiceName) + StripIllegalCharacters(aParameter.Grouping);
    aParameter.BodyId = kBodyPrefix + StripIllegalCharacters(aServiceName) + StripIllegalCharacters(aParameter.Grouping);
    if (!aValidHeaderIds.contains(aParameter.HeaderId)){
        aValidHeaderIds[aValidHeaderIds.length] = aParameter.HeaderId;
    }
    var priorityHeader = (aParameter.PriorityIndex > 0 ? "" + aParameter.PriorityIndex : "");
    var priorityCollectionHeader = (aParameter.PriorityCollectionIndex > 0 ? "" + aParameter.PriorityCollectionIndex : "");
    if (aParameter.Collection) {
        aParameter.Id = aParameter.HeaderId + "_" + priorityCollectionHeader + StripIllegalCharacters(aParameter.Target) + "_" + priorityHeader + StripIllegalCharacters(aParameter.Name);
        aParameter.CollectionId = aParameter.HeaderId + "_" + priorityCollectionHeader + StripIllegalCharacters(aParameter.Target);
        if (!aValidCollectionIds.contains(aParameter.CollectionId)) {
            aValidCollectionIds[aValidCollectionIds.length] = aParameter.CollectionId;
        }
    }
    else {
        aParameter.Id = aParameter.HeaderId + "_" + priorityHeader + StripIllegalCharacters(aParameter.Name);
    }
    aValidParameterIds[aValidParameterIds.length] = aParameter.Id;
    
    switch (aParameter.Type.toUpperCase()){
        case "INTEGER":
        {
            aParameter.MinValue = -2147483648;
            aParameter.MaxValue = 2147483647;
            break;
        }
        case "POSITIVEINTEGER":
        {
            aParameter.MinValue = 0;
            aParameter.MaxValue = 4294967295;
            break;
        }
        case "BOOLEAN":
        {
            // convert bool to enum type using provided boolean text values
            aParameter.Type = "enum";
            aParameter.AllowedValues = [{Text:aParameter.BooleanValues.True, Value:'true'}, {Text:aParameter.BooleanValues.False, Value:'false'}];
            aParameter.Value = "" + aParameter.Value;
            break;
        }
        case "STRING":
        case "URI":
        {
            if (aParameter.AllowedValueList) {
                aParameter.Type = "enum";
                var allowedValues = [];
                $(aParameter.AllowedValueList.Value).each(function(aIdx, aValue){
                    allowedValues[allowedValues.length] = {Text:aValue, Value:aValue};
                });
                aParameter.AllowedValues = allowedValues;
                aParameter.Value = "" + aParameter.Value;
            }
            break;
        }
        default: {break;}
    }
    
    // PARAMETER SPECIFIC TWEAKS
    aParameter.IsComboBox = false;        
    aParameter.IsVisible = true;
    aParameter.DisplayName = aParameter.Name;
    aParameter.ForceRefresh = false;
    aParameter.ServiceName = aServiceName;
    aParameter.RequiresReboot = false;
    aParameter.ProductService = aContainer.Services().Product;

    switch (aParameter.Grouping + aParameter.Name){    
        case (Linn.Parameter.kTargetTuneIn + Linn.Parameter.kNameTuneInPassword):
        case (Linn.Parameter.kTargetTuneIn + Linn.Parameter.kNameTuneInTestMode):
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameStandbyDisabled):
        {
        	if (isReleaseBuild) {
                aParameter.IsVisible = false;
        	}
            break;
        }
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameEnableInternalVolumeControl):
        case (Linn.Parameter.kTargetRs232 + Linn.Parameter.kNameDiscPlayerComPort):
        case (Linn.Parameter.kTargetRs232 + Linn.Parameter.kNameKontrolProductComPort):
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameHdmi):
        {
            aParameter.RequiresReboot = true;
            break;
        }
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameStartupVolume):
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameVolumeLimit):
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameAnalogAttenuation):
        {
            aParameter.DisplayName = aParameter.Name + " (" + Linn.Parameter.kUnitsDb + ")";
            break;
        } 
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceAdcInputLevel):
        {
            aParameter.DisplayName = aParameter.Name + " (" + Linn.Parameter.kUnitsVrms + ")";
            break;
        }
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameHeadphoneVolumeOffset):
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceVolumeOffset):
        case (Linn.Parameter.kTargetHdmi + Linn.Parameter.kNameHdmiDownstreamVolumeOffset):
        {
            aParameter.ForceRefresh = true;
            aParameter.DisplayName = aParameter.Name + " (" + Linn.Parameter.kUnitsDb + ")";
            aParameter.Value = MilliDbToDb(aParameter.Value*1);
            aParameter.CoerceValueFunction = function(aValue){
                return DbToMilliDb(aValue) + "";
            };
            break;
        } 
        case (Linn.Parameter.kCollectionDelays + Linn.Parameter.kNameDelayPresetDelay):
        {    
            aParameter.ForceRefresh = true;
            aParameter.DisplayName = aParameter.Name + " (" + Linn.Parameter.kUnitsMs + ")";
            aParameter.Value = RoundToFive(aParameter.Value*1);
            aParameter.CoerceValueFunction = function(aValue){
                return RoundToFive(aValue) + "";
            }
            break;
        }
        case (Linn.Parameter.kTargetHdmi + Linn.Parameter.kNameHdmiAvLatency):
        {    
            aParameter.DisplayName = aParameter.Name + " (" + Linn.Parameter.kUnitsMs + ")";
            break;
        }
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameStartupSourceIndex):
        case (Linn.Parameter.kTargetRem020Handset + Linn.Parameter.kNameDirectSource1):
        case (Linn.Parameter.kTargetRem020Handset + Linn.Parameter.kNameDirectSource2):
        case (Linn.Parameter.kTargetRem020Handset + Linn.Parameter.kNameDirectSource3):
        {
            aParameter.Type = "enum";
            var allowedValues = [];
            var sourceJson = $.xml2json(containers[aServiceName].Services().Product.Variables().SourceXml.Value());
            $(sourceJson.Source).each(function(aIdx, aValue){
                allowedValues[allowedValues.length] = {Text:aValue.Name, Value:aIdx};
            });
            aParameter.AllowedValues = allowedValues;
            break;
        }
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameStartupSourceEnabled):
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceName):
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceVisible):
        {
        	if (aContainer.ServiceName == "Cd") {
               aParameter.IsVisible = false;
            }
        	break;
        }
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameCurrentDelayPreset):
        {
            aParameter.Type = "enum";
            var allowedValues = [];
            var delayPresetJson = $.xml2json(containers[aServiceName].Services().Configuration.Variables().ParameterXml.Value());
            $(delayPresetJson.Parameter).each(function(aIdx, aValue){
                if (aValue.Collection == Linn.Parameter.kCollectionDelays && aValue.Name == Linn.Parameter.kNameDelayPresetName) {
                    var substring = aValue.Target.substring(Linn.Parameter.kDelayPresetPrefix.length);
                    var index = parseInt(substring) - 1;
                    allowedValues[allowedValues.length] = {Text:aValue.Value, Value:index};
                }
            });
            aParameter.AllowedValues = allowedValues;
            break;
        }
        case (Linn.Parameter.kTargetRs232 + Linn.Parameter.kNameKontrolProductConnected):
        {   
        	gProxyPreamp = aParameter.Value;
            aParameter.RequiresReboot = true;
            var proxyPresent = aParameter.Value != Linn.ProductSupport.kModelProxyNone;
            if (proxyPresent && typeof(containers["Preamp"]) == "undefined"){  
            	if (!isSerivceSpecified) { // no tabs for proxy devices if a particular service is requested             
                    containers["Preamp"] = AppendContainer("Preamp", false);
            	}
                if (gProxyPreamp == gProxyCd && typeof(containers["Cd"]) != "undefined") {
	                RemoveContainer("Cd");
	            }
            }
            else if (!proxyPresent && typeof(containers["Preamp"]) != "undefined"){
                RemoveContainer("Preamp");
            }
            break;
        }
        case (Linn.Parameter.kTargetRs232 + Linn.Parameter.kNameDiscPlayerConnected):
        {   
        	gProxyCd = aParameter.Value;
            aParameter.RequiresReboot = true;
            var proxyPresent = aParameter.Value != Linn.ProductSupport.kModelProxyNone;
            if (proxyPresent && gProxyPreamp != gProxyCd && typeof(containers["Cd"]) == "undefined"){  
            	if (!isSerivceSpecified) { // no tabs for proxy devices if a particular service is requested            
                    containers["Cd"] = AppendContainer("Cd", false);
            	}
            }
            else if (!proxyPresent && typeof(containers["Cd"]) != "undefined"){
                RemoveContainer("Cd");
            }
            break;
        }
        case (Linn.Parameter.kTargetJukebox + Linn.Parameter.kNameJukeboxPresetPrefix):
        {
            aParameter.Type = "uri";
            break;
        }
        case (Linn.Parameter.kTargetTuneIn + Linn.Parameter.kNameTuneInUsername):
        {
            aParameter.Value = aParameter.Value.toLowerCase();
            aParameter.Type = "enum";    
            aParameter.DisplayName = aParameter.Name;     
            aParameter.AllowedValues = [
                                        {Text:kComboBoxToggleOption, Value:""},
                                        {Text:"Worldwide (Default)", Value:"linnproducts"},
                                        {Text:"Australia", Value:"linnproducts-australia"},
                                        {Text:"Belgium (Flemish)", Value:"linnproducts-belgium-vlaanderen"},
                                        {Text:"Belgium (French)", Value:"linnproducts-belgique"},
                                        {Text:"France", Value:"linnproducts-france"},
                                        {Text:"Germany", Value:"linnproducts-germany"},
                                        {Text:"Germany (South)", Value:"linnproducts-germany-south"},
                                        {Text:"Gibraltar", Value:"linnproducts-gibraltar"},
                                        {Text:"Japan", Value:"linnproducts-japan"},
                                        {Text:"Netherlands", Value:"linnproducts-netherlands"},
                                        {Text:"New Zealand", Value:"linnproducts-newzealand"},
                                        {Text:"Poland", Value:"linnproducts-poland"},
                                        {Text:"Portugal", Value:"linnproducts-portugal"},
                                        {Text:"Russia", Value:"linnproducts-russia"},
                                        {Text:"Spain", Value:"linnproducts-spain"},
                                        {Text:"Switzerland", Value:"linnproducts-switzerland"},
                                        {Text:"UK", Value:"linnproducts-uk"},
                                        {Text:"USA", Value:"linnproducts-usa"}
                                       ]; 
            aParameter.IsComboBox = true;
            break;
        }
        default: {
        	break;
        }
    }
    
    return aParameter;
}

function GetHelpText(aParameter, aErrorInfoOnly)
{
	var modelName = aParameter.ProductService.Variables().ModelName.Value();
	var kDeviceName = (modelName == null ? "Device" : modelName);
	var kDescriptionHeader = "<span class='HelpTextDescription'>";
	var kDescriptionFooter = "</span><hr/>";
	var kCurrentValueHeader = "<span class='HelpTextTitle'>Current Value: </span><span class='HelpTextOption'>";
	var kCurrentValueFooter = "</span><br/>"; 
	var kOptionsHeader = "<span class='HelpTextTitle'>Options: </span><span class='HelpTextOption'>";
	var kOptionsFooter = "</span><br/>";
	var kDefaultHeader = "<span class='HelpTextTitle'>Default: </span><span class='HelpTextOption'>";
	var kDefaultFooter = "</span><br/>"; 
	var kRebootHeader = "<hr/><span class='HelpTextTitle'>Reboot Required: </span>";
    var kRebootFooter = "<br/>"; 
    var kToggleHeader = "<hr/>";
    var kToggleFooter = "<br/>";
	
	var kOptionsString = "Maximum of 20 characters, can not be blank.";
	var kOptionsUri = "Valid URL (Maximum of 1024 bytes)";
	var kOptionsVolumeOffset = "-15 to 15 in 0.5" + Linn.Parameter.kUnitsDb + " steps";
	
	var descriptionValue = "Help Description Missing";
	var currentValue = "";
	var optionsValue = "";
	var defaultValue = "";
	var defaultIndex = 0;
    
    switch (aParameter.Grouping + aParameter.Name){    
	    case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameRoom): {
	        descriptionValue = "Enter a name for the room the " + kDeviceName + " is installed in (e.g. Lounge), for display on a Linn control point. This setting should be consistent across a system.";
	        defaultValue = "Main Room";
	        break;
	    }
	    case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameName): {
	        descriptionValue = "Enter a name for the " + kDeviceName + ", for display on a Linn control point.";
	        defaultValue = kDeviceName;
	        break;
	    }
	    case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameHandsetCommandsAccepted): {
	        descriptionValue = "Select which commands received via the IR receiver on the front panel the " + kDeviceName + " will accept. Please note, the <span class='HelpTextOption'>" + Linn.Parameter.kTargetRem020Handset + "</span> (REM020) uses <span class='HelpTextOption'>" + aParameter.AllowedValues[3].Text + "</span> mode only.";
	        defaultIndex = 1;
	        break;
	    }
	    case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameBasik3CommandsAccepted): {
            descriptionValue = "Select which commands received via the Basik 3 in-wall control terminal the " + kDeviceName + " will accept. Basik 3 must be connected to the provided socket on the " + kDeviceName + ".";
            defaultIndex = 1;
            break;
        }
	    case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameEnableInternalPowerAmp): {
            descriptionValue = "If you wish to use the " + kDeviceName + " with an external power amplifier, select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>. Otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> to use the internal power amplifier.";
            break;
        }
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameDigitalOutputMode): {
            descriptionValue = "If your digital audio receiver can process audio at any sample rate, select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span> or <span class='HelpTextOption'>" + aParameter.AllowedValues[2].Text + "</span> to sample convert all audio to 88/96kHz or 176/192kHz. Select <span class='HelpTextOption'>" + aParameter.AllowedValues[3].Text + "</span> if you are not using the digital output.";
            break;
        }
	    case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameStartupSourceEnabled): {
            descriptionValue = "If you wish the " + kDeviceName + " to select a specific source when brought out of sleep, select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise the last source selected will remain selected out of sleep. Must be set in combination with <span class='HelpTextOption'>" + Linn.Parameter.kNameStartupSourceIndex + "</span>.";
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameStartupSourceIndex): {
            descriptionValue = "Set the specific source the " + kDeviceName + " selects when brought out of sleep. Must be set in combination with <span class='HelpTextOption'>" + Linn.Parameter.kNameStartupSourceEnabled + "</span>.";
            break;
        }
	    case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameAutoPlayEnabled): {
            descriptionValue = "If you wish the " + kDeviceName + " to auto play on source selection (including coming out of sleep), select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>.";
            defaultIndex = 1;
            break;
        }
	    case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameBootIntoStandby): {
            descriptionValue = "If you wish the " + kDeviceName + " to start in sleep mode when the device powers on, select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>.";
            break;
        }
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameCurrentDelayPreset): {
            descriptionValue = "Select which delay preset (used for lip sync delay) will be applied to all <span class='HelpTextOption'>" + Linn.Parameter.kCollectionSources + "</span> with <span class='HelpTextOption'>" + Linn.Parameter.kNameSourceDelayMode + "</span> configured for variable delay. See <span class='HelpTextOption'>" + Linn.Parameter.kCollectionDelays + "</span> for individual delay configuration.";
            break;
        }
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameAudioOutputMono): {
            descriptionValue = "Select your preferred audio output mode. <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span> will provide two independent signals through two separate channels. <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> will provide a single signal to both channels."
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameOutputModeRca): {
            descriptionValue = "Select your preferred analog output mode. This setting should match the output connection used on the " + kDeviceName + ".";
            defaultIndex = (kDeviceName == Linn.ProductSupport.kModelAkurateKontrol ? 0 : 1);
            break;
        }
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameHdmi): {
            descriptionValue = "If you are not using the available HDMI inputs on the " + kDeviceName + " select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>, otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>";
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceName): {
            descriptionValue = "Enter a name for the given source, for display on a Linn control point.<br/><br/><span class='HelpTextTitle'>System Setup: </span>external input names should match the Device <span class='HelpTextOption'>" + Linn.Parameter.kNameName + "</span> connected to that input. All devices in a particular system should have a consistent <span class='HelpTextOption'>" + Linn.Parameter.kNameRoom + "</span> setting as well.";
            defaultValue = aParameter.Target;
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceIconName): {
            descriptionValue = "Select the icon that will appear on the front panel display of the " + kDeviceName + " when the " + aParameter.Target + " source is in use.";
            defaultValue = aParameter.Target;
            defaultIndex = -1; // insure AllowedValues(defaultIndex) is ignored
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceVisible): {
            descriptionValue = "If you wish the given source to be visible and selectable from a Linn control point, select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>.";
            defaultIndex = (aParameter.Target == "UpnpAv" ? 1 : 0);
            break;
        }
	    case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceVolumeOffset): {
            descriptionValue = "If you wish the volume of the given source to be automatically increased/decreased by a fixed amount, enter the value in " + Linn.Parameter.kUnitsDb + ".";
            optionsValue = kOptionsVolumeOffset;
            defaultValue = "0.0";
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceUnityGain): {
            descriptionValue = "If you wish the volume of the given source to fixed at a value of 0" + Linn.Parameter.kUnitsDb + " (equivalent to volume = 80), select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>.";
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceAdcInputLevel): {
            descriptionValue = "Set the analog input level for the given source in " + Linn.Parameter.kUnitsVrms + ". This value should match the output level of the corresponding external source.";
            defaultValue = "2";
            defaultIndex = -1; // insure AllowedValues(defaultIndex) is ignored
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceDelayMode): {
        	pureAnalog = (kDeviceName == Linn.ProductSupport.kModelSekritDsi ? "" : " (This provides a pure analog audio path for analog sources as well)");
            descriptionValue = "If you require zero latency (no delay) for the given source from input to output select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>" + pureAnalog + ". If you require source synchronisation with attached Songcast receivers when used as an Songcast sender (fixed minimum delay) select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>. If you require variable lip sync delay (also provides Songcast synchronisation) select <span class='HelpTextOption'>" + aParameter.AllowedValues[2].Text + "</span>. You will need to configure your <span class='HelpTextOption'>" + Linn.Parameter.kCollectionDelays + "</span> and select your <span class='HelpTextOption'>" + Linn.Parameter.kNameCurrentDelayPreset + "</span> for lip sync to be active.";
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameSourceTransformerEnabled): {
            descriptionValue = "If you wish to enable the source input transformer select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. If you wish to bypass the source input transformer select select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>.";
            break;
        }
        case (Linn.Parameter.kCollectionSources + Linn.Parameter.kNameNetAuxAutoSwitchEnable): {
            descriptionValue = "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> to have the " + aParameter.Target +  " source selected automatically when a compatible device is connected. Select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span> to force the " + aParameter.Target + " source to be selected manually (Must be <span class='HelpTextOption'>" + Linn.Parameter.kNameSourceVisible + "</span> as well for manual selection).";
            break;
        }
        case (Linn.Parameter.kCollectionDelays + Linn.Parameter.kNameDelayPresetName): {
            descriptionValue = "Enter a name for the given delay.";
            defaultValue = aParameter.Target;
            if (defaultValue == Linn.Parameter.kDelayPresetPrefix + "1") {
            	defaultValue = "TV";
            }
            else if (defaultValue == Linn.Parameter.kDelayPresetPrefix + "2") {
                defaultValue = "Projector";
            }
            break;
        }
        case (Linn.Parameter.kCollectionDelays + Linn.Parameter.kNameDelayPresetVisible): {
            descriptionValue = "If you wish the given delay to be available when browsing presets from the IR handset, select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>.";
            break;
        }
        case (Linn.Parameter.kCollectionDelays + Linn.Parameter.kNameDelayPresetDelay): {
            descriptionValue = "Enter a value for the given delay (lip sync) in " + Linn.Parameter.kUnitsMs + ".";
            defaultValue = "100";
            optionsValue = "100 to 2000 in 5" + Linn.Parameter.kUnitsMs + " steps";
            break;
        }
        case (Linn.Parameter.kTargetSender + Linn.Parameter.kNameSenderEnabled): {
            descriptionValue = "If you wish to allow other Songcast receivers in your home to listen to the " + kDeviceName + " select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>.";
            break;
        }
        case (Linn.Parameter.kTargetSender + Linn.Parameter.kNameSenderPreset): {
            descriptionValue = "If you wish the " + kDeviceName + " (Songcast sender) to be selectable as a numeric preset from the handset, set the preset number.";
            optionsValue = "0 (Disabled), 1 to 9999 (Enabled)";
            defaultValue = "0 (Disabled)";
            break;
        }
        case (Linn.Parameter.kTargetSender + Linn.Parameter.kNameSenderMulticast): {
            descriptionValue = "Only select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> if your network is capable of handling multicast.";
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetSender + Linn.Parameter.kNameSenderChannel): {
            descriptionValue = "Only change this setting if you have a channel conflict between two Songcast sender devices in your home.<br/><br/>This setting is only relevant if <span class='HelpTextOption'>" + Linn.Parameter.kNameSenderMulticast + "</span> is set to <span class='HelpTextOption'>" + Linn.Parameter.kValueMulticast + "</span>.";
            optionsValue = "0 to 65535";
            defaultValue = "Random";
            break;
        }
        case (Linn.Parameter.kTargetDisplay + Linn.Parameter.kNameDisplayBrightness): {
            descriptionValue = "Enter a number from 0 (Off) to 100 (Full Brightness) for setting the brightness of the " + kDeviceName + " front panel display";
            optionsValue = "0 to 100";
            defaultValue = (kDeviceName == Linn.ProductSupport.kModelKikoDsm ? "50" : "100");
            break;
        }
        case (Linn.Parameter.kTargetDisplay + Linn.Parameter.kNameDisplaySleep): {
        	descriptionValue = (kDeviceName == Linn.ProductSupport.kModelSekritDsi ? 
        	   "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> if you want the LEDS's on the " + kDeviceName + " to sleep after 10 seconds of inactivity (fault conditions will override this setting, IR input will wake the LED's for 10 seconds)." : 
        	   "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> if you want the front panel display on the " + kDeviceName + " to sleep after 10 seconds of inactivity (scrolling text will complete if <span class='HelpTextOption'>" + Linn.Parameter.kNameDisplayScrollText + "</span> is enabled).");
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetDisplay + Linn.Parameter.kNameDisplayScrollText): {
            descriptionValue = "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> to enable track metadata text (title, artist, and album) to scroll on the " + kDeviceName + " front panel display when the current track changes.";
            break;
        }
        case (Linn.Parameter.kTargetDisplay + Linn.Parameter.kNameDisplayAutoBrightness): {
            descriptionValue = "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> to enable the brightness of the " + kDeviceName + " front panel display to be set automatically based on ambient light levels.";
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetDisplay + Linn.Parameter.kNameDisplayFlipOrientation): {
            descriptionValue = "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> to flip the orientation of the " + kDeviceName + " front panel display upside down.";
            if (kDeviceName == Linn.ProductSupport.kModelKikoDsm) {
            	descriptionValue = "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span> to change the orientation of the " + kDeviceName + " front panel display to the vertical position. Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> to change the orientation of the " + kDeviceName + " front panel display to the horizontal position. This setting is only required if the display orientation does not change automatically when the device is rotated.";
            }
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetDisplay + Linn.Parameter.kNameDisplayFrontLedOffStandby): {
            descriptionValue = "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> to turn the blue display LED on the front of the " + kDeviceName + " off when in sleep (device still powered). Select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span> to dim the LED in sleep.";
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetDisplay + Linn.Parameter.kNameUpdateNotifications): {
            descriptionValue = "If you wish to be notified when a firmware update is available, select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise, select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>.";
            break;
        }
        case (Linn.Parameter.kTargetDisplay + Linn.Parameter.kNameDisplayOrientationLed): {
            descriptionValue = "If you wish to the Linn Logo LED to be visible on the front panel during normal operation of your " + kDeviceName + ", select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise, select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>.";
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetJukebox + Linn.Parameter.kNameJukeboxPresetPrefix): {
            descriptionValue = "Enter the location of your presets folder within your music collection. Must be a valid URL.<br/><br/><span class='HelpTextTitle'>Example: </span>http://&lt;Ip Address of Server&gt;/&lt;Music Collection Root&gt;/&lt;_Presets Folder&gt;";
            defaultValue = "";
            break;
        }
        case (Linn.Parameter.kTargetJukebox + Linn.Parameter.kNameJukeboxAutoLoadEnabled): {
            descriptionValue = "If you wish the " + kDeviceName + " to auto load the last jukebox preset selected on auto play (only if the playlist is empty), select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>. Must be set in combination with <span class='HelpTextOption'>" + Linn.Parameter.kNameAutoPlayEnabled + "</span>.";
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetTuneIn + Linn.Parameter.kNameTuneInUsername): {
            descriptionValue = "Enter your TuneIn Radio username for accessing your stored radio presets. Go to <a href='http://TuneIn.com/' target='_blank'>TuneIn.com</a> to setup your account. You may also select from the drop down list of Pre-defined accounts.";
            optionsValue = "<span class='HelpTextDescription'>User-defined account: </span>maximum of 128 characters. <span class='HelpTextDescription'>Pre-defined accounts: </span>"
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameVolumeLimit): {
            descriptionValue = "Set the maximum volume level of the " + kDeviceName + " in " + Linn.Parameter.kUnitsDb + ".";
            optionsValue = "0 to 100";
            defaultValue = "100";
            break;
        }
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameStartupVolumeEnabled): {
            descriptionValue = "If you wish the " + kDeviceName + " to select a specific volume in " + Linn.Parameter.kUnitsDb + " when brought out of sleep, select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>. Otherwise the last volume selected will remain selected out of sleep. Must be set in combination with <span class='HelpTextOption'>" + Linn.Parameter.kNameStartupVolume + "</span>.";
            break;
        }
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameStartupVolume): {
            descriptionValue = "Set the specific volume in " + Linn.Parameter.kUnitsDb + " the " + kDeviceName + " selects when brought out of sleep. Must be set in combination with <span class='HelpTextOption'>" + Linn.Parameter.kNameStartupVolumeEnabled + "</span>.";
            optionsValue = "0 to 100";
            defaultValue = "50";
            break;
        }
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameBalance): {
            descriptionValue = "Set the left/right balance of the " + kDeviceName + ".";
            optionsValue = "-15 (left) to 15 (right)";
            defaultValue = "0 (middle)";
            break;
        }
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameHeadphoneVolumeOffset): {
            descriptionValue = "If you wish the volume of the headphone output to be automatically increased/decreased by a fixed amount, enter the value in " + Linn.Parameter.kUnitsDb + ".";
            optionsValue = kOptionsVolumeOffset;
            defaultValue = "0.0";
            break;
        }
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameEnableInternalVolumeControl): {
        	var linnPreampNote = "(Selecting <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> will automatically disable any <span class='HelpTextOption'>" + Linn.Parameter.kNameKontrolProductConnected + "</span> connection)";
        	switch (kDeviceName) {
                case Linn.ProductSupport.kModelAkurateDsm:
                case Linn.ProductSupport.kModelKlimaxDsm:
                case Linn.ProductSupport.kModelMajikDsm:
                case Linn.ProductSupport.kModelKikoDsm:
                case Linn.ProductSupport.kModelSekritDsi:
                case Linn.ProductSupport.kModelMajikDsi:
                case Linn.ProductSupport.kModelAkurateKontrol: {
                	linnPreampNote = "";
                	defaultIndex = 0;
                	break;
                }
                default: {
                	defaultIndex = 1;
                	break;
                }
            }
            descriptionValue = "If you wish to use the " + kDeviceName + " with an external pre/power amplifier, select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span>. Otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span>" + linnPreampNote + ".";
            break;
        }
        case (Linn.Parameter.kTargetVolume + Linn.Parameter.kNameAnalogAttenuation): {
            descriptionValue = "Set the analog output attenuation level for the " + kDeviceName + " in " + Linn.Parameter.kUnitsDb + ". Smaller (more negative) attenuation will make the " + kDeviceName + " quieter. 0" + Linn.Parameter.kUnitsDb + " is the loudest setting.";
            defaultIndex = (kDeviceName == Linn.ProductSupport.kModelSneakyMusicDs ? 0 : 1);
            break;
        }
        case (Linn.Parameter.kTargetRs232 + Linn.Parameter.kNameKontrolProductConnected): {
            descriptionValue = "If the " + kDeviceName + " is connected to a Linn Preamp that can be controlled over RS232, select that product from the list (Selecting a Linn Preamp will automatically disable <span class='HelpTextOption'>" + Linn.Parameter.kNameEnableInternalVolumeControl + "</span>).";
            break;
        }
        case (Linn.Parameter.kTargetRs232 + Linn.Parameter.kNameKontrolProductComPort): {
            descriptionValue = "If the " + kDeviceName + " is connected to a Linn Preamp that can be controlled over RS232, select the COM port it is connected on. Available RS232 ports are labelled on the back of the " + kDeviceName + ".";
            break;
        }
        case (Linn.Parameter.kTargetRs232 + Linn.Parameter.kNameDiscPlayerConnected): {
            descriptionValue = "If the " + kDeviceName + " is connected to a Linn Disc Player that can be controlled over RS232, select that product from the list.";
            break;
        }
        case (Linn.Parameter.kTargetRs232 + Linn.Parameter.kNameDiscPlayerComPort): {
            descriptionValue = "If the " + kDeviceName + " is connected to a Linn Disc Player that can be controlled over RS232, select the COM port it is connected on. Available RS232 ports are labelled on the back of the " + kDeviceName + ".";
            break;
        }
        case (Linn.Parameter.kTargetTuneIn + Linn.Parameter.kNameTuneInPassword):
        case (Linn.Parameter.kTargetTuneIn + Linn.Parameter.kNameTuneInTestMode):
        case (Linn.Parameter.kTargetDevice + Linn.Parameter.kNameStandbyDisabled): {
        	descriptionValue = "This parameter is not user visible. Used for test purposes only.";
        	defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetRem020Handset + Linn.Parameter.kNameDirectSource1):
        case (Linn.Parameter.kTargetRem020Handset + Linn.Parameter.kNameDirectSource2):
        case (Linn.Parameter.kTargetRem020Handset + Linn.Parameter.kNameDirectSource3): {
            descriptionValue = "Select a source to map to the <span class='HelpTextOption'>" + aParameter.Name + "</span>. Setting only applies to the <span class='HelpTextOption'>" + Linn.Parameter.kTargetRem020Handset + "</span> (REM020).";
            switch (kDeviceName) {
            	case Linn.ProductSupport.kModelAkurateDsm: { defaultIndex = 15; break; }
            	case Linn.ProductSupport.kModelKlimaxDsm: { defaultIndex = 10; break; }
            	case Linn.ProductSupport.kModelSekritDsi: { defaultIndex = 5; break; }
            	case Linn.ProductSupport.kModelMajikDsi: { defaultIndex = 13; break; }
            	case Linn.ProductSupport.kModelMajikDsm: { defaultIndex = 16; break; }
            	case Linn.ProductSupport.kModelKikoDsm: { defaultIndex = 9; break; }
            	case Linn.ProductSupport.kModelAkurateKontrol: { defaultIndex = 0; break; }
            	default: { defaultIndex = -1; break; } // insure AllowedValues(defaultIndex) is ignored
            }
            if (defaultIndex >= 0) {
                defaultIndex += Number(aParameter.Name.match(/\d+/)[0]) - 1;
            }
            break;
        }
        case (Linn.Parameter.kTargetHdmi + Linn.Parameter.kNameHdmiOffInSleep):
        {
            descriptionValue = "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> to turn HDMI pass through off in sleep mode for the purpose of power saving. Select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span> to allow HDMI pass through in sleep mode, for the purpose of continued listening to HDMI sources through a downstream device (i.e. your Television)";
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetHdmi + Linn.Parameter.kNameHdmiMode):
        {
        	descriptionValue = "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> to enable integration of the " + kDeviceName + " with an existing AV Receiver for surround audio. Otherwise select <span class='HelpTextOption'>" + aParameter.AllowedValues[1].Text + "</span> for normal operation";
            defaultIndex = 1;
        	break;
        }
        case (Linn.Parameter.kTargetHdmi + Linn.Parameter.kNameHdmiMixCentreChannel):
        {
            descriptionValue = "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> if there is no centre speaker in your surround system (downmix centre channel audio to front speakers).<br/><br/>This setting is only relevant if <span class='HelpTextOption'>" + Linn.Parameter.kNameHdmiMode + "</span> is set to <span class='HelpTextOption'>" + Linn.Parameter.kValueSurroundMode + "</span>.";
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetHdmi + Linn.Parameter.kNameHdmiMixLfeChannel):
        {
            descriptionValue = "Select <span class='HelpTextOption'>" + aParameter.AllowedValues[0].Text + "</span> if there is no subwoofer in your surround system (downmix low frequency effects to front speakers).<br/><br/>This setting is only relevant if <span class='HelpTextOption'>" + Linn.Parameter.kNameHdmiMode + "</span> is set to <span class='HelpTextOption'>" + Linn.Parameter.kValueSurroundMode + "</span>.";
            defaultIndex = 1;
            break;
        }
        case (Linn.Parameter.kTargetHdmi + Linn.Parameter.kNameHdmiDownstreamVolumeOffset):
        {
            descriptionValue = "Use this setting to align the volume of the front speakers (" + kDeviceName + ") with the volume of the surround speakers (AV Receiver).<br/><br/>Positive values will boost the volume of the surround speakers, negative values will boost the volume of the front speakers.<br/><br/>This setting is only relevant if <span class='HelpTextOption'>" + Linn.Parameter.kNameHdmiMode + "</span> is set to <span class='HelpTextOption'>" + Linn.Parameter.kValueSurroundMode + "</span>.";
            optionsValue = kOptionsVolumeOffset;
            defaultValue = "0.0";
            break;
        }
        case (Linn.Parameter.kTargetHdmi + Linn.Parameter.kNameHdmiAvLatency):
        {
            descriptionValue = "Enter a value for the native latency of your AV Receiver in " + Linn.Parameter.kUnitsMs + " (sync audio between surround speakers and front speakers).<br/><br/>This setting is only relevant if <span class='HelpTextOption'>" + Linn.Parameter.kNameHdmiMode + "</span> is set to <span class='HelpTextOption'>" + Linn.Parameter.kValueSurroundMode + "</span>.";
            defaultValue = "0";
            optionsValue = "0 to 500 in 1" + Linn.Parameter.kUnitsMs + " steps";
            break;
        }
        default: { break; }
    }
    if (aParameter.Type.toUpperCase() == "STRING") {
    	optionsValue = kOptionsString;
    }
    else if (aParameter.Type.toUpperCase() == "URI") {
    	optionsValue = kOptionsUri;
    }
    if (aParameter.AllowedValues) {
    	$(aParameter.AllowedValues).each(function(aIdx, aValue){
            if (aIdx == defaultIndex) {
                defaultValue = aValue.Text;
            }
            if (aValue.Text != kComboBoxToggleOption) {
	            optionsValue += aValue.Text;
	            if (aIdx < (aParameter.AllowedValues.length-1)) {
                    optionsValue += ", ";
                }
            }
        });
    }
    
    var text = $("<span class='HelpTextComplete'></span>");
    // help description
    if (descriptionValue.length > 0 && !aErrorInfoOnly) {
        text.append(kDescriptionHeader + descriptionValue + kDescriptionFooter);
    }
    // current value (may be blank)
    currentValue = GetElementCurrentValue(aParameter, aErrorInfoOnly);
    text.append(kCurrentValueHeader + currentValue + kCurrentValueFooter);
    // options
    if (optionsValue.length > 0) {
        text.append(kOptionsHeader + optionsValue + kOptionsFooter);
    }
    // default (may be blank)
    if (!aErrorInfoOnly) {
        text.append(kDefaultHeader + defaultValue + kDefaultFooter);
    }
    // reboot message
    if (aParameter.RequiresReboot && !aErrorInfoOnly) {
        var rebootIcon = $("<span class='SmallIcon'></span>")
	        .click(function(){
	        	if ($("#dialogHelpText").dialog('isOpen')) {
	        	  $("#dialogHelpText").dialog('close');
	        	}
	            Reboot(aParameter.ServiceName, aParameter.DisplayName);
	            return false; // prevent the default action, e.g., following a link
	        });
        var rebootIconRef = $("<a href='#' title='Reboot'></a>").appendTo(rebootIcon);
        $("<img id='RebootIconHelp' src='../../../images/Reboot.png'/>")
            .error(function(){
                this.src= "../../../../images/Reboot.png";
            }).appendTo(rebootIconRef);
            
        text.append(kRebootHeader + "<span class='HelpTextDescription'>Click the Reboot button </span>");
        text.append(rebootIcon);
        text.append("<span class='HelpTextDescription'> for the new setting to take effect.</span>" + kRebootFooter);  
    }
    // toggle message
    if (aParameter.IsComboBox && !aErrorInfoOnly) {
        var toggleIcon = $("<span class='SmallIcon'></span>")
            .click(function(){
            	if ($("#dialogHelpText").dialog('isOpen')) {
                  $("#dialogHelpText").dialog('close');
                }
                $(".Toggle_" + aParameter.Id).toggle();
                return false; // prevent the default action, e.g., following a link
            });
        var toggleIconRef = $("<a href='#' title='Toggle (User-defined | Pre-defined)'></a>").appendTo(toggleIcon);
        $("<img id='ToggleIconHelp' src='../../../images/Toggle.png'/>")
            .error(function(){
                this.src= "../../../../images/Toggle.png";
            }).appendTo(toggleIconRef);
            
        text.append(kToggleHeader + "<span class='HelpTextDescription'>Click the Toggle button </span>");
        text.append(toggleIcon);
        text.append("<span class='HelpTextDescription'> to change between User-defined and Pre-defined.</span>" + kToggleFooter);  
    }
    return text;
}

function StripIllegalCharacters(aString){
    return aString.replace(/[^a-zA-Z0-9]/g, '');
}

function SoftwareVersionPretty(aString) {
	var swSplit = aString.split('.');
	var newVersion = aString;
	if (swSplit.length == 3) {
		switch (swSplit[0]) {
			case "1": {
				newVersion = "Auskerry " + swSplit[1] + " (" + aString + ")";
				break;
			}
			case "2": {
                newVersion = "Bute " + swSplit[1] + " (" + aString + ")";
                break;
            }
            case "3": {
                newVersion = "Cara " + swSplit[1] + " (" + aString + ")";
                break;
            }
            case "4": {
                newVersion = "Davaar " + swSplit[1] + " (" + aString + ")";
                break;
            }
			default: {break;}
		}
	}
	return newVersion;
}

function RoundToFive(aValue){
    return (aValue % 5) >= 2.5 ? parseInt(aValue / 5,10) * 5 + 5 : parseInt(aValue / 5,10) * 5;
}

function MilliDbToDb(aValue){
    var value = RoundToHalf(aValue/1024);
    if (!/.*\.5$/.test(value)){
        value = value + ".0";
    }
    return value; 
}
    
function DbToMilliDb(aValue){
    return RoundToHalf(aValue)*1024; 
}

function RoundToHalf(aValue) {
   var converted = parseFloat(aValue);
   var decimal = (converted - parseInt(converted, 10));
   decimal = Math.round(decimal * 10);
   if (decimal == 5) { return (parseInt(converted, 10)+0.5); }
   if ( (decimal < 3) || (decimal > 7) ) {
      return Math.round(converted);
   } else {
      return (parseInt(converted, 10)+0.5);
   }
} 

function RemoveUnusedElements(aValidHeaderIds, aValidParameterIds, aValidCollectionIds, aServiceName){
    var count = 0;
    $("#header" + aServiceName + " > li").each(function(aIdx, aHeader){
        if (!aValidHeaderIds.contains($(aHeader).attr('id'))){
            $(aHeader).remove();
            count += 1;
        }
    });
    $("#body" + aServiceName + " > div > div").each(function(aIdx, aElement){       
        if (!aValidParameterIds.contains($(aElement).attr('id'))){
        	if (!aValidCollectionIds.contains($(aElement).attr('id'))){
                $(aElement).remove();
        	}
        }
    }); 
    return count;
}

function GetElement(aParameter){
    return $("#" + aParameter.Id);
}

function CreateElement(aParameter){    
	var container = GetContainer(aParameter);
    if (!container){
        container = CreateContainer(aParameter);        
    }
    var body = container.body;
    var newElement = CreateElementHtml(aParameter);
    
    var appendCollection = false;
    var newCollection = [];
    if (aParameter.Collection) {
        // seaprate collection targets
        var collection = $("#" + aParameter.CollectionId);
        if (collection.length == 0){
            newCollection = $("<div id='" + aParameter.CollectionId + "'/>");
            newCollection.append("<hr/>");
            var collectionHeader = $("<h3></h3>").click(function(){
	            var elem = collectionHeader.find("a");
	            if (elem.attr('title') == "Expand") {
	            	elem.attr('title', "Collapse");
	            	elem.empty();
	            	elem.append("[ &minus; ]&nbsp;&nbsp;" + aParameter.Target);          
	            }
	            else {
	            	elem.attr('title', "Expand");
                    elem.empty();
                    elem.append("[ + ]&nbsp;&nbsp;" + aParameter.Target);
	            }
	            $(".Toggle_" + aParameter.CollectionId).toggle();
	            return false; // prevent the default action, e.g., following a link
	        }).appendTo(newCollection);
	        $("<a title='Expand' href='#'>[ + ]&nbsp;&nbsp;" + aParameter.Target + "</a>").appendTo(collectionHeader);
            appendCollection = true;        
        }
    }
    
    var bodyEntries = body.children();
    var groupListBody = [];
    for (var i=0; i<bodyEntries.length; i++){
        groupListBody[i] = bodyEntries[i].id;
    }
    if (appendCollection) {
        groupListBody[groupListBody.length] = aParameter.CollectionId;
    }
    groupListBody[groupListBody.length] = aParameter.Id;
    groupListBody.sort();
    
    var loc = $.inArray(aParameter.Id, groupListBody) + 1;
    if ((loc > 0) && (loc < groupListBody.length)) {
        // not the last entry in the array so place it alphapbetically
        $("#" + groupListBody[loc]).before(newElement);
        if (appendCollection) {
        	$(newElement).before(newCollection);
        }
    }
    else {
    	if (appendCollection) {
            body.append(newCollection);
        }
        body.append(newElement);
    }

    return newElement;
}

function CreateElementHtml(aParameter){
	var expandable = (aParameter.Collection ? "class='Expandable Toggle_" + aParameter.CollectionId + "'" : "");
    var placeholder = $("<div id='" + aParameter.Id +"'" + expandable + "></div>");
    var helpIcon = $("<span class='SmallIcon'></span>")
        .click(function(){
        	ShowHelpText(aParameter);
        	return false; // prevent the default action, e.g., following a link
        }).appendTo(placeholder);
    var helpIconRef = $("<a href='#' title='Help'></a>").appendTo(helpIcon);
    $("<img id='HelpIcon' src='../../../images/Help.png'/>")
        .error(function(){
            this.src= "../../../../images/Help.png";
        }).appendTo(helpIconRef);
    placeholder.append("<span class='ElementDescription'>" + aParameter.DisplayName + "</span>");
    placeholder.append(CreateElementEditor(aParameter));
    if (aParameter.RequiresReboot){
    	var rebootIcon = $("<span class='SmallIcon'></span>")
	        .click(function(){
	            Reboot(aParameter.ServiceName, aParameter.DisplayName);
	            return false; // prevent the default action, e.g., following a link
	        }).appendTo(placeholder);
	    var rebootIconRef = $("<a href='#' title='Reboot'></a>").appendTo(rebootIcon);
	    $("<img id='RebootIcon' src='../../../images/Reboot.png'/>")
	        .error(function(){
	            this.src= "../../../../images/Reboot.png";
	        }).appendTo(rebootIconRef);
    }
    else if (aParameter.IsComboBox) {
    	var toggleIcon = $("<span class='SmallIcon'></span>")
            .click(function(){
                // change between drop down list and string editor
                $(".Toggle_" + aParameter.Id).toggle();
                return false; // prevent the default action, e.g., following a link
            }).appendTo(placeholder);
        var toggleIconRef = $("<a href='#' title='Toggle (User-defined | Pre-defined)'></a>").appendTo(toggleIcon);
        $("<img id='ToggleIcon' src='../../../images/Toggle.png'/>")
            .error(function(){
                this.src= "../../../../images/Toggle.png";
            }).appendTo(toggleIconRef);
    }
    if (aParameter.TrailingBreak) {
        placeholder.append("<hr/>");
    }
    return placeholder;
}

var helpDialog = $("<div id='dialogHelpText'></div>");
function ShowHelpText(aParameter) {
	helpDialog
        .html(GetHelpText(aParameter, false))
        .dialog({
        	buttons: { "Close": function() { $(this).dialog('close'); } },
            width: 400,
            resizable: false,
            modal: true,
            position: 'center',
            autoOpen: false,
            title: aParameter.DisplayName
        });
    helpDialog.dialog('open');
}

function Reboot(aServiceName){
	Reboot(aServiceName, null)
}

var rebootDialog = $("<div id='dialogReboot'></div>");
function Reboot(aServiceName, aParameterName){
    // show dialog
    var modelName = containers[aServiceName].Services().Product.Variables().ModelName.Value();
    var deviceName = (modelName == null ? "Device" : modelName);
    var dialogTitle = "Reboot";
    if (aParameterName) {
    	dialogTitle = "Reboot (Required)"
    }
    var dialogText = "Reboot the " + deviceName + " now?<br/><br/>";
    if (aParameterName) {
        dialogText = "Reboot required for new <span class='HelpTextOption'>" + aParameterName + "</span> setting to take effect. " + dialogText;
    }
    dialogText = "<span class='HelpTextDescription'>" + dialogText + "</span>";
    var rebootConfirm = $('<div></div>')
        .html(dialogText)
        .dialog({
            buttons: { "Yes": function() {
            	             $(this).dialog('close');
            	             containers[aServiceName].Services().Volkano.Reboot();
            	             rebootDialog
								    .html("Device is Rebooting...")
								    .dialog({
								        buttons: { "OK": function() {
								                       $(this).dialog('close');
								                   }},
								        width: 400,
								        resizable: false,
								        modal: true,
								        position: 'center',
								        autoOpen: false,
								        title: "Reboot"
								    });
            	             rebootDialog.dialog('open');
            	         },
                       "No": function() { 
                             $(this).dialog('close'); 
                         }, },
            width: 400,
            resizable: false,
            modal: true,
            position: 'center',
            autoOpen: false,
            title: dialogTitle
        });
    rebootConfirm.dialog('open');
}

function CreateElementEditor(aParameter){
    var editor;
    switch(aParameter.Type.toUpperCase()){
    	case "URI":
        case "STRING":
        case "INTEGER":
        case "POSITIVEINTEGER":
        {
            var validation = typeof(aParameter.Validation) != "undefined" ? "validation='" + aParameter.Validation + "' " : "";
            var maxLength = typeof(aParameter.MaxLength) != "undefined" ? "maxLength='" + aParameter.MaxLength + "' " : "";
            var minValue = typeof(aParameter.MinValue) != "undefined" ? "minValue='" + aParameter.MinValue + "' " : "";
            var maxValue = typeof(aParameter.MaxValue) != "undefined" ? "maxValue='" + aParameter.MaxValue + "' " : "";
            var target = "target='" + aParameter.Target + "' ";
            var parameterName = "parameterName='" + aParameter.Name + "' ";
            var parameterId = "parameterId='" + aParameter.Id + "' ";
            var initialValue = "initialValue='" + aParameter.Value + "' ";
            var value = "value='" + aParameter.Value + "' ";
            var className = "class='" + aParameter.Type + (isIPad ? "_iPad" : "") + "' ";
            var serviceName = "serviceName='" + aParameter.ServiceName + "' ";
            editor = $("<input type='text' " + className + maxLength + validation + target + parameterName + parameterId + initialValue + value + minValue + maxValue + serviceName + "/>");
            editor.keyup(InputChanged);
            editor.change(InputUpdated);            
            break;
        }
        case "ENUM":{
            editor = CreateSelect(aParameter); 
            if (aParameter.IsComboBox) {
            	// enum part
                var editor1 = $("<span class='ElementEditor' />").append(editor);
                // string part
                var visible = (editor.css('display') == 'none'); // enum and string should never be visible at the same time
                var validation = typeof(aParameter.Validation) != "undefined" ? "validation='" + aParameter.Validation + "' " : "";
	            var maxLength = typeof(aParameter.MaxLength) != "undefined" ? "maxLength='" + aParameter.MaxLength + "' " : "";
	            var minValue = typeof(aParameter.MinValue) != "undefined" ? "minValue='" + aParameter.MinValue + "' " : "";
	            var maxValue = typeof(aParameter.MaxValue) != "undefined" ? "maxValue='" + aParameter.MaxValue + "' " : "";
	            var target = "target='" + aParameter.Target + "' ";
	            var parameterName = "parameterName='" + aParameter.Name + "' ";
	            var parameterId = "parameterId='" + aParameter.Id + "' ";
	            var initialValue = "initialValue='" + aParameter.Value + "' ";
	            var value = "value='" + aParameter.Value + "' ";
	            var className = "class='string" + (isIPad ? "_iPad" : "") + " Toggle_" + aParameter.Id + "' ";
	            var serviceName = "serviceName='" + aParameter.ServiceName + "' ";
	            editor = $("<input type='text' " + className + maxLength + validation + target + parameterName + parameterId + initialValue + value + minValue + maxValue + serviceName + "/>");
	            editor.keyup(InputChanged);
	            editor.change(InputUpdated);
	            if (visible) {
	            	editor.css('display', 'inline');
	            }
	            else {
	            	editor.css('display', 'none');
	            }
                var editor2 = $("<span class='ElementEditor' />").append(editor);
                
                var comboEditor = $("<span class='ElementEditorCombo' />");
                comboEditor.append(editor2);
                comboEditor.append(editor1);
                return comboEditor;
            }
            break;
        }
        default:
        {
            editor = $("<span>" + aParameter.Value + "</span>");
            break;
        }
    }

    return $("<span class='ElementEditor' />").append(editor);
}

function CreateSelect(aParameter){
    var target = "target='" + aParameter.Target + "' ";
    var parameterName = "parameterName='" + aParameter.Name + "' ";
    var parameterId = "parameterId='" + aParameter.Id + "' ";
    var initialValue = "initialValue='" + aParameter.Value + "' ";
    var serviceName = "serviceName='" + aParameter.ServiceName + "' ";
    var className = (aParameter.IsComboBox ? "class='selectBox Toggle_" + aParameter.Id + "'" : "class='selectBox'");
    var select = $("<select " + className + target + parameterName + parameterId + initialValue + serviceName + "/>");
    var found = false;
    $(aParameter.AllowedValues).each(function(aIdx, aValue){
        var selected = (aParameter.Value == aValue.Value ? kSelectSelectedAttribute : "");
        select.append($("<option value='" + aValue.Value + "'" + selected + ">" + aValue.Text + "</option>"));
        if (aParameter.Value == aValue.Value){
            found = true;
            if (aParameter.IsComboBox) {
            	if (aValue.Value != "") {
                    select.css('display', 'inline');
            	}
            	else {
            		select.css('display', 'none');
            	}
            }
        }
    });    
    if (!found){
        select.append($("<option value='" + aParameter.Value + "'" + kSelectSelectedAttribute + ">" + (aParameter.IsComboBox ? aParameter.Value : "") + "</option>"));                
        if (aParameter.IsComboBox) {
            select.css('display', 'none');
        }
    }
    select.change(SelectUpdated); 
    return select;
}

function OnSelectChanged(elem){   
    var value = elem.find("option:selected").attr('value');
    elem.toggleClass("Changed", value != elem.attr('initialValue'));    
}

function SelectUpdated(){
    OnSelectUpdated($(this), $(this).find("option:selected").attr('value'));
}

function OnSelectUpdated(aElem, aNewValue){    
    var initialValue = aElem.attr('initialValue');
    var target = aElem.attr('target');
    var parameterName = aElem.attr('parameterName');
    var serviceName = aElem.attr('serviceName');
    var parameterId = aElem.attr('parameterId');

    if (initialValue != aNewValue){
    	var parameter = GetParameter(parameterId, serviceName);
    	if (parameter.IsComboBox && aNewValue == "") {
    		parameter.Value = aNewValue;
            Refresh(containers[serviceName], false);
        }
        containers[serviceName].Services().Configuration.SetParameter(target, 
	        parameterName, 
	        aNewValue, 
	        function(){
	        	aElem.attr('initialValue', aNewValue);
	        	OnSelectChanged(aElem);
	        	if (parameter.RequiresReboot) {
	        		Reboot(parameter.ServiceName, parameter.DisplayName);
	        	}
	        }, 
	        function(aMessage, aTransport){
	        	if (aMessage.indexOf("errorCode: 805") >= 0) {
                    ShowSetParameterErrorMessage(parameter, aMessage);
                }
                else {
                	var modelName = parameter.ProductService.Variables().ModelName.Value();
				    var deviceName = (modelName == null ? "Device" : modelName);
                    ShowCommsErrorMessage(deviceName);
                }
	        });
    }
}

function GetParameter(aId, aServiceName){
    var parameter;
    $(containers[aServiceName].ParameterJson.Parameter).each(function(aIdx, aParameter){
            if (aParameter.Id == aId){
                parameter = aParameter;
                return false;
            }
        });
        
    return parameter;
}

function InputChanged(){
    return Validate($(this));
}

function Validate(elem){    
    var newValue = elem.attr('value');
    var initialValue = elem.attr('initialValue');
    var validation = elem.attr('validation');
    elem.toggleClass("Changed", newValue != initialValue);
    var valid = true;
    var isNumericElement = (typeof(elem.attr('minValue')) != "undefined" || typeof(elem.attr('maxValue')) != "undefined");
    if (isNumericElement){
        var minValue = elem.attr('minValue');
        var maxValue = elem.attr('maxValue');
        var numeric = !isNaN(newValue); 
        if (!numeric || 
           (typeof(minValue) != "undefined" && newValue * 1 < minValue) || 
           (typeof(maxValue) != "undefined" && newValue * 1 > maxValue)){
            valid = false;
        }        
    }else if (typeof(validation) != "undefined"){
        var regex = new RegExp(validation);
        valid = regex.test(newValue);
    }
    elem.toggleClass("Invalid", !valid);
    return valid;
}

function ShowSetParameterErrorMessage(aParameter, aMessage) {
	var desc = aMessage.split("errorDescription: ");
	var message = (desc.length > 1 ? (desc[1] + ".<br>") : "");
    var errorDialog = $('<div></div>')
		.html($("<span class='HelpTextDescription'>" + message + "Please provide a valid value.<br/><br/></span>").append(GetHelpText(aParameter, true)))
		.dialog({
			buttons: { "OK": function() { $(this).dialog('close'); },
			           "Show Help": function() { 
			                 $(this).dialog('close'); 
			                 ShowHelpText(aParameter);
			             }, },
            width: 400,
		    resizable: false,
		    modal: true,
		    position: 'center',
		    autoOpen: false,
		    title: "<span class='HelpTextInvalid'>Invalid: </span>" + aParameter.DisplayName + ""
		});
    errorDialog.dialog('open');
}

function ShowCommsErrorMessage(aModelName) {
    var errorDialog = $('<div></div>')
        .html($("<span class='HelpTextDescription'>Could not communicate with your " + aModelName + ".<br/><br/>Please ensure your " + aModelName + " is still connected to the network and has not changed IP address.<br/><br/></span>"))
        .dialog({
            buttons: { "OK": function() { $(this).dialog('close'); }, },
            width: 400,
            resizable: false,
            modal: true,
            position: 'center',
            autoOpen: false,
            title: "<span class='HelpTextInvalid'>Connection Error</span>"
        });
    errorDialog.dialog('open');
}

function InputUpdated(){    
    var elem = $(this);
    var initialValue = elem.attr('initialValue');
    var newValue = elem.attr('value');
    var target = elem.attr('target');
    var parameterName = elem.attr('parameterName');
    var parameterId = elem.attr('parameterId');
    var serviceName = elem.attr('serviceName');
    
    if (initialValue != newValue && Validate(elem)){
        var parameter = GetParameter(parameterId, serviceName);
        var coercedValue;
        if (typeof(parameter.CoerceValueFunction) != "undefined"){
            coercedValue = parameter.CoerceValueFunction(newValue);
        }else{
            coercedValue = newValue;
        }
        var forceRefresh = parameter.ForceRefresh;
        var container = containers[serviceName];
        container.Services().Configuration.SetParameter(target, 
                                                        parameterName, 
                                                        coercedValue, 
                                                        function(){
                                                        	elem.attr('initialValue', newValue);
                                                        	Validate(elem);
                                                        	if (forceRefresh) {
                                                        		parameter.Value = coercedValue; Refresh(container, false);
                                                        	}
                                                        	if (parameter.RequiresReboot) {
                                                                Reboot(parameter.ServiceName, parameter.DisplayName);
                                                            }
                                                        }, 
                                                        function(aMessage, aTransport){
                                                        	if (aMessage.indexOf("errorCode: 805") >= 0) {
											                    ShowSetParameterErrorMessage(parameter, aMessage);
											                    elem.toggleClass("Invalid",true);
											                }
											                else {
											                    var modelName = parameter.ProductService.Variables().ModelName.Value();
											                    var deviceName = (modelName == null ? "Device" : modelName);
											                    ShowCommsErrorMessage(deviceName);
											                }
                                                        });
    }
}

function GetElementCurrentValue(aParameter, aErrorInfoOnly) {
	var input;
    switch(aParameter.Type.toUpperCase()){
        case "STRING":
        case "URI":
        case "INTEGER":
        case "POSITIVEINTEGER":
        {
            input = $("#" + aParameter.Id).find("span > input");
            break;     
        }
        case "ENUM":
        {
            input = $("#" + aParameter.Id).find("select");   
            break;
        }
        default: {
        	return "";
        }
    }
    var currentValue = input.attr('initialValue');
    var newCurrentValue = currentValue;
    if (aParameter.AllowedValues) {
    	var found = false;
        $(aParameter.AllowedValues).each(function(aIdx, aValue){
        	if (aParameter.BooleanValues) {
        		newCurrentValue = (currentValue == Linn.Parameter.kValueTrue ? aParameter.BooleanValues.True : aParameter.BooleanValues.False);
        	    found = true;
        	}
        	else if (aIdx == currentValue && aValue.Text != aValue.Value) {
                newCurrentValue = aValue.Text;
                found = true;
            }
            else if (aValue.Value == currentValue) {
            	newCurrentValue = aValue.Text;
            	found = true;
            }
        });
        if (!found && !aParameter.IsComboBox) {
        	newCurrentValue = "";
        }
    }
    if (input.attr('class').indexOf("Invalid") >= 0) {
        return "<span class='HelpTextInvalid'>" + input.attr('value') + "</span> (Invalid)";
    }
    else if (aErrorInfoOnly) {
    	return "<span class='HelpTextInvalid'>" + input.attr('value') + "</span>";
    }
    else {
        return newCurrentValue;    
    } 
}

function UpdateElement(aParameter, aElement){
    var input;
    switch(aParameter.Type.toUpperCase()){
        case "STRING":
        case "URI":
        case "INTEGER":
        case "POSITIVEINTEGER":
        {
            input = aElement.find("span > input");
            if (aParameter.Value != input.attr('initialValue')){
                input.attr('value', aParameter.Value);
                input.attr('initialValue', aParameter.Value);       
            }            
            break;
        }
        case "ENUM":
        {
            input = aElement.find("select");   
            var optionsChanged = false;
            var options = input.find("option");
            optionsChanged = aParameter.AllowedValues.length != options.length;
            if (!optionsChanged){
                for (var i=0;i<aParameter.AllowedValues.length;i++){
                    if ($(options[i]).attr('value') != aParameter.AllowedValues[i].Value
                        || $(options[i]).text() != aParameter.AllowedValues[i].Text){
                            optionsChanged = true;
                            break;
                        }
                }
            }
            
            var newSelect = null;
            if (aParameter.Value != input.attr('initialValue') || optionsChanged){            
                var newSelect = CreateSelect(aParameter);
                input.replaceWith(newSelect);
            }
            
            if (aParameter.IsComboBox) {
            	// string part
            	input = aElement.find("span > input");
	            if (aParameter.Value != input.attr('initialValue')){
	                input.attr('value', aParameter.Value);
	                input.attr('initialValue', aParameter.Value);       
	            }
	            if (newSelect) {
                    if (newSelect.css('display') == 'none') { // if ComboBox, enum and string should never be visible at the same time
                        input.css('display', 'inline');
                    }
                    else {
                    	input.css('display', 'none');
                    }
                }
            }
            break;
        }
        default: {break;}
    }
    Validate(aElement);
}

function GetContainer(aParameter){
    var header = $("#" + aParameter.HeaderId);
    var body = $("#" + aParameter.BodyId);
    if (header.length > 0 && body.length > 0){
        return {header:header, body:body};
    }else{
        return null;
    }
}

function CreateContainer(aParameter){
    var newHeader = $("<li id='" + aParameter.HeaderId + "'><a href='#'>" + aParameter.Grouping + "</a></li>");
    var newBody = $("<div id='" + aParameter.BodyId + "'/>");
    
    var headerEntries = $("#header" + aParameter.ServiceName).children();
    var groupListHeader = [];
    for (var i=0; i<headerEntries.length; i++){
        groupListHeader[i] = headerEntries[i].id;
    }
    groupListHeader[groupListHeader.length] = aParameter.HeaderId;
    groupListHeader.sort();
    
    var bodyEntries = $("#body" + aParameter.ServiceName).children();
    var groupListBody = [];
    for (var i=0; i<bodyEntries.length; i++){
        groupListBody[i] = bodyEntries[i].id;
    }
    groupListBody[groupListBody.length] = aParameter.BodyId;
    groupListBody.sort();
    
    var loc = $.inArray(aParameter.HeaderId, groupListHeader) + 1;
    if ((loc > 0) && (loc < groupListHeader.length)) {
        // not the last entry in the array so place it alphapbetically
        $("#" + groupListHeader[loc]).before(newHeader);
        $("#" + groupListBody[loc]).before(newBody);
    }
    else {
        $("#header" + aParameter.ServiceName).append(newHeader);
        $("#body" + aParameter.ServiceName).append(newBody);
    }
    return {header: newHeader, body:newBody};
}

})(jQuery);