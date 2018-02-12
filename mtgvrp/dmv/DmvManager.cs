using System;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using GTANetworkAPI;



using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.database_manager;
using mtgvrp.group_manager.lsgov;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;
using MongoDB.Driver;
using System.Collections.Generic;

namespace mtgvrp.dmv
{
    public class DmvManager : Script
    {
        private readonly List<Vector3> _testCheckpoints = new List<Vector3>
        {
            new Vector3(275.7367, -381.1554, 44.43955),
            new Vector3(295.1309, -451.0204, 42.92255),
            new Vector3(240.3799, -470.0326, 41.2066),
            new Vector3(81.14854, -476.974, 33.42332),
            new Vector3(-60.00119, -487.6081, 31.58802),
            new Vector3(-156.1391, -487.6473, 28.13439),
            new Vector3(-214.1319, -478.0857, 26.06907),
            new Vector3(-351.3909, -475.7861, 37.55101),
            new Vector3(-413.848, -538.1572, 41.53019),
            new Vector3(-415.9911, -650.6707, 36.68476),
            new Vector3(-415.7881, -711.6459, 36.66223),
            new Vector3(-416.6297, -908.2397, 36.6805),
            new Vector3(-415.8291, -1130.886, 36.6318),
            new Vector3(-417.3184, -1304.933, 36.59539),
            new Vector3(-387.8974, -1374.748, 36.83),
            new Vector3(-381.8099, -1327.558, 36.63412),
            new Vector3(-293.8431, -1250.104, 36.7085),
            new Vector3(-186.7111, -1245.432, 36.65297),
            new Vector3(-78.04612, -1255.107, 36.51565),
            new Vector3(29.52601, -1262.698, 28.92844),
            new Vector3(56.68351, -1258.65, 28.7719),
            new Vector3(72.45765, -1217.64, 28.70172),
            new Vector3(71.9919, -1129.681, 28.74697),
            new Vector3(101.6237, -1026.051, 28.8411),
            new Vector3(147.6756, -901.7576, 29.75008),
            new Vector3(190.9861, -782.1708, 31.36706),
            new Vector3(217.9093, -707.7353, 34.88936),
            new Vector3(265.1003, -592.2642, 42.60115),
            new Vector3(296.4409, -510.9495, 42.73027),
            new Vector3(312.4164, -413.073, 44.50725),
            new Vector3(288.2681, -366.7339, 44.55527),
            new Vector3(290.8339, -339.3133, 44.36193),
        };

        public static readonly dynamic[][] _testVehicles =
        {
            new dynamic[] {new Vector3(266.3742, -332.2829, 44.48646), new Vector3(0.02338559, -0.0002331526, -109.5953), null},
            new dynamic[] {new Vector3(267.7154, -329.0442, 44.48596), new Vector3(0.02338559, -0.0002331526, -109.5953), null},
            new dynamic[] {new Vector3(268.8224, -325.9093, 44.48621), new Vector3(0.02338559, -0.0002331526, -109.5953), null},
            new dynamic[] {new Vector3(269.9587, -322.6313, 44.48601), new Vector3(0.02338559, -0.0002331526, -109.5953), null},
            new dynamic[] {new Vector3(271.149, -319.2983, 44.4863), new Vector3(0.02338559, -0.0002331526, -109.5953), null},
        };

        public DmvManager()
        {
            Event.OnResourceStart += API_onResourceStart;
            VehicleManager.OnVehicleEngineToggle += OnVehicleEngineToggle;
            Event.OnPlayerExitVehicle += API_onPlayerExitVehicle;
        }

        [RemoteEvent("DMV_REGISTER_VEHICLE")]
        private void DMVRegisterVehicle(Client player, params object[] arguments)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop?.Type != PropertyManager.PropertyTypes.DMV)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You need to be at a DMV.");
                return;
            }

            var c = player.GetCharacter();

            if (InventoryManager.DoesInventoryHaveItem<IdentificationItem>(c).Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must have a valid identification.");
                return;
            }

            if (InventoryManager.DoesInventoryHaveItem<DrivingLicenseItem>(c).Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must have a valid driving license.");
                return;
            }

            int vehid = Convert.ToInt32(arguments[0]);
            if (!c.OwnedVehicles.Exists(x => x.Id == vehid))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't own that vehicle!!!");
                return;
            }

            var veh = VehicleManager.Vehicles.FirstOrDefault(x => x.Id == vehid);
            if (veh == null || veh.OwnerId != c.Id)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't own that vehicle!!!");
                return;
            }

            if (veh.IsRegistered)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Vehicle is already registered!!!");
                return;
            }

            if (Money.GetCharacterMoney(c) < 100)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You need $100 to register your vehicle.");
                return;
            }

            //Set License Plate.
            veh.LicensePlate = GetValidLicensePlate();
            veh.IsRegistered = true;

            if (veh.IsSpawned)
            {
                API.SetVehicleNumberPlate(veh.NetHandle, veh.LicensePlate);
            }

            //Remove money.
            InventoryManager.DeleteInventoryItem<Money>(c, 100);

            NAPI.Chat.SendChatMessageToPlayer(player, $"You've successfully registered your {VehicleOwnership.returnCorrDisplayName(veh.VehModel)}. License: {veh.LicensePlate}");
            veh.Save();
        }

        [RemoteEvent("DMV_TEST_FINISH")]
        private void DMVTestFinish(Client player, params object[] arguments)
        {
            var c = player.GetCharacter();
            var isOnTime = DateTime.Now.Subtract(c.TimeStartedDmvTest) <= TimeSpan.FromMinutes(5);
            var isOnHealth = player.Vehicle.Health >= 999;

            if (isOnTime && isOnHealth)
            {
                InventoryManager.GiveInventoryItem(c, new DrivingLicenseItem(), 1, true);
                NAPI.Chat.SendChatMessageToPlayer(player, "You have ~g~COMPLETED~w~ your test.");
                LogManager.Log(LogManager.LogTypes.Stats, $"[DMV] {c.CharacterName}[{player.GetAccount().AccountName}] has got his driving license in {DateTime.Now.Subtract(c.TimeStartedDmvTest).Minutes:D2}:{DateTime.Now.Subtract(c.TimeStartedDmvTest).Seconds:D2}");
            }
            else
                NAPI.Chat.SendChatMessageToPlayer(player, "You have ~r~FAILED~w~ your test.");

            NAPI.Chat.SendChatMessageToPlayer(player,
                isOnTime
                    ? $"* Time: ~g~ {DateTime.Now.Subtract(c.TimeStartedDmvTest).Minutes:D2}:{DateTime.Now.Subtract(c.TimeStartedDmvTest).Seconds:D2} / 05:00"
                    : $"* Time: ~r~ {DateTime.Now.Subtract(c.TimeStartedDmvTest).Minutes:D2}:{DateTime.Now.Subtract(c.TimeStartedDmvTest).Seconds:D2} / 05:00");

            NAPI.Chat.SendChatMessageToPlayer(player,
                isOnHealth
                    ? $"* Vehicle Health: ~g~ {player.Vehicle.Health} / 999"
                    : $"* Vehicle Health: ~r~ {player.Vehicle.Health} / 999");

            VehicleManager.respawn_vehicle(player.Vehicle.Handle.GetVehicle());
            // CONV NOTE: proper delay needed probably
            //API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));
            Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player));

            c.IsInDmvTest = false;
        }

        string GetValidLicensePlate()
        {
            var rng = new Random();
            char GetRandomCharacter()
            {
                int index = rng.Next("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Length);
                return "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[index];
            }
            char GetRandomNumber()
            {
                int index = rng.Next("1234567890".Length);
                return "1234567890"[index];
            }

            RestartProcess:

            var plate = $"{GetRandomCharacter()}{GetRandomCharacter()}{GetRandomCharacter()}{GetRandomNumber()}{GetRandomNumber()}{GetRandomNumber()}";
            
            //Make sure plate doesn't exist.
            var filter = MongoDB.Driver.Builders<LicensePlate>.Filter.Eq(x => x.Plate, plate);
            if (DatabaseManager.NumberPlatesTable.Count(filter) > 0)
            {
                goto RestartProcess; //Restart
            }

            //Save.
            DatabaseManager.NumberPlatesTable.InsertOne(new LicensePlate() {Plate = plate});

            //Else return;
            return plate;
        }

        private void API_onPlayerExitVehicle(Client player, GTANetworkAPI.Vehicle vehicle)
        {
            var c = player.GetCharacter();

            if (c == null)
                return;

            if (!c.IsInDmvTest)
                return;

            if (player.HasData("DMV_VEHICLE"))
                VehicleManager.respawn_vehicle(((NetHandle)player.GetData("DMV_VEHICLE")).GetVehicle());
            // CONV NOTE: proper delay needed probably
            //API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));
            Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player));

            player.ResetData("DMV_VEHICLE");
            c.IsInDmvTest = false;
            NAPI.ClientEvent.TriggerClientEvent(player, "DMV_CANCEL_TEST");

            NAPI.Chat.SendChatMessageToPlayer(player, "Test Cancelled.");
        }

        private void API_onResourceStart()
        {
            //Creating the vehicles.
            foreach (var car in _testVehicles)
            {
                car[2] = VehicleManager.CreateVehicle(VehicleHash.Asea, car[0], car[1], "DMV", 0,
                    vehicle_manager.GameVehicle.VehTypePerm, 89, 89);

                VehicleManager.spawn_vehicle(car[2]);
            }

            ObjectRemoval.RegisterObject(new Vector3(266.102691650391, -348.641571044922, 43.7301368713379), 242636620);
            ObjectRemoval.RegisterObject(new Vector3(285.719482421875, -356.067474365234, 44.1401863098145), 406416082);

            NAPI.Util.ConsoleOutput("Spawned DMV Vehicles.");
        }

        private void OnVehicleEngineToggle(Client player, NetHandle vehicle, bool state)
        {
            if (state == true)
            {
                if (vehicle.GetVehicle() == null)
                    return;

                if (_testVehicles.Any(x => x[2] == vehicle.GetVehicle()))
                {
                    var c = player.GetCharacter();

                    if (c.IsInDmvTest)
                    {
                        c.TimeStartedDmvTest = DateTime.Now;
                        player.SetData("DMV_VEHICLE", vehicle);
                        NAPI.ClientEvent.TriggerClientEvent(player, "DMV_STARTTEST", _testCheckpoints, vehicle);
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~** GO! You'll have to finish in less than or equal to 5 minutes and with more than 999 damage to the vehicle.");
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~** You have plently of time so drive safe and make sure you don't break your vehicle.");
                    }
                    else
                    {
                        if (!player.GetAccount().AdminDuty)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, "You haven't started the driving test.");
                            NAPI.Vehicle.SetVehicleEngineStatus(vehicle, false);
                            // CONV NOTE: proper delay needed probably
                            //API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));
                            Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player));
                        }
                    }
                }

            }
        }

        [Command("starttest"), Help(HelpManager.CommandGroups.Vehicles, "Starts driving test. (Must be at a DMV)")]
        public void StartTest(Client player)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop?.Type != PropertyManager.PropertyTypes.DMV)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You need to be at a DMV.");
                return;
            }

            var c = player.GetCharacter();

            if (InventoryManager.DoesInventoryHaveItem<IdentificationItem>(c).Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must have a valid identification to do your driving test.");
                return;
            }

            if (InventoryManager.DoesInventoryHaveItem<DrivingLicenseItem>(c).Length > 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You already have a valid driving license.");
                return;
            }

            if (c.IsInDmvTest)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You already started the DMV test.");
                return;
            }

            if (Money.GetCharacterMoney(c) < prop.ItemPrices["drivingtest"])
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You need $" + prop.ItemPrices["drivingtest"] + " to do the test.");
                return;
            }

            InventoryManager.DeleteInventoryItem<Money>(c, prop.ItemPrices["drivingtest"]);

            c.IsInDmvTest = true;

            NAPI.Chat.SendChatMessageToPlayer(player, "You've started the driving test, please head to a DMV vehicle.");
            NAPI.Chat.SendChatMessageToPlayer(player, "~r~ The timer begins once you start the engine of the vehicle. Exit the vehicle to cancel the test.");
        }

        [Command("registervehicle"), Help(HelpManager.CommandGroups.Vehicles, "Register your vehicle. (At DMV)")]
        public void RegisterVehicle(Client player)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop?.Type != PropertyManager.PropertyTypes.DMV)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You need to be at a DMV.");
                return;
            }

            var c = player.GetCharacter();

            if (InventoryManager.DoesInventoryHaveItem<IdentificationItem>(c).Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must have a valid identification.");
                return;
            }

            if (InventoryManager.DoesInventoryHaveItem<DrivingLicenseItem>(c).Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must have a valid driving license.");
                return;
            }

            string[][] vehList = c.OwnedVehicles.Where(x => x.IsRegistered == false).Select(x => new[] {VehicleOwnership.returnCorrDisplayName(x.VehModel), x.Id.ToString() }).ToArray();
            NAPI.ClientEvent.TriggerClientEvent(player, "DMV_SELECTVEHICLE", NAPI.Util.ToJson(vehList));
        }

        [Command("showlicense"), Help(HelpManager.CommandGroups.Vehicles, "Show your driving license to someone.", "Id of target.")]
        public void ShowLicense(Client player, string target)
        {
            var targetPlayer = PlayerManager.ParseClient(target);
            if (targetPlayer == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "That player is not online.");
                return;
            }

            var c = player.GetCharacter();

            if (InventoryManager.DoesInventoryHaveItem<DrivingLicenseItem>(c).Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't have a driving license.");
                return;
            }

            if (targetPlayer.Position.DistanceTo(player.Position) > 3.0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "The player must be near you.");
                return;
            }

            NAPI.Chat.SendChatMessageToPlayer(targetPlayer, " [************** Driving License ~g~VALID~w~ **************]");
            NAPI.Chat.SendChatMessageToPlayer(targetPlayer, $"* Name: ~h~{c.rp_name()}~h~ | Age: ~h~{c.Age}~h~");
            NAPI.Chat.SendChatMessageToPlayer(targetPlayer, $"* DOB: ~h~{c.Birthday}~h~ | Birth Place: ~h~{c.Birthplace}~h~");
            NAPI.Chat.SendChatMessageToPlayer(targetPlayer, " [**********************************************************]");

            ChatManager.RoleplayMessage(player, "shows his driving license to " + targetPlayer.GetCharacter().rp_name(), ChatManager.RoleplayMe);
        }

        [Command("showregistration", Alias = "showreg"), Help(HelpManager.CommandGroups.Vehicles, "Show your vehicles registeration to someone.", "Id of target.")]
        public void ShowReg(Client player, string target)
        {
            var targetPlayer = PlayerManager.ParseClient(target);
            if (targetPlayer == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "That player is not online.");
                return;
            }

            var c = player.GetCharacter();

            if (!c.OwnedVehicles.Any(x => x.IsRegistered))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't have any registered vehicles.");
                return;
            }

            if (targetPlayer.Position.DistanceTo(player.Position) > 3.0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "The player must be near you.");
                return;
            }

            NAPI.Chat.SendChatMessageToPlayer(targetPlayer, $" [************** Vehicles Of {c.rp_name()} **************]");
            foreach (var veh in c.OwnedVehicles.Where(x => x.IsRegistered))
            {
                NAPI.Chat.SendChatMessageToPlayer(targetPlayer, $"* Model: {VehicleOwnership.returnCorrDisplayName(veh.VehModel)} | Registration: {veh.LicensePlate}");
            }
            NAPI.Chat.SendChatMessageToPlayer(targetPlayer, " [**********************************************************]");

            ChatManager.RoleplayMessage(player, "shows his vehicle registrations to " + targetPlayer.GetCharacter().rp_name(), ChatManager.RoleplayMe);
        }
    }

    public class LicensePlate
    {
        public string Plate { get; set; }
    }
}
