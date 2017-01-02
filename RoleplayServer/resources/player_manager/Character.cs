using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace RoleplayServer
{
    public class Character
    {
        public int _id { get; set; }
        public int account_id { get; set; }

        public string character_name { get; set; }
       
        public Vector3 last_pos { get; set; }
        public Vector3 last_rot { get; set; }

        public int money { get; set; }
        public int bank_balance { get; set; }

        public PedHash skin { get; set; }

        public List<Vehicle> owned_vehicles = new List<Vehicle>();
        
        public Client client { get; set; }

        public Character()
        {

            character_name = "Default_Name";
           
            last_pos = new Vector3(0.0, 0.0, 0.0);
            last_rot = new Vector3(0.0, 0.0, 0.0);

            money = 0;
            bank_balance = 0;

            skin = PedHash.FreemodeMale01;

        }

        public void insert()
        {
            this._id = DatabaseManager.getNextId("characters");
            DatabaseManager.character_table.InsertOne(this);
        }

        public void save()
        {
            FilterDefinition<Character> filter = Builders<Character>.Filter.Eq("_id", this._id);
            DatabaseManager.character_table.ReplaceOneAsync(filter, this);
        }

        public static bool IsNameRegistered(string name)
        {
            FilterDefinition<Account> filter = Builders<Account>.Filter.Eq("account_name", name);

            if (DatabaseManager.account_table.Find(filter).Count() > 0)
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
