var myBrowser = null;
var Args = null;

API.onServerEventTrigger.connect((event, args) => {
    if (event === "SHOW_MODDING_GUI") {
        var res = API.getScreenResolution();
        myBrowser = API.createCefBrowser(res.Width, res.Height);
        API.waitUntilCefBrowserInit(myBrowser);
        API.setCefBrowserPosition(myBrowser, 0, 0);
        API.loadPageCefBrowser(myBrowser, "vehicle_manager/modding/gui/ModGui.html");
        API.waitUntilCefBrowserLoaded(myBrowser);
        API.setHudVisible(false);
        API.showCursor(true);
        Args = args;
    }
    else if (event === "MODDING_EVENT") {
        if (myBrowser == null)
            return;

        var eventName = args[0];
        var restOfargs = Array.prototype.slice.call(args, 1);
        myBrowser.call(eventName, ...restOfargs);
    }
});

function loaded() {
    myBrowser.call("addTypes", Args[0]);
}

function callServerEvent(eventName /* Args */) {
    var args = Array.prototype.slice.call(arguments, 1);
    API.triggerServerEvent(eventName, ...args);
}