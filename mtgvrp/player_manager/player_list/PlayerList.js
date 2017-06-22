var player_list_browser = null;
var res = null;

API.onResourceStart.connect(function () {
    res = API.getScreenResolutionMantainRatio();
});

API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "send_player_list":
            player_list_browser.call("show_list", args[0], args[1]);
            break;
    }
});


API.onKeyDown.connect(function(Player, args){
    if (args.KeyCode == Keys.F1){
        if(player_list_browser == null){
	        var pos = resource.JsFunctions.scaleCoordsToReal({ X: res.Width, Y:  res.Height});
            player_list_browser = API.createCefBrowser(pos.X, pos.Y);
            API.waitUntilCefBrowserInit(player_list_browser);
            API.setCefBrowserPosition(player_list_browser, 0, 0);
            API.loadPageCefBrowser(player_list_browser, "player_manager/player_list/PlayerList.html");
            API.showCursor(true);
            API.setCanOpenChat(false);
        }
        else {
            API.destroyCefBrowser(player_list_browser);
            //API.setCefDrawState(false);
            API.showCursor(false);
            API.setCanOpenChat(true);
            player_list_browser = null;
        }
    }
});

function ready() {
	API.triggerServerEvent("fetch_player_list", 0);
}

function fetch_player_list(type) {
	API.triggerServerEvent("fetch_player_list", type);
}

function player_list_pm(name, msg) {
    API.triggerServerEvent("player_list_pm", name, msg);
}

function player_list_teleport(name) {
    API.triggerServerEvent("player_list_teleport", name);
}

function player_list_spectate(name) {
    API.triggerServerEvent("player_list_spectate", name);
}

function player_list_kick(name) {
    API.triggerServerEvent("player_list_kick", name);
}