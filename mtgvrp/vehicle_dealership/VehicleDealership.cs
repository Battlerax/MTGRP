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
    class VehicleDealership : Script
    {
        #region Vehicle Info [NAME/HASH/PRICE]

        //Could be changed to dynamic later on.
        private readonly string[][] _motorsycles =
        {
            new[] {"Quad", "-2128233223", "8000"},
            new[] {"Faggio", "-1842748181", "5000"},
            new[] {"Hexer", "301427732", "25000"},
            new[] {"Sanchez", "788045382", "12000"},
            new[] {"PCJ", "-909201658", "23000"},
            new[] {"Bagger", "-2140431165", "14000"}
        };

        private readonly string[][] _copues =
        {
            new[] {"Mini", "-1177863319", "14000"},
            new[] {"Blista", "1039032026", "28000"},
            new[] {"Rhapsody", "841808271", "30000"},
            new[] {"Prairie", "-1450650718", "25000"}
        };

        private readonly string[][] _trucksnvans =
        {
            new[] {"Benson", "2053223216", "60000"},
            new[] {"Mule", "904750859", "70000"},
        };

        private readonly string[][] _offroad =
        {
            new[] {"Bodhi", "-1435919434", "38000"},
            new[] {"Sandking", "-1189015600", "53000"},
            new[] {"Rebel", "-2045594037", "65000"},
            new[] {"Mesa", "914654722", "75000"},
            new[] {"RancherXL", "1645267888", "80000"},
        };

        private readonly string[][] _musclecars =
        {
            new[] {"Dominator", "80636076", "55000"},
            new[] {"Buccaneer", "-682211828", "40000"},
            new[] {"Gauntlet", "-1800170043", "58000"},
            new[] {"Tampa", "972671128", "34000"},
            new[] {"Ruiner", "-227741703", "66000"},
            new[] {"SabreGT", "-1685021548", "115000"},
            new[] {"VooDoo", "2006667053", "15000"},
            new[] {"Faction", "-2119578145", "35000"},
        };

        private readonly string[][] _suv =
        {
            new[] {"Baller", "-808831384", "75000"},
            new[] {"Cavalcade", "2006918058", "55000"},
            new[] {"Gresley", "-1543762099", "48000"},
            new[] {"Granger", "-1775728740", "70000"},
            new[] {"Dubsta", "1177543287", "95000"},
            new[] {"Huntley", "486987393", "65000"},
            new[] {"XLS", "1203490606", "39000"},
        };

        private readonly string[][] _supercars =
        {
            new[] {"Elegy", "196747873", "85000"},
            new[] {"Fusilade", "499169875", "120000"},
            new[] {"Coquette", "108773431", "150000"},
            new[] {"Lynx", "482197771", "165000"},
        };

        #endregion

        //Vars: 
        private readonly Vector3[] _dealershipsLocations =
        {
            new Vector3(-56.77422f, -1097.052f, 26.42235f)
        };

        private List<MarkerZone> _markerZones = new List<MarkerZone>();
        public VehicleDealership()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;

            //Setup the blip.
            foreach (var loc in _dealershipsLocations)
            {
                var marker = new MarkerZone(loc, new Vector3()) {BlipSprite = 100, TextLabelText = "/buyvehicle"};
                marker.Create();
                API.shared.setBlipShortRange(marker.Blip, true);
                API.shared.setBlipName(marker.Blip, "Vehicle Dealership");
                _markerZones.Add(marker);
            }
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "vehicledealer_selectcar")
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
                        new Vector3(-42.44885, -1115.865, 25.86397),
                        new Vector3(-45.04674, -1116.336, 25.86318),
                        new Vector3(-47.77579, -1116.404, 25.86377),
                        new Vector3(-50.53339, -1116.758, 25.86334),
                        new Vector3(-53.56507, -1116.645, 25.86412),
                        new Vector3(-56.42249, -1116.668, 25.8641),
                        new Vector3(-59.06798, -1117.206, 25.86339),
                        new Vector3(-61.86055, -1117.122, 25.8629)
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
                    API.triggerClientEvent(sender, "dealership_exitdealermenu");
                }
                else
                    API.sendChatMessageToPlayer(sender,
                        $"You don't have enough money to buy the ~g~{selectedCar[0]}~w~.");
            }
        }

        [Command("buyvehicle"), Help(HelpManager.CommandGroups.Vehicles, "Command used inside dealership to buy a vehicle.", null)]
        public void BuyVehicle(Client player)
        {
            //Check if can buy more cars.
            Character character = player.GetCharacter();
            if (character.OwnedVehicles.Count >= VehicleManager.GetMaxOwnedVehicles(player))
            {
                API.sendChatMessageToPlayer(player, "You can't own anymore vehicles.");
                API.sendChatMessageToPlayer(player, "~g~NOTE: You can buy VIP to increase your vehicle slots.");
                return;
            }

            var currentPos = API.getEntityPosition(player);
            if (_dealershipsLocations.Any(dealer => currentPos.DistanceTo(dealer) < 5F))
            {
                API.triggerClientEvent(player, "dealership_showbuyvehiclemenu", API.toJson(_motorsycles),
                    API.toJson(_copues), API.toJson(_trucksnvans), API.toJson(_offroad), API.toJson(_musclecars),
                    API.toJson(_suv), API.toJson(_supercars));
            }
            else
                API.sendChatMessageToPlayer(player, "You aren't near any dealership.");
        }
    }
}