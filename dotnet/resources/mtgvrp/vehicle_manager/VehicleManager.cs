/*
 *  File: VehicleManager.cs
 *  Author: Chenko
 *  Date: 12/24/2016
 * 
 * 
 *  Purpose: Loads vehicles from the database and provides functions to manage them
 * 
 * 
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;


using GTANetworkAPI;





using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.group_manager;
using mtgvrp.inventory;
using mtgvrp.job_manager;
using mtgvrp.player_manager;
using MongoDB.Driver;
using mtgvrp.core.Help;
using mtgvrp.dmv;
using mtgvrp.vehicle_manager.modding;
using Color = mtgvrp.core.Color;
using System.Threading.Tasks;

namespace mtgvrp.vehicle_manager
{
    public class VehicleManager : Script
    {
        public static List<GameVehicle> Vehicles = new List<GameVehicle>();

        public static readonly Dictionary<int, string> VehicleColors = new Dictionary<int, string>()
        {
            {0, "Metallic Black"},
            {1, "Metallic Graphite Black"},
            {2, "Metallic Black Steal"},
            {3, "Metallic Dark Silver"},
            {4, "Metallic Silver"},
            {5, "Metallic Blue Silver"},
            {6, "Metallic Steel Gray"},
            {7, "Metallic Shadow Silver"},
            {8, "Metallic Stone Silver"},
            {9, "Metallic Midnight Silver"},
            {10, "Metallic Gun Metal"},
            {11, "Metallic Anthracite Grey"},
            {12, "Matte Black"},
            {13, "Matte Gray"},
            {14, "Matte Light Grey"},
            {15, "Util Black"},
            {16, "Util Black Poly"},
            {17, "Util Dark silver"},
            {18, "Util Silver"},
            {19, "Util Gun Metal"},
            {20, "Util Shadow Silver"},
            {21, "Worn Black"},
            {22, "Worn Graphite"},
            {23, "Worn Silver Grey"},
            {24, "Worn Silver"},
            {25, "Worn Blue Silver"},
            {26, "Worn Shadow Silver"},
            {27, "Metallic Red"},
            {28, "Metallic Torino Red"},
            {29, "Metallic Formula Red"},
            {30, "Metallic Blaze Red"},
            {31, "Metallic Graceful Red"},
            {32, "Metallic Garnet Red"},
            {33, "Metallic Desert Red"},
            {34, "Metallic Cabernet Red"},
            {35, "Metallic Candy Red"},
            {36, "Metallic Sunrise Orange"},
            {37, "Metallic Classic Gold"},
            {38, "Metallic Orange"},
            {39, "Matte Red"},
            {40, "Matte Dark Red"},
            {41, "Matte Orange"},
            {42, "Matte Yellow"},
            {43, "Util Red"},
            {44, "Util Bright Red"},
            {45, "Util Garnet Red"},
            {46, "Worn Red"},
            {47, "Worn Golden Red"},
            {48, "Worn Dark Red"},
            {49, "Metallic Dark Green"},
            {50, "Metallic Racing Green"},
            {51, "Metallic Sea Green"},
            {52, "Metallic Olive Green"},
            {53, "Metallic Green"},
            {54, "Metallic Gasoline Blue Green"},
            {55, "Matte Lime Green"},
            {56, "Util Dark Green"},
            {57, "Util Green"},
            {58, "Worn Dark Green"},
            {59, "Worn Green"},
            {60, "Worn Sea Wash"},
            {61, "Metallic Midnight Blue"},
            {62, "Metallic Dark Blue"},
            {63, "Metallic Saxony Blue"},
            {64, "Metallic Blue"},
            {65, "Metallic Mariner Blue"},
            {66, "Metallic Harbor Blue"},
            {67, "Metallic Diamond Blue"},
            {68, "Metallic Surf Blue"},
            {69, "Metallic Nautical Blue"},
            {70, "Metallic Bright Blue"},
            {71, "Metallic Purple Blue"},
            {72, "Metallic Spinnaker Blue"},
            {73, "Metallic Ultra Blue"},
            {74, "Metallic Bright Blue"},
            {75, "Util Dark Blue"},
            {76, "Util Midnight Blue"},
            {77, "Util Blue"},
            {78, "Util Sea Foam Blue"},
            {79, "Util Lightning blue"},
            {80, "Util Maui Blue Poly"},
            {81, "Util Bright Blue"},
            {82, "Matte Dark Blue"},
            {83, "Matte Blue"},
            {84, "Matte Midnight Blue"},
            {85, "Worn Dark blue"},
            {86, "Worn Blue"},
            {87, "Worn Light blue"},
            {88, "Metallic Taxi Yellow"},
            {89, "Metallic Race Yellow"},
            {90, "Metallic Bronze"},
            {91, "Metallic Yellow Bird"},
            {92, "Metallic Lime"},
            {93, "Metallic Champagne"},
            {94, "Metallic Pueblo Beige"},
            {95, "Metallic Dark Ivory"},
            {96, "Metallic Choco Brown"},
            {97, "Metallic Golden Brown"},
            {98, "Metallic Light Brown"},
            {99, "Metallic Straw Beige"},
            {100, "Metallic Moss Brown"},
            {101, "Metallic Biston Brown"},
            {102, "Metallic Beechwood"},
            {103, "Metallic Dark Beechwood"},
            {104, "Metallic Choco Orange"},
            {105, "Metallic Beach Sand"},
            {106, "Metallic Sun Bleeched Sand"},
            {107, "Metallic Cream"},
            {108, "Util Brown"},
            {109, "Util Medium Brown"},
            {110, "Util Light Brown"},
            {111, "Metallic White"},
            {112, "Metallic Frost White"},
            {113, "Worn Honey Beige"},
            {114, "Worn Brown"},
            {115, "Worn Dark Brown"},
            {116, "Worn straw beige"},
            {117, "Brushed Steel"},
            {118, "Brushed Black steel"},
            {119, "Brushed Aluminium"},
            {120, "Chrome"},
            {121, "Worn Off White"},
            {122, "Util Off White"},
            {123, "Worn Orange"},
            {124, "Worn Light Orange"},
            {125, "Metallic Securicor Green"},
            {126, "Worn Taxi Yellow"},
            {127, "police car blue"},
            {128, "Matte Green"},
            {129, "Matte Brown"},
            {130, "Worn Orange"},
            {131, "Matte White"},
            {132, "Worn White"},
            {133, "Worn Olive Army Green"},
            {134, "Pure White"},
            {135, "Hot Pink"},
            {136, "Salmon pink"},
            {137, "Metallic Vermillion Pink"},
            {138, "Orange"},
            {139, "Green"},
            {140, "Blue"},
            {141, "Mettalic Black Blue"},
            {142, "Metallic Black Purple"},
            {143, "Metallic Black Red"},
            {144, "hunter green"},
            {145, "Metallic Purple"},
            {146, "Metaillic V Dark Blue"},
            {147, "MODSHOP BLACK1"},
            {148, "Matte Purple"},
            {149, "Matte Dark Purple"},
            {150, "Metallic Lava Red"},
            {151, "Matte Forest Green"},
            {152, "Matte Olive Drab"},
            {153, "Matte Desert Brown"},
            {154, "Matte Desert Tan"},
            {155, "Matte Foilage Green"},
            {156, "DEFAULT ALLOY COLOR"},
            {157, "Epsilon Blue"},
            {158, "Pure Gold"},
            {159, "Brushed Gold"},
        };

        /*
        * 
        * ========== CONSTRUCTOR =========
        * 
        */
        public ColShape dropcarShape;
        public Vector3 dropcarPosition = new Vector3(487.0575, -1334.377, 29.30219);

        public Timer VehicleRespawnTimer = new Timer();

        public VehicleManager()
        {
            DebugManager.DebugMessage("[VehicleM] Initializing vehicle manager...");

            //Setup respawn timer.
            VehicleRespawnTimer.Interval = 5000;
            VehicleRespawnTimer.Elapsed += VehicleRespawnTimer_Elapsed;
            VehicleRespawnTimer.Start();

            //Register for on character enter to show his cars.
            CharacterMenu.OnCharacterLogin += CharacterMenu_OnCharacterLogin;

            // Create vehicle table + 
            //load_all_unowned_vehicles();
            dropcarShape = API.CreateCylinderColShape(dropcarPosition, 2f, 3f);

            dropcarShape.OnEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = NAPI.Player.GetPlayerFromHandle(entity)) != null)
                {
                    Character character = player.GetCharacter();

                    if (!character.IsOnDropcar)
                    {
                        return;
                    }

                    var veh = GetVehFromNetHandle(NAPI.Player.GetPlayerVehicle(player));
                    float payment = API.GetVehicleHealth(NAPI.Player.GetPlayerVehicle(player)) / 2;
                    VehicleManager.respawn_vehicle(veh);
                    API.Shared.SetVehicleEngineStatus(veh.Entity, false);
                    inventory.InventoryManager.GiveInventoryItem(character, new Money(), (int) payment);
                    character.IsOnDropcar = false;
                    NAPI.ClientEvent.TriggerClientEvent(player, "dropcar_removewaypoint");
                    player.SendChatMessage("Vehicle delivered. You earned $" + (int)payment);
                }
            };


            DebugManager.DebugMessage("[VehicleM] Vehicle Manager initalized!");
        }

        [RemoteEvent("VehicleStreamedForPlayer")]
        public void VehicleStreamedForPlayer(Client sender, params object[] arguments)
        {
            var veh = (Entity)arguments[0];

            //Sync horns
            if (veh.GetVehicle()?.VehMods?.ContainsKey("14") ?? false)
            {
                API.SendNativeToPlayer(sender, Hash.SET_VEHICLE_MOD, veh, 14,
                    Convert.ToInt32(veh.GetVehicle().VehMods["14"]));
            }

            //Sync tyre smokes
            if (veh.GetVehicle()?.VehMods?.ContainsKey(ModdingManager.TyresSmokeColorId.ToString()) ?? false)
            {
                API.SendNativeToPlayer(sender, Hash.TOGGLE_VEHICLE_MOD, veh, 20, true);
            }
            else
            {
                API.SendNativeToPlayer(sender, Hash.TOGGLE_VEHICLE_MOD, veh, 20, false);
            }
        }

        private void VehicleRespawnTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var x in Vehicles)
            {
                if (x.IsSpawned && NAPI.Vehicle.GetVehicleOccupants(x.Entity).Count == 0 && x.OwnerId == 0 && x.LastOccupied != new DateTime() && (DateTime.Now - x.LastOccupied) >= x.RespawnDelay)
                {
                    x.LastOccupied = new DateTime();
                    respawn_vehicle(x);
                }
            }
        }

        /*
        * 
        * ========== COMMANDS =========
        * 
        */

        [Command("spawnveh"), Help(HelpManager.CommandGroups.AdminLevel4, "To spawn only the sickiest of rides for you to use.", new[] { "Vehiclehash model/name", "Colour 1", "Colour 2", "Dimension" })]
        public void spawnveh_cmd(Client player, VehicleHash model, int color1 = 0, int color2 = 0, int dimension = 0)
        {
            var account = player.GetAccount();
            if (account.AdminLevel < 4)
            {
                return;
            }
            var pos = player.Position;
            var rot = player.Rotation;

            var veh = CreateVehicle(model, pos, rot, "ABC123", 0, GameVehicle.VehTypeTemp, color1, color2, dimension);
            spawn_vehicle(veh);
            
            NAPI.Player.SetPlayerIntoVehicle(player, veh.Entity, -1);

            if (API.Shared.GetVehicleClass(veh.VehModel) != 13) //If not cycle
                NAPI.Vehicle.SetVehicleEngineStatus(veh.Entity, true);

            NAPI.Chat.SendChatMessageToPlayer(player, "You have spawned a " + model);
            NAPI.Chat.SendChatMessageToPlayer(player, "This vehicle is unsaved and may behave incorrectly.");
            
        }

        [Command("savevehicle"), Help(HelpManager.CommandGroups.AdminLevel4, "Save a vehicle to the database", null)]
        public void savevehicle_cmd(Client player)
        {
            var account = player.GetAccount();
            if (account.AdminLevel < 4)
            {
                return;
            }
            var vehHandle = NAPI.Player.GetPlayerVehicle(player);
            var veh = GetVehFromNetHandle(vehHandle);

            if(veh == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ ERROR: You are not inside a vehicle.");
                return;
            }

            if(veh.VehType != GameVehicle.VehTypeTemp)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ ERROR: You must be inside a temporary vehicle to save it.");
                return;
            }

            if(veh.is_saved())
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ ERROR: This vehicle is already saved into the database.");
                return;
            }

            
            veh.Insert();

            veh.VehType = GameVehicle.VehTypePerm;
            veh.Save();

            NAPI.Chat.SendChatMessageToPlayer(player, "You have saved vehicle " + Vehicles.IndexOf(veh) + ". (SQL: " + veh.Id + ")");
        }

        [Command("vstorage"), Help(HelpManager.CommandGroups.Vehicles, "Used to use your vehicles boot.", null)]
        public void VehicleStorage(Client player)
        {
            var lastVehNetHandle = GetClosestVehicle(player, 10f);
            var lastVeh = lastVehNetHandle.GetVehicle();

            if (lastVeh == null) return;
            if (!DoesPlayerHaveVehicleAccess(player, lastVeh))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must have access to the vehicle.");
                return;
            }

            if (!API.GetVehicleDoorState(lastVeh.Entity, 5))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Trunk must be open to access the storage.");
                return;
            }

            if (lastVeh.Inventory == null) lastVeh.Inventory = new List<IInventoryItem>();
            InventoryManager.ShowInventoryManager(player, player.GetCharacter(), lastVeh, "Inventory: ", "Vehicle: ");
        }

        public delegate void OnVehicleEngineToggleHandle(Client player, Entity vehicle, bool state);
        public static event OnVehicleEngineToggleHandle OnVehicleEngineToggle;

        [Command("engine", Alias = "e"), Help(HelpManager.CommandGroups.Vehicles, "Turning on and off your vehicle.", null)]
        public static void engine_cmd(Client player)
        {
            Character character = player.GetCharacter();
            var vehicleHandle = API.Shared.GetPlayerVehicle(player);
            GameVehicle vehicle = API.Shared.GetEntityData(vehicleHandle, "Vehicle");
            if (vehicle == null) return;

            if (API.Shared.GetPlayerVehicleSeat(player) != -1)
            {
                player.SendChatMessage("You must be the driver of a vehicle to do this.");
                return;
            }

            var engineState = API.Shared.GetVehicleEngineStatus(vehicleHandle);
            var vehAccess = DoesPlayerHaveVehicleAccess(player, vehicle);
            if (!engineState)
            {
                if (vehicle.Fuel <= 0)
                {
                    API.Shared.SendChatMessageToPlayer(player, "The vehicle has no fuel.");
                    return;
                }
            }

            if (API.Shared.GetVehicleClass(vehicle.VehModel) == 13) // Cycles
            {
                API.Shared.SendChatMessageToPlayer(player, "The vehicle has no engine.");
                return;
            }

            if (vehAccess)
            {
                if (engineState)
                {
                    API.Shared.SetVehicleEngineStatus(vehicleHandle, false);
                    ChatManager.RoleplayMessage(character, "turns off the vehicle engine.", ChatManager.RoleplayMe);
                    OnVehicleEngineToggle?.Invoke(player, vehicleHandle, false);
                }
                else
                {
                    API.Shared.SetVehicleEngineStatus(vehicleHandle, true);
                    ChatManager.RoleplayMessage(character, "turns on the vehicle engine.", ChatManager.RoleplayMe);
                    OnVehicleEngineToggle?.Invoke(player, vehicleHandle, true);
                }
            }
            else
            {
                API.Shared.SendChatMessageToPlayer(player, "You don't have access to this vehicle.");
            }

        }

        [Command("hotwire"), Help(HelpManager.CommandGroups.Vehicles, "Used to turn on a vehicle when you don't have keys to it", null)]
        public static void hotwire_cmd(Client player)
        {
            if (player.IsInVehicle == false)
            {
                API.Shared.SendChatMessageToPlayer(player, "You are not in a vehicle.");
                return;
            }

            Character character = player.GetCharacter();
            var veh = API.Shared.GetPlayerVehicle(player);
            GameVehicle vehicle = API.Shared.GetEntityData(veh, "Vehicle");

            if (API.Shared.GetVehicleEngineStatus(veh) == true)
            {
                API.Shared.SendChatMessageToPlayer(player, "This vehicle is already started.");
                return;
            }

            if (API.Shared.GetVehicleClass(vehicle.VehModel) == 13) // Cycles
            {
                API.Shared.SendChatMessageToPlayer(player, "The vehicle has no engine.");
                return;
            }

            if (vehicle.Fuel < 1)
            {
                API.Shared.SendChatMessageToPlayer(player, "This vehicle has no fuel.");
                return;
            }

            if (API.Shared.GetVehicleLocked(veh))
            {
                API.Shared.SendChatMessageToPlayer(player, "The vehicle is locked.");
                return;
            }

            if (character.NextHotWire > DateTime.Now)
            {
                API.Shared.SendChatMessageToPlayer(player, $"You have to wait {character.NextHotWire.Subtract(DateTime.Now).Seconds} more second(s) before attempting to hotwire.");
                return;
            }

            ChatManager.RoleplayMessage(character, "attempts to hotwire the vehicle.", ChatManager.RoleplayMe);

            Random ran = new Random();

            var hotwireChance = ran.Next(100);

            if (hotwireChance < 40)
            {
                API.Shared.SetVehicleEngineStatus(veh, true);
                ChatManager.RoleplayMessage(character, "succeeded in hotwiring the vehicle.", ChatManager.RoleplayMe);
            }
            else
            {
                API.Shared.SetPlayerHealth(player, player.Health - 10);
                player.SendChatMessage("You attempted to hotwire the vehicle and got shocked!");
                ChatManager.RoleplayMessage(character, "failed to hotwire the vehicle.", ChatManager.RoleplayMe);
                character.NextHotWire = DateTime.Now.Add(TimeSpan.FromSeconds(10));
            }

        }

        [Command("dropcar"), Help(HelpManager.CommandGroups.Vehicles, "Use this to sell a vehicle that is unowned by a player for some quick cash.", null)]
        public void dropcar_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (player.IsInVehicle == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are not in a vehicle.");
                return;
            }

            if (character.DropcarReset > TimeManager.GetTimeStamp)
            {
                player.SendChatMessage($@"Please wait {TimeManager.SecondsToMinutes(character.DropcarReset - TimeManager.GetTimeStamp)} more minutes before dropping another car.");
                return;
            }

            var veh = GetVehFromNetHandle(NAPI.Player.GetPlayerVehicle(player));

            if (veh.Group != Group.None || veh.OwnerId != 0 || veh.Job != Job.None || veh.JobId != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "This is an owned vehicle.");
                return;
            }

            character.DropcarReset = TimeManager.GetTimeStampPlus(TimeSpan.FromMinutes(15));
            character.IsOnDropcar = true;
            NAPI.ClientEvent.TriggerClientEvent(player, "dropcar_setwaypoint", new Vector3(487.0575, -1334.377, 29.30219) - new Vector3(0, 0, 1));
            player.SendChatMessage("A waypoint has been set. Take this vehicle to the waypoint to earn money.");
        }

        [Command("lock"), Help(HelpManager.CommandGroups.Vehicles, "How to lock and unlock your vehicle.", null)]
        public void Lockvehicle_cmd(Client player)
        {
            var lastVehNetHandle = GetClosestVehicle(player, 10f);
            var lastVeh = lastVehNetHandle.GetVehicle();

            if (lastVeh == null) return;
            if (!DoesPlayerHaveVehicleAccess(player, lastVeh)) return;

            var lockState = API.GetVehicleLocked(lastVeh.Entity);
            if (lockState)
            {
                API.SetVehicleLocked(lastVeh.Entity, false);
                ChatManager.RoleplayMessage(player, "unlocks the doors of the vehicle.", ChatManager.RoleplayMe);
            }
            else
            {
                API.SetVehicleLocked(lastVeh.Entity, true);
                ChatManager.RoleplayMessage(player, "locks the doors of the vehicle.", ChatManager.RoleplayMe);
            }
        }

        [Command("respawnunownedcars"), Help(HelpManager.CommandGroups.AdminLevel4, "Used to find your character statistics", null)]
        public void respawnallcars_cmd(Client player)
        {
            var account = player.GetAccount();
            if (account.AdminLevel < 4)
            {
                return;
            }

            foreach (var v in Vehicles)
            {
                if (v.Driver == null && v.OwnerId == 0 && v.GroupId == 0)
                {
                    VehicleManager.respawn_vehicle(v);
                }
            }
            NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "All unowned and unoccupied vehicles have been respawned.");
            return;
        }

        [Command("respawnnearbycars"), Help(HelpManager.CommandGroups.AdminLevel4, "Respawns all the vehicles near you to their original pos.", new [] {"The radius around you where the vehicles will get respawned."})]
        public void respawnnearbycars_cmd(Client player, int radius = 15)
        {
            var account = player.GetAccount();
            if (account.AdminLevel < 4)
            {
                return;
            }

            if (radius <= 0)
            {
                return;
            }

            foreach (var v in Vehicles)
            {
                if (v.Driver == null)
                {
                    if (player.Position.DistanceTo(NAPI.Entity.GetEntityPosition(v.Entity)) <= radius)
                    {
                        VehicleManager.respawn_vehicle(v);
                    }
                }
            }
            NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "Respawned all unowned and unoccupied cars in a radius of " + radius);
        }

        [Command("groupvehicles", Alias = "gvehicles"), Help(HelpManager.CommandGroups.Vehicles, "Used to locate vehicles owned by your group.", null)]
        public void commandGroupVehicles(Client player)
        {
            Character character = player.GetCharacter();
            var group = character.Group;
            if(group != Group.None)
            {
                List<GameVehicle> gCarsList = new List<GameVehicle>();
                foreach(var v in Vehicles)
                {
                    if(v.GroupId == group.Id)
                    {
                        gCarsList.Add(v);
                    }
                }
                if (gCarsList.Count > 0)
                {
                    string[][] cars = gCarsList
                        .Select(x => new[]
                        {
                            VehicleOwnership.returnCorrDisplayName(x.VehModel), x.Id.ToString(),
                            x.Entity.Value.ToString(), x.LicensePlate
                        })
                        .ToArray();
                    NAPI.ClientEvent.TriggerClientEvent(player, "groupvehicles_showmenu", NAPI.Util.ToJson(cars.ToArray()));
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player,"Your group has no vehicles!");
                }
            }
        }

        /*
        * 
        * ========== CALLBACKS =========
        * 
        */

        private void CharacterMenu_OnCharacterLogin(object sender, CharacterMenu.CharacterLoginEventArgs e)
        {
            //Load them.
            Vehicles.AddRange(DatabaseManager.VehicleTable.Find(x => x.OwnerId == e.Character.Id).ToList());

            //Spawn his cars.
            var maxVehs = GetMaxOwnedVehicles(e.Character.Client);
            if (maxVehs > e.Character.OwnedVehicles.Count) maxVehs = e.Character.OwnedVehicles.Count;
            for (int i = 0; i < maxVehs; i++)
            {
                var car = e.Character.OwnedVehicles[i];
                if (car == null)
                    continue;

                if (spawn_vehicle(car) != 1)
                    NAPI.Util.ConsoleOutput($"There was an error spawning vehicle #{car.Id} of {e.Character.CharacterName}.");
            }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Client player, byte type, string reason)
        {
            //DeSpawn his cars.
            Character character = player.GetCharacter();
            if (character == null)
                return;
            var maxVehs = GetMaxOwnedVehicles(character.Client);
            if (maxVehs > character.OwnedVehicles.Count) maxVehs = character.OwnedVehicles.Count;
            for (int i = 0; i < maxVehs; i++)
            {
                var car = character.OwnedVehicles[i];
                if (car == null)
                    continue;

                if (despawn_vehicle(car) != 1)
                    NAPI.Util.ConsoleOutput($"There was an error despawning vehicle #{car.Id} of {character.CharacterName}.");
            }

            //Delete.
            Vehicles.RemoveAll(x => x.OwnerId == character.Id);
            if (character.IsJailed)
            {
                character.JailTimer.Stop();
            }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Client player, Vehicle vehicleHandle, sbyte seat)
        {
            // Admin check in future

            var veh = GetVehFromNetHandle(vehicleHandle);

            if (veh == null)
                return;

            NAPI.Blip.SetBlipTransparency(veh.Blip, 0);

            Character character = player.GetCharacter();
            Account account = player.GetAccount();

            //IS A GROUP VEHICLE
            if (veh.Group != null && character.Group != veh.Group && veh.Group != Group.None && seat == 0 && account.AdminDuty == false)
            {
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "You must be a member of " + veh.Group.Name + " to use this vehicle.");
                    //API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));
                    Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player)); // CONV NOTE: delay fixme
                    return;
                }
            }

            //IS A VIP VEHICLE
            if (veh.IsVip == true && account.VipLevel <= 1 && seat == 0)
            {
                player.SendChatMessage("This is a ~y~VIP~y~ vehicle. You must be a VIP to drive it.");
                //API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));
                Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player)); // CONV NOTE: delay fixme
                return;
            }

            if (account.AdminLevel > 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~w~[VehicleM] You have entered vehicle ~r~" + Vehicles.IndexOf(veh) + "(Owned by: " + PlayerManager.Players.SingleOrDefault(x => x.Id == veh.OwnerId)?.CharacterName + ")");
            }
            
            if (API.GetVehicleLocked(vehicleHandle))
            {
                //API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));
                Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player)); // CONV NOTE: delay fixme
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~The vehicle is locked.");
                return;
            }
            
            //Vehicle Interaction Menu Setup
            var vehInfo = VehicleOwnership.returnCorrDisplayName(veh.VehModel) + " - " + veh.LicensePlate;
            API.SetEntitySharedData(player.Handle, "CurrentVehicleInfo", vehInfo);
            API.SetEntitySharedData(player.Handle, "OwnsVehicle", DoesPlayerHaveVehicleAccess(player, veh));
            API.SetEntitySharedData(player.Handle, "CanParkCar", DoesPlayerHaveVehicleParkLockAccess(player, veh));

           NAPI.ClientEvent.TriggerClientEvent(player, "speedo_entervehicle", account.IsSpeedoOn);

            veh.Driver = player.GetCharacter();
            veh.LastOccupied = DateTime.Now;
            API.SetPlayerSeatbelt(player, true);

            foreach (var p in API.GetAllPlayers())
            {
                if (p == null)
                    continue;

                if (p.Position.DistanceTo(NAPI.Entity.GetEntityPosition(vehicleHandle)) <= 250.0f)
                {
                    API.SendNativeToPlayer(player, Hash.SET_ENTITY_INVINCIBLE, vehicleHandle, false);
                    API.SendNativeToPlayer(player, Hash.SET_ENTITY_PROOFS, vehicleHandle, 0, 0, 0, 0, 0, 0, 0, 0);
                    API.SendNativeToPlayer(player, Hash.SET_VEHICLE_TYRES_CAN_BURST, vehicleHandle, 1);
                    API.SendNativeToPlayer(player, Hash.SET_VEHICLE_WHEELS_CAN_BREAK, vehicleHandle, 1);
                    API.SendNativeToPlayer(player, Hash.SET_VEHICLE_CAN_BE_VISIBLY_DAMAGED, vehicleHandle, 1);
                }
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Client player, Vehicle vehicleHandle)
        {
            var veh = GetVehFromNetHandle(vehicleHandle);

            if (veh == null)
            {
                return;
            }

            API.SetBlipTransparency(veh.Blip, 100);

            if (veh.VehType == GameVehicle.VehTypeTemp)
            {
                despawn_vehicle(veh);
                delete_vehicle(veh);

                NAPI.Notification.SendNotificationToPlayer(player, "Your vehicle was deleted on exit because it was temporary.");
            }

            if (veh.Driver == player.GetCharacter())
                veh.Driver = null;

            Character character = player.GetCharacter();

            if (character == null)
                return;

            character.LastVehicle = veh;

            if (character.IsOnDropcar)
            {
                character.IsOnDropcar = false;
                NAPI.ClientEvent.TriggerClientEvent(player, "dropcar_removewaypoint");
                player.SendChatMessage("You exited the vehicle. The dropcar has ended.");
            }

            if (NAPI.Vehicle.GetVehicleOccupants(vehicleHandle).Count == 0)
            {
                foreach (var p in API.GetAllPlayers())
                {
                    if (p == null)
                        continue;

                    if (p.Position.DistanceTo(NAPI.Entity.GetEntityPosition(vehicleHandle)) <= 250.0f)
                    {
                        API.SendNativeToPlayer(player, Hash.SET_ENTITY_INVINCIBLE, vehicleHandle, true);
                        API.SendNativeToPlayer(player, Hash.SET_ENTITY_PROOFS, vehicleHandle, 1, 1, 1, 1, 1, 1, 1, 1);
                        API.SendNativeToPlayer(player, Hash.SET_VEHICLE_TYRES_CAN_BURST, vehicleHandle, 0);
                        API.SendNativeToPlayer(player, Hash.SET_VEHICLE_WHEELS_CAN_BREAK, vehicleHandle, 0);
                        API.SendNativeToPlayer(player, Hash.SET_VEHICLE_CAN_BE_VISIBLY_DAMAGED, vehicleHandle, 0);
                    }
                }
            }
        }

        [ServerEvent(Event.VehicleDeath)]
        public void OnVehicleDeath(Vehicle vehicleHandle)
        {
            var veh = GetVehFromNetHandle(vehicleHandle);
            if (veh == null) return;
            NAPI.Util.ConsoleOutput("Vehicle " + vehicleHandle + " died");
            Task.Delay(1000).ContinueWith(t => respawn_vehicle(veh));
        }

        /*
        * 
        * ========== FUNCTIONS =========
        * 
        */

        public static Vehicle GetClosestVehicle(Client sender, float distance = 1000.0f)
        {
            foreach (var veh in NAPI.Pools.GetAllVehicles())
            {
                Vector3 vehPos = API.Shared.GetEntityPosition(veh);
                float distanceVehicleToPlayer = sender.Position.DistanceTo(vehPos);
                if (distanceVehicleToPlayer < distance)
                {
                    distance = distanceVehicleToPlayer;
                    return veh;

                }
            }
            return null;
        }

        public static GameVehicle GetVehicleById(int id)
        {
            foreach (GameVehicle v in Vehicles)
            {
                if (v.Id == id) { return v; }
            }

            return null;
        }

        public static int GetMaxOwnedVehicles(Client chr)
        {
            Account acc = chr.GetAccount();
            switch (acc.VipLevel)
            {
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 2:
                    return 4;
                case 3:
                    return 5;
            }
            return 0;
        }

    public static GameVehicle CreateVehicle(VehicleHash model, Vector3 pos, Vector3 rot, string license, int ownerid, int vehtype, int color1 = 0, int color2 = 0, int dimension = 0)
        {
            var veh = new GameVehicle
            {
                VehModel = model,
                SpawnPos = pos,
                SpawnRot = rot,
                VehMods = new Dictionary<string, string>() { [ModdingManager.PrimaryColorId.ToString()] = color1.ToString(), [ModdingManager.SecondryColorId.ToString()] = color2.ToString() },
                SpawnDimension = dimension,
                LicensePlate = license,
                OwnerId = ownerid,
                VehType = vehtype
            };

            Vehicles.Add(veh);
            return veh;
        }

        public static void delete_vehicle(GameVehicle veh)
        {
            Vehicles.Remove(veh);
        }

        public static GameVehicle GetVehFromNetHandle(Entity Handle)
        {
            return API.Shared.GetEntityData(Handle, "Vehicle") ?? null;
        }

        public static int spawn_vehicle(GameVehicle veh, Vector3 pos)
        {
            var returnCode = veh.Spawn(pos);

            if (returnCode == 1)
            {
                API.Shared.SetEntityData(veh.Entity, "Vehicle", veh);
            }

            API.Shared.SetVehicleEngineStatus(veh.Entity, false);
            if (veh.OwnerId != 0)
            {
                API.Shared.SetVehicleLocked(veh.Entity, true);
            }

            if (veh.Group.CommandType == Group.CommandTypeLspd)
            {
                API.Shared.SetVehicleEnginePowerMultiplier(veh.Entity, 4);
            }

            //Install modifications.
            ModdingManager.ApplyVehicleMods(veh);

            //Set wheel type. CONV NOTE: ERROR
            //API.SetVehicleWheelType(veh.Entity, VehicleInfo.Get(veh.VehModel).wheelType);
            return returnCode;
        }

        public static int spawn_vehicle(GameVehicle veh)
        {
            return spawn_vehicle(veh, veh.SpawnPos);
        }

        public static int despawn_vehicle(GameVehicle veh)
        {
            var returnCode = veh.Despawn();

            if (returnCode == 1)
            {
                API.Shared.ResetEntityData(veh.Entity, "Vehicle");
            }

            return returnCode;
        }

        public static int respawn_vehicle(GameVehicle veh, Vector3 pos)
        {
            if (API.Shared.HasEntityData(veh.Entity, "Vehicle"))
            {
                despawn_vehicle(veh);
            }
            return spawn_vehicle(veh, pos);
        }

        public static int respawn_vehicle(GameVehicle veh)
        {
            if (veh == null)
                return -1;

            return respawn_vehicle(veh, veh.SpawnPos);
        }

        public static bool DoesPlayerHaveVehicleAccess(Client player, GameVehicle vehicle)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

            if (vehicle == null)
                return false;

            if (account.AdminLevel >= 3 && account.AdminDuty) { return true; }
            if (character.Id == vehicle.OwnerId) { return true; }
            if (vehicle.GroupId == character.GroupId && character.GroupId != 0) return true;
            if (character.JobOne == vehicle.Job && character.JobOne != Job.None) { return true; }
            if (DmvManager._testVehicles.Any(x => x[2] == vehicle))
                return true;
            
            return false;
        }

        public static bool DoesPlayerHaveVehicleParkLockAccess(Client player, GameVehicle vehicle)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

            if (account.AdminLevel >= 3 && account.AdminDuty) { return true; }
            if (character.Id == vehicle.OwnerId) { return true; }
            if (vehicle.GroupId == character.GroupId && character.GroupId != 0) return true;
            return false;
        }

        /*public void load_all_group_vehicles()
        {
            int j = 0;

            foreach (var i in DatabaseManager.GroupTable.Find(Builders<Group>.Filter.Empty).ToList())
            {

                var filter = Builders<Vehicle>.Filter.Eq("GroupId", i.Id);
                var groupVehicles = DatabaseManager.VehicleTable.Find(filter).ToList();

                foreach (var v in groupVehicles)
                {
                    spawn_vehicle(v);
                    Vehicles.Add(v);
                    j++;
                }
            }

            DebugManager.DebugMessage("Loaded " + j + " group vehicles from the database.");
        }*/

        public static void load_all_unowned_vehicles()
        {
            var filter = Builders<GameVehicle>.Filter.Eq("OwnerId", "0");
            var unownedVehicles = DatabaseManager.VehicleTable.Find(filter).ToList();

            foreach (var v in unownedVehicles)
            {
                spawn_vehicle(v);

                v.Job = JobManager.GetJobById(v.JobId);
                v.Group = GroupManager.GetGroupById(v.GroupId);

                API.Shared.SetBlipTransparency(v.Blip, 100);

                Vehicles.Add(v);
            }

            DebugManager.DebugMessage("Loaded " + unownedVehicles.Count + " unowned vehicles from the database.");
        }
    }
}
