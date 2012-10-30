// function to initialise all instance of elements with the class "linn-toggle-button"
// as toggle buttons
function linnToggleButtonInit()
{
        
    $(".linn-toggle-button").each(function (index)
    {
        // replace the specified 'onclick' handler 
        var id = this['id'];
        var button = $('#' + id);

        // store the onclick handler specified in the HTML to stored as a
        // handler for a separate event
        button.on('onClickDelegate', this['onclick']);

        // constuct the real "onclick" handler - this changes the button state
        // and then calls the external handler
        button.removeAttr('onclick');
        button.click(function(){
            linnToggleButtonSetValue($(this), !linnToggleButtonValue($(this)));
            $(this).trigger('onClickDelegate');
        });
        
        
        var onLabel = 'ON', offLabel='OFF';
        if (typeof button.data('on-label') != "undefined"){
            onLabel = button.data('on-label');
        }
        if (typeof button.data('off-label') != "undefined"){
            offLabel = button.data('off-label');
        }
        
        // add the additional HTML for the button
        button.append('<div class="linn-toggle-button-label on-label">' + onLabel + '</div>'
                    + '<div class="linn-toggle-button-label off-label">' + offLabel + '</div>');
    });
};

// function to get the state of a toggle button
function linnToggleButtonValue(button)
{
    return button.hasClass('on');
};

// function to set the state of a toggle button
function linnToggleButtonSetValue(button, value)
{
    if (value) {
        button.addClass('on');
    }
    else {
        button.removeClass('on');
    }
};
