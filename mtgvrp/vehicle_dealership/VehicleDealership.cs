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

        // Several vehicles are bugged - https://bug.gt-mp.net/view.php?id=230 
        // Known bugged - Faggio, Ratbike, Chimera, Zombie Bikes + Anything in the Bikers DLC
        private readonly string[][] _motorsycles =
        {
            new[] {"Faggio Sport", "-1842748181", "5000"},
            new[] {"Faggio Classic", "55628203","4000" },
            new[] {"Hexer", "301427732", "27500"},
            new[] {"Sanchez", "788045382", "25000"},
            new[] {"PCJ", "-909201658", "40000"},
            new[] {"Bagger", "-2140431165", "17500"},
            new[] {"Bati", "-891462355", "160000"},
            new[] {"Gargoyle", "741090084","65000" },
            new[] { "Daemon", "2006142190", "60000" },
            new[] {"Innovation", "-159126838", "45000"},
            new[] {"Akuma", "1672195559","160000" },
            new [] {"Ratbike", "1873600305", "20000" },
            new [] {"Zombie", "-1009268949", "60000" },
            new [] {"Avarus", "-2115793025", "50000" },
            new [] {"Chimera", "6774487","120000" }
        };

        private readonly string[][] _copues =
        {
            new[] {"Blista", "1039032026", "25000"},
            new[] {"Rhapsody", "841808271", "14000"},
            new[] {"Prairie", "-1450650718", "15000"},
            new[] {"Oracle", "1348744438", "30000"},
            new[] {"Zion", "-1122289213", "40000"},
        };

        private readonly string[][] _trucksnvans =
        {
            new[] {"Benson", "2053223216", "50000"},
            new[] {"Mule", "904750859", "60000"},
            new[] {"Speedo", "-810318068", "40000" },
            new[] {"Burrito", "-1743316013","40000" },
            new[] {"Surfer", "-1311240698","25000" }
        };

        private readonly string[][] _offroad =
        {
            new[] {"Bodhi", "-1435919434", "38000"},
            new[] {"Sandking", "-1189015600", "70000"},
            new[] {"Rebel", "-2045594037", "50000"},
            new[] {"Mesa", "914654722", "45000"},
            new[] {"RancherXL", "1645267888", "55000"},
            new[] {"DuneLoader", "1770332643","25000" }
        };

        private readonly string[][] _musclecars =
        {
            new[] {"Dominator", "80636076", "120000"},
            new[] {"Buccaneer", "-682211828", "35000"},
            new[] {"Gauntlet", "-1800170043", "80000"},
            new[] {"Tampa", "972671128", "40000"},
            new[] {"Ruiner", "-227741703", "70000"},
            new[] {"SabreGT", "-1685021548", "70000"},
            new[] {"VooDoo", "2006667053", "15000"},
            new[] {"Faction", "-2119578145", "35000"},
            new[] {"Futo", "2016857647", "25000"},
            new [] { "Phoenix", "-2095439403","60000" },
            new [] { "Rat-Loader", "-667151410","20000" }
        };

        private readonly string[][] _suv =
        {
            new[] {"Baller", "-808831384", "65000"},
            new[] {"Cavalcade", "2006918058", "55000"},
            new[] {"Gresley", "-1543762099", "60000"},
            new[] {"Granger", "-1775728740", "100000"},
            new[] {"Dubsta", "1177543287", "55000"},
            new[] {"Huntley", "486987393", "75000"},
            new[] {"XLS", "1203490606", "80000"},
        };

        private readonly string[][] _supercars =
        {
       
            new[] {"Fusilade", "499169875", "200000"},
            new[] {"Coquette", "108773431", "280000"},
            new[] {"Lynx", "482197771", "340000"},
            new[] { "Turismo", "-982130927", "506000"},
            new[] { "Tyrus", "2067820283", "667000"},
            new[] { "Italigtb", "-2048333973", "700000"},
            new[] { "Nero", "1034187331", "750000"},
            new[] { "Zentorno", "-1403128555","800000" }
        };

        private readonly string[][] _cycles =
{
            new[] { "Bmx", "1131912276", "1500"},
            new[] { "Cruiser", "448402357", "900"},
            new[] {"Fixter", "-836512833", "1000"},
            new[] { "TriBike", "-400295096", "2000"},
        };

        private readonly string[][] _sedans =
        {
            new[] {"Asea", "-1809822327", "55000"},
            new[] {"Primo", "-1150599089", "45000"},
            new[] {"Surge", "-1894894188", "45000"},
            new[] {"Warrender", "1373123368", "45000"},
            new[] {"Washington", "1777363799", "70000"},
            new[] {"Stanier", "-1477580979", "60000"},
            new[] { "Emperor", "-685276541","30000" },
            new[] {"Stretch", "-1961627517","80000" },
            new [] {"Tailgater", "-1008861746","85000" },
            new[] { "Schafter", "-1255452397","125000" }



        };

        private readonly string[][] _sportsCars =
        {
            new[] {"Elegy", "196747873", "195000"},
            new[] {"Sultan", "970598228", "125000"},
            new[] {"Kuruma", "-1372848492", "250000"},
            new[] {"Penumbra", "-377465520", "150000"},
            new[] {"Obey 9F", "1032823388","400000" },
            new[] { "Feltzer", "-1995326987","125000" }
        };

        private readonly string[][] _compactCars =
        {
            new[] {"Panto", "-431692672", "17500"},
            new[] {"Brioso", "1549126457", "35000"},
            new[] {"Mini", "-1177863319", "35000"},
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
                    case 7:
                        selectedCar = _cycles[(int)arguments[1]];
                        break;
                    case 8:
                        selectedCar = _sedans[(int) arguments[1]];
                        break;
                    case 9:
                        selectedCar = _sportsCars[(int) arguments[1]];
                        break;
                    case 10:
                        selectedCar = _compactCars[(int) arguments[1]];
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
                        "...",
                        character.Id,
                        vehicle_manager.Vehicle.VehTypePerm
                    );
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
                    API.toJson(_suv), API.toJson(_supercars), API.toJson(_cycles),API.toJson(_sedans),API.toJson(_sportsCars),API.toJson(_compactCars));
            }
            else
                API.sendChatMessageToPlayer(player, "You aren't near any dealership.");
        }
    }
}