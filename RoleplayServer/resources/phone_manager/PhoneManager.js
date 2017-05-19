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
            break;

        case "phone_calling":
            showPhoneIfNotShown();
            API.sleep(500);
            myBrowser.call("calling", args[0], args[1]);
            break;

        case "phone_incoming-call":
            showPhoneIfNotShown();
            API.sleep(500);
            myBrowser.call("incoming_call", args[0], args[1]);
            break;

        case "phone-call-closed":
            myBrowser.call("callClosed");
            break;
    }
});

var isMouseShown = false;
API.onKeyUp.connect(function (sender, e) {
    if (myBrowser !== null && e.KeyCode === Keys.Escape) {
        API.destroyCefBrowser(myBrowser);
        API.showCursor(false);
        myBrowser = null;
    }
    else if (myBrowser !== null && e.KeyCode === Keys.M) {
        isMouseShown = !isMouseShown;
        API.showCursor(isMouseShown);
    }
})

function callPhone(number) {
    API.triggerServerEvent("phone_callphone", number);
}
function answerCall() {
    API.triggerServerEvent("phone_answercall");
}

function closeCall() {
    API.triggerServerEvent("phone_hangout");
}