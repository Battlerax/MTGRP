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

        public static ObjectId Id { get; set; }

        public int Level { get; set; }
        public string Name { get; set; }
        public int JailTime { get; set; }
        public int Fine { get; set; }

        public Crime(int level, string name, int jailTime, int fine)
        {
            Level = level;
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
            var filter = MongoDB.Driver.Builders<Crime>.Filter.Eq("Id", Id);
            DatabaseManager.CrimeTable.DeleteOne(filter);
        }

        public static void InsertCrime(int level, string name, int jailTime, int fine)
        {
            var crime = new Crime(level, name, jailTime, fine);
            crime.Insert();
            Crimes.Add(crime);
        }

        public static bool CrimeExists(string crimeName)
        {
            return Crimes.Count(i => string.Equals(i.Name, crimeName, StringComparison.OrdinalIgnoreCase)) > 0;
        }
    }
}
