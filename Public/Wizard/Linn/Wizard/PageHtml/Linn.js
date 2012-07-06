$(document).ready(loadcustom);

// customizable custom page...
var color_hover = "#EB8784";
var color_active = "rgb(223, 80, 92)";
var color_inactive = "rgb(198, 191, 183)";
//var productimage = "";
function hoveron() {
    $(this).css("background-color", color_hover);
    $(this).find(".customSourceNameEdit").css("background-color", "white");
}

function hoveroff() {
    $(this).css("background-color", color_inactive);
    $(this).find(".customSourceNameEdit").css("background-color", "white");
}

function linkClicked(url) {
    xappSend("LinkClicked", url);
}

function showsourceimage() {
   // alert($(this).find(".customSourceImage").attr("src"));
    $("#CustomProductImage").attr("src", $(this).find(".customSourceImage").attr("src")); // use image stored in concealed element
    $("#CustomProductImage").css("visibility", "visible");
}

function showproductimage() {
   // alert(productimage);
    $("#CustomProductImage").attr("src", $(document).find("#CustomProductImageDefault").attr("src"));
    $("#CustomProductImage").css("visibility", "visible");
}


function onNetworkChangeExit(aAlertString)
{
    alert("Network change detected! Please restart the application.");
    xappSend('CloseApplication', "");
}


function highlighton() {
    if ($(this).hasClass("customSourceEntry")) {
        $(this).off("mouseenter");
        $(this).off("mouseleave");
        $(this).on("mouseenter", showsourceimage);
        $(this).on("mouseleave", showproductimage);
        $(this).css("background-color", color_active);
        $(this).find(".customSourceNameEdit").css("background-color", "white");
        $(this).find(".customSourceNameEdit").css("visibility", "visible");
 //       $(this).find(".customSourceIconEdit").css("visibility", "visible");
      //  $(this).children('.panel-icons').each(function () {
     //       $(this).children(".panel-icons").css("visibility", "hidden"); // if focus going back to div of same source entry need to clear icon selection
     //   });
    } 
}

function highlightoff() {
    if ($(this).hasClass("customSourceEntry")) {
        $(this).on("mouseenter", hoveron);
        $(this).on("mouseleave", hoveroff);

        $(this).css("background-color", color_inactive);
        $(this).find(".customSourceNameEdit").css("border", "none");
        $(this).find(".customSourceNameEdit").css("background-color", "white");
        $(this).children(".customSourceIconEdit").css("visibility", "hidden");      // only clear if it is a child of 'this' - ie it is the parent object losing focus

   //     $(this).children('.panel-icons').each(function () {
   //      alert($(this).children(".panel-icons").css("background-color"));
   //            $(this).children(".panel-icons").css("visibility", "hidden");      // only clear if it is a child of 'this' - ie it is the parent object losing focus
   //     });
    }

 //   if ($(this).hasClass("panel-icons")) {
  //      $(this).css("visibility", "hidden"); 
 //   }
}

var currentSource = "";
function selectSourceEntry() {
    highlighton();      // do 'focus' highlight

    if ($(this).hasClass("customSourceEntry")) {
        currentSource = $(this).attr("id");
    }
}

function loadcustom() {

    $(document).find(".customSourceEntry").each(function () {
        $(this).on("mouseenter", hoveron);
        $(this).on("mouseenter", showsourceimage);
        $(this).on("mouseleave", hoveroff);
        $(this).on("mouseleave", showproductimage);

        $(this).focusin(highlighton);
        //$(this).focus(highlighton);
        $(this).click(selectSourceEntry); //safari doesn't give focus to images

        $(this).focusout(highlightoff);
       // $(this).blur(highlightoff);

    });
}



var config = {
    icons: [
		{ img: 'iconcable', alt: 'Cable box' },
		{ img: 'iconcomputer', alt: 'Computer' },
		{ img: 'iconcontroller', alt: 'Controller' },
		{ img: 'icondisc', alt: 'Disc' },
		{ img: 'iconipad', alt: 'iPad' },
		{ img: 'iconiphone', alt: 'iPhone' },
		{ img: 'iconipod', alt: 'iPod' },
		{ img: 'iconmoviesfilm', alt: 'Films' },
		{ img: 'iconmp3', alt: 'Mp3' },
		{ img: 'iconplaystation', alt: 'Playstation' },
		{ img: 'iconsatellite', alt: 'Satellite' },
		{ img: 'iconturntable', alt: 'Turntable' },
		{ img: 'icontv', alt: 'TV' },
		{ img: 'iconwii', alt: 'Wii' },
		{ img: 'iconxbox', alt: 'Xbox' }
	]
}
/*
function setIcon(control, index, img) {
    alert("icon " + index + img);

    $(document).find('#' + control.id).siblings(".panel-icons").css("background-img", img);
}
*/
function setIcon(control, index) {
    //alert("icon " + index + control.src + "; " + control.id);

    $(document).find('.customSourceEntry').each(function () {
    });
//    $(document).find('#' + control.id).parent(".customSourceEntry").css("background-image", "url(Resources/Images/SourceIcons/" + config.icons[index].img + ".png");
}

function customIconSelect(control) {

 //   $(document).find('#CustomSource1').css("background-color", "blue");

    // insert icon panel
    var icon_list = document.createElement("ul")

    for (var ii = 0 in config.icons) {
        var img = '../Resources/Images/SourceIcons/' + config.icons[ii].img + '.png';
        
        var icon = '<li><span><img id="IconSelectElement' + ii + '"' + ' src="' + img + '"' + ' alt="' + config.icons[ii].alt + '"' + ' onclick="setIcon(this' + ',' + ii + ');" /></span></li>';
 //       var icon = '<li><span><img src="' + img + '"' + ' alt="' + config.icons[ii].alt + '"' + ' onclick="setIcon(this, ' + ii + ',' + img + ');" /></span></li>';
        //       var icon = '<li><span><img src="' + img + '"' + ' alt="' + config.icons[ii].alt + '"' + ' onclick="setIcon(this, ' + ii + ');" /></span></li>';
        //alert(icon);
        $(icon_list).append(icon);
    }

    $(document).find('#' + control.id).siblings(".panel-icons").empty();
    $(document).find('#' + control.id).siblings(".panel-icons").append(icon_list);

    //e.stopPropagation(); 
    //$(document).find('#' + control.id).css("background-color", color_inactive);
    $(document).find('#' + control.id).siblings(".panel-icons").css("visibility", "visible");			
}


function getTime() {
    var dTime = new Date();
    return (dTime);
}


function action(type) {
    xappSend(type, "");
}

function genericClick(control) {
    xappSend(control.id, "");
}

function textEntryLoad(control) {
    alert("onload");            // we don't get this - is it lost in main framework?    ToDo!!!
}

function textEntryClick(control) {
    if ($("#" + control.id).html() == "Main room") {
        $("#" + control.id).html("");
    }
    document.getElementById(control.id).style.color = "black";
}

function sourceEntryUpdate(control) {
    if (document.getElementById(currentSource) == undefined) {
//        alert("no currentSource" + currentSource);
    } else if (document.getElementById(control.id) == undefined) {
 //       alert("no control.id" + control.id);
    }
    else {
        var param = document.getElementById(control.id).className + "," + $('#' + control.id).val() + "," + document.getElementById(currentSource).className;    // use comma delimited list of params (no json decode in c# libs)
        xappSend("UpdateDeviceParameter", param);
    }
}

function sourceEntryKey(control, e) {
    var keypress = (e.keyCode ? e.keyCode : e.which);
    if (keypress == 13) {
        return false; //ignore enter key
    }

    if ($('#' + control.id).val().length >= 20) {
        return false; //limit length of name
    }
    return true; // use key
}



function escapeSpecials(s) {
    var escaped = "";

    for (i in s) {
        var c = s.charAt(i);
        if ((c <= ' ') || (c == '&') || (c == '=') || (c == '_') || (c == '%') || (c == 'x') || (c == '+')) {
            // convert character to representation as hex string
            var code = c.charCodeAt(0);
            var hex = Number(code).toString(16);
            while (hex.length < 2) {
                hex = "0" + hex;
            }
            escaped += "x" + hex;   // cannot use %
        }
        else {
            escaped += c;
        }
    }
    
    return escaped;
}

function textEntryUpdate(control) {
   var param = document.getElementById(control.id).className + "," + $('#' + control.id).val(); // use comma delimited list of params (no json decode in c# libs)    var param = document.getElementById(control.id).className + "," + $("#" + control.id).html();    // use comma delimited list of params (no json decode in c# libs)
    xappSend("UpdateDeviceParameter", param);
}

function textEntryKey(control, e) {
    var keypress = (e.keyCode ? e.keyCode : e.which);
    if (keypress == 13) {
        return false; //ignore key
    }

    if ($('#' + control.id).val().length >= 20) {
        return false; //limit length of name
    }


    return true; // use key
}

function gotoTag(tag) {
    xappSend('GotoTag', tag);
}

function SelectProductModel(aTag) {
    xappSend('SelectProductModel', aTag);
    xappSend('GotoTag', aTag);
}


function gotoPage(page) {
    xappSend('GotoPage', page);
}

function onHide(json) {
    var elem = document.getElementById(json.Value);
    if (elem) {
        elem.style.visibility = "hidden";
    }
}

function onUnhide(json) {
    var elem = document.getElementById(json.Value);
    if (elem) {
        elem.style.visibility = "visible";
    }
}

function onDisable(json) {
    var elem = document.getElementById(json.Value);
    if (elem) {
        elem.disabled = true;
    }
}


function onEnable(json) {
    var elem = document.getElementById(json.Value);
    if (elem) {
        elem.disabled = false;
    }
}

function onEnableBreadcrumb() {
    $(".progress_step").height("inherit");      // this re-applies the li item width after the text has been added (needed for safari)
}

function onRender(json) {
    var id = json.Id;
    var n = document.getElementById(id);
    var element = $(document).find(id);

    if (n) {
        if (json.Enabled != undefined) {
            n.disabled = !json.Enabled;
        }
        if (json.Top != undefined) {
            n.style.top = json.Top;
        }
        if (json.Left != undefined) {
            n.style.left = json.Left;
        }

        switch (json.Type) {
            case "Text":
                if (json.Text != undefined) {
                    if (json.Text == null) {
                        alert("no text");
                    }
                    else {
                        $("#" + id).html(json.Text);
                    }
                }

                if (json.Visible != undefined) {
                    n.visible = !json.Visible;
                    if (!json.Visible)
                        n.style.visibility = "hidden";
                    else
                        n.style.visibility = "visible";
                }

                if (json.Class != undefined) {
                    n.className += " Service_" + json.Type + "_" + json.Class;
                }
                break;

            case "Image":
                if (json.Image != undefined) {
                    n.src = "Resources/" + json.Image;
                }
                
                if (json.Visible != undefined) {
                    if (!json.Visible)
                        n.style.visibility = "hidden";
                    else 
                        n.style.visibility = "visible";
                }
                
                if ((json.HeightSet != undefined) && (json.Height != undefined)) {
                    if (json.HeightSet)
                        n.height = json.Height;
                }
                if (json.Class != undefined) {
                    n.className += " Service_" + json.Type + "_" + json.Class;
                }
                if ((json.BackgroundImage != undefined) && (json.BackgroundImage != "")) {
                    n.style.backgroundImage = "url(Resources/" + json.BackgroundImage + ")";
                }
                break;

            case "Control":
                if (json.Text != undefined) {
                    if (json.Text == null) {
                        alert("no control text");
                    }
                    n.value = json.Text;

                    $("#" + id).html(json.Text);
                }
                if (json.Visible != undefined) {
                    if (!json.Visible)
                        n.style.visibility = "hidden";
                    else
                        n.style.visibility = "visible";
                }
                if (json.Displayed != undefined) {
                    if (!json.Displayed) {
                        n.style.display = "none";
                    }
                }
                if (json.Image != undefined) {
                    n.src = "Resources/" + json.Image;
                }
                if (json.Color != undefined) {
                    n.style.color = json.Color;
                }
                if (json.BackgroundColor != undefined) {
                    n.style.backgroundColor = json.BackgroundColor;
                }
                if ((json.BackgroundImage != undefined) && (json.BackgroundImage != "")) {
                    n.style.backgroundImage = "url(Resources/" + json.BackgroundImage + ")";
                }
                if (json.Class != undefined) {
                    n.className += " Service_" + json.Type + "_" + json.Class;
                }
                break;

            case "Special":
                xappSend("Special", json.Id);
                break;

        } //switch

    } // if(n)

}

