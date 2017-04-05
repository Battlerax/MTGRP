var beaconMarker = null;
let menuPool = null;

API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
		
        case "update_beacon":
            beaconMarker = API.createMarker(1,
                args[0],
                new Vector3(),
                new Vector3(),
                new Vector3(5.0, 5.0, 5.0),
                255,
                255,
                0,
                255);

            var beaconBlip = API.createBlip(args[0]);
            var beaconBlip = API.createBlip(args[0]);
            API.setBlipSprite(beaconBlip, 459);
            API.setBlipName(beaconBlip, "Backup Beacon");

            API.attachEntity(beaconBlip, args[1], 0, new Vector3(), new Vector3())
            API.attachEntity(beaconMarker, args[1], 0, new Vector3(), new Vector3())
            break;

        case "delete_beacon":
            API.deleteEntity(beaconBlip);
            API.deleteEntity(beaconMarker);
            break;

    }
});

API.onKeyDown.connect(function (player, args) {
    if (args.KeyCode == Keys.M && !API.isChatOpen()) {
        API.triggerServerEvent("toggle_megaphone_key", player);
    }

});


