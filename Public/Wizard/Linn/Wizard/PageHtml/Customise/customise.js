/**
 * Customise DSM ports
 * requires jQuery v1.7.2
 *
 * Generic script that should handle the configuration 
 * of any list of ports displayed in an ordered list
 *
 * Outputs a list of the configured ports as a JSON string on save
 * Tested for Webkit and IE9
 *
 */

$(document).ready(function () {

    PortWizard.init();
    alert("init");
    $("#btnSave").click(function () {
        // ***********
        // handle save - this would get sent to a server/device?
        // currently just dumping to firebug debugger, condition added so IE doesn't throw any errors
        if (window.console) console.log(JSON.stringify(ports));
        // ***********
    });

});
	 
var ports = []; // array of ports
var PortWizard = {}; // port wizard object
 
// container for each port
PortWizard.Port = function(id, name, icon) {
	 this.id = id;
	 this.name = name;
	 this.icon = icon;
	 };
 
 
 /**
 * Initalise port wizard
 * Load all available ports into an array
 * Set rollover events
 * Set click events
 */
 PortWizard.init = function () {
	 
	 
	//	@defaultField			- default string displayed in port edit field
	//	@iconPath				- path to port icons
	//	@productPath			- path to product images (for rollover states showing different ports)
	//	@productPathOriginal	- stores the original image path before any mouse events change it				
	//	@icons[]				- contains image names and alts, names can be [name].png or [name]_on.png for selected items
	this.config = {
		defaultField: "Type name",
		iconPath: "../Resources/Images/SourceIcons/",
		productPath: "./",
		productPathOriginal: $("#product-rear img:first-child").attr("src"),
		icons : [
					{img : 'iconcable', alt : 'Cable box'},
					{img : 'iconcomputer', alt : 'Computer'},
					{img : 'iconcontroller', alt : 'Controller'},
					{img : 'icondisc', alt : 'Disc'},
					{img : 'iconipad', alt : 'iPad'},
					{img : 'iconiphone', alt : 'iPhone'},
					{img : 'iconipod', alt : 'iPod'},
					{img : 'iconmoviesfilm', alt : 'Films'},
					{img : 'iconmp3', alt : 'Mp3'},
					{img : 'iconplaystation', alt : 'Playstation'},
					{img : 'iconsatellite', alt : 'satellite'},
					{img : 'iconturntable', alt : 'Turntable'},
					{img : 'icontv', alt : 'TV'},
					{img : 'iconwii', alt : 'WII'},
					{img : 'iconxbox', alt : 'Xbox'}
				]

	};

	// reset product image to default once user has been messing with the ports
	$("div.customise-list ul").mouseleave(function() {
		$("#product-rear img:first-child").attr("src", PortWizard.config.productPathOriginal );
	});

	// handle row interactions
    $("div.customise-list li").each(function(){
		
		// populate our array of ports
        var port = new PortWizard.Port(
			$(this).attr("id"),
			"",
			""
		);
		ports.push(port);
		
		
		// configure hover events to show/hide ports on product image
		// each image showing ports is stored as an HTML5 data attribute ('data-img') on the <li>
		var img_product = $("#product-rear img:first-child");
		$(this).hover(
			function(){
				$(img_product).attr("src", PortWizard.config.productPath + $(this).data("img") );
			},
			function(){
				$(img_product).attr("src", img_product.attr("src") );
			}
		);
		
		
		// insert html controls for each row
		var row_label = $(this).find("span.label");
		var row_options = $(this).find("span.options");
		var editing_row = $(this).attr("id");
		
		// insert controls
		row_options.append(row_label[0].firstChild.textContent  + '<input id="customise-id_' + editing_row + '" type="text" value="' + PortWizard.getLabel(editing_row) + '" onblur="PortWizard.setLabel(\'' + editing_row + '\', this);" onkeypress="PortWizard.checkEnter(event, \'' + editing_row + '\', this);" onfocus="PortWizard.checkLabelDefault(this);" class="field" /><div class="choose-icon"><a href="#" class="customise-btn">Choose Icon</a><div class="panel-icons"></div></div>');
		
		// insert icon panel
		var icon_list = document.createElement("ul")

		for (var ii=0 in PortWizard.config.icons) {
			$(icon_list).append('<li><span><img src="' + PortWizard.config.iconPath + PortWizard.config.icons[ii].img + '.png" alt="' + PortWizard.config.icons[ii].alt + '" onclick="PortWizard.setIcon(\'' + editing_row + '\', \'' + PortWizard.config.icons[ii].img + '\');" /></span></li>');
		}

		$(this).find(".panel-icons").append(icon_list);
			
		
      });
  
	  // config row click event
	  $("div.customise-list li").click(function(e) {
		
		// prevent click events happening on child elements
		// without this the click event on the 'choose icon' button fires, making bad things happen
		e.stopPropagation(); 
		
		var editing_row_id = $(this).attr("id");
		var editing_row_last_id = $("#hdEditing").val();

		// only switch the edit around if we are on a different row
		// Also prevents child element click events bubbling
		if (editing_row_id != editing_row_last_id){
			
			// set current edit row
			$("#hdEditing").val(editing_row_id);		
			// hide any previous action on another row (if user has been editing other rows)
			PortWizard.resetGrid();
			
			// get the string to be edited
			$(this).find("input#customise-id_" + editing_row_id).val(PortWizard.getLabel(editing_row_id));
			
			// show edit line
			$(this).addClass("customise");
			$(this).find("span.label").hide();
			$(this).find("span.options").show();
		
		}

	  });
	  
	// config icon chooser
	$("div.customise-list li div.choose-icon").click(function(e) {
		
		// if we dont't have this, then when a user clicks an icon its seen as 
		// 		a second click further down the DOM
		e.stopPropagation(); 

		// show hide our icon panel
		$(this).find(".panel-icons").toggle();
		
	});


};


 /**
 * Set value of current user edited field into array of ports - called onblur
 * 
 *	@editing_row_id	-	id of row being edited
 *	@edit_field		-	text field new value is being set
 */
PortWizard.setLabel = function (editing_row_id, edit_field) {

    if ($(edit_field).val() !== this.config.defaultField && $(edit_field).val().length > 0) {
        var this_port = PortWizard.getPort(editing_row_id);
        this_port.name = $(edit_field).val();
        $("#" + editing_row_id).find("span.label-user").html(this_port.name);
        PortWizard.setPort(this_port);
        $("#" + editing_row_id).addClass("edited");

        alert("row " + editing_row_id + "; name " + this_port.name + "; element " + $("#" + editing_row_id) + "; element name " + $("#" + editing_row_id).find("span.label-user").html());
    }

    // done editing - set row back to default display
    // ** changed this to only happen when user hits return in the form field
    // ** otherwise user cannot edit the label and icon in one edit
    // $("#hdEditing").val(''); 
    // PortWizard.resetGrid();

};

 /**
 * Gets value of current user edited field
 * 
 *	@editing_row_id	-	id of row being edited
 */
PortWizard.getLabel = function (editing_row_id) {

    var this_port = PortWizard.getPort(editing_row_id);
    if (this_port != null) {
        return this_port.name.length > 0 ? this_port.name : this.config.defaultField;
    }
    else
    {
        return this.config.defaultField;
    }

};

 /**
 * overrides .setLabel on a return keypress
 * 
 *	@e				-	keypress event
 *	@editing_row_id	-	id of row being edited
 *	@edit_field		-	text field new value is being set
 */
PortWizard.checkEnter = function (e, editing_row_id, edit_field) {

    var keypress = (e.keyCode ? e.keyCode : e.which);
    if (keypress == 13) {
        alert("row " + editing_row_id + " field " + edit_field);
        PortWizard.setLabel(editing_row_id, edit_field);
        PortWizard.resetGrid(); // if the user hits return, update grid
        return false;
    }
    else {
        return true;
    }
};

 /**
 * Clear default value of form field when user clicks inside- called onfucus
 * 
 *	@edit_field		-	text field being edited
 */
PortWizard.checkLabelDefault = function (edit_field) {
	if ($(edit_field).val() == this.config.defaultField ){
		$(edit_field).val('');	
	}
};


 /**
 * Returns single port object based on id
 * 
 *	@editing_row_id	-	id of row being edited
 */
PortWizard.getPort = function(editing_row_id){
	for (var ii=0 in ports) {
		if (ports[ii].id == editing_row_id)
            return ports[ii];
    }
	return null;
};

 /**
 * Updates our port array with a new port
 * 
 *	@new_port	-	new port to replace old one in the array
 */
PortWizard.setPort = function(new_port) {
	for (var ii=0 in ports) {
		if (ports[ii].id == new_port.id){
			 ports[ii] = new_port;
			 return true;
		}
          
    }
	return false;
};

 /**
 * Updates our port array with a new icon and refreshes display
 * 
 *	@editing_row_id	-	id of row being edited
 *	@selected_image	-	selected image name
 */
PortWizard.setIcon = function(editing_row_id, selected_image) {
	
		var this_port = PortWizard.getPort(editing_row_id);
		var this_row = $("div.customise-list li#" + editing_row_id);
		
		// save new icon
		this_port.icon = selected_image;
		PortWizard.setPort(this_port);
		
		// update row
		this_row.css("background-image", "url(" + this.config.iconPath + selected_image + "_on.png)");	
		this_row.addClass("edited");

		// hide icon panel
		$(this_row).find(".panel-icons").toggle();
		return false; // <-- this might be redundant?
};


 /**
 * Resets port grid back to default, non edit, non over state
 * 
 */
PortWizard.resetGrid = function() {

	var grid = $("div.customise-list li.customise");

	if (grid.length > 0){
		
		var row_options = grid.find("span.options");
		var row_label = grid.find("span.label");
		var row_icon_panel = grid.find("div.panel-icons");
		
		$("div.customise-list li").removeClass("customise");
		row_icon_panel.hide();
		row_options.hide();
		row_label.show();
	}
	
};



/* end */