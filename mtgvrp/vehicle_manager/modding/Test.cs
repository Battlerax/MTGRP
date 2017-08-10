using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Shared;
using VehicleInfoLoader;
using VehicleInfoLoader.Data;

namespace mtgvrp.vehicle_manager.modding
{
    public class Test : Script
    {
        public Test()
        {
            API.onResourceStart += API_onResourceStart;
        }

        private void API_onResourceStart()
        {
            VehicleInfo.Setup(Path.Combine(API.getResourceFolder(), @"vehicle_manager\modding\modinfo\"));

            var manifest = VehicleInfo.Get(VehicleHash.T20);
            foreach (var i in manifest.ModTypes)
            {
                API.consoleOutput($"MOD TYPES {i}: " + manifest.ModType(i).amount);
            }
            
        }
    }
}
