using System;

using GTANetworkAPI;

using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using mtgvrp.inventory;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.core.Items;
using System.Linq;

using mtgvrp.vehicle_manager.modding;

namespace mtgvrp.job_manager.taxi
{
    public class MechanicJob : Script
    {

        [Command("fixcar"), Help(HelpManager.CommandGroups.MechanicJob, "Used to fix the car you're inside.")]
        public void fixcar_cmd(Client player)
        {
            Character character = player.GetCharacter();
            var veh = VehicleManager.GetVehFromNetHandle(API.GetPlayerVehicle(player));

            if (veh == null)
                return;

            if (character?.JobOne?.Type != JobManager.JobTypes.Mechanic)
            {
                API.SendPictureNotificationToPlayer(player, "You must be a mechanic to use this command.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (veh == null)
            {
                API.SendChatMessageToPlayer(player, "You must be inside of the vehicle to fix it.");
                return;
            }

            if (API.GetVehicleEngineStatus(veh.NetHandle) == true)
            {
                player.SendChatMessage("You must turn the engine off before fixing it.");
                return;
            }

            if (TimeManager.GetTimeStamp < character.FixcarPrevention)
            {
                API.SendChatMessageToPlayer(player, $@"You must wait {character.FixcarPrevention - TimeManager.GetTimeStamp} more seconds before fixing another car.");
                return;
            }

            
            if (InventoryManager.DoesInventoryHaveItem(character, typeof(EngineParts)).Length == 0)
            {
                player.SendChatMessage("You don't have enough engine parts.");
                return;
            }

            API.SetVehicleHealth(veh.NetHandle, 1000);
            API.RepairVehicle(veh.NetHandle);
            InventoryManager.DeleteInventoryItem(character, typeof(EngineParts), 1);
            player.SendChatMessage("Vehicle repaired.");
            LogManager.Log(LogManager.LogTypes.Stats, $"[Vehicle] {character.CharacterName}[{player.GetAccount().AccountName}] has fixed vehicle #{veh.Id}.");
            character.FixcarPrevention = TimeManager.GetTimeStampPlus(TimeSpan.FromSeconds(10));
        }

        [Command("paintcar"), Help(HelpManager.CommandGroups.MechanicJob, "Used to paint the car you're inside. <br/> Use the wiki to get the color ids.", "The primary color.", "The secondary color.")]
        public void paintcar_cmd(Client player, int col1, int col2)
        {
            Character character = player.GetCharacter();
            var veh = VehicleManager.GetVehFromNetHandle(API.GetPlayerVehicle(player));

            if (veh == null)
                return;

            if (character?.JobOne?.Type != JobManager.JobTypes.Mechanic)
            {
                API.SendPictureNotificationToPlayer(player, "You must be a mechanic to use this command.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (InventoryManager.DoesInventoryHaveItem(character, typeof(SprayPaint)).Length == 0)
            {
                player.SendChatMessage("You don't have spray paint.");
                return;
            }

            if (API.GetVehicleEngineStatus(veh.NetHandle) == true)
            {
                player.SendChatMessage("You must turn the engine off before painting it.");
                return;
            }

            API.SetVehiclePrimaryColor(API.GetPlayerVehicle(player), col1);
            API.SetVehicleSecondaryColor(API.GetPlayerVehicle(player), col2);
            veh.VehMods[ModdingManager.PrimaryColorId.ToString()] = col1.ToString();
            veh.VehMods[ModdingManager.SecondryColorId.ToString()] = col2.ToString();
            veh.Save();
            InventoryManager.DeleteInventoryItem(character, typeof(SprayPaint), 1);
            player.SendChatMessage("Vehicle painted.");

            LogManager.Log(LogManager.LogTypes.Stats, $"[Vehicle] {character.CharacterName}[{player.GetAccount().AccountName}] has painted vehicle #{veh.Id}.");
        }
   
    }

}
