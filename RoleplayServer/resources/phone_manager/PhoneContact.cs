using MongoDB.Driver;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.phone_manager
{
    public class PhoneContact
    {
        public int Id { get; set; }
        public string ContactName { get; set; }
        public int ContactNumber { get; set; }
        public int PhoneId { get; set; }

        public PhoneContact()
        {
            Id = 0;
            ContactName = "unnamed";
            ContactNumber = 0;
            PhoneId = 0;
        }

        public void Insert()
        {
            Id = DatabaseManager.GetNextId("contactphones");
            DatabaseManager.ContactTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = Builders<PhoneContact>.Filter.Eq("Id", Id);
            DatabaseManager.ContactTable.ReplaceOne(filter, this);
        }

    }
}
