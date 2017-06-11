using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RoleplayServer.database_manager;

namespace RoleplayServer.phone_manager
{
    public class PhoneContact
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string PhoneId { get; set; }

        public PhoneContact()
        {
            Name = "unnamed";
            Number = "0";
            PhoneId = "None";
        }

        public void Insert()
        {
            DatabaseManager.ContactTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = Builders<PhoneContact>.Filter.Eq("Id", Id);
            DatabaseManager.ContactTable.ReplaceOne(filter, this);
        }

    }
}
