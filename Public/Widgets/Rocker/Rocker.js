

// function to initialise all rocker controls with class "linn-rocker"
function linnRockerInit()
{
    $(".linn-rocker").each(function (index)
    {
        var id = this['id'];
        var rocker = $('#' + id);

        // add the additional HTML for the control
        rocker.append('<div class="centre"/>'
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
        rocker.data('knobAngle', 0);

        rocker.on('mousedown', function(e)
        {
            root = $('#' + this['id']);
            if (!root.hasClass('disabled'))
            {
                currentData = new linnRockerData(root);
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
function linnRockerSetStartRotation(rocker, func)
{
    rocker.on('startRotation', func);
}
function linnRockerSetCancelRotation(rocker, func)
{
    rocker.on('cancelRotation', func);
}
function linnRockerSetEndRotation(rocker, func)
{
    rocker.on('endRotation', func);
}
function linnRockerSetClockwiseRotate(rocker, func)
{
    rocker.on('clockwiseRotate', func);
}
function linnRockerSetAnticlockwiseRotate(rocker, func)
{
    rocker.on('anticlockwiseRotate', func);
}
function linnRockerSetCentreClicked(rocker, func)
{
    rocker.on('centreClicked', func);
}

// function to enable/disable the rocker control
function linnRockerSetEnabled(rocker, enabled)
{
    if (enabled)
    {
        rocker.removeClass('disabled');
    }
    else
    {
        rocker.addClass('disabled');
    }
}

// function to get the visible state of adornment
function linnRockerIsAdornmentVisible(rocker)
{
    var adornment = $('#' + rocker.attr('id') + ' div.adornment');
    var visibility = adornment.css('visibility');
    return (visibility == 'visible');
}

// function to set the visible state of adornment
function linnRockerSetAdornmentVisible(rocker, value)
{
    var adornment = $('#' + rocker.attr('id') + ' div.adornment');
    adornment.css({ 'visibility': (value ? 'visible' : 'hidden') });
}

// function to set the value of the progress bar
function linnRockerSetProgress(rocker, value, maxValue)
{
    var progressbar = $('#' + rocker.attr('id') + ' div.progressbar');
    var lbar = $('#' + rocker.attr('id') + ' div.progressbar div.l-bar');
    var rbar = $('#' + rocker.attr('id') + ' div.progressbar div.r-bar');

    var size = progressbar.css('height');
    var halfsize = (parseInt(size, 10) / 2) + 'px';

    lbar.css({ 'clip': 'rect(0px, ' + halfsize + ', ' + size + ', 0px)' });
    rbar.css({ 'clip': 'rect(0px, ' + size + ', ' + size + ', ' + halfsize + ')' });

    var ratio = value / maxValue;
    var v = ratio * 360;

    if (v >= 0 && v <= 180)
    {
        var a = -1 * (180 - v);
        progressbar.css({ 'clip': 'rect(0px, ' + halfsize + ', ' + size + ', 0px)' });
        lbar.css({ '-webkit-transform': 'rotate(' + a + 'deg)' });
        lbar.css({ '-moz-transform': 'rotate(' + a + 'deg)' });
        lbar.css({ '-o-transform': 'rotate(' + a + 'deg)' });
        lbar.css({ '-ms-transform': 'rotate(' + a + 'deg)' });
        lbar.css({ 'transform': 'rotate(' + a + 'deg)' });
        rbar.css({ 'display': 'none'});
    }
    else if(v > 180 && v <= 360)
    {
        progressbar.css({ 'clip': 'rect(0px, ' + size + ', ' + size + ', 0px)' });
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
    }
}


// the instance of linnRockerData for the current rocker interaction
var currentData = null;

// class definition for the data used in the rocker control
function linnRockerData(target)
{
    this.kAutoRepeatStartInterval = 0.3;
    this.kAutoRepeatInterval = 0.1;
    
    this.target = target;
    
    // define some metrics specific to this instance
    this.centreX = this.target.offset().left + (this.target.width() / 2);
    this.centreY = this.target.offset().top + (this.target.height() / 2);
    this.outerRadius2 = this.centreX - this.target.offset().left;
    this.innerRadius2 = this.outerRadius2 * 0.3;
    this.outerRadius2 = this.outerRadius2 * this.outerRadius2;
    this.innerRadius2 = this.innerRadius2 * this.innerRadius2;

    // initialise other data related to this rotation
    this.touchedInner = false;
    this.touchedOuterLeft = false;
    this.touchedOuterRight = false;

    // remove classes for various states
    this.target.removeClass('left-down right-down inner-down');
}

// function to start the rotation interaction
linnRockerData.prototype.Start = function(e)
{
    var dx = e.pageX - this.centreX;
    var dy = e.pageY - this.centreY;
    var r2 = dx*dx + dy*dy;

    if (r2 > this.innerRadius2)
    {
        if(r2 < this.outerRadius2)
        {
            if(dx < 0)
            {
                this.touchedOuterLeft = true;
                this.target.addClass('left-down');
            }
            else
            {
                this.touchedOuterRight = true;
                this.target.addClass('right-down');
            }
            this.target.trigger('startRotation');
            
            this.DoStep();
            
            this.timer = setTimeout(DoAutoRepeat, this.kAutoRepeatStartInterval * 1000);
        }
    }
    else
    {
        this.touchedInner = true;
        this.target.addClass('inner-down');
    }
}

// function to handle mouse movement during the rotation interaction
linnRockerData.prototype.MouseMove = function(e)
{
    var dx = e.pageX - this.centreX;
    var dy = e.pageY - this.centreY;
    var r2 = dx*dx + dy*dy;
    
    if (this.touchedOuterLeft || this.touchedOuterRight)
    {
        if ((r2 > this.outerRadius2) || (r2 < this.innerRadius2))
        {
            clearTimeout(this.timer);
            
            this.touchedOuterLeft = false;
            this.touchedOuterRight = false;
            this.target.removeClass('left-down right-down');
            
            this.target.trigger('cancelRotation');
        }
    }
}

// function to handle mouse up during the rotation interaction
linnRockerData.prototype.MouseUp = function(e)
{
    clearTimeout(this.timer);

    var dx = e.pageX - this.centreX;
    var dy = e.pageY - this.centreY;
    var r2 = dx*dx + dy*dy;

    if (this.touchedOuterLeft || this.touchedOuterRight)
    {
        if (r2 > this.outerRadius2)
        {
            this.target.trigger('cancelRotation');
        }
        else
        {
            this.target.trigger('endRotation');
        }
    }
    else if (this.touchedInner)
    {
        if (r2 < this.innerRadius2)
        {
            this.target.trigger('centreClicked');
        }
    }

    // remove classes for various states
    this.target.removeClass('left-down right-down inner-down');
}

linnRockerData.prototype.DoStep = function()
{
    if(this.touchedOuterLeft)
    {
        this.target.trigger('anticlockwiseRotate');
    }
    else if(this.touchedOuterRight)
    {
        this.target.trigger('clockwiseRotate');
    }
}

function DoAutoRepeat()
{
    if(currentData != null)
    {
        if(currentData.touchedOuterLeft || currentData.touchedOuterRight)
        {
            currentData.DoStep();
            
            currentData.timer = setTimeout(DoAutoRepeat, currentData.kAutoRepeatInterval * 1000);
        }
    }
}



