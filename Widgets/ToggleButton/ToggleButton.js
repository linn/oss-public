
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
        button.attr('onclick', "linnToggleButtonSetValue($(this), !linnToggleButtonValue($(this)));"
                             + "$(this).trigger('onClickDelegate');");

        // add the additional HTML for the button
        button.append('<div class="linn-toggle-button-label on-label">ON</div>'
                    + '<div class="linn-toggle-button-label off-label">OFF</div>');

        // apply some styling that is dynamically determined
        button.css('line-height', button.css('height'));
        button.css('font-size', ((parseInt(button.css('height'), 10) * 2) / 3) + 'px');
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
