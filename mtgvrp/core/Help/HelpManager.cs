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
            General,
            Roleplay,
            Animation,
            Inventory,
            Vehicles,
            Bussiness,
            Houses,
            ScubaActivity,
            HuntingActivity,
            JobsGeneral,
            TaxiJob,
            MechanicJob,
            FisherJob,
            DeliveryJob,
            GarbageJob,
            TruckerJob,
            GroupGeneral,
            LSNN,
            LSPD,
            Gov,
            AdminLevel1,
            AdminLevel2,
            AdminLevel3,
            AdminLevel4,
            AdminLevel5,
            AdminLevel6,
            AdminLevel7,
            AdminLevel8,
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

            API.consoleOutput($"*** Intializing Help. [ {methods.Length + animCmds.Length} Commands ]");

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

        [Command("help")]
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

            API.triggerClientEvent(player, "help_showMenu", CommandStuff, player.GetAccount().AdminLevel, isPD, isLSNN, isGov);
        }
    }
}
