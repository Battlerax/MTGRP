var weaponmenu;
var marker = null;
var res = null;
API.onServerEventTrigger.connect((event, args) => {
    switch (event) {
        case "get_street_name":
            var pos = args[0];
            lastStreet = API.getStreetName(pos);
            lastZone = API.getZoneName(pos);
            API.triggerServerEvent("update_location", lastStreet, lastZone);
            break;

        case "open_gunrun_menu":
            API.showCursor(true);
            res = API.getScreenResolutionMaintainRatio();
            let width = 679;
            let height = 450;
            var pos = resource.JsFunctions.scaleCoordsToReal({ X: (res.Width / 2) - (width / 2), Y: (height * 20/100) });
            var size = resource.JsFunctions.scaleCoordsToReal({ X: width, Y: height });
            weaponmenu = API.createCefBrowser(size.X, size.Y);
            API.waitUntilCefBrowserInit(weaponmenu);
            API.setCefBrowserPosition(weaponmenu, pos.X, pos.Y);
            API.loadPageCefBrowser(weaponmenu, "job_manager/gunrunner/gunrunnermenu.html");
			break;

        case "create_zone_marker":
            var location = args[0];
            var scale = args[1];
            marker = API.createMarker(1, location, new Vector3(), new Vector3(), new Vector3(scale, scale, scale), 255, 0, 0, 255);
            break;

        case "send_renown":
			weaponmenu.call("display_renown", args[0]);
			break;

        case "delete_zone_marker":
            API.deleteEntity(marker);
            break;

        case "track_weapon_dealer":
            API.setWaypoint(args[0], args[1]);
            break;

        case "intervene_track_player":
		    var pos = args[0];
            lastStreet = API.getStreetName(pos);
            lastZone = API.getZoneName(pos);
			API.sendChatMessage("Yuri Orlov says: The player with the most renown is located at '" + lastStreet + ", " + lastZone + "'. That's all I can give you right now.")
            break;

        case "close_gunrun_menu":
            if (weaponmenu != null) {
                API.destroyCefBrowser(weaponmenu);
            }
            API.showCursor(false);
            API.triggerServerEvent("gunrun_menu_closed");
            break;

		case "send_melee_list":
			weaponmenu.call("show_weapon_list", args[0]);
			break;
		case "send_pistol_list":
			weaponmenu.call("show_weapon_list", args[0], args[1]);
			break;
		case "send_shotgun_list":
			weaponmenu.call("show_weapon_list", args[0], args[1], args[2]);
			break;
		case "send_machinegun_list":
			weaponmenu.call("show_weapon_list", args[0], args[1], args[2], args[3]);
			break;
		case "send_assaultrifle_list":
			weaponmenu.call("show_weapon_list", args[0], args[1], args[2], args[3], args[4]);
			break;
		case "send_sniper_list":
			weaponmenu.call("show_weapon_list", args[0],args[1], args[2], args[3], args[4], args[5]);
			break;
    }
});

function finish_menu(totalPrice = 0, totalWeapons = null){
    if (weaponmenu != null) {
        API.destroyCefBrowser(weaponmenu);
    }
	API.showCursor(false);
    API.triggerServerEvent("gunrun_menu_closed", totalPrice, totalWeapons)
}

function loaded()
{
    API.triggerServerEvent("fetch_weapon_list");
}