using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.player_manager
{
    public class Account
    {
        public ObjectId Id { get; set; }

        public string AccountName { get; set; }
        public int AdminLevel { get; set; }
        public string AdminName { get; set; }
        public string AdminPin { get; set; }
        public bool AdminDuty { get; set; }
        public int DevLevel { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }

        public int VipLevel { get; set; }
        public DateTime VipExpirationDate { get; set; }

        public string LastIp { get; set; }

        [BsonIgnore]
        public bool IsLoggedIn { get; set; }
        public bool IsSpectating { get; set; }

        public Account()
        {
            AccountName = "default_account";
            AdminLevel = 0;
            AdminPin = string.Empty;
            Password = string.Empty;
            Salt = string.Empty;
        }

        public void load_by_name()
        {
            var filter = Builders<Account>.Filter.Eq("AccountName", AccountName);
            var foundAccount = DatabaseManager.AccountTable.Find(filter).ToList();

            foreach(var a in foundAccount)
            {
                Id = a.Id;
                AdminLevel = a.AdminLevel;
                AdminName = a.AdminName;
                AdminPin = a.AdminPin;
                AdminDuty = a.AdminDuty;
                DevLevel = a.DevLevel;
                Password = a.Password;
                Salt = a.Salt;

                VipLevel = a.VipLevel;
                VipExpirationDate = a.VipExpirationDate;

                LastIp = a.LastIp;

                break;
            }
        }

        public void Register()
        {
            DatabaseManager.AccountTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = Builders<Account>.Filter.Eq("_id", Id);
            DatabaseManager.AccountTable.ReplaceOneAsync(filter, this, new UpdateOptions { IsUpsert = true });
        }

        public bool is_registered()
        {
            var filter = Builders<Account>.Filter.Eq("AccountName", AccountName);

            if(DatabaseManager.AccountTable.Find(filter).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
