//Can't Be Converted due to missing functions in rage.

/*
function getSafeResolution() {
    let offsetX = 0;
    let {x, y} = mp.game.graphics.getScreenActiveResolution(x, y);
    if (x / y > 1.7777) {
        // aspect ratio is larger than 16:9
        const idealBox = Math.ceil(y * 1.7777);
        // ex: 2850 - 1920 == 660 / 2 == 330
        offsetX = (x - idealBox) / 2;
        // and gotta set the ideal box to make it work
        x = idealBox;
    }

    return { offsetX, x, y }
}

function scaleCoordsToReal(point) {
    const ratioScreen = API.getScreenResolutionMaintainRatio();
    let {x2, y2} = mp.game.graphics.getScreenActiveResolution(x, y);

    const widthDivisor = x / ratioScreen.Width;
    const heightDivisor = y / ratioScreen.Height;

    return { X: point.X * widthDivisor, Y: point.Y * heightDivisor }
}
*/

/*
var mapMarginLeft = API.getScreenResolutionMaintainRatio().Width / 64;
var mapMarginBottom = API.getScreenResolutionMaintainRatio().Height / 60;
var mapWidth = API.getScreenResolutionMaintainRatio().Width / 7.11;
var mapHeight = API.getScreenResolutionMaintainRatio().Height / 5.71;
var mapX = mapMarginLeft + mapWidth + mapMarginLeft;
var mapY = API.getScreenResolutionMaintainRatio().Height - mapHeight - mapMarginBottom;
*/
var lastObj;

mp.events.add({
    "PLACE_OBJECT_ON_GROUND_PROPERLY": (object, arg2) => {
        lastObj = object;
        // API.callNative("PLACE_OBJECT_ON_GROUND_PROPERLY", lastObj);
        var pos = lastObj.getCoords(false);
        var rot = lastObj.getRotation(2);
        mp.events.callRemote("OBJECT_PLACED_PROPERLY", object, pos, rot);
        if (arg2 !== "")
            mp.events.callRemote(arg2, lastObj)
    },
    
    "COMPLETE_FREEZE": (state) => {
        let p = mp.players.local
        p.freezePosition(state);
        
        if (p.vehicle) {
            p.vehicle.freezePosition(state)
            if (state === true) 
            p.vehicle.setDoorsLocked(4);
            else
            p.vehicle.setDoorsLocked(0);
        }
    },
    
    'entityStreamIn': (entity) => {
        if (entity.getVariable("TargetObj") === lastObj) {
            // API.callNative("PLACE_OBJECT_ON_GROUND_PROPERLY", entity);
            var pos = entity.getCoords(false);
            var rot = entity.getRotation(2);
            // API.triggerServerEvent(lastEvent, lastObj, pos, rot); ?? wat
            mp.events.callRemote("OBJECT_PLACED_PROPERLY", lastObj, pos, rot);
            return true;
        }
    }
})