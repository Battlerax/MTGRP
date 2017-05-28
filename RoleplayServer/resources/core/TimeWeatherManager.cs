using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using NodaTime;

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
        public static DateTime CurrentTime
        {
            get
            {
                DateTimeZone zone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
                Instant now = SystemClock.Instance.GetCurrentInstant();
                ZonedDateTime pacificNow = now.InZone(zone);
                return pacificNow.ToDateTimeUnspecified();
            }
        }
    }
}
