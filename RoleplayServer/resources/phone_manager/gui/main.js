$(document).ready(function() {
    resourceCall("callServerEvent", "phone_getNotifications");
});

$(document).ready(function () {
    $(".tile").click(function () {
        var file = $(this).data("file");
       
        //Load app: 
        $("#app-browser").attr("src", "apps/" + file);
        $("#apps-list").css("display", "none"); //Set the apps list as hidden.
        $("#app-content").css("display", "block"); //Show the content.
    });

    $("#navigation").click(function () {
        //Unload app: 
        $("#app-browser").attr("src", "");
        $("#apps-list").css("display", "block"); //Set the apps list as shown.
        $("#app-content").css("display", "none"); //Hide the content.

        if ($("#call-interface").css("display") === "block") {
            resourceCall("callServerEvent", "phone_hangout");
            $("#call-interface").css("display", "none");
        }

        resourceCall("callServerEvent", "phone_getNotifications");
    });

    $("#calling_answer").click(function () {
        resourceCall("callServerEvent", "phone_answercall");
    });
    $("#calling_end-call").click(function () {
        resourceCall("callServerEvent", "phone_hangout");
    });
    $("#calling_ignore").click(function() {
        //We will just ignore it CEF side.
        callClosed();
    });
});

function showNotifications(messages) {
    $("#unreadMessages").text(messages);
}

function callAppFunction(functionName /* Args */) {
    var args = Array.prototype.slice.call(arguments, 1);
    var sent = [functionName, args];
    document.getElementById('app-browser').contentWindow.postMessage(sent, "*");
}

function incoming_call(name, number)
{
	$("#apps-list").css("display", "none");
    $("#app-content").css("display", "none"); 
    $("#call-interface").css("display", "block");
    $("#calling_header").text("INCOMING CALL");
	$("#caller_name").text(name);	
	$("#caller_number").text(number);
	$("#calling_end-call").css("display", "none");
	$("#calling_answer").css("display", "inline");
	$("#calling_ignore").css("display", "inline");
}

function calling(name, number) {
    $("#apps-list").css("display", "none");
    $("#app-content").css("display", "none");
    $("#call-interface").css("display", "block");
    $("#calling_header").text("CALLING");
    $("#caller_name").text(name);
    $("#caller_number").text(number);
    $("#calling_end-call").css("display", "inline");
    $("#calling_answer").css("display", "none");
    $("#calling_ignore").css("display", "none");
}

function callClosed() {
    if ($("#call-interface").css("display") === "block") {
        $("#apps-list").css("display", "block"); //Set the apps list as shown.
        $("#app-content").css("display", "none"); //Hide the content.
        $("#call-interface").css("display", "none");
    }
}