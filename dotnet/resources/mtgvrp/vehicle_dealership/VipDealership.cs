using System;
using System.Collections.Generic;
using System.Linq;

using GTANetworkAPI;



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
            //Setup the blip.
            /*foreach (var loc in _dealershipsLocations)
            {
                var marker = new MarkerZone(loc, new Vector3()) {BlipSprite = 100, TextLabelText = "/buyvipvehicle", BlipColor = 46};
                marker.Create();
                API.Shared.SetBlipShortRange(marker.Blip, true);
                API.Shared.SetBlipName(marker.Blip, "VIP Vehicle Dealership");
                API.Shared.SetBlipColor(marker.Blip, 46);
                _markerZones.Add(marker);
            }
            */
        }

        [RemoteEvent("vipdealer_selectcar")]
        public void VIPDealerSelectCar(Client sender, params object[] arguments)
        {
            Character character = sender.GetCharacter();

            string[] selectedCar = null;
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
                    vehicle_manager.GameVehicle.VehTypePerm
                );
                theVehicle.OwnerName = character.CharacterName;
                theVehicle.IsVip = true;
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
                NAPI.ClientEvent.TriggerClientEvent(sender, "vipdealership_exitdealermenu");
            }
            else
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"You don't have enough money to buy the ~g~{selectedCar[0]}~w~.");
        }

        //[Command("buyvipvehicle"), Help(HelpManager.CommandGroups.Vehicles, "Command used inside dealership to buy a vehicle.", null)]
        public void BuyVehicle(Client player)
        {
            //Check if can buy more cars.
            Character character = player.GetCharacter();
            Account account = player.GetAccount();

            if (account.VipLevel < 1)
            {
                player.SendChatMessage("You must be a VIP to use the VIP dealership.");
                return;
            }
            if (character.OwnedVehicles.Count >= VehicleManager.GetMaxOwnedVehicles(player))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You can't own anymore vehicles.");
                NAPI.Chat.SendChatMessageToPlayer(player, "~g~NOTE: You can upgrade your VIP to increase your vehicle slots.");
                return;
            }
            
            var currentPos = NAPI.Entity.GetEntityPosition(player);
            if (_dealershipsLocations.Any(dealer => currentPos.DistanceTo(dealer) < 5F))
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "vipdealership_showbuyvehiclemenu", NAPI.Util.ToJson(_motorsycles),
                    NAPI.Util.ToJson(_copues), NAPI.Util.ToJson(_trucksnvans), NAPI.Util.ToJson(_offroad), NAPI.Util.ToJson(_musclecars),
                    NAPI.Util.ToJson(_suv), NAPI.Util.ToJson(_supercars));
            }
            else
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't near any dealership.");
        }
    }
}
