using System;
using System.Linq;
using System.Timers;

using GTANetworkAPI;

using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.group_manager;
using mtgvrp.player_manager;

using mtgvrp.core.Help;
using MongoDB.Driver;

namespace mtgvrp.door_manager
{
    public class DoorManager : Script
    {
        Timer reloadDoorsTimer = new Timer();

        public DoorManager()
        {
            Event.OnClientEventTrigger += API_onClientEventTrigger;

            API.ConsoleOutput("Loading all doors.");
            foreach (var door in DatabaseManager.DoorsTable.Find(FilterDefinition<Door>.Empty).ToEnumerable())
            {
                door.RegisterDoor();
            }
            API.ConsoleOutput("Loaded " + DatabaseManager.DoorsTable.Count(FilterDefinition<Door>.Empty) + " Doors");

            reloadDoorsTimer.Interval = 1000;
            reloadDoorsTimer.Elapsed += ReloadDoorsTimer_Elapsed;
            reloadDoorsTimer.Start();
        }

        private void ReloadDoorsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var door in Door.Doors)
            {
                foreach (var player in door.Shape.getAllEntities())
                {
                    var client = API.GetPlayerFromHandle(player);
                    if(client == null)
                        continue;
                    API.Shared.SendNativeToPlayer(client, Door.SetStateOfClosestDoorOfType,
                        door.Hash, door.Position.X, door.Position.Y, door.Position.Z,
                        door.Locked, door.State, false);
                }
            }
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

                        API.SendChatMessageToPlayer(sender, $"[Door Manager] Created a door with id {door.Id}");
                    }
                    break;

                case "doormanager_togglelock":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                        if (door == null)
                        {
                            API.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        door.Locked = !door.Locked;
                        door.RefreshDoor();
                        door.Save();
                        API.SendChatMessageToPlayer(sender, "[Door Manager] Door has been " + (door.Locked ? "~g~Locked" : "~r~Unlocked"));
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
                            API.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        door.Description = desc;
                        door.Save();
                        API.SendChatMessageToPlayer(sender, "[Door Manager] Door description has been set to ~g~" + desc);
                    }
                    break;

                case "doormanager_goto":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                        if (door == null)
                        {
                            API.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        sender.position = door.Position;
                        API.SendChatMessageToPlayer(sender, "[Door Manager] Teleported to door id " + door.Id);
                    }
                    break;

                case "doormanager_delete":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                        if (door == null)
                        {
                            API.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        door.Delete();
                        API.SendChatMessageToPlayer(sender, "[Door Manager] Door removed, id:" + door.Id);
                    }
                    break;

                case "doormanager_setgroup":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                        if (door == null)
                        {
                            API.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        int groupid;
                        if (int.TryParse((string)arguments[1], out groupid))
                        {
                            if (GroupManager.Groups.Exists(x => x.Id == groupid))
                            {
                                door.GroupId = groupid;
                                door.Save();
                                API.SendChatMessageToPlayer(sender,
                                    "[Door Manager] Group id set to " + groupid + " for door id:" + door.Id);
                            }
                            else
                            {
                                API.SendChatMessageToPlayer(sender, "Invalid group id.");
                            }
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
                            API.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        int prop;
                        if (int.TryParse((string)arguments[1], out prop))
                        {
                            if (Door.Doors.Exists(x => x.Id == prop))
                            {
                                door.PropertyId = prop;
                                door.Save();
                                API.SendChatMessageToPlayer(sender,
                                    "[Door Manager] PropertyID set to " + prop + " for door id:" + door.Id);
                            }
                            else
                            {
                                API.SendChatMessageToPlayer(sender, "Invalid door id.");
                            }
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
                            API.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                            return;
                        }
                        door.DoesShowInAdmin = !door.DoesShowInAdmin;
                        door.Save();
                        API.SendChatMessageToPlayer(sender,
                            "[Door Manager] Door was " + (door.DoesShowInAdmin ? "unhidden" : "hidden") +
                            " from /managedoors.");
                        API.SendChatMessageToPlayer(sender, "Could edit the door with /editdoor [id]");
                    }
                    break;

                case "doormanager_locknearestdoor":
                    float distance = -1.0f;
                    var cdoor = new Door(0, new Vector3(0, 0, 0), "NULL", false, false);
                    Vector3 playerPos = API.GetEntityPosition(sender.handle);
                    foreach(Door d in Door.Doors)
                    {
                        if(playerPos.DistanceTo(d.Position) < distance || distance == -1.0f)
                        {
                            cdoor = d;
                            distance = playerPos.DistanceTo(d.Position);
                        }
                    }
                    if(cdoor != null)
                    {
                        if(DoesPlayerHaveDoorAccess(sender, cdoor))
                        {
                            if(distance <= 10.0f)
                            {
                                if(cdoor.Locked)
                                {
                                    cdoor.Locked = false;
                                    API.SendChatMessageToPlayer(sender, "Door " + cdoor.Id + " ~g~Unlocked!");
                                }
                                else
                                {
                                    cdoor.Locked = true;
                                    API.SendChatMessageToPlayer(sender, "Door " + cdoor.Id + " ~r~Locked!");
                                }
                                cdoor.RefreshDoor();
                                cdoor.Save();
                            }
                        }
                    }
                    break;
            }
        }

        [Command("managedoors"), Help(HelpManager.CommandGroups.AdminLevel5, "Manage all existing doors.")]
        public void manage_doors(Client player)
        {
            if (player.GetAccount().AdminLevel >= 5)
            {
                var doors = Door.Doors.Where(x => x.DoesShowInAdmin == true).Select(x => new[] {x.Description, x.Id.ToString()}).ToArray();
                API.TriggerClientEvent(player, "doormanager_managedoors", API.ToJson(doors));
            }
        }

        [Command("editdoor"), Help(HelpManager.CommandGroups.AdminLevel5, "Edit a door.", "Door id")]
        public void edit_door(Client player, int id)
        {
            if (player.GetAccount().AdminLevel >= 5)
            {
                var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                if (door == null)
                {
                    API.SendChatMessageToPlayer(player, "[Door Manager] That ID doesn't exist.");
                    return;
                }
                API.TriggerClientEvent(player, "doormanager_editdoor", id);
            }
        }

        //Failsafe if the cursor doesn't work.
        [Command("createdoor", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel5, "Create a door manually.", "The door object model", "X position", "Y position", "Z position", "Description of the door")]
        public void create_door(Client player, int model, float x, float y, float z, string desc)
        {
            if (player.GetAccount().AdminLevel >= 5)
            {
                var door = new Door(model, new Vector3(x, y, z), desc, false, true);
                door.Insert();
                door.RegisterDoor();

                API.SendChatMessageToPlayer(player, $"[Door Manager] Created a door with id {door.Id}");
            }
        }

        public bool DoesPlayerHaveDoorAccess(Client client, Door d)
        {
            Character c = client.GetCharacter();
            Account a = client.GetAccount();
            if (c.GroupId != 0 && c.GroupId == d.GroupId) return true;
            //TODO add check for property
            if (a.AdminLevel >= 5) return true;
            return false;
        }

        [Command("lockdoor"), Help(HelpManager.CommandGroups.AdminLevel5 | HelpManager.CommandGroups.GroupGeneral, "Locks a door.", "Door id")]
        public void Lockdoor(Client player, int id)
        {
            var door = Door.Doors.SingleOrDefault(x => x.Id == id);
            if (door == null)
            {
                API.SendChatMessageToPlayer(player, "Invalid door id.");
                return;
            }
            if (DoesPlayerHaveDoorAccess(player, door))
            {
                if (player.position.DistanceTo(door.Position) > 10.0f)
                {
                    API.SendChatMessageToPlayer(player, "You must be near the door.");
                    return;
                }

                if(door.Locked)
                {
                    door.Locked = false;
                    API.SendChatMessageToPlayer(player, "Door ~g~Unlocked!");
                }
                else
                {
                    door.Locked = true;
                    API.SendChatMessageToPlayer(player, "Door ~r~Locked!");
                }
                
                door.RefreshDoor();
                door.Save();
            }
            else
                API.SendChatMessageToPlayer(player, "Insufficient permissions.");
        }

        /*[Command("unlockdoor"), Help(HelpManager.CommandGroups.AdminLevel5 | HelpManager.CommandGroups.GroupGeneral, "Unlocks a door.", "Door id")]
        public void Unlockdoor(Client player, int id)
        {
            var door = Door.Doors.SingleOrDefault(x => x.Id == id);
            if (door == null)
            {
                API.SendChatMessageToPlayer(player, "Invalid door id.");
                return;
            }
            if (DoesPlayerHaveDoorAccess(player, door)) //TODO: add check for property.
            {
                if (player.position.DistanceTo(door.Position) > 10.0f)
                {
                    API.SendChatMessageToPlayer(player, "You must be near the door.");
                    return;
                }

                door.Locked = false;
                door.RefreshDoor();
                door.Save();
                API.SendChatMessageToPlayer(player, "Door ~g~Unlocked!");
            }
            else
                API.SendChatMessageToPlayer(player, "Insufficient permissions.");
        }*/
    }
}
