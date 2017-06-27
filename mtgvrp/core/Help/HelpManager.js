var myBrowser;
var argss;

API.onServerEventTrigger.connect((eventName, args) => {
    if (eventName === "help_showMenu") {
        var res = API.getScreenResolutionMantainRatio();
        var width = 800;
        var height = 500;
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


        argss = args[0];
    }
});

function loaded() {
    myBrowser.call("fillUpCommands", argss);
}