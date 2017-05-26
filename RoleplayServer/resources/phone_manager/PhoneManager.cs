using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkServer;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.phone_manager
{
    public class PhoneManager : Script
    {
        public static List<Phone> Phones = new List<Phone>();

        public PhoneManager()
        {
            DebugManager.DebugMessage("[PhoneM] Initalizing Phone Manager...");

            API.onChatMessage += OnChatMessage;
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
                    var cntcs = Phone.GetContactListOfMessages(lmcphone.Number);
                    
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

                    var returnMsgs = Phone.GetMessageLog(lmphone.Number, numbera, 10, toSkip);
                    var actualMsgs = returnMsgs.Select(x => new[] {x.SenderNumber, x.Message, x.DateSent.ToString("g")}).ToArray();
                    API.triggerClientEvent(sender, "phone_showMessages", lmphone.Number, API.toJson(actualMsgs),
                        (Phone.GetMessageCount(lmphone.Number, numbera) - (toSkip + 10)) > 0, toSkip == 0);
                    break;
            }
        }

        public void OnChatMessage(Client player, string msg, CancelEventArgs e)
        {
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");
            if (account.AdminDuty == 0 && character.InCallWith != Character.None)
            {
                Character talkingTo = character.InCallWith;
                string phonemsg;
                var charitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
                var targetitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
                var charphone = (Phone)charitems[0];
                var targetphone = (Phone)targetitems[0];
                msg = "[Phone]" + character.rp_name() + " says: " + msg;
                ChatManager.NearbyMessage(player, 15, msg);
                if (targetphone.HasContactWithNumber(charphone.Number))
                {
                    phonemsg = "[" + targetphone.Contacts.Find(pc => pc.Number == charphone.Number).Name + "]" +
                         character.rp_name() + " says: " + msg;
                }
                else
                {
                    phonemsg = "[" + charphone.Number + "]" + character.rp_name() + " says: " + msg;
                }
                API.sendChatMessageToPlayer(talkingTo.Client, Color.Grey, phonemsg);
                e.Cancel = true;
                e.Reason = "Phone";
            }
        }


        [Command("setphonename", GreedyArg = true)]
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
            character.BeingCalledBy.CallingPlayer = Character.None;
            character.BeingCalledBy.InCallWith = character;
            character.InCallWith = character.BeingCalledBy;
            character.BeingCalledBy = Character.None;

            var targetitems = InventoryManager.DoesInventoryHaveItem(character.InCallWith, typeof(Phone));
            var targetphone = (Phone)targetitems[0];

            var contact = targetphone.Contacts.Find(pc => pc.Number == targetphone.Number);
            API.triggerClientEvent(player, "phone_calling", contact?.Name ?? "Unknown", targetphone.Number);
        }

        public void h_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.InCallWith == Character.None)
            {
                API.sendChatMessageToPlayer(player, "You are not on a phone call.");
                return;
            }
            Character talkingTo = character.InCallWith;
            talkingTo.InCallWith = Character.None;
            character.InCallWith = Character.None;
            API.sendChatMessageToPlayer(player, "You have terminated the call.");
            API.sendChatMessageToPlayer(talkingTo.Client, "The other party has ended the call.");
            API.triggerClientEvent(player, "phone_call-closed");
            API.triggerClientEvent(talkingTo.Client, "phone_call-closed");
        }

        [Command("togphone")]
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
            if (character.InCallWith == Character.None && sender.InCallWith == Character.None && character.BeingCalledBy == sender.CallingPlayer)
            {
                character.BeingCalledBy = Character.None;
                sender.CallingPlayer = Character.None;
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
                if (!DoesNumberExist(input))
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The call failed to connect. (Phone number is not registered.");
                    return;
                }

                var character = PlayerManager.Players.Find(c => InventoryManager.DoesInventoryHaveItem<Phone>(c)?[0].Number == input);
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
                API.sendChatMessageToPlayer(character.Client, "Incoming call from " + senderphone.Number + "...");
                ChatManager.RoleplayMessage(character, "'s phone starts to ring...", ChatManager.RoleplayMe);
                sender.CallingPlayer = character;
                character.BeingCalledBy = sender;

                var contact = senderphone.Contacts.Find(pc => pc.Number == input);
                var targetContact = charphone.Contacts.Find(pc => pc.Number == charphone.Number);

                API.triggerClientEvent(player, "phone_calling", contact?.Name ?? "Unknown", input);
                API.triggerClientEvent(character.Client, "phone_incoming-call", targetContact?.Name ?? "Unknown", charphone.Number);


                System.Threading.Timer timer = new System.Threading.Timer(OnCallSemiEnd, new[] {character, sender}, 30000, -1);
            }
            else
            {
                if (!senderphone.HasContactWithName(input))
                {
                    API.sendChatMessageToPlayer(player, Color.White, "You do not have a contact with the name: " + input);
                    return;
                }

                var contact = senderphone.Contacts.Find(pc => string.Equals(pc.Name, input, StringComparison.OrdinalIgnoreCase));
                var character = PlayerManager.Players.Find(c => InventoryManager.DoesInventoryHaveItem<Phone>(c)?[0].Number == contact.Number);
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
                var targetContact = charphone.Contacts.Find(pc => pc.Number == charphone.Number);
                API.triggerClientEvent(character.Client, "phone_incoming-call", targetContact.Name, charphone.Number);

                //Function to hangup after 30 seconds with no answer.
                System.Threading.Timer timer = new System.Threading.Timer(OnCallSemiEnd, new[] { character, sender }, 30000, -1);
            }
        }


        //TODO: Test Command.
        [Command("setphonenumber")]
        public void setphone_cmd(Client player, string id, string number)
        {
            var receiver = PlayerManager.ParseClient(id);
            Character rec = API.getEntityData(receiver.handle, "Character");
            if (DoesNumberExist(number))
            {
                API.sendChatMessageToPlayer(player, "That number is taken.");
                return;
            }

            var phone = new Phone
            {
                Number = number,
                PhoneName = "default",
                IsOn = true
            };

            API.sendChatMessageToPlayer(player, InventoryManager.GiveInventoryItem(rec, phone).ToString());
            Phone.InsertNumber(phone.Id, number);

            API.sendChatMessageToPlayer(player, "You have given " + rec.CharacterName + " a phone. Number is " + number + ".");
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
                        "The text message failed to send. (Phone number is not registered.");
                    return;
                }

                var character = PlayerManager.Players.Find(c => InventoryManager.DoesInventoryHaveItem<Phone>(c)?[0].Number == input);
                if (character != null)
                {
                    var charphone = InventoryManager.DoesInventoryHaveItem<Phone>(character)[0];
                    API.triggerClientEvent(character.Client, "phone_incomingMessage",
                        charphone.HasContactWithNumber(senderphone.Number)
                            ? charphone.Contacts.Find(pc => pc.Number == senderphone.Number).Name
                            : senderphone.Number, message);
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

                Phone.LogMessage(senderphone.Number, input, message);
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

                var character = PlayerManager.Players.Find(c => InventoryManager.DoesInventoryHaveItem<Phone>(c)?[0].Number == contact.Number);
                if (character != null)
                {
                    var charphone = InventoryManager.DoesInventoryHaveItem<Phone>(character)[0];
                    API.triggerClientEvent(character.Client, "phone_incomingMessage",
                        charphone.HasContactWithNumber(senderphone.Number)
                            ? charphone.Contacts.Find(pc => pc.Number == senderphone.Number).Name
                            : senderphone.Number, message);
                    API.sendChatMessageToPlayer(character.Client, Color.Sms, "You've received an SMS.");
                    ChatManager.RoleplayMessage(character, "'s phone vibrates..", ChatManager.RoleplayMe);
                }
               

                var toMsg = "SMS to " + contact.Name + ": " + message;
                API.sendChatMessageToPlayer(sender.Client, Color.Sms, toMsg);
                ChatManager.AmeLabelMessage(player, "presses a few buttons on their phone, sending a message.", 4000);

                Phone.LogMessage(senderphone.Number, contact.Number, message);
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
            API.triggerClientEvent(player, "phone_showphone");
        }

        public bool DoesNumberExist(string num)
        {
            var filter = Builders<PhoneNumber>.Filter.Eq(x => x.Number, num);
            return DatabaseManager.PhoneNumbersTable.Find(filter).Count() > 0;
        }
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