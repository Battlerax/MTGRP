var mdcBrowser = null;
//From server
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "showMDC":
            var res = API.getScreenResolution();
            mdcBrowser = API.createCefBrowser(res.Width, res.Height);
            API.waitUntilCefBrowserInit(mdcBrowser);
            API.setCefBrowserPosition(mdcBrowser, 0, 0);
            API.loadPageCefBrowser(mdcBrowser, "MDC.html");
            API.showCursor(true);
            API.setCanOpenChat(false);
            break;
        case "hideMDC":
            API.showCursor(false);
            API.destroyCefBrowser(mdcBrowser);
            break;
        case "add911":
            //number, time, info
            mdcBrowser.call("add911", args[0], args[1], args[2]);
            break;
        case "addBolo":
            //boloId, officer, time, priority, info
            mdcBrowser.call("addBolo", args[0], args[1], args[2], args[3], args[4]);
            break;
        case "remove911":
            break;
    }
});
//From HTML 
function updateMdcAnnoucement(text) {
    API.triggerServerEvent("updateMdcAnnouncement", text);
}
function removeBolo(boloId) {
    API.triggerServerEvent("removeBolo", boloId);
}
function sendBoloToServer(info, priority) {
    API.triggerServerEvent("createBolo", info, priority);
}
