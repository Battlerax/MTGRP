using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;



using GTANetworkAPI;


using VehicleInfoLoader;


using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.property_system;
using Newtonsoft.Json;

namespace mtgvrp.vehicle_manager.modding
{
    public class ModdingManager : Script
    {
        [RemoteEvent("MODDING_GETMODS")]
        public void ModdingGetMods(Player sender, params object[] arguments)
        {
            var modsList = new List<string[]>();

            var modType = ModTypes.First(x => x.Value == (string)arguments[0]).Key;
            var manifest = VehicleInfo.Get(sender.Vehicle);
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
            NAPI.ClientEvent.TriggerClientEvent(sender, "MODDING_FILL_MODS", NAPI.Util.ToJson(modsList.ToArray()));
        }

        [RemoteEvent("MODDING_EXITMENU")]
        public void ModdingExitMenu(Player sender, params object[] arguments)
        {
            
            var Vehicle = sender.Vehicle;
            ClearVehicleMods(Vehicle.GetVehicle());
            ApplyVehicleMods(Vehicle.GetVehicle());
            NAPI.Entity.SetEntityPosition(sender.Vehicle, sender.GetData<Vector3>("ModLastPos"));
            NAPI.Entity.SetEntityDimension(sender.Vehicle, 0);
            NAPI.Entity.SetEntityDimension(sender, 0);
            if (sender.GetAccount().IsSpeedoOn)
                NAPI.ClientEvent.TriggerClientEvent(sender, "TOGGLE_SPEEDO");
        }

        [RemoteEvent("MODDONG_PURCHASE_ITEMS")]
        public void ModdingPurchaseItems(Player sender, params object[] arguments)
        {
            dynamic items = JsonConvert.DeserializeObject((string)arguments[0]);
            int allPrices = 0;
            int itemCount = 0;
            foreach (var itm in items)
            {
                var modType = int.Parse(itm[0].ToString().Trim());
                int modId;
                int.TryParse(itm[1].ToString().Trim(), out modId);

                var price = GetModPrice(modType, modId);
                allPrices += price;
                itemCount++;
            }
            var prop = PropertyManager.Properties.First(x => x.Id == NAPI.Data.GetEntityData(sender, "MOD_ID"));
            if (prop.Supplies != -1 && prop.Supplies < itemCount * 5)
            {
                NAPI.ClientEvent.TriggerClientEvent(sender, "MODDING_ERROR",
                    "This modshop doesn't have enough supplies.");
                return;
            }

            var c = sender.GetCharacter();
            if (Money.GetCharacterMoney(c) < allPrices)
            {
                NAPI.ClientEvent.TriggerClientEvent(sender, "MODDING_ERROR",
                    "You don't have enough money to purchase these mods. Your balance: " +
                    Money.GetCharacterMoney(c));
                return;
            }

            InventoryManager.DeleteInventoryItem<Money>(c, allPrices);
            InventoryManager.GiveInventoryItem(prop, new Money(), allPrices);

            if (prop.Supplies != -1)
                prop.Supplies -= itemCount * 5;

            var veh = sender.Vehicle.GetVehicle();
            foreach (var itm in items)
            {
                var modType = int.Parse(itm[0].ToString().Trim());
                AddVehicleMod(veh, modType, itm[1].ToString().Trim());
            }
            ClearVehicleMods(veh);
            ApplyVehicleMods(veh);
            veh.Save();
            NAPI.ClientEvent.TriggerClientEvent(sender, "MODDING_CLOSE");
            NAPI.Chat.SendChatMessageToPlayer(sender,
                "You have successfully purchased some Vehicle mods for a total of ~g~" +
                allPrices.ToString("C"));
            NAPI.Entity.SetEntityPosition(sender.Vehicle, sender.GetData<Vector3>("ModLastPos"));
            NAPI.Entity.SetEntityDimension(sender.Vehicle, 0);
            NAPI.Entity.SetEntityDimension(sender, 0);
            if (sender.GetAccount().IsSpeedoOn)
                NAPI.ClientEvent.TriggerClientEvent(sender, "TOGGLE_SPEEDO");
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            //VehicleInfo.Setup(Path.Combine(API.GetResourceFolder(), @"vehicle_manager/modding/modinfo"));
        }

        void AddVehicleMod(GameVehicle veh, int type, string mod)
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

        public static void ClearVehicleMods(GameVehicle veh)
        {
            foreach (var type in ModTypes.Keys)
            {
                API.Shared.RemoveVehicleMod(veh.Entity, type);
            }
            API.Shared.SetVehicleCustomPrimaryColor(veh.Entity, 0, 0, 0);
            API.Shared.SetVehicleCustomSecondaryColor(veh.Entity, 0, 0, 0);
            API.Shared.SetVehicleTyreSmokeColor(veh.Entity, new GTANetworkAPI.Color(0, 0, 0));
            API.Shared.SetVehicleNeonColor(veh.Entity, 0, 0, 0);
            API.Shared.SetVehicleWindowTint(veh.Entity, 0);
        }

        public static void ApplyVehicleMods(GameVehicle veh)
        {
            foreach (var mod in veh.VehMods ?? new Dictionary<string, string>())
            {
                var modid = Convert.ToInt32(mod.Key);
                if (modid == PrimaryColorId)
                {
                    var clrs = ((string) mod.Value).Split('|');
                    if(clrs.Length == 1)
                        API.Shared.SetVehiclePrimaryColor(veh.Entity, Convert.ToInt32(clrs[0]));
                    else
                        API.Shared.SetVehicleCustomPrimaryColor(veh.Entity, Convert.ToInt32(clrs[0]), Convert.ToInt32(clrs[1]), Convert.ToInt32(clrs[2]));
                }
                else if (modid == SecondryColorId)
                {
                    var clrs = ((string)mod.Value).Split('|');
                    if (clrs.Length == 1)
                        API.Shared.SetVehicleSecondaryColor(veh.Entity, Convert.ToInt32(clrs[0]));
                    else
                        API.Shared.SetVehicleCustomSecondaryColor(veh.Entity, Convert.ToInt32(clrs[0]), Convert.ToInt32(clrs[1]), Convert.ToInt32(clrs[2]));
                }
                else if (modid == TyresSmokeColorId)
                {
                    var clrs = ((string) mod.Value).Split('|');
                    API.Shared.SetVehicleTyreSmokeColor(veh.Entity, new GTANetworkAPI.Color(Convert.ToInt32(clrs[0]),
                        Convert.ToInt32(clrs[1]), Convert.ToInt32(clrs[2])));

                    foreach (var p in API.Shared.GetAllPlayers())
                    {
                        if (p == null)
                            continue;

                        if (API.Shared.GetEntityPosition(veh.Entity)
                                .DistanceTo(p.Position) <= 500)
                        {
                            if (Convert.ToInt32(clrs[1]) == 0 && Convert.ToInt32(clrs[1]) == 0 &&
                                Convert.ToInt32(clrs[1]) == 0)
                            {
                                API.Shared.SendNativeToPlayer(p,
                                    Hash.TOGGLE_VEHICLE_MOD, veh.Entity, 20, false);
                            }
                            else
                            {
                                API.Shared.SendNativeToPlayer(p,
                                    Hash.TOGGLE_VEHICLE_MOD, veh.Entity, 20, true);
                            }
                        }
                    }
                }
                else if (modid == NeonColorId)
                {
                    var clrs = ((string) mod.Value).Split('|');
                    API.Shared.SetVehicleNeonColor(veh.Entity, Convert.ToInt32(clrs[0]), Convert.ToInt32(clrs[1]),
                        Convert.ToInt32(clrs[2]));
                }
                else if (modid == WindowTintId)
                {
                    API.Shared.SetVehicleWindowTint(veh.Entity, Convert.ToInt32(mod.Value));
                }
                else
                {
                    API.Shared.SetVehicleMod(veh.Entity, modid, Convert.ToInt32(mod.Value));

                    if (modid == 14) //Horns
                    {
                        foreach (var p in API.Shared.GetAllPlayers())
                        {
                            if (p == null)
                                continue;

                            if (API.Shared.GetEntityPosition(veh.Entity)
                                    .DistanceTo(p.Position) <= 500)
                            {
                                API.Shared.SendNativeToPlayer(p,
                                    Hash.SET_VEHICLE_MOD, veh.Entity, 14, Convert.ToInt32(mod.Value));
                            }
                        }
                    }
                }
            }
        }

        [Command("modvehicle")]
        public void ModVehicle(Player player)
        {
            var prop = PropertyManager.IsAtPropertyEntrance(player);
            if (prop?.Type != PropertyManager.PropertyTypes.ModdingShop || prop.OwnerId == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must be at an owned modding shop to modify your Vehicle.");
                return;
            }

            if (!player.IsInVehicle)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must be in a Vehicle to modify it.");
                return;
            }

            if (!player.GetCharacter().OwnedVehicles.Contains(player.Vehicle.GetVehicle()) && player.GetAccount().AdminLevel == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must own the Vehicle you're modifying");
                return;
            }

            //Boats, Cycles, Helis, Planes, Trains
            if (player.Vehicle.Class == 14 || player.Vehicle.Class == 13 || player.Vehicle.Class == 15 || player.Vehicle.Class == 16 || player.Vehicle.Class == 21)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You cannot modify this Vehicle.");
                return;
            }

            List<string[]> modList = new List<string[]>();
            var manifest = VehicleInfo.Get(player.Vehicle);

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

            NAPI.Data.SetEntityData(player, "ModLastPos", player.Position.Copy());
            NAPI.Data.SetEntityData(player, "MOD_ID", prop.Id);
            NAPI.Entity.SetEntityPosition(player.Vehicle, new Vector3(-335.8468, -138.2994, 38.43893));
            NAPI.Entity.SetEntityRotation(player.Vehicle, new Vector3(0.1579523, 0.0001232202, -84.06439));
            NAPI.Entity.SetEntityDimension(player, (uint)player.GetCharacter().Id);
            NAPI.Entity.SetEntityDimension(player.Vehicle, (uint)player.GetCharacter().Id);
            
            NAPI.ClientEvent.TriggerClientEvent(player, "SHOW_MODDING_GUI", NAPI.Util.ToJson(modList.ToArray()), player.GetAccount().VipLevel > 0);
            if(player.GetAccount().IsSpeedoOn)
                NAPI.ClientEvent.TriggerClientEvent(player, "TOGGLE_SPEEDO");
        }


        [Command("toggleneon"), Help(HelpManager.CommandGroups.Vehicles, "Toggles the neon of your Vehicle on or off.")]
        public void ToggleNeon(Player player, int slot = -1)
        {

            if (!player.IsInVehicle)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't in a a Vehicle.");
                return;
            }

            if (player.GetAccount().VipLevel == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must be VIP to use neons.");
                return;
            }

            if (slot > 3 || slot < 0 || slot == -1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "USAGE: /toggleneon [0-3]");
                NAPI.Chat.SendChatMessageToPlayer(player, "* 0 -> Left Neon");
                NAPI.Chat.SendChatMessageToPlayer(player, "* 1 -> Right Neon");
                NAPI.Chat.SendChatMessageToPlayer(player, "* 2 -> Front Neon");
                NAPI.Chat.SendChatMessageToPlayer(player, "* 3 -> Back Neon");
                return;
            }

            // TODO: this use to take a slot?
            var newState = !NAPI.Vehicle.GetVehicleNeonState(player.Vehicle);
            NAPI.Vehicle.SetVehicleNeonState(player.Vehicle, newState);
            ChatManager.RoleplayMessage(player, newState ? "turns on the neon of his Vehicle." : "turns off the neon of his Vehicle.", ChatManager.RoleplayMe);
        }
    }
}
