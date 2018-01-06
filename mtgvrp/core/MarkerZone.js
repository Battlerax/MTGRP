Event.OnServerEventTrigger.connect((eventName, args) => {
    if (eventName == "setMarkerZoneRouteVisible") {
        API.setBlipRouteColor(args[0], args[2]);
        API.setBlipRouteVisible(args[0], args[1]);
    }
});