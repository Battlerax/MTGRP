using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.core;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.vehicle_manager
{
    class VehicleOwnership : Script
    {
        public VehicleOwnership()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            Character character = sender.GetCharacter();
            switch (eventName)
            {
                case "myvehicles_locatecar":
                    vehicle_manager.Vehicle lcVeh =
                        VehicleManager.Vehicles.Single(x => x.NetHandle.Value == Convert.ToInt32(arguments[0]) && x.OwnerId == character.Id);
                    Vector3 loc = API.getEntityPosition(lcVeh.NetHandle);
                    API.triggerClientEvent(sender, "myvehicles_setCheckpointToCar", loc.X, loc.Y, loc.Z);
                    API.sendChatMessageToPlayer(sender, "A checkpoint has been set to the vehicle.");
                    break;

                case "myvehicles_abandoncar":
                    vehicle_manager.Vehicle acVeh =
                        VehicleManager.Vehicles.Single(x => x.Id == Convert.ToInt32(arguments[0]) && x.OwnerId == character.Id);
                    VehicleManager.despawn_vehicle(acVeh);
                    VehicleManager.delete_vehicle(acVeh);
                    acVeh.Delete();
                    character.OwnedVehicles.Remove(acVeh.Id);
                    API.sendChatMessageToPlayer(sender, $"You have sucessfully abandoned your ~r~{API.getVehicleDisplayName(acVeh.VehModel)}~w~");
                    break;

                case "myvehicles_sellcar":
                    vehicle_manager.Vehicle scVeh =
                        VehicleManager.Vehicles.Single(x => x.Id == Convert.ToInt32(arguments[0]) && x.OwnerId == character.Id);
                    var tid = (string)arguments[1];
                    var target = PlayerManager.ParseClient(tid);
                    if (target == null)
                    {
                        API.sendChatMessageToPlayer(sender, "That player isn't online or doesn't exist.");
                        return;
                    }
                    var targetChar = target.GetCharacter();
                    API.sendChatMessageToPlayer(sender,
                        $"Are you sure you would like to sell the ~r~{API.getVehicleDisplayName(scVeh.VehModel)}~w~ for ~r~${arguments[2]}~w~ to the player ~r~{targetChar.CharacterName}~w~");
                    API.sendChatMessageToPlayer(sender, "Use /confirmsellvehicle to sell.");
                    API.setEntityData(sender, "sellcar_selling", new[] {scVeh, targetChar, arguments[2]});
                    break;
            }
        }

        [Command("myvehicles")]
        public void myvehicles_cmd(Client player)
        {
            //Get all owned vehicles and send them.
            Character character = player.GetCharacter();
            string[][] cars = VehicleManager.Vehicles
                .Where(x => x.OwnerId == character.Id)
                .Select(x => new [] { API.getVehicleDisplayName(x.VehModel), x.Id.ToString(), x.NetHandle.Value.ToString()}).ToArray();

            API.triggerClientEvent(player, "myvehicles_showmenu", API.toJson(cars));
        }

        [Command("confirmsellvehicle")]
        public void confirmsellvehicle_cmd(Client player)
        {
            Character character = player.GetCharacter();
            var data = API.getEntityData(player, "sellcar_selling");
            if (data != null)
            {
                Vehicle veh = data[0]; Character target = data[1]; string price = data[2];
                API.setEntityData(target.Client, "sellcar_buying", new dynamic[] {character, veh, price});
                API.setEntityData(player, "sellcar_selling", null);
                API.sendChatMessageToPlayer(target.Client, $"~r~{character.CharacterName}~w~ has offered to sell you a ~r~{API.getVehicleDisplayName(veh.VehModel)}~w~ for ~r~${price}~w~.");
                API.sendChatMessageToPlayer(target.Client, "Use /confirmbuyvehicle to buy it.");
                API.sendChatMessageToPlayer(player, "Request sent.");
            }
            else
                API.sendChatMessageToPlayer(player, "You aren't selling any car.");
        }

        [Command("confirmbuyvehicle")]
        public void confirmbuyvehicle_cmd(Client player)
        {
            Character character = player.GetCharacter();
            Account account = player.GetAccount();
            var data = API.getEntityData(player, "sellcar_buying");
            if (data != null)
            {
                Character buyingFrom = data[0]; Vehicle veh = data[1]; int price = Convert.ToInt32(data[2]);
                //Make sure near him.
                var buyingPos = buyingFrom.Client.position;
                if (player.position.DistanceTo(buyingPos) <= 5f)
                {
                    //make sure still have slots.
                    if (character.OwnedVehicles.Count < VehicleManager.GetMaxOwnedVehicles(character.Client))
                    {
                        //make sure have money.
                        if (character.Money >= price)
                        {
                            //Do actual process.
                            buyingFrom.Money += price;
                            character.Money -= price;
                            veh.OwnerId = character.Id;
                            buyingFrom.OwnedVehicles.Remove(veh.Id);
                            character.OwnedVehicles.Add(veh.Id);

                            //DONE, now spawn if hes vip.
                            if (!veh.IsSpawned)
                            {
                                //He won't be able to buy it anyways if he wasn't VIP... so I THINK he can now have it spawned, right ? :/
                                if (VehicleManager.spawn_vehicle(veh) != 1)
                                    API.consoleOutput($"There was an error spawning vehicle #{veh.Id} of {character.CharacterName}.");
                            }

                            //Tell.
                            API.sendChatMessageToPlayer(player, "You have sucessfully bought the car.");
                            API.sendChatMessageToPlayer(buyingFrom.Client, "You have successfully sold the car.");
                            API.setEntityData(player, "sellcar_buying", null);
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, "You don't have enough money.");
                            API.sendChatMessageToPlayer(buyingFrom.Client, "The buyer doesn't have enough money.");
                            API.setEntityData(player, "sellcar_buying", null);
                        }
                    }
                    else {
                        API.sendChatMessageToPlayer(player, "You can't own anymore vehicles.");
                        API.sendChatMessageToPlayer(buyingFrom.Client, "The buyer can't own anymore vehicles.");
                        API.setEntityData(player, "sellcar_buying", null);
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "You must be near the buyer.");
                    API.sendChatMessageToPlayer(buyingFrom.Client, "The buyer must be near you.");
                    API.setEntityData(player, "sellcar_buying", null);
                }
            }
            else
                API.sendChatMessageToPlayer(player, "You aren't buying any car.");
        }
    }
}
