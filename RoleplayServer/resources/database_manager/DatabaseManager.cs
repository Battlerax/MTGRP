using System;
using MongoDB.Driver;
using MongoDB.Bson;


namespace RoleplayServer
{
    public static class DatabaseManager
    {
        private static IMongoClient mongo_client = new MongoClient("mongodb://localhost");
        private static IMongoDatabase database = null;

        private static IMongoCollection<BsonDocument> counters_table = null;
        public static IMongoCollection<Vehicle> vehicle_table = null;
        public static IMongoCollection<Account> account_table = null; 
        public static IMongoCollection<Character> character_table = null;
        public static IMongoCollection<Component> component_table = null;
       
        public static void DatabaseManagerInit()
        {
            DebugManager.debugMessage("[DatabaseM] Initalizing database manager...");

            database = mongo_client.GetDatabase("test_db");

            counters_table = database.GetCollection<BsonDocument>("counters");
            vehicle_table = database.GetCollection<Vehicle>("vehicles");
            account_table = database.GetCollection<Account>("accounts");
            character_table = database.GetCollection<Character>("characters");
            component_table = database.GetCollection<Component>("components");
           
            DebugManager.debugMessage("[DatabaseM] Database Manager initalized!");
        }

        public static int getNextId(string table_name)
        {
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", table_name);
            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Inc("sequence", 1);
            var result = counters_table.FindOneAndUpdate(filter, update, new FindOneAndUpdateOptions<BsonDocument> { IsUpsert = true });

            return result.GetValue("sequence").ToInt32();
        }

    }
}