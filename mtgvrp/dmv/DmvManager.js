var oldMarker = null;
var blipNow = null;
var oldBlip = null;

var menuPool = null;

var isInTest = false;
var checkpoints = null;
var nextCheckpoint = null;
var testVehicle;
var player;

Event.OnServerEventTrigger.connect((event, args) => {
    if (event === "DMV_CANCEL_TEST") {
        checkpoints = null;
        nextCheckpoint = null;
        isInTest = false;
        updateMarkers(new Vector3(), new Vector3());
    }
    else if (event === "DMV_STARTTEST") {
        checkpoints = args[0];
        nextCheckpoint = 0;
        isInTest = true;
        player = API.getLocalPlayer();
        testVehicle = args[1];
        updateMarkers(checkpoints[nextCheckpoint], checkpoints[nextCheckpoint + 1]);
    }
    else if (event === "DMV_SELECTVEHICLE") {
        menuPool = API.getMenuPool();
        var myCars = API.createMenu("Your Vehicles", "Select a vehicle to register.", 0, 0, 4);
        var carsList = JSON.parse(args[0]);
        for (var i = 0; i < carsList.length; i++) {
            var car = API.createMenuItem(carsList[i][0], `ID: #${carsList[i][1]}`);
            myCars.AddItem(car);
        }

        menuPool.Add(myCars);
        myCars.Visible = true;

        myCars.OnItemSelect.connect(function (csender, citem, cindex) {
            myCars.Visible = false;
            API.triggerServerEvent("DMV_REGISTER_VEHICLE", carsList[cindex][1]);
        });
    }
});

function ApplyNextCheckpoint() {
    if (isInTest) {
        nextCheckpoint++;
        if (nextCheckpoint < checkpoints.Count) {
            if (nextCheckpoint + 1 < checkpoints.Count) {
                updateMarkers(checkpoints[nextCheckpoint], checkpoints[nextCheckpoint + 1]);
            }
            else {
                updateMarkers(checkpoints[nextCheckpoint], new Vector3());
            }
        }
        else {
            //Sucess
            API.triggerServerEvent("DMV_TEST_FINISH");
            checkpoints = null;
            nextCheckpoint = null;
            isInTest = false;
            updateMarkers(new Vector3(), new Vector3());
        }

    }
}

function updateMarkers(nextLoc, afterLoc) {
    if (oldMarker !== null && API.doesEntityExist(oldMarker))
        API.deleteEntity(oldMarker);

    if (oldBlip !== null && API.doesEntityExist(oldBlip))
        API.deleteEntity(oldBlip);

    if (blipNow !== null && API.doesEntityExist(blipNow))
        API.deleteEntity(blipNow);

    if (nextLoc.X !== 0.0 && nextLoc.Y !== 0.0 && nextLoc.Z !== 0.0) {
        oldMarker = API.createMarker(1,
            nextLoc.Subtract(new Vector3(0, 0, 0.5)),
            new Vector3(),
            new Vector3(),
            new Vector3(5, 5, 5),
            255,
            225,
            0,
            255);

        blipNow = API.createBlip(nextLoc);
        API.setBlipSprite(blipNow, 1);
        API.setBlipColor(blipNow, 46);
    }

    if (afterLoc.X !== 0.0 && afterLoc.Y !== 0.0 && afterLoc.Z !== 0.0) {
        oldBlip = API.createBlip(afterLoc);
        API.setBlipSprite(oldBlip, 1);
        API.setBlipColor(oldBlip, 47);
    }
}

Event.OnUpdate.connect(function () {
    if (menuPool != null) {
        menuPool.ProcessMenus();
    }

    if (isInTest == true) {
        if (API.getEntityPosition(player).DistanceTo(checkpoints[nextCheckpoint]) <= 5.0) {
            API.playSoundFrontEnd("GOLF_NEW_RECORD", "HUD_AWARDS");
            ApplyNextCheckpoint();
        }
    }
});