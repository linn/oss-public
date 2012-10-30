

function allowedValueToClass(allowedValue)
{
    return allowedValue.replace(/ /g, "").replace(/\//g, "");
}


function selectionClick(item)
{
    // the selection item click area is passed in
    var clickedItem = item.parent();

    // change the selection in the selector widget
    $('.selection-item').removeClass('selected');
    clickedItem.addClass('selected');

    // set the data associated with this item
    xappSetData(clickedItem.data('dataId'), clickedItem.data('value'));

    // update the visibility of the 'next' button
    $('#NextButton').css('visibility', 'visible');

    // update the class for the selector wrapper
    var selectorWrapper = $('#SelectorWrapper');
    selectorWrapper.removeClass();
    selectorWrapper.addClass(allowedValueToClass(clickedItem.data('value')));
}


function selectionMouseOver(item)
{
    // change the hover in the selector widget
    $('.selection-item').removeClass('hover');
    item.parent().addClass('hover');
}


function selectionMouseOut(item)
{
    item.parent().removeClass('hover');
}


function onSelectorRender(json)
{
    // generate the html for the widget
    var html = "";

    for (var i=0 ; i<json.AllowedValues.length ; i++)
    {
        var allowedVal = allowedValueToClass(json.AllowedValues[i]);

        html += '<div class="selection-item ';
        html += allowedVal;
        html += '" id="SelectionArea';
        html += i;
        html += '">';
        html += '<div class="selection-item-clickarea" onclick="selectionClick($(this));" onmouseover="selectionMouseOver($(this));" onmouseout="selectionMouseOut($(this));"/>'
        html += '<div class="selection-item-overlay"/>'
        html += '</div>';
    }

    $('#' + json.Id).html(html);

    // add the custom data to each dom node for the selectable items
    var currentItem = null;

    for (var i=0 ; i<json.AllowedValues.length ; i++)
    {
        var item = $('#SelectionArea' + i);
        item.data('dataId', json.DataId);
        item.data('value', json.AllowedValues[i]);

        if (json.AllowedValues[i] == json.Value)
        {
            currentItem = item;
        }
    }

    // clear the current selection and apply the new one
    $('.selection-item').removeClass('selected');

    if (currentItem != null)
    {
        currentItem.addClass('selected');
    }

    // now set the state of the 'next' button
    $('#NextButton').css('visibility', (currentItem != null ? 'visible' : 'hidden'));

    // update the selector wrapper class
    var selectorWrapper = $('#SelectorWrapper')
    selectorWrapper.removeClass();
    if (currentItem != null)
    {
        selectorWrapper.addClass(allowedValueToClass(currentItem.data('value')));
    }
}


