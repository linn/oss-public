

function linnSetUnselectable(element)
{
    element.attr('unselectable', 'on');

    element.children().each(function(index) {
        linnSetUnselectable($(this));
    });
}


