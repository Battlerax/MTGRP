API.onEntityStreamIn.connect(function (ent, entType) {
    if (entType == 6 || entType == 8) {
        API.triggerServerEvent("update_ped_for_client", ent);
    }
});

var player_money = null;
var jail_time = 0;

API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "update_money_display": {
            player_money = args[0];
            break;
        }

		case "update_jail_time": {
			jail_time = args[0];
			break;
		}
    }
});

API.onUpdate.connect(function () {
    if(player_money != null){
        API.drawText("~g~$~w~" + player_money, API.getScreenResolutionMantainRatio().Width - 15, 100, 1, 115, 186, 131, 255, 4, 2, false, true, 0);
    }

	if(jail_time != 0){
		API.drawText("~r~JAIL TIME LEFT: ~w~" + jail_time, API.getScreenResolutionMantainRatio().Width - 15, 160, 1, 115, 186, 131, 255, 4, 2, false, true, 0);
	}
    //API.dxDrawTexture("/cef_resources/MTGVRP_LOGO_SMALL.png", new Point(API.getScreenResolutionMantainRatio().Width - 100, 0), new Size(100, 100), 0);
});