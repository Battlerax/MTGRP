/// <reference path="../../types-gtanetwork/index.d.ts" />

var myBrowser = null;
API.onServerEventTrigger.connect((eventName, args) => {
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

            //Send to fill items.
            API.sleep(500);
            myBrowser.call("fillItems", args[0], args[1], args[2], args[3], args[4], args[5]);
            break;

        case 'moveItemFromLeftToRightSuccess': 
            myBrowser.call("moveItemFromLeftToRightSuccess", args[0], args[1], args[2], args[3]);
            break;
        case 'moveItemFromRightToLeftSuccess':
            myBrowser.call("moveItemFromRightToLeftSuccess", args[0], args[1], args[2], args[3]);
            break;
    }
});

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