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
            NAPI.Util.ConsoleOutput("Loading all doors.");
            foreach (var door in DatabaseManager.DoorsTable.Find(FilterDefinition<Door>.Empty).ToEnumerable())
            {
                door.RegisterDoor();
            }
            NAPI.Util.ConsoleOutput("Loaded " + DatabaseManager.DoorsTable.CountDocuments(FilterDefinition<Door>.Empty) + " Doors");

            reloadDoorsTimer.Interval = 1000;
            reloadDoorsTimer.Elapsed += ReloadDoorsTimer_Elapsed;
            reloadDoorsTimer.Start();
        }

        // TODO: fixme
        private void ReloadDoorsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var door in Door.Doors)
            {
                /*foreach (var player in door.Shape.getAllEntities())
                {
                    var client = NAPI.Player.GetPlayerFromHandle(player);
                    if(client == null)
                        continue;
                    API.Shared.SendNativeToPlayer(client, Door.SetStateOfClosestDoorOfType,
                        door.Hash, door.Position.X, door.Position.Y, door.Position.Z,
                        door.Locked, door.State, false);
                }*/
            }
        }

        [RemoteEvent("doormanager_createdoor")]
        public void DoorManagerCreateDoor(Player sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var model = (int)arguments[0];
                var position = (Vector3)arguments[1];
                var desc = (string)arguments[2];

                var door = new Door(model, position, desc, false, true);
                door.Insert();
                door.RegisterDoor();

                NAPI.Chat.SendChatMessageToPlayer(sender, $"[Door Manager] Created a door with id {door.Id}");
            }
        }

        [RemoteEvent("doormanager_togglelock")]
        public void DoorManagerToggleLock(Player sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                if (door == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                    return;
                }
                door.Locked = !door.Locked;
                door.RefreshDoor();
                door.Save();
                NAPI.Chat.SendChatMessageToPlayer(sender, "[Door Manager] Door has been " + (door.Locked ? "~g~Locked" : "~r~Unlocked"));
            }
        }

        [RemoteEvent("doormanager_changedesc")]
        public void DoorManagerChangeDesc(Player sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var desc = (string)arguments[1];
                var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                if (door == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                    return;
                }
                door.Description = desc;
                door.Save();
                NAPI.Chat.SendChatMessageToPlayer(sender, "[Door Manager] Door description has been set to ~g~" + desc);
            }
        }

        [RemoteEvent("doormanager_goto")]
        public void DoorManagerGoto(Player sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                if (door == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                    return;
                }
                sender.Position = door.Position;
                NAPI.Chat.SendChatMessageToPlayer(sender, "[Door Manager] Teleported to door id " + door.Id);
            }
        }

        [RemoteEvent("doormanager_delete")]
        public void DoorManagerDelete(Player sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                if (door == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                    return;
                }
                door.Delete();
                NAPI.Chat.SendChatMessageToPlayer(sender, "[Door Manager] Door removed, id:" + door.Id);
            }
        }

        [RemoteEvent("doormanager_setgroup")]
        public void DoorManagerSetGroup(Player sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                if (door == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                    return;
                }
                int groupid;
                if (int.TryParse((string)arguments[1], out groupid))
                {
                    if (GroupManager.Groups.Exists(x => x.Id == groupid))
                    {
                        door.GroupId = groupid;
                        door.Save();
                        NAPI.Chat.SendChatMessageToPlayer(sender,
                            "[Door Manager] Group id set to " + groupid + " for door id:" + door.Id);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(sender, "Invalid group id.");
                    }
                }
            }
        }

        [RemoteEvent("doormanager_setproperty")]
        public void DoorManagerSetProperty(Player sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                if (door == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                    return;
                }
                int prop;
                if (int.TryParse((string)arguments[1], out prop))
                {
                    if (Door.Doors.Exists(x => x.Id == prop))
                    {
                        door.PropertyId = prop;
                        door.Save();
                        NAPI.Chat.SendChatMessageToPlayer(sender,
                            "[Door Manager] PropertyID set to " + prop + " for door id:" + door.Id);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(sender, "Invalid door id.");
                    }
                }
            }
        }

        [RemoteEvent("doormanager_hide")]
        public void DoorManagerHide(Player sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                if (door == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Door Manager] That ID doesn't exist.");
                    return;
                }
                door.DoesShowInAdmin = !door.DoesShowInAdmin;
                door.Save();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    "[Door Manager] Door was " + (door.DoesShowInAdmin ? "unhidden" : "hidden") +
                    " from /managedoors.");
                NAPI.Chat.SendChatMessageToPlayer(sender, "Could edit the door with /editdoor [id]");
            }
        }

        [RemoteEvent("doormanager_locknearestdoor")]
        public void DoorManagerLockNearestDoor(Player sender, params object[] arguments)
        {
            float distance = -1.0f;
            var cdoor = new Door(0, new Vector3(0, 0, 0), "NULL", false, false);
            Vector3 playerPos = NAPI.Entity.GetEntityPosition(sender);
            foreach (Door d in Door.Doors)
            {
                if (playerPos.DistanceTo(d.Position) < distance || distance == -1.0f)
                {
                    cdoor = d;
                    distance = playerPos.DistanceTo(d.Position);
                }
            }
            if (cdoor != null)
            {
                if (DoesPlayerHaveDoorAccess(sender, cdoor))
                {
                    if (distance <= 10.0f)
                    {
                        if (cdoor.Locked)
                        {
                            cdoor.Locked = false;
                            NAPI.Chat.SendChatMessageToPlayer(sender, "Door " + cdoor.Id + " ~g~Unlocked!");
                        }
                        else
                        {
                            cdoor.Locked = true;
                            NAPI.Chat.SendChatMessageToPlayer(sender, "Door " + cdoor.Id + " ~r~Locked!");
                        }
                        cdoor.RefreshDoor();
                        cdoor.Save();
                    }
                }
            }
        }

        [Command("managedoors"), Help(HelpManager.CommandGroups.AdminLevel5, "Manage all existing doors.")]
        public void manage_doors(Player player)
        {
            if (player.GetAccount().AdminLevel >= 5)
            {
                var doors = Door.Doors.Where(x => x.DoesShowInAdmin == true).Select(x => new[] {x.Description, x.Id.ToString()}).ToArray();
                NAPI.ClientEvent.TriggerClientEvent(player, "doormanager_managedoors", NAPI.Util.ToJson(doors));
            }
        }

        [Command("editdoor"), Help(HelpManager.CommandGroups.AdminLevel5, "Edit a door.", "Door id")]
        public void edit_door(Player player, int id)
        {
            if (player.GetAccount().AdminLevel >= 5)
            {
                var door = Door.Doors.SingleOrDefault(x => x.Id == id);
                if (door == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "[Door Manager] That ID doesn't exist.");
                    return;
                }
                NAPI.ClientEvent.TriggerClientEvent(player, "doormanager_editdoor", id);
            }
        }

        //Failsafe if the cursor doesn't work.
        [Command("createdoor", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel5, "Create a door manually.", "The door object model", "X position", "Y position", "Z position", "Description of the door")]
        public void create_door(Player player, int model, float x, float y, float z, string desc)
        {
            if (player.GetAccount().AdminLevel >= 5)
            {
                var door = new Door(model, new Vector3(x, y, z), desc, false, true);
                door.Insert();
                door.RegisterDoor();

                NAPI.Chat.SendChatMessageToPlayer(player, $"[Door Manager] Created a door with id {door.Id}");
            }
        }

        public bool DoesPlayerHaveDoorAccess(Player client, Door d)
        {
            Character c = client.GetCharacter();
            Account a = client.GetAccount();
            if (c.GroupId != 0 && c.GroupId == d.GroupId) return true;
            //TODO add check for property
            if (a.AdminLevel >= 5) return true;
            return false;
        }

        [Command("lockdoor"), Help(HelpManager.CommandGroups.AdminLevel5 | HelpManager.CommandGroups.GroupGeneral, "Locks a door.", "Door id")]
        public void Lockdoor(Player player, int id)
        {
            var door = Door.Doors.SingleOrDefault(x => x.Id == id);
            if (door == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Invalid door id.");
                return;
            }
            if (DoesPlayerHaveDoorAccess(player, door))
            {
                if (player.Position.DistanceTo(door.Position) > 10.0f)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "You must be near the door.");
                    return;
                }

                if(door.Locked)
                {
                    door.Locked = false;
                    NAPI.Chat.SendChatMessageToPlayer(player, "Door ~g~Unlocked!");
                }
                else
                {
                    door.Locked = true;
                    NAPI.Chat.SendChatMessageToPlayer(player, "Door ~r~Locked!");
                }
                
                door.RefreshDoor();
                door.Save();
            }
            else
                NAPI.Chat.SendChatMessageToPlayer(player, "Insufficient permissions.");
        }

        /*[Command("unlockdoor"), Help(HelpManager.CommandGroups.AdminLevel5 | HelpManager.CommandGroups.GroupGeneral, "Unlocks a door.", "Door id")]
        public void Unlockdoor(Player player, int id)
        {
            var door = Door.Doors.SingleOrDefault(x => x.Id == id);
            if (door == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Invalid door id.");
                return;
            }
            if (DoesPlayerHaveDoorAccess(player, door)) //TODO: add check for property.
            {
                if (player.position.DistanceTo(door.Position) > 10.0f)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "You must be near the door.");
                    return;
                }

                door.Locked = false;
                door.RefreshDoor();
                door.Save();
                NAPI.Chat.SendChatMessageToPlayer(player, "Door ~g~Unlocked!");
            }
            else
                NAPI.Chat.SendChatMessageToPlayer(player, "Insufficient permissions.");
        }*/
    }
}
