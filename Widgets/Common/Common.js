

function linnSetUnselectable(element)
{
    if(!element.is("input")) {
        element.attr('unselectable', 'on');
    }

    element.children().each(function(index) {
        linnSetUnselectable($(this));
    });
}


