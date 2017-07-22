using System;
using System.Collections.Generic;
using mtgvrp.AdminSystem;
using mtgvrp.database_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace mtgvrp.player_manager
{
    public class Account
    {
        public List<PlayerWarns> PlayerWarns = new List<PlayerWarns>();
        public ObjectId Id { get; set; }

        public string AccountName { get; set; }
        public int AdminLevel { get; set; }
        public string AdminName { get; set; }

        public bool AdminDuty { get; set; }
        public string AdminPin { get; set; }
        public int DevLevel { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }

        public int VipLevel { get; set; }
        public DateTime VipExpirationDate { get; set; }

        public int CharacterSlots { get; set; }

        public string LastIp { get; set; }

        public DateTime TempBanExpiration { get; set; }
        public bool IsTempbanned { get; set; }
        public int TempbanLevel { get; set; }
        public bool IsBanned { get; set; }
        public string BanReason { get; set; }

        public bool IsSpeedoOn { get; set; } = true;

        [BsonIgnore]
        public bool IsLoggedIn { get; set; }
        public bool IsSpectating { get; set; }

        public string DiscordCode { get; set; } = null;
        public string DiscordUser { get; set; } = null;

        public int TotalPlayingHours { get; set; }

        public Account()
        {
            AccountName = "default_account";
            AdminLevel = 0;
            AdminName = "";
            CharacterSlots = 3;
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

                CharacterSlots = a.CharacterSlots;

                LastIp = a.LastIp;

                TempbanLevel = a.TempbanLevel;
                IsBanned = a.IsBanned;
                PlayerWarns = a.PlayerWarns;
                TempBanExpiration = a.TempBanExpiration;
                IsTempbanned = a.IsTempbanned;
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
            DatabaseManager.AccountTable.ReplaceOne(filter, this);
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
