var holdingbag = false;
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {

	        case "garbage_setwaypoint":
		    var location = args[0];
            API.setWaypoint(location.X, location.Y);   
			mark = API.createMarker(1, location, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 0, 0, 255);
            break;

			case "garbage_holdbag":
			holdingbag = true;
			break;

			case "garbage_dropbag":
			holdingbag = false;
			break;

    }
});

API.onUpdate.connect(() => {
    if (holdingbag) {
        API.disableControlThisFrame(25);
        API.disableControlThisFrame(21);
        API.disableControlThisFrame(24);
        API.disableControlThisFrame(22);
    }

	if (API.isControlJustPressed(24)) {
        if (holdingbag === true) {
            holdingbag = false;
            API.triggerServerEvent("garbage_throwbag");
        } 
    }
});