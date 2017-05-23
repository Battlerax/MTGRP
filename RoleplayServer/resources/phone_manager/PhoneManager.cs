using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkServer;
using MongoDB.Driver;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;
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
                    string[][] contacts = sender.GetCharacter().Phone.Contacts.Select(x => new[] { x.Name, x.Number.ToString()}).ToArray();
                    API.triggerClientEvent(sender, "phone_showContacts", API.toJson(contacts));
                    break;

                case "phone_saveContact":
                    var name = arguments[0].ToString();
                    int num;
                    if (int.TryParse((string)arguments[1], out num))
                    {
                        addcontact_cmd(sender, num, name);
                    }
                    else
                    {
                        API.sendNotificationToPlayer(sender, "Invalid number entered.");
                    }
                    break;

                case "phone_editContact":
                    int anum;
                    if (int.TryParse((string)arguments[2], out anum))
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
                    if (character.Phone == Phone.None)
                    {
                        API.sendChatMessageToPlayer(sender, Color.White, "You do not have a phone!");
                        return;
                    }
                    //First get all messages for this phone.
                    var cntcs = Phone.GetContactListOfMessages(character.Phone.Number);
                    
                    //Now loop through them, substituting with name.
                    var newContacts = cntcs.Select(x => new[] { character.Phone.Contacts.SingleOrDefault(y => y.Number.ToString() == x[0])?.Name ?? x[0], x[1], x[2], x[3]}).ToArray();
                    API.triggerClientEvent(sender, "phone_messageContactsLoaded", API.toJson(newContacts));
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
                msg = "[Phone]" + character.rp_name() + " says: " + msg;
                ChatManager.NearbyMessage(player, 15, msg);
                if (talkingTo.Phone.HasContactWithNumber(character.Phone.Number))
                {
                    phonemsg = "[" + talkingTo.Phone.Contacts.Find(pc => pc.Number == character.Phone.Number).Name + "]" +
                         character.rp_name() + " says: " + msg;
                }
                else
                {
                    phonemsg = "[" + character.Phone.Number + "]" + character.rp_name() + " says: " + msg;
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

            if (character.Phone == Phone.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You do not have a phone!");
                return;
            }

            character.Phone.PhoneName = name;
            character.Phone.Save();
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

            var contact = character.InCallWith.Phone.Contacts.Find(pc => pc.Number == character.InCallWith.Phone.Number);
            API.triggerClientEvent(player, "phone_calling", contact?.Name ?? "Unknown", character.InCallWith.Phone.Number);
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

            if (character.Phone == Phone.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You do not have a phone!");
                return;
            }

            if (character.Phone.IsOn)
            {
                ChatManager.RoleplayMessage(character, "turned their phone off.", ChatManager.RoleplayMe);
                character.Phone.IsOn = false;
                character.Phone.Save();
            }
            else
            {
                ChatManager.RoleplayMessage(character, "turned their phone on.", ChatManager.RoleplayMe);
                character.Phone.IsOn = true;
                character.Phone.Save();
            }
        }


        public void call_cmd(Client player, string input)
        {
            int number;
            Character sender = API.shared.getEntityData(player.handle, "Character");

            if (sender.Phone == Phone.None)
            {
                API.sendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }

            if (sender.Phone.IsOn == false)
            {
                API.sendChatMessageToPlayer(player, "Your phone is turned off.");
                return;
            }

            var canConvert = int.TryParse(input, out number);
            if (canConvert)
            {
                if (!DoesNumberExist(number))
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The call failed to connect. (Phone number is not registered.");
                    return;
                }

                var character = PlayerManager.Players.Find(c => c.Phone.Number == number);

                if (character == null)
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The call failed to connect. ((No one online found with that phone number))");
                    return;
                }

                if (character.Phone.IsOn == false)
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
                API.sendChatMessageToPlayer(character.Client, "Incoming call from " + sender.PhoneNumber + "...");
                ChatManager.RoleplayMessage(character, "'s phone starts to ring...", ChatManager.RoleplayMe);
                sender.CallingPlayer = character;
                character.BeingCalledBy = sender;

                var contact = sender.Phone.Contacts.Find(pc => pc.Number == number);
                var targetContact = character.Phone.Contacts.Find(pc => pc.Number == character.Phone.Number);

                API.triggerClientEvent(player, "phone_calling", contact?.Name ?? "Unknown", number);
                API.triggerClientEvent(character.Client, "phone_incoming-call", targetContact?.Name ?? "Unknown", character.Phone.Number);

                //Function to hangup after 30 seconds with no answer.
                Task.Factory.StartNew(() =>
                {
                    //Wait.
                    System.Threading.Thread.Sleep(30000);
                    
                    //Make sure is not in call.
                    if (character.InCallWith == Character.None && character.BeingCalledBy != Character.None &&
                        sender.CallingPlayer != Character.None)
                    {
                        character.BeingCalledBy = Character.None;
                        sender.CallingPlayer = Character.None;
                        API.sendChatMessageToPlayer(sender.Client, "Call hanged up with no answer.");
                        ChatManager.RoleplayMessage(character.Client, "'s phone stops to ring.", ChatManager.RoleplayMe);
                        API.triggerClientEvent(character.Client, "phone_call-closed");
                        API.triggerClientEvent(sender.Client, "phone_call-closed");
                    }
                });
            }
            else
            {
                if (!sender.Phone.HasContactWithName(input))
                {
                    API.sendChatMessageToPlayer(player, Color.White, "You do not have a contact with the name: " + input);
                    return;
                }

                var contact = sender.Phone.Contacts.Find(pc => string.Equals(pc.Name, input, StringComparison.OrdinalIgnoreCase));
                var character = PlayerManager.Players.Find(c => c.Phone.Number == contact.Number);

                if (character == null)
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The call failed to connect. ((No one online found with that phone number))");
                    return;
                }

                if (character.Phone.IsOn == false)
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
                API.sendChatMessageToPlayer(character.Client, "Incoming call from" + sender.PhoneNumber + "...");
                ChatManager.RoleplayMessage(character, "'s phone starts to ring...", ChatManager.RoleplayMe);
                sender.CallingPlayer = character;
                character.BeingCalledBy = sender;
                API.triggerClientEvent(player, "phone_calling", contact.Name, contact.Number);
                var targetContact = character.Phone.Contacts.Find(pc => pc.Number == character.Phone.Number);
                API.triggerClientEvent(character.Client, "phone_incoming-call", targetContact.Name, character.Phone.Number);

                //Function to hangup after 30 seconds with no answer.
                Task.Factory.StartNew(() =>
                {
                    //Wait.
                    System.Threading.Thread.Sleep(30000);

                    //Make sure is not in call.
                    if (character.InCallWith == Character.None && character.BeingCalledBy != Character.None &&
                        sender.CallingPlayer != Character.None)
                    {
                        character.BeingCalledBy = Character.None;
                        sender.CallingPlayer = Character.None;
                        API.sendChatMessageToPlayer(sender.Client, "Call hanged up with no answer.");
                        ChatManager.RoleplayMessage(character.Client, "'s phone stops to ring.", ChatManager.RoleplayMe);
                        API.triggerClientEvent(character.Client, "phone_call-closed");
                        API.triggerClientEvent(sender.Client, "phone_call-closed");
                    }
                });
            }
        }


        [Command("setphonenumber")]
        public void setphone_cmd(Client player, string id, int number)
        {
            var receiver = PlayerManager.ParseClient(id);
            Character rec = API.getEntityData(receiver.handle, "Character");

            var phone = new Phone
            {
                Number = number,
                IsOn = true
            };

            phone.Insert();

            rec.PhoneNumber = number;
            rec.Phone = phone;
            rec.Save();
            API.sendChatMessageToPlayer(player, "You have changed " + rec.CharacterName + "'s phone number to " + number + ".");
        }

        public void editcontact_cmd(Client player, string oldname, string newname, int number)
        {
            Character c = API.getEntityData(player.handle, "Character");

            if (c.Phone == Phone.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You do not have a phone!");
                return;
            }

            var contact = c.Phone.Contacts.Find(x => x.Name == oldname);
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

        public void addcontact_cmd(Client player, int number, string name)
        {
            Character c = API.getEntityData(player.handle, "Character");

            if (c.Phone == Phone.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You do not have a phone!");
                return;
            }

            if (c.Phone.HasContact(name, number))
            {
                API.sendChatMessageToPlayer(player, Color.White,
                    "You already have a contact with that phone number or name.");
                return;
            }

            c.Phone.InsertContact(name, number);
            API.sendChatMessageToPlayer(player, Color.White, "You have added the contact " + name + " (" + number + ") to your phone.");
            API.triggerClientEvent(player, "phone_contactAdded", name, number);
        }

        public void removecontact_cmd(Client player, string name)
        {
            Character c = API.getEntityData(player.handle, "Character");

            if (c.Phone == Phone.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You do not have a phone!");
                return;
            }

            if (string.Equals(name, "All", StringComparison.OrdinalIgnoreCase))
            {
                c.Phone.DeleteAllContacts();
                API.sendChatMessageToPlayer(player, Color.White, "You have deleted all of your contacts.");
            }
            else
            {
                if (!c.Phone.HasContactWithName(name))
                {
                    API.sendChatMessageToPlayer(player, Color.White, "You do not have a phone contact with that name.");
                    return;
                }

                var contact = c.Phone.Contacts.FirstOrDefault(pc => string.Equals(pc.Name, name, StringComparison.OrdinalIgnoreCase));
                API.sendChatMessageToPlayer(player, Color.White, "You have deleted " + name + " (" + contact.Number + ") from your contacts.");
                c.Phone.DeleteContact(contact);
                API.triggerClientEvent(player, "phone_contactRemoved", name);
            }
        }

        [Command("sms", GreedyArg = true)]
        public void sms_cmd(Client player, string input, string message)
        {
            int number;
            Character sender = API.shared.getEntityData(player.handle, "Character");

            if (sender.Phone == Phone.None)
            {
                API.sendChatMessageToPlayer(player, "You don't have a phone.");
                return;
            }

            if (sender.Phone.IsOn == false)
            {
                API.sendChatMessageToPlayer(player, "Your phone is turned off.");
                return;
            }

            var canConvert = int.TryParse(input, out number);// this so i can check if player put a number or a contact name
            if (canConvert) // this is if player puts a number
            {
                if (!DoesNumberExist(number))
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The text message failed to send. (Phone number is not registered.");
                    return;
                }

                var character = PlayerManager.Players.Find(c => c.Phone.Number == number);

                if (character == null)
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The text message failed to send. ((No one online found with that phone number))");
                    return;
                }


                string fromMsg;
                string toMsg;

                if (sender.Phone.HasContactWithNumber(number))
                {
                    toMsg = "SMS to " + sender.Phone.Contacts.Find(pc => pc.Number == number).Name + ": " +
                            message;
                }
                else
                {
                    toMsg = "SMS to " + number + ": " +
                            message;
                }

                if (character.Phone.HasContactWithNumber(sender.Phone.Number))
                {
                    fromMsg = "SMS from " +
                                character.Phone.Contacts.Find(pc => pc.Number == sender.Phone.Number).Name + ": " +
                                message;
                }
                else
                {
                    fromMsg = "SMS from " + sender.Phone.Number + ": " + message;
                }


                API.sendChatMessageToPlayer(character.Client, Color.Sms, fromMsg);
                API.sendChatMessageToPlayer(sender.Client, Color.Sms, toMsg);

                ChatManager.AmeLabelMessage(player, "presses a few buttons on their phone, sending a message.", 4000);
                ChatManager.RoleplayMessage(character, "'s phone vibrates..", ChatManager.RoleplayMe);

                Phone.LogMessage(sender.Phone.Number, character.Phone.Number, message);
            }
            else
            {
                if (!sender.Phone.HasContactWithName(input))
                {
                    API.sendChatMessageToPlayer(player, Color.White, "You do not have a contact with the name: " + input);
                    return;
                }

                var contact = sender.Phone.Contacts.Find(pc => string.Equals(pc.Name, input, StringComparison.OrdinalIgnoreCase));
                var character = PlayerManager.Players.Find(c => c.Phone.Number == contact.Number);

                if (character == null)
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "The text message failed to send. ((No one online found with that phone number))");
                    return;
                }

                string fromMsg;

                if (character.Phone.HasContactWithNumber(sender.Phone.Number))
                {
                    fromMsg = "SMS from " + character.Phone.Contacts.Find(pc => pc.Number == sender.Phone.Number).Name +
                              ": " + message;
                }
                else
                {
                    fromMsg = "SMS from " + sender.Phone.Number + ": " + number;
                }

                var toMsg = "SMS to " + contact.Name + ": " + message;

                API.sendChatMessageToPlayer(character.Client, Color.Sms, fromMsg);
                API.sendChatMessageToPlayer(sender.Client, Color.Sms, toMsg);

                ChatManager.AmeLabelMessage(player, "presses a few buttons on their phone, sending a message.", 4000);
                ChatManager.RoleplayMessage(character, "'s phone vibrates..", ChatManager.RoleplayMe);

                Phone.LogMessage(sender.Phone.Number, character.Phone.Number, message);
            }
        }

        [Command("phone")]
        public void ShowPhone(Client player)
        {
            API.triggerClientEvent(player, "phone_showphone");
        }

        public static Phone GetPhoneByNumber(int number)
        {
            var phoneFilter = Builders<Phone>.Filter.Eq("Number", number);
            var foundPhones = DatabaseManager.PhoneTable.Find(phoneFilter).ToList();
            return foundPhones.Count > 0 ? foundPhones[0] : Phone.None;
        }

        public bool DoesNumberExist(long num)
        {
            var filter = Builders<Phone>.Filter.Eq("Number", num);

            return DatabaseManager.PhoneTable.Find(filter).Count() > 0;
        }
    }
}

