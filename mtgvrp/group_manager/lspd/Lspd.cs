using System.Collections.Generic;
using System.Linq;
using System.Timers;

using GTANetworkAPI;

using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.weapon_manager;
using mtgvrp.core.Help;
using Color = mtgvrp.core.Color;

namespace mtgvrp.group_manager.lspd
{
    class Lspd : Script
    {

        public Lspd()
        {
            Event.OnResourceStart += StartLspd;
            Event.OnPlayerDisconnected += API_onPlayerDisconnected;
        }

        private void API_onPlayerDisconnected(Client player, byte type, string reason)
        {
            var character = player.GetCharacter();
            if (character == null) return;

            if (character.MegaPhoneObject != null && API.DoesEntityExist(character.MegaPhoneObject))
                API.DeleteEntity(character.MegaPhoneObject);
        }

        //LSPD Locations. TODO: MAKE IT WORK WITH MARKERZONE!!!!
        public static readonly Vector3 JailOne = new Vector3(458.0021f, -1001.581f, 24.91485f);
        public static readonly Vector3 JailTwo = new Vector3(458.7058f, -998.1188f, 24.91487f);
        public static readonly Vector3 JailThree = new Vector3(459.6695f, -994.0704f, 24.91487f);
        public static readonly Vector3 FreeJail = new Vector3(427.7434f, -976.0182f, 30.70999f);
        public readonly Vector3 JailPosOne = new Vector3(461.8065, -994.4086, 25.06443);
        public readonly Vector3 JailPosTwo = new Vector3(461.8065, -997.6583, 25.06443);
        public readonly Vector3 JailPosThree = new Vector3(461.8065, -1001.302, 25.06443);

        public LinkedList<Object> Objects = new LinkedList<Object>();

        private void StartLspd()
        {
            Crime.LoadCrimes();
        }

        [RemoteEvent("LSPD_Menu_Change_Clothes")]
        private void LSPDMenuChangeClothes(Client player, params object[] arguments)
        {
            Character c = player.GetCharacter();

            if (c.Group.CommandType != Group.CommandTypeLspd)
            {
                return;
            }

            c.IsInPoliceUniform = !c.IsInPoliceUniform;
            ChatManager.AmeLabelMessage(player,
                "changes into their " + (c.IsInPoliceUniform ? "police uniform." : "civilian clothing."), 8000);
            c.update_ped();
            c.Save();
        }

        [RemoteEvent("LSPD_Menu_Toggle_Duty")]
        private void LSPDMenuToggleDuty(Client player, params object[] arguments)
        {
            Character c = player.GetCharacter();

            if (c.Group.CommandType != Group.CommandTypeLspd)
            {
                return;
            }

            c.IsOnPoliceDuty = !c.IsOnPoliceDuty;
            GroupManager.SendGroupMessage(player,
                c.rp_name() + " is now " + (c.IsOnPoliceDuty ? "on" : "off") + " police duty.");
            c.Save();
        }

        [RemoteEvent("LSPD_Menu_Equip_Standard_Equipment")]
        private void LSPDMenuEquipStandardEquipment(Client player, params object[] arguments)
        {
            Character c = player.GetCharacter();

            if (c.Group.CommandType != Group.CommandTypeLspd)
            {
                return;
            }

            GiveLspdEquipment(player);
            NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You have been given the standard LSPD equipment.");
        }

        [RemoteEvent("LSPD_Menu_Equip_SWAT_Equipment")]
        private void LSPDMenuEquipSWATEquipment(Client player, params object[] arguments)
        {
            Character c = player.GetCharacter();

            if (c.Group.CommandType != Group.CommandTypeLspd)
            {
                return;
            }

            GiveLspdEquipment(player, 1);
            NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You have been given the standard SWAT equipment.");
        }

        [Command("recordcrime", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Record a player's crime, adding them to the wanted list.", new[] { "The target player ID.", "The crime ID" })]
        public void recordcrimes_cmd(Client player, string id, string type, string crimename, string jailTime, string fine)
        {
            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            receiverCharacter.RecordCrime(character.CharacterName, new Crime(type, crimename, int.Parse(jailTime), int.Parse(fine)));
            NAPI.Notification.SendNotificationToPlayer(player, "You have recorded " + receiverCharacter.rp_name() + " for committing: " + crimename);
            NAPI.Notification.SendNotificationToPlayer(receiver, character.rp_name() + " has recorded a crime you committed: ~r~" + crimename + "~w~.");
        }

        /*
        [Command("recordcrime", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Record a player's crime, adding them to the wanted list.", new[] { "The target player ID.", "The crime ID" })]
        public void recordcrimes_cmd(Client player, string id, string crimeid)
        {
            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            var crime = Crime.Crimes.Find(c => c.Id == int.Parse(crimeid));

            if (crime == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "No crime with that ID exists.");
                return;
            }

            receiverCharacter.RecordCrime(character.CharacterName, crime);
            NAPI.Notification.SendNotificationToPlayer(player, "You have recorded " + receiverCharacter.CharacterName + " for committing: " + crime.Name);
            NAPI.Notification.SendNotificationToPlayer(receiver, character.CharacterName + " has recorded a crime you committed: ~r~" + crime.Name + "~w~.");
        }
        */

        [Command("showcriminalrecord", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Show the criminal record of a player.", new[] { "The target player ID." })]
        public void criminalrecord_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            NAPI.Chat.SendChatMessageToPlayer(player, "=======CRIMIMNAL RECORD FOR " + receiverCharacter.rp_name() + "=======");
            foreach (var i in receiverCharacter.GetCriminalRecord())
            {
                if(i.ActiveCrime == true)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "~r~[ACTIVE] Type: " + i.Crime.Type + " Crime: " + i.Crime.Name + " Date issued: " + i.DateTime + " Recording officer: " + i.OfficerName);
                }

                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "Type: " + i.Crime.Type + "Crime: " + i.Crime.Name + " Date issued: " + i.DateTime + " Recording officer: " + i.OfficerName);
                }
            }
        }

        [Command("listcrimes"), Help(HelpManager.CommandGroups.LSPD, "List the available crimes and their crime ID", null)]
        public void listcrimes_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            player.SendChatMessage("======================");
            player.SendChatMessage("CRIME LIST");
            player.SendChatMessage("======================");
            int f = 0;
            foreach (var i in Crime.Crimes)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, f + " | " + i.Type + " | " + i.Name + " | " + i.JailTime + " | " + i.Fine); //TODO: REPLACE WITH A MENU
                f++;
            }
        }

        [Command("createcrime", GreedyArg=true), Help(HelpManager.CommandGroups.LSPD, "Create a crime and add it to the crime list.", "The type of the crime", "The jail time in seconds", "The fine amount", "Crime name")]
        public void createcrime_cmd(Client player, string type, int jailTime, int fine, string crimeName)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }


            if (Crime.CrimeExists(crimeName))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "This crime already exists!");
                return;
            }

            Crime.InsertCrime(type, crimeName, jailTime, fine);
            NAPI.Chat.SendChatMessageToPlayer(player, "Crime created and added to crime list.");
        }

        [Command("editcrime"), Help(HelpManager.CommandGroups.LSPD, "Edit a crime.", new[] { "Crime ID", "The crime type", "The crime name", "The jail time", "The fine for this crime" })]
        public void editcrime_cmd(Client player, int id, string type, string crimeName, int jailTime, int fine)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (Crime.CrimeExists(crimeName))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "This crime already exists!");
                return;
            }

            Crime crime = Crime.Crimes[id];
            Crime.Crimes[id] = new Crime(type, crimeName, jailTime, fine);
            Crime.UpdateCrimes();
            NAPI.Chat.SendChatMessageToPlayer(player, "Crime edited.");
        }

        [Command("deletecrime"), Help(HelpManager.CommandGroups.LSPD, "Delete a crime by its ID.", new[] { "The target crime ID." })]
        public void deletecrime_cmd(Client player, int id)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (!(id < Crime.Crimes.Count))
            {
                player.SendChatMessage("That crime does not exist.");
                return;
            }

            Crime crimeDelete = Crime.Crimes[id];


            crimeDelete.Delete();
            NAPI.Chat.SendChatMessageToPlayer(player, "Crime deleted from crime list.");
        }

        [Command("wanted", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Show the wanted list.", null)]
        public void wanted_cmd(Client player)
        {
            Character character = player.GetCharacter();
            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            var players = API.GetAllPlayers();

            NAPI.Chat.SendChatMessageToPlayer(player, "---------------------WANTED LIST---------------------");


            foreach (var c in PlayerManager.Players)
            {
                if (c.HasActiveCriminalRecord() > 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, c.rp_name() + " is wanted with " + c.HasActiveCriminalRecord() + " crimes.");
                }
            }
        }


        [Command("arrest", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Arrest a player after their crimes are recorded.", new[] { "The target player ID." })]
        public void arrest_cmd(Client player, string id)
        {

            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (receiver == player)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You can't arrest yourself!");
                return;
            }

            if (receiverCharacter.HasActiveCriminalRecord() == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "This player has no active crimes.");
            }
            
            if (character.GroupId == receiverCharacter.GroupId)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ You cannot arrest a member of the LSPD.");
                return;
            }

            if (player.Position.DistanceTo(character.Group.ArrestLocation.Location) > 4)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You are not at the arrest location.");
                return;
            }

            int time = 0;
            int fine = 0;

            var criminalRecord = receiverCharacter.GetCriminalRecord();

            foreach (var j in criminalRecord)
            {
                if (j.ActiveCrime == true)
                {
                    time += j.Crime.JailTime;
                    fine += j.Crime.Fine;
                    j.ActiveCrime = false;
                    j.Save();
                }
            }

            API.SendNativeToAllPlayers(Hash.SET_ENABLE_HANDCUFFS, receiverCharacter.Client.Handle, false);
            receiverCharacter.IsCuffed = false;
            API.StopPlayerAnimation(receiverCharacter.Client);

            NAPI.Notification.SendNotificationToPlayer(player, "You have arrested ~b~" + receiverCharacter.rp_name() + "~w~.");
            NAPI.Notification.SendNotificationToPlayer(receiver, "You have been arrested by ~b~" + character.rp_name() + "~w~.");
            InventoryManager.DeleteInventoryItem(receiverCharacter, typeof(Money), fine);
            receiverCharacter.JailTimeLeft = time * 1000 * 60;
            JailControl(receiver, time);

        }

        [Command("release", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Release a player from prison.", new[] { "The target player ID." })]
        public void release_cmd(Client player, string id)
        {

            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            if ((character.GroupRank < 4 || character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd) && player.GetAccount().AdminLevel < 2)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You don't have permission to use this command.");
                return;
            }

            if (receiverCharacter.IsJailed == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "This player is not jailed.");
                return;
            }

            NAPI.Notification.SendNotificationToPlayer(player, "You have released ~b~" + receiverCharacter.rp_name() + "~w~ from prison.");
            NAPI.Notification.SendNotificationToPlayer(receiver, "You have been released from prison by ~b~" + character.rp_name() + "~w~.");

            if (player.GetAccount().AdminLevel >= 2)
            {
                AdminSystem.AdminCommands.SendtoAllAdmins($"{player.GetAccount().AdminName} has released {receiverCharacter.CharacterName} from prison.");
            }
            SetFree(receiver);

        }

        [Command("givebadge"), Help(HelpManager.CommandGroups.LSPD, "Give a badge to a police officer.", new[] { "The target player ID.", "The badge number being given." })]
        public void givebadge_cmd(Client player, string id, string number)
        {
            var receiver = PlayerManager.ParseClient(id);

            Character character = player.GetCharacter();
            Character receivercharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are not in the LSPD.");
                return;
            }

            if (character.GroupRank < 5)
            {
                player.SendChatMessage("You must be rank 5+ to use this command.");
                return;
            }

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (NAPI.Entity.GetEntityPosition(player).DistanceToSquared(NAPI.Entity.GetEntityPosition(receiver)) > 16f)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            if (receivercharacter.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                player.SendChatMessage("This player must be a member of the LSPD to own a badge.");
                return;
            }

            receivercharacter.BadgeNumber = number;
            player.SendChatMessage($"You have handed badge #{number} to {receivercharacter.rp_name()}");
            player.SendChatMessage($"You have received badge #{number} from {character.rp_name()}");
            receivercharacter.Save();
        }

        [Command("showbadge"), Help(HelpManager.CommandGroups.LSPD, "Show our police badge to a player.", new[] { "The target player ID." })]
        public void showbadge_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);

            Character character = player.GetCharacter();
            Character receivercharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are not in the LSPD.");
                return;
            }

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (NAPI.Entity.GetEntityPosition(player).DistanceToSquared(NAPI.Entity.GetEntityPosition(receiver)) > 16f)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            ChatManager.RoleplayMessage(character, $"shows their badge to {receivercharacter.rp_name()}", ChatManager.RoleplayMe);
            receiver.SendChatMessage($"~h~Badge:~h~ #{character.BadgeNumber} | ~h~Officer:~h~ {character.rp_name()}");
        }

        [Command("cuff", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Handcuff a player.", new[] { "The target player ID." })]
        public void cuff_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);

            Character character = player.GetCharacter();
            Character receivercharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are not in the LSPD.");
                return;
            }

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receiver == player)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You can't cuff yourself!");
                return;
            }

            if (NAPI.Entity.GetEntityPosition(player).DistanceToSquared(NAPI.Entity.GetEntityPosition(receiver)) > 16f)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            var isStunned = API.FetchNativeFromPlayer<bool>(receiver, Hash.IS_PED_BEING_STUNNED, receiver.Handle, 0);

            if (receivercharacter.AreHandsUp == false && isStunned == false)
            {
                player.SendChatMessage("Players must have their hands up or must be tazed before they can be cuffed.");
                return;
            }

            API.GivePlayerWeapon(player, WeaponHash.Unarmed, 1);
            API.SendNativeToAllPlayers(Hash.SET_ENABLE_HANDCUFFS, receivercharacter, true);
            receivercharacter.IsCuffed = true;
            API.PlayPlayerAnimation(receiver, (1 << 0 | 1 << 4 | 1 << 5), "mp_arresting", "idle");
            NAPI.Player.FreezePlayer(receiver, true);
            ChatManager.RoleplayMessage(player, "places handcuffs onto " + receivercharacter.rp_name(), ChatManager.RoleplayMe);
        }


        [Command("uncuff", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Remove handcuffs from a player.", new[] { "The target player ID." })]
        public void uncuff_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);

            Character character = player.GetCharacter();
            Character receivercharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are not in the LSPD.");
                return;
            }

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter == character)
            {
                NAPI.Chat.SendChatMessageToPlayer(player,"You're not allowed to uncuff yourself.");
                return;
            }
           
            if (NAPI.Entity.GetEntityPosition(player).DistanceToSquared(NAPI.Entity.GetEntityPosition(receiver)) > 16f)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            if (!receivercharacter.IsCuffed)
            {
                player.SendChatMessage("This player is not handcuffed.");
                return;
            }

            API.SendNativeToAllPlayers(Hash.SET_ENABLE_HANDCUFFS, receivercharacter, false);
            receivercharacter.IsCuffed = false;
            NAPI.Player.FreezePlayer(receiver, false);
            ChatManager.RoleplayMessage(player, "removes handcuffs from " + receivercharacter.rp_name(), ChatManager.RoleplayMe);
        }

        [Command("frisk", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD | HelpManager.CommandGroups.General, "Frisk a player to show their inventory items.", new[] { "The target player ID." })]
        public void frisk_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receivercharacter = receiver.GetCharacter();
                       
            if (receiver == player)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You can't frisk yourself!");
                return;
            }

            if (NAPI.Entity.GetEntityPosition(player).DistanceToSquared(NAPI.Entity.GetEntityPosition(receiver)) > 16f)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }
            
            if (receivercharacter.AreHandsUp == true || receivercharacter.IsCuffed == true)
            {
            
                ChatManager.RoleplayMessage(character, "pats down " + receivercharacter.rp_name() + " searching through their items.",ChatManager.RoleplayMe);

                NAPI.Chat.SendChatMessageToPlayer(player, "-------------PLAYER INVENTORY-------------");
                foreach (var item in receivercharacter.Inventory)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, $"* ~r~{item.LongName}~w~[{item.CommandFriendlyName}] ({item.Amount})");
                }
                NAPI.Chat.SendChatMessageToPlayer(player, "-------------PLAYER INVENTORY-------------");
                return;
            }
            NAPI.Chat.SendChatMessageToPlayer(player, "Players must be cuffed or have their hands up before you can frisk them.");

        }

        [Command("beacon", Alias = "bc", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Deploy a backup beacon.", null)]
        public void backupbeacon_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            NAPI.Notification.SendNotificationToPlayer(player, "~b~Backup beacon deployed~w~. Available officers have been notified.");
            GroupManager.SendGroupMessage(player, character.rp_name() + " has deployed a backup beacon. Use /acceptbeacon to accept.");

            foreach(var c in PlayerManager.Players) { c.BeaconSet = false; }
            character.BeaconSet = true;
            character.BeaconResetTimer = new Timer { Interval = 60000 };
            character.BeaconResetTimer.Elapsed += delegate { ResetBeacon(player); };
            character.BeaconResetTimer.Start();

        }

        [Command("acceptbeacon", Alias = "ab", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Accept the recent backup beacon.", null)]
        public void acceptbeacon_cmd(Client player)
        {
            Character character = player.GetCharacter();
            var beaconCreator = player;

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            foreach (var c in PlayerManager.Players)
            {
                int i = 0;
                if (c.BeaconSet == true)
                {
                    beaconCreator = c.Client;
                    NAPI.Notification.SendNotificationToPlayer(player, "~b~Beacon accepted~w~. A waypoint has been added to your map.");
                    NAPI.ClientEvent.TriggerClientEvent(player, "update_beacon", NAPI.Entity.GetEntityPosition(beaconCreator));
                    return;
                }
                i++;
                if (i == PlayerManager.Players.Count())
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "There are no active beacons.");
                    return;
                }
            }
        }

        [Command("megaphonetoggle", Alias = "mp", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Toggle the megaphone.", null)]
        public void megaphonetog_cmd(Client player)
        {
            var playerPos = NAPI.Entity.GetEntityPosition(player);
            Character character = player.GetCharacter();

            if (character?.Group == Group.None || character?.Group.CommandType != Group.CommandTypeLspd)
            {
                //NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }
            if (NAPI.Data.GetEntityData(player, "MegaphoneStatus") != true)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "You are speaking through a megaphone", true);
                NAPI.Data.SetEntityData(player, "MegaphoneStatus", true);
                character.MegaPhoneObject = API.CreateObject(API.GetHashKey("prop_megaphone_01"), playerPos, new Vector3());
                API.AttachEntityToEntity(character.MegaPhoneObject, player, "IK_R_Hand", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                return;
            }
            NAPI.Notification.SendNotificationToPlayer(player, "You are no longer speaking through a megaphone.");
            NAPI.Data.SetEntityData(player, "MegaphoneStatus", false);
            if(character.MegaPhoneObject != null && API.DoesEntityExist(character.MegaPhoneObject))
                API.DeleteEntity(character.MegaPhoneObject);
            character.MegaPhoneObject = null;
        }


        [Command("locker"), Help(HelpManager.CommandGroups.LSPD, "View the locker menu.", null)]
        public void locker_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (player.Position.DistanceTo(character.Group.Locker.Location) > 8)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You are not in the LSPD locker room.");
                return;
            }

            NAPI.ClientEvent.TriggerClientEvent(player, "show_lspd_locker");
        }


        [Command("ticket"), Help(HelpManager.CommandGroups.LSPD, "Give a player a ticket with a fine.", new[] { "The target player ID.", "Fine amount." })]
        public void ticket_cmd(Client player, string id, int amount)
        {
            var target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = target.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (NAPI.Entity.GetEntityPosition(player).DistanceToSquared(NAPI.Entity.GetEntityPosition(target)) > 16f)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            NAPI.Chat.SendChatMessageToPlayer(target, character.rp_name() + " is offering to hand you a ticket. Use /acceptcopticket to accept it.");
            NAPI.Chat.SendChatMessageToPlayer(player, "You offer to hand " + receiverCharacter.rp_name() + " a ticket.");
            receiverCharacter.SentTicketAmount = amount;
            receiverCharacter.SentTicket = true;
            character.TicketTimer = new Timer { Interval = 10000 };
            character.TicketTimer.Elapsed += delegate { ResetTicket(target); };
            character.TicketTimer.Start();

        }

        [Command("unpaidtickets", GreedyArg = false), Help(HelpManager.CommandGroups.General | HelpManager.CommandGroups.LSPD, "Check your own, or another player's unpaid tickets as a cop.", new[] { "The target player ID." })]
        public void unpaidtickets_cmd(Client player, string id = null)
        {
            var target = PlayerManager.ParseClient(id);
            Character character = player.GetCharacter();

            if (target == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You have ~b~ " + character.UnpaidTickets + "~w~ unpaid tickets.");
                return;
            }

            Character receiverCharacter = target.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            NAPI.Chat.SendChatMessageToPlayer(player, receiverCharacter.rp_name() + " has ~b~ " + receiverCharacter.UnpaidTickets + "~w~ unpaid tickets.");
        }

        [Command("acceptcopticket", GreedyArg = true), Help(HelpManager.CommandGroups.General, "Accept the cop ticket.", null)]
        public void ticketaccept_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.SentTicket == true)
            {
                character.SentTicket = false;
                character.TicketBalance += character.SentTicketAmount;
                character.UnpaidTickets += 1;
                character.Save();
                NAPI.Chat.SendChatMessageToPlayer(player, "Ticket accepted. Pay for this ticket at the main desk of the police station.");
                ChatManager.RoleplayMessage(character, "has accepted a cop ticket", ChatManager.RoleplayMe);

            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "There are no active tickets to accept.");
            }

        }

        [Command("paycoptickets", GreedyArg = true), Help(HelpManager.CommandGroups.General, "Pay all current cop tickets.", null)]
        public void paytickets_cmd(Client player)
        {
            Character character = player.GetCharacter();

            foreach (Group group in GroupManager.Groups)
            {
                if (group.CommandType == Group.CommandTypeLspd)
                {
                    if (player.Position.DistanceTo(group.FrontDesk.Location) > 5)
                    {
                        NAPI.Notification.SendNotificationToPlayer(player, "~r~You are not at the front desk of the police station.");
                        return;
                    }
                }
            }

            if (character.UnpaidTickets == 0)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You have no active tickets to pay for.");
                return;
            }

            if (character.TicketBalance > Money.GetCharacterMoney(character))
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~You cannot afford to pay for your tickets.");
                return;
            }

            NAPI.Notification.SendNotificationToPlayer(player, "~r~Congratulations! Your tickets have been paid off.");
            InventoryManager.DeleteInventoryItem(character, typeof(Money), character.TicketBalance);
            character.Save();
            character.UnpaidTickets = 0;
        }

        [Command("deploy", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Deploy an LSPD object.", new[] { "The target object ID" })]
        public void deploy_cmd(Client player, string objectid)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            GroupManager.GroupCommandPermCheck(character, 4);

            if (int.Parse(objectid) > 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Deployable items are between 1-5");
            }
            var playerpos = NAPI.Entity.GetEntityPosition(player);
            var playerrot = API.GetEntityRotation(player);
            var playerDimension = API.GetEntityDimension(player);

            switch (objectid)
            {
                case "1":
                    {
                        var item = API.CreateObject((int)API.GetHashKey("prop_mp_barrier_01"), playerpos - new Vector3(0, 0, 1f), new Quaternion(playerrot.X, playerrot.Y, playerrot.Z, 0), playerDimension);
                        Objects.AddLast(item);
                        break;
                    }

                case "2":
                    {
                        var item = API.CreateObject((int)API.GetHashKey("prop_barrier_wat_03b"), playerpos - new Vector3(0, 0, 1f), new Quaternion(playerrot.X, playerrot.Y, playerrot.Z, 0), playerDimension);
                        Objects.AddLast(item);
                        break;
                    }
                case "3":
                    {
                        var item = API.CreateObject((int)API.GetHashKey("prop_barrier_work04a"), playerpos - new Vector3(0, 0, 1f), new Quaternion(playerrot.X, playerrot.Y, playerrot.Z, 0), playerDimension);
                        Objects.AddLast(item);
                        break;
                    }
                case "4":
                    {
                        var item = API.CreateObject((int)API.GetHashKey("prop_mp_conc_barrier_01"), playerpos - new Vector3(0, 0, 1f), new Quaternion(playerrot.X, playerrot.Y, playerrot.Z, 0), playerDimension);
                        Objects.AddLast(item);
                        break;
                    }
                case "5":
                    {
                        var item = API.CreateObject((int)API.GetHashKey("prop_barrier_work05"), playerpos - new Vector3(0, 0, 1f), new Quaternion(playerrot.X, playerrot.Y, playerrot.Z, 0), playerDimension);
                        Objects.AddLast(item);
                        break;
                    }
            }
            NAPI.Notification.SendNotificationToPlayer(player, "Object placed. There are now ~r~" + Objects.Count + "~w~ objects placed.");
        }


        [Command("removelastobject", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Remove the last placed object.", null)]
        public void removeobject_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (Objects.Count() == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "There are no more objects to remove.");
                return;
            }

            API.DeleteEntity(Objects.Last());
            Objects.RemoveLast();
            NAPI.Notification.SendNotificationToPlayer(player, "Object removed. There are now ~r~" + Objects.Count + "~w~ placed.");
        }

        [Command("removenearestobject")]
        public void command_removenearestobject(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (Objects.Count() == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "There are no more objects to remove.");
                return;
            }

            int id = -1;
            float distance = -1.0f;
            Vector3 playerPos = NAPI.Entity.GetEntityPosition(player.Handle);
            Object currentObject = null;
            int cid = 0;

            foreach(Object o in Objects)
            {
                Vector3 objectPos = NAPI.Entity.GetEntityPosition(o.Handle);
                if(objectPos.DistanceTo(playerPos) < distance || distance == -1.0f)
                {
                    id = cid;
                    distance = objectPos.DistanceTo(playerPos);
                    currentObject = o;
                }
                cid++;
            }
            if(id != -1)
            {
                if(NAPI.Entity.GetEntityPosition(currentObject).DistanceTo(playerPos) <= 3.0f)
                {
                    API.DeleteEntity(currentObject.Handle);
                    Objects.Remove(currentObject);
                    NAPI.Notification.SendNotificationToPlayer(player, "Object removed. There are now ~r~" + Objects.Count + "~w~ placed.");
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "You aren't in range of any deployed objects.");
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "There aren't any objects for you to remove.");
            }
        }

        [Command("removeallobjects", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Remove all placed LSPD objects.", null)]
        public void removeallobjects_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (Objects.Count() == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "There are no more objects to remove.");
                return;
            }

            int len = Objects.Count();

            foreach (var i in Objects)
            {
                API.DeleteEntity(i);
            }

            var node = Objects.First;

            int j = 0;
            while (j <= len)
            {
                Objects.RemoveLast();
                j++;
            }

            NAPI.Notification.SendNotificationToPlayer(player, "~r~" + len + " objects removed.");
        }

        [Command("setlockerpos"), Help(HelpManager.CommandGroups.LSPD, "Set the locker position.", null)]
        public void setlockerpos_cmd(Client player)
        {
            Character character = player.GetCharacter();

            GroupManager.GroupCommandPermCheck(character, 10);

            if (character.Group.Type != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "Only the LSPD may use this command.");
                return;
            }

            if (character.Group.Locker == MarkerZone.None)
            {
                character.Group.Locker = new MarkerZone(character.Client.Position, character.Client.Rotation,
                    (int)character.Client.Dimension)
                { TextLabelText = "LSPD Locker Room~n~/locker" };
                character.Group.Save();
                character.Group.Locker.Create();
            }
            else
            {
                character.Group.Locker.Location = character.Client.Position;
                character.Group.Locker.Rotation = character.Client.Rotation;
                character.Group.Locker.Dimension = (int)character.Client.Dimension;
                character.Group.Locker.TextLabelText = "LSPD Locker Room~n~/locker";
                character.Group.Locker.Refresh();
                character.Group.Save();
            }

            NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You have moved the LSPD locker location.");
            return;
        }

        [Command("setfrontdeskpos"), Help(HelpManager.CommandGroups.LSPD, "Se the front desk position.", null)]
        public void setfrontdeskpos_cmd(Client player)
        {
            Character character = player.GetCharacter();

            GroupManager.GroupCommandPermCheck(character, 10);

            if (character.Group.Type != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "Only the LSPD may use this command.");
                return;
            }

            if (character.Group.FrontDesk == MarkerZone.None)
            {
                character.Group.FrontDesk = new MarkerZone(character.Client.Position, character.Client.Rotation,
                    (int)character.Client.Dimension)
                { TextLabelText = "LSPD Front Desk~n~/paycoptickets" };
                character.Group.Save();
                character.Group.FrontDesk.Create();
            }
            else
            {
                character.Group.FrontDesk.Location = character.Client.Position;
                character.Group.FrontDesk.Rotation = character.Client.Rotation;
                character.Group.FrontDesk.Dimension = (int)character.Client.Dimension;
                character.Group.FrontDesk.TextLabelText = "LSPD Front Desk~n~/paycoptickets";
                character.Group.FrontDesk.Refresh();
                character.Group.Save();
            }

            NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You have moved the LSPD front desk location.");
            return;
        }

        [Command("setarrestpos"), Help(HelpManager.CommandGroups.LSPD, "Set the arrest location.", null)]
        public void setarrestpos_cmd(Client player)

        {
            Character character = player.GetCharacter();

            GroupManager.GroupCommandPermCheck(character, 10);

            if (character.Group.Type != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "Only the LSPD may use this command.");
                return;
            }

            if (character.Group.ArrestLocation == MarkerZone.None)
            {
                character.Group.ArrestLocation = new MarkerZone(character.Client.Position, character.Client.Rotation,
                        (int)character.Client.Dimension)
                { TextLabelText = "Arrest Location~n~/arrest" };

                character.Group.ArrestLocation.Create();
            }
            else
            {
                character.Group.ArrestLocation.Location = character.Client.Position;
                character.Group.ArrestLocation.Rotation = character.Client.Rotation;
                character.Group.ArrestLocation.Dimension = (int)character.Client.Dimension;
                character.Group.ArrestLocation.TextLabelText = "Arrest Location~n~/arrest";
                character.Group.ArrestLocation.Refresh();
            }

            NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You have moved the LSPD arrest location.");
        }

        public void GiveLspdEquipment(Client player, int type = 0)
        {
            WeaponManager.RemoveAllPlayerWeapons(player);
            Character character = player.GetCharacter();
            switch (type)
            {
                case 0:
                    WeaponManager.CreateWeapon(player, WeaponHash.StunGun, WeaponTint.Normal, false, false, true);
                    WeaponManager.CreateWeapon(player, WeaponHash.Nightstick, WeaponTint.Normal, false, false, true);
                    WeaponManager.CreateWeapon(player, WeaponHash.CombatPistol, WeaponTint.Lspd, false, false, true);
                    WeaponManager.CreateWeapon(player, WeaponHash.Flashlight, WeaponTint.Normal, false, false, true);
                    break;
                case 1:
                    WeaponManager.CreateWeapon(player, WeaponHash.CombatPistol, WeaponTint.Lspd, false, false, true);
                    WeaponManager.CreateWeapon(player, WeaponHash.CombatPDW, WeaponTint.Lspd, false, false, true);
                    WeaponManager.CreateWeapon(player, WeaponHash.SmokeGrenade, WeaponTint.Normal, false, false, true);
                    WeaponManager.CreateWeapon(player, WeaponHash.BZGas, WeaponTint.Normal, false, false, true);
                    break;
            }
            API.SetPlayerHealth(player, 100);
            API.SetPlayerArmor(player, 100);
        }

        public void ResetTicket(Client player)
        {
            Character character = player.GetCharacter();
            character.SentTicket = false;
            character.TicketTimer.Stop();
        }


        public void ResetBeacon(Client player)
        {
            Character character = player.GetCharacter();
            character.BeaconSet = false;
            character.BeaconResetTimer.Stop();
        }

        public static void SendToCops(Client player, string message)
        {
            foreach(var p in PlayerManager.Players)
            {
                if (p.Group.CommandType == Group.CommandTypeLspd)
                {
                    p.Client.SendChatMessage(message);
                }
            }
        }
        public static void JailControl(Client player, int seconds)
        {
            Character character = player.GetCharacter();

            int jailOnePlayers = API.Shared.GetPlayersInRadiusOfPosition(3.7f, JailOne).Count;
            int jailTwoPlayers = API.Shared.GetPlayersInRadiusOfPosition(3.7f, JailTwo).Count;
            int jailThreePlayers = API.Shared.GetPlayersInRadiusOfPosition(3.7f, JailThree).Count;
            int smallest = API.Shared.GetAllPlayers().SkipWhile(x =>  x== null).Count();
            int chosenCell = 0;
            List<int> list = new List<int>(new int[] { jailOnePlayers, jailTwoPlayers, jailThreePlayers });


            //Choose correct cell for player
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] < smallest)
                {
                    smallest = list[i];
                    chosenCell = i;
                }
            }

            if (chosenCell == 0)
                API.Shared.SetEntityPosition(player, JailOne);
            else if (chosenCell == 1)
                API.Shared.SetEntityPosition(player, JailTwo);
            else
                API.Shared.SetEntityPosition(player, JailThree);

            WeaponManager.RemoveAllPlayerWeapons(player);
            character.IsJailed = true;

            API.Shared.SendChatMessageToPlayer(player, "You have been placed in jail for " + character.JailTimeLeft/60/1000 + " minutes.");

            character.JailTimeLeftTimer = new Timer { Interval = 1000 };
            character.JailTimeLeftTimer.Elapsed += delegate { UpdateTimer(player); };
            character.JailTimeLeftTimer.Start();
            character.JailTimer = new Timer { Interval = character.JailTimeLeft };
            character.JailTimer.Elapsed += delegate { SetFree(player); };
            character.JailTimer.Start();
        }

        public static void UpdateTimer(Client player)
        {
            Character character = player.GetCharacter();
            character.JailTimeLeft -= 1000;
        }

        public static void SetFree(Client player)
        {
            Character character = player.GetCharacter();
            if (character.IsJailed == false)
            {
                return;
            }
            character.JailTimeLeft = 0;
            API.Shared.SendChatMessageToPlayer(player, "~b~You are free to go.");
            character.IsJailed = false;
            API.Shared.SetEntityPosition(player, FreeJail);
            character.JailTimer.Stop();
            character.JailTimeLeftTimer.Stop();
            API.Shared.SetEntityDimension(character.Client, 0);

        }
    }
}

