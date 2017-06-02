/// <reference path="../../types-gtanetwork/index.d.ts" />

var myBrowser = null;
API.onServerEventTrigger.connect((eventName, args) => {
    switch (eventName) {
        case 'invmanagement_showmanager':
            var res = API.getScreenResolution();
            myBrowser = API.createCefBrowser(720, 660);
            API.waitUntilCefBrowserInit(myBrowser);
            API.setCefBrowserPosition(myBrowser, (res.Width / 2) - (720 / 2),
                (res.Height / 2) - (660 / 2));
            API.loadPageCefBrowser(myBrowser, "inventory/ManageInv.html");
            API.showCursor(true);

            //Send to fill items.
            API.sleep(500);
            myBrowser.call("fillItems", args[0], args[1], args[2], args[3], args[4], args[5]);
            break;

        case 'moveItemFromLeftToRightSuccess': 
            myBrowser.call("moveItemFromLeftToRightSuccess", args[0], args[1], args[2], args[3], args[4]);
            break;
        case 'moveItemFromRightToLeftSuccess':
            myBrowser.call("moveItemFromRightToLeftSuccess", args[0], args[1], args[2], args[3], args[4]);
            break;
    }
});

function moveFromLeftToRight(id, shortname, amount) {
    API.triggerServerEvent("invmanagement_moveFromLeftToRight", id, shortname, amount);
}

function moveFromRightToLeft(id, shortname, amount) {
    API.triggerServerEvent("invmanagement_moveFromRightToLeft", id, shortname, amount);
}

function ExitWindow() {
    API.destroyCefBrowser(myBrowser);
    API.showCursor(false);
    API.setCanOpenChat(true);
    myBrowser = null;
    API.triggerServerEvent("invmanagement_cancelled");
}