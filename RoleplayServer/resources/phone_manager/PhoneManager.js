var myBrowser;

API.onServerEventTrigger.connect((eventName, args) => {
    switch (eventName) {
        case "phone_showphone":
            if (myBrowser == null) {
                API.sendChatMessage("You already have the phone opened.");
                break;
            }

            var res = API.getScreenResolution();
            var width = 405;
            var height = 590;
            myBrowser = API.createCefBrowser(width, height);
            API.waitUntilCefBrowserInit(myBrowser);
            API.setCefBrowserPosition(myBrowser, res.Width - width,
                res.Height - height);
            API.loadPageCefBrowser(myBrowser, "phone_manager/gui/main.html");
            break;
    }
});

var isMouseShown = false;
API.onKeyUp.connect(function (sender, e) {
    if (myBrowser !== null && e.KeyCode === Keys.Escape) {
        API.destroyCefBrowser(myBrowser);
        myBrowser = null;
    }
    else if (myBrowser !== null && e.KeyCode === Keys.M) {
        isMouseShown = !isMouseShown;
        API.showCursor(isMouseShown);
    }
})