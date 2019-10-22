/// <reference path="../types-gtanetwork/index.d.ts" />

var vehDealerList;
var currentVehicleList;
var currentVeh;

function VehicleJSONToMenu(json, type) {
    var realArr = JSON.parse(json);
    var list = API.createMenu("Boat Dealership", type, 0, 0, 6);
    for (var i = 0; i < realArr.length; i++) {
        var item = API.createMenuItem(realArr[i][0], `This vehicle costs \$${realArr[i][2]}.`);
        list.AddItem(item);
    }
    return list;
}

//Events.
Event.OnServerEventTrigger.connect((eventName, args) => {
    switch(eventName) {
        case "dealership_showbuyboatmenu":
            //Create main list.
            //TODO: proabably change this descriptions xD
            vehDealerList = API.createMenu("Boat Dealership", "Welcome to the boat dealership.", 0, 0, 6);
            var boats = API.createMenuItem("Boats", "We're famous for our selection of boats!");
            //NOTE: THE ARRENGNEMENT IS SOOO IMPORTANT CAUSE I USE INDEX FOR KNOWING THE CURRENT GROUP AND NOT NAME.
            vehDealerList.AddItem(boats);

            //Show it.
            vehDealerList.Visible = true;

            //Set.
            API.setEntityPositionFrozen(API.getLocalPlayer(), true);
            var newCamera = API.createCamera(new Vector3(223.5987, -990.639, -96.99989), new Vector3(0, 0, 0));
            API.setActiveCamera(newCamera);
            API.pointCameraAtPosition(newCamera, new Vector3(230.5009, -990.5709, -99.49818));
	        API.callNative("13507514344510389797", 230.5009, -990.5709, -99.49818, 0.0, 0.0, 0.0); //STREAMING::_SET_FOCUS_AREA

            API.SendChatMessage("~g~NOTE: You can use the PLUS and MINUS keys to rotate your vehicle!");

            //Listen for click: 
            vehDealerList.OnItemSelect.connect(function (sender, item, index) {
                //Show apporpriate list depending on index.
                vehDealerList.Visible = false;
                currentVehicleList = VehicleJSONToMenu(args[index], item.Text);
                currentVehicleList.Visible = true;

                currentVehicleList.OnItemSelect.connect(function (csender, citem, cindex) {
                    //Send event to server about selected car.
                    API.triggerServerEvent("boatdealer_selectcar", index, cindex);
                });

                currentVehicleList.OnIndexChange.connect(function (osender, oindex) {
                    var realArr = JSON.parse(args[index]);
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
                API.callNative("3582399230505917858"); //STREAMING::CLEAR_FOCUS
                API.setActiveCamera(null);
                vehDealerList = null;
                currentVehicleList = null;
            });
            break;

        case "dealership_exitdealermenu":
            API.setEntityPositionFrozen(API.getLocalPlayer(), false);
            if (currentVeh != null)
                API.deleteEntity(currentVeh);
            API.callNative("3582399230505917858"); //STREAMING::CLEAR_FOCUS
            API.setActiveCamera(null);
            vehDealerList = null;
            currentVehicleList = null;
            break;
    }
});

Event.OnKeyDown.connect(function(sender, e) {
    if (e.KeyCode == Keys.Oemplus && currentVeh != null) {
        var rot = API.getEntityRotation(currentVeh).Add(new Vector3(0, 0, 4));
        API.setEntityRotation(currentVeh, rot);

    }
    else if (e.KeyCode == Keys.OemMinus && currentVeh != null) {
        var newRot = API.getEntityRotation(currentVeh).Add(new Vector3(0, 0, -4));
        API.setEntityRotation(currentVeh, newRot);
    }
});

Event.OnUpdate.connect(function () {
    if (vehDealerList != null)
        API.drawMenu(vehDealerList);
    if (currentVehicleList != null)
        API.drawMenu(currentVehicleList);
});