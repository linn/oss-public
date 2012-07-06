
function onPlayButtonClick() {
    xappSend('Play', '');
}

function onStopButtonClick() {
    xappSend('Stop', '');
}

function onVolumeIncButtonClick() {
    xappSend('VolumeInc', '');
}

function onVolumeDecButtonClick() {
    xappSend('VolumeDec', '');
}

function onMainTextStreamStatus(json) {
    $("#MainTextStreamStatus").html(json.Value);
}

function onMainTextVolume(json) {
    $("#MainTextVolume").html(json.Value);
}
