using System;
using System.IO;
using System.Threading;
using System.Timers;
using GTANetworkAPI;
using Timer = System.Timers.Timer;

namespace mtgvrp.core
{
    public static class LogManager
    {
        private static Timer archiveTimer = null;

        public static void StartLogArchiveTimer()
        {
            ArchiveLogs();

            archiveTimer = new Timer {Interval = 3.6e+6};
            archiveTimer.Elapsed += ArchiveTimer_Elapsed;
            archiveTimer.Start();
        }

        private static void ArchiveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ArchiveLogs();
        }

        static void ArchiveLogs()
        {
            try
            {
                DebugManager.DebugMessage("** Starting log archive process.", 1);

                var path = "Logs/OldLogs/" + DateTime.Now.Year + "-" + DateTime.Now.Month;
                //Make sure folders exist.
                Directory.CreateDirectory(path);

                //Move the files.
                foreach (var file in Directory.GetFiles("Logs", "*.log"))
                {
                    File.Move(file, path + "/" + Path.GetFileNameWithoutExtension(file) + "-" + TimeManager.GetTimeStamp + Path.GetExtension(file));
                }
                DebugManager.DebugMessage("** Logs has been archived.");
            }
            catch (Exception e)
            {
                DebugManager.DebugMessage(e.Message);
            }
        }

        public enum LogTypes
        {
            AdminActions,   //Logged
            Bans,           //Logged
            Commands,       //Logged
            Connection,     //Logged
            Death,          //Logged
            GroupChat,      //Logged
            RadioChat,      //Logged
            GroupInvites,   //Logged
            ICchat,         //Logged
            Phone,          //Logged
            OOCchat,        //Logged
            PMchat,         //Logged
            Stats,          //Logged
            Storage,        //Logged
            Unbans,         //Logged
            Warns,          //Logged
            MappingRequests,
            Ads,            //Logged
        }

        public static void Log(LogTypes type, string log)
        {
            //prepare file name.
            var file = type.ToString() + ".log";

            //if dir not there, make it.
            Directory.CreateDirectory("Logs");

            //Append
            ThreadPool.QueueUserWorkItem(WriteToFile, new [] { "Logs/" + file, $"[{DateTime.Now:R}] " + log });
        }

        // the lock
        private static object writeLock = new object();
        public static void WriteToFile(object msg)
        {
            lock (writeLock)
            {
                var pars = (string[]) msg;

                using (var writer = File.AppendText(pars[0]))
                {
                    writer.WriteLine(pars[1]);
                }
            }
        }

        public static string GetLogName(Player player) => $"{player.GetCharacter().CharacterName}[{player.GetAccount().AccountName}]";
    }
}
