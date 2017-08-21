var myBrowser = null;
var Args = null;

var curMods = [];

var curCamPos = new Vector3(-331.7626, -135.005, 41.0);
var camera = API.createCamera(curCamPos, new Vector3(0, 0, 135.6836));
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
        if (i === 22 && curMods[i] === -1) {
            curMods[i] = 0;
        }
    }

    if (API.getVehiclePrimaryColor(veh) > 159) {
        //Load custom colors.
        curMods[100] = API.getVehicleCustomPrimaryColor(veh);
        curMods[101] = API.getVehicleCustomSecondaryColor(veh);
    } else {
        curMods[100] = API.getVehiclePrimaryColor(veh);
        curMods[101] = API.getVehicleSecondaryColor(veh);
    }


    curMods[102] = null; //todo: fix this next GTMP update.
    curMods[103] = API.getVehicleNeonColor(veh);
    curMods[104] = API.getVehicleWindowTint(veh);

}

function resetModType(type) {
    if (type === 100) {
        if (curMods[100].hasOwnProperty("R"))
            API.setVehicleCustomPrimaryColor(veh, curMods[100].R, curMods[100].G, curMods[100].B);
        else
            API.setVehiclePrimaryColor(veh, curMods[100]);
    }
    else if (type === 101) {
        if (curMods[101].hasOwnProperty("R"))
            API.setVehicleCustomSecondaryColor(veh, curMods[101].R, curMods[101].G, curMods[101].B);
        else
            API.setVehicleSecondaryColor(veh, curMods[101]);
    }
    else if (type === 102) {
        API.setVehicleTyreSmokeColor(veh, 0, 0, 0); //todo: fix this next GTMP update.
    }
    else if (type === 103) {
        API.setVehicleNeonColor(veh, curMods[103].R, curMods[103].G, curMods[103].B);
    } else if (type === 104) {
        API.setVehicleWindowTint(veh, curMods[104]);
    }
    else {
        API.removeVehicleMod(veh, type);
        API.setVehicleMod(veh, type, curMods[type]);
    }
}

function callServerEvent(eventName /* Args */) {
    var args = Array.prototype.slice.call(arguments, 1);
    API.triggerServerEvent(eventName, ...args);
}

function putmod(type, id) {
    if (type === 104) {
        API.setVehicleWindowTint(veh, parseInt(id));
    } else {
        API.setVehicleMod(veh, parseInt(type), parseInt(id));
    }
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
        if (!curMods[100].hasOwnProperty("R"))
            API.setVehiclePrimaryColor(veh, 0);

        API.setVehicleCustomPrimaryColor(veh, r, g, b);
    } else if (type === "secondarycolor") {
        if (!curMods[101].hasOwnProperty("R"))
            API.setVehicleSecondaryColor(veh, 0);

        API.setVehicleCustomSecondaryColor(veh, r, g, b);
    } else if (type === "tyresmoke") {
        API.setVehicleTyreSmokeColor(veh, r, g, b);
    } else if (type === "neoncolor") {
        API.setVehicleNeonColor(veh, r, g, b);

        if (!API.getVehicleNeonState(veh, 0) === false)
            API.setVehicleNeonState(veh, 0, true);
        if (!API.getVehicleNeonState(veh, 1) === false)
            API.setVehicleNeonState(veh, 1, true);
        if (!API.getVehicleNeonState(veh, 2) === false)
            API.setVehicleNeonState(veh, 2, true);
        if (!API.getVehicleNeonState(veh, 3) === false)
            API.setVehicleNeonState(veh, 3, true);
    }
}

API.onKeyUp.connect((sender, e) => {
    if (e.KeyCode == Keys.Escape && myBrowser != null) {
        exitModShop();
    }
});

function exitModShop() {
    API.setHudVisible(true);
    API.showCursor(false);
    API.destroyCefBrowser(myBrowser);
    myBrowser = null;
    API.setActiveCamera(null);
    API.setChatVisible(true);
    API.triggerServerEvent("MODDING_EXITMENU");
}

/* Vehicle Rotation */
var rotating = 0;
API.onKeyDown.connect(function (sender, e) {
    if (myBrowser == null)
        return;

    if (e.KeyCode == Keys.Oemplus) {
        rotating = 4;

    } else if (e.KeyCode == Keys.OemMinus) {
        rotating = -4;
    }
    else if (e.KeyCode == Keys.ShiftKey) {
        if (Math.round(curCamPos.X) == -340) {
            myBrowser.call("showError", "You cannot zoom any further.");
            return;
        }

        curCamPos.X -= 1;
        curCamPos.Y -= 1;
        API.setCameraPosition(camera, curCamPos);
        API.pointCameraAtEntity(camera, API.getPlayerVehicle(API.getLocalPlayer()), new Vector3());
    }
    else if (e.KeyCode == Keys.ControlKey) {
        if (Math.round(curCamPos.X) == -330) {
            myBrowser.call("showError", "You cannot zoom any further.");
            return;
        }

        curCamPos.X += 1;
        curCamPos.Y += 1;
        API.setCameraPosition(camera, curCamPos);
        API.pointCameraAtEntity(camera, API.getPlayerVehicle(API.getLocalPlayer()), new Vector3());
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