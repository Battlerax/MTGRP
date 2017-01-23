using MongoDB.Bson;
using MongoDB.Driver;
using RoleplayServer.resources.core;
using RoleplayServer.resources.job_manager;
using RoleplayServer.resources.player_manager;
using RoleplayServer.resources.vehicle_manager;

namespace RoleplayServer.resources.database_manager
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
       
        public static void DatabaseManagerInit()
        {
            DebugManager.DebugMessage("[DatabaseM] Initalizing database manager...");

            _database = MongoClient.GetDatabase("mtg_test");

            _countersTable = _database.GetCollection<BsonDocument>("counters");
            VehicleTable = _database.GetCollection<Vehicle>("vehicles");
            AccountTable = _database.GetCollection<Account>("accounts");
            CharacterTable = _database.GetCollection<Character>("characters");
            JobTable = _database.GetCollection<Job>("jobs");
           
            DebugManager.DebugMessage("[DatabaseM] Database Manager initalized!");
        }

        public static int GetNextId(string tableName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", tableName);
            var update = Builders<BsonDocument>.Update.Inc("sequence", 1);
            var result = _countersTable.FindOneAndUpdate(filter, update, new FindOneAndUpdateOptions<BsonDocument> { IsUpsert = true });

            return result.GetValue("sequence").ToInt32();
        }

    }
}