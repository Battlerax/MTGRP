var myBrowser = null;
var newArgs = null;
mp.events.add({
    "render": () => {
        if (mp.players.local.getVariable('OVERWEIGHT')) {
            mp.game.controls.disableControlAction(2, 25, true);
            mp.game.controls.disableControlAction(2, 21, true);
            mp.game.controls.disableControlAction(2, 24, true);
            mp.game.controls.disableControlAction(2, 22, true);
        }
    },

    "loaded": () => {
        mp.events.call("fillItems", newArgs[0], newArgs[1], newArgs[2], newArgs[3], newArgs[4], newArgs[5]);
    },

    "moveFromLeftToRight": (shortname, amount) => {
        mp.events.callRemote("invmanagement_moveFromLeftToRight", shortname, amount);
    },

    "moveFromRightToLeft": (shortname, amount) => {
        mp.events.callRemote("invmanagement_moveFromRightToLeft", shortname, amount);
    },

    "ExitWindow": () => {
        if (myBrowser)
            myBrowser.destroy();
        mp.gui.chat.show(true)
        mp.gui.cursor.show(false)
        myBrowser = null;
        mp.events.callRemote('invmanagement_cancelled')
    },

    "invmanagement_showmanager": (args) => {
        if (myBrowser === null)
            myBrowser = mp.browsers.new("inventory/ManageInv.html")
        mp.gui.cursor.show(true)
        newArgs = args;
    },

    "moveItemFromLeftToRightSuccess": () => {
        mp.events.call("moveItemFromLeftToRightSuccess", newArgs[0], newArgs[1], newArgs[2], newArgs[3]);
    },

    "moveItemFromRightToLeftSuccess": () => {
        mp.events.call("moveItemFromRightToLeftSuccess", newArgs[0], newArgs[1], newArgs[2], newArgs[3]);
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