var vehicle_edit_browser = null;
var res = null;

API.onResourceStart.connect(function () {
    res = API.getScreenResolutionMantainRatio();
});

API.onServerEventTrigger.connect(function (eventName, args) {

    switch (eventName) {
        case "show_vehicle_edit_menu":
	        var pos = resource.JsFunctions.scaleCoordsToReal({ X: res.Width, Y:  res.Height});
            vehicle_edit_browser = API.createCefBrowser(pos.X, pos.Y);
            API.waitUntilCefBrowserInit(vehicle_edit_browser);
            API.setCefBrowserPosition(vehicle_edit_browser, 0, 0);
            API.loadPageCefBrowser(vehicle_edit_browser, "vehicle_manager/vehicle_editor/VehicleEdit.html");
            API.showCursor(true);
            API.setCanOpenChat(false);
            //API.setCefDrawState(true);
            
            API.sleep(1000);
            vehicle_edit_browser.call("populate_fields", args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
            break;
        case "send_veh_edit_error":
            vehicle_edit_browser.call("send_error", args[0]);
            break;
        case "finish_veh_edit":
            API.destroyCefBrowser(vehicle_edit_browser);
            //API.setCefDrawState(false);
            API.showCursor(false);
            API.setCanOpenChat(true);
            break;
    }

});

function change_spawn() {
    API.triggerServerEvent("vehicle_edit_change_spawn");
}

function cancel_veh_edit() {
    API.destroyCefBrowser(vehicle_edit_browser);
    API.showCursor(false);
    API.triggerServerEvent("cancel_veh_edit");
    API.setCanOpenChat(true);
}

function vehicle_edit_changes(vehicle_id, model, owner, license_plate, color_1, color_2, respawn_delay, job_id, group_id) {
    API.triggerServerEvent("vehicle_edit_save", vehicle_id, model, owner, license_plate, color_1, color_2, respawn_delay, job_id, group_id);
}

function respawn_veh() {
    API.triggerServerEvent("vehicle_edit_respawn");
}

function is_edit_vehicle_menu_open() {
    if (vehicle_edit_browser != null)
        return true;
    else return false;
}

