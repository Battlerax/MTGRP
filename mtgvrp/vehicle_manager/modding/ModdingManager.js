var myBrowser = null;
var Args = null;

var curMods = [];

API.onServerEventTrigger.connect((event, args) => {
    if (event === "SHOW_MODDING_GUI") {
        var res = API.getScreenResolution();
        myBrowser = API.createCefBrowser(res.Width, res.Height);
        API.waitUntilCefBrowserInit(myBrowser);
        API.setCefBrowserPosition(myBrowser, 0, 0);
        API.loadPageCefBrowser(myBrowser, "vehicle_manager/modding/gui/ModGui.html");
        API.waitUntilCefBrowserLoaded(myBrowser);
        API.setHudVisible(false);
        API.showCursor(true);
        Args = args;
    }
    else if (event === "MODDING_FILL_MODS") {
        if (myBrowser == null)
            return;

        myBrowser.call("showMods", args[0]);
    }
    else if (event === "MODDING_ERROR") {
        if (myBrowser == null)
            return;

        myBrowser.call("showError", args[0]);
    }
    else if (event === "MODDING_CLOSE") {
        if (myBrowser == null)
            return;

        API.setHudVisible(true);
        API.showCursor(false);
        API.destroyCefBrowser(myBrowser);
        myBrowser = null;
    }
});

var isVIP = false;

function loaded() {
    isVIP = Args[1];
    myBrowser.call("addTypes", Args[0], Args[1]);

    //Save current mods.
    var veh = API.getPlayerVehicle(API.getLocalPlayer());
    for (var i = 0; i < 70; i++) {
        curMods[i] = API.getVehicleMod(veh, i);
    }
}

function resetModType(type) {
    var veh = API.getPlayerVehicle(API.getLocalPlayer());
    API.removeVehicleMod(veh, type);
    API.setVehicleMod(veh, type, curMods[type]);
}

function callServerEvent(eventName /* Args */) {
    var args = Array.prototype.slice.call(arguments, 1);
    API.triggerServerEvent(eventName, ...args);
}

function putmod(type, id) {
    var veh = API.getPlayerVehicle(API.getLocalPlayer());
    API.setVehicleMod(veh, parseInt(type), parseInt(id));
}

function updateCurrentColor(type) {
    var clr;
    var veh = API.getPlayerVehicle(API.getLocalPlayer());
    if (type === "primarycolor") {
        clr = API.getVehicleCustomPrimaryColor(veh);
    } else if (type === "secondarycolor") {
        clr = API.getVehicleCustomSecondaryColor(veh);
    } else if (type === "tyresmoke") {
        return;
    } else if (type === "neoncolor") {
        clr = API.getVehicleNeonColor(veh);
    }
    myBrowser.call("updateColorPicker", Math.round(clr.R), Math.round(clr.G), Math.round(clr.B));
}

function updateColor(type, r, g, b) {

    r = Math.round(r);
    g = Math.round(g);
    b = Math.round(b);

    var veh = API.getPlayerVehicle(API.getLocalPlayer());
    if (type === "primarycolor") {
        API.setVehicleCustomPrimaryColor(veh, r, g, b);
    } else if (type === "secondarycolor") {
        API.setVehicleCustomSecondaryColor(veh, r, g, b);
    } else if (type === "tyresmoke") {
        API.setVehicleTyreSmokeColor(veh, r, g, b);
    } else if (type === "neoncolor") {
        API.setVehicleNeonColor(veh, r, g, b);
    }
}

API.onKeyUp.connect((sender, e) => {
    if (e.KeyCode == Keys.Escape && myBrowser != null) {
        API.setHudVisible(true);
        API.showCursor(false);
        API.destroyCefBrowser(myBrowser);
        myBrowser = null;
        API.triggerServerEvent("MODDING_EXITMENU");
    }
});