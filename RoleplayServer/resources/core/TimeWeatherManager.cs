using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;

namespace RoleplayServer.resources.core
{
    public class TimeWeatherManager : Script
    {

        public TimeWeatherManager()
        {
            API.onResourceStart += API_onResourceStart;
        }

        private void API_onResourceStart()
        {
            API.consoleOutput("Loading Weather Module.");

            API.consoleOutput("Weather Updated To LA.");
        }

        //Los Angeles Time.
        public static DateTime CurrentTime => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"));
    }
}
