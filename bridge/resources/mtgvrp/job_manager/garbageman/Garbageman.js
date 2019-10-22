﻿var holdingbag = false;
Event.OnServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {

	        case "garbage_setwaypoint":
		    var location = args[0];
            API.setWaypoint(location.X, location.Y);   
            break;

			case "garbage_holdbag":
			holdingbag = true;
			break;

			case "garbage_dropbag":
			holdingbag = false;
			break;

            case "garbage_removewaypoint":
            API.removeWaypoint();
            break;
    }
});

Event.OnUpdate.connect(() => {
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

Event.OnKeyDown.connect((sender, e) =>
{
	if (e.KeyCode === Keys.E) {
	API.triggerServerEvent("garbage_pickupbag");
	}
});