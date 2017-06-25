let menuPool = null;
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
		
        case "update_beacon":
		    var location = args[0];
            API.setWaypoint(location.X, location.Y);         
            break;

    }
});


