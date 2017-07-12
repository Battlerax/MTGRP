function getSafeResolution () {
	var offsetX = 0;
	var screen = API.getScreenResolutionMaintainRatio();
	var screenX = screen.Width;
	var screenY = screen.Height;
	if (screenX / screenY > 1.7777) {
		// aspect ratio is larger than 16:9
		var idealBox = Math.ceil(screenY * 1.7777);
		// ex: 2850 - 1920 == 660 / 2 == 330
		offsetX = (screenX - idealBox) / 2;
		// and gotta set the ideal box to make it work
		screenX = idealBox;
	}

	return { Offset: offsetX, X: screenX, Y: screenY }
}

function scaleCoordsToReal (point) {
	var ratioScreen = getSafeResolution();
	var realScreen = API.getScreenResolution();

	var widthDivisor = realScreen.Width / ratioScreen.X;
	var heightDivisor = realScreen.Height / ratioScreen.Y;

	return { X: (point.X * widthDivisor) + ratioScreen.Offset, Y: point.Y * heightDivisor }
}

var lastObj;
var lastEvent;
API.onServerEventTrigger.connect((event, args) => {
    if (event === "PLACE_OBJECT_ON_GROUND_PROPERLY") {
        //0: Object, 1: Eventname

        lastObj = args[0];
        lastEvent = args[1];

        var obj = null;
        //Find object.
        var objs = API.getStreamedObjects();
        for (var i = 0; i < objs.Count(); i++) {
            if (API.getEntitySyncedData(objs[i], "TargetObj") === args[0]) {
                obj = objs[i];
                break;
            }
        }

        if (obj === null) {
            return;
        }

        API.callNative("0x58A850EAEE20FAA3", obj);
        var pos = API.getEntityPosition(obj);
        var rot = API.getEntityRotation(obj);
        API.triggerServerEvent(args[1], args[0], pos, rot);
    }
});

API.onEntityStreamIn.connect((entity, entityType) => {
    if (API.getEntitySyncedData(entity, "TargetObj") === lastObj) {
        API.callNative("0x58A850EAEE20FAA3", entity);
        var pos = API.getEntityPosition(entity);
        var rot = API.getEntityRotation(entity);
        API.triggerServerEvent(lastEvent, lastObj, pos, rot);
    }
})