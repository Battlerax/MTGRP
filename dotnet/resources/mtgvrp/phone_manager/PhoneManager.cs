using System;
using System.Data.SqlTypes;
using System.Linq;

using GTANetworkAPI;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using MongoDB.Driver;
using mtgvrp.core.Help;
using mtgvrp.property_system;

using Color = mtgvrp.core.Color;

namespace mtgvrp.phone_manager
{
    public class PhoneManager : Script
    {
        public PhoneManager()
        {
            DebugManager.DebugMessage("[PhoneM] Initalizing Phone Manager...");

            CharacterMenu.OnCharacterLogin += CharacterMenu_OnCharacterLogin;

            DebugManager.DebugMessage("[PhoneM] Phone Manager initalized.");
        }

        private void CharacterMenu_OnCharacterLogin(object sender, CharacterMenu.CharacterLoginEventArgs e)
        {
            var item = InventoryManager.DoesInventoryHaveItem<Phone>(e.Character);
            if (item.Length > 0)
            {
                //Check if in DB.
                var dbNumber = DatabaseManager.PhoneNumbersTable.Find(x => x.Number == item[0].PhoneNumber);
                if (dbNumber.Count() > 0)
                {
                    //Check if id isn't same.
                    if (dbNumber.First().PhoneId != item[0].Id)
                    {
                        InventoryManager.DeleteInventoryItem<Phone>(e.Character);
                        NAPI.Chat.SendChatMessageToPlayer(e.Character.Player, "Your phone has been removed due to the number being used already, you've been refunded.");
                        InventoryManager.GiveInventoryItem(e.Character, new Money(), 500);
                        e.Character.Save();
                        return;
                    }
                }
                else
                {
                    item[0].InsertNumber();
                }
            }
        }

        [RemoteEvent("phone_callphone")]
        public void PhoneCallPhone(Player sender, params object[] arguments)
        {
            string number = (string)arguments[0];
            call_cmd(sender, number);
        }

        [RemoteEvent("phone_answercall")]
        public void PhoneAnswerCall(Player sender, params object[] arguments)
        {
            pickup_cmd(sender);
        }

        [RemoteEvent("phone_hangout")]
        public void PhoneHangOut(Player sender, params object[] arguments)
        {
            h_cmd(sender);
        }

        [RemoteEvent("phone_getallContacts")]
        public void PhoneGetAllContacts(Player sender, params object[] arguments)
        {
            //Check if has phone.
            var items = InventoryManager.DoesInventoryHaveItem(sender.GetCharacter(), typeof(Phone));
            if (items.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have a phone.");
                return;
            }
            var phone = (Phone)items[0];

            string[][] contacts = phone.Contacts.Select(x => new[] { x.Name, x.Number.ToString() }).ToArray();
            NAPI.ClientEvent.TriggerClientEvent(sender, "phone_showContacts", NAPI.Util.ToJson(contacts));
        }

        [RemoteEvent("phone_saveContact")]
        public void PhoneSaveContact(Player sender, params object[] arguments)
        {
            var name = arguments[0].ToString();
            string num = (string)arguments[1];
            if (IsDigitsOnly(num))
            {
                addcontact_cmd(sender, num, name);
            }
            else
            {
                NAPI.Notification.SendNotificationToPlayer(sender, "Invalid number entered.");
            }
        }

        [RemoteEvent("phone_editContact")]
        public void PhoneEditContact(Player sender, params object[] arguments)
        {
            string anum = (string)arguments[2];
            if (IsDigitsOnly(anum))
            {
                editcontact_cmd(sender, (string)arguments[0], (string)arguments[1], anum);
            }
            else
            {
                NAPI.Notification.SendNotificationToPlayer(sender, "Invalid number entered.");
            }
        }

        [RemoteEvent("phone_deleteContact")]
        public void PhoneDeleteContact(Player sender, params object[] arguments)
        {
            removecontact_cmd(sender, arguments[0].ToString());
        }

        [RemoteEvent("phone_loadMessagesContacts")]
        public void PhoneLoadMessagesContacts(Player sender, params object[] arguments)
        {
            Character character = sender.GetCharacter();
            var lmcitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
            if (lmcitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have a phone.");
                return;
            }
            var lmcphone = (Phone)lmcitems[0];

            //First get all messages for this phone.
            var cntcs = Phone.GetContactListOfMessages(lmcphone.PhoneNumber);

            //Now loop through them, substituting with name.
            var newContacts = cntcs.Select(x => new[] { lmcphone.Contacts.SingleOrDefault(y => y.Number.ToString() == x[0])?.Name ?? x[0], x[1], x[2], x[3] }).ToArray();
            NAPI.ClientEvent.TriggerClientEvent(sender, "phone_messageContactsLoaded", NAPI.Util.ToJson(newContacts));
        }

        [RemoteEvent("phone_sendMessage")]
        public void PhoneSendMessage(Player sender, params object[] arguments)
        {
            sms_cmd(sender, (string)arguments[0], (string)arguments[1]);
        }

        [RemoteEvent("phone_loadMessages")]
        public void PhoneLoadMessages(Player sender, params object[] arguments)
        {
            var contact = (string)arguments[0];
            var toSkip = (int)arguments[1];

            Character lmcharacter = sender.GetCharacter();
            var lmitems = InventoryManager.DoesInventoryHaveItem(lmcharacter, typeof(Phone));
            if (lmitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have a phone.");
                return;
            }
            var lmphone = (Phone)lmitems[0];
            string numbera = IsDigitsOnly(contact) ? contact : lmphone.Contacts.FirstOrDefault(x => x.Name == contact)?.Number ?? contact;

            var returnMsgs = Phone.GetMessageLog(lmphone.PhoneNumber, numbera, 10, toSkip);
            var actualMsgs = returnMsgs.Select(x => new[] { x.SenderNumber, x.Message, x.DateSent.ToString(), x.IsRead.ToString() }).ToArray();
            NAPI.ClientEvent.TriggerClientEvent(sender, "phone_showMessages", lmphone.PhoneNumber, NAPI.Util.ToJson(actualMsgs),
                (Phone.GetMessageCount(lmphone.PhoneNumber, numbera) - (toSkip + 10)) > 0, toSkip == 0);

            Phone.MarkMessagesAsRead(lmphone.PhoneNumber); //Mark as read.
        }

        [RemoteEvent("phone_getNotifications")]
        public void PhoneGetNotifications(Player sender, params object[] arguments)
        {
            Character gncharacter = sender.GetCharacter();
            var gnitems = InventoryManager.DoesInventoryHaveItem(gncharacter, typeof(Phone));
            if (gnitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have a phone.");
                return;
            }
            var gnphone = (Phone)gnitems[0];

            //Ready unread notification thing.
            var unreadMessages =
                DatabaseManager.MessagesTable.Count(x => x.ToNumber == gnphone.PhoneNumber && x.IsRead == false).ToString();

            NAPI.ClientEvent.TriggerClientEvent(sender, "phone_showNotifications", unreadMessages);
        }

        [RemoteEvent("phone_markMessagesRead")]
        public void PhoneMarkMessageRead(Player sender, params object[] arguments)
        {
            Character mkcharacter = sender.GetCharacter();
            var mkitems = InventoryManager.DoesInventoryHaveItem(mkcharacter, typeof(Phone));
            if (mkitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have a phone.");
                return;
            }
            var mkphone = (Phone)mkitems[0];
            Phone.MarkMessagesAsRead(mkphone.PhoneNumber);
        }

        [RemoteEvent("settings_getSettings")]
        public void SettingsGetSettings(Player sender, params object[] arguments)
        {
            Character sscharacter = sender.GetCharacter();
            var ssitems = InventoryManager.DoesInventoryHaveItem(sscharacter, typeof(Phone));
            if (ssitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have a phone.");
                return;
            }
            var ssphone = (Phone)ssitems[0];
            NAPI.ClientEvent.TriggerClientEvent(sender, "phone_showSettings", ssphone.PhoneNumber, ssphone.IsOn.ToString());
        }

        [RemoteEvent("settings_togPhone")]
        public void SettingsTogPhone(Player sender, params object[] arguments)
        {
            togphone_cmd(sender);
        }

        [Command("setphonename"), Help(HelpManager.CommandGroups.General, "To change your phone's name from being boring.", new[] { "Name of phone" })]
        public void setphonename_cmd(Player player, string name)
        {
            Character character = player.GetCharacter();
            var targetitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
            if (targetitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var charphone = (Phone)targetitems[0];

            charphone.PhoneName = name;
            NAPI.Chat.SendChatMessageToPlayer(player, "You have changed your phone name to " + name + ".");
        }

        [Command("pickup"), Help(HelpManager.CommandGroups.General, "To answer a call.", null)]
        public void pickup_cmd(Player player)
        {
            Character character = player.GetCharacter();
            if (character.InCallWith != Character.None)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are already on a phone call.");
                return;
            }

            if (character.BeingCalledBy == Character.None)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "None is trying to reach you.");
                return;
            }

            NAPI.Chat.SendChatMessageToPlayer(player, "You have answered the phone call.");
            NAPI.Chat.SendChatMessageToPlayer(character.BeingCalledBy.Player, "The other party have answered the phone.");
            character.BeingCalledBy.CallingPlayer = Character.None;
            character.BeingCalledBy.InCallWith = character;
            character.InCallWith = character.BeingCalledBy;
            character.BeingCalledBy = Character.None;
            character.InCallWith.CallingTimer.Dispose();

            var targetitems = InventoryManager.DoesInventoryHaveItem(character.InCallWith, typeof(Phone));
            var targetphone = (Phone)targetitems[0];

            var contact = targetphone.Contacts.Find(pc => pc.Number == targetphone.PhoneNumber);
            NAPI.ClientEvent.TriggerClientEvent(player, "phone_calling", contact?.Name ?? "Unknown", targetphone.PhoneNumber);
        }

        [Command("h"), Help(HelpManager.CommandGroups.General, "Hangup.", null)]
        public static void h_cmd(Player player)
        {
            Character character = player.GetCharacter();
            Character talkingTo;

            if (character.InCallWith == Character.None && character.CallingPlayer == Character.None && character.Calling911 == false)
            {
                API.Shared.SendChatMessageToPlayer(player, "You are not on a phone call.");
                return;
            }

            if (character.CallingPlayer != Character.None)
            {
                talkingTo = character.CallingPlayer;
                talkingTo.BeingCalledBy = Character.None;
                character.CallingPlayer = Character.None;
                API.Shared.SendChatMessageToPlayer(player, "You have terminated the call.");
                API.Shared.SendChatMessageToPlayer(talkingTo.Player, "The other party has ended the call.");
                API.Shared.TriggerClientEvent(player, "phone_call-closed");
                API.Shared.TriggerClientEvent(talkingTo.Player, "phone_call-closed");
            }
            else if(character.InCallWith != Character.None)
            {
                talkingTo = character.InCallWith;
                talkingTo.InCallWith = Character.None;
                character.InCallWith = Character.None;
                API.Shared.SendChatMessageToPlayer(player, "You have terminated the call.");
                API.Shared.SendChatMessageToPlayer(talkingTo.Player, "The other party has ended the call.");
                API.Shared.TriggerClientEvent(player, "phone_call-closed");
                API.Shared.TriggerClientEvent(talkingTo.Player, "phone_call-closed");
            }
            else if (character.Calling911 == true)
            {
                character.Calling911 = false;
                API.Shared.SendChatMessageToPlayer(player, "You have terminated the call.");
                API.Shared.TriggerClientEvent(player, "phone_call-closed");
            }
        }

        public void togphone_cmd(Player player)
        {
            Character character = player.GetCharacter();

            var targetitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
            if (targetitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var charphone = (Phone)targetitems[0];

            if (charphone.IsOn)
            {
                ChatManager.RoleplayMessage(character, "turned their phone off.", ChatManager.RoleplayMe);
                charphone.IsOn = false;
            }
            else
            {
                ChatManager.RoleplayMessage(character, "turned their phone on.", ChatManager.RoleplayMe);
                charphone.IsOn = true;
            }
        }

        static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        void OnCallSemiEnd(object args)
        {
            var aArgs = (Character[]) args;
            var character = aArgs[0];
            var sender = aArgs[1];
            if (character.InCallWith == Character.None && sender.InCallWith == Character.None && character.BeingCalledBy == sender && sender.CallingPlayer == character)
            {
                character.BeingCalledBy = Character.None;
                sender.CallingPlayer = Character.None;
                sender.CallingTimer.Dispose();
                NAPI.Chat.SendChatMessageToPlayer(sender.Player, "Call hanged up with no answer.");
                ChatManager.RoleplayMessage(character.Player, "'s phone stops to ring.", ChatManager.RoleplayMe);
                NAPI.ClientEvent.TriggerClientEvent(character.Player, "phone_call-closed");
                NAPI.ClientEvent.TriggerClientEvent(sender.Player, "phone_call-closed");
            }
        }
        public void call_cmd(Player player, string input)
        {
            Character sender = player.GetCharacter();

            var targetitems = InventoryManager.DoesInventoryHaveItem(sender, typeof(Phone));
            if (targetitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var senderphone = (Phone)targetitems[0];

            if (senderphone.IsOn == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Your phone is turned off.");
                return;
            }

            if (IsDigitsOnly(input))
            {
                if (input.Equals("911", StringComparison.OrdinalIgnoreCase))
                {
                    ChatManager.AmeLabelMessage(player, "takes out their phone and presses a few numbers..", 4000);
                    sender.Calling911 = true;
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.Grey, "911 Operator says: Los Santos Police Department, what is the nature of your emergency?");
                    NAPI.ClientEvent.TriggerClientEvent(player, "phone_calling", "LSPD", input);
                    return;
                }

                if (!DoesNumberExist(input))
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.White,
                        "The call failed to connect. (Phone number is not registered.)");
                    return;
                }

                var character = GetPlayerWithNumber(input);
                if (character == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.White,
                        "The call failed to connect. ((No one online found with that phone number))");
                    return;
                }
                var charphone = InventoryManager.DoesInventoryHaveItem<Phone>(character)[0];

                if (charphone.IsOn == false)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow,
                        "The number you are trying to reach is currently unavailable.");
                    return;
                }

                if (character.BeingCalledBy != Character.None || character.CallingPlayer != Character.None ||
                    character.InCallWith != Character.None)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow, "The line you're trying to call is busy.");
                    return;
                }

                ChatManager.AmeLabelMessage(player, "takes out their phone and presses a few numbers..", 4000);
                NAPI.Chat.SendChatMessageToPlayer(character.Player, "Incoming call from " + senderphone.PhoneNumber + "...");
                ChatManager.RoleplayMessage(character, "'s phone starts to ring...", ChatManager.RoleplayMe);
                sender.CallingPlayer = character;
                character.BeingCalledBy = sender;

                var contact = senderphone.Contacts.Find(pc => pc.Number == input);
                var targetContact = charphone.Contacts.Find(pc => pc.Number == senderphone.PhoneNumber);

                NAPI.ClientEvent.TriggerClientEvent(player, "phone_calling", contact?.Name ?? "Unknown", input);
                NAPI.ClientEvent.TriggerClientEvent(character.Player, "phone_incoming-call", targetContact?.Name ?? "Unknown", senderphone.PhoneNumber);


                sender.CallingTimer = new System.Threading.Timer(OnCallSemiEnd, new[] {character, sender}, 30000, -1);
            }
            else
            {
                if (!senderphone.HasContactWithName(input))
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You do not have a contact with the name: " + input);
                    return;
                }

                var contact = senderphone.Contacts.Find(pc => string.Equals(pc.Name, input, StringComparison.OrdinalIgnoreCase));
                var character = GetPlayerWithNumber(contact.Number);
                if (character == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.White,
                        "The call failed to connect. ((No one online found with that phone number))");
                    return;
                }
                var charphone = InventoryManager.DoesInventoryHaveItem<Phone>(character)[0];

                if (charphone.IsOn == false)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow,
                        "The number you are trying to reach is currently unavailable.");
                    return;
                }

                if (character.BeingCalledBy != Character.None || character.CallingPlayer != Character.None ||
                    character.InCallWith != Character.None)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow, "The line you're trying to call is busy.");
                    return;
                }

                ChatManager.AmeLabelMessage(player, "takes out their phone and presses a few numbers..", 4000);
                NAPI.Chat.SendChatMessageToPlayer(character.Player, "Incoming call from" + senderphone + "...");
                ChatManager.RoleplayMessage(character, "'s phone starts to ring...", ChatManager.RoleplayMe);
                sender.CallingPlayer = character;
                character.BeingCalledBy = sender;
                NAPI.ClientEvent.TriggerClientEvent(player, "phone_calling", contact.Name, contact.Number);
                var targetContact = charphone.Contacts.Find(pc => pc.Number == senderphone.PhoneNumber);
                NAPI.ClientEvent.TriggerClientEvent(character.Player, "phone_incoming-call", targetContact?.Name ?? "Unknown", senderphone.PhoneNumber);

                //Function to hangup after 30 seconds with no answer.
                sender.CallingTimer = new System.Threading.Timer(OnCallSemiEnd, new[] { character, sender }, 30000, -1);
            }
        }

        public void editcontact_cmd(Player player, string oldname, string newname, string number)
        {
            Character c = player.GetCharacter();

            var targetitems = InventoryManager.DoesInventoryHaveItem(c, typeof(Phone));
            if (targetitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var senderphone = (Phone)targetitems[0];

            var contact = senderphone.Contacts.Find(x => x.Name == oldname);
            if (contact != null)
            {
                contact.Name = newname;
                contact.Number = number;
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You have edited the contact " + oldname + "  to " + newname + " (" + number + ").");
                NAPI.ClientEvent.TriggerClientEvent(player, "phone_contactEdited", oldname, newname, number);
            }
            else
            {
                NAPI.Notification.SendNotificationToPlayer(player, "That contact doesn't exist.");
            }
        }

        public void addcontact_cmd(Player player, string number, string name)
        {
            Character c = player.GetCharacter();
            var targetitems = InventoryManager.DoesInventoryHaveItem(c, typeof(Phone));
            if (targetitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var senderphone = (Phone)targetitems[0];

            if (senderphone.HasContact(name, number))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White,
                    "You already have a contact with that phone number or name.");
                return;
            }

            senderphone.InsertContact(name, number);
            NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You have added the contact " + name + " (" + number + ") to your phone.");
            NAPI.ClientEvent.TriggerClientEvent(player, "phone_contactAdded", name, number);
        }

        public void removecontact_cmd(Player player, string name)
        {
            Character c = player.GetCharacter();
            var targetitems = InventoryManager.DoesInventoryHaveItem(c, typeof(Phone));
            if (targetitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var senderphone = (Phone)targetitems[0];

            if (string.Equals(name, "All", StringComparison.OrdinalIgnoreCase))
            {
                senderphone.DeleteAllContacts();
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You have deleted all of your contacts.");
            }
            else
            {
                if (!senderphone.HasContactWithName(name))
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You do not have a phone contact with that name.");
                    return;
                }

                var contact = senderphone.Contacts.First(pc => string.Equals(pc.Name, name, StringComparison.OrdinalIgnoreCase));
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You have deleted " + name + " (" + contact.Number + ") from your contacts.");
                senderphone.DeleteContact(contact);
                NAPI.ClientEvent.TriggerClientEvent(player, "phone_contactRemoved", name);
            }
        }

        public static void sms_cmd(Player player, string input, string message)
        {
            Character sender = player.GetCharacter();

            var targetitems = InventoryManager.DoesInventoryHaveItem(sender, typeof(Phone));
            if (targetitems.Length == 0)
            {
                API.Shared.SendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var senderphone = (Phone)targetitems[0];

            if (senderphone.IsOn == false)
            {
                API.Shared.SendChatMessageToPlayer(player, "Your phone is turned off.");
                return;
            }

            if (IsDigitsOnly(input)) // this is if player puts a number
            {
                if (!DoesNumberExist(input))
                {
                    API.Shared.SendChatMessageToPlayer(player, Color.White,
                        "The text message failed to send. (Phone number is not registered.)");
                    return;
                }

                var character = GetPlayerWithNumber(input);
                if (character != null)
                {
                    var charphone = InventoryManager.DoesInventoryHaveItem<Phone>(character)[0];
                    if (!charphone.IsOn)
                    {
                        API.Shared.SendChatMessageToPlayer(player, Color.White,
                            "The text message failed to send. That number is switched off. Please try again later.");
                        return;
                    }
                    API.Shared.TriggerClientEvent(character.Player, "phone_incomingMessage",
                        charphone.HasContactWithNumber(senderphone.PhoneNumber)
                            ? charphone.Contacts.Find(pc => pc.Number == senderphone.PhoneNumber).Name
                            : senderphone.PhoneNumber, message);
                    API.Shared.SendChatMessageToPlayer(character.Player, Color.Sms, "You've received an SMS.");
                    ChatManager.RoleplayMessage(character, "'s phone vibrates..", ChatManager.RoleplayMe);
                }
                else
                {
                    API.Shared.SendChatMessageToPlayer(player, Color.White,
                        "The text message failed to send. (Phone number is not online.)");
                    return;
                }

                string toMsg;

                if (senderphone.HasContactWithNumber(input))
                {
                    toMsg = "SMS to " + senderphone.Contacts.Find(pc => pc.Number == input).Name + ": " +
                            message;
                }
                else
                {
                    toMsg = "SMS to " + input + ": " +
                            message;
                }
                API.Shared.SendChatMessageToPlayer(sender.Player, Color.Sms, toMsg);
                ChatManager.AmeLabelMessage(player, "presses a few buttons on their phone, sending a message.", 4000);

                Phone.LogMessage(senderphone.PhoneNumber, input, message);
                API.Shared.TriggerClientEvent(player, "phone_messageSent");
            }
            else
            {
                if (!senderphone.HasContactWithName(input))
                {
                    API.Shared.SendChatMessageToPlayer(player, Color.White, "You do not have a contact with the name: " + input);
                    return;
                }

                var contact = senderphone.Contacts.Find(pc => string.Equals(pc.Name, input, StringComparison.OrdinalIgnoreCase));

                if (!DoesNumberExist(contact.Number))
                {
                    API.Shared.SendChatMessageToPlayer(player, Color.White,
                        "The text message failed to send. (Phone number is not registered.)");
                    return;
                }

                var character = GetPlayerWithNumber(contact.Number);
                if (character != null)
                {
                    var charphone = InventoryManager.DoesInventoryHaveItem<Phone>(character)[0];
                    if (!charphone.IsOn)
                    {
                        API.Shared.SendChatMessageToPlayer(player, Color.White,
                            "The text message failed to send. That number is switched off. Please try again later.");
                        return;
                    }
                    API.Shared.TriggerClientEvent(character.Player, "phone_incomingMessage",
                        charphone.HasContactWithNumber(senderphone.PhoneNumber)
                            ? charphone.Contacts.Find(pc => pc.Number == senderphone.PhoneNumber).Name
                            : senderphone.PhoneNumber, message);
                    API.Shared.SendChatMessageToPlayer(character.Player, Color.Sms, "You've received an SMS.");
                    ChatManager.RoleplayMessage(character, "'s phone vibrates..", ChatManager.RoleplayMe);

                    var toMsg = "SMS to " + contact.Name + ": " + message;
                    API.Shared.SendChatMessageToPlayer(sender.Player, Color.Sms, toMsg);
                    ChatManager.AmeLabelMessage(player, "presses a few buttons on their phone, sending a message.",
                        4000);

                    Phone.LogMessage(senderphone.PhoneNumber, contact.Number, message);
                    API.Shared.TriggerClientEvent(player, "phone_messageSent");
                }
                else
                {
                    API.Shared.SendChatMessageToPlayer(player, Color.White,
                        "The text message failed to send. ((Phone number is not online.))");
                }
            }
        }

        [Command("phone"), Help(HelpManager.CommandGroups.General, "How to view your phone.", null)]
        public void ShowPhone(Player player)
        {
            Character character = player.GetCharacter();
            var targetitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
            if (targetitems.Length == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }

            NAPI.Chat.SendChatMessageToPlayer(player, "~y~* Press ~h~F2~h~ to show the cursor.");
            var curTime = TimeWeatherManager.CurrentTime;
            NAPI.ClientEvent.TriggerClientEvent(player, "phone_showphone", curTime.Hour, curTime.Minute);
        }

        public static bool DoesNumberExist(string num)
        {
            var filter = Builders<PhoneNumber>.Filter.Eq(x => x.Number, num);
            return DatabaseManager.PhoneNumbersTable.Find(filter).Count() > 0;
        }

        public static Character GetPlayerWithNumber(string number)
        {
            foreach (var p in API.Shared.GetAllPlayers())
            {
                if (p == null)
                    continue;

                var c = p.GetCharacter();
                if(c == null)
                    continue;

                if (InventoryManager.DoesInventoryHaveItem<Phone>(c, x => x.PhoneNumber == number).Length > 0)
                    return c;
            }
            return null;
        }

        public static string GetNewNumber(int size = 6)
        {
            Random rnd = new Random();
            RestartNumberGenerate:
            string number = "";
            for (int i = 0; i < 6; i++)
            {
                var num = rnd.Next(0, 10);
                number = number + num;
            }
            if (DoesNumberExist(number))
            {
                goto RestartNumberGenerate;
            }
            return number;
        }
    }

/* To store numbers for phone. */
    public class PhoneNumber
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId PhoneId { get; set; }
        public string Number { get; set; }
    }
}
