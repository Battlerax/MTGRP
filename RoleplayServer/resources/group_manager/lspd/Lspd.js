let menuPool = null;
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
		
        case "update_beacon":
		    var location = args[0];
            API.setWaypoint(location.X, location.Y);         
            break;

    }
});

API.onKeyDown.connect(function (player, args) {
    if (args.KeyCode == Keys.M && !API.isChatOpen()) {
        API.triggerServerEvent("toggle_megaphone_key", player);
    }

});


