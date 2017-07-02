using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using IniParser;
using IniParser.Model;

namespace mtgvrp.core
{
    public static class SettingsManager
    {

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static IniData _data;
        private static FileIniDataParser _parser = new FileIniDataParser();

        public static void Load()
        {
            if (!File.Exists(AssemblyDirectory + "/Configuration.ini"))
                File.WriteAllText(AssemblyDirectory + "/Configuration.ini", "");

            _data = _parser.ReadFile(AssemblyDirectory + "/Configuration.ini");

            API.shared.consoleOutput("Configuration loaded successfully.");
        }

        public static void Save()
        {
            _parser.WriteFile(AssemblyDirectory + "/Configuration.ini", _data);
            API.shared.consoleOutput("Configuration saved successfully.");
        }

        public static void SetSetting(string var, string value)
        {
            _data["general"][var] = value;
        }

        public static string GetSetting(string var)
        {
            return _data["general"][var];
        }
    }
}
