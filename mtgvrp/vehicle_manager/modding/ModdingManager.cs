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
                        modsList.Add(new string[] {m.localizedName, modType.ToString(), modid.ToString()});
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
