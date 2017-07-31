using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using mtgvrp.group_manager;

namespace mtgvrp.core.Help
{
    public class HelpManager : Script
    {
        [Flags]
        public enum CommandGroups
        {
            General = 1,
            Chat = 2,
            Roleplay = 4,
            Animation = 8,
            Inventory = 16,
            Vehicles = 32,
            Bussiness = 64,
            Houses = 128,
            ScubaActivity = 256,
            HuntingActivity = 512,
            JobsGeneral = 1024,
            TaxiJob = 2048,
            MechanicJob = 4096,
            LumberJob = 8192,
            FisherJob = 16384,
            DeliveryJob = 32768,
            GarbageJob = 65536,
            TruckerJob = 131072,
            GroupGeneral = 262144,
            LSNN = 524288,
            LSPD = 1048576,
            Gov = 2097152,
            AdminLevel1 = 4194304,
            AdminLevel2 = 8388608,
            AdminLevel3 = 16777216,
            AdminLevel4 = 33554432,
            AdminLevel5 = 67108864,
            AdminLevel6 = 134217728,
            AdminLevel7 = 268435456,
            AdminLevel8 = 536870912,
            PropertyGeneral = 1073741824
        }

        public string CommandStuff;

        public HelpManager()
        {
            var methods = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(Help), false).Length > 0)
                .ToArray();

            //Animation Commands.
            var animCmds = typeof(Animations).GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0).ToArray();

            var totalCommands = Assembly.GetExecutingAssembly()
                .GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0).ToArray();

            API.consoleOutput($"*** Intializing Help. [ {methods.Length + animCmds.Length} Commands Of {totalCommands.Length} ]");

            //Show commands with no help.
            foreach (var missing in totalCommands.Except(animCmds).Except(methods))
            {
                var info = missing.GetCustomAttribute<CommandAttribute>();
                API.consoleOutput($"*** [ERROR] COMMAND `/{info.CommandString}` HAS NO HELP.");
            }

            Dictionary<CommandGroups, List<string[]>> cmds = new Dictionary<CommandGroups, List<string[]>>();
            foreach (var cmd in methods)
            {
                Help commandHelp = (Help)cmd.GetCustomAttributes(typeof(Help), false)[0];
                CommandAttribute commandInfo = cmd.GetCustomAttribute<CommandAttribute>();

                if (cmd.GetParameters().Skip(1).Select(x => x.Name).Count() != commandHelp.Parameters.Length)
                {
                    API.consoleOutput($"*** [ERROR] COMMAND `/{commandInfo.CommandString}` HAS AMOUNT OF PARAMETER DESCRIPTIONS NOT EQUAL TO ACTUAL PARAMETERS.");
                    continue;
                }

                foreach (var grp in commandHelp.Group.GetIndividualFlags().Cast<CommandGroups>())
                {
                    if (!cmds.ContainsKey(grp))
                        cmds[grp] = new List<string[]>();

                    cmds[grp].Add(new[] { commandInfo.CommandString, string.Join("|", cmd.GetParameters().Skip(1).Select(x => x.Name)) /* Skip sender */ , commandHelp.Description, string.Join("|", commandHelp.Parameters) });
                }
            }

            //Animation Commands: 
            if (!cmds.ContainsKey(CommandGroups.Animation))
                cmds[CommandGroups.Animation] = new List<string[]>();
            foreach (var cmd in animCmds)
            {
                var commandInfo = cmd.GetCustomAttribute<CommandAttribute>();
                cmds[CommandGroups.Animation].Add(new[] { commandInfo.CommandString, string.Join("|", cmd.GetParameters().Skip(1).Select(x => x.Name)) /* Skip sender */ , "", ""});
            }

            CommandStuff = API.toJson(cmds);

            API.consoleOutput($"*** Help Done");
        }

        [Command("help"), Help(CommandGroups.General, "Shows the list of commands available.")]
        public void Help_cmd(Client player)
        {
            if (!player.GetAccount().IsLoggedIn)
                return;

            var character = player.GetCharacter();

            bool isPD = false;
            bool isLSNN = false;
            bool isGov = false;
            if (character.Group != Group.None)
            {
                if (character.Group.CommandType == Group.CommandTypeLspd)
                    isPD = true;

                if (character.Group.CommandType == Group.CommandTypeLsnn)
                    isLSNN = true;

                if (character.Group.CommandType == Group.CommandTypeLSGov)
                    isGov = true;
            }

            Init.SendEvent(player, "help_showMenu", CommandStuff, player.GetAccount().AdminLevel, isPD, isLSNN, isGov);
        }
    }
}
