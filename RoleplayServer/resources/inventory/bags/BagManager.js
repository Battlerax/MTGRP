/// <reference path="../../types-gtanetwork/index.d.ts" />

var myBrowser = null;
API.onServerEventTrigger.connect((eventName, args) => {
    switch (eventName) {
        case 'bag_showmanager':
            var res = API.getScreenResolution();
            myBrowser = API.createCefBrowser(725, 605);
            API.waitUntilCefBrowserInit(myBrowser);
            API.setCefBrowserPosition(myBrowser, (res.Width / 2) - (725 / 2),
                (res.Height / 2) - (605 / 2));
            API.loadPageCefBrowser(myBrowser, "inventory/bags/managebag.html");
            API.showCursor(true);

            //Send to fill items.
            API.sleep(500);
            myBrowser.call("fillItems", args[0], args[1]);
            break;

        case 'moveItemFromLeftToRightSucess': 
            myBrowser.call("moveItemFromLeftToRightSuccess", args[0], args[1], args[2]);
            break;
    }
});

function moveFromLeftToRight(id, shortname, amount) {
    API.triggerServerEvent("bag_moveFromLeftToRight", id, shortname, amount);
}

function ExitWindow() {
    API.destroyCefBrowser(myBrowser);
    API.showCursor(false);
    API.setCanOpenChat(true);
    myBrowser = null;
    API.triggerServerEvent("invmanagement_cancelled");
}