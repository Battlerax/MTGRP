using System;
using System.Collections.Generic;
using System.Linq;
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

            DebugManager.DebugMessage("[PhoneM] Phone Manager initalized.");
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

        /*  [Command("call")]
          public void call_cmd(Client player, string number)
          {
              if (DoesNumberExist(number))
              {
                  Character caller = API.shared.getEntityData(player.handle, "Character");
                  FilterDefinition<Phone> filter = Builders<Phone>.Filter.Eq("Number", number);
                  List<Phone> found_phones = DatabaseManager.PhoneTable.Find(filter).ToList<Phone>();
                  foreach (Phone phone in found_phones)
                  {
                      if (character.Id = phone.Id)
                      {
                          if (phone.Status == 1)
                          {
                              ChatManager.RoleplayMessage(character, "'s phone starts to ring...", ChatManager.ROLEPLAY_ME, 10);
                              ChatManager.RoleplayMessage(caller, "takes out their phone and presses a few numbers.", ChatManager.ROLEPLAY_ME, 10);

                              // the rest will be done when chenko adds phone vars in Players data
                          }
                      }
                  }
              }
              else
              {
                  API.sendChatMessageToPlayer(player, "The number you are trying to reach is unavailable.");
                  return;
              }
          }*/

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

        [Command("addcontact", GreedyArg = true)]
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
        }

        [Command("removecontact", GreedyArg = true)]
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

                var contact = c.Phone.Contacts.Single(pc => string.Equals(pc.Name, name, StringComparison.OrdinalIgnoreCase));
                API.sendChatMessageToPlayer(player, Color.White, "You have deleted " + name + " (" + contact.Number + ") from your contacts.");
                c.Phone.DeleteContact(contact);
            }
        }



        [Command("listcontacts", GreedyArg = true)]
        public void listcontacts_cmd(Client player)
        {
            API.sendChatMessageToPlayer(player, "---------------------------- PHONE CONTACTS ----------------------------");

            Character character = API.getEntityData(player.handle, "Character");

            if (character.Phone == Phone.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You do not have a phone!");
                return;
            }

            foreach (var pc in character.Phone.Contacts)
            {
                API.sendChatMessageToPlayer(player, pc.Name + " - " + pc.Number);
            }

            API.sendChatMessageToPlayer(player, "---------------------------------------------------------------------------------- ");
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
            }
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


