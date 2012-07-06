function onUpdateBoxTitle(json) {
    $("#UpdateBoxTitle").html(json.Value);
}

function onUpdateBoxText(json) {
    $("#UpdateBoxText").html(json.Value);
}

function onUpdateProgressText(json) {
    $("#UpdateProgressText").html(json.Value);
}

function onUpdateProgress(json) {
    document.getElementById(json.Id).style.width = json.Value;
}