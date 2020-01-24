var browser = null;

Event.OnServerEventTrigger.connect((eventName, args) => {
    switch (eventName) {
        case "send_error":
            browser.call("send_error", args[0]);
            break;
        case "populateViewMappingRequest":
            browser.call("populateViewMappingRequest", args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
            break;
        case "showRequestCode":
            browser.call("showRequestCode", args[0]);
            break;
        case "addMappingRequest":
            browser.call("addMappingRequest", args[0], args[1], args[2], args[3]);
            break;
        case "createPagination":
            browser.call("createPagination", args[0], args[1], args[2]);
            break;
        case "emptyMappingTable":
            browser.call("emptyMappingTable");
            break;
        case "showMappingManager":
            var res = API.getScreenResolutionMantainRatio();
            var width = 1000;
            var height = 600;
            var pos = resource.JsFunctions.scaleCoordsToReal({ X: (res.Width / 2) - (width / 2), Y: (res.Height / 2) - (height / 2) });
            var size = resource.JsFunctions.scaleCoordsToReal({ X: width, Y: height });

            browser = API.createCefBrowser(size.X, size.Y);
            API.waitUntilCefBrowserInit(browser);
            API.setCefBrowserPosition(browser, pos.X, pos.Y);
            API.loadPageCefBrowser(browser, "mapping_manager/MappingManager.html");
            API.waitUntilCefBrowserLoaded(browser);
            API.showCursor(true);
            API.setCanOpenChat(false);

            break;
    }
});

function pageLoaded() {
    API.triggerServerEvent("requestFirstMappingPage");
}

function callServerEvent(eventName /* Args */) {
    var args = Array.prototype.slice.call(arguments, 1);
    API.triggerServerEvent(eventName, ...args);
}

Event.OnKeyDown.connect((sender, e) => {
    if (e.KeyCode === Keys.Escape && browser !== null) {
        API.destroyCefBrowser(browser);
        API.showCursor(false);
        API.setCanOpenChat(true);
        browser = null;
    }
});