using System;
using System.Collections.Generic;
using System.Linq;
using mtgvrp.database_manager;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace mtgvrp.group_manager.lspd
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

        public void Delete()
        {
            Crimes.Remove(this);
            var filter = Builders<Crime>.Filter.Eq("Id", Id);
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

        public static void LoadCrimes()
        {
            Crimes = new List<Crime>();

            foreach (var crime in DatabaseManager.CrimeTable.Find(FilterDefinition<Crime>.Empty).ToList())
            {
                Crimes.Add(crime);
            }
            
        }

        public static void UpdateCrimes()
        {
            foreach(var crime in DatabaseManager.CrimeTable.Find(FilterDefinition<Crime>.Empty).ToList())
            {
                var filter = Builders<Crime>.Filter.Eq("Id", crime.Id);
                DatabaseManager.CrimeTable.DeleteOne(filter);
            }

            foreach(var setCrime in Crimes)
            {
                var InsertCrime = new Crime(setCrime.Type, setCrime.Name, setCrime.JailTime, setCrime.Fine) { Id = DatabaseManager.GetNextId("crimes") };
                InsertCrime.Insert();
            }
        }
    }
}
