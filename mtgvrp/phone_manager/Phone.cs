using System;
using System.Collections.Generic;
using System.Linq;
using mtgvrp.database_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace mtgvrp.phone_manager
{
    public class Phone : IInventoryItem
    {
        #region InvRelated Stuff

        [BsonId]
        public ObjectId Id { get; set; }

        public int Amount { get; set; }

        public int AmountOfSlots => 25;

        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStacked => false;
        public bool CanBeStashed => true;
        public bool CanBeStored => true;

        public Dictionary<Type, int> MaxAmount
        {
            get
            {
                var itm = new Dictionary<Type, int> {{typeof(Character), 1}};
                return itm;
            }
        }

        public string CommandFriendlyName => "phone_" + PhoneName;
        public string LongName => "Phone (" + PhoneName + ")";
        public int Object => 0;

        #endregion

        public string PhoneNumber { get; set; }
        public bool IsOn { get; set; }
        public string PhoneName { get; set; }

        [BsonIgnore]
        public List<PhoneContact> Contacts = new List<PhoneContact>();

        public Phone()
        {
            PhoneNumber = "0";
            IsOn = true;
            PhoneName = "Phone";
        }

        public void InsertNumber()
        {
            PhoneNumber number = new PhoneNumber()
            {
                Number = PhoneNumber,
                PhoneId = Id
            };
            DatabaseManager.PhoneNumbersTable.InsertOne(number);
        }

        public void SaveNumber()
        {
            PhoneNumber number = new PhoneNumber()
            {
                Number = PhoneNumber,
                PhoneId = Id
            };
            DatabaseManager.PhoneNumbersTable.ReplaceOneAsync(y => y.PhoneId == Id, number);
        }

        /* ============== CONTACTS ================ */

        public void InsertContact(string name, string number)
        {
            var contact = new PhoneContact
            {
                Name = name,
                Number = number,
                PhoneId = Id.ToString()
            };

            contact.Insert();
            Contacts.Add(contact);
        }

        public void DeleteContact(PhoneContact contact)
        {
            Contacts.Remove(contact);
            var filter = Builders<PhoneContact>.Filter.Eq("Id", contact.Id);
            DatabaseManager.ContactTable.DeleteOneAsync(filter);
        }

        public void DeleteAllContacts()
        {
            Contacts.Clear();
            var filter = Builders<PhoneContact>.Filter.Eq("PhoneId", Id.ToString());
            DatabaseManager.ContactTable.DeleteManyAsync(filter);
        }

        public void LoadContacts()
        {
            Contacts = new List<PhoneContact>();

            var filter = Builders<PhoneContact>.Filter.Eq("PhoneId", Id.ToString());
            Contacts = DatabaseManager.ContactTable.Find(filter).ToList();
        }

        public bool HasContactWithName(string name)
        {
            return Contacts.Count(pc => string.Equals(pc.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;
        }

        public bool HasContactWithNumber(string number)
        {
            return Contacts.Count(pc => pc.Number == number) > 0;
        }

        public bool HasContact(string name, string number)
        {
            return Contacts.Count(pc => pc.Number == number || string.Equals(pc.Name, name, StringComparison.OrdinalIgnoreCase)) > 0;
        }


        /* ============== TEXT MESSAGES =============*/
        //NOTE: These are static methods and are not linked to a phone basically.. for performance.

        public static long GetMessageCount(string contact1, string contact2)
        {
            var filter = (Builders<PhoneMessage>.Filter.Eq(x => x.SenderNumber, contact1) & Builders<PhoneMessage>.Filter.Eq(x => x.ToNumber, contact2)) | (Builders<PhoneMessage>.Filter.Eq(x => x.SenderNumber, contact2) & Builders<PhoneMessage>.Filter.Eq(x => x.ToNumber, contact1));
            return DatabaseManager.MessagesTable.Find(filter).Count();
        }

        public static void LogMessage(string from, string to, string message)
        {
            var msg = new PhoneMessage()
            {
                Message = message,
                DateSent = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
                SenderNumber = from,
                ToNumber = to,
                IsRead = false
            };
            msg.Insert();
        }

        public static List<PhoneMessage> GetMessageLog(string contact1, string contact2, int limit = 20, int toBeSkipped = 0)
        {
            var filter = (Builders<PhoneMessage>.Filter.Eq(x => x.SenderNumber, contact1) & Builders<PhoneMessage>.Filter.Eq(x => x.ToNumber, contact2)) | (Builders<PhoneMessage>.Filter.Eq(x => x.SenderNumber, contact2) & Builders<PhoneMessage>.Filter.Eq(x => x.ToNumber, contact1));
            var sort = Builders<PhoneMessage>.Sort.Descending(x => x.DateSent);
            var messages = DatabaseManager.MessagesTable.Find(filter).Sort(sort).Skip(toBeSkipped).Limit(limit).ToList();
            return messages;
        }

        public static void MarkMessagesAsRead(string contact)
        {
            var filter = Builders<PhoneMessage>.Filter.Eq(x => x.ToNumber, contact);
            var update = Builders<PhoneMessage>.Update.Set(x => x.IsRead, true);
            DatabaseManager.MessagesTable.UpdateMany(filter, update);
        }

        public static List<string[]> GetContactListOfMessages(string number)
        {
            var filter = Builders<PhoneMessage>.Filter.Eq(x => x.ToNumber, number);
            var sort = Builders<PhoneMessage>.Sort.Descending(x => x.DateSent);
            var numbersList = DatabaseManager.MessagesTable.Find(filter).Sort(sort).Project(x => new [] { x.SenderNumber, x.Message, x.DateSent.ToString(), x.IsRead.ToString() }).ToEnumerable();
            List<string[]> numbers = new List<string[]>();
            foreach (var itm in numbersList)
            {
                var item = numbers.SingleOrDefault(x => x[0] == itm[0]);
                if (item == null)
                {
                    if (itm[3] == "False")
                        itm[3] = "1";
                    else
                        itm[3] = "0";

                    numbers.Add(itm);
                }
                else
                {
                    if (itm[3] == "False")
                        item[3] = (Convert.ToInt32(item[3]) + 1).ToString();
                }
            }
            return numbers;
        }
        /* ============== PHONE CALLS ==============*/
    }
}