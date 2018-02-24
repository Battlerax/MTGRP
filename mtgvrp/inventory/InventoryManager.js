/// <reference path="../../types-gtanetwork/index.d.ts" />

var myBrowser = null;
mp.events.add(
    {
        "render": () => {
            if (mp.players.local.getVariable('OVERWEIGHT')) {
                mp.game.controls.disableControlAction(2, 25, true);
                mp.game.controls.disableControlAction(2, 21, true);
                mp.game.controls.disableControlAction(2, 24, true);
                mp.game.controls.disableControlAction(2, 22, true);
            }
        },

        "invmanagement_showmanager": () => {
            if (myBrowser === null)
            myBrowser = mp.browsers.new("inventory/ManageInv.html")
            mp.gui.cursor.show(true)
        },

        "moveItemFromLeftToRightSuccess": (arg1, arg2, arg3, arg4) => {
            mp.events.call("moveItemFromLeftToRightSuccess", arg1, arg2, arg3, arg4);
        },

        "moveItemFromRightToLeftSuccess": (arg1, arg2, arg3, arg4) => {
            mp.events.call("moveItemFromRightToLeftSuccess", arg1, arg2, arg3, arg4);
        }

    })

    /*
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
            mp.events.call("moveItemFromLeftToRightSuccess", args[0], args[1], args[2], args[3]);
            break;
        case 'moveItemFromRightToLeftSuccess':
            mp.events.call("moveItemFromRightToLeftSuccess", args[0], args[1], args[2], args[3]);
            break;
    }
});
*/

/* function loaded() {
    mp.events.call("fillItems", newArgs[0], newArgs[1], newArgs[2], newArgs[3], newArgs[4], newArgs[5]);
}

*/

/* function moveFromLeftToRight(shortname, amount) {
    mp.events.callRemote("invmanagement_moveFromLeftToRight", shortname, amount);
}
*/

/* function moveFromRightToLeft(shortname, amount) {
    mp.events.callRemote("invmanagement_moveFromRightToLeft", shortname, amount);
}
*/

/* function ExitWindow() {
    if (myBrowser) 
        myBrowser.destroy();
    mp.gui.chat.show(true)
    mp.gui.cursor.show(false)
    myBrowser = null;
    mp.events.callRemote('invmanagement_cancelled')
}
*/
