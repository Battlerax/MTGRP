using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace mtgvrp.core
{
    public static class SettingsManager
    {
        public static SettingsClass Settings = new SettingsClass();

        public static void Load()
        {
            if (!File.Exists("Configuration.json"))
                File.WriteAllText("Configuration.json", "");

            Settings = JsonConvert.DeserializeObject<SettingsClass>(File.ReadAllText("Configuration.json")) ?? new SettingsClass();
        }

        public static void Save()
        {
            string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            File.WriteAllText(@"Configuration.json", json);
        }

        public class SettingsClass
        {
           public int WoodSupplies { get; set; } 
           public int TruckerSupplies { get; set; } 
        }
        
    }
}
