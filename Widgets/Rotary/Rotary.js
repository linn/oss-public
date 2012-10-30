

// function to initialise all rotary controls with class "linn-rotary"
function linnRotaryInit()
{
    $(".linn-rotary").each(function (index)
    {
        var id = this['id'];
        var rotary = $('#' + id);

        // add the additional HTML for the control
        rotary.append('<div class="centre"/>'
                    + '<div class="wheel">'
                    +   '<div class="grip"/>'
                    +   '<div class="progressbar">'
                    +     '<div class="l-bar"/>'
                    +     '<div class="r-bar"/>'
                    +   '</div>'
                    +   '<div class="filler1"/>'
                    +   '<div class="filler2"/>'
                    +   '<div class="adornment"/>'
                    + '</div>');

        // add data specific to this instance
        rotary.data('knobAngle', 0);

        rotary.on('mousedown', function(e)
        {
            var root = $('#' + this['id']);
            if (!root.hasClass('disabled'))
            {
                currentData = new linnRotaryData(root);
                currentData.Start(e);
            }
        });
    });

    $(document).mousemove(function(e)
    {
        if (currentData != null)
        {
            currentData.MouseMove(e);
        }
    });

    $(document).mouseup(function(e)
    {
        if (currentData != null)
        {
            currentData.MouseUp(e);
            currentData = null;
        }
    });
}

// functions to set event handlers
function linnRotarySetStartRotation(rotary, func)
{
    rotary.on('startRotation', func);
}
function linnRotarySetCancelRotation(rotary, func)
{
    rotary.on('cancelRotation', func);
}
function linnRotarySetEndRotation(rotary, func)
{
    rotary.on('endRotation', func);
}
function linnRotarySetClockwiseRotate(rotary, func)
{
    rotary.on('clockwiseRotate', func);
}
function linnRotarySetAnticlockwiseRotate(rotary, func)
{
    rotary.on('anticlockwiseRotate', func);
}
function linnRotarySetCentreClicked(rotary, func)
{
    rotary.on('centreClicked', func);
}

// function to enable/disable the rotary control
function linnRotarySetEnabled(rotary, enabled)
{
    if (enabled)
    {
        rotary.removeClass('disabled');
    }
    else
    {
        rotary.addClass('disabled');
    }
}

// function to get the visible state of adornment
function linnRotaryIsAdornmentVisible(rotary)
{
    var adornment = $('#' + rotary.attr('id') + ' div.adornment');
    var visibility = adornment.css('visibility');
    return (visibility == 'visible');
}

// function to set the visible state of adornment
function linnRotarySetAdornmentVisible(rotary, value)
{
    var adornment = $('#' + rotary.attr('id') + ' div.adornment');
    adornment.css({ 'visibility': (value ? 'visible' : 'hidden') });
}

// function to set the value of the progress bar
function linnRotarySetProgress(rotary, value, maxValue)
{
    var progressbar = $('#' + rotary.attr('id') + ' div.progressbar');
    var lbar = $('#' + rotary.attr('id') + ' div.progressbar div.l-bar');
    var rbar = $('#' + rotary.attr('id') + ' div.progressbar div.r-bar');

    var size = progressbar.height();
    var halfsize = (size / 2) + 'px';

    // if we are not IE 7/8 we use images instead so no clipping is needed
    if (typeof String.prototype.trim == 'function') {
        lbar.css({ 'clip': 'rect(0px, ' + halfsize + ', ' + size + 'px, 0px)' });
        rbar.css({ 'clip': 'rect(0px, ' + size + 'px, ' + size + 'px, ' + halfsize + ')' });
    }

    var ratio = value / maxValue;
    var v = ratio * 360;

    if (v >= 0 && v <= 180)
    {
        var a = -1 * (180 - v);
        progressbar.css({ 'clip': 'rect(0px, ' + halfsize + ', ' + size + 'px, 0px)' });
        lbar.css({ '-webkit-transform': 'rotate(' + a + 'deg)' });
        lbar.css({ '-moz-transform': 'rotate(' + a + 'deg)' });
        lbar.css({ '-o-transform': 'rotate(' + a + 'deg)' });
        lbar.css({ '-ms-transform': 'rotate(' + a + 'deg)' });
        lbar.css({ 'transform': 'rotate(' + a + 'deg)' });
        rbar.css({ 'display': 'none' });

        // if we are IE 7/8
        if (typeof String.prototype.trim != 'function') {
            progressbar.css({ 'clip': 'rect(0px ' + halfsize + ' ' + size + 'px 0px)' });

            var r = a * (Math.PI / 180);
            var costheta = Math.cos(r);
            var sintheta = Math.sin(r);
            lbar.css({ 'filter': 'progid:DXImageTransform.Microsoft.Matrix(M11=' + costheta + ', M12=' + -sintheta + ', M21=' + sintheta + ', M22=' + costheta + ', sizingMethod=\'auto expand\')' });
            lbar.css({ '-ms-filter': 'progid:DXImageTransform.Microsoft.Matrix(M11=' + costheta + ', M12=' + -sintheta + ', M21=' + sintheta + ', M22=' + costheta + ', sizingMethod=\'auto expand\')' });
            var top = (size - lbar.height()) / 2;
            var left = (size - lbar.width()) / 2;
            lbar.css({ 'top': top + 'px' });
            lbar.css({ 'left': left + 'px' });
        }
    }
    else if(v > 180 && v <= 360)
    {
        progressbar.css({ 'clip': 'rect(0px, ' + size + 'px, ' + size + 'px, 0px)' });
        lbar.css({ '-webkit-transform': 'rotate(0deg)' });
        lbar.css({ '-moz-transform': 'rotate(0deg)' });
        lbar.css({ '-o-transform': 'rotate(0deg)' });
        lbar.css({ '-ms-transform': 'rotate(0deg)' });
        lbar.css({ 'transform': 'rotate(0deg)' });
        rbar.css({ '-webkit-transform': 'rotate(' + v + 'deg)' });
        rbar.css({ '-moz-transform': 'rotate(' + v + 'deg)' });
        rbar.css({ '-o-transform': 'rotate(' + v + 'deg)' });
        rbar.css({ '-ms-transform': 'rotate(' + v + 'deg)' });
        rbar.css({ 'transform': 'rotate(' + v + 'deg)' });
        rbar.css({ 'display': 'block' });

        // if we are IE 7/8
        if (typeof String.prototype.trim != 'function') {
            progressbar.css({ 'clip': 'rect(0px ' + size + 'px ' + size + 'px 0px)' });

            lbar.css({ 'filter': 'progid:DXImageTransform.Microsoft.Matrix(M11=1, M12=0, M21=0, M22=1, sizingMethod=\'auto expand\')' });
            lbar.css({ '-ms-filter': 'progid:DXImageTransform.Microsoft.Matrix(M11=1, M12=0, M21=0, M22=1, sizingMethod=\'auto expand\')' });
            var top = (size - lbar.height()) / 2;
            var left = (size - lbar.width()) / 2;
            lbar.css({ 'top': top + 'px' });
            lbar.css({ 'left': left + 'px' });

            var r = v * (Math.PI / 180);
            var costheta = Math.cos(r);
            var sintheta = Math.sin(r);
            rbar.css({ 'filter': 'progid:DXImageTransform.Microsoft.Matrix(M11=' + costheta + ', M12=' + -sintheta + ', M21=' + sintheta + ', M22=' + costheta + ', sizingMethod=\'auto expand\')' });
            rbar.css({ '-ms-filter': 'progid:DXImageTransform.Microsoft.Matrix(M11=' + costheta + ', M12=' + -sintheta + ', M21=' + sintheta + ', M22=' + costheta + ', sizingMethod=\'auto expand\')' });
            top = (size - rbar.height()) / 2;
            left = (size - rbar.width()) / 2;
            rbar.css({ 'top': top + 'px' });
            rbar.css({ 'left': left + 'px' });
        }
    }
}


// the instance of linnRotaryData for the current rotary interaction
var currentData = null;

// class definition for the data used in the rotary control
function linnRotaryData(target)
{
    this.target = target;

    // define some metrics specific to this instance
    this.width = this.target.width();
    this.height = this.target.height();
    this.centreX = this.target.offset().left + (this.target.width() / 2);
    this.centreY = this.target.offset().top + (this.target.height() / 2);
    this.outerRadius2 = this.centreX - this.target.offset().left;
    this.innerRadius2 = this.outerRadius2 * 0.3;
    this.outerRadius2 = this.outerRadius2 * this.outerRadius2;
    this.innerRadius2 = this.innerRadius2 * this.innerRadius2;

    // initialise other data related to this rotation
    this.touchedInner = false;
    this.touchedOuter = false;
    this.startAngle = 0;
    this.lastAngle = 0;

    // remove classes for various states
    this.target.removeClass('inner-down outer-down');
}

// function to start the rotation interaction
linnRotaryData.prototype.Start = function(e)
{
    var dx = e.pageX - this.centreX;
    var dy = e.pageY - this.centreY;
    var r2 = dx*dx + dy*dy;

    if (r2 > this.innerRadius2)
    {
        if(r2 < this.outerRadius2)
        {
            this.touchedOuter = true;
            this.target.trigger('startRotation');
            this.target.addClass('outer-down');
        }
    }
    else
    {
        this.touchedInner = true;
        this.target.addClass('inner-down');
    }

    this.startAngle = this.CalculateMouseAngle(e);
    this.lastAngle = this.startAngle;
}

// function to handle mouse movement during the rotation interaction
linnRotaryData.prototype.MouseMove = function (e) {
    if (this.touchedOuter) {
        // calculate the new mouse angle and send increment notifications if needed
        var angle = this.CalculateMouseAngle(e);

        var angleDiff = angle - this.lastAngle;
        while (angleDiff > 180.0) {
            angleDiff = angleDiff - 360.0;
        }
        while (angleDiff < -180.0) {
            angleDiff = angleDiff + 360.0;
        }

        var lastSegment = Math.floor((this.lastAngle - this.startAngle) / 30.0);
        var thisSegment = Math.floor((this.lastAngle + angleDiff - this.startAngle) / 30.0);

        if (thisSegment > lastSegment) {
            this.target.trigger('clockwiseRotate');
        }
        else if (thisSegment < lastSegment) {
            this.target.trigger('anticlockwiseRotate');
        }

        this.lastAngle = angle;

        // update the angle of the grip
        var currKnobAngle = this.target.data('knobAngle') + angle - this.startAngle;

        var grip = $('#' + this.target.attr('id') + ' div.grip');
        grip.css({ '-webkit-transform': 'rotate(' + currKnobAngle + 'deg)' });
        grip.css({ '-moz-transform': 'rotate(' + currKnobAngle + 'deg)' });
        grip.css({ '-o-transform': 'rotate(' + currKnobAngle + 'deg)' });
        grip.css({ '-ms-transform': 'rotate(' + currKnobAngle + 'deg)' });
        grip.css({ 'transform': 'rotate(' + currKnobAngle + 'deg)' });

        // if we are IE 7/8
        if (typeof String.prototype.trim != 'function') {
            var r = currKnobAngle * (Math.PI / 180);
            var costheta = Math.cos(r);
            var sintheta = Math.sin(r);
            grip.css({ 'filter': 'progid:DXImageTransform.Microsoft.Matrix(M11=' + costheta + ', M12=' + -sintheta + ', M21=' + sintheta + ', M22=' + costheta + ', sizingMethod=\'auto expand\')' });
            grip.css({ '-ms-filter': 'progid:DXImageTransform.Microsoft.Matrix(M11=' + costheta + ', M12=' + -sintheta + ', M21=' + sintheta + ', M22=' + costheta + ', sizingMethod=\'auto expand\')' });
            var top = (this.height - grip.height()) / 2;
            var left = (this.width - grip.width()) / 2;
            grip.css({ 'top': top + 'px' });
            grip.css({ 'left': left + 'px' });
        }
    }
}

// function to handle mouse up during the rotation interaction
linnRotaryData.prototype.MouseUp = function(e)
{
    var dx = e.pageX - this.centreX;
    var dy = e.pageY - this.centreY;
    var r2 = dx*dx + dy*dy;

    if (this.touchedOuter)
    {
        if (r2 > this.outerRadius2)
        {
            this.target.trigger('cancelRotation');
        }
        else
        {
            this.target.trigger('endRotation');
        }
        
        // update the resulting grip angle
        var angle = this.CalculateMouseAngle(e);
        this.target.data('knobAngle', this.target.data('knobAngle') + angle - this.startAngle);
    }
    else if (this.touchedInner)
    {
        if (r2 < this.innerRadius2)
        {
            this.target.trigger('centreClicked');
        }
    }

    // clear classes
    this.target.removeClass('inner-down outer-down');
}

// function to calculate the mouse angle to the centre of the control
linnRotaryData.prototype.CalculateMouseAngle = function(e)
{
    var radians = Math.atan2(e.pageY - this.centreY, e.pageX - this.centreX);
    return (radians * 180 / Math.PI);
}



