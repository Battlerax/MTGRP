var player_list_browser = null;
var res = null;

API.onResourceStart.connect(function () {
    res = API.getScreenResolution();
});

API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "send_player_list":

            player_list_browser.call("empty_player_list");

            var player_list = args[0];
            for (var i = 0; i < player_list.Count; i++) {

                var obj = JSON.parse(player_list[i]);
                player_list_browser.call("add_player", obj.id, obj.name, args[1]);

            }
            player_list_browser.call("show_list");
            break;
    }
});


API.onKeyDown.connect(function(Player, args){
    if (args.KeyCode == Keys.F1){
        if(player_list_browser == null){
            player_list_browser = API.createCefBrowser(res.Width, res.Height);
            API.waitUntilCefBrowserInit(player_list_browser);
            API.setCefBrowserPosition(player_list_browser, 0, 0);
            API.loadPageCefBrowser(player_list_browser, "player_manager/player_list/PlayerList.html");
            API.showCursor(true);
            API.setCanOpenChat(false);
            API.setCefDrawState(true);

            API.sleep(500);
            API.triggerServerEvent("fetch_player_list", 0)
        }
        else {
            API.destroyCefBrowser(player_list_browser);
            API.setCefDrawState(false);
            API.showCursor(false);
            API.setCanOpenChat(true);
            player_list_browser = null;
        }
    }
});

function fetch_player_list(type) {
    API.triggerServerEvent("fetch_player_list", type)
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