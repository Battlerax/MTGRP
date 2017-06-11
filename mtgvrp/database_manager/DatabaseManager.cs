using MongoDB.Bson;
using MongoDB.Driver;
using RoleplayServer.core;
using RoleplayServer.door_manager;
using RoleplayServer.group_manager;
using RoleplayServer.group_manager.lspd;
using RoleplayServer.job_manager;
using RoleplayServer.phone_manager;
using RoleplayServer.player_manager;
using RoleplayServer.property_system;
using RoleplayServer.vehicle_manager;

namespace RoleplayServer.database_manager
{
    public static class DatabaseManager
    {
        private static readonly IMongoClient MongoClient = new MongoClient("mongodb://localhost");
        private static IMongoDatabase _database;

        private static IMongoCollection<BsonDocument> _countersTable;
        public static IMongoCollection<Vehicle> VehicleTable;
        public static IMongoCollection<Account> AccountTable; 
        public static IMongoCollection<Character> CharacterTable;
        public static IMongoCollection<Job> JobTable;
        public static IMongoCollection<PhoneNumber> PhoneNumbersTable;
        public static IMongoCollection<PhoneContact> ContactTable;
        public static IMongoCollection<PhoneMessage> MessagesTable;
        public static IMongoCollection<Group> GroupTable;
        public static IMongoCollection<Property> PropertyTable;
        public static IMongoCollection<Door> DoorsTable;

        public static IMongoCollection<Crime> CrimeTable;
        public static IMongoCollection<CriminalRecord> CriminalRecordTable;


        public static void DatabaseManagerInit()
        {
            DebugManager.DebugMessage("[DatabaseM] Initalizing database manager...");

            _database = MongoClient.GetDatabase("mtg_test");

            _countersTable = _database.GetCollection<BsonDocument>("counters");
            VehicleTable = _database.GetCollection<Vehicle>("vehicles");
            AccountTable = _database.GetCollection<Account>("accounts");
            CharacterTable = _database.GetCollection<Character>("characters");
            JobTable = _database.GetCollection<Job>("jobs");
            PhoneNumbersTable = _database.GetCollection<PhoneNumber>("phonenumbers");
            ContactTable = _database.GetCollection<PhoneContact>("phonecontacts");
            MessagesTable = _database.GetCollection<PhoneMessage>("phonemessages");
            GroupTable = _database.GetCollection<Group>("groups");
            PropertyTable = _database.GetCollection<Property>("properties");
            DoorsTable = _database.GetCollection<Door>("doors");

            CrimeTable = _database.GetCollection<Crime>("crimes");
            CriminalRecordTable = _database.GetCollection<CriminalRecord>("criminalrecords");

            DebugManager.DebugMessage("[DatabaseM] Database Manager initalized!");
        }

        public static int GetNextId(string tableName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", tableName);
            var update = Builders<BsonDocument>.Update.Inc("sequence", 1);
            var result = _countersTable.FindOneAndUpdate(filter, update, new FindOneAndUpdateOptions<BsonDocument> { IsUpsert = true }) ??
                         _countersTable.FindOneAndUpdate(filter, update, new FindOneAndUpdateOptions<BsonDocument> { IsUpsert = true }); //Not sure why do I need to do this lol but it works.
            return result.GetValue("sequence").ToInt32();
        }
    }
}