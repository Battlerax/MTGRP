var oldMarker = null;
var blipNow = null;
var oldBlip = null;

API.onServerEventTrigger.connect((event, args) => {
    if (event === "DMV_UPDATE_MARKER") {

        if (oldMarker !== null && API.doesEntityExist(oldMarker))
            API.deleteEntity(oldMarker);

        if (oldBlip !== null && API.doesEntityExist(oldBlip))
            API.deleteEntity(oldBlip);

        if (blipNow !== null && API.doesEntityExist(blipNow))
            API.deleteEntity(blipNow);

        if (args[0].X !== 0.0 && args[0].Y !== 0.0 && args[0].Z !== 0.0) {
            oldMarker = API.createMarker(1,
                args[0],
                new Vector3(),
                new Vector3(),
                new Vector3(5, 5, 5),
                255,
                225,
                0,
                255);

            blipNow = API.createBlip(args[0]);
            API.setBlipSprite(blipNow, 1);
            API.setBlipColor(blipNow, 46);
        }

        if (args[1].X !== 0.0 && args[1].Y !== 0.0 && args[1].Z !== 0.0) {
            oldBlip = API.createBlip(args[1]);
            API.setBlipSprite(oldBlip, 1);
            API.setBlipColor(oldBlip, 47);
        }
    }
});