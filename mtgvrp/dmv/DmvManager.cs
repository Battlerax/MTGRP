using System;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.database_manager;
using mtgvrp.group_manager.lsgov;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;

namespace mtgvrp.dmv
{
    public class DmvManager : Script
    {
        private readonly Vector3[] _testCheckpoints =
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

        private readonly dynamic[][] _testVehicles =
        {
            new dynamic[] {new Vector3(266.3742, -332.2829, 44.48646), new Vector3(0.02338559, -0.0002331526, -109.5953), null},
            new dynamic[] {new Vector3(267.7154, -329.0442, 44.48596), new Vector3(0.02338559, -0.0002331526, -109.5953), null},
            new dynamic[] {new Vector3(268.8224, -325.9093, 44.48621), new Vector3(0.02338559, -0.0002331526, -109.5953), null},
            new dynamic[] {new Vector3(269.9587, -322.6313, 44.48601), new Vector3(0.02338559, -0.0002331526, -109.5953), null},
            new dynamic[] {new Vector3(271.149, -319.2983, 44.4863), new Vector3(0.02338559, -0.0002331526, -109.5953), null},
        };

        public DmvManager()
        {
            API.onResourceStart += API_onResourceStart;
            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;
            API.onPlayerExitVehicle += API_onPlayerExitVehicle;
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            if (eventName == "DMV_REGISTER_VEHICLE")
            {
                var prop = PropertyManager.IsAtPropertyInteraction(player);
                if (prop?.Type != PropertyManager.PropertyTypes.DMV)
                {
                    API.sendChatMessageToPlayer(player, "You need to be at a DMV.");
                    return;
                }

                var c = player.GetCharacter();

                if (InventoryManager.DoesInventoryHaveItem<IdentificationItem>(c).Length == 0)
                {
                    API.sendChatMessageToPlayer(player, "You must have a valid identification.");
                    return;
                }

                if (InventoryManager.DoesInventoryHaveItem<DrivingLicenseItem>(c).Length == 0)
                {
                    API.sendChatMessageToPlayer(player, "You must have a valid driving license.");
                    return;
                }

                int vehid = Convert.ToInt32(arguments[0]);
                if (!c.OwnedVehicles.Exists(x => x == vehid))
                {
                    API.sendChatMessageToPlayer(player, "You don't own that vehicle!!!");
                    return;
                }

                var veh = VehicleManager.Vehicles.FirstOrDefault(x => x.Id == vehid);
                if (veh == null || veh.OwnerId != c.Id)
                {
                    API.sendChatMessageToPlayer(player, "You don't own that vehicle!!!");
                    return;
                }

                if (veh.IsRegistered)
                {
                    API.sendChatMessageToPlayer(player, "Vehicle is already registered!!!");
                    return;
                }

                if (Money.GetCharacterMoney(c) < 100)
                {
                    API.sendChatMessageToPlayer(player, "You need $100 to register your vehicle.");
                    return;
                }

                //Set License Plate.
                veh.LicensePlate = GetValidLicensePlate();
                veh.IsRegistered = true;

                if (veh.IsSpawned)
                {
                    VehicleManager.respawn_vehicle(veh);
                }

                //Remove money.
                InventoryManager.DeleteInventoryItem<Money>(c, 100);

                API.sendChatMessageToPlayer(player, $"You've sucessfully registered your {API.getVehicleDisplayName(veh.VehModel)}. License: {veh.LicensePlate}");
                veh.Save();
            }
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

            var plate = $"{GetRandomCharacter()}{GetRandomCharacter()}{GetRandomCharacter()} {GetRandomNumber()}{GetRandomNumber()}{GetRandomNumber()}";
            
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

        private void API_onPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            var c = player.GetCharacter();

            if (!c.IsInDmvTest)
                return;

            if (player.hasData("DMV_VEHICLE"))
                VehicleManager.respawn_vehicle(((NetHandle)player.getData("DMV_VEHICLE")).GetVehicle());
            API.warpPlayerOutOfVehicle(player);

            player.resetData("DMV_VEHICLE");
            c.IsInDmvTest = false;

            if (c.NextDmvCheckpointColShape != null)
                API.deleteColShape(c.NextDmvCheckpointColShape);

            API.triggerClientEvent(player, "DMV_UPDATE_MARKER", new Vector3(), new Vector3());

            API.sendChatMessageToPlayer(player, "Test Cancelled.");
        }

        private void API_onResourceStart()
        {
            //Creating the vehicles.
            foreach (var car in _testVehicles)
            {
                car[2] = VehicleManager.CreateVehicle(VehicleHash.Asea, car[0], car[1], "DMV", 0,
                    vehicle_manager.Vehicle.VehTypePerm, 89, 89);

                VehicleManager.spawn_vehicle(car[2]);
            }

            ObjectRemoval.RegisterObject(new Vector3(266.102691650391, -348.641571044922, 43.7301368713379), 242636620);
            ObjectRemoval.RegisterObject(new Vector3(285.719482421875, -356.067474365234, 44.1401863098145), 406416082);

            API.consoleOutput("Spawned DMV Vehicles.");
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            if (_testVehicles.Any(x => x[2] == vehicle.GetVehicle()))
            {
                var c = player.GetCharacter();

                if (c.IsInDmvTest)
                {
                    c.TimeStartedDmvTest = DateTime.Now;
                    c.DmvTestStep = 0;
                    NextCheckpoint(player);
                    player.setData("DMV_VEHICLE", vehicle);
                    API.sendChatMessageToPlayer(player, "~y~** GO! You'll have to finish in less than or equal to 2 minutes and with less than 995 damage to the vehicle.");
                }
                else
                {
                    if (!player.GetAccount().AdminDuty)
                    {
                        API.sendChatMessageToPlayer(player, "You haven't started the driving test.");
                        API.warpPlayerOutOfVehicle(player);
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
                API.sendChatMessageToPlayer(player, "You need to be at a DMV.");
                return;
            }

            var c = player.GetCharacter();

            if (InventoryManager.DoesInventoryHaveItem<IdentificationItem>(c).Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You must have a valid identification to do your driving test.");
                return;
            }

            if (InventoryManager.DoesInventoryHaveItem<DrivingLicenseItem>(c).Length > 0)
            {
                API.sendChatMessageToPlayer(player, "You already have a valid driving license.");
                return;
            }

            if (c.IsInDmvTest)
            {
                API.sendChatMessageToPlayer(player, "You already started the DMV test.");
                return;
            }

            if (Money.GetCharacterMoney(c) < prop.ItemPrices["drivingtest"])
            {
                API.sendChatMessageToPlayer(player, "You need $" + prop.ItemPrices["drivingtest"] + " to do the test.");
            }

            InventoryManager.DeleteInventoryItem<Money>(c, prop.ItemPrices["drivingtest"]);

            c.IsInDmvTest = true;

            API.sendChatMessageToPlayer(player, "You've started the driving test, please head to a DMV vehicle to start.");
            API.sendChatMessageToPlayer(player, "~r~ The timer starts once you enter the vehicle. Remember to start your engine. Exit the vehicle to cancel the test.");
        }

        void NextCheckpoint(Client player)
        {
            var c = player.GetCharacter();

            if (c.NextDmvCheckpointColShape != null)
                API.deleteColShape(c.NextDmvCheckpointColShape);

            //Check next.
            if (c.DmvTestStep < _testCheckpoints.Length)
            {
                c.NextDmvCheckpointColShape = API.createSphereColShape(_testCheckpoints[c.DmvTestStep], 5.0f);
                c.NextDmvCheckpointColShape.onEntityEnterColShape += (shape, entity) =>
                {
                    if (entity != player) return;

                    if (player.getData("DMV_VEHICLE") != player.vehicle.handle)
                    {
                        API.sendChatMessageToPlayer(player, "This isn't your test vehicle.");
                        return;
                    }

                    c.DmvTestStep += 1;
                    NextCheckpoint(player);
                };
                API.triggerClientEvent(player, "DMV_UPDATE_MARKER",
                    _testCheckpoints[c.DmvTestStep].Subtract(new Vector3(0, 0, 0.3)), c.DmvTestStep + 1 < _testCheckpoints.Length ? _testCheckpoints[c.DmvTestStep + 1].Subtract(new Vector3(0, 0, 0.3)) : new Vector3());
            }
            else
            {
                API.triggerClientEvent(player, "DMV_UPDATE_MARKER", new Vector3(), new Vector3());

                var isOnTime = DateTime.Now.Subtract(c.TimeStartedDmvTest) <= TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(5));
                var isOnHealth = player.vehicle.health >= 995;

                if (isOnTime && isOnHealth)
                {
                    InventoryManager.GiveInventoryItem(c, new DrivingLicenseItem(), 1, true);
                    API.sendChatMessageToPlayer(player, "You have ~g~COMPLETED~w~ your test.");
                }
                else
                    API.sendChatMessageToPlayer(player, "You have ~r~FAILED~w~ your test.");

                API.sendChatMessageToPlayer(player,
                    isOnTime
                        ? $"* Time: ~g~ {DateTime.Now.Subtract(c.TimeStartedDmvTest).Minutes:D2}:{DateTime.Now.Subtract(c.TimeStartedDmvTest).Seconds:D2} / 02:05"
                        : $"* Time: ~r~ {DateTime.Now.Subtract(c.TimeStartedDmvTest).Minutes:D2}:{DateTime.Now.Subtract(c.TimeStartedDmvTest).Seconds:D2} / 02:05");

                API.sendChatMessageToPlayer(player,
                    isOnHealth
                        ? $"* Vehicle Health: ~g~ {player.vehicle.health} / 995"
                        : $"* Vehicle Health: ~r~ {player.vehicle.health} / 995");

                VehicleManager.respawn_vehicle(player.vehicle.handle.GetVehicle());
                API.warpPlayerOutOfVehicle(player);

                c.IsInDmvTest = false;
            }

        }

        [Command("registervehicle"), Help(HelpManager.CommandGroups.Vehicles, "Register your vehicle. (At DMV)")]
        public void RegisterVehicle(Client player)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop?.Type != PropertyManager.PropertyTypes.DMV)
            {
                API.sendChatMessageToPlayer(player, "You need to be at a DMV.");
                return;
            }

            var c = player.GetCharacter();

            if (InventoryManager.DoesInventoryHaveItem<IdentificationItem>(c).Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You must have a valid identification.");
                return;
            }

            if (InventoryManager.DoesInventoryHaveItem<DrivingLicenseItem>(c).Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You must have a valid driving license.");
                return;
            }

            string[][] vehList = VehicleManager.Vehicles.Where(x => c.OwnedVehicles.Exists(y => x.Id == y) && x.IsRegistered == false).Select(x => new[] {API.getVehicleDisplayName(x.VehModel), x.Id.ToString() }).ToArray();
            API.triggerClientEvent(player, "DMV_SELECTVEHICLE", API.toJson(vehList));
        }

        [Command("showlicense"), Help(HelpManager.CommandGroups.Vehicles, "Show your driving license to someone.", "Id of target.")]
        public void ShowLicense(Client player, string target)
        {
            var targetPlayer = PlayerManager.ParseClient(target);
            if (targetPlayer == null)
            {
                API.sendChatMessageToPlayer(player, "That player is not online.");
                return;
            }

            var c = player.GetCharacter();

            if (InventoryManager.DoesInventoryHaveItem<DrivingLicenseItem>(c).Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have a driving license.");
                return;
            }

            if (targetPlayer.position.DistanceTo(player.position) > 3.0)
            {
                API.sendChatMessageToPlayer(player, "The player must be near you.");
                return;
            }

            API.sendChatMessageToPlayer(targetPlayer, " [************** Driving License ~g~VALID~w~ **************]");
            API.sendChatMessageToPlayer(targetPlayer, $"* Name: ~h~{c.rp_name()}~h~ | Age: ~h~{c.Age}~h~");
            API.sendChatMessageToPlayer(targetPlayer, $"* DOB: ~h~{c.Birthday}~h~ | Birth Place: ~h~{c.Birthplace}~h~");
            API.sendChatMessageToPlayer(targetPlayer, " [**********************************************************]");

            ChatManager.RoleplayMessage(player, "shows his driving license to " + targetPlayer.GetCharacter().rp_name(), ChatManager.RoleplayMe);
        }

        [Command("showregistration", Alias = "showreg"), Help(HelpManager.CommandGroups.Vehicles, "Show your vehicles registeration to someone.", "Id of target.")]
        public void ShowReg(Client player, string target)
        {
            var targetPlayer = PlayerManager.ParseClient(target);
            if (targetPlayer == null)
            {
                API.sendChatMessageToPlayer(player, "That player is not online.");
                return;
            }

            var c = player.GetCharacter();

            if (!VehicleManager.Vehicles.Where(x => c.OwnedVehicles.Contains(x.Id)).Any(x => x.IsRegistered))
            {
                API.sendChatMessageToPlayer(player, "You don't have any registered vehicles.");
                return;
            }

            if (targetPlayer.position.DistanceTo(player.position) > 3.0)
            {
                API.sendChatMessageToPlayer(player, "The player must be near you.");
                return;
            }

            API.sendChatMessageToPlayer(targetPlayer, $" [************** Vehicles Of {c.rp_name()} **************]");
            foreach (var veh in VehicleManager.Vehicles.Where(x => c.OwnedVehicles.Contains(x.Id)).Where(x => x.IsRegistered))
            {
                API.sendChatMessageToPlayer(player, $"* Model: {API.getVehicleDisplayName(veh.VehModel)} | Registeration: {veh.LicensePlate}");
            }
            API.sendChatMessageToPlayer(targetPlayer, " [**********************************************************]");

            ChatManager.RoleplayMessage(player, "shows his vehicle registrations to " + targetPlayer.GetCharacter().rp_name(), ChatManager.RoleplayMe);
        }
    }

    public class LicensePlate
    {
        public string Plate { get; set; }
    }
}
