using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.group_manager.lspd
{
    public class Crime
    {
        public static List<Crime> Crimes = new List<Crime>();

        [BsonId]
        public int Id { get; set; }

        public string Type { get; set; }
        public string Name { get; set; }
        public int JailTime { get; set; }
        public int Fine { get; set; }

        public Crime(string type, string name, int jailTime, int fine)
        {
            Type = type;
            Name = name;
            JailTime = jailTime;
            Fine = fine;
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

        public static void Delete(Crime name)
        {

            Crimes.Remove(name);
            var filter = MongoDB.Driver.Builders<Crime>.Filter.Eq("Id", name.Id);
            DatabaseManager.CrimeTable.DeleteOne(filter);
        }

        public static void InsertCrime(string type, string name, int jailTime, int fine)
        {
            var crime = new Crime(type, name, jailTime, fine) {Id = DatabaseManager.GetNextId("crimes")};
            crime.Insert();
            Crimes.Add(crime);
        }

        public static bool CrimeExists(string crimeName)
        {
            return Crimes.Count(i => string.Equals(i.Name, crimeName, StringComparison.OrdinalIgnoreCase)) > 0;
        }
    }
}
