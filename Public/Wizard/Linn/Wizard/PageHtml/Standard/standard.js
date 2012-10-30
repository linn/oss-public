

// Method for rendering user-changable text - used in <Widget xappEvent="TextAreaRender" ...> to
// tie model data to an input textarea
function onTextAreaRender(json)
{
    var control = $('#' + json.Id);
    control.html(json.Value);
    control.data('dataId', json.DataId);

    textAreaChanged(control);
}

// Method called when a textarea is clicked
function textAreaClick(control)
{
//    if ($("#" + control.id).html() == "Main room") {
//        $("#" + control.id).html("");
//    }

    control.css('color', 'black');
}

// Method called when a textarea loses focus - set the associated data
function textAreaUpdate(control)
{
    xappSetData(control.data('dataId'), control.val());
}

// Method to limit the text input
function textAreaKey(control, e)
{
    var keypress = (e.keyCode ? e.keyCode : e.which);
    if (keypress == 13)
    {
        return false; //ignore key
    }

    if (control.val().length >= 20)
    {
        return false; //limit length of name
    }

    return true; // use key
}

// Method called for the dialog box onchange and onkeyup events
function textAreaChanged(control)
{
    $('#NextButton').css('visibility', (control.val().length != 0 ? 'visible' : 'hidden'));
}



