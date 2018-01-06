using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GTANetworkAPI;



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
            Event.OnPlayerEnterVehicle += API_onPlayerEnterVehicle;
            Event.OnPlayerExitVehicle += API_onPlayerExitVehicle;
            Event.OnEntityEnterColShape += API_onEntityEnterColShape;
            Event.OnPlayerDisconnected += API_onPlayerDisconnected;
        }

        private static readonly Vector3 TruckerLocationCheck = new Vector3(979.6286,-2532.368, 28.30198);
        private const int PermittedDistance = 150;

        private void API_onPlayerDisconnected(Client player, byte type, string reason)
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
            if (c == null)
                return;

            if (c.TruckingStage != Character.TruckingStages.None && API.GetEntityData(vehicle, "TRUCKER_DRIVER") == player)
            {
                API.SendChatMessageToPlayer(player, "You have a minute to get back into the truck before its cancelled.");
                API.SetEntityData(player, "TRUCKING_CANCELTIMER", new Timer(state => CancelRun(player), null, 60000, Timeout.Infinite));
                return;
            }
        }

        private void API_onEntityEnterColShape(ColShape colshape, NetHandle entity)
        {
            if (API.GetEntityType(entity) != EntityType.Player)
                return;

            var player = API.GetPlayerFromHandle(entity);
            var character = player.GetCharacter();

            if (character?.JobOne?.Type != JobManager.JobTypes.Trucker)
                return;

            if (player.IsInVehicle && character.TruckingStage == Character.TruckingStages.HeadingForFuelSupplies)
            {
                if (character.JobOne.MiscOne.ColZone == colshape) //Fuel load.
                {
                    var veh = player.Vehicle;
                    var vehicle = veh.Handle.GetVehicle();

                    if (vehicle.Job?.Type != JobManager.JobTypes.Trucker ||
                        character.JobOne?.Type != JobManager.JobTypes.Trucker) return;

                    if (API.GetEntityData(veh, "TRUCKER_DRIVER") != player)
                    {
                        API.SendChatMessageToPlayer(player, "~r~[Trucking]~w~ You aren't in your truck.");
                        return;
                    }

                    API.TriggerClientEvent(player, "update_beacon", new Vector3());

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
                        API.SendChatMessageToPlayer(player, "No gas stations need fuel at the moment.");
                        return;
                    }

                    API.TriggerClientEvent(player, "COMPLETE_FREEZE", true);
                    API.SendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Please wait 10 seconds while your truck is loading fuel...");

                    API.SetEntityData(player, "TRUCKING_LOAD_TIMER", new Timer(state =>
                    {
                        character.TruckingStage = Character.TruckingStages.DeliveringFuel;
                        API.TriggerClientEvent(player, "COMPLETE_FREEZE", false);

                        API.TriggerClientEvent(player, "update_beacon", needsGasProp.EntranceMarker.Location);
                        
                        API.SetEntityData(player, "TRUCKING_TARGETGAS", needsGasProp);

                        API.SendChatMessageToPlayer(player,
                            "~r~[Trucking]~w~ Your truck have been loaded, head to the checkpoint to deliver them.");
                        API.ResetEntityData(player, "TRUCKING_LOAD_TIMER");

                    }, null, 10000, Timeout.Infinite));
                }
            }
            else if (player.IsInVehicle && character.TruckingStage == Character.TruckingStages.HeadingForWoodSupplies)
            {
                if (JobManager.GetJobById(character.JobZone)?.Type == JobManager.JobTypes.Lumberjack &&
                    character.JobZoneType == 3)
                {
                    var veh = player.Vehicle;
                    var vehicle = veh.Handle.GetVehicle();

                    if (vehicle.Job?.Type != JobManager.JobTypes.Trucker ||
                        character.JobOne?.Type != JobManager.JobTypes.Trucker) return;

                    if (API.GetEntityData(veh, "TRUCKER_DRIVER") != player)
                    {
                        API.SendChatMessageToPlayer(player, "~r~[Trucking]~w~ You aren't in your truck.");
                        return;
                    }

                    API.TriggerClientEvent(player, "update_beacon", new Vector3());

                    if (SettingsManager.Settings.WoodSupplies < 50)
                    {
                        API.SendChatMessageToPlayer(player, "There isn't any wood to load.");
                        return;
                    }

                    API.TriggerClientEvent(player, "COMPLETE_FREEZE", true);
                    API.SendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Please wait 10 seconds while your truck is loading supplies...");

                    API.SetEntityData(player, "TRUCKING_LOAD_TIMER", new Timer(state =>
                    {
                        character.TruckingStage = Character.TruckingStages.DeliveringWood;
                        API.TriggerClientEvent(player, "COMPLETE_FREEZE", false);

                        API.TriggerClientEvent(player, "update_beacon", character.JobOne.MiscTwo.Location);
                        
                        API.SendChatMessageToPlayer(player,
                            "~r~[Trucking]~w~ Your truck have been loaded, head to the checkpoint to deliver them.");
                        API.ResetEntityData(player, "TRUCKING_LOAD_TIMER");
                        SettingsManager.Settings.WoodSupplies -= 50;

                    }, null, 10000, Timeout.Infinite));
                }
            }
            else if (character.TruckingStage == Character.TruckingStages.DeliveringWood && player.IsInVehicle)
            {
                if (JobManager.GetJobById(character.JobZone)?.Type == JobManager.JobTypes.Trucker &&
                    character.JobZoneType == 3)
                {
                    var veh = player.Vehicle;
                    var vehicle = veh.Handle.GetVehicle();

                    if (vehicle.Job?.Type != JobManager.JobTypes.Trucker ||
                        character.JobOne?.Type != JobManager.JobTypes.Trucker) return;

                    if (API.GetEntityData(veh, "TRUCKER_DRIVER") != player)
                    {
                        API.SendChatMessageToPlayer(player, "~r~[Trucking]~w~ You aren't in your truck.");
                        return;
                    }

                    API.TriggerClientEvent(player, "update_beacon", new Vector3());

                    API.TriggerClientEvent(player, "COMPLETE_FREEZE", true);
                    API.SendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Please wait 10 seconds while your truck is unloading...");

                    API.SetEntityData(player, "TRUCKING_UNLOAD_TIMER", new Timer(state =>
                    {
                        character.TruckingStage = Character.TruckingStages.HeadingBack;
                        API.TriggerClientEvent(player, "COMPLETE_FREEZE", false);

                        API.TriggerClientEvent(player, "update_beacon", character.JobOne.JoinPos.Location);
                        
                        API.SendChatMessageToPlayer(player,
                            "~r~[Trucking]~w~ Your truck have been unloaded, head to the checkpoint finish your run.");
                        API.ResetEntityData(player, "TRUCKING_UNLOAD_TIMER");
                        SettingsManager.Settings.TruckerSupplies += 50;

                    }, null, 10000, Timeout.Infinite));
                }
            }
            else if (character.TruckingStage == Character.TruckingStages.DeliveringFuel && player.IsInVehicle)
            {
                Property prop = player.GetData("TRUCKING_TARGETGAS");
                if (prop == null)
                    return;

                if (prop.EntranceMarker.ColZone == colshape)
                {

                    var veh = player.Vehicle;
                    var vehicle = veh.Handle.GetVehicle();

                    if (vehicle.Job?.Type != JobManager.JobTypes.Trucker ||
                        character.JobOne?.Type != JobManager.JobTypes.Trucker) return;

                    if (API.GetEntityData(veh, "TRUCKER_DRIVER") != player)
                    {
                        API.SendChatMessageToPlayer(player, "~r~[Trucking]~w~ You aren't in your truck.");
                        return;
                    }

                    API.TriggerClientEvent(player, "update_beacon", new Vector3());

                    API.TriggerClientEvent(player, "COMPLETE_FREEZE", true);
                    API.SendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Please wait 10 seconds while your truck is unloading...");

                    API.SetEntityData(player, "TRUCKING_UNLOAD_TIMER", new Timer(state =>
                    {
                        character.TruckingStage = Character.TruckingStages.HeadingBack;
                        API.TriggerClientEvent(player, "COMPLETE_FREEZE", false);

                        API.TriggerClientEvent(player, "update_beacon", character.JobOne.JoinPos.Location);
                        

                        API.SendChatMessageToPlayer(player,
                            "~r~[Trucking]~w~ Your truck have been unloaded, head to the checkpoint finish your run.");
                        API.ResetEntityData(player, "TRUCKING_UNLOAD_TIMER");
                        prop.Supplies += 20;

                    }, null, 10000, Timeout.Infinite));
                }
            }
            else if (character.TruckingStage == Character.TruckingStages.HeadingBack && player.IsInVehicle && character.JobOne.JoinPos.ColZone == colshape)
            {
                var veh = player.Vehicle;
                var vehicle = veh.Handle.GetVehicle();

                if (vehicle.Job?.Type != JobManager.JobTypes.Trucker ||
                    character.JobOne?.Type != JobManager.JobTypes.Trucker) return;

                if (API.GetEntityData(veh, "TRUCKER_DRIVER") != player)
                {
                    API.SendChatMessageToPlayer(player, "~r~[Trucking]~w~ You aren't in your truck.");
                    return;
                }

                if (player.GetData("TRUCKING_TYPE") == "supplies")
                {
                    player.SendChatMessage("You have been paid ~g~$3000.");
                    InventoryManager.GiveInventoryItem(character, new Money(), 3000, true);
                    LogManager.Log(LogManager.LogTypes.Stats, $"[Job] {player.GetCharacter().CharacterName}[{player.GetAccount().AccountName}] has earned $2000 from a trucking run.");
                }
                else if (player.GetData("TRUCKING_TYPE") == "gas")
                {
                    player.SendChatMessage("You have been paid ~g~$1000.");
                    InventoryManager.GiveInventoryItem(character, new Money(), 1000, true);
                    LogManager.Log(LogManager.LogTypes.Stats, $"[Job] {player.GetCharacter().CharacterName}[{player.GetAccount().AccountName}] has earned $900 from a trucking run.");
                }

                API.TriggerClientEvent(player, "update_beacon", new Vector3());
                CancelRun(player);
            }

        }

        [Command("startrun"), Help(HelpManager.CommandGroups.TruckerJob, "Start a trucker run.", "Run type [gas/supplies]")]
        public void StartRun(Client player, string type)
        {
            var character = player.GetCharacter();
            
            if (API.GetPlayerVehicleSeat(player) != -1)
            {
                player.SendChatMessage("You must be the driver of the truck to start the truck run.");
                return;
            }
            if (API.GetEntityPosition(player).DistanceTo(TruckerLocationCheck) > PermittedDistance)
            {
                API.SendChatMessageToPlayer(player,"You need to be at the depot to start a supply run!");
                return;
            }
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
                        API.SendChatMessageToPlayer(player, "No gas stations need fuel at the moment.");
                        return;
                    }

                    API.TriggerClientEvent(player, "update_beacon", character.JobOne.MiscOne.Location);
                    
                    API.SendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Head to the checkpoint to load your truck with fuel.");
                    player.GetCharacter().TruckingStage = Character.TruckingStages.HeadingForFuelSupplies;
                    API.SetEntityData(player, "TRUCKING_TYPE", "gas");
                }
                else if (type == "supplies")
                {
                    var job = JobManager.Jobs.FirstOrDefault(x => x.Type == JobManager.JobTypes.Lumberjack);
                    if (job == null)
                        return;

                    API.TriggerClientEvent(player, "update_beacon", job.MiscTwo.Location);
                    
                    API.SendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Head to the checkpoint to load your truck with supplies.");
                    player.GetCharacter().TruckingStage = Character.TruckingStages.HeadingForWoodSupplies;
                    API.SetEntityData(player, "TRUCKING_TYPE", "supplies");
                }
            }
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle, byte seat)
        {
            var veh = vehicle.GetVehicle();
            var character = player.GetCharacter();

            if (veh?.Job?.Type == JobManager.JobTypes.Trucker && character.JobOne?.Type == JobManager.JobTypes.Trucker)
            {
                if (API.HasEntityData(player, "TRUCKING_CANCELTIMER") &&
                    API.GetEntityData(vehicle, "TRUCKER_DRIVER") == player)
                {
                    Timer timer = API.GetEntityData(player, "TRUCKING_CANCELTIMER");
                    timer.Dispose();
                    API.ResetEntityData(player, "TRUCKING_CANCELTIMER");
                    API.SendChatMessageToPlayer(player, "You've got back into the truck.");
                    return;
                }

                if (API.GetEntityData(vehicle, "TRUCKER_DRIVER") == player)
                    return;

                if (character.TruckingStage != Character.TruckingStages.None)
                {
                    //API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));
                    Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player)); // CONV NOTE: delay fixme
                    return;
                }

                character.TruckingStage = Character.TruckingStages.GettingTrailer;
                API.SetEntityData(vehicle, "TRUCKER_DRIVER", player);
                API.SetEntityData(player, "TRUCKER_VEHICLE", vehicle);
                API.SendChatMessageToPlayer(player, "~r~[Trucking]~w~ Decide what to deliver using ~g~/startrun [supplies/gas].");
            }
        }

        [Command("canceltruck"), Help(HelpManager.CommandGroups.TruckerJob, "Cancels the current trucker run")]
        public void CancelRun(Client player)
        {
            if (player.GetCharacter().TruckingStage == Character.TruckingStages.None)
                return;

            //Respawn cars and warp out.
            player.GetCharacter().TruckingStage = Character.TruckingStages.None;
            NetHandle veh = API.GetEntityData(player, "TRUCKER_VEHICLE");

            if (player.Vehicle != null && player.Vehicle == veh)
                //API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));
                Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player)); // CONV NOTE: delay fixme

            if (!veh.IsNull)
            {
                VehicleManager.respawn_vehicle(veh.GetVehicle());
            }
            
            //Reset Timers.
            Timer timer = API.GetEntityData(player, "TRUCKING_CANCELTIMER");
            timer?.Dispose();
            timer = API.GetEntityData(player, "TRUCKING_UNLOAD_TIMER");
            timer?.Dispose();
            timer = API.GetEntityData(player, "TRUCKING_LOAD_TIMER");
            timer?.Dispose();
            API.ResetEntityData(player, "TRUCKING_CANCELTIMER");
            API.ResetEntityData(player, "TRUCKING_UNLOAD_TIMER");
            API.ResetEntityData(player, "TRUCKING_LOAD_TIMER");

            //Reset other variables
            API.ResetEntityData(player, "TRUCKER_VEHICLE");
            API.ResetEntityData(veh, "TRUCKER_DRIVER");

            //Unfreeze and send message.
            API.FreezePlayer(player, false);
            API.SendChatMessageToPlayer(player, "The trucking run has been done or cancelled.");
        }

        [Command("supplydemand"), Help(HelpManager.CommandGroups.TruckerJob, "Check the current supply and gas status.")]
        public void CheckDemand(Client player)
        {

            double fuel = 0;
            double maxfuel = PropertyManager.Properties.Count(x => x.Type == PropertyManager.PropertyTypes.GasStation) *
                          Property.MaxGasSupplies;

            PropertyManager.Properties.Where(x => x.Type == PropertyManager.PropertyTypes.GasStation).AsParallel()
                .ForAll(x => fuel += x.Supplies);

            API.SendChatMessageToPlayer(player,
                "The amount of supplies stored: " +
                SettingsManager.Settings.TruckerSupplies);

            if (maxfuel == 0)
                API.SendChatMessageToPlayer(player, "The amount of fuel available in gas stations: 100%");
            else
                API.SendChatMessageToPlayer(player,
                    "The amount of fuel available in gas stations: " + Math.Round((fuel / maxfuel) * 100) + "%");
        }
    }
}
