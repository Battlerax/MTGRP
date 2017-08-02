using System;
using System.Collections.Generic;
using System.Linq;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using mtgvrp.core.Help;

namespace mtgvrp.vehicle_dealership
{
    class ChopperDealership : Script
    {
        #region Vehicle Info [NAME/HASH/PRICE]

        //Could be changed to dynamic later on.
        private readonly string[][] _helicopters =
        {
            new[] {"Frogger", "744705981", "240000"},
            new[] {"Maverick", "-1660661558", "400000"},
        };
        #endregion

        //Vars: 
        private readonly Vector3[] _dealershipsLocations =
        {
            new Vector3(-1154.221, -2715.568, 19.88731)
        };

        private List<MarkerZone> _markerZones = new List<MarkerZone>();
        public ChopperDealership()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;

            //Setup the blip.
            foreach (var loc in _dealershipsLocations)
            {
                var marker = new MarkerZone(loc, new Vector3()) {BlipSprite = 43, TextLabelText = "/buychopper", BlipColor = 46};
                marker.Create();
                API.shared.setBlipShortRange(marker.Blip, true);
                API.shared.setBlipName(marker.Blip, "Chopper Dealership");
                API.shared.setBlipColor(marker.Blip, 46);
                _markerZones.Add(marker);
            }
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "chopperdealer_selectcar")
            {
                Character character = sender.GetCharacter();

                string[] selectedCar = null;

                #region Get Car Info.

                switch ((int) arguments[0])
                {
                    case 0:
                        selectedCar = _helicopters[(int) arguments[1]];
                        break;
                }

                #endregion

                if (selectedCar == null) return;

                if (Money.GetCharacterMoney(character) >= Convert.ToInt32(selectedCar[2]))
                {
                    //Remove price.
                    InventoryManager.DeleteInventoryItem(character, typeof(Money), Convert.ToInt32(selectedCar[2]));

                    //Spawn positions.
                    Vector3[] spawnPoss =
                    {
                        new Vector3(-1112.07, -2883.274, 13.94603),
                        new Vector3(-1145.618, -2864.848, 13.94607),
                        new Vector3(-1177.989, -2845.755, 13.94576),
                    };
                    var randomPos = new Random().Next(1, spawnPoss.Length) - 1;

                    //Create the vehicle.
                    var theVehicle = VehicleManager.CreateVehicle(
                        (VehicleHash)Convert.ToInt32(selectedCar[1]),
                        spawnPoss[randomPos],
                        new Vector3(0.1917319, 0.1198539, -177.1394),
                        "Unregistered",
                        character.Id,
                        vehicle_manager.Vehicle.VehTypePerm
                    );
                    //theVehicle.IsVip = true;
                    theVehicle.OwnerName = character.CharacterName;
                    //Add it to the players cars.
                    theVehicle.Insert();

                    //Spawn it.
                    if (VehicleManager.spawn_vehicle(theVehicle) != 1)
                        API.sendChatMessageToPlayer(sender, "An error occured while spawning your vehicle.");

                    //Notify.
                    API.sendChatMessageToPlayer(sender,
                        $"You have sucessfully bought the ~g~{selectedCar[0]}~w~ for ${selectedCar[2]}.");
                    API.sendChatMessageToPlayer(sender, "Use /myvehicles to manage it.");

                    //Exit.
                    API.triggerClientEvent(sender, "chopperdealership_exitdealermenu");
                }
                else
                    API.sendChatMessageToPlayer(sender,
                        $"You don't have enough money to buy the ~g~{selectedCar[0]}~w~.");
            }
        }

        [Command("buychopper"), Help(HelpManager.CommandGroups.Vehicles, "Command used inside dealership to buy a helicopter.", null)]
        public void BuyVehicle(Client player)
        {
            //Check if can buy more cars.
            Character character = player.GetCharacter();
            Account account = API.getEntityData(player, "Account");

            /*if (account.VipLevel < 1)
            {
                player.sendChatMessage("You must be a VIP to use the chopper dealership.");
                return;
            }
            */
            if (character.OwnedVehicles.Count >= VehicleManager.GetMaxOwnedVehicles(player))
            {
                API.sendChatMessageToPlayer(player, "You can't own anymore vehicles.");
                API.sendChatMessageToPlayer(player, "~g~NOTE: You can upgrade your VIP to increase your vehicle slots.");
                return;
            }

            var currentPos = API.getEntityPosition(player);
            if (_dealershipsLocations.Any(dealer => currentPos.DistanceTo(dealer) < 5F))
            {
                API.triggerClientEvent(player, "chopperdealership_showbuyvehiclemenu", API.toJson(_helicopters));
            }
            else
                API.sendChatMessageToPlayer(player, "You aren't near any dealership.");
        }
    }
}