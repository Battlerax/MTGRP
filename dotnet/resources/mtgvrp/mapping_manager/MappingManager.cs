using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.database_manager;
using mtgvrp.property_system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

using GTANetworkAPI;


using MongoDB.Driver;

namespace mtgvrp.mapping_manager
{
    public class MappingManager : Script
    {
        public List<Mapping> Mapping;

        public MappingManager()
        {
            DebugManager.DebugMessage("[MAPPING MANAGER] Initializing mapping manager...");

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

        [RemoteEvent("requestCreateMapping")]
        public void RequestCreateMapping(Player player, params object[] arguments)
        {
            var propLink = Convert.ToInt32(arguments[0]);
            var dimension = Convert.ToInt32(arguments[1]);
            var pastebinLink = Convert.ToString(arguments[2]);
            var description = Convert.ToString(arguments[3]);

            if (dimension < 0)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "send_error", "The dimension entered is less than 0.");
                return;
            }

            if (propLink > 0)
            {
                if (PropertyManager.Properties.FindAll(p => p.Id == propLink).Count < 1)
                {
                    NAPI.ClientEvent.TriggerClientEvent(player, "send_error", "The property link ID you entered is invalid.");
                    return;
                }
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
                Mapping.Add(newMapping);

                LogManager.Log(LogManager.LogTypes.MappingRequests, player.GetAccount().AccountName + " has created mapping request #" + newMapping.Id);
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "You have successfully created and loaded mapping request #" + newMapping.Id);
                return;
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode.ToString() == "NotFound")
                {
                    NAPI.ClientEvent.TriggerClientEvent(player, "send_error", "The pastebin link you entered does not exist.");
                    return;
                }
            }
        }

        [RemoteEvent("searchForMappingRequest")]
        public void SearchForMappingRequest(Player player, params object[] arguments)
        {
            var searchForId = Convert.ToInt32(arguments[0]);

            var foundRequest = Mapping.Find(m => m.Id == searchForId);

            if (foundRequest == null)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "send_error", "The mapping request you searched for does not exist.");
                return;
            }

            //id, createdBy, createdDate, propLink, dim, pastebinLink, description, isLoaded, isActive
            NAPI.ClientEvent.TriggerClientEvent(player, "populateViewMappingRequest", foundRequest.Id, foundRequest.CreatedBy, foundRequest.CreatedDate.ToString(), foundRequest.PropertyLinkId, foundRequest.Dimension, foundRequest.PastebinLink, foundRequest.Description, foundRequest.IsSpawned, foundRequest.IsActive);
            player.GetAccount().ViewingMappingRequest = foundRequest;
            player.SendChatMessage("You are now viewing mapping request #" + foundRequest.Id);
        }

        [RemoteEvent("saveMappingRequest")]
        public void SaveMappingRequest(Player player, params object[] arguments)
        {
            var mappingId = Convert.ToInt32(arguments[0]);
            var newPropLink = Convert.ToInt32(arguments[1]);
            var newDim = Convert.ToInt32(arguments[2]);
            var newDesc = Convert.ToString(arguments[3]);

            var editingRequest = player.GetAccount().ViewingMappingRequest;

            if (editingRequest.Id != mappingId)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "send_error", "The mapping ID you are saving does not match the one you are viewing. Please hit search first.");
                return;
            }

            editingRequest.PropertyLinkId = newPropLink;
            editingRequest.Dimension = newDim;
            editingRequest.Description = newDesc;
            editingRequest.Save();
            player.SendChatMessage("You saved mapping request #" + mappingId);
            LogManager.Log(LogManager.LogTypes.MappingRequests, player.GetAccount().AccountName + " has saved mapping request #" + mappingId);
        }

        [RemoteEvent("deleteMappingRequest")]
        public void DeleteMappingRequest(Player player, params object[] arguments)
        {
            var mappingId = Convert.ToInt32(arguments[0]);
            var editingRequest = player.GetAccount().ViewingMappingRequest;

            if (editingRequest.Id != mappingId)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "send_error", "The mapping ID you are saving does not match the one you are viewing. Please hit search first.");
                return;
            }

            player.SendChatMessage("You have deleted mapping request #" + mappingId + ". This cannot be undone.");
            LogManager.Log(LogManager.LogTypes.MappingRequests, player.GetAccount().AccountName + " has deleted mapping request #" + mappingId);
            Mapping.Remove(editingRequest);
            editingRequest.Unload();
            editingRequest.Delete();
        }

        [RemoteEvent("toggleMappingLoaded")]
        public void ToggleMappingLoaded(Player player, params object[] arguments)
        {
            var mappingId = Convert.ToInt32(arguments[0]);
            var editingRequest = player.GetAccount().ViewingMappingRequest;

            if (editingRequest.Id != mappingId)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "send_error", "The mapping ID you are saving does not match the one you are viewing. Please hit search first.");
                return;
            }

            editingRequest.IsSpawned = !editingRequest.IsSpawned;

            if (editingRequest.IsSpawned == true)
            {
                editingRequest.Load();
            }
            else
            {
                editingRequest.Unload();
            }

            editingRequest.Save();
            player.SendChatMessage("You have " + ((editingRequest.IsSpawned == true) ? ("loaded") : ("unloaded")) + " mapping request #" + mappingId);
            LogManager.Log(LogManager.LogTypes.MappingRequests, player.GetAccount().AccountName + "has  " + ((editingRequest.IsSpawned == true) ? ("loaded") : ("unloaded")) + " mapping request #" + mappingId);
        }

        [RemoteEvent("toggleMappingActive")]
        public void ToggleMappingActive(Player player, params object[] arguments)
        {
            var mappingId = Convert.ToInt32(arguments[0]);
            var editingRequest = player.GetAccount().ViewingMappingRequest;

            if (editingRequest.Id != mappingId)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "send_error", "The mapping ID you are saving does not match the one you are viewing. Please hit search first.");
                return;
            }

            editingRequest.IsActive = !editingRequest.IsActive;

            if (editingRequest.IsActive == true)
            {
                editingRequest.Load();
            }

            editingRequest.Save();
            player.SendChatMessage("You have " + ((editingRequest.IsActive == true) ? ("actived") : ("deactivated")) + " mapping request #" + mappingId + ". Deactived mapping does not spawn on server load.");
            LogManager.Log(LogManager.LogTypes.MappingRequests, player.GetAccount().AccountName + "has  " + ((editingRequest.IsActive == true) ? ("activated") : ("deactived")) + " mapping request #" + mappingId);
        }

        [RemoteEvent("requestMappingCode")]
        public void RequestMappingCode(Player player, params object[] arguments)
        {
            var mappingId = Convert.ToInt32(arguments[0]);
            var editingRequest = player.GetAccount().ViewingMappingRequest;

            if (editingRequest.Id != mappingId)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "send_error", "The mapping ID you are saving does not match the one you are viewing. Please hit search first.");
                return;
            }

            var mappingString = "";

            foreach (var o in editingRequest.Objects)
            {

                if (o.Type == MappingObject.ObjectType.CreateObject)
                {
                    //API.CreateObject(int model, Vector3 pos, Vector3 rot, int dimension = 0);
                    mappingString += string.Format("API.CreateObject({0}, new Vector3({1}, {2}, {3}), new Vector3({4}, {5}, {6}, {7});\n", o.Model, o.Pos.X, o.Pos.Y, o.Pos.Z, o.Rot.X, o.Rot.Y, o.Rot.Z, editingRequest.Dimension);
                }
                else
                {
                    //API.DeleteObject(Player client, Vector3 position, int modelHash);
                    mappingString += string.Format("API.DeleteObject(player, new Vector3({0}, {1}, {2}), {4});\n", o.Pos.X, o.Pos.Y, o.Pos.Z, o.Model);
                }

            }

            NAPI.ClientEvent.TriggerClientEvent(player, "showRequestCode", mappingString);
        }

        [RemoteEvent("requestFirstMappingPage")]
        public void RequestFirstMappingPage(Player player, params object[] arguments)
        {
            var count = 0;
            foreach (var o in Mapping)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "addMappingRequest", o.Id, o.Description, o.CreatedBy, o.IsActive);
                count++;
                if (count == 10)
                    break;
            }

            var numOfPages = (Mapping.Count() + 9) / 10;
            var middlePage = ((1 + numOfPages) / 2) - 1;
            NAPI.ClientEvent.TriggerClientEvent(player, "createPagination", 1, middlePage, numOfPages);
        }

        [RemoteEvent("requestMappingPage")]
        public void RequestMappingPage(Player player, params object[] arguments)
        {
            var page = Convert.ToInt32(arguments[0]);
            NAPI.ClientEvent.TriggerClientEvent(player, "emptyMappingTable");
            var count = 0;
            foreach (var o in Mapping.Skip(page * 10))
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "addMappingRequest", o.Id, o.Description, o.CreatedBy, o.IsActive);
                count++;

                if (count == 10)
                    break;
            }
        }

        public List<MappingObject> ParseObjectsFromString(string input)
        {
            List<MappingObject> objectList = new List<MappingObject>();

            var objectPattern = @"API.CreateObject\s*\((?<model>-?[0-9]+)\s*,\s*new\s*Vector3\s*\(\s*(?<posX>-?[0-9.]*)\s*,\s*(?<posY>-?[0-9.]*)\s*,\s*(?<posZ>-?[0-9.]*)\)\s*,\s*new\s*Vector3\s*\(\s*(?<rotX>-?[0-9.]*)\s*,\s*(?<rotY>-?[0-9.]*)\s*,\s*(?<rotZ>-?[0-9.]*)\s*\)\s*\)";
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

            var objectPattern = @"API.DeleteObject\s*\(player,\s*new\s*Vector3\s*\(\s*(?<posX>-?[0-9.]*)\s*,\s*(?<posY>-?[0-9.]*)\s*,\s*(?<posZ>-?[0-9.]*)\)\s*,\s*(?<model>-?[0-9]+)\s*\);";
            var regex = new Regex(objectPattern);
            foreach (Match match in regex.Matches(input))
            {
                objectList.Add(new MappingObject(MappingObject.ObjectType.DeleteObject, Convert.ToInt32(match.Groups["model"].ToString()), new Vector3((float)Convert.ToDouble(match.Groups["posX"].ToString()), (float)Convert.ToDouble(match.Groups["posY"].ToString()), (float)Convert.ToDouble(match.Groups["posZ"].ToString())), new Vector3((float)Convert.ToDouble(match.Groups["rotX"].ToString()), (float)Convert.ToDouble(match.Groups["rotY"].ToString()), (float)Convert.ToDouble(match.Groups["rotZ"].ToString()))));
            }

            return objectList;
        }

        [Command("mappingmanager"), Help(HelpManager.CommandGroups.AdminLevel5, "Used to manage the server mapping. Do not play around with this.", null)]
        public void mappingmanager_cmd(Player player)
        {
            if(player.GetAccount().AdminLevel < 5)
            {
                return;
            }

            player.SendChatMessage("Mapping manager opened. Hit ~r~ESC~w~ to close it.");
            NAPI.ClientEvent.TriggerClientEvent(player, "showMappingManager");
            return;
        }
    }
}
