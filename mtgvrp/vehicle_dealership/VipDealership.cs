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
    class VipDealership : Script
    {
        #region Vehicle Info [NAME/HASH/PRICE]

        //Could be changed to dynamic later on.
        private readonly string[][] _motorsycles =
        {
            new[] {"Quad", "-2128233223", "8000"},
            new[] { "Ruffian", "-893578776", "20000"},
            new[] { "Nemesis", " -634879114", "30000"},
        };

        private readonly string[][] _copues =
        {
            new[] { "Windsor2", "-1930048799", "50000"},
            new[] { "Zion", "-1122289213", "20000"}
        };

        private readonly string[][] _trucksnvans =
        {
            new[] { "DLoader", "1770332643", "15000"},
        };

        private readonly string[][] _offroad =
        {
            new[] { "Kalahari", "92612664", "25000"},
        };

        private readonly string[][] _musclecars =
        {
            new[] { "SlamVan", "729783779", "35000"},
            new[] { "Stalion", "1923400478", "40000"},
        };

        private readonly string[][] _suv =
        {
            new[] { "Landstalker", "1269098716", "75000"},
            new[] { "Seminole", "1221512915", "40000"},
            new[] { "Patriot", "-808457413", "90000"},
        };

        private readonly string[][] _supercars =
        {
            new[] { "Adder", "-1216765807", "125000"},
            new[] { "Osiris", "1987142870", "150000"},
            new[] {"Nero", "1034187331", "170000"},
            new[] { "Cheetah", " -1311154784", "200000"},
            new[] { "Bullet", "-1696146015", "190000"},
        };

        #endregion

        //Vars: 
        private readonly Vector3[] _dealershipsLocations =
        {
            new Vector3(-41.50781, -1675.438, 29.42231)
        };

        private List<MarkerZone> _markerZones = new List<MarkerZone>();
        public VipDealership()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;

            //Setup the blip.
            foreach (var loc in _dealershipsLocations)
            {
                var marker = new MarkerZone(loc, new Vector3()) {BlipSprite = 100, TextLabelText = "/buyvipvehicle", BlipColor = 46};
                marker.Create();
                API.shared.setBlipShortRange(marker.Blip, true);
                API.shared.setBlipName(marker.Blip, "VIP Vehicle Dealership");
                API.shared.setBlipColor(marker.Blip, 46);
                _markerZones.Add(marker);
            }
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "vipdealer_selectcar")
            {
                Character character = sender.GetCharacter();

                string[] selectedCar = null;

                #region Get Car Info.

                switch ((int) arguments[0])
                {
                    case 0:
                        selectedCar = _motorsycles[(int) arguments[1]];
                        break;
                    case 1:
                        selectedCar = _copues[(int) arguments[1]];
                        break;
                    case 2:
                        selectedCar = _trucksnvans[(int) arguments[1]];
                        break;
                    case 3:
                        selectedCar = _offroad[(int) arguments[1]];
                        break;
                    case 4:
                        selectedCar = _musclecars[(int) arguments[1]];
                        break;
                    case 5:
                        selectedCar = _suv[(int) arguments[1]];
                        break;
                    case 6:
                        selectedCar = _supercars[(int) arguments[1]];
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
                        new Vector3(-51.88548, -1694.663, 29.09777),
                        new Vector3(-52.7189, -1689.862, 29.09228),
                        new Vector3(-58.41274, -1683.702, 29.09858),
                        new Vector3(-55.86712, -1687.208, 29.09822),
                    };
                    var randomPos = new Random().Next(1, spawnPoss.Length) - 1;

                    //Create the vehicle.
                    var theVehicle = VehicleManager.CreateVehicle(
                        (VehicleHash)Convert.ToInt32(selectedCar[1]),
                        spawnPoss[randomPos],
                        new Vector3(0.1917319, 0.1198539, -177.1394),
                        " ",
                        character.Id,
                        vehicle_manager.Vehicle.VehTypePerm
                    );
                    theVehicle.OwnerName = character.CharacterName;
                    theVehicle.IsVip = true;
                    //Add it to the players cars.
                    theVehicle.Insert();
                    character.OwnedVehicles.Add(theVehicle.Id);

                    //Spawn it.
                    if (VehicleManager.spawn_vehicle(theVehicle) != 1)
                        API.sendChatMessageToPlayer(sender, "An error occured while spawning your vehicle.");

                    //Notify.
                    API.sendChatMessageToPlayer(sender,
                        $"You have sucessfully bought the ~g~{selectedCar[0]}~w~ for ${selectedCar[2]}.");
                    API.sendChatMessageToPlayer(sender, "Use /myvehicles to manage it.");

                    //Exit.
                    API.triggerClientEvent(sender, "vipdealership_exitdealermenu");
                }
                else
                    API.sendChatMessageToPlayer(sender,
                        $"You don't have enough money to buy the ~g~{selectedCar[0]}~w~.");
            }
        }

        [Command("buyvipvehicle"), Help(HelpManager.CommandGroups.Vehicles, "Command used inside dealership to buy a vehicle.", null)]
        public void BuyVehicle(Client player)
        {
            //Check if can buy more cars.
            Character character = player.GetCharacter();
            Account account = API.getEntityData(player, "Account");

            if (account.VipLevel < 1)
            {
                player.sendChatMessage("You must be a VIP to use the VIP dealership.");
                return;
            }
            if (character.OwnedVehicles.Count >= VehicleManager.GetMaxOwnedVehicles(player))
            {
                API.sendChatMessageToPlayer(player, "You can't own anymore vehicles.");
                API.sendChatMessageToPlayer(player, "~g~NOTE: You can upgrade your VIP to increase your vehicle slots.");
                return;
            }

            var currentPos = API.getEntityPosition(player);
            if (_dealershipsLocations.Any(dealer => currentPos.DistanceTo(dealer) < 5F))
            {
                API.triggerClientEvent(player, "vipdealership_showbuyvehiclemenu", API.toJson(_motorsycles),
                    API.toJson(_copues), API.toJson(_trucksnvans), API.toJson(_offroad), API.toJson(_musclecars),
                    API.toJson(_suv), API.toJson(_supercars));
            }
            else
                API.sendChatMessageToPlayer(player, "You aren't near any dealership.");
        }
    }
}