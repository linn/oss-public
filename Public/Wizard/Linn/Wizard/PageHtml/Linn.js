

// Method used by controls to set the value of some data in the wizard
function xappSetData(dataId, value)
{
    xappSend('setData' + dataId, value);
}


// Method for rendering static text - used in <Widget xappEvent="StaticTextRender" ...> to tie
// some model data to a given html element
function onStaticTextRender(json)
{
    $('#' + json.Id).html(json.Value);
}


function linkClicked(url) {
    xappSend("LinkClicked", url);
}

function onNetworkChangeExit(aAlertString)
{
    alert("Network change detected! Please restart the application.");
    xappSend('CloseApplication', "");
}

function action(type) {
    xappSend(type, "");
}

function sendAction(control, aValue) {
    actionName = "action" + control.attr("id");
    xappSend(actionName, aValue);
}

function genericClick(control) {
    xappSend(control.id, "");
}

function returnToPage(page) {
    xappSend('ReturnToPage', page);
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
                if ((json.HeightSet != undefined) && (json.Height != undefined)) {
                    if (json.HeightSet)
                        n.style.height = json.Height;
                }
                if ((json.WidthSet != undefined) && (json.Width != undefined)) {
                    if (json.WidthSet)
                        n.style.width = json.Width;
                }
                break;

        } //switch

    } // if(n)

}

