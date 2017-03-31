/// <reference path="../types-gtanetwork/index.d.ts" />

var menu_pool;
var currentVehicleList;

function VehicleJSONToMenu(json, type) {
    var realArr = API.fromJson(json);
    var list = API.createMenu("Vehicle Dealership", type, 0, 0, 6);
    for (var i = 0; i < realArr.length; i++) {
        var item = API.createMenuItem(realArr[0], "This vehicle costs \$${realArr[2]}.");
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
            var motorsycles = API.createMenuItem("Motorsycles", "All 2 wheel vehicles."); vehDealerList.AddItem(motorsycles);
            var copues = API.createMenuItem("Copues", "Normal Class Vehicles."); vehDealerList.AddItem(copues);
            var trucksnvans = API.createMenuItem("Trucks and Vans", "Big vehicles."); vehDealerList.AddItem(trucksnvans);
            var offroad = API.createMenuItem("Offroard", "Vehicles that can go offroard."); vehDealerList.AddItem(offroad);
            var musclecars = API.createMenuItem("Muscle Cars", "Powerful cars ?!."); vehDealerList.AddItem(musclecars);
            var suv = API.createMenuItem("SUV", "SUV."); vehDealerList.AddItem(suv);
            var supercars = API.createMenuItem("Supercars", "The best cars we have."); vehDealerList.AddItem(supercars);
            menu_pool.Add(vehDealerList);

            //Setup submenu for showing rest of cars.
            menu_pool.Add(currentVehicleList);

            //Show it.
            vehDealerList.Visible = true;

            //Listen for click: 
            vehDealerList.OnItemSelect.connect(function (sender, item, index) {
                //Show apporpriate list depending on index.
                vehDealerList.Visible = false;
                currentVehicleList = VehicleJSONToMenu(args[index], item.Text);
                currentVehicleList.Visible = true;
            });
            break;
    }
});

//On Vehicle select: 
vehDealerList.OnItemSelect.connect(function (sender, item, index) {
    //Send event to server about selected car.
    API.triggerServerEvent("vehicledealer_selectcar", sender.Text, item.Text);
});

API.onUpdate.connect(function () {
    if (menu_pool != null) {
        menu_pool.ProcessMenus();
    }
});