var mdcBrowser = null;

API.onResourceStart.connect(function () {
    API.sendChatMessage("MDC.js started...");
});

//From server
API.onServerEventTrigger.connect(function (eventName, args) {
    API.sendChatMessage("Event triggered: " + eventName);
    switch (eventName) {
        case "showMDC":
            API.sendChatMessage("Starting to show MDC.");
            var res = API.getScreenResolution();
            mdcBrowser = API.createCefBrowser(res.Width, res.Height);
            API.sendChatMessage("Browsered created.");
            API.waitUntilCefBrowserInit(mdcBrowser);
            API.sendChatMessage("Finished waiting for CEF.");
            API.setCefBrowserPosition(mdcBrowser, 0, 0);
            API.sendChatMessage("Loading page..."):
            API.loadPageCefBrowser(mdcBrowser, "group_manager/lspd/MDC/MDC.html");
            API.sendChatMessage("Page loaded.");
            API.setCefDrawState(true);
            API.setCanOpenChat(false);
            API.sendChatMessage("Openeding MDC, requesting information.");

            API.sleep(500);
            API.triggerServerEvent("requestInformation");
            API.sendChatMessage("Info requested.");
            break;

        case "hideMDC":

            API.showCursor(false);
            API.destroyCefBrowser(mdcBrowser);
            API.setCefDrawState(false);
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
