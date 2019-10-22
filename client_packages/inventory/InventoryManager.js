/// <reference path="../../types-gtanetwork/index.d.ts" />

var myBrowser = null;
var newArgs = null;
Event.OnServerEventTrigger.connect((eventName, args) => {
    switch (eventName) {
        case 'invmanagement_showmanager':
            var res = API.getScreenResolutionMaintainRatio();
            myBrowser = API.createCefBrowser(720, 660);
            API.waitUntilCefBrowserInit(myBrowser);
	        var pos = resource.JsFunctions.scaleCoordsToReal({X: (res.Width / 2) - (720 / 2), Y: (res.Height / 2) - (660 / 2)});
            API.setCefBrowserPosition(myBrowser, pos.X, pos.Y);
            API.loadPageCefBrowser(myBrowser, "inventory/ManageInv.html");
            //API.setCefDrawState(true);
            API.showCursor(true);
            newArgs = args;
            break;

        case 'moveItemFromLeftToRightSuccess': 
            myBrowser.call("moveItemFromLeftToRightSuccess", args[0], args[1], args[2], args[3]);
            break;
        case 'moveItemFromRightToLeftSuccess':
            myBrowser.call("moveItemFromRightToLeftSuccess", args[0], args[1], args[2], args[3]);
            break;
    }
});

function loaded() {
    myBrowser.call("fillItems", newArgs[0], newArgs[1], newArgs[2], newArgs[3], newArgs[4], newArgs[5]);
}

function moveFromLeftToRight(shortname, amount) {
    API.triggerServerEvent("invmanagement_moveFromLeftToRight", shortname, amount);
}

function moveFromRightToLeft(shortname, amount) {
    API.triggerServerEvent("invmanagement_moveFromRightToLeft", shortname, amount);
}

function ExitWindow() {
    API.destroyCefBrowser(myBrowser);
    //API.setCefDrawState(false);
    API.showCursor(false);
    API.setCanOpenChat(true);
    myBrowser = null;
    API.triggerServerEvent("invmanagement_cancelled");
}

Event.OnUpdate.connect(() => {
    if (API.hasEntitySyncedData(API.getLocalPlayer(), "OVERWEIGHT")) {
        API.disableControlThisFrame(25);
        API.disableControlThisFrame(21);
        API.disableControlThisFrame(24);
        API.disableControlThisFrame(22);
    }
});