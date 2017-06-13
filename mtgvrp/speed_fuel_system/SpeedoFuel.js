API.onServerEventTrigger.connect((eventName, args) => {
	switch (eventName) {
	case "fuel_updatevalue":
		if(myBrowser !== null)
			myBrowser.call("setFuel", args[0]);
		break;
	}
});

var myBrowser = null;

API.onPlayerEnterVehicle.connect((vehicle) => {
	if (API.getPlayerVehicleSeat(API.getLocalPlayer()) !== -1) return;

	var res = API.getScreenResolution();
	var width = 380;
	var height = 225;
	myBrowser = API.createCefBrowser(width, height);
	API.waitUntilCefBrowserInit(myBrowser);
	API.setCefBrowserPosition(myBrowser,
		310,
		res.Height - height - 5);
	API.loadPageCefBrowser(myBrowser, "speed_fuel_system/SpeedoFuel.html");
	API.setCefDrawState(true);
	API.waitUntilCefBrowserLoaded(myBrowser);
});

function loaded() {
	var vehicle = API.getPlayerVehicle(API.getLocalPlayer());
	var speed = API.getVehicleMaxSpeed(API.getEntityModel(vehicle));
	var intSpeed = Math.round(speed * 4.3); //m/s to km/h  | I know this is not a real correct rate but the game for some reason isnt accurate so I increased the rate to make sure speed never goes above max.
	myBrowser.call("setupSpeed", intSpeed);

	API.triggerServerEvent("fuel_getvehiclefuel");
}

API.onPlayerExitVehicle.connect((vehicle) => {
	if(myBrowser === null) return;

	API.destroyCefBrowser(myBrowser);
	API.setCefDrawState(false);
	myBrowser = null;
});

var posUpdateTick = Date.now();

function getDirectionName(direction) {
	var angle = Math.round(direction.Z);
	API.sendChatMessage("HEY: " + angle);
	if (angle >= -23 && angle < 23)
		return "N";
	else if (angle >= 23 && angle < 67)
		return "NE";
	else if (angle >= 67 && angle < 112)
		return "E";
	else if (angle >= 112 && angle < 156)
		return "SE";
	else if ((angle >= 156 && angle < 180) || (angle < -156 && angle >= -180))
		return "S";
	else if (angle < -23 && angle >= -67)
		return "NW";
	else if (angle < -67 && angle >= -112)
		return "W";
	else if (angle < -112 && angle >= -156)
		return "SW";
	else
		return "NO";
}

API.onUpdate.connect(() => {
	if (myBrowser !== null) {
		var vehicule = API.getPlayerVehicle(API.getLocalPlayer());
		var velocity = API.getEntityVelocity(vehicule);
		var speed = Math.sqrt(
			velocity.X * velocity.X +
			velocity.Y * velocity.Y +
			velocity.Z * velocity.Z
		);
		speed = Math.floor(speed * 3.6);
		myBrowser.call("setSpeed", speed);

		//Direction
		var rot = API.getEntityRotation(API.getLocalPlayer());
		myBrowser.call("setDirection", getDirectionName(rot));

		//ZoneStreet name.
		if (Date.now() >= posUpdateTick) {
			posUpdateTick = Date.now() + 1000;
			var pos = API.getEntityPosition(API.getLocalPlayer());
			myBrowser.call("setZoneStreet", API.getStreetName(pos), API.getZoneName(pos));
		}
	}
});