using mtgvrp.core;
using mtgvrp.dmv;
using mtgvrp.door_manager;
using mtgvrp.group_manager;
using mtgvrp.group_manager.lspd;
using mtgvrp.job_manager;
using mtgvrp.job_manager.lumberjack;
using mtgvrp.phone_manager;
using mtgvrp.player_manager;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;
using MongoDB.Bson;
using MongoDB.Driver;
using mtgvrp.mapping_manager;

namespace mtgvrp.database_manager
{
    public static class DatabaseManager
    {
        private static readonly IMongoClient MongoClient = new MongoClient("mongodb://localhost");
        private static IMongoDatabase _database;

        private static IMongoCollection<BsonDocument> _countersTable;
        public static IMongoCollection<GameVehicle> VehicleTable;
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
        public static IMongoCollection<Tree> TreesTable;
        public static IMongoCollection<Mapping> MappingTable;
        public static IMongoCollection<LicensePlate> NumberPlatesTable;
        public static IMongoCollection<Container> ContainersTable;
        public static IMongoCollection<ContainerZone> ContainerZonesTable;


        public static void DatabaseManagerInit()
        {
            DebugManager.DebugMessage("[DatabaseM] Initalizing database manager...");

            _database = MongoClient.GetDatabase("mtg_main");

            _countersTable = _database.GetCollection<BsonDocument>("counters");
            VehicleTable = _database.GetCollection<GameVehicle>("vehicles");
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
            TreesTable = _database.GetCollection<Tree>("lumberjacktrees");
            MappingTable = _database.GetCollection<Mapping>("mapping");
            NumberPlatesTable = _database.GetCollection<LicensePlate>("numberplates");
            ContainersTable = _database.GetCollection<Container>("containers");
            ContainerZonesTable = _database.GetCollection<ContainerZone>("containerzones");

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
