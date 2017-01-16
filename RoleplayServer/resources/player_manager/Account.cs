using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace RoleplayServer
{
    public class Account
    {
        public ObjectId _id { get; set; }

        public string account_name { get; set; }
        public int admin_level { get; set; }
        public string admin_name { get; set; }
        public int admin_duty { get; set; }
        public int dev_level { get; set; }
        public string password { get; set; }
        public string salt { get; set; }

        public int vip_level { get; set; }
        public DateTime vip_expiration_date { get; set; }

        public string last_ip { get; set; }

        [BsonIgnore]
        public bool is_logged_in { get; set; }

        public Account()
        {
            account_name = "default_account";
            admin_level = 0;
            password = System.String.Empty;
            salt = System.String.Empty;
        }

        public void load_by_name()
        {
            FilterDefinition<Account> filter = Builders<Account>.Filter.Eq("account_name", this.account_name);
            List<Account> found_account = DatabaseManager.account_table.Find(filter).ToList<Account>();

            foreach(Account a in found_account)
            {
                this._id = a._id;
                this.admin_level = a.admin_level;
                this.admin_name = a.admin_name;
                this.admin_duty = a.admin_duty;
                this.dev_level = a.dev_level;
                this.password = a.password;
                this.salt = a.salt;

                this.vip_level = a.vip_level;
                this.vip_expiration_date = a.vip_expiration_date;

                this.last_ip = a.last_ip;

                break;
            }
        }

        public void register()
        {
            DatabaseManager.account_table.InsertOne(this);
        }

        public void save()
        {
            FilterDefinition<Account> filter = Builders<Account>.Filter.Eq("_id", this._id);
            DatabaseManager.account_table.ReplaceOneAsync(filter, this, new UpdateOptions { IsUpsert = true });
        }

        public bool is_registered()
        {
            FilterDefinition<Account> filter = Builders<Account>.Filter.Eq("account_name", this.account_name);

            if(DatabaseManager.account_table.Find(filter).Count() > 0)
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
