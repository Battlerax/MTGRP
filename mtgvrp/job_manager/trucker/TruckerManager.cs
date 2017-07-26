using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;

namespace mtgvrp.job_manager.trucker
{
    public class TruckerManager : Script
    {
        public TruckerManager()
        {
            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;
            API.onPlayerExitVehicle += API_onPlayerExitVehicle;
            API.onEntityEnterColShape += API_onEntityEnterColShape;
            API.onPlayerDisconnected += API_onPlayerDisconnected;
        }

        private void API_onPlayerDisconnected(Client player, string reason)
        {
            Character c = player.GetCharacter();

            if (c == null)
                return;

            if (c.TruckingStage != Character.TruckingStages.None)
            {
                CancelRun(player);
            }
        }

        private void API_onPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            Character c = player.GetCharacter();
            if (c.TruckingStage != Character.TruckingStages.None && API.getEntityData(vehicle, "TRUCKER_DRIVER") == player)
            {
                API.sendChatMessageToPlayer(player, "You have a minute to get back into the truck before its cancelled.");
                API.setEntityData(player, "TRUCKING_CANCELTIMER", new Timer(state => CancelRun(player), null, 60000, Timeout.Infinite));
                return;
            }
        }

        private void API_onEntityEnterColShape(ColShape colshape, NetHandle entity)
        {
            if (API.getEntityType(entity) != EntityType.Player)
                return;

            var player = API.getPlayerFromHandle(entity);
            var character = player.GetCharacter();

            if (character.JobOne?.Type != JobManager.JobTypes.Trucker)
                return;

            if (player.isInVehicle && character.TruckingStage == Character.TruckingStages.HeadingForFuelSupplies)
            {
                if (character.JobOne.MiscOne.ColZone == colshape) //Fuel load.
                {
                    var veh = player.vehicle;
                    var vehicle = veh.handle.GetVehicle();

                    if (vehicle.Job?.Type != JobManager.JobTypes.Trucker ||
                        character.JobOne?.Type != JobManager.JobTypes.Trucker) return;

                    if (API.getEntityData(veh, "TRUCKER_DRIVER") != player)
                    {
                        API.sendChatMessageToPlayer(player, "~r~[Trucking]~w~ You aren't in your truck.");
                        return;
                    }

                    API.triggerClientEvent(player, "update_beacon", new Vector3());

                    Property needsGasProp = null;
                    foreach (var prop in PropertyManager.Properties.Where(x => x.Type == PropertyManager.PropertyTypes.GasStation))
                    {
                        if (needsGasProp == null && prop.Supplies < Property.MaxGasSupplies)
                            needsGasProp = prop;

                        if (needsGasProp != null && prop.Supplies < needsGasProp.Supplies)
                        {
                            needsGasProp = prop;
                        }
                    }
                    if (needsGasProp == null)
                    {
                        API.sendChatMessageToPlayer(player, "No gas stations need fuel at the moment.");
                        return;
                    }

                    player.freeze(true);
                    API.sendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Please wait 10 seconds while your truck is loading fuel...");

                    API.setEntityData(player, "TRUCKING_LOAD_TIMER", new Timer(state =>
                    {
                        character.TruckingStage = Character.TruckingStages.DeliveringFuel;
                        player.freeze(false);

                        API.triggerClientEvent(player, "update_beacon", needsGasProp.EntranceMarker.Location);
                        
                        API.setEntityData(player, "TRUCKING_TARGETGAS", needsGasProp);

                        API.sendChatMessageToPlayer(player,
                            "~r~[Trucking]~w~ Your truck have been loaded, head to the checkpoint to deliver them.");
                        API.resetEntityData(player, "TRUCKING_LOAD_TIMER");

                    }, null, 10000, Timeout.Infinite));
                }
            }
            else if (player.isInVehicle && character.TruckingStage == Character.TruckingStages.HeadingForWoodSupplies)
            {
                if (JobManager.GetJobById(character.JobZone)?.Type == JobManager.JobTypes.Lumberjack &&
                    character.JobZoneType == 3)
                {
                    var veh = player.vehicle;
                    var vehicle = veh.handle.GetVehicle();

                    if (vehicle.Job?.Type != JobManager.JobTypes.Trucker ||
                        character.JobOne?.Type != JobManager.JobTypes.Trucker) return;

                    if (API.getEntityData(veh, "TRUCKER_DRIVER") != player)
                    {
                        API.sendChatMessageToPlayer(player, "~r~[Trucking]~w~ You aren't in your truck.");
                        return;
                    }

                    API.triggerClientEvent(player, "update_beacon", new Vector3());

                    if (SettingsManager.Settings.WoodSupplies < 50)
                    {
                        API.sendChatMessageToPlayer(player, "There is no any wood to load.");
                        return;
                    }

                    player.freeze(true);
                    API.sendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Please wait 10 seconds while your truck is loading supplies...");

                    API.setEntityData(player, "TRUCKING_LOAD_TIMER", new Timer(state =>
                    {
                        character.TruckingStage = Character.TruckingStages.DeliveringWood;
                        player.freeze(false);

                        API.triggerClientEvent(player, "update_beacon", character.JobOne.MiscTwo.Location);
                        
                        API.sendChatMessageToPlayer(player,
                            "~r~[Trucking]~w~ Your truck have been loaded, head to the checkpoint to deliver them.");
                        API.resetEntityData(player, "TRUCKING_LOAD_TIMER");
                        SettingsManager.Settings.WoodSupplies -= 50;

                    }, null, 10000, Timeout.Infinite));
                }
            }
            else if (character.TruckingStage == Character.TruckingStages.DeliveringWood && player.isInVehicle)
            {
                if (JobManager.GetJobById(character.JobZone)?.Type == JobManager.JobTypes.Trucker &&
                    character.JobZoneType == 3)
                {
                    var veh = player.vehicle;
                    var vehicle = veh.handle.GetVehicle();

                    if (vehicle.Job?.Type != JobManager.JobTypes.Trucker ||
                        character.JobOne?.Type != JobManager.JobTypes.Trucker) return;

                    if (API.getEntityData(veh, "TRUCKER_DRIVER") != player)
                    {
                        API.sendChatMessageToPlayer(player, "~r~[Trucking]~w~ You aren't in your truck.");
                        return;
                    }

                    API.triggerClientEvent(player, "update_beacon", new Vector3());

                    player.freeze(true);
                    API.sendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Please wait 10 seconds while your truck is unloading...");

                    API.setEntityData(player, "TRUCKING_UNLOAD_TIMER", new Timer(state =>
                    {
                        character.TruckingStage = Character.TruckingStages.HeadingBack;
                        player.freeze(false);

                        API.triggerClientEvent(player, "update_beacon", character.JobOne.JoinPos.Location);
                        
                        API.sendChatMessageToPlayer(player,
                            "~r~[Trucking]~w~ Your truck have been unloaded, head to the checkpoint finish your run.");
                        API.resetEntityData(player, "TRUCKING_UNLOAD_TIMER");
                        SettingsManager.Settings.TruckerSupplies += 50;

                    }, null, 10000, Timeout.Infinite));
                }
            }
            else if (character.TruckingStage == Character.TruckingStages.DeliveringFuel && player.isInVehicle)
            {
                Property prop = player.getData("TRUCKING_TARGETGAS");
                if (prop == null)
                    return;

                if (prop.EntranceMarker.ColZone == colshape)
                {

                    var veh = player.vehicle;
                    var vehicle = veh.handle.GetVehicle();

                    if (vehicle.Job?.Type != JobManager.JobTypes.Trucker ||
                        character.JobOne?.Type != JobManager.JobTypes.Trucker) return;

                    if (API.getEntityData(veh, "TRUCKER_DRIVER") != player)
                    {
                        API.sendChatMessageToPlayer(player, "~r~[Trucking]~w~ You aren't in your truck.");
                        return;
                    }

                    API.triggerClientEvent(player, "update_beacon", new Vector3());

                    player.freeze(true);
                    API.sendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Please wait 10 seconds while your truck is unloading...");

                    API.setEntityData(player, "TRUCKING_UNLOAD_TIMER", new Timer(state =>
                    {
                        character.TruckingStage = Character.TruckingStages.HeadingBack;
                        player.freeze(false);

                        API.triggerClientEvent(player, "update_beacon", character.JobOne.JoinPos.Location);
                        

                        API.sendChatMessageToPlayer(player,
                            "~r~[Trucking]~w~ Your truck have been unloaded, head to the checkpoint finish your run.");
                        API.resetEntityData(player, "TRUCKING_UNLOAD_TIMER");
                        prop.Supplies += 20;

                    }, null, 10000, Timeout.Infinite));
                }
            }
            else if (character.TruckingStage == Character.TruckingStages.HeadingBack && player.isInVehicle && character.JobOne.JoinPos.ColZone == colshape)
            {
                var veh = player.vehicle;
                var vehicle = veh.handle.GetVehicle();

                if (vehicle.Job?.Type != JobManager.JobTypes.Trucker ||
                    character.JobOne?.Type != JobManager.JobTypes.Trucker) return;

                if (API.getEntityData(veh, "TRUCKER_DRIVER") != player)
                {
                    API.sendChatMessageToPlayer(player, "~r~[Trucking]~w~ You aren't in your truck.");
                    return;
                }

                if (player.getData("TRUCKING_TYPE") == "supplies")
                {
                    player.sendChatMessage("You have been paid ~g~$3200.");
                    InventoryManager.GiveInventoryItem(character, new Money(), 3200, true);
                }
                else if (player.getData("TRUCKING_TYPE") == "gas")
                {
                    player.sendChatMessage("You have been paid ~g~$1500.");
                    InventoryManager.GiveInventoryItem(character, new Money(), 1500, true);
                }

                API.triggerClientEvent(player, "update_beacon", new Vector3());
                CancelRun(player);
            }

        }

        [Command("startrun"), Help(HelpManager.CommandGroups.TruckerJob, "Start a trucker run.", "Run type [gas/supplies]")]
        public void StartRun(Client player, string type)
        {
            var character = player.GetCharacter();
            if (character.TruckingStage == Character.TruckingStages.GettingTrailer)
            {
                if (type == "gas")
                {
                    Property needsGasProp = null;
                    foreach (var prop in PropertyManager.Properties.Where(x => x.Type == PropertyManager.PropertyTypes.GasStation))
                    {
                        if (needsGasProp == null && prop.Supplies < Property.MaxGasSupplies)
                            needsGasProp = prop;

                        if (needsGasProp != null && prop.Supplies < needsGasProp.Supplies)
                        {
                            needsGasProp = prop;
                        }
                    }
                    if (needsGasProp == null)
                    {
                        API.sendChatMessageToPlayer(player, "No gas stations need fuel at the moment.");
                        return;
                    }

                    API.triggerClientEvent(player, "update_beacon", character.JobOne.MiscOne.Location);
                    
                    API.sendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Head to the checkpoint to load your truck with fuel.");
                    player.GetCharacter().TruckingStage = Character.TruckingStages.HeadingForFuelSupplies;
                    API.setEntityData(player, "TRUCKING_TYPE", "gas");
                }
                else if (type == "supplies")
                {
                    var job = JobManager.Jobs.FirstOrDefault(x => x.Type == JobManager.JobTypes.Lumberjack);
                    if (job == null)
                        return;

                    API.triggerClientEvent(player, "update_beacon", job.MiscTwo.Location);
                    
                    API.sendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Head to the checkpoint to load your truck with supplies.");
                    player.GetCharacter().TruckingStage = Character.TruckingStages.HeadingForWoodSupplies;
                    API.setEntityData(player, "TRUCKING_TYPE", "supplies");
                }
            }
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            var veh = vehicle.GetVehicle();
            var character = player.GetCharacter();

            if (veh.Job?.Type == JobManager.JobTypes.Trucker && character.JobOne?.Type == JobManager.JobTypes.Trucker)
            {
                if (API.hasEntityData(player, "TRUCKING_CANCELTIMER") &&
                    API.getEntityData(vehicle, "TRUCKER_DRIVER") == player)
                {
                    Timer timer = API.getEntityData(player, "TRUCKING_CANCELTIMER");
                    timer.Dispose();
                    API.resetEntityData(player, "TRUCKING_CANCELTIMER");
                    API.sendChatMessageToPlayer(player, "You've got back into the truck.");
                    return;
                }

                if (API.getEntityData(vehicle, "TRUCKER_DRIVER") == player)
                    return;

                if (character.TruckingStage != Character.TruckingStages.None)
                {
                    API.warpPlayerOutOfVehicle(player);
                    return;
                }

                character.TruckingStage = Character.TruckingStages.GettingTrailer;
                API.setEntityData(vehicle, "TRUCKER_DRIVER", player);
                API.setEntityData(player, "TRUCKER_VEHICLE", vehicle);
                API.sendChatMessageToPlayer(player, "~r~[Trucking]~w~ Decide what to deliver using ~g~/startrun [supplies/gas].");
            }
        }

        [Command("canceltruck"), Help(HelpManager.CommandGroups.TruckerJob, "Cancels the current trucker run")]
        public void CancelRun(Client player)
        {
            if (player.GetCharacter().TruckingStage == Character.TruckingStages.None)
                return;

            //Respawn cars and warp out.
            player.GetCharacter().TruckingStage = Character.TruckingStages.None;
            NetHandle veh = API.getEntityData(player, "TRUCKER_VEHICLE");

            if (player.vehicle != null && player.vehicle == veh)
                API.warpPlayerOutOfVehicle(player);

            if (!veh.IsNull)
            {
                VehicleManager.respawn_vehicle(veh.GetVehicle());
            }
            
            //Reset Timers.
            Timer timer = API.getEntityData(player, "TRUCKING_CANCELTIMER");
            timer?.Dispose();
            timer = API.getEntityData(player, "TRUCKING_UNLOAD_TIMER");
            timer?.Dispose();
            timer = API.getEntityData(player, "TRUCKING_LOAD_TIMER");
            timer?.Dispose();
            API.resetEntityData(player, "TRUCKING_CANCELTIMER");
            API.resetEntityData(player, "TRUCKING_UNLOAD_TIMER");
            API.resetEntityData(player, "TRUCKING_LOAD_TIMER");

            //Reset other variables
            API.resetEntityData(player, "TRUCKER_VEHICLE");
            API.resetEntityData(veh, "TRUCKER_DRIVER");

            //Unfreeze and send message.
            API.freezePlayer(player, false);
            API.sendChatMessageToPlayer(player, "The trucking run has been done or cancelled.");
        }

        [Command("supplydemand"), Help(HelpManager.CommandGroups.TruckerJob, "Check the current supply and gas status.")]
        public void CheckDemand(Client player)
        {

            double fuel = 0;
            double maxfuel = PropertyManager.Properties.Count(x => x.Type == PropertyManager.PropertyTypes.GasStation) *
                          Property.MaxGasSupplies;

            PropertyManager.Properties.Where(x => x.Type == PropertyManager.PropertyTypes.GasStation).AsParallel()
                .ForAll(x => fuel += x.Supplies);

            API.sendChatMessageToPlayer(player,
                "The amount of supplies available at deliverymens: " +
                SettingsManager.Settings.TruckerSupplies);

            if (maxfuel == 0)
                API.sendChatMessageToPlayer(player, "The amount of fuel availale in gas stations: 100%");
            else
                API.sendChatMessageToPlayer(player,
                    "The amount of fuel availale in gas stations: " + Math.Round((fuel / maxfuel) * 100) + "%");
        }
    }
}