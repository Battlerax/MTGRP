using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;

namespace mtgvrp.core.Help
{
    public class HelpManager : Script
    {
        public enum CommandGroups
        {
            General,
            Roleplay,
            Animation,
            Inventory,
            Vehicles,
            Property,
            GroupGeneral,
            LSNN,
            LSPD,
            AdminLevel1,
            AdminLevel2,
            AdminLevel3,
            AdminLevel4,
            AdminLevel5,
            AdminLevel6,
            AdminLevel7,
            AdminLevel8,
        }

        public Dictionary<CommandGroups, string> CommandStuff = new Dictionary<CommandGroups, string>();

        public HelpManager()
        {
            var methods = Assembly.GetExecutingAssembly().GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(Help), false).Length > 0)
                .ToArray();

            API.consoleOutput($"*** Intializing Help. [ {methods.Length} Commands ]");

            Dictionary<CommandGroups, List<string>> cmds = new Dictionary<CommandGroups, List<string>>();
            foreach (var cmd in methods)
            {
                Help commandHelp = (Help)cmd.GetCustomAttributes(typeof(Help), false)[0];
                CommandAttribute commandInfo = (CommandAttribute)cmd.GetCustomAttributes(typeof(CommandAttribute), false)[0];

                if(!cmds.ContainsKey(commandHelp.Group))
                    cmds[commandHelp.Group] = new List<string>();

                cmds[commandHelp.Group].Add(API.toJson(new[] { commandInfo.CommandString, API.toJson(cmd.GetParameters().Select(x => x.Name).ToArray()), commandHelp.Description, API.toJson(commandHelp.Parameters) }));
            }

            foreach (var group in cmds.Keys)
            {
                string[] cmdsArray = cmds[group].ToArray();
                CommandStuff.Add(group, API.toJson(cmdsArray));
            }

            API.consoleOutput($"*** Help Done");
        }
    }


}
