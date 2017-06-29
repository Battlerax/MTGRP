API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {

	        case "garbage_setwaypoint":
		    var location = args[0];
            API.setWaypoint(location.X, location.Y);   
			mark = API.createMarker(1, location, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 0, 0, 255);
            break;
    }
});