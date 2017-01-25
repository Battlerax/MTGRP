using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.phone_manager
{
    public class Phone
    {
        public static readonly Phone None = new Phone();

        public ObjectId Id { get; set; }
        public int Number { get; set; }
        public bool IsOn { get; set; }
        public string PhoneName { get; set; }

        [BsonIgnore]
        public List<PhoneContact> Contacts = new List<PhoneContact>();

        public Phone()
        {
            Number = 0;
            IsOn = true;
            PhoneName = "Phone";
        }

        public void Insert()
        {
            DatabaseManager.PhoneTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = Builders<Phone>.Filter.Eq("Id", Id);
            DatabaseManager.PhoneTable.ReplaceOneAsync(filter, this);
        }

        /* ============== CONTACTS ================ */

        public void InsertContact(string name, int number)
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
            var filter = Builders<PhoneContact>.Filter.Eq("PhoneId", Id.ToString());
            Contacts = DatabaseManager.ContactTable.Find(filter).ToList();
        }

        public bool HasContactWithName(string name)
        {
            return Contacts.Count(pc => pc.Name == name) > 0;
        }

        public bool HasContactWithNumber(int number)
        {
            return Contacts.Count(pc => pc.Number == number) > 0;
        }

        public bool HasContact(string name, int number)
        {
            return Contacts.Count(pc => pc.Number == number || pc.Name == name) > 0;
        }


        /* ============== TEXT MESSAGES =============*/

        /* ============== PHONE CALLS ==============*/
    }
}