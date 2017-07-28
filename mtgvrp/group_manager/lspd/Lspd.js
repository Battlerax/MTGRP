let menuPool = null;
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
		
        case "update_beacon":
            var location = args[0];

            if (location.X == 0 && location.Y == 0){
				API.removeWaypoint();
				break;
			}

            API.setWaypoint(location.X, location.Y);  
            break;
    }
});


