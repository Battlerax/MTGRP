var oldMarker = null;

API.onServerEventTrigger.connect((event, args) => {
    if (event === "DMV_UPDATE_MARKER") {

        if (oldMarker !== null && API.doesEntityExist(oldMarker))
            API.deleteEntity(oldMarker);

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
        }
    }
});