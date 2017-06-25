using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GTANetworkServer;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace mtgvrp.phone_manager
{
    public class PhoneManager : Script
    {
        public PhoneManager()
        {
            DebugManager.DebugMessage("[PhoneM] Initalizing Phone Manager...");

            API.onClientEventTrigger += API_onClientEventTrigger;

            DebugManager.DebugMessage("[PhoneM] Phone Manager initalized.");
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "phone_callphone":
                    string number = (string)arguments[0];
                    call_cmd(sender, number);
                    break;

                case "phone_answercall":
                    pickup_cmd(sender);
                    break;

                case "phone_hangout":
                    h_cmd(sender);
                    break;

                case "phone_getallContacts":
                    //Check if has phone.
                    var items = InventoryManager.DoesInventoryHaveItem(sender.GetCharacter(), typeof(Phone));
                    if (items.Length == 0)
                    {
                        API.sendChatMessageToPlayer(sender, "You don't have a phone.");
                        return;
                    }
                    var phone = (Phone) items[0];

                    string[][] contacts = phone.Contacts.Select(x => new[] { x.Name, x.Number.ToString()}).ToArray();
                    API.triggerClientEvent(sender, "phone_showContacts", API.toJson(contacts));
                    break;

                case "phone_saveContact":
                    var name = arguments[0].ToString();
                    string num = (string)arguments[1];
                    if (IsDigitsOnly(num))
                    {
                        addcontact_cmd(sender, num, name);
                    }
                    else
                    {
                        API.sendNotificationToPlayer(sender, "Invalid number entered.");
                    }
                    break;

                case "phone_editContact":
                    string anum = (string)arguments[2];
                    if (IsDigitsOnly(anum))
                    {
                        editcontact_cmd(sender, (string)arguments[0], (string)arguments[1], anum);
                    }
                    else
                    {
                        API.sendNotificationToPlayer(sender, "Invalid number entered.");
                    }
                    break;

                case "phone_deleteContact":
                    removecontact_cmd(sender, arguments[0].ToString());
                    break;

                case "phone_loadMessagesContacts":
                    Character character = sender.GetCharacter();
                    var lmcitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
                    if (lmcitems.Length == 0)
                    {
                        API.sendChatMessageToPlayer(sender, "You don't have a phone.");
                        return;
                    }
                    var lmcphone = (Phone)lmcitems[0];

                    //First get all messages for this phone.
                    var cntcs = Phone.GetContactListOfMessages(lmcphone.PhoneNumber);
                    
                    //Now loop through them, substituting with name.
                    var newContacts = cntcs.Select(x => new[] { lmcphone.Contacts.SingleOrDefault(y => y.Number.ToString() == x[0])?.Name ?? x[0], x[1], x[2], x[3]}).ToArray();
                    API.triggerClientEvent(sender, "phone_messageContactsLoaded", API.toJson(newContacts));
                    break;

                case "phone_sendMessage":
                    sms_cmd(sender, (string)arguments[0], (string)arguments[1]);
                    break;

                case "phone_loadMessages":
                    var contact = (string)arguments[0];
                    var toSkip = (int) arguments[1];

                    Character lmcharacter = sender.GetCharacter();
                    var lmitems = InventoryManager.DoesInventoryHaveItem(lmcharacter, typeof(Phone));
                    if (lmitems.Length == 0)
                    {
                        API.sendChatMessageToPlayer(sender, "You don't have a phone.");
                        return;
                    }
                    var lmphone = (Phone)lmitems[0];
                    string numbera = IsDigitsOnly(contact) ? contact : lmphone.Contacts.Find(x => x.Name == contact).Number;

                    var returnMsgs = Phone.GetMessageLog(lmphone.PhoneNumber, numbera, 10, toSkip);
                    var actualMsgs = returnMsgs.Select(x => new[] {x.SenderNumber, x.Message, x.DateSent.ToString(), x.IsRead.ToString()}).ToArray();
                    API.triggerClientEvent(sender, "phone_showMessages", lmphone.PhoneNumber, API.toJson(actualMsgs),
                        (Phone.GetMessageCount(lmphone.PhoneNumber, numbera) - (toSkip + 10)) > 0, toSkip == 0);

                    Phone.MarkMessagesAsRead(lmphone.PhoneNumber); //Mark as read.
                    break;

                case "phone_getNotifications":
                    Character gncharacter = sender.GetCharacter();
                    var gnitems = InventoryManager.DoesInventoryHaveItem(gncharacter, typeof(Phone));
                    if (gnitems.Length == 0)
                    {
                        API.sendChatMessageToPlayer(sender, "You don't have a phone.");
                        return;
                    }
                    var gnphone = (Phone)gnitems[0];

                    //Ready unread notification thing.
                    var unreadMessages =
                        DatabaseManager.MessagesTable.Count(x => x.ToNumber == gnphone.PhoneNumber && x.IsRead == false).ToString();
                    
                    API.triggerClientEvent(sender, "phone_showNotifications", unreadMessages);
                    break;

                case "phone_markMessagesRead":
                    Character mkcharacter = sender.GetCharacter();
                    var mkitems = InventoryManager.DoesInventoryHaveItem(mkcharacter, typeof(Phone));
                    if (mkitems.Length == 0)
                    {
                        API.sendChatMessageToPlayer(sender, "You don't have a phone.");
                        return;
                    }
                    var mkphone = (Phone)mkitems[0];
                    Phone.MarkMessagesAsRead(mkphone.PhoneNumber);
                    break;

                case "settings_getSettings":
                    Character sscharacter = sender.GetCharacter();
                    var ssitems = InventoryManager.DoesInventoryHaveItem(sscharacter, typeof(Phone));
                    if (ssitems.Length == 0)
                    {
                        API.sendChatMessageToPlayer(sender, "You don't have a phone.");
                        return;
                    }
                    var ssphone = (Phone)ssitems[0];
                    API.triggerClientEvent(sender, "phone_showSettings", ssphone.PhoneNumber, ssphone.IsOn.ToString());
                    break;

                case "settings_togPhone":
                    togphone_cmd(sender);
                    break;
            }
        }


        [Command("setphonename")]
        public void setphonename_cmd(Client player, string name)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");
            var targetitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
            if (targetitems.Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var charphone = (Phone)targetitems[0];

            charphone.PhoneName = name;
            API.sendChatMessageToPlayer(player, "You have changed your phone name to " + name + ".");
        }

        [Command("pickup")]
        public void pickup_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.InCallWith != Character.None)
            {
                API.sendChatMessageToPlayer(player, "You are already on a phone call.");
                return;
            }

            if (character.BeingCalledBy == Character.None)
            {
                API.sendChatMessageToPlayer(player, "None is trying to reach you.");
                return;
            }

            API.sendChatMessageToPlayer(player, "You have answered the phone call.");
            API.sendChatMessageToPlayer(character.BeingCalledBy.Client, "The other party have answered the phone.");
            character.BeingCalledBy.CallingPlayer = Character.None;
            character.BeingCalledBy.InCallWith = character;
            character.InCallWith = character.BeingCalledBy;
            character.BeingCalledBy = Character.None;
            character.InCallWith.CallingTimer.Dispose();

            var targetitems = InventoryManager.DoesInventoryHaveItem(character.InCallWith, typeof(Phone));
            var targetphone = (Phone)targetitems[0];

            var contact = targetphone.Contacts.Find(pc => pc.Number == targetphone.PhoneNumber);
            API.triggerClientEvent(player, "phone_calling", contact?.Name ?? "Unknown", targetphone.PhoneNumber);
        }

        [Command("h")]
        public static void h_cmd(Client player)
        {
            Character character = player.GetCharacter();
            Character talkingTo;

            if (character.InCallWith == Character.None && character.CallingPlayer == Character.None && character.Calling911 == false)
            {
                API.shared.sendChatMessageToPlayer(player, "You are not on a phone call.");
                return;
            }

            if (character.CallingPlayer != Character.None)
            {
                talkingTo = character.CallingPlayer;
                talkingTo.BeingCalledBy = Character.None;
                character.CallingPlayer = Character.None;
                API.shared.sendChatMessageToPlayer(player, "You have terminated the call.");
                API.shared.sendChatMessageToPlayer(talkingTo.Client, "The other party has ended the call.");
                API.shared.triggerClientEvent(player, "phone_call-closed");
                API.shared.triggerClientEvent(talkingTo.Client, "phone_call-closed");
            }
            else if(character.InCallWith != Character.None)
            {
                talkingTo = character.InCallWith;
                talkingTo.InCallWith = Character.None;
                character.InCallWith = Character.None;
                API.shared.sendChatMessageToPlayer(player, "You have terminated the call.");
                API.shared.sendChatMessageToPlayer(talkingTo.Client, "The other party has ended the call.");
                API.shared.triggerClientEvent(player, "phone_call-closed");
                API.shared.triggerClientEvent(talkingTo.Client, "phone_call-closed");
            }
            else if (character.Calling911 == true)
            {
                character.Calling911 = false;
                API.shared.sendChatMessageToPlayer(player, "You have terminated the call.");
                API.shared.triggerClientEvent(player, "phone_call-closed");
            }
        }

        public void togphone_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            var targetitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
            if (targetitems.Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have a phone.");
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

        bool IsDigitsOnly(string str)
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
                API.sendChatMessageToPlayer(sender.Client, "Call hanged up with no answer.");
                ChatManager.RoleplayMessage(character.Client, "'s phone stops to ring.", ChatManager.RoleplayMe);
                API.triggerClientEvent(character.Client, "phone_call-closed");
                API.triggerClientEvent(sender.Client, "phone_call-closed");
            }
        }
        public void call_cmd(Client player, string input)
        {
            Character sender = API.shared.getEntityData(player.handle, "Character");

            var targetitems = InventoryManager.DoesInventoryHaveItem(sender, typeof(Phone));
            if (targetitems.Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var senderphone = (Phone)targetitems[0];

            if (senderphone.IsOn == false)
            {
                API.sendChatMessageToPlayer(player, "Your phone is turned off.");
                return;
            }

            if (IsDigitsOnly(input))
            {
                if (input.Equals("911", StringComparison.OrdinalIgnoreCase))
                {
                    ChatManager.AmeLabelMessage(player, "takes out their phone and presses a few numbers..", 4000);
                    sender.Calling911 = true;
                    API.sendChatMessageToPlayer(player, Color.Grey, "911 Operator says: Los Santos Police Department, what is the nature of your emergency?");
                    API.triggerClientEvent(player, "phone_calling", "LSPD", input);
                    return;
                }

                if (!DoesNumberExist(input))
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The call failed to connect. (Phone number is not registered.)");
                    return;
                }

                var character = PlayerManager.Players.Find(c => InventoryManager.DoesInventoryHaveItem<Phone>(c)?[0].PhoneNumber == input);
                if (character == null)
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The call failed to connect. ((No one online found with that phone number))");
                    return;
                }
                var charphone = InventoryManager.DoesInventoryHaveItem<Phone>(character)[0];

                if (charphone.IsOn == false)
                {
                    API.sendChatMessageToPlayer(player, Color.Yellow,
                        "The number you are trying to reach is currently unavailable.");
                    return;
                }

                if (character.BeingCalledBy != Character.None || character.CallingPlayer != Character.None ||
                    character.InCallWith != Character.None)
                {
                    API.sendChatMessageToPlayer(player, Color.Yellow, "The line you're trying to call is busy.");
                    return;
                }

                ChatManager.AmeLabelMessage(player, "takes out their phone and presses a few numbers..", 4000);
                API.sendChatMessageToPlayer(character.Client, "Incoming call from " + senderphone.PhoneNumber + "...");
                ChatManager.RoleplayMessage(character, "'s phone starts to ring...", ChatManager.RoleplayMe);
                sender.CallingPlayer = character;
                character.BeingCalledBy = sender;

                var contact = senderphone.Contacts.Find(pc => pc.Number == input);
                var targetContact = charphone.Contacts.Find(pc => pc.Number == charphone.PhoneNumber);

                API.triggerClientEvent(player, "phone_calling", contact?.Name ?? "Unknown", input);
                API.triggerClientEvent(character.Client, "phone_incoming-call", targetContact?.Name ?? "Unknown", senderphone.PhoneNumber);


                sender.CallingTimer = new System.Threading.Timer(OnCallSemiEnd, new[] {character, sender}, 30000, -1);
            }
            else
            {
                if (!senderphone.HasContactWithName(input))
                {
                    API.sendChatMessageToPlayer(player, Color.White, "You do not have a contact with the name: " + input);
                    return;
                }

                var contact = senderphone.Contacts.Find(pc => string.Equals(pc.Name, input, StringComparison.OrdinalIgnoreCase));
                var character = PlayerManager.Players.Find(c => InventoryManager.DoesInventoryHaveItem<Phone>(c)?[0].PhoneNumber == contact.Number);
                if (character == null)
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The call failed to connect. ((No one online found with that phone number))");
                    return;
                }
                var charphone = InventoryManager.DoesInventoryHaveItem<Phone>(character)[0];

                if (charphone.IsOn == false)
                {
                    API.sendChatMessageToPlayer(player, Color.Yellow,
                        "The number you are trying to reach is currently unavailable.");
                    return;
                }

                if (character.BeingCalledBy != Character.None || character.CallingPlayer != Character.None ||
                    character.InCallWith != Character.None)
                {
                    API.sendChatMessageToPlayer(player, Color.Yellow, "The line you're trying to call is busy.");
                    return;
                }

                ChatManager.AmeLabelMessage(player, "takes out their phone and presses a few numbers..", 4000);
                API.sendChatMessageToPlayer(character.Client, "Incoming call from" + senderphone + "...");
                ChatManager.RoleplayMessage(character, "'s phone starts to ring...", ChatManager.RoleplayMe);
                sender.CallingPlayer = character;
                character.BeingCalledBy = sender;
                API.triggerClientEvent(player, "phone_calling", contact.Name, contact.Number);
                var targetContact = charphone.Contacts.Find(pc => pc.Number == charphone.PhoneNumber);
                API.triggerClientEvent(character.Client, "phone_incoming-call", targetContact?.Name ?? "Unknown", senderphone.PhoneNumber);

                //Function to hangup after 30 seconds with no answer.
                sender.CallingTimer = new System.Threading.Timer(OnCallSemiEnd, new[] { character, sender }, 30000, -1);
            }
        }

        public void editcontact_cmd(Client player, string oldname, string newname, string number)
        {
            Character c = API.getEntityData(player.handle, "Character");

            var targetitems = InventoryManager.DoesInventoryHaveItem(c, typeof(Phone));
            if (targetitems.Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var senderphone = (Phone)targetitems[0];

            var contact = senderphone.Contacts.Find(x => x.Name == oldname);
            if (contact != null)
            {
                contact.Name = newname;
                contact.Number = number;
                API.sendChatMessageToPlayer(player, Color.White, "You have edited the contact " + oldname + "  to " + newname + " (" + number + ").");
                API.triggerClientEvent(player, "phone_contactEdited", oldname, newname, number);
            }
            else
            {
                API.sendNotificationToPlayer(player, "That contact doesn't exist.");
            }
        }

        public void addcontact_cmd(Client player, string number, string name)
        {
            Character c = API.getEntityData(player.handle, "Character");
            var targetitems = InventoryManager.DoesInventoryHaveItem(c, typeof(Phone));
            if (targetitems.Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var senderphone = (Phone)targetitems[0];

            if (senderphone.HasContact(name, number))
            {
                API.sendChatMessageToPlayer(player, Color.White,
                    "You already have a contact with that phone number or name.");
                return;
            }

            senderphone.InsertContact(name, number);
            API.sendChatMessageToPlayer(player, Color.White, "You have added the contact " + name + " (" + number + ") to your phone.");
            API.triggerClientEvent(player, "phone_contactAdded", name, number);
        }

        public void removecontact_cmd(Client player, string name)
        {
            Character c = API.getEntityData(player.handle, "Character");
            var targetitems = InventoryManager.DoesInventoryHaveItem(c, typeof(Phone));
            if (targetitems.Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var senderphone = (Phone)targetitems[0];

            if (string.Equals(name, "All", StringComparison.OrdinalIgnoreCase))
            {
                senderphone.DeleteAllContacts();
                API.sendChatMessageToPlayer(player, Color.White, "You have deleted all of your contacts.");
            }
            else
            {
                if (!senderphone.HasContactWithName(name))
                {
                    API.sendChatMessageToPlayer(player, Color.White, "You do not have a phone contact with that name.");
                    return;
                }

                var contact = senderphone.Contacts.First(pc => string.Equals(pc.Name, name, StringComparison.OrdinalIgnoreCase));
                API.sendChatMessageToPlayer(player, Color.White, "You have deleted " + name + " (" + contact.Number + ") from your contacts.");
                senderphone.DeleteContact(contact);
                API.triggerClientEvent(player, "phone_contactRemoved", name);
            }
        }

        public void sms_cmd(Client player, string input, string message)
        {
            Character sender = player.GetCharacter();

            var targetitems = InventoryManager.DoesInventoryHaveItem(sender, typeof(Phone));
            if (targetitems.Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }
            var senderphone = (Phone)targetitems[0];

            if (senderphone.IsOn == false)
            {
                API.sendChatMessageToPlayer(player, "Your phone is turned off.");
                return;
            }

            if (IsDigitsOnly(input)) // this is if player puts a number
            {
                if (!DoesNumberExist(input))
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The text message failed to send. (Phone number is not registered.)");
                    return;
                }

                var character = PlayerManager.Players.Find(c => InventoryManager.DoesInventoryHaveItem<Phone>(c)?[0].PhoneNumber == input);
                if (character != null)
                {
                    var charphone = InventoryManager.DoesInventoryHaveItem<Phone>(character)[0];
                    API.triggerClientEvent(character.Client, "phone_incomingMessage",
                        charphone.HasContactWithNumber(senderphone.PhoneNumber)
                            ? charphone.Contacts.Find(pc => pc.Number == senderphone.PhoneNumber).Name
                            : senderphone.PhoneNumber, message);
                    API.sendChatMessageToPlayer(character.Client, Color.Sms, "You've received an SMS.");
                    ChatManager.RoleplayMessage(character, "'s phone vibrates..", ChatManager.RoleplayMe);
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
                API.sendChatMessageToPlayer(sender.Client, Color.Sms, toMsg);
                ChatManager.AmeLabelMessage(player, "presses a few buttons on their phone, sending a message.", 4000);

                Phone.LogMessage(senderphone.PhoneNumber, input, message);
                API.triggerClientEvent(player, "phone_messageSent");
            }
            else
            {
                if (!senderphone.HasContactWithName(input))
                {
                    API.sendChatMessageToPlayer(player, Color.White, "You do not have a contact with the name: " + input);
                    return;
                }

                var contact = senderphone.Contacts.Find(pc => string.Equals(pc.Name, input, StringComparison.OrdinalIgnoreCase));

                if (!DoesNumberExist(contact.Number))
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The text message failed to send. (Phone number is not registered.");
                    return;
                }

                var character = PlayerManager.Players.Find(c => InventoryManager.DoesInventoryHaveItem<Phone>(c)?[0].PhoneNumber == contact.Number);
                if (character != null)
                {
                    var charphone = InventoryManager.DoesInventoryHaveItem<Phone>(character)[0];
                    API.triggerClientEvent(character.Client, "phone_incomingMessage",
                        charphone.HasContactWithNumber(senderphone.PhoneNumber)
                            ? charphone.Contacts.Find(pc => pc.Number == senderphone.PhoneNumber).Name
                            : senderphone.PhoneNumber, message);
                    API.sendChatMessageToPlayer(character.Client, Color.Sms, "You've received an SMS.");
                    ChatManager.RoleplayMessage(character, "'s phone vibrates..", ChatManager.RoleplayMe);
                }
               

                var toMsg = "SMS to " + contact.Name + ": " + message;
                API.sendChatMessageToPlayer(sender.Client, Color.Sms, toMsg);
                ChatManager.AmeLabelMessage(player, "presses a few buttons on their phone, sending a message.", 4000);

                Phone.LogMessage(senderphone.PhoneNumber, contact.Number, message);
                API.triggerClientEvent(player, "phone_messageSent");
            }
        }

        [Command("phone")]
        public void ShowPhone(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");
            var targetitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
            if (targetitems.Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }

            API.sendChatMessageToPlayer(player, "~y~* Press ~h~F2~h~ to show the cursor.");
            var curTime = TimeWeatherManager.CurrentTime;
            API.triggerClientEvent(player, "phone_showphone", curTime.Hour, curTime.Minute);
        }

        public static bool DoesNumberExist(string num)
        {
            var filter = Builders<PhoneNumber>.Filter.Eq(x => x.Number, num);
            return DatabaseManager.PhoneNumbersTable.Find(filter).Count() > 0;
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