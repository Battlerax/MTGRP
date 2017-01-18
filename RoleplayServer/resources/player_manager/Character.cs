using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System.Timers;

namespace RoleplayServer
{
    public class Character
    {
        [BsonIgnore]
        public static int GENDER_MALE = 0;
        [BsonIgnore]
        public static int GENDER_FEMALE = 1;

        public int _id { get; set; }
        public string account_id { get; set; }

        public string character_name { get; set; }
        public bool is_created { get; set; }
       
        public Model model = new Model();

        public Vector3 last_pos { get; set; }
        public Vector3 last_rot { get; set; }
        public int last_dimension { get; set; }

        private int Money = 0;
        public int money
        {
            get
            {
                return Money;
            }
            set
            {
                if (client != null)
                    API.shared.triggerClientEvent(client, "update_money_display", value);

                Money = value;
            }
        }
        

        public int bank_balance { get; set; }

        public PedHash skin { get; set; }

        public List<Vehicle> owned_vehicles = new List<Vehicle>();

        public List<int> outfit = new List<int>();
        public List<int> outfit_variation = new List<int>();

        public int age { get; set; }
        public string birthday { get; set; }
        public string birthplace { get; set; }

        [BsonIgnore]
        public Client client { get; set; }

        //Jobs
        public int job_one_id { get; set; }
        [BsonIgnore]
        public Job job_one { get; set; }

        //Playing time
        [BsonIgnore]
        private long time_logged_in { get; set; }
        public long time_played { get; set; }

        //AME 
        [BsonIgnore]
        public NetHandle ame_text { get; set; }
        [BsonIgnore]
        public Timer ame_timer { get; set; }

        //Chat cooldowns
        [BsonIgnore]
        public long newbie_cooldown { get; set; }
        [BsonIgnore]
        public long ooc_cooldown { get; set; }
        
        //Job zone related
        [BsonIgnore]
        public int job_zone { get; set; }
        [BsonIgnore]
        public int job_zone_type { get; set; }

        //Taxi Related
        [BsonIgnore]
        public Character taxi_passenger { get; set; }
        [BsonIgnore]
        public Character taxi_driver { get; set; }
        public int taxi_fare { get; set; }

        [BsonIgnore]
        public Vector3 taxi_start { get; set; }
        [BsonIgnore]
        public int total_fare { get; set; }
        [BsonIgnore]
        public Timer taxi_timer { get; set; }

        public Character()
        {
            _id = 0;
            account_id = "none";
            is_created = false;
            character_name = "Default_Name";
           
            last_pos = new Vector3(0.0, 0.0, 0.0);
            last_rot = new Vector3(0.0, 0.0, 0.0);

            money = 0;
            bank_balance = 0;

            skin = PedHash.FreemodeMale01;

            client = null;

            taxi_fare = TaxiJob.MIN_FARE;
            taxi_passenger = null;
            taxi_driver = null;
        }

        public void insert()
        {
            _id = DatabaseManager.getNextId("characters");
            DatabaseManager.character_table.InsertOne(this);
        }

        public void save()
        {
            FilterDefinition<Character> filter = Builders<Character>.Filter.Eq("_id", _id);
            DatabaseManager.character_table.ReplaceOne(filter, this);
        }

        public static bool IsCharacterRegistered(string name)
        {
            FilterDefinition<Character> filter = Builders<Character>.Filter.Eq("character_name", name);

            if (DatabaseManager.character_table.Find(filter).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void update_ped()
        {
            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_BLEND_DATA, client.handle, this.model.father_id, this.model.mother_id, 0, this.model.father_id, this.model.mother_id, 0, this.model.parent_lean, this.model.parent_lean, 0, false);

            API.shared.setPlayerClothes(client, 2, this.model.hair_style, 0);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HAIR_COLOR, client.handle, this.model.hair_color);
            
            //API.shared.sendNativeToAllPlayers(Hash._SET_PED_EYE_COLOR, client.handle, this.model.eye_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, client.handle, 2, this.model.eyebrows, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, client.handle, 2, 1, this.model.hair_color, this.model.hair_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, client.handle, 0, this.model.blemishes, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, client.handle, 1, this.model.facial_hair, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, client.handle, 1, 1, this.model.hair_color, this.model.hair_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, client.handle, 3, this.model.ageing, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, client.handle, 8, this.model.lipstick, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, client.handle, 8, 2, this.model.lipstick_color, this.model.lipstick_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, client.handle, 4, this.model.makeup, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, client.handle, 4, 0, this.model.makeup_color, this.model.makeup_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, client.handle, 5, this.model.blush, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, client.handle, 5, 2, this.model.blush_color, this.model.blush_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, client.handle, 6, this.model.complexion, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, client.handle, 7, this.model.sun_damage, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, client.handle, 9, this.model.moles_freckles, 1.0f);

            API.shared.setPlayerClothes(client, 4, this.model.pants_style, this.model.pants_var - 1); // Pants
            API.shared.setPlayerClothes(client, 6, this.model.shoe_style, this.model.shoe_var - 1); // Shoes
            API.shared.setPlayerClothes(client, 7, this.model.accessory_style, this.model.accessory_var - 1); // Accessories
            API.shared.setPlayerClothes(client, 8, this.model.undershirt_style, this.model.undershirt_var - 1); //undershirt
            API.shared.setPlayerClothes(client, 11, this.model.top_style, this.model.top_var - 1); //top

            //API.shared.setPlayerAccessory(client, 0, this.model.hat_style, this.model.hat_var - 1); // hats
            //API.shared.setPlayerAccessory(client, 1, this.model.glasses_style, this.model.glasses_var - 1); // glasses
            //API.shared.setPlayerAccessory(client, 2, this.model.ear_style, this.model.ear_var - 1); // earings

            //Work around until setPlayerAccessory is fixed.
            API.shared.sendNativeToAllPlayers(Hash.SET_PED_PROP_INDEX, client.handle, 0, this.model.hat_style, this.model.hat_var - 1, true);
            API.shared.sendNativeToAllPlayers(Hash.SET_PED_PROP_INDEX, client.handle, 1, this.model.glasses_style, this.model.glasses_var - 1, true);
            API.shared.sendNativeToAllPlayers(Hash.SET_PED_PROP_INDEX, client.handle, 2, this.model.ear_style, this.model.ear_var - 1, true);

        }

        public void update_nametag()
        {
            API.shared.setPlayerNametag(this.client, this.character_name + " (" + PlayerManager.getPlayerId(this) + ")");
        }

        public void startTrackingTimePlayed()
        {
            time_logged_in = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        }

        public long getTimePlayed()
        {
            this.time_played += new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - time_logged_in;
            return this.time_played;
        }

        public int getPlayingHours()
        {
            this.time_played += new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - time_logged_in;
            return (int)this.time_played / 3600;
        }

        public string rp_name()
        {
            return this.character_name.Replace("_", " ");
        }
    }
}
