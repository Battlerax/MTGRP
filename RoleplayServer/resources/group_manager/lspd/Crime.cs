using System.Collections.Generic;
using MongoDB.Bson;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.group_manager.lspd
{
    public class Crime
    {
        public static List<Crime> Crimes = new List<Crime>();

        public ObjectId Id { get; set; }

        public int Level { get; set; }
        public string Name { get; set; }
        public int JailTime { get; set; }
        public int Fine { get; set; }

        public Crime(int level, string name, int jailTime, int fine)
        {
            Level = level;
            Name = name;
            JailTime = JailTime;
            Fine = fine;
            Crimes.Add(this);
        }

        public void Update()
        {
            var filter = MongoDB.Driver.Builders<Crime>.Filter.Eq("Id", Id);
            DatabaseManager.CrimeTable.ReplaceOne(filter, this);
        }

        public void Insert()
        {
            DatabaseManager.CrimeTable.InsertOne(this);
        }

        public void Delete()
        {
            var filter = MongoDB.Driver.Builders<Crime>.Filter.Eq("Id", Id);
            DatabaseManager.CrimeTable.DeleteOne(filter);
        }
    }
}
