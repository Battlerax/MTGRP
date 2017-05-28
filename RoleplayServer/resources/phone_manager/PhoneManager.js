var myBrowser = null;

function showPhoneIfNotShown() {
    if (myBrowser == null) {
        var res = API.getScreenResolution();
        var width = 405;
        var height = 590;
        myBrowser = API.createCefBrowser(width, height);
        API.waitUntilCefBrowserInit(myBrowser);
        API.setCefBrowserPosition(myBrowser,
            res.Width - width,
            res.Height - height);
        API.loadPageCefBrowser(myBrowser, "phone_manager/gui/main.html");
        API.waitUntilCefBrowserLoaded(myBrowser);
        return true;
    }
    return false;
}

var funcToBeCalled = "";
var args;
function setToBeCalled(func /* args */) {
    var a = Array.prototype.slice.call(arguments, 1);
    funcToBeCalled = func;
    args = a;
}
function phoneLoaded() {
    if (funcToBeCalled !== "") {
        myBrowser.call(funcToBeCalled, ...args);
    }
}


API.onServerEventTrigger.connect((eventName, args) => {
    switch (eventName) {
        case "phone_showphone":
            if (myBrowser !== null) {
                API.sendChatMessage("You already have the phone opened.");
                break;
            }

            showPhoneIfNotShown();
            setToBeCalled("setTime", args[0], args[1]);
            break;

        case "phone_calling":
            if (showPhoneIfNotShown() === true) {
                setToBeCalled("calling", args[0], args[1]);
            } else {
                myBrowser.call("calling", args[0], args[1]);
            }
            break;

        case "phone_incoming-call":
            if (showPhoneIfNotShown() === true) {
                setToBeCalled("incoming_call", args[0], args[1]);
            } else {
                myBrowser.call("incoming_call", args[0], args[1]);
            }
            break;

        case "phone_call-closed":
            myBrowser.call("callClosed");
            break;

        case "phone_showContacts":
            myBrowser.call("callAppFunction", "showContacts", args[0]);
            break;

        case "phone_contactAdded":
            myBrowser.call("callAppFunction", "contactAdded", args[0], args[1]);
            break;

        case "phone_contactEdited":
            myBrowser.call("callAppFunction", "contactEdited", args[0], args[1], args[2]);
            break;

        case "phone_contactRemoved":
            myBrowser.call("callAppFunction", "contactRemoved", args[0]);
            break;

        case "phone_messageContactsLoaded":
            myBrowser.call("callAppFunction", "messageContactsLoaded", args[0]);
            break;

        case "phone_messageSent":
            myBrowser.call("callAppFunction", "messageSent");
            break;

        case "phone_showMessages":
            myBrowser.call("callAppFunction", "showMessages", args[0], args[1], args[2], args[3]);
            break;

        case "phone_incomingMessage":
            myBrowser.call("callAppFunction", "incomingMessage", args[0], args[1]);
            break;

        case "phone_showNotifications":
            myBrowser.call("showNotifications", args[0]);
            break;

        case "phone_showSettings":
            myBrowser.call("callAppFunction", "setSettings", args[0], args[1]);
            break;
    }
});

var isMouseShown = false;
API.onKeyUp.connect(function(sender, e) {
    if (myBrowser !== null && e.KeyCode === Keys.Escape) {
        API.destroyCefBrowser(myBrowser);
        API.showCursor(false);
        API.setCanOpenChat(true);
        myBrowser = null;
    } else if (myBrowser !== null && e.KeyCode === Keys.F2) {
        isMouseShown = !isMouseShown;
        API.showCursor(isMouseShown);
        API.setCanOpenChat(!isMouseShown);
    }
});

function callServerEvent(eventName /* Args */) {
    var args = Array.prototype.slice.call(arguments, 1);
    API.triggerServerEvent(eventName, ...args);
}