using MongoDB.Bson;
using MongoDB.Driver;

namespace RoleplayServer
{
    public class Account
    {
        public ObjectId id { get; set; }

        public string account_name { get; set; }
        public int admin_level { get; set; }
        public string password { get; set; }
        public string salt { get; set; }

        public Account()
        {
            account_name = "default_account";
            admin_level = 0;
            password = System.String.Empty;
            salt = System.String.Empty;
        }

        public void save()
        {
            FilterDefinition<Account> filter = Builders<Account>.Filter.Eq("_id", this.id);
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
