/// <reference path="../types-gtanetwork/index.d.ts" />

var menuPool;

API.onServerEventTrigger.connect((eventName, args) => {
    switch(eventName) {
        case "myvehicles_showmenu":
            menuPool = API.getMenuPool();
            var myCars = API.createMenu("Your Vehicles", "Select a vehicle to manage.", 0, 0, 4);
            var carsList = JSON.parse(args[0]);
            for (var i = 0; i < carsList.length; i++) {
                var car;
                if (carsList[i][2] == "0")
                    car = API.createMenuItem(carsList[i][0] + " - Unspawned(VIP)", `NetHandle: #${carsList[i][2]} | ID: #${carsList[i][1]}`);
                else
                    car = API.createMenuItem(carsList[i][0], `NetHandle: #${carsList[i][2]} | ID: #${carsList[i][1]}`);
                myCars.AddItem(car);
            }

            menuPool.Add(myCars);
            myCars.Visible = true;

            //Setup submenu.
            var actionsMenu = API.createMenu("Manage Vehicle", `Select an action to do your vehicle.`, 0, 0, 4);
            actionsMenu.AddItem(API.createMenuItem("Locate", "Set a checkpoint to the location of your car."));
            actionsMenu.AddItem(API.createMenuItem("Sell", "Sell your car."));
            menuPool.Add(actionsMenu);

            actionsMenu.OnMenuClose.connect(function (closesender) {
                myCars.Visible = true;
            });

            var currentSelectedCar = -1;
            myCars.OnItemSelect.connect(function (csender, citem, cindex) {
                myCars.Visible = false;
                actionsMenu.Visible = true;
                currentSelectedCar = cindex;
                API.sendChatMessage(`You are managing your ~r~${carsList[cindex][0]}~w~.`);
            });

            actionsMenu.OnItemSelect.connect(function (osender, oitem, oindex) {
                if (currentSelectedCar !== -1) {
                    switch (oindex) {
                        case 0:
                            if (carsList[currentSelectedCar][2] !== "0") {
                                API.triggerServerEvent("myvehicles_locatecar", carsList[currentSelectedCar][2]);
                            } else
                                API.sendChatMessage("You can't locate an unspawned car.");
                            break;
                        case 1:
                            API.sendChatMessage("Sell pressed.");
                            break;
                    }
                }
            });
            break;

        case "myvehicles_setCheckpointToCar":
            API.setWaypoint(args[0], args[1]);
            break;
    }
});

API.onUpdate.connect(function () {
    if (menuPool != null) {
        menuPool.ProcessMenus();
    }
});