var myBrowser = null;
var argss;

API.onServerEventTrigger.connect((eventName, args) => {
    if (eventName === "help_showMenu") {
        var res = API.getScreenResolutionMaintainRatio();
        var width = 1000;
        var height = 600;
        var pos = resource.JsFunctions.scaleCoordsToReal({ X: (res.Width / 2) - (width / 2), Y: (res.Height / 2) - (height / 2) });
        var size = resource.JsFunctions.scaleCoordsToReal({ X: width, Y: height });
        myBrowser = API.createCefBrowser(size.X, size.Y);
        API.waitUntilCefBrowserInit(myBrowser);
        API.setCefBrowserPosition(myBrowser, pos.X, pos.Y);
        API.loadPageCefBrowser(myBrowser, "core/Help/HelpMenu.html");
        //API.setCefDrawState(true);
        API.waitUntilCefBrowserLoaded(myBrowser);
        API.showCursor(true);
        API.setCanOpenChat(false);


        argss = args;
    }
});

function loaded() {
    myBrowser.call("fillUpCommands", argss[0], argss[1], argss[2], argss[3]);
}

function unauth() {
    API.sendNotification("You are unauthorized to view these commands.");
}

API.onKeyDown.connect((sender, e) => {
    if (e.KeyCode === Keys.Escape && myBrowser !== null) {
        API.destroyCefBrowser(myBrowser);
        API.showCursor(false);
        API.setCanOpenChat(true);
        myBrowser = null;
    }
});