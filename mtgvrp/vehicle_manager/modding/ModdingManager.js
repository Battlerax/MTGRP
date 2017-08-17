var myBrowser = null;
var Args = null;

var curMods = [];

var camera = API.createCamera(new Vector3(-331.7626, -135.005, 40.0), new Vector3(0, 0, 135.6836));
var veh;

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
        API.setChatVisible(false);

        API.pointCameraAtEntity(camera, API.getPlayerVehicle(API.getLocalPlayer()), new Vector3());
        API.setActiveCamera(camera);

        veh = API.getPlayerVehicle(API.getLocalPlayer());
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
        API.setActiveCamera(null);
        API.setChatVisible(true);
    }
});

var isVIP = false;

function loaded() {
    isVIP = Args[1];
    myBrowser.call("addTypes", Args[0], Args[1]);

    //Save current mods.
    for (var i = 0; i < 70; i++) {
        curMods[i] = API.getVehicleMod(veh, i);
    }
}

function resetModType(type) {
    API.removeVehicleMod(veh, type);
    API.setVehicleMod(veh, type, curMods[type]);
}

function callServerEvent(eventName /* Args */) {
    var args = Array.prototype.slice.call(arguments, 1);
    API.triggerServerEvent(eventName, ...args);
}

function putmod(type, id) {
    API.setVehicleMod(veh, parseInt(type), parseInt(id));
}

function updateCurrentColor(type) {
    var clr;
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
        API.setActiveCamera(null);
        API.setChatVisible(true);
        API.triggerServerEvent("MODDING_EXITMENU");
    }
});

/* Vehicle Rotation */
var rotating = 0;
API.onKeyDown.connect(function (sender, e) {
    if (e.KeyCode == Keys.Oemplus) {
        rotating = 4;

    } else if (e.KeyCode == Keys.OemMinus) {
        rotating = -4;
    }
});

API.onKeyUp.connect(function (sender, e) {
    if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.OemMinus) {
        rotating = 0;
    }
});

API.onUpdate.connect(function () {
    if (rotating != 0 && myBrowser !== null) {
        var new_rot = API.getEntityRotation(veh).Add(new Vector3(0, 0, rotating));
        API.setEntityRotation(veh, new_rot);
    }
});