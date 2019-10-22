using System;
using System.Collections.Generic;
using System.Linq;

using GTANetworkAPI;





using mtgvrp.core.Items;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using mtgvrp.core.Help;

namespace mtgvrp.vehicle_dealership
{
    class BoatDealership : Script
    {
        #region Vehicle Info [NAME/HASH/PRICE]

        //Could be changed to dynamic later on.
        private readonly string[][] _boats =
        {
            new[] {"Dinghy", "1033245328", "60000"},
            new[] {"Jetmax", "861409633", "150000"},
            new[] {"Tug", "-2100640717", "300000"},
        };

        #endregion

        //Vars: 
        private readonly Vector3[] _dealershipsLocations =
        {
            new Vector3(-1820.654, -1219.635, 13.01743)
        };

        private List<MarkerZone> _markerZones = new List<MarkerZone>();
        public BoatDealership()
        {
            //Setup the blip.
            foreach (var loc in _dealershipsLocations)
            {
                var marker = new MarkerZone(loc, new Vector3()) { BlipSprite = 371, TextLabelText = "/buyboat /buyrod" };
                marker.Create();
                API.Shared.SetBlipShortRange(marker.Blip, true);
                API.Shared.SetBlipName(marker.Blip, "Boat Dealership");
                _markerZones.Add(marker);
            }
        }

        [RemoteEvent("boatdealer_selectcar")]
        public void BoatDealerSelectCar(Client sender, params object[] arguments)
        {
            Character character = sender.GetCharacter();

            string[] selectedCar = null;

            switch ((int)arguments[0])
            {
                case 0:
                    selectedCar = _boats[(int)arguments[1]];
                    break;
            }

            if (selectedCar == null) return;

            if (Money.GetCharacterMoney(character) >= Convert.ToInt32(selectedCar[2]))
            {
                //Remove price.
                InventoryManager.DeleteInventoryItem(character, typeof(Money), Convert.ToInt32(selectedCar[2]));

                //Spawn positions.
                Vector3[] spawnPoss =
                {
                    new Vector3(-1792.381, -1235.88, -0.3676317),
                    new Vector3(-1775.264, -1213.21, 0.5480041),
                    new Vector3(-1771.058, -1238.927, -0.6069559)

                };
                var randomPos = new Random().Next(1, spawnPoss.Length) - 1;

                //Create the vehicle.
                var theVehicle = VehicleManager.CreateVehicle(
                    (VehicleHash)Convert.ToInt32(selectedCar[1]),
                    spawnPoss[randomPos],
                    new Vector3(0.1917319, 0.1198539, -177.1394),
                    "...",
                    character.Id,
                    vehicle_manager.GameVehicle.VehTypePerm
                );
                theVehicle.OwnerName = character.CharacterName;

                //Add it to the players cars.
                theVehicle.Insert();

                //Spawn it.
                if (VehicleManager.spawn_vehicle(theVehicle) != 1)
                    NAPI.Chat.SendChatMessageToPlayer(sender, "An error occured while spawning your vehicle.");

                //Notify.
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"You have sucessfully bought the ~g~{selectedCar[0]}~w~ for ${selectedCar[2]}.");
                NAPI.Chat.SendChatMessageToPlayer(sender, "Use /myvehicles to manage it.");

                //Exit.
                NAPI.ClientEvent.TriggerClientEvent(sender, "dealership_exitdealermenu");
            }
            else
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"You don't have enough money to buy the ~g~{selectedCar[0]}~w~.");
        }

        [Command("buyrod")]
        public void buyrod_cmd(Client player)
        {

            var character = player.GetCharacter();
            var currentPos = NAPI.Entity.GetEntityPosition(player);
            if (_dealershipsLocations.Any(dealer => currentPos.DistanceTo(dealer) < 5F))
            {

                if (NAPI.Player.IsPlayerInAnyVehicle(player))
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "You're not able to buy a rod while in a vehicle!");
                    return;
                }

                if (Money.GetCharacterMoney(character) < 250)
                {
                    player.SendChatMessage("You can't afford a fishing rod.");
                    return;
                }

                if (InventoryManager.DoesInventoryHaveItem<FishingRod>(character).Length >= 1)
                {
                    player.SendChatMessage("You already have a fishing rod.");
                    return;
                }

                switch (InventoryManager.GiveInventoryItem(player.GetCharacter(), new FishingRod()))
                {
                    case InventoryManager.GiveItemErrors.Success:
                        InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(Money), 250);
                        player.SendChatMessage("You have purchased a fishing rod. Use /fish to begin fishing!"); break;

                    case InventoryManager.GiveItemErrors.NotEnoughSpace:
                        NAPI.Chat.SendChatMessageToPlayer(player,
                            $"[BUSINESS] You dont have enough space for that item. You need {new FishingRod().AmountOfSlots} Slots.");
                        break;
                }
            }
            else
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't near the boat dealership.");
        }

        [Command("buyboat"), Help(HelpManager.CommandGroups.Vehicles, "Command used inside dealership to buy a vehicle.", null)]
        public void BuyVehicle(Client player)
        {
            //Check if can buy more cars.
            Character character = player.GetCharacter();
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                NAPI.Chat.SendChatMessageToPlayer(player,"You're not able to buy a boat while in a vehicle!");
                return;
            }

            if (character.OwnedVehicles.Count >= VehicleManager.GetMaxOwnedVehicles(player))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You can't own anymore vehicles.");
                NAPI.Chat.SendChatMessageToPlayer(player, "~g~NOTE: You can buy VIP to increase your vehicle slots.");
                return;
            }

            var currentPos = NAPI.Entity.GetEntityPosition(player);
            if (_dealershipsLocations.Any(dealer => currentPos.DistanceTo(dealer) < 5F))
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "dealership_showbuyboatmenu", NAPI.Util.ToJson(_boats));
            }
            else
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't near any dealership.");
        }
    }
}
