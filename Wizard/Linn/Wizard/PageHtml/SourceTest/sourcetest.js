

function onSelectedIconRender(json)
{
    var iconName = json.Value.replace(/ /g, "").replace(/\//g, "");
    var root = $('#' + json.Id);
    root.removeClass();
    root.addClass('sourcetest-item ' + iconName);
}


function onSelectedInputRender(json)
{
    var root = $('#' + json.Id);
    root.removeClass();
    root.addClass('sourcetest-item ' + json.Value);
}


function onButtonClicked(item)
{
    if (item.hasClass('enabled'))
    {
        sendAction(item, '');
    }
}


function onStartClicked(item)
{
    if (item.hasClass('enabled'))
    {
        item.removeClass('enabled');
        $('#SourceTestStopButton').addClass('enabled');
        $('#SourceTestVolumeDecButton').addClass('enabled');
        $('#SourceTestVolumeIncButton').addClass('enabled');

        sendAction(item, '');
    }
}

function onStopClicked(item)
{
    if (item.hasClass('enabled'))
    {
        item.removeClass('enabled');
        $('#SourceTestVolumeDecButton').removeClass('enabled');
        $('#SourceTestVolumeIncButton').removeClass('enabled');
        $('#SourceTestStartButton').addClass('enabled');

        sendAction(item, '');
    }
}


