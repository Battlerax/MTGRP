var myBrowser;

API.onServerEventTrigger.connect((eventName, args) => {
    switch (eventName) {
        case "phone_showphone":
            var res = API.getScreenResolution();
            var width = 350;
            var height = 450;
            myBrowser = API.createCefBrowser(width, height);
            API.waitUntilCefBrowserInit(myBrowser);
            API.setCefBrowserPosition(myBrowser, res.Width - (width + 10),
                res.Height - (height + 10));
            API.loadPageCefBrowser(myBrowser, "phone_manager/gui/main.html");
            API.showCursor(true);
            break;
    }
});