using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using VehicleInfoLoader;
using VehicleInfoLoader.Data;
using GrandTheftMultiplayer.Server.Managers;

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
                        var m = manifest.Mod(modType, modid);
                        modsList.Add(new string[] {m.localizedName, modType.ToString(), modid.ToString(), GetModPrice(modType, modid).ToString("C")});
                    }
                    API.triggerClientEvent(sender, "MODDING_FILL_MODS", API.toJson(modsList.ToArray()));
                    break;
                }
            }
        }

        private void API_onResourceStart()
        {
            VehicleInfo.Setup(Path.Combine(API.getResourceFolder(), @"vehicle_manager\modding\modinfo\"));
        }

        private static readonly Dictionary<int, int> _modTypePrices = new Dictionary<int, int>
        {
            {0, 1000},
            {1, 1500},
            {2, 1500},
            {3, 750},
            {4, 800},
            {5, 0},
            {6, 900},
            {7, 1000},
            {8, 0},
            {9, 0},
            {10, 1200},
            {11, 0},
            {12, 0},
            {13, 0},
            {14, 500}, //VIP
            {15, 1000},
            {16, -1},
            {18, 0},
            {22, 0},
            {23, 600},
            {24, 600},
            {25, 0},
            {27, 0},
            {28, 0},
            {30, 0},
            {33, 0},
            {34, 0},
            {35, 0},
            {38, 2000},
            {48, 0},
            {62, 0},
            {66, 0},
            {67, 0},
            {69, 0},
        };

        private static readonly Dictionary<KeyValuePair<int, int>, int> _modPrices = new Dictionary<KeyValuePair<int, int>, int>
        {
            {new KeyValuePair<int, int>(11, 1), 4000},
            {new KeyValuePair<int, int>(11, 2), 6000},
            {new KeyValuePair<int, int>(11, 3), 8000},
            {new KeyValuePair<int, int>(11, 4), 10000},

            {new KeyValuePair<int, int>(12, 1), 4000},
            {new KeyValuePair<int, int>(12, 2), 5000},
            {new KeyValuePair<int, int>(12, 3), 6000},

            {new KeyValuePair<int, int>(13, 1), 3000},
            {new KeyValuePair<int, int>(13, 2), 5000},
            {new KeyValuePair<int, int>(13, 3), 7000},
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

        public Dictionary<int, string> ModTypes = new Dictionary<int, string>
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
            {22, "Xenon"},
            {23, "Front Wheels"},
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

        [Command("openmod")]
        public void GetAvailableTypes(Client player)
        {
            List<string> modList = new List<string>();
            var manifest = VehicleInfo.Get(player.vehicle);
            foreach (var i in manifest.ModTypes)
            {
                modList.Add(ModTypes[i]);
            }

            API.triggerClientEvent(player, "SHOW_MODDING_GUI", API.toJson(modList.ToArray()));
        }
    }
}
