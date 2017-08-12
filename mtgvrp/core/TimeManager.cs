using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mtgvrp.core
{
    public static class TimeManager
    {
        public static int GetTimeStamp => (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static double GetTimeStampPlus(TimeSpan span)
        {
            return GetTimeStamp + span.TotalSeconds;
        }

        public static double SecondsToMinutes(double x) => Math.Round(x / 60);
        public static double MinutesToHours(double x) => Math.Round(x / 60);
        public static double SecondsToHours(double x) => Math.Round(x / 60 / 60);
    }
}
