/// <reference path="../types-gtanetwork/index.d.ts" />

var menu_pool;

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
            menu_pool = API.getMenuPool();
            //TODO: proabably change this descriptions xD
            var vehDealerList = API.createMenu("Vehicle Dealership", "Welcome to the vehicler dealership.", 0, 0, 6);
            var motorsycles = API.createMenuItem("Motorsycles", "All 2 wheel vehicles.");
            var copues = API.createMenuItem("Copues", "Normal Class Vehicles.");
            var trucksnvans = API.createMenuItem("Trucks and Vans", "Big vehicles.");
            var offroad = API.createMenuItem("Offroad", "Vehicles that can go offroard.");
            var musclecars = API.createMenuItem("Muscle Cars", "Powerful cars ?!.");
            var suv = API.createMenuItem("SUV", "SUV.");
            var supercars = API.createMenuItem("Supercars", "The best cars we have.");
            //NOTE: THE ARRENGNEMENT IS SOOO IMPORTANT CAUSE I USE INDEX FOR KNOWING THE CURRENT GROUP AND NOT NAME.
            vehDealerList.AddItem(motorsycles);
            vehDealerList.AddItem(copues);
            vehDealerList.AddItem(trucksnvans);
            vehDealerList.AddItem(offroad);
            vehDealerList.AddItem(musclecars);
            vehDealerList.AddItem(suv);
            vehDealerList.AddItem(supercars);
            menu_pool.Add(vehDealerList);

            //Show it.
            vehDealerList.Visible = true;

            //Freeze.
            API.setEntityPositionFrozen(API.getLocalPlayer(), true);

            //Listen for click: 
            vehDealerList.OnItemSelect.connect(function (sender, item, index) {
                //Show apporpriate list depending on index.
                vehDealerList.Visible = false;
                var currentVehicleList = VehicleJSONToMenu(args[index], item.Text);
                menu_pool.Add(currentVehicleList);
                currentVehicleList.Visible = true;

                currentVehicleList.OnItemSelect.connect(function (csender, citem, cindex) {
                    //Send event to server about selected car.
                    API.triggerServerEvent("vehicledealer_selectcar", index, cindex);
                });

                currentVehicleList.OnMenuClose.connect(function (closesender) {
                    vehDealerList.Visible = true;
                });
            });
            break;
    }
});

API.onUpdate.connect(function () {
    if (menu_pool != null) {
        menu_pool.ProcessMenus();
    }
});