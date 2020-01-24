var player_list_browser = null;
var res = null;

Event.OnResourceStart.connect(function () {
    res = API.getScreenResolutionMaintainRatio();
});

Event.OnServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "send_player_list":
            player_list_browser.call("show_list", args[0], args[1]);
            break;
    }
});


Event.OnKeyUp.connect(function(Player, args){
    if (args.KeyCode == Keys.F1){
		if (player_list_browser == null) {
			let width = 500;
			let height = 600;
			var pos = resource.JsFunctions.scaleCoordsToReal({ X: (res.Width / 2) - (width / 2), Y: (height * 20/100) });
			var size = resource.JsFunctions.scaleCoordsToReal({ X: width, Y: height });
			player_list_browser = API.createCefBrowser(size.X, size.Y);
            API.waitUntilCefBrowserInit(player_list_browser);
			API.setCefBrowserPosition(player_list_browser, pos.X, pos.Y);
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