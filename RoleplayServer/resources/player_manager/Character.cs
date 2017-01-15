using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;

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

        public int money { get; set; }
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

        public void update_ped(NetHandle handle)
        {
            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_BLEND_DATA, handle, this.model.father_id, this.model.mother_id, 0, this.model.father_id, this.model.mother_id, 0, this.model.parent_lean, this.model.parent_lean, 0, false);

            API.shared.setPlayerClothes(client, 2, this.model.hair_style, 0);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HAIR_COLOR, handle, this.model.hair_color);
            
            //API.shared.sendNativeToAllPlayers(Hash._SET_PED_EYE_COLOR, handle, this.model.eye_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, handle, 2, this.model.eyebrows, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, handle, 2, 1, this.model.hair_color, this.model.hair_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, handle, 0, this.model.blemishes, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, handle, 1, this.model.facial_hair, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, handle, 1, 1, this.model.hair_color, this.model.hair_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, handle, 3, this.model.ageing, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, handle, 8, this.model.lipstick, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, handle, 8, 2, this.model.lipstick_color, this.model.lipstick_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, handle, 4, this.model.makeup, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, handle, 4, 0, this.model.makeup_color, this.model.makeup_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, handle, 5, this.model.blush, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, handle, 5, 2, this.model.blush_color, this.model.blush_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, handle, 6, this.model.complexion, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, handle, 7, this.model.sun_damage, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, handle, 9, this.model.moles_freckles, 1.0f);


            API.shared.setPlayerClothes(client, 1, 0, 0);
            API.shared.setPlayerClothes(client, 4, this.model.pants_style, this.model.pants_var - 1); // Pants
            API.shared.setPlayerClothes(client, 6, this.model.shoe_style, this.model.shoe_var - 1); // Shoes
            API.shared.setPlayerClothes(client, 7, this.model.accessory_style, this.model.accessory_var - 1); // Accessories
            API.shared.setPlayerClothes(client, 8, this.model.undershirt_style, this.model.undershirt_var - 1); //undershirt
            API.shared.setPlayerClothes(client, 11, this.model.top_style, this.model.top_var - 1); //top

            API.shared.setPlayerAccessory(client, 0, this.model.hat_style, this.model.hat_var - 1); // hats
            //API.shared.setPlayerAccessory(client, 1, this.model.glasses_style, this.model.glasses_var - 1); // glasses
            API.shared.setPlayerAccessory(client, 2, this.model.ear_style, this.model.ear_var - 1); // earings
       
        }

        public void update_nametag()
        {
            API.shared.setPlayerNametag(this.client, this.character_name + " (" + PlayerManager.getPlayerId(this) + ")");
        }
    }
}
