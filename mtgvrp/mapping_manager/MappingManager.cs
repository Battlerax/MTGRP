using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.property_system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mtgvrp.mapping_manager
{
    public class MappingManager : Script
    {
        public List<Mapping> Mapping;

        public MappingManager()
        {
            DebugManager.DebugMessage("[MAPPING MANAGER] Initializing mapping manager...");

            API.onClientEventTrigger += Mapping_onClientEventTrigger;

            Mapping = DatabaseManager.MappingTable.Find(FilterDefinition<Mapping>.Empty).ToList();

            foreach(var m in Mapping)
            {
                if (m.IsActive)
                {
                    m.Load();
                }
            }

            DebugManager.DebugMessage("[MAPPING MANAGER] Loaded " + Mapping.Count() + " mapping requests (" + Mapping.FindAll(m => m.IsActive == true).Count() + " active)");

            DebugManager.DebugMessage("[MAPPING MANAGER] Mapping initialized.");
        }

        public void Mapping_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "requestCreateMapping":
                    var propLink = Convert.ToInt32(arguments[0]);
                    var dimension = Convert.ToInt32(arguments[1]);
                    var pastebinLink = Convert.ToString(arguments[2]);
                    var description = Convert.ToString(arguments[3]);

                    if (dimension < 0)
                    {
                        API.triggerClientEvent(player, "callMappingFunction", "send_error", "The dimension entered is less than 0.");
                        return;
                    }

                    if (PropertyManager.Properties.FindAll(p => p.Id == propLink).Count < 1)
                    {
                        API.triggerClientEvent(player, "callMappingFunction", "send_error", "The property link ID you entered is invalid.");
                        return;
                    }

                    var webClient = new WebClient();

                    try
                    {
                        var pastebinData = webClient.DownloadString("http://pastebin.com/raw/" + pastebinLink);

                        var newMapping = new Mapping(player.GetAccount().AdminName, pastebinLink, description, propLink, dimension);
                        newMapping.Objects = ParseObjectsFromString(pastebinData);
                        newMapping.DeleteObjects = ParseDeleteObjectsFromString(pastebinData);
                        newMapping.Insert();
                        newMapping.Load();

                        LogManager.Log(LogManager.LogTypes.MappingRequests, player.GetAccount().AccountName + " has created mapping request #" + newMapping.Id);
                        API.sendChatMessageToPlayer(player, Color.White, "You have successfully created and loaded mapping request #" + newMapping.Id);
                        return;
                    }
                    catch (WebException e)
                    {
                        ((HttpWebResponse)e.Response).StatusCode.ToString() == "NotFound"){
                            API.triggerClientEvent(player, "callMappingFunction", "send_error", "The pastebin link you entered does not exist.");
                            return;
                        }
                    }
                case "searchForMappingRequest":
                    var searchForId = Convert.ToInt32(arguments[0]);

                    var foundRequest = Mapping.Find(m => m.Id == searchForId);

                    if(foundRequest == null)
                    {
                        API.triggerClientEvent(player, "callMappingFunction", "send_error", "The mapping request you searched for does not exist.");
                        return;
                    }

                    player.GetAccount().ViewingMappingRequest = foundRequest;
                    player.sendChatMessage("You are now viewing mapping request #" + foundRequest.Id);

                    //id, createdBy, createdDate, propLink, dim, pastebinLink, description, isLoaded, isActive
                    API.triggerClientEvent(player, "callMappingFunction", "populateViewMappingRequest", foundRequest.Id, foundRequest.CreatedBy, foundRequest.CreatedDate, foundRequest.PropertyLinkId, foundRequest.Dimension, foundRequest.PastebinLink, foundRequest.Description, foundRequest.IsSpawned, foundRequest.IsActive);
                    break;
                case "saveMappingRequest":
                {
                    var mappingId = Convert.ToInt32(arguments[0]);
                    var newPropLink = Convert.ToInt32(arguments[1]);
                    var newDim = Convert.ToInt32(arguments[2]);
                    var newDesc = Convert.ToString(arguments[3]);

                    var editingRequest = player.GetAccount().ViewingMappingRequest;

                    if (editingRequest.Id != mappingId)
                    {
                        API.triggerClientEvent(player, "callMappingFunction", "send_error", "The mapping ID you are saving does not match the one you are viewing. Please hit search first.");
                        return;
                    }

                    editingRequest.PropertyLinkId = newPropLink;
                    editingRequest.Dimension = newDim;
                    editingRequest.Description = newDesc;
                    editingRequest.Save();
                    player.sendChatMessage("You saved mapping request #" + mappingId);
                    LogManager.Log(LogManager.LogTypes.MappingRequests, player.GetAccount().AccountName + " has saved mapping request #" + mappingId);
                    break;
                }
                case "deleteMappingRequest":
                {
                    var mappingId = Convert.ToInt32(arguments[0]);
                    var editingRequest = player.GetAccount().ViewingMappingRequest;

                    if (editingRequest.Id != mappingId)
                    {
                        API.triggerClientEvent(player, "callMappingFunction", "send_error", "The mapping ID you are saving does not match the one you are viewing. Please hit search first.");
                        return;
                    }

                    player.sendChatMessage("You have deleted mapping request #" + mappingId + ". This cannot be undone.");
                    LogManager.Log(LogManager.LogTypes.MappingRequests, player.GetAccount().AccountName + " has deleted mapping request #" + mappingId);
                    Mapping.Remove(editingRequest);
                    editingRequest.Unload();
                    editingRequest.Delete();
                    break;
                }
                case "toggleMappingLoaded":

                    break;
                case "toggleMappingActive":

                    break;
                case "requestMappingCode":

                    break;
            }
        }

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
