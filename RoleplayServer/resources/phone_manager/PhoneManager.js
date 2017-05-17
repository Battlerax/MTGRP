var myBrowser;

API.onServerEventTrigger.connect((eventName, args) => {
    switch (eventName) {
        case "phone_showphone":
            var res = API.getScreenResolution();
            var width = 405;
            var height = 590;
            myBrowser = API.createCefBrowser(width, height);
            API.waitUntilCefBrowserInit(myBrowser);
            API.setCefBrowserPosition(myBrowser, res.Width - width,
                res.Height - height);
            API.loadPageCefBrowser(myBrowser, "phone_manager/gui/main.html");
            API.setCanOpenChat(false);
            API.showCursor(true);
            break;
    }
});


API.onKeyUp.connect(function (sender, e) {
    if (myBrowser !== null && e.KeyCode === Keys.Escape) {
        API.destroyCefBrowser(myBrowser);
        API.showCursor(false);
        API.setCanOpenChat(true);
        myBrowser = null;
    }
})