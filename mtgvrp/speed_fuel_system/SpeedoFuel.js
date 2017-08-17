var myBrowser = null;
var vehicleHandle = null;

API.onServerEventTrigger.connect((eventName, args) => {
	switch (eventName) {
	case "fuel_updatevalue":
	    if (myBrowser !== null) {
            myBrowser.call("setFuel", args[0]);
	    } else {
            lastFuel = "~r~Fuel: ~w~" + args[0] + "%";
	    }

	    break;
    case "speedo_entervehicle":
        if (API.getPlayerVehicleSeat(API.getLocalPlayer()) !== -1) return;

        if (args[0] === true) {
            if (resource.Introduction.isonintro) return;

            var res = API.getScreenResolutionMaintainRatio();
            var width = 450;
            var height = 200;
            var size = resource.JsFunctions.scaleCoordsToReal({ X: width, Y: height });
            var pos = resource.JsFunctions.scaleCoordsToReal({ X: 310, Y: res.Height - height - 5 });
            myBrowser = API.createCefBrowser(size.X, size.Y);
            API.waitUntilCefBrowserInit(myBrowser);
            API.setCefBrowserPosition(myBrowser, pos.X, pos.Y);
            API.loadPageCefBrowser(myBrowser, "speed_fuel_system/SpeedoFuel.html");
            API.waitUntilCefBrowserLoaded(myBrowser);
        }
        vehicleHandle = API.getPlayerVehicle(API.getLocalPlayer());
        API.triggerServerEvent("fuel_getvehiclefuel");
        break;

    case "TOGGLE_SPEEDO":
            if (myBrowser === null) {
                if (API.getPlayerVehicleSeat(API.getLocalPlayer()) !== -1) return;

                var res2 = API.getScreenResolutionMaintainRatio();
                var width2 = 450;
                var height2 = 200;
                var size2 = resource.JsFunctions.scaleCoordsToReal({ X: width2, Y: height2 });
                var pos2 = resource.JsFunctions.scaleCoordsToReal({ X: 310, Y: res2.Height - height2 - 5 });
                myBrowser = API.createCefBrowser(size2.X, size2.Y);
                API.waitUntilCefBrowserInit(myBrowser);
                API.setCefBrowserPosition(myBrowser, pos2.X, pos2.Y);
                API.loadPageCefBrowser(myBrowser, "speed_fuel_system/SpeedoFuel.html");
                API.waitUntilCefBrowserLoaded(myBrowser);
            } else {
                API.destroyCefBrowser(myBrowser);
                myBrowser = null;
            }
        vehicleHandle = API.getPlayerVehicle(API.getLocalPlayer());
        break;
	}
});

function loaded() {
	var vehicle = API.getPlayerVehicle(API.getLocalPlayer());
	var speed = API.getVehicleMaxSpeed(API.getEntityModel(vehicle));
	var intSpeed = Math.round(speed * 4.3); //m/s to km/h  | I know this is not a real correct rate but the game for some reason isnt accurate so I increased the rate to make sure speed never goes above max.
	if(myBrowser !== null) myBrowser.call("setupSpeed", intSpeed);
	API.triggerServerEvent("fuel_getvehiclefuel");
}

API.onPlayerExitVehicle.connect((vehicle) => {
    vehicleHandle = null;

    if (myBrowser === null) return;

	API.destroyCefBrowser(myBrowser);
    myBrowser = null;
});

var posUpdateTick = Date.now();
var speedUpdateTick = Date.now();

function getDirectionName(direction) {
	var angle = Math.round(direction.Z);
	if (angle >= -23 && angle < 23)
		return "N";
	else if (angle >= 23 && angle < 67)
		return "NW";
	else if (angle >= 67 && angle < 112)
		return "W";
	else if (angle >= 112 && angle < 156)
		return "SW";
	else if ((angle >= 156 && angle < 180) || (angle < -156 && angle >= -180))
		return "S";
	else if (angle < -23 && angle >= -67)
		return "NE";
	else if (angle < -67 && angle >= -112)
		return "E";
	else if (angle < -112 && angle >= -156)
		return "SE";
	else
		return "NO";
}

var lastZone = "";
var lastStreet = "";
var lastDirection = "";

var lastHealth = "";
var lastSpeed = "";
var lastFuel = "";

var screenRes = null;

API.onUpdate.connect(() => {

    if (resource.ModdingManager.myBrowser !== null)
        return;

    //ZoneStreet name.
    if (Date.now() >= posUpdateTick) {
        posUpdateTick = Date.now() + 1000;
        var pos = API.getEntityPosition(API.getLocalPlayer());
        lastStreet = API.getStreetName(pos);
        lastZone = API.getZoneName(pos);

        //Health update.
        if (vehicleHandle !== null && API.doesEntityExist(vehicleHandle)) {
            lastHealth = "~r~Health: ~w~" + Math.round(API.getVehicleHealth(vehicleHandle));
        }

        if (myBrowser !== null)
            myBrowser.call("setZoneStreet", lastStreet, lastZone);
    }

    //Direction update every 100ms
    if (Date.now() >= speedUpdateTick) {
        var rot = API.getEntityRotation(API.getLocalPlayer());
        lastDirection = getDirectionName(rot);
    }


    if (vehicleHandle !== null) {
        if (Date.now() >= speedUpdateTick) {
            speedUpdateTick = Date.now() + 100;

            var velocity = API.getEntityVelocity(vehicleHandle);
            var speed = Math.sqrt(
                velocity.X * velocity.X +
                velocity.Y * velocity.Y +
                velocity.Z * velocity.Z
            );
            speed = Math.floor(speed * 3.6);

            if (myBrowser !== null) {
                myBrowser.call("setSpeed", speed);
                myBrowser.call("setDirection", lastDirection);
            } else {
                lastSpeed = "~r~Speed: ~w~" + speed + " KMH";
            }
        }

    }

    if(myBrowser === null)
    {
        if (resource.Introduction.isonintro == true) {
            return;
        }

        if (screenRes === null)
            screenRes = API.getScreenResolutionMaintainRatio();

        if (lastDirection !== "")
            API.drawText(lastDirection, 310, screenRes.Height - 80, 1, 225, 225, 225, 255, 4, 0, false, true, 0); //155

        if (lastStreet !== "")
            API.drawText(lastStreet, 365, screenRes.Height - 75, 0.5, 225, 225, 225, 255, 4, 0, false, true, 0);

        if (lastZone !== "")
            API.drawText(lastZone, 365, screenRes.Height - 50, 0.5, 225, 225, 225, 255, 4, 0, false, true, 0);
    }

    if (vehicleHandle !== null && API.doesEntityExist(vehicleHandle)) {
        if (lastHealth !== "")
            API.drawText(lastHealth, 40, screenRes.Height - 290, 0.5, 225, 225, 225, 255, 4, 0, false, true, 0);

        if (myBrowser == null) {

            if (lastFuel !== "")
                API.drawText(lastFuel, 40, screenRes.Height - 315, 0.5, 225, 225, 225, 255, 4, 0, false, true, 0);

            if (lastSpeed !== "")
                API.drawText(lastSpeed, 40, screenRes.Height - 340, 0.5, 225, 225, 225, 255, 4, 0, false, true, 0);
        }
    }
});