using MongoDB.Driver;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.phone_manager
{
    public class Phone
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int Status { get; set; }
        public string PhoneName { get; set; }

        public Phone()
        {
            Id = 0;
            Number = 0;
            Status = 0;
            PhoneName = "Phone";
        }

        public void Insert()
        {
            Id = DatabaseManager.GetNextId("phones");
            DatabaseManager.PhoneTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = Builders<Phone>.Filter.Eq("Id", Id);
            DatabaseManager.PhoneTable.ReplaceOne(filter, this);
        }



    }
}