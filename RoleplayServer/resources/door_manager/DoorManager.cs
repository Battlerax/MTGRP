using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.door_manager
{
    public class DoorManager : Script
    {
        public DoorManager()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;

            API.consoleOutput("Loading all doors.");
            foreach (var door in DatabaseManager.DoorsTable.Find(FilterDefinition<Door>.Empty).ToEnumerable())
            {
                door.RegisterDoor();
            }
            API.consoleOutput("Loaded " + DatabaseManager.DoorsTable.Count(FilterDefinition<Door>.Empty) + " Doors");
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "doormanager_createdoor":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var model = (int)arguments[0];
                        var position = (Vector3) arguments[1];
                        var desc = (string) arguments[2];
                        
                        var door = new Door(model, position, desc, false, true);
                        door.Insert();
                        door.RegisterDoor();

                        API.sendChatMessageToPlayer(sender, $"[Door Manager] Created a door with id {door.Id}");
                    }
                    break;

                case "doormanager_togglelock":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                        if (door == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        door.Locked = !door.Locked;
                        door.RefreshDoor();
                        door.Save();
                        API.sendChatMessageToPlayer(sender, "[Door Manager] Door has been " + (door.Locked ? "~g~Locked" : "~r~Unlocked"));
                    }
                    break;

                case "doormanager_changedesc":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var desc = (string) arguments[1];
                        var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                        if (door == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        door.Description = desc;
                        door.Save();
                        API.sendChatMessageToPlayer(sender, "[Door Manager] Door description has been set to ~g~" + desc);
                    }
                    break;

                case "doormanager_goto":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                        if (door == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        sender.position = door.Position;
                        API.sendChatMessageToPlayer(sender, "[Door Manager] Teleported to door id " + door.Id);
                    }
                    break;

                case "doormanager_delete":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                        if (door == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        door.Delete();
                        API.sendChatMessageToPlayer(sender, "[Door Manager] Door removed, id:" + door.Id);
                    }
                    break;

                case "doormanager_setgroup":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                        if (door == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        int groupid;
                        if (int.TryParse((string)arguments[1], out groupid))
                        {
                            door.GroupId = groupid;
                            door.Save();
                            API.sendChatMessageToPlayer(sender, "[Door Manager] Group id set to " + groupid + " for door id:" + door.Id);
                        }
                    }
                    break;

                case "doormanager_setproperty":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                        if (door == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        int prop;
                        if (int.TryParse((string)arguments[1], out prop))
                        {
                            door.PropertyId = prop;
                            door.Save();
                            API.sendChatMessageToPlayer(sender, "[Door Manager] PropertyID set to " + prop + " for door id:" + door.Id);
                        }
                    }
                    break;

                case "doormanager_hide":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                        if (door == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        door.DoesShowInAdmin = !door.DoesShowInAdmin;
                        door.Save();
                        API.sendChatMessageToPlayer(sender,
                            "[Door Manager] Door was " + (door.DoesShowInAdmin ? "unhidden" : "hidden") +
                            " from /managedoors.");
                        API.sendChatMessageToPlayer(sender, "Could edit the door with /editdoor [id]");
                    }
                    break;
            }
        }

        [Command("managedoors")]
        public void manage_doors(Client player)
        {
            if (player.GetAccount().AdminLevel >= 5)
            {
                var doors = Door.Doors.Where(x => x.DoesShowInAdmin == true).Select(x => new[] {x.Description, x.Id.ToString()}).ToArray();
                API.triggerClientEvent(player, "doormanager_managedoors", API.toJson(doors));
            }
        }

        [Command("editdoor")]
        public void edit_door(Client player, int id)
        {
            if (player.GetAccount().AdminLevel >= 5)
            {
                var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                if (door == null)
                {
                    API.sendChatMessageToPlayer(player, "[Door Manager] That ID doesn't exist.");
                    return;
                }
                API.triggerClientEvent(player, "doormanager_editdoor", id);
            }
        }

        //Failsafe if the cursor doesn't work.
        [Command("createdoor", GreedyArg = true)]
        public void create_door(Client player, int model, float x, float y, float z, string desc)
        {
            if (player.GetAccount().AdminLevel >= 5)
            {
                var door = new Door(model, new Vector3(x, y, z), desc, false, true);
                door.Insert();
                door.RegisterDoor();

                API.sendChatMessageToPlayer(player, $"[Door Manager] Created a door with id {door.Id}");
            }
        }

        [Command("lockdoor")]
        public void Lockdoor(Client player, int id)
        {
            var door = Door.Doors.SingleOrDefault(x => x.Id == id);
            if (door == null)
            {
                API.sendChatMessageToPlayer(player, "Invalid door id.");
                return;
            }
            var character = player.GetCharacter();
            var account = player.GetAccount();
            if (character.GroupId == door.GroupId || account.AdminLevel >= 5) //TODO: add check for property.
            {
                if (player.position.DistanceTo(door.Position) > 10.0f)
                {
                    API.sendChatMessageToPlayer(player, "You must be near the door.");
                    return;
                }

                door.Locked = true;
                door.RefreshDoor();
                door.Save();
                API.sendChatMessageToPlayer(player, "Door ~r~Locked!");
            }
            else
                API.sendChatMessageToPlayer(player, "Insufficient permissions.");
        }

        [Command("unlockdoor")]
        public void Unlockdoor(Client player, int id)
        {
            var door = Door.Doors.SingleOrDefault(x => x.Id == id);
            if (door == null)
            {
                API.sendChatMessageToPlayer(player, "Invalid door id.");
                return;
            }
            var character = player.GetCharacter();
            var account = player.GetAccount();
            if (character.GroupId == door.GroupId || account.AdminLevel >= 5) //TODO: add check for property.
            {
                if (player.position.DistanceTo(door.Position) > 10.0f)
                {
                    API.sendChatMessageToPlayer(player, "You must be near the door.");
                    return;
                }

                door.Locked = false;
                door.RefreshDoor();
                door.Save();
                API.sendChatMessageToPlayer(player, "Door ~g~Unocked!");
            }
            else
                API.sendChatMessageToPlayer(player, "Insufficient permissions.");
        }
    }
}
