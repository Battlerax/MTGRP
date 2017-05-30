API.onServerEventTrigger.connect((eventName, args) => {

	switch (eventName) {
		
		case "update_beacon":
			setTimeout(function(){

			var beaconBlip = API.createBlip(args[0]);
			API.setBlipSprite(beaconBlip, 459);
			API.setBlipName(beaconBlip, "Backup Beacon");

            beaconMarker = API.createMarker(1,
            args[0],
            new Vector3(),
            new Vector3(),
            new Vector3(5.0, 5.0, 5.0),
            255,
            255,
            0,
            255);
            break;

			},5000);

			API.deleteEntity(beaconBlip);
			API.deleteEntity(beaconMarker);
		}
});

API.onKeyDown.connect(function(player, args){
    if (args.KeyCode == Keys.M && !API.isChatOpen()) {
	        API.triggerServerEvent("toggle_megaphone_key", player);
        }

});
