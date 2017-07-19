using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mtgvrp.mapping_manager
{
    public class MappingManager
    {
        public List<MappingObject> ParseObjectsFromString(string input)
        {
            List<MappingObject> objectList = new List<MappingObject>();

            var objectPattern = @"API.createObject\s*\((?<model>-?[0-9]+)\s*,\s*new\s*Vector3\s*\(\s*(?<posX>-?[0-9.]*)\s*,\s*(?<posY>-?[0-9.]*)\s*,\s*(?<posZ>-?[0-9.]*)\)\s*,\s*new\s*Vector3\s*\(\s*(?<rotX>-?[0-9.]*)\s*,\s*(?<rotY>-?[0-9.]*)\s*,\s*(?<rotZ>-?[0-9.]*)\s*\)\s*\)";
            var regex = new Regex(objectPattern);
            foreach(Match match in regex.Matches(input))
            {
                objectList.Add(new MappingObject(MappingObject.ObjectType.CreateObject, Convert.ToInt32(match.Groups["model"].ToString()), new Vector3((float)Convert.ToDouble(match.Groups["posX"].ToString()), (float)Convert.ToDouble(match.Groups["posY"].ToString()), (float)Convert.ToDouble(match.Groups["posZ"].ToString())), new Vector3((float)Convert.ToDouble(match.Groups["rotX"].ToString()), (float)Convert.ToDouble(match.Groups["rotY"].ToString()), (float)Convert.ToDouble(match.Groups["rotZ"].ToString()))));
            }

            return objectList;
        }

        public List<MappingObject> ParseDeleteObjectsFromString(string input) 
        {
            List<MappingObject> objectList = new List<MappingObject>();

            var objectPattern = @"API.deleteObject\s*\(player,\s*new\s*Vector3\s*\(\s*(?<posX>-?[0-9.]*)\s*,\s*(?<posY>-?[0-9.]*)\s*,\s*(?<posZ>-?[0-9.]*)\)\s*,\s*(?<model>-?[0-9]+)\s*\);";
            var regex = new Regex(objectPattern);
            foreach (Match match in regex.Matches(input))
            {
                objectList.Add(new MappingObject(MappingObject.ObjectType.CreateObject, Convert.ToInt32(match.Groups["model"].ToString()), new Vector3((float)Convert.ToDouble(match.Groups["posX"].ToString()), (float)Convert.ToDouble(match.Groups["posY"].ToString()), (float)Convert.ToDouble(match.Groups["posZ"].ToString())), new Vector3((float)Convert.ToDouble(match.Groups["rotX"].ToString()), (float)Convert.ToDouble(match.Groups["rotY"].ToString()), (float)Convert.ToDouble(match.Groups["rotZ"].ToString()))));
            }

            return objectList;
        }
    }
}
