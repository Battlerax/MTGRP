using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.phone_manager
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
