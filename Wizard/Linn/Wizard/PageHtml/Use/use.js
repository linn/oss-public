

function onSummaryItemRender(json)
{
    var item = $('#' + json.Id);
    item.removeClass();
    item.addClass('summary-item');
    item.addClass(json.Value);
}


