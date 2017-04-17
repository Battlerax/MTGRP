var fare_marker = null

var fare_price = 0;
var total_fare = 0;
var fare_msg = "";

API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "set_taxi_waypoint":
            var location = args[0];
            API.setWaypoint(location.X, location.Y);         
            break;

        case "get_waypoint_position":
            if (!API.isWaypointSet()) {
                API.chatMessage("~y~[TAXI] ~r~You do not have a way point set!");
                return;
            }
            
            API.triggerServerEvent("update_taxi_destination", API.getWaypointPosition());
            break;

        case "update_fare_display":
            fare_price = args[0];
            total_fare = args[1];
            fare_msg = args[2];
            break;
    }
});


API.onUpdate.connect(function () {
    if (fare_price != 0) {
        API.drawText("~y~Fare: ~g~$~w~" + fare_price + "~n~~y~Current Total: ~g~$~w~" +  total_fare + " " + fare_msg, API.getScreenResolutionMantainRatio().Width - 15, API.getScreenResolutionMantainRatio().Height - 200, 1, 115, 186, 131, 255, 4, 2, false, true, 0);
    }
});