var isBlindfolded;

Event.OnServerEventTrigger.connect((eventName, args) => {
	if (eventName === "blindfold_intiate") {
		var camera = API.createCamera(API.getEntityPosition(API.getLocalPlayer()).Add(new Vector3(0, 0, 1)), new Vector3(90, 0, 0));
		API.setActiveCamera(camera);
		isBlindfolded = true;
	}
	else if (eventName === "blindfold_cancel") {
		API.setActiveCamera(null);
		isBlindfolded = false;
	}
});

Event.OnUpdate.connect(() => {
	if (isBlindfolded) {
		API.callNative("8187532053442985248"); //HIDE_HUD_AND_RADAR_THIS_FRAME
	}
});