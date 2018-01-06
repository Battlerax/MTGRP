function getSafeResolution() {
    let offsetX = 0;
    const screen = API.getScreenResolutionMaintainRatio();
    let screenX = screen.Width;
    const screenY = screen.Height;
    if (screenX / screenY > 1.7777) {
        // aspect ratio is larger than 16:9
        const idealBox = Math.ceil(screenY * 1.7777);
        // ex: 2850 - 1920 == 660 / 2 == 330
        offsetX = (screenX - idealBox) / 2;
        // and gotta set the ideal box to make it work
        screenX = idealBox;
    }

    return { offsetX, screenX, screenY }
}

function scaleCoordsToReal(point) {
    const ratioScreen = API.getScreenResolutionMaintainRatio();
    const realScreen = API.getScreenResolution();

    const widthDivisor = realScreen.Width / ratioScreen.Width;
    const heightDivisor = realScreen.Height / ratioScreen.Height;

    return { X: point.X * widthDivisor, Y: point.Y * heightDivisor }
}

var mapMarginLeft = API.getScreenResolutionMaintainRatio().Width / 64;
var mapMarginBottom = API.getScreenResolutionMaintainRatio().Height / 60;
var mapWidth = API.getScreenResolutionMaintainRatio().Width / 7.11;
var mapHeight = API.getScreenResolutionMaintainRatio().Height / 5.71;
var mapX = mapMarginLeft + mapWidth + mapMarginLeft;
var mapY = API.getScreenResolutionMaintainRatio().Height - mapHeight - mapMarginBottom;

var lastObj;
var lastEvent;
Event.OnServerEventTrigger.connect((event, args) => {
    if (event === "PLACE_OBJECT_ON_GROUND_PROPERLY") {
        //0: Object

        lastObj = args[0];

        API.callNative("PLACE_OBJECT_ON_GROUND_PROPERLY", lastObj);
        var pos = API.getEntityPosition(lastObj);
        var rot = API.getEntityRotation(lastObj);
        API.triggerServerEvent("OBJECT_PLACED_PROPERLY", args[0], pos, rot);
        if (args[1] !== "")
            API.triggerServerEvent(args[1], lastObj);
    }
    else if (event === "COMPLETE_FREEZE") {
        var state = args[0];
        var p = API.getLocalPlayer();

        API.callNative("FREEZE_ENTITY_POSITION", p, state);
        if (API.isPlayerInAnyVehicle(p)) {
            API.callNative("FREEZE_ENTITY_POSITION", API.getPlayerVehicle(p), state);
            if (state === true)
                API.callNative("SET_VEHICLE_DOORS_LOCKED", API.getPlayerVehicle(p), 4);
            else
                API.callNative("SET_VEHICLE_DOORS_LOCKED", API.getPlayerVehicle(p), 0);
        }
    }
});

Event.OnEntityStreamIn.connect((entity, entityType) => {
    if (API.getEntitySyncedData(entity, "TargetObj") === lastObj) {
        API.callNative("0x58A850EAEE20FAA3", entity);
        var pos = API.getEntityPosition(entity);
        var rot = API.getEntityRotation(entity);
        API.triggerServerEvent(lastEvent, lastObj, pos, rot);
        return;
    }
});