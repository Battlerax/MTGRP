using System.Collections.Generic;
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

            //API.onPlayerConnected += OnPlayerConnected;

            DebugManager.DebugMessage("[PhoneM] Phone Manager initalized.");
        }

        [Command("setphonename", GreedyArg = true)]
        public void setphonename_cmd(Client player, string name)
        {
            Character owner = API.shared.getEntityData(player.handle, "Character");
            var filter = Builders<Phone>.Filter.Eq("Number", owner.PhoneNumber);
            var foundPhones = DatabaseManager.PhoneTable.Find(filter).ToList();
            foreach (var phone in foundPhones)
            {
                phone.PhoneName = name;
                phone.Save();
                API.sendChatMessageToPlayer(player, "You have changed your phone name to " + name + ".");
            }
        }

        /* [Command("togphone")]
         public void togphone_cmd(Client player)
         {
             Character owner = API.getEntityData(player.handle, "Character");

             var filter = Builders<Phone>   
                 filter = Builders<Phone>.Filter.Eq("Id", owner.Id);
             List<Phone> found_phones = DatabaseManager.PhoneTable.Find(filter).ToList<Phone>();
             foreach (Phone phone in found_phones)
             {
                 if (phone.status == 1)
                 {
                     ChatManager.RoleplayMessage(character, "turned their phone off.", ChatManager.ROLEPLAY_ME, 10);
                     phone.status = 0;
                     phone.Save();
                 }
                 else if (phone.status == 0)
                 {
                     ChatManager.RoleplayMessage(character, "turned their phone on.", ChatManager.ROLEPLAY_ME, 10);
                     phone.status = 1;
                 }

             }
         }

         [Command("call")]
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

        public bool DoesNumberExist(long num)
        {
            var filter = Builders<Phone>.Filter.Eq("Number", num);

            if (DatabaseManager.PhoneTable.Find(filter).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [Command("setphonenumber")]
        public void setphone_cmd(Client player, string id, int number)
        {
            var receiver = PlayerManager.ParseClient(id);
            Character rec = API.getEntityData(receiver.handle, "Character");

            var phone = new Phone();

            phone.Number = number;
            phone.Status = 1;
            phone.Insert();

            rec.PhoneNumber = number;
            rec.Save();
            API.sendChatMessageToPlayer(player, "You have changed " + rec.CharacterName + "'s phone number to " + number + ".");
        }

        [Command("addcontact", GreedyArg = true)]
        public void addcontact_cmd(Client player, int number, string name)
        {
            Character c = API.getEntityData(player.handle, "Character");
            var pc = new PhoneContact();
            pc.PhoneId = GetNumberSqlid(c.PhoneNumber);
            pc.ContactName = name;
            pc.ContactNumber = number;
            pc.Insert();
        }

        [Command("removecontact", GreedyArg = true)]
        public void removecontact_cmd(Client player, string name)
        {
            Character c = API.getEntityData(player.handle, "Character");

            if (name == "all")
            {
                var filter0 = Builders<PhoneContact>.Filter.Eq("PhoneId", GetNumberSqlid(c.PhoneNumber));
                DatabaseManager.ContactTable.DeleteMany(filter0);
                API.sendChatMessageToPlayer(player, "You have deleted all your contacts.");
                return;
            }

            var builder = Builders<PhoneContact>.Filter;
            var filter1 = builder.Eq("ContactName", name) & builder.Eq("PhoneId", GetNumberSqlid(c.PhoneNumber));
            DatabaseManager.ContactTable.DeleteOne(filter1);

            API.sendChatMessageToPlayer(player, "You have removed " + name + " from your contacts.");

        }



        [Command("listcontacts", GreedyArg = true)]
        public void listcontacts_cmd(Client player)
        {
            API.sendChatMessageToPlayer(player, "---------------------------- PHONE CONTACTS ----------------------------");
            Character c = API.getEntityData(player.handle, "Character");
            API.sendChatMessageToPlayer(player, "SQLID: " + GetNumberSqlid(c.PhoneNumber));
            var filter = Builders<PhoneContact>.Filter.Eq("PhoneId", GetNumberSqlid(c.PhoneNumber));
            var foundContacts = DatabaseManager.ContactTable.Find(filter).ToList();
            foreach (var pc in foundContacts)
            {
                API.sendChatMessageToPlayer(player, pc.ContactName + " - " + pc.ContactNumber);

            }
            API.sendChatMessageToPlayer(player, "---------------------------------------------------------------------------------- ");
        }

        public int GetNumberSqlid(int number)
        {
            var filter = Builders<Phone>.Filter.Eq("Number", number);
            var foundPhones = DatabaseManager.PhoneTable.Find(filter).ToList();
            foreach (var p in foundPhones)
            {
                return p.Id;
            }
            return 0;
        }

        [Command("sms", GreedyArg = true)]
        public void sms_cmd(Client player, string num, string message)
        {
            long number;
            Character sender = API.shared.getEntityData(player.handle, "Character");
            var canConvert = long.TryParse(num, out number);// this so i can check if player put a number or a contact name
            if (canConvert) // this is if player puts a number
            {
                if (DoesNumberExist(number))
                {

                    var filter = Builders<Phone>.Filter.Eq("Number", number);
                    var foundPhones = DatabaseManager.PhoneTable.Find(filter).ToList();
                    foreach (var phone in foundPhones)
                    {
                        foreach (var character in PlayerManager.Players)
                        {
                            if (character.PhoneNumber == phone.Number)
                            {
                                if (phone.Status == 1)
                                {

                                    ChatManager.AmeLabelMessage(player, "presses a few buttons on their phone, sending a message.", 4000);
                                    ChatManager.RoleplayMessage(character, "'s phone vibrates..", ChatManager.RoleplayMe);
                                    var str1 = "SMS From " + phone.Number + ": " + message;
                                    API.sendChatMessageToPlayer(character.Client, Color.Sms, str1);
                                    var str2 = "SMS To " + number + ": " + message;
                                    API.sendChatMessageToPlayer(sender.Client, Color.Sms, str2);
                                }
                            }
                        }
                    }
                }
            }
            /* else
             {
                 var builder = Builders<PhoneContact>.Filter;
                 var filter = builder.Eq("ContactName", num) & &builder.Eq("PhoneId", character.Id);
                 List<PhoneContact> found_phone_contacts = DatabaseManager.ContactTable.Find(filter).ToList<PhoneContact>();
                 foreach (PhoneContact pc in found_phone_contacts)
                 {
                     foreach (Character reciever in PlayerManager.Players)
                     {
                         if (reciever.Id == pc.PhoneId)
                         {
                             if (pc. == 1)
                             {
                                 ChatManager.RoleplayMessage(character, "'s phone vibrates..", ChatManager.ROLEPLAY_ME, 10);
                                 FilterDefinition<Phone> filter = Builders<Phone>.Filter.Eq("Id", character.Id);
                                 Phone phone = DatabaseManager.PhoneTable.Find(filter);
                                 string str1 = "SMS From" + phone.Number + ": " + message;
                                 API.sendChatMessageToPlayer(character.Client, str1);
                                 ChatManager.RoleplayMessage(sender, "presses a few buttons on their phone, sending a message.", ChatManager.ROLEPLAY_ME, 10);
                                 string str2 = "SMS To " + number + ": " + message;
                                 API.sendChatMessageToPlayer(sender.Client, str2);
                             }
                         }
                     }

                 }
             }*/
        }
    }
}



