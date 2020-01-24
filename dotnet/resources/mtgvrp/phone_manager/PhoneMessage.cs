using mtgvrp.database_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.phone_manager
{
    public class PhoneMessage
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string SenderNumber { get; set; }
        public string ToNumber { get; set; }
        public string Message { get; set; }
        public int DateSent { get; set; }
        public bool IsRead { get; set; }

        public void Insert()
        {
            DatabaseManager.MessagesTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = MongoDB.Driver.Builders<PhoneMessage>.Filter.Eq("Id", Id);
            DatabaseManager.MessagesTable.ReplaceOne(filter, this);
        }
    }
}
