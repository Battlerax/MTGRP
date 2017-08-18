using System.Collections.Generic;
using System.Linq;
using System.Timers;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;


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
            API.onResourceStart += StartLspd;
            API.onClientEventTrigger += API_onClientEventTrigger;
            API.onPlayerDisconnected += API_onPlayerDisconnected;
        }

        private void API_onPlayerDisconnected(Client player, string reason)
        {
            var character = player.GetCharacter();
            if (character == null) return;

            if (character.MegaPhoneObject != null && API.doesEntityExist(character.MegaPhoneObject))
                API.deleteEntity(character.MegaPhoneObject);
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


        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "LSPD_Menu_Change_Clothes":
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

                        break;
                    }
                case "LSPD_Menu_Toggle_Duty":
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

                        break;
                    }
                case "LSPD_Menu_Equip_Standard_Equipment":
                    {
                        Character c = player.GetCharacter();

                        if (c.Group.CommandType != Group.CommandTypeLspd)
                        {
                            return;
                        }

                        GiveLspdEquipment(player);
                        API.sendChatMessageToPlayer(player, Color.White, "You have been given the standard LSPD equipment.");

                        break;
                    }
                case "LSPD_Menu_Equip_SWAT_Equipment":
                    {
                        Character c = player.GetCharacter();

                        if (c.Group.CommandType != Group.CommandTypeLspd)
                        {
                            return;
                        }

                        GiveLspdEquipment(player, 1);
                        API.sendChatMessageToPlayer(player, Color.White, "You have been given the standard SWAT equipment.");
                        break;
                    }
            }
        }
        [Command("recordcrime", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Record a player's crime, adding them to the wanted list.", new[] { "The target player ID.", "The crime ID" })]
        public void recordcrimes_cmd(Client player, string id, string type, string crimename, string jailTime, string fine)
        {
            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            receiverCharacter.RecordCrime(character.CharacterName, new Crime(type, crimename, int.Parse(jailTime), int.Parse(fine)));
            API.sendNotificationToPlayer(player, "You have recorded " + receiverCharacter.rp_name() + " for committing: " + crimename);
            API.sendNotificationToPlayer(receiver, character.rp_name() + " has recorded a crime you committed: ~r~" + crimename + "~w~.");
        }

        /*
        [Command("recordcrime", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Record a player's crime, adding them to the wanted list.", new[] { "The target player ID.", "The crime ID" })]
        public void recordcrimes_cmd(Client player, string id, string crimeid)
        {
            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            var crime = Crime.Crimes.Find(c => c.Id == int.Parse(crimeid));

            if (crime == null)
            {
                API.sendChatMessageToPlayer(player, Color.White, "No crime with that ID exists.");
                return;
            }

            receiverCharacter.RecordCrime(character.CharacterName, crime);
            API.sendNotificationToPlayer(player, "You have recorded " + receiverCharacter.CharacterName + " for committing: " + crime.Name);
            API.sendNotificationToPlayer(receiver, character.CharacterName + " has recorded a crime you committed: ~r~" + crime.Name + "~w~.");
        }
        */

        [Command("showcriminalrecord", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Show the criminal record of a player.", new[] { "The target player ID." })]
        public void criminalrecord_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            API.sendChatMessageToPlayer(player, "=======CRIMIMNAL RECORD FOR " + receiverCharacter.rp_name() + "=======");
            foreach (var i in receiverCharacter.GetCriminalRecord())
            {
                if(i.ActiveCrime == true)
                {
                    API.sendChatMessageToPlayer(player, "~r~[ACTIVE] Type: " + i.Crime.Type + " Crime: " + i.Crime.Name + " Date issued: " + i.DateTime + " Recording officer: " + i.OfficerName);
                }

                else
                {
                    API.sendChatMessageToPlayer(player, "Type: " + i.Crime.Type + "Crime: " + i.Crime.Name + " Date issued: " + i.DateTime + " Recording officer: " + i.OfficerName);
                }
            }
        }

        [Command("listcrimes"), Help(HelpManager.CommandGroups.LSPD, "List the available crimes and their crime ID", null)]
        public void listcrimes_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            player.sendChatMessage("======================");
            player.sendChatMessage("CRIME LIST");
            player.sendChatMessage("======================");
            int f = 0;
            foreach (var i in Crime.Crimes)
            {
                API.sendChatMessageToPlayer(player, f + " | " + i.Type + " | " + i.Name + " | " + i.JailTime + " | " + i.Fine); //TODO: REPLACE WITH A MENU
                f++;
            }
        }

        [Command("createcrime", GreedyArg=true), Help(HelpManager.CommandGroups.LSPD, "Create a crime and add it to the crime list.", "The type of the crime", "The jail time in seconds", "The fine amount", "Crime name")]
        public void createcrime_cmd(Client player, string type, int jailTime, int fine, string crimeName)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }


            if (Crime.CrimeExists(crimeName))
            {
                API.sendChatMessageToPlayer(player, "This crime already exists!");
                return;
            }

            Crime.InsertCrime(type, crimeName, jailTime, fine);
            API.sendChatMessageToPlayer(player, "Crime created and added to crime list.");
        }

        [Command("editcrime"), Help(HelpManager.CommandGroups.LSPD, "Edit a crime.", new[] { "Crime ID", "The crime type", "The crime name", "The jail time", "The fine for this crime" })]
        public void editcrime_cmd(Client player, int id, string type, string crimeName, int jailTime, int fine)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (Crime.CrimeExists(crimeName))
            {
                API.sendChatMessageToPlayer(player, "This crime already exists!");
                return;
            }

            Crime crime = Crime.Crimes[id];
            Crime.Crimes[id] = new Crime(type, crimeName, jailTime, fine);
            Crime.UpdateCrimes();
            API.sendChatMessageToPlayer(player, "Crime edited.");
        }

        [Command("deletecrime"), Help(HelpManager.CommandGroups.LSPD, "Delete a crime by its ID.", new[] { "The target crime ID." })]
        public void deletecrime_cmd(Client player, int id)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (!(id < Crime.Crimes.Count))
            {
                player.sendChatMessage("That crime does not exist.");
                return;
            }

            Crime crimeDelete = Crime.Crimes[id];


            crimeDelete.Delete();
            API.sendChatMessageToPlayer(player, "Crime deleted from crime list.");
        }

        [Command("wanted", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Show the wanted list.", null)]
        public void wanted_cmd(Client player)
        {
            Character character = player.GetCharacter();
            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            var players = API.getAllPlayers();

            API.sendChatMessageToPlayer(player, "---------------------WANTED LIST---------------------");


            foreach (var c in PlayerManager.Players)
            {
                if (c.HasActiveCriminalRecord() > 0)
                {
                    API.sendChatMessageToPlayer(player, c.rp_name() + " is wanted with " + c.HasActiveCriminalRecord() + " crimes.");
                }
            }
        }


        [Command("arrest", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Arrest a player after their crimes are recorded.", new[] { "The target player ID." })]
        public void arrest_cmd(Client player, string id)
        {

            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (receiver == player)
            {
                API.sendNotificationToPlayer(player, "~r~You can't arrest yourself!");
                return;
            }

            if (receiverCharacter.HasActiveCriminalRecord() == 0)
            {
                API.sendChatMessageToPlayer(player, "This player has no active crimes.");
            }
            
            if (character.GroupId == receiverCharacter.GroupId)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You cannot arrest a member of the LSPD.");
                return;
            }

            if (player.position.DistanceTo(character.Group.ArrestLocation.Location) > 4)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You are not at the arrest location.");
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

            API.sendNativeToAllPlayers(Hash.SET_ENABLE_HANDCUFFS, receiverCharacter.Client.handle, false);
            receiverCharacter.IsCuffed = false;
            API.stopPlayerAnimation(receiverCharacter.Client);

            API.sendNotificationToPlayer(player, "You have arrested ~b~" + receiverCharacter.rp_name() + "~w~.");
            API.sendNotificationToPlayer(receiver, "You have been arrested by ~b~" + character.rp_name() + "~w~.");
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
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            if ((character.GroupRank < 4 || character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd) && player.GetAccount().AdminLevel < 2)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You don't have permission to use this command.");
                return;
            }

            if (receiverCharacter.IsJailed == false)
            {
                API.sendChatMessageToPlayer(player, "This player is not jailed.");
                return;
            }

            API.sendNotificationToPlayer(player, "You have released ~b~" + receiverCharacter.rp_name() + "~w~ from prison.");
            API.sendNotificationToPlayer(receiver, "You have been released from prison by ~b~" + character.rp_name() + "~w~.");

            if (player.GetAccount().AdminLevel >= 2)
            {
                AdminSystem.AdminCommands.SendtoAllAdmins($"{player.GetAccount().AdminName} has relased {receiverCharacter.CharacterName} from prison.");
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
                API.sendChatMessageToPlayer(player, "You are not in the LSPD.");
                return;
            }

            if (character.GroupRank < 5)
            {
                player.sendChatMessage("You must be rank 5+ to use this command.");
                return;
            }

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(receiver)) > 16f)
            {
                API.sendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            if (receivercharacter.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                player.sendChatMessage("This player must be a member of the LSPD to own a badge.");
                return;
            }

            receivercharacter.BadgeNumber = number;
            player.sendChatMessage($"You have handed badge #{number} to {receivercharacter.rp_name()}");
            player.sendChatMessage($"You have received badge #{number} from {character.rp_name()}");
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
                API.sendChatMessageToPlayer(player, "You are not in the LSPD.");
                return;
            }

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(receiver)) > 16f)
            {
                API.sendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            ChatManager.RoleplayMessage(character, $"shows their badge to {receivercharacter.rp_name()}", ChatManager.RoleplayMe);
            receiver.sendChatMessage($"~h~Badge:~h~ #{character.BadgeNumber} | ~h~Officer:~h~ {character.rp_name()}");
        }

        [Command("cuff", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Handcuff a player.", new[] { "The target player ID." })]
        public void cuff_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);

            Character character = player.GetCharacter();
            Character receivercharacter = receiver.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, "You are not in the LSPD.");
                return;
            }

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receiver == player)
            {
                API.sendNotificationToPlayer(player, "~r~You can't cuff yourself!");
                return;
            }

            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(receiver)) > 16f)
            {
                API.sendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            var isStunned = API.fetchNativeFromPlayer<bool>(receiver, Hash.IS_PED_BEING_STUNNED, receiver.handle, 0);

            if (receivercharacter.AreHandsUp == false && isStunned == false)
            {
                player.sendChatMessage("Players must have their hands up or must be tazed before they can be cuffed.");
                return;
            }

            API.givePlayerWeapon(player, WeaponHash.Unarmed, 1, true, true);
            API.sendNativeToAllPlayers(Hash.SET_ENABLE_HANDCUFFS, receivercharacter, true);
            receivercharacter.IsCuffed = true;
            API.playPlayerAnimation(receiver, (1 << 0 | 1 << 4 | 1 << 5), "mp_arresting", "idle");
            API.freezePlayer(receiver, true);
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
                API.sendChatMessageToPlayer(player, "You are not in the LSPD.");
                return;
            }

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter == character)
            {
                API.sendChatMessageToPlayer(player,"You're not allowed to uncuff yourself.");
                return;
            }
           
            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(receiver)) > 16f)
            {
                API.sendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            if (!receivercharacter.IsCuffed)
            {
                player.sendChatMessage("This player is not handcuffed.");
                return;
            }

            API.sendNativeToAllPlayers(Hash.SET_ENABLE_HANDCUFFS, receivercharacter, false);
            receivercharacter.IsCuffed = false;
            API.freezePlayer(receiver, false);
            ChatManager.RoleplayMessage(player, "removes handcuffs from " + receivercharacter.rp_name(), ChatManager.RoleplayMe);
        }

        [Command("frisk", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD | HelpManager.CommandGroups.General, "Frisk a player to show their inventory items.", new[] { "The target player ID." })]
        public void frisk_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = player.GetCharacter();
            Character receivercharacter = receiver.GetCharacter();
                       
            if (receiver == player)
            {
                API.sendNotificationToPlayer(player, "~r~You can't frisk yourself!");
                return;
            }

            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(receiver)) > 16f)
            {
                API.sendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }
            
            if (receivercharacter.AreHandsUp == true || receivercharacter.IsCuffed == true)
            {
            
                ChatManager.RoleplayMessage(character, "pats down " + receivercharacter.rp_name() + " searching through their items.",ChatManager.RoleplayMe);

                API.sendChatMessageToPlayer(player, "-------------PLAYER INVENTORY-------------");
                foreach (var item in receivercharacter.Inventory)
                {
                    API.sendChatMessageToPlayer(player, $"* ~r~{item.LongName}~w~[{item.CommandFriendlyName}] ({item.Amount})");
                }
                API.sendChatMessageToPlayer(player, "-------------PLAYER INVENTORY-------------");
                return;
            }
            API.sendChatMessageToPlayer(player, "Players must be cuffed or have their hands up before you can frisk them.");

        }

        [Command("beacon", Alias = "bc", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Deploy a backup beacon.", null)]
        public void backupbeacon_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            API.sendNotificationToPlayer(player, "~b~Backup beacon deployed~w~. Available officers have been notified.");
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
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            foreach (var c in PlayerManager.Players)
            {
                int i = 0;
                if (c.BeaconSet == true)
                {
                    beaconCreator = c.Client;
                    API.sendNotificationToPlayer(player, "~b~Beacon accepted~w~. A waypoint has been added to your map.");
                    API.triggerClientEvent(player, "update_beacon", API.getEntityPosition(beaconCreator));
                    return;
                }
                i++;
                if (i == PlayerManager.Players.Count())
                {
                    API.sendChatMessageToPlayer(player, "There are no active beacons.");
                    return;
                }
            }
        }

        [Command("megaphonetoggle", Alias = "mp", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Toggle the megaphone.", null)]
        public void megaphonetog_cmd(Client player)
        {
            var playerPos = API.getEntityPosition(player);
            Character character = player.GetCharacter();

            if (character?.Group == Group.None || character?.Group.CommandType != Group.CommandTypeLspd)
            {
                //API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }
            if (API.getEntityData(player, "MegaphoneStatus") != true)
            {
                API.sendNotificationToPlayer(player, "You are speaking through a megaphone", true);
                API.setEntityData(player, "MegaphoneStatus", true);
                character.MegaPhoneObject = API.createObject(API.getHashKey("prop_megaphone_01"), playerPos, new Vector3());
                API.attachEntityToEntity(character.MegaPhoneObject, player, "IK_R_Hand", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                return;
            }
            API.sendNotificationToPlayer(player, "You are no longer speaking through a megaphone.");
            API.setEntityData(player, "MegaphoneStatus", false);
            if(character.MegaPhoneObject != null && API.doesEntityExist(character.MegaPhoneObject))
                API.deleteEntity(character.MegaPhoneObject);
            character.MegaPhoneObject = null;
        }


        [Command("locker"), Help(HelpManager.CommandGroups.LSPD, "View the locker menu.", null)]
        public void locker_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (player.position.DistanceTo(character.Group.Locker.Location) > 8)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You are not in the LSPD locker room.");
                return;
            }

            API.triggerClientEvent(player, "show_lspd_locker");
        }


        [Command("ticket"), Help(HelpManager.CommandGroups.LSPD, "Give a player a ticket with a fine.", new[] { "The target player ID.", "Fine amount." })]
        public void ticket_cmd(Client player, string id, int amount)
        {
            var target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.sendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }

            Character character = player.GetCharacter();
            Character receiverCharacter = target.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(target)) > 16f)
            {
                API.sendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            API.sendChatMessageToPlayer(target, character.rp_name() + " is offering to hand you a ticket. Use /acceptcopticket to accept it.");
            API.sendChatMessageToPlayer(player, "You offer to hand " + receiverCharacter.rp_name() + " a ticket.");
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
                API.sendChatMessageToPlayer(player, "You have ~b~ " + character.UnpaidTickets + "~w~ unpaid tickets.");
                return;
            }

            Character receiverCharacter = target.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            API.sendChatMessageToPlayer(player, receiverCharacter.rp_name() + " has ~b~ " + receiverCharacter.UnpaidTickets + "~w~ unpaid tickets.");
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
                API.sendChatMessageToPlayer(player, "Ticket accepted. Pay for this ticket at the main desk of the police station.");
                ChatManager.RoleplayMessage(character, "has accepted a cop ticket", ChatManager.RoleplayMe);

            }
            else
            {
                API.sendChatMessageToPlayer(player, Color.White, "There are no active tickets to accept.");
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
                    if (player.position.DistanceTo(group.FrontDesk.Location) > 5)
                    {
                        API.sendNotificationToPlayer(player, "~r~You are not at the front desk of the police station.");
                        return;
                    }
                }
            }

            if (character.UnpaidTickets == 0)
            {
                API.sendNotificationToPlayer(player, "~r~You have no active tickets to pay for.");
                return;
            }

            if (character.TicketBalance > Money.GetCharacterMoney(character))
            {
                API.sendNotificationToPlayer(player, "~r~You cannot afford to pay for your tickets.");
                return;
            }

            API.sendNotificationToPlayer(player, "~r~Congratulations! Your tickets have been paid off.");
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
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            GroupManager.GroupCommandPermCheck(character, 4);

            if (int.Parse(objectid) > 5)
            {
                API.sendChatMessageToPlayer(player, "Deployable items are between 1-5");
            }
            var playerpos = API.getEntityPosition(player);
            var playerrot = API.getEntityRotation(player);
            var playerDimension = API.getEntityDimension(player);

            switch (objectid)
            {
                case "1":
                    {
                        var item = API.createObject(API.getHashKey("prop_mp_barrier_01"), playerpos - new Vector3(0, 0, 1f), playerrot, playerDimension);
                        Objects.AddLast(item);
                        break;
                    }

                case "2":
                    {
                        var item = API.createObject(API.getHashKey("prop_barrier_wat_03b"), playerpos - new Vector3(0, 0, 1f), playerrot, playerDimension);
                        Objects.AddLast(item);
                        break;
                    }
                case "3":
                    {
                        var item = API.createObject(API.getHashKey("prop_barrier_work04a"), playerpos - new Vector3(0, 0, 1f), playerrot, playerDimension);
                        Objects.AddLast(item);
                        break;
                    }
                case "4":
                    {
                        var item = API.createObject(API.getHashKey("prop_mp_conc_barrier_01"), playerpos - new Vector3(0, 0, 1f), playerrot, playerDimension);
                        Objects.AddLast(item);
                        break;
                    }
                case "5":
                    {
                        var item = API.createObject(API.getHashKey("prop_barrier_work05"), playerpos - new Vector3(0, 0, 1f), playerrot, playerDimension);
                        Objects.AddLast(item);
                        break;
                    }
            }
            API.sendNotificationToPlayer(player, "Object placed. There are now ~r~" + Objects.Count + "~w~ objects placed.");
        }


        [Command("removelastobject", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Remove the last placed object.", null)]
        public void removeobject_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (Objects.Count() == 0)
            {
                API.sendChatMessageToPlayer(player, "There are no more objects to remove.");
                return;
            }

            API.deleteEntity(Objects.Last());
            Objects.RemoveLast();
            API.sendNotificationToPlayer(player, "Object removed. There are now ~r~" + Objects.Count + "~w~ placed.");
        }

        [Command("removeallobjects", GreedyArg = true), Help(HelpManager.CommandGroups.LSPD, "Remove all placed LSPD objects.", null)]
        public void removeallobjects_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (Objects.Count() == 0)
            {
                API.sendChatMessageToPlayer(player, "There are no more objects to remove.");
                return;
            }

            int len = Objects.Count();

            foreach (var i in Objects)
            {
                API.deleteEntity(i);
            }

            var node = Objects.First;

            int j = 0;
            while (j <= len)
            {
                Objects.RemoveLast();
                j++;
            }

            API.sendNotificationToPlayer(player, "~r~" + len + " objects removed.");
        }

        [Command("setlockerpos"), Help(HelpManager.CommandGroups.LSPD, "Set the locker position.", null)]
        public void setlockerpos_cmd(Client player)
        {
            Character character = player.GetCharacter();

            GroupManager.GroupCommandPermCheck(character, 10);

            if (character.Group.Type != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "Only the LSPD may use this command.");
                return;
            }

            if (character.Group.Locker == MarkerZone.None)
            {
                character.Group.Locker = new MarkerZone(character.Client.position, character.Client.rotation,
                    character.Client.dimension)
                { TextLabelText = "LSPD Locker Room~n~/locker" };
                character.Group.Save();
                character.Group.Locker.Create();
            }
            else
            {
                character.Group.Locker.Location = character.Client.position;
                character.Group.Locker.Rotation = character.Client.rotation;
                character.Group.Locker.Dimension = character.Client.dimension;
                character.Group.Locker.TextLabelText = "LSPD Locker Room~n~/locker";
                character.Group.Locker.Refresh();
                character.Group.Save();
            }

            API.sendChatMessageToPlayer(player, Color.White, "You have moved the LSPD locker location.");
            return;
        }

        [Command("setfrontdeskpos"), Help(HelpManager.CommandGroups.LSPD, "Se the front desk position.", null)]
        public void setfrontdeskpos_cmd(Client player)
        {
            Character character = player.GetCharacter();

            GroupManager.GroupCommandPermCheck(character, 10);

            if (character.Group.Type != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "Only the LSPD may use this command.");
                return;
            }

            if (character.Group.FrontDesk == MarkerZone.None)
            {
                character.Group.FrontDesk = new MarkerZone(character.Client.position, character.Client.rotation,
                    character.Client.dimension)
                { TextLabelText = "LSPD Front Desk~n~/paycoptickets" };
                character.Group.Save();
                character.Group.FrontDesk.Create();
            }
            else
            {
                character.Group.FrontDesk.Location = character.Client.position;
                character.Group.FrontDesk.Rotation = character.Client.rotation;
                character.Group.FrontDesk.Dimension = character.Client.dimension;
                character.Group.FrontDesk.TextLabelText = "LSPD Front Desk~n~/paycoptickets";
                character.Group.FrontDesk.Refresh();
                character.Group.Save();
            }

            API.sendChatMessageToPlayer(player, Color.White, "You have moved the LSPD front desk location.");
            return;
        }

        [Command("setarrestpos"), Help(HelpManager.CommandGroups.LSPD, "Set the arrest location.", null)]
        public void setarrestpos_cmd(Client player)

        {
            Character character = player.GetCharacter();

            GroupManager.GroupCommandPermCheck(character, 10);

            if (character.Group.Type != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "Only the LSPD may use this command.");
                return;
            }

            if (character.Group.ArrestLocation == MarkerZone.None)
            {
                character.Group.ArrestLocation = new MarkerZone(character.Client.position, character.Client.rotation,
                        character.Client.dimension)
                { TextLabelText = "Arrest Location~n~/arrest" };

                character.Group.ArrestLocation.Create();
            }
            else
            {
                character.Group.ArrestLocation.Location = character.Client.position;
                character.Group.ArrestLocation.Rotation = character.Client.rotation;
                character.Group.ArrestLocation.Dimension = character.Client.dimension;
                character.Group.ArrestLocation.TextLabelText = "Arrest Location~n~/arrest";
                character.Group.ArrestLocation.Refresh();
            }

            API.sendChatMessageToPlayer(player, Color.White, "You have moved the LSPD arrest location.");
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
                    WeaponManager.CreateWeapon(player, WeaponHash.CombatPistol, WeaponTint.LSPD, false, false, true);
                    WeaponManager.CreateWeapon(player, WeaponHash.Flashlight, WeaponTint.Normal, false, false, true);
                    break;
                case 1:
                    WeaponManager.CreateWeapon(player, WeaponHash.CombatPistol, WeaponTint.LSPD, false, false, true);
                    WeaponManager.CreateWeapon(player, WeaponHash.CombatPDW, WeaponTint.LSPD, false, false, true);
                    WeaponManager.CreateWeapon(player, WeaponHash.SmokeGrenade, WeaponTint.Normal, false, false, true);
                    WeaponManager.CreateWeapon(player, WeaponHash.BZGas, WeaponTint.Normal, false, false, true);
                    break;
            }
            API.setPlayerHealth(player, 100);
            API.setPlayerArmor(player, 100);
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
                    p.Client.sendChatMessage(message);
                }
            }
        }
        public static void JailControl(Client player, int seconds)
        {
            Character character = player.GetCharacter();

            int jailOnePlayers = API.shared.getPlayersInRadiusOfPosition(3.7f, JailOne).Count;
            int jailTwoPlayers = API.shared.getPlayersInRadiusOfPosition(3.7f, JailTwo).Count;
            int jailThreePlayers = API.shared.getPlayersInRadiusOfPosition(3.7f, JailThree).Count;
            int smallest = API.shared.getAllPlayers().SkipWhile(x =>  x== null).Count();
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
                API.shared.setEntityPosition(player, JailOne);
            else if (chosenCell == 1)
                API.shared.setEntityPosition(player, JailTwo);
            else
                API.shared.setEntityPosition(player, JailThree);

            WeaponManager.RemoveAllPlayerWeapons(player);
            character.IsJailed = true;

            API.shared.sendChatMessageToPlayer(player, "You have been placed in jail for " + character.JailTimeLeft/60/1000 + " minutes.");

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
            API.shared.sendChatMessageToPlayer(player, "~b~You are free to go.");
            character.IsJailed = false;
            API.shared.setEntityPosition(player, FreeJail);
            character.JailTimer.Stop();
            character.JailTimeLeftTimer.Stop();
            API.shared.setEntityDimension(character.Client, 0);

        }
    }
}

