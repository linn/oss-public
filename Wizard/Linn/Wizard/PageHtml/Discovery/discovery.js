function onDoneButtonClick() {
    var sel = document.getElementById("ListBox");
    if (sel.selectedIndex > 0) {
        var device = sel.options[sel.selectedIndex].value;
        xappSend('Update', device);
    }
}

function onRefreshButtonClick() {
    xappSend('Refresh', '');
}

function onPageUpButtonClick() {
    xappSend('PageUp', '');
}

function onPageDownButtonClick() {
    xappSend('PageDown', '');
}


function onListBoxClick() {
    var sel = document.getElementById("ListBox");
    alert("selected " + sel.selectedIndex);
    if (sel.selectedIndex > 0) {
        var device = sel.options[sel.selectedIndex].value;
        xappSend('SelectionChanged', device);
    }
}


function onSelectionAdd(json) {
    var opt = document.createElement("option");

    // Assign text and value to Option object
    opt.text = json.Text;
    opt.value = json.Id;

    // Add an Option object to Drop Down/List Box
    var list = document.getElementById("ListBox");
    list.options.add(opt);
    if (json.Selected) {
        list.options[list.options.length - 1].selected = true;
    }

    xappSend('ListChange', 'Added');
}

function onSelectionClear(json) {
    var listbox = document.getElementById("ListBox");
    var i;
    for (i = listbox.options.length - 1; i >= 0; i--) {
        listbox.options.remove(i);
    }

    xappSend('ListChange', 'Cleared');
}

function onSelectionReplace(json) {
    var listbox = document.getElementById("ListBox");

    var i;
    for (i = 0; listbox.options.length > i; i++) {
        // find matching entry... require old and new values in json !!!!

    }
}

function onTitle(json) {
    $("#Title").html(json.Value);
}

function onMainText(json) {
    $("#MainText").html(json.Value);
}

function onDeviceButtonUpdate(json) {
    n = document.getElementById(json.Button);
    if (n) {
        $(n).html(json.Room);

        if (json.Color != undefined) {
            n.style.color = json.Color;
        }

        if (json.New != undefined) {
            if (json.New) {
                n.style.backgroundImage = "url('../Resources/Images/Miscellaneous/NewIcon.png')";
            }
            else {
                n.style.backgroundImage = "none";
            }
        }

        if (json.BackgroundColor != undefined) {
            n.style.backgroundColor = json.BackgroundColor;
        }
    }
}

function onError(json) {
    $("#Main").html(json.Value);
    document.getElementById("Main").style.color = "red";
}
