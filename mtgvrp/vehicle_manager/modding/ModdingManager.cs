using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using GrandTheftMultiplayer.Server;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using VehicleInfoLoader;
using VehicleInfoLoader.Data;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.property_system;
using Newtonsoft.Json;

namespace mtgvrp.vehicle_manager.modding
{
    public class ModdingManager : Script
    {
        public ModdingManager()
        {
            API.onResourceStart += API_onResourceStart;
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "MODDING_GETMODS":
                {
                    var modsList = new List<string[]>();

                    var modType = ModTypes.First(x => x.Value == (string) arguments[0]).Key;
                    var manifest = VehicleInfo.Get(sender.vehicle);
                    foreach (var modid in manifest.ModIds(modType))
                    {
                        var price = GetModPrice(modType, modid);
                        if (price == -1)
                            continue;

                        var m = manifest.Mod(modType, modid);
                        bool isVip = _vipMods.ContainsKey(modType) &&
                                     (_vipMods[modType] == -1 || _vipMods[modType] == modid);

                        modsList.Add(new string[]
                        {
                            m.localizedName == "" ? "No Name" : (m.HasFlag("chrome") ? "CHROME - " + m.localizedName : m.localizedName) , modType.ToString(), modid.ToString(), price.ToString(),
                            isVip == true ? "true" : "false"
                        });
                    }
                    API.triggerClientEvent(sender, "MODDING_FILL_MODS", API.toJson(modsList.ToArray()));
                    break;
                }

                case "MODDING_EXITMENU":
                {
                    var vehicle = sender.vehicle;
                    ClearVehicleMods(vehicle.handle.GetVehicle());
                    ApplyVehicleMods(vehicle.handle.GetVehicle());
                    API.setEntityPosition(sender.vehicle, sender.getData("ModLastPos"));
                    API.setEntityDimension(sender.vehicle, 0);
                    API.setEntityDimension(sender, 0);
                    if (sender.GetAccount().IsSpeedoOn)
                        API.triggerClientEvent(sender, "TOGGLE_SPEEDO");
                        break;
                }

                case "MODDONG_PURCHASE_ITEMS":
                {
                    dynamic items = JsonConvert.DeserializeObject((string) arguments[0]);
                    int allPrices = 0;
                    foreach (var itm in items)
                    {
                        var modType = int.Parse(itm[0].ToString().Trim());
                        int modId;
                        int.TryParse(itm[1].ToString().Trim(), out modId);

                        var price = GetModPrice(modType, modId);
                        allPrices += price;
                    }
                    var c = sender.GetCharacter();
                    if (Money.GetCharacterMoney(c) < allPrices)
                    {
                        API.triggerClientEvent(sender, "MODDING_ERROR",
                            "You don't have enough money to purchase these mods. Your balance: " +
                            Money.GetCharacterMoney(c));
                        return;
                    }

                    InventoryManager.DeleteInventoryItem<Money>(c, allPrices);

                    var veh = sender.vehicle.handle.GetVehicle();
                    foreach (var itm in items)
                    {
                        var modType = int.Parse(itm[0].ToString().Trim());
                        AddVehicleMod(veh, modType, itm[1].ToString().Trim());
                    }
                    ClearVehicleMods(veh);
                    ApplyVehicleMods(veh);
                    veh.Save();
                    API.triggerClientEvent(sender, "MODDING_CLOSE");
                    API.sendChatMessageToPlayer(sender,
                        "You have successfully purchased some vehicle mods for a total of ~g~" +
                        allPrices.ToString("C"));
                    API.setEntityPosition(sender.vehicle, sender.getData("ModLastPos"));
                    API.setEntityDimension(sender.vehicle, 0);
                    API.setEntityDimension(sender, 0);
                    if (sender.GetAccount().IsSpeedoOn)
                        API.triggerClientEvent(sender, "TOGGLE_SPEEDO");
                        break;
                }
            }
        }

        private void API_onResourceStart()
        {
            VehicleInfo.Setup(Path.Combine(API.getResourceFolder(), @"vehicle_manager/modding/modinfo"));
        }

        void AddVehicleMod(Vehicle veh, int type, string mod)
        {
            if(veh.VehMods == null)
                veh.VehMods = new Dictionary<string, string>();

            if (veh.VehMods.ContainsKey(type.ToString()))
            {
                veh.VehMods[type.ToString()] = mod;
            }
            else
            {
                veh.VehMods.Add(type.ToString(), mod);
            }
        }

        private static readonly Dictionary<int, int> _modTypePrices = new Dictionary<int, int>
        {
            {0, 1000},
            {1, 1500},
            {2, 1500},
            {3, 750},
            {4, 800},
            {5, 500},
            {6, 900},
            {7, 1000},
            {8, 600},
            {9, 600},
            {10, 1200},
            {11, 0},
            {12, 0},
            {13, 0},
            {14, 500},
            {15, 0},
            {16, -1},
            {18, -1},
            {22, 0},
            {23, 600},
            {24, 600},
            {25, 200},
            {27, 200},
            {28, -1},
            {30, 100},
            {33, 100},
            {34, 100},
            {35, 100},
            {38, 2000},
            {48, 100},
            {62, -1},
            {66, -1},
            {67, -1},
            {69, -1},
            {PrimaryColorId, 100},
            {SecondryColorId, 100},
            {TyresSmokeColorId, 500},
            {NeonColorId, 300},
            {WindowTintId, 200},
        };

        private static readonly Dictionary<KeyValuePair<int, int>, int> _modPrices = new Dictionary<KeyValuePair<int, int>, int>
        {
            {new KeyValuePair<int, int>(11, 0), 4000},
            {new KeyValuePair<int, int>(11, 1), 6000},
            {new KeyValuePair<int, int>(11, 2), 8000},
            {new KeyValuePair<int, int>(11, 3), 10000},

            {new KeyValuePair<int, int>(12, 0), 4000},
            {new KeyValuePair<int, int>(12, 1), 5000},
            {new KeyValuePair<int, int>(12, 2), 6000},

            {new KeyValuePair<int, int>(13, 0), 3000},
            {new KeyValuePair<int, int>(13, 1), 5000},
            {new KeyValuePair<int, int>(13, 2), 7000},

            {new KeyValuePair<int, int>(15, 0), 3000},
            {new KeyValuePair<int, int>(15, 1), 5000},
            {new KeyValuePair<int, int>(15, 2), 7000},
            {new KeyValuePair<int, int>(15, 3), 9000},
            {new KeyValuePair<int, int>(15, 4), 11000},

            {new KeyValuePair<int, int>(22, 0), 100},
            {new KeyValuePair<int, int>(22, 1), 300},
        };

        private static readonly Dictionary<int, int> _vipMods = new Dictionary<int, int>
        {
            {14, -1},
            {48, -1},
        };

        int GetModPrice(int type, int mod)
        {
            foreach (var itm in _modPrices)
            {
                if (itm.Key.Key == type && itm.Key.Value == mod)
                    return itm.Value;
            }

            foreach (var itm in _modTypePrices)
            {
                if (itm.Key == type)
                    return itm.Value;
            }

            return -1;
        }

        public static Dictionary<int, string> ModTypes = new Dictionary<int, string>
        { 
            {0, "Spoilers"},
            {1, "Front Bumper"},
            {2, "Rear Bumper"},
            {3, "Side Skirt"},
            {4, "Exhaust"},
            {5, "Frame"},
            {6, "Grille"},
            {7, "Hood"},
            {8, "Fender"},
            {9, "Right Fender"},
            {10, "Roof"},
            {11, "Engine"},
            {12, "Brakes"},
            {13, "Transmission"},
            {14, "Horns"},
            {15, "Suspension"},
            {16, "Armor"},
            {18, "Turbo"},
            {22, "Headlights"},
            {23, "Wheels"},
            {24, "Back Wheels"},
            {25, "Plate holders"},
            {27, "Trim Design"},
            {28, "Ornaments"},
            {30, "Dial Design"},
            {33, "Steering Wheel"},
            {34, "Shift Lever"},
            {35, "Plaques"},
            {38, "Hydraulics"},
            {48, "Livery"},
            {62, "Plate"},
            {66, "Colour 1"},
            {67, "Colour 2"},
            {69, "Window Tint"},
        };

        //These values are the same in JS
        //DO NOT CHANGE
        public const int PrimaryColorId = 100;
        public const int SecondryColorId = 101;
        public const int TyresSmokeColorId = 102;
        public const int NeonColorId = 103;
        public const int WindowTintId = 104;

        public static void ClearVehicleMods(Vehicle veh)
        {
            foreach (var type in ModTypes.Keys)
            {
                API.shared.removeVehicleMod(veh.NetHandle, type);
            }
            API.shared.setVehicleCustomPrimaryColor(veh.NetHandle, 0, 0, 0);
            API.shared.setVehicleCustomSecondaryColor(veh.NetHandle, 0, 0, 0);
            API.shared.setVehicleTyreSmokeColor(veh.NetHandle, 0, 0, 0);
            API.shared.setVehicleNeonColor(veh.NetHandle, 0, 0, 0);
            API.shared.setVehicleWindowTint(veh.NetHandle, 0);
        }

        public static void ApplyVehicleMods(Vehicle veh)
        {
            foreach (var mod in veh.VehMods ?? new Dictionary<string, string>())
            {
                var modid = Convert.ToInt32(mod.Key);
                if (modid == PrimaryColorId)
                {
                    var clrs = ((string) mod.Value).Split('|');
                    if(clrs.Length == 1)
                        GrandTheftMultiplayer.Server.API.API.shared.setVehiclePrimaryColor(veh.NetHandle, Convert.ToInt32(clrs[0]));
                    else
                        API.shared.setVehicleCustomPrimaryColor(veh.NetHandle, Convert.ToInt32(clrs[0]), Convert.ToInt32(clrs[1]), Convert.ToInt32(clrs[2]));
                }
                else if (modid == SecondryColorId)
                {
                    var clrs = ((string)mod.Value).Split('|');
                    if (clrs.Length == 1)
                        GrandTheftMultiplayer.Server.API.API.shared.setVehicleSecondaryColor(veh.NetHandle, Convert.ToInt32(clrs[0]));
                    else
                        API.shared.setVehicleCustomSecondaryColor(veh.NetHandle, Convert.ToInt32(clrs[0]), Convert.ToInt32(clrs[1]), Convert.ToInt32(clrs[2]));
                }
                else if (modid == TyresSmokeColorId)
                {
                    var clrs = ((string) mod.Value).Split('|');
                    API.shared.setVehicleTyreSmokeColor(veh.NetHandle, Convert.ToInt32(clrs[0]),
                        Convert.ToInt32(clrs[1]), Convert.ToInt32(clrs[2]));

                    foreach (var p in API.shared.getAllPlayers())
                    {
                        if (p == null)
                            continue;

                        if (GrandTheftMultiplayer.Server.API.API.shared.getEntityPosition(veh.NetHandle)
                                .DistanceTo(p.position) <= 500)
                        {
                            if (Convert.ToInt32(clrs[1]) == 0 && Convert.ToInt32(clrs[1]) == 0 &&
                                Convert.ToInt32(clrs[1]) == 0)
                            {
                                GrandTheftMultiplayer.Server.API.API.shared.sendNativeToPlayer(p,
                                    Hash.TOGGLE_VEHICLE_MOD, veh.NetHandle, 20, false);
                            }
                            else
                            {
                                GrandTheftMultiplayer.Server.API.API.shared.sendNativeToPlayer(p,
                                    Hash.TOGGLE_VEHICLE_MOD, veh.NetHandle, 20, true);
                            }
                        }
                    }
                }
                else if (modid == NeonColorId)
                {
                    var clrs = ((string) mod.Value).Split('|');
                    API.shared.setVehicleNeonColor(veh.NetHandle, Convert.ToInt32(clrs[0]), Convert.ToInt32(clrs[1]),
                        Convert.ToInt32(clrs[2]));
                }
                else if (modid == WindowTintId)
                {
                    API.shared.setVehicleWindowTint(veh.NetHandle, Convert.ToInt32(mod.Value));
                }
                else
                {
                    API.shared.setVehicleMod(veh.NetHandle, modid, Convert.ToInt32(mod.Value));

                    if (modid == 14) //Horns
                    {
                        foreach (var p in API.shared.getAllPlayers())
                        {
                            if (p == null)
                                continue;

                            if (GrandTheftMultiplayer.Server.API.API.shared.getEntityPosition(veh.NetHandle)
                                    .DistanceTo(p.position) <= 500)
                            {
                                GrandTheftMultiplayer.Server.API.API.shared.sendNativeToPlayer(p,
                                    Hash.SET_VEHICLE_MOD, veh.NetHandle, 14, Convert.ToInt32(mod.Value));
                            }
                        }
                    }
                }
            }
        }

        [Command("modvehicle")]
        public void ModVehicle(Client player)
        {
            var prop = PropertyManager.IsAtPropertyEntrance(player);
            if (prop?.Type != PropertyManager.PropertyTypes.ModdingShop)
            {
                API.sendChatMessageToPlayer(player, "You must be at a modding shop to modify your vehicle.");
                return;
            }

            if (!player.isInVehicle)
            {
                API.sendChatMessageToPlayer(player, "You must be in a vehicle to modify it.");
                return;
            }

            if (!player.GetCharacter().OwnedVehicles.Contains(player.vehicle.handle.GetVehicle()) && player.GetAccount().AdminLevel == 0)
            {
                API.sendChatMessageToPlayer(player, "You must own the vehicle you're modifying");
                return;
            }

            //Boats, Helis, Planes, Trains
            if (player.vehicle.Class == 14 || player.vehicle.Class == 15 || player.vehicle.Class == 16 || player.vehicle.Class == 21)
            {
                API.sendChatMessageToPlayer(player, "You cannot modify this vehicle.");
                return;
            }

            List<string[]> modList = new List<string[]>();
            var manifest = VehicleInfo.Get(player.vehicle);

            foreach (var i in manifest.ModTypes)
            {
                var price = GetModPrice(i, 0);
                if (price == -1)
                    continue;

                if (ModTypes.ContainsKey(i))
                    modList.Add(new String[]
                    {
                        ModTypes[i],
                        _vipMods.ContainsKey(i) ? "true" : "false"
                    });
            }

            API.setEntityData(player, "ModLastPos", player.position.Copy());
            API.setEntityPosition(player.vehicle, new Vector3(-335.8468, -138.2994, 38.43893));
            API.setEntityRotation(player.vehicle, new Vector3(0.1579523, 0.0001232202, -84.06439));
            API.setEntityDimension(player, player.GetCharacter().Id);
            API.setEntityDimension(player.vehicle, player.GetCharacter().Id);
            
            API.triggerClientEvent(player, "SHOW_MODDING_GUI", API.toJson(modList.ToArray()), player.GetAccount().VipLevel > 0);
            if(player.GetAccount().IsSpeedoOn)
                API.triggerClientEvent(player, "TOGGLE_SPEEDO");
        }


        [Command("toggleneon"), Help(HelpManager.CommandGroups.Vehicles, "Toggles the neon of your vehicle on or off.")]
        public void ToggleNeon(Client player, int slot = -1)
        {

            if (!player.isInVehicle)
            {
                API.sendChatMessageToPlayer(player, "You aren't in a a vehicle.");
                return;
            }

            if (player.GetAccount().VipLevel == 0)
            {
                API.sendChatMessageToPlayer(player, "You must be VIP to use neons.");
                return;
            }

            if (slot > 3 || slot < 0 || slot == -1)
            {
                API.sendChatMessageToPlayer(player, "USAGE: /toggleneon [0-3]");
                API.sendChatMessageToPlayer(player, "* 0 -> Left Neon");
                API.sendChatMessageToPlayer(player, "* 1 -> Right Neon");
                API.sendChatMessageToPlayer(player, "* 2 -> Front Neon");
                API.sendChatMessageToPlayer(player, "* 3 -> Back Neon");
                return;
            }

            var newState = !API.getVehicleNeonState(player.vehicle, slot);
            API.setVehicleNeonState(player.vehicle, slot, newState);
            ChatManager.RoleplayMessage(player, newState ? "turns on the neon of his vehicle." : "turns off the neon of his vehicle.", ChatManager.RoleplayMe);
        }
    }
}
