API.onServerEventTrigger.connect((eventName, args) => {

});

var myBrowser = null;

API.onPlayerEnterVehicle.connect((vehicle) => {
	if (API.getPlayerVehicleSeat(API.getLocalPlayer()) !== -1) return;

	var res = API.getScreenResolution();
	var width = 380;
	var height = 190;
	myBrowser = API.createCefBrowser(width, height);
	API.waitUntilCefBrowserInit(myBrowser);
	API.setCefBrowserPosition(myBrowser,
		300,
		res.Height - height - 10);
	API.loadPageCefBrowser(myBrowser, "speed_fuel_system/SpeedoFuel.html");
	API.setCefDrawState(true);
	API.waitUntilCefBrowserLoaded(myBrowser);
});

function loaded() {
	var vehicle = API.getPlayerVehicle(API.getLocalPlayer());
	var speed = API.getVehicleMaxSpeed(API.getEntityModel(vehicle));
	var intSpeed = Math.round(speed * 4); //m/s to km/h  | I know this is not a real correct rate but the game for some reason isnt accurate so I increased the rate to make sure speed never goes above max.
	myBrowser.call("setupSpeed", intSpeed);
}

API.onPlayerExitVehicle.connect((vehicle) => {
	if (API.getPlayerVehicleSeat(API.getLocalPlayer()) !== -1) return;

	API.destroyCefBrowser(myBrowser);
	API.setCefDrawState(false);
	myBrowser = null;
});

var lastPos = "";
var posUpdateTick = Date.now();

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
	}

	if (lastPos !== "") {
		API.drawText("~w~" + lastPos, 20, API.getScreenResolution().Height - 300, 1, 115, 186, 131, 255, 4, 0, false, true, 0);
	}

	if (Date.now() >= posUpdateTick) {
		posUpdateTick = Date.now() + 1000;
		lastPos = API.getZoneName(API.getEntityPosition(API.getLocalPlayer()));
	}
});