var menu_pool = null;
var vehicle_menu = null;

var door_index = 0;

API.onPlayerExitVehicle.connect(function (player, vehicle) {
    if (vehicle_menu != null) {
        if (vehicle_menu.Visible == true) {
            vehicle_menu.Visible = false;
        }
    }
});

API.onKeyDown.connect(function(Player, args){
    if (args.KeyCode == Keys.N && !API.isChatOpen() && resource.MDC.mdcBrowser == null) {
        if (vehicle_menu == null || vehicle_menu.Visible == false) {
            
            var player = API.getLocalPlayer();

            if (API.isPlayerInAnyVehicle(player) == false)
                return;


            menu_pool = API.getMenuPool();

           
            var player_veh = API.getPlayerVehicle(player);
            var player_seat = API.getPlayerVehicleSeat(player);

            var veh_info = API.getEntitySyncedData(player, "CurrentVehicleInfo");
            var player_owns_veh = API.getEntitySyncedData(player, "OwnsVehicle");
            var canparkcar = API.getEntitySyncedData(player, "CanParkCar");

            vehicle_menu = API.createMenu("Vehicle Interaction", veh_info, 0, 0, 3);

            var engine_state_item = null;
            var lock_state_item = null;
            var park_car_item = null;
            var door_item = null;

            if (player_seat == -1) { //Only show engine & parking option for driver
                if (player_owns_veh == true) {
                    engine_state_item = API.createMenuItem("Toggle Engine", "Turn the engine on or off.");
                    
					if(canparkcar)
						park_car_item = API.createMenuItem("Park Car", "Save the vehicles spawn point to its current location");
						
                }
                else {
                    if (API.getVehicleEngineStatus(player_veh) == false) {
                        engine_state_item = API.createMenuItem("Attempt Hotwire", "Attempt to hotwire the vehicle.");
                    }
                }
            }

	        if(canparkcar)
				lock_state_item = API.createMenuItem("Toggle Locks", "Lock or unlock the vehicle.");

            var door_list = new List(String);
            door_list.Add("Front Left");
            door_list.Add("Front Right");
            door_list.Add("Back Left");
            door_list.Add("Back Right");
            door_list.Add("Hood");
            door_list.Add("Trunk");

            door_item = API.createListItem("Door Options", "Open and close the vehicle doors.", door_list, 0);

            if (player_seat == -1 && engine_state_item !== null) {
                vehicle_menu.AddItem(engine_state_item);
            }

            if (lock_state_item !== null) {
                vehicle_menu.AddItem(lock_state_item);
                lock_state_item.Activated.connect(function (menu, item) {
                    API.triggerServerEvent("OnVehicleMenuTrigger", player_veh, "lock");
                });
            }

            if (player_seat == -1 && player_owns_veh == true && park_car_item !== null) {
                vehicle_menu.AddItem(park_car_item);
            }

            vehicle_menu.AddItem(door_item);

            menu_pool.Add(vehicle_menu);
            vehicle_menu.Visible = true;

            //Send this shit to the server cause we can't trust client side for owner information... frickin cheaters
            if (engine_state_item !== null) {
                engine_state_item.Activated.connect(function (menu, item) {
                    API.triggerServerEvent("OnVehicleMenuTrigger", player_veh, "engine");
                });
            }

            if (park_car_item !== null) {
                park_car_item.Activated.connect(function (menu, item) {
                    API.triggerServerEvent("OnVehicleMenuTrigger", player_veh, "park");
                });
            }

            door_item.Activated.connect(function(menu, item) {
                API.triggerServerEvent("OnVehicleMenuTrigger", player_veh, "door", door_index);
            });

            door_item.OnListChanged.connect(function (sender, new_index) {
                door_index = new_index;
            });
        }
        else {
            vehicle_menu.Visible = false;
        }

    }

});

API.onUpdate.connect(function () {
    if (menu_pool != null) {
        menu_pool.ProcessMenus();
    }
});