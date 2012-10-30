

$(document).ready(function () {

    $('#TicketFirstName').data('max', 64);
    $('#TicketLastName').data('max', 64);
    $('#TicketPhoneNumber').data('max', 64);
    $('#TicketEmail').data('max', 256);
    $('#TicketContactNotes').data('max', 512);
    $('#TicketFaultDescription').data('max', 512);

});


function SetAllFieldsValid() {
    SetFieldStatus("#TicketFirstNameTitle", true);
    SetFieldStatus("#TicketLastNameTitle", true);
    SetFieldStatus("#TicketPhoneNumberTitle", true);
    SetFieldStatus("#TicketEmailTitle", true);
    SetFieldStatus("#TicketFaultDescriptionTitle", true);
    SetFieldStatus("#TicketContactNotesTitle", true);
}


function SetFieldStatus(aFieldName, aStatus) {

    if (aStatus == true) {
        $(aFieldName).removeClass('text_invalid');
    }
    else {
        $(aFieldName).addClass('text_invalid');
    }
}

function onSubmissionFailed(report) {
    ActivateSubmissionFailedPage(report);
}


function onReportSubmitted(report) {

    var n = report.Value.split(",");
    var message = n[1];
    ActivateReportSubmittedPage(message);
}



function TextKeyup(control, e) {
    //alert("keyup");
    var fieldName = '#' + control.id;
    var maxLength = $(fieldName).data('max');
    var content = $(fieldName).val();

    if (content.length > maxLength) {
        var text = content.substr(0, maxLength); // truncate
        $(fieldName).val(text); 
    }
    else {
        SetFieldStatus(fieldName + "Title", true);
    }
}



function TextEntryKey(control, e) {
    //alert("keypress");
    var fieldName = '#' + control.id;
    SetFieldStatus(fieldName+"Title", true);
    var textBoxHasSpaceForThisChar = ($(fieldName).val().length < $(fieldName).data('max'));
    return (textBoxHasSpaceForThisChar);
}


function onSubmitButtonClick() {

    //alert($('#FormSubmitButton').text());
    if ($('#FormSubmitButton').text() == "Retry")   // previous submit failed
    {
        $('#HelpTitle').text($('#HelpTitle').data('defaultText'));
        $('#HelpText').text($('#HelpText').data('defaultText'));
        $('#FormSubmitButton').text($('#FormSubmitButton').data('defaultText'));
        ShowSubmitItems();
    }
    else {

        $('#FormSubmitButton').hide();
        $('#FormClearButton').hide();

        if (ValidateInputFields()) {
            var firstname = $('#TicketFirstName').val();
            var lastname = $('#TicketLastName').val();
            var email = $('#TicketEmail').val();
            var phone = $('#TicketPhoneNumber').val();
            var contact = $('#TicketContactNotes').val();
            var description = $('#TicketFaultDescription').val();

            var escapedFirstname = Escaped(firstname, ',', '/');
            var escapedLastname = Escaped(lastname, ',', '/');
            var escapedEmail = Escaped(email, ',', '/');
            var escapedPhone = Escaped(phone, ',', '/');
            var escapedContact = Escaped(contact, ',', '/');
            var escapedDescription = Escaped(description, ',', '/');

            var concatStr = escapedFirstname + "," + escapedLastname + "," + escapedEmail + "," + escapedPhone + "," + escapedContact + "," + escapedDescription

            xappSend('Submit', concatStr);
        }
        else {
            $('#FormSubmitButton').show();
            $('#FormClearButton').show();
        }
    }
}

function ValidateInputFields() {

    SetAllFieldsValid();
    var allValid = true;

    if (IsEmpty($('#TicketFirstName').val())) {
        SetFieldStatus('#TicketFirstNameTitle', false);
        allValid = false;
    }
    
    if (IsEmpty($('#TicketLastName').val())) {
        SetFieldStatus('#TicketLastNameTitle', false);
        allValid = false;
    }

    if (IsEmpty($('#TicketEmail').val() + $('#TicketPhoneNumber').val()))
    {
        SetFieldStatus("#TicketEmailTitle", false);
        SetFieldStatus('#TicketPhoneNumberTitle', false);
        allValid = false;
    }

    // trim trailing whitespace
    var text = $('#TicketFirstName').val();
    $('#TicketFirstName').val($.trim(text));

    var text = $('#TicketLastName').val();
    $('#TicketLastName').val($.trim(text));

    var text = $('#TicketEmail').val();
    $('#TicketEmail').val($.trim(text));

    var text = $('#TicketPhoneNumber').val();
    $('#TicketPhoneNumber').val($.trim(text));


    return allValid;
}


function IsEmpty(str) {
    // returns true if blank or only contains whitespace 
    return str.replace(/^\s+|\s+$/g, '').length == 0;
}



function onClearButtonClick() {
    $('#TicketFirstName').val("");
    $('#TicketLastName').val("");
    $('#TicketEmail').val("");
    $('#TicketPhoneNumber').val("");
    $('#TicketContactNotes').val("");
    $('#TicketFaultDescription').val("");
    SetAllFieldsValid();
}


function Escaped(aUnescaped, aChar, aEscapeChar) {
    var escapedStr = "";

    for (i = 0; i < aUnescaped.length; i++) {
        if (aUnescaped[i] == aChar) {
            escapedStr += aEscapeChar;
        }
        escapedStr += aUnescaped[i];
    }
    //alert(escaped);
    return (escapedStr);
}


function ActivateSubmissionFailedPage(aResponse) 
{
    // we're not changing pages here, just modifying this one (HelpPage)
    HideSubmitItems();
    $('#HelpTitle').data('defaultText', $('#HelpTitle').text());    // save for reverting later
    $('#HelpText').data('defaultText', $('#HelpText').text());  // save for reverting later
    $('#FormSubmitButton').data('defaultText', $('#FormSubmitButton').text());  // save for reverting later

    $('#FormSubmitButton').text("Retry");
    $('#FormSubmitButton').show();

    $('#HelpTitle').text("Connection Problem");
    $('#HelpText').html("It has not been possible to contact Linn.<BR/><BR/>Please check your internet connection or try again later.");
}



function ActivateReportSubmittedPage(aResponse)
{
    // we're not changing pages here, just modifying this one (HelpPage)
    HideSubmitItems();

    $('#HelpTitle').text("Report submitted");
    $('#HelpText').text(aResponse);
}


function HideSubmitItems() {
    $('#TicketFirstNameTitle').hide();
    $('#TicketFirstName').hide();
    $('#TicketLastNameTitle').hide();
    $('#TicketLastName').hide();
    $('#TicketLastNameTitle').hide();
    $('#TicketPhoneNumberTitle').hide();
    $('#TicketPhoneNumber').hide();
    $('#TicketEmailTitle').hide();
    $('#TicketEmail').hide();
    $('#TicketFaultDescriptionTitle').hide();
    $('#TicketFaultDescription').hide();
    $('#TicketLastName').hide();
    $('#TicketContactNotesTitle').hide();
    $('#TicketContactNotes').hide();

    $('#FormSubmitButton').hide();
    $('#FormClearButton').hide();
}


function ShowSubmitItems() {
    $('#TicketFirstNameTitle').show();
    $('#TicketFirstName').show();
    $('#TicketLastNameTitle').show();
    $('#TicketLastName').show();
    $('#TicketLastNameTitle').show();
    $('#TicketPhoneNumberTitle').show();
    $('#TicketPhoneNumber').show();
    $('#TicketEmailTitle').show();
    $('#TicketEmail').show();
    $('#TicketFaultDescriptionTitle').show();
    $('#TicketFaultDescription').show();
    $('#TicketLastName').show();
    $('#TicketContactNotesTitle').show();
    $('#TicketContactNotes').show();

    $('#FormSubmitButton').show();
    $('#FormClearButton').show();

}

