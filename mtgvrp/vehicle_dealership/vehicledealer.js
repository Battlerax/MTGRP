/// <reference path="../types-gtanetwork/index.d.ts" />

var vehDealerList;
var currentVehicleList;
var currentVeh;

function VehicleJSONToMenu(json, type) {
    var realArr = JSON.parse(json);
    var list = API.createMenu("Vehicle Dealership", type, 0, 0, 6);
    for (var i = 0; i < realArr.length; i++) {
        var item = API.createMenuItem(realArr[i][0], `This vehicle costs \$${realArr[i][2]}.`);
        list.AddItem(item);
    }
    return list;
}

//Events.
API.onServerEventTrigger.connect((eventName, args) => {
    switch(eventName) {
        case "dealership_showbuyvehiclemenu":
            //Create main list.
            //TODO: proabably change this descriptions xD
            vehDealerList = API.createMenu("Vehicle Dealership", "Welcome to the vehicle dealership.", 0, 0, 6);
            var motorsycles = API.createMenuItem("Motorcycles", "All 2 wheel vehicles.");
            var coupes = API.createMenuItem("Coupes", "Normal Class Vehicles.");
            var trucksnvans = API.createMenuItem("Trucks and Vans", "Big vehicles.");
            var offroad = API.createMenuItem("Offroad", "Vehicles that can go offroard.");
            var musclecars = API.createMenuItem("Muscle Cars", "Powerful cars ?!.");
            var suv = API.createMenuItem("SUV", "SUV.");
            var supercars = API.createMenuItem("Supercars", "The best cars we have.");
			var cycles = API.createMenuItem("Cycles", "Ride bikes in style.");
            //NOTE: THE ARRENGNEMENT IS SOOO IMPORTANT CAUSE I USE INDEX FOR KNOWING THE CURRENT GROUP AND NOT NAME.
            vehDealerList.AddItem(motorsycles);
            vehDealerList.AddItem(coupes);
            vehDealerList.AddItem(trucksnvans);
            vehDealerList.AddItem(offroad);
            vehDealerList.AddItem(musclecars);
            vehDealerList.AddItem(suv);
            vehDealerList.AddItem(supercars);
			vehDealerList.AddItem(cycles);

            //Show it.
            vehDealerList.Visible = true;

            //Set.
            API.setEntityPositionFrozen(API.getLocalPlayer(), true);
            var newCamera = API.createCamera(new Vector3(223.5987, -990.639, -96.99989), new Vector3(0, 0, 0));
            API.setActiveCamera(newCamera);
            API.pointCameraAtPosition(newCamera, new Vector3(230.5009, -990.5709, -99.49818));
            API.callNative("_SET_FOCUS_AREA", 230.5009, -990.5709, -99.49818, 0.0, 0.0, 0.0);

            API.sendChatMessage("~g~NOTE: You can use the PLUS and MINUS keys to rotate your vehicle!");

            //Listen for click: 
            vehDealerList.OnItemSelect.connect(function (sender, item, index) {
                //Show apporpriate list depending on index.
                vehDealerList.Visible = false;
                currentVehicleList = VehicleJSONToMenu(args[index], item.Text);
                currentVehicleList.Visible = true;
                var realArr = JSON.parse(args[index]);

                currentVehicleList.OnItemSelect.connect(function (csender, citem, cindex) {
                    //Send event to server about selected car.
                    API.triggerServerEvent("vehicledealer_selectcar", index, cindex);
                });

                currentVehicleList.OnIndexChange.connect(function (osender, oindex) {
                    if (currentVeh != null)
                        API.deleteEntity(currentVeh);

                    currentVeh = API.createVehicle(parseInt(realArr[oindex][1]), new Vector3(230.5009, -990.5709, -99.49818), new Vector3(0.03913954, -0.07241886, 179.1236));
                });

                currentVehicleList.OnMenuClose.connect(function (closesender) {
                    vehDealerList.Visible = true;
                });
            });

            vehDealerList.OnMenuClose.connect(function (closesender) {
                API.setEntityPositionFrozen(API.getLocalPlayer(), false);
                if (currentVeh != null)
                    API.deleteEntity(currentVeh);
                API.callNative("CLEAR_FOCUS");
                API.setActiveCamera(null);
                vehDealerList = null;
                currentVehicleList = null;
            });
            break;

        case "dealership_exitdealermenu":
            API.setEntityPositionFrozen(API.getLocalPlayer(), false);
            if (currentVeh != null)
                API.deleteEntity(currentVeh);
            API.callNative("CLEAR_FOCUS");
            API.setActiveCamera(null);
            vehDealerList = null;
            currentVehicleList = null;
            break;
    }
});

API.onKeyDown.connect(function(sender, e) {
    if (e.KeyCode == Keys.Oemplus && currentVeh != null) {
        var rot = API.getEntityRotation(currentVeh).Add(new Vector3(0, 0, 4));
        API.setEntityRotation(currentVeh, rot);

    }
    else if (e.KeyCode == Keys.OemMinus && currentVeh != null) {
        var newRot = API.getEntityRotation(currentVeh).Add(new Vector3(0, 0, -4));
        API.setEntityRotation(currentVeh, newRot);
    }
});

API.onUpdate.connect(function () {
    if (vehDealerList != null)
        API.drawMenu(vehDealerList);
    if (currentVehicleList != null)
        API.drawMenu(currentVehicleList);
});