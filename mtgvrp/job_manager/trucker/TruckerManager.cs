using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.property_system;

namespace mtgvrp.job_manager.trucker
{
    public class TruckerManager : Script
    {
        public TruckerManager()
        {
            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;
            API.onVehicleTrailerChange += API_onVehicleTrailerChange;
            API.onEntityEnterColShape += API_onEntityEnterColShape;
        }

        private void API_onEntityEnterColShape(ColShape colshape, NetHandle entity)
        {
            if (API.getEntityType(entity) != EntityType.Player)
                return;

            var player = API.getPlayerFromHandle(entity);
            var character = player.GetCharacter();

            if (player.isInVehicle && character.TruckingStage == 2)
            {
                var veh = player.vehicle;
                var vehicle = veh.handle.GetVehicle();

                if (vehicle.Job?.Type != JobManager.JobTypes.Trucker ||
                    character.JobOne?.Type != JobManager.JobTypes.Trucker) return;

                if (veh.trailer == null)
                {
                    API.sendChatMessageToPlayer(player, "~r~[Trucking]~w~ You must have a trailer to load your truck.");
                    return;
                }

                if (JobManager.GetJobById(character.JobZone)?.MiscOne.ColZone == colshape)
                {
                    veh.setData("TRUCKING_TRAILER", veh.trailer);
                    API.setBlipRouteVisible(JobManager.GetJobById(character.JobZone).MiscOne.Blip, false);

                    player.freeze(true);
                    API.sendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Please wait 10 seconds while your truck is loading...");

                    API.setEntityData(player, "TRUCKING_LOAD_TIMER", new Timer(state =>
                    {
                        character.TruckingStage = 2;
                        player.freeze(false);

                        API.setBlipRouteVisible(character.JobOne.MiscTwo.Blip, true);
                        API.setBlipRouteColor(character.JobOne.MiscTwo.Blip, 54);

                        API.sendChatMessageToPlayer(player,
                            "~r~[Trucking]~w~ Your truck have been loaded, head to the checkpoint to deliver them.");
                        API.resetEntityData(player, "TRUCKING_LOAD_TIMER");

                    }, null, 10000, Timeout.Infinite));
                }
                else if (JobManager.GetJobById(character.JobZone)?.Type == JobManager.JobTypes.Trucker &&
                         character.JobZoneType == 2)
                {
                    veh.setData("TRUCKING_TRAILER", veh.trailer);
                    API.setBlipRouteVisible(JobManager.GetJobById(character.JobZone).MiscOne.Blip, false);

                    player.freeze(true);
                    API.sendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Please wait 10 seconds while your truck is loading...");

                    API.setEntityData(player, "TRUCKING_LOAD_TIMER", new Timer(state =>
                    {
                        character.TruckingStage = 2;
                        player.freeze(false);



                        API.setBlipRouteVisible(character.JobOne.MiscTwo.Blip, true);
                        API.setBlipRouteColor(character.JobOne.MiscTwo.Blip, 54);

                        API.sendChatMessageToPlayer(player,
                            "~r~[Trucking]~w~ Your truck have been loaded, head to the checkpoint to deliver them.");
                        API.resetEntityData(player, "TRUCKING_LOAD_TIMER");

                    }, null, 10000, Timeout.Infinite));
                }

            }
        }

        private void API_onVehicleTrailerChange(GTANetworkShared.NetHandle tower, GTANetworkShared.NetHandle trailer)
        {
            var player = (Client) API.getEntityData(tower, "TRUCKER_DRIVER");

            if (player == null)
                return;

            var character = player.GetCharacter();
            if (character.TruckingStage == 1)
            {
                if (API.getEntityModel(trailer) == (int) VehicleHash.Tanker)
                {
                    API.setBlipRouteVisible(character.JobOne.MiscOne.Blip, true);
                    API.setBlipRouteColor(character.JobOne.MiscOne.Blip, 54);
                    API.sendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Head to the checkpoint to load your truck with fuel.");
                }
                else if (API.getEntityModel(trailer) == (int) VehicleHash.Trailers2 ||
                         API.getEntityModel(trailer) == (int) VehicleHash.Trailers3)
                {
                    var job = JobManager.Jobs.FirstOrDefault(x => x.Type == JobManager.JobTypes.Lumberjack);
                    if (job == null)
                        return;

                    API.setBlipRouteVisible(job.MiscOne.Blip, true);
                    API.setBlipRouteColor(job.MiscOne.Blip, 54);
                    API.sendChatMessageToPlayer(player,
                        "~r~[Trucking]~w~ Head to the checkpoint to load your truck with supplies.");
                }
                player.GetCharacter().TruckingStage = 2;
            }
        }

        private void API_onPlayerEnterVehicle(Client player, GTANetworkShared.NetHandle vehicle)
        {
            var veh = vehicle.GetVehicle();
            var character = player.GetCharacter();

            if (veh.Job?.Type == JobManager.JobTypes.Trucker && character.JobOne?.Type == JobManager.JobTypes.Trucker)
            {
                if (character.TruckingStage != 0)
                {
                    API.sendChatMessageToPlayer(player, "~r~[Trucking]~w~ You are already in a trucking run.");
                    return;
                }

                character.TruckingStage = 1;
                API.setEntityData(vehicle, "TRUCKER_DRIVER", player);
                API.sendChatMessageToPlayer(player, "~r~[Trucking]~w~ Attach a trailer to decide what to deliver.");
            }
        }

        [Command("supplydemand")]
        public void CheckDemand(Client player)
        {
            int supplies = 0;
            int maxsupplies =
                (PropertyManager.Properties.Count(x => x.Type != PropertyManager.PropertyTypes.GasStation) - 1) * 20;
            PropertyManager.Properties.Where(x => x.Type != PropertyManager.PropertyTypes.GasStation).AsParallel()
                .ForAll(x => supplies += x.Supplies);

            int fuel = 0;
            int maxfuel =
                (PropertyManager.Properties.Count(x => x.Type == PropertyManager.PropertyTypes.GasStation) - 1) * 20;
            PropertyManager.Properties.Where(x => x.Type != PropertyManager.PropertyTypes.GasStation).AsParallel()
                .ForAll(x => fuel += x.Supplies);

            API.sendChatMessageToPlayer(player,
                "The amount of supplies needed: " + (supplies / maxsupplies) * 100 + "%");
            API.sendChatMessageToPlayer(player, "The amount of fuel needed: " + (fuel / maxfuel) * 100 + "%");
        }
    }
}