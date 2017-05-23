using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RoleplayServer.resources.database_manager;
using RoleplayServer.resources.group_manager;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.job_manager;
using RoleplayServer.resources.job_manager.fisher;
using RoleplayServer.resources.job_manager.taxi;
using RoleplayServer.resources.phone_manager;
using Vehicle = RoleplayServer.resources.vehicle_manager.Vehicle;

namespace RoleplayServer.resources.player_manager
{
    public class Character : IStorage
    {
        public static readonly Character None = new Character();

        [BsonIgnore]
        public static int GenderMale = 0;
        [BsonIgnore]
        public static int GenderFemale = 1;

        public int Id { get; set; }
        public string AccountId { get; set; }

        public string CharacterName { get; set; }
        public bool IsCreated { get; set; }

        public Model Model = new Model();

        public Vector3 LastPos { get; set; }
        public Vector3 LastRot { get; set; }
        public int LastDimension { get; set; }

        private int _money;

        public int Money
        {
            get { return _money; }
            set
            {
                if (Client != null)
                    API.shared.triggerClientEvent(Client, "update_money_display", value);

                _money = value;
            }
        }


        public int BankBalance { get; set; }

        public PedHash Skin { get; set; }

        public List<int> OwnedVehicles = new List<int>();

        public List<int> Outfit = new List<int>();
        public List<int> OutfitVariation = new List<int>();

        public int Age { get; set; }
        public string Birthday { get; set; }
        public string Birthplace { get; set; }

        public string playerCrimes{ get; set; }

        [BsonIgnore]
        public Client Client { get; set; }

        public Vehicle LastVehicle { get; set; }

        //Jobs
        public int JobOneId { get; set; }

        [BsonIgnore]
        public Job JobOne { get; set; }

        //Playing time
        [BsonIgnore]
        private long TimeLoggedIn { get; set; }

        public long TimePlayed { get; set; }

        //AME 
        [BsonIgnore]
        public NetHandle AmeText { get; set; }

        [BsonIgnore]
        public Timer AmeTimer { get; set; }

        //Chat cooldowns
        [BsonIgnore]
        public long NewbieCooldown { get; set; }

        [BsonIgnore]
        public long OocCooldown { get; set; }

        //Job zone related
        [BsonIgnore]
        public int JobZone { get; set; }

        [BsonIgnore]
        public int JobZoneType { get; set; }

        //Taxi Related
        [BsonIgnore]
        public Character TaxiPassenger { get; set; }

        [BsonIgnore]
        public Character TaxiDriver { get; set; }

        public int TaxiFare { get; set; }

        [BsonIgnore]
        public Vector3 TaxiStart { get; set; }

        [BsonIgnore]
        public int TotalFare { get; set; }

        [BsonIgnore]
        public Timer TaxiTimer { get; set; }

        //Fisherman Related
        [BsonIgnore]
        public bool IsInFishingZone { get; set; }

        [BsonIgnore]
        public Timer CatchTimer { get; set; }

        [BsonIgnore]
        public Fish CatchingFish { get; set; }

        [BsonIgnore]
        public int PerfectCatchStrength { get; set; }

        public Dictionary<Fish, int> FishOnHand = new Dictionary<Fish, int>();

        //Phone System
        public int PhoneNumber { get; set; }

        [BsonIgnore]
        public Phone Phone { get; set; }
        [BsonIgnore]
        public Character InCallWith { get; set; }
        [BsonIgnore]
        public Character BeingCalledBy { get; set; }
        [BsonIgnore]
        public Character CallingPlayer { get; set; }

        //Groups
        public int GroupId { get; set; }
        public int GroupRank { get; set; }
        public int Division { get; set; }
        public int DivisionRank { get; set; }

        [BsonIgnore]
        public Group Group { get; set; }

        [BsonIgnore]
        public Group LockerZoneGroup { get; set; }

        //LSPD Related
        public bool IsInPoliceUniform { get; set; }

        //Player Interaction
        [BsonIgnore]
        public Character FollowingPlayer { get; set; }

        [BsonIgnore]
        public bool IsBeingDragged { get; set; }

        [BsonIgnore]
        public Timer FollowingTimer { get; set; }

        [BsonIgnore]
        public bool AreHandsUp { get; set; }

        public bool IsCuffed { get; set; }

        public List<IInventoryItem> Inventory { get; set; }

        [BsonIgnore]
        public int MaxInvStorage => 100; //TODO: change this later on.

        public Character()
        {
            Id = 0;
            AccountId = "none";
            IsCreated = false;
            CharacterName = "Default_Name";

            LastPos = new Vector3(0.0, 0.0, 0.0);
            LastRot = new Vector3(0.0, 0.0, 0.0);

            Money = 0;
            BankBalance = 0;

            Skin = PedHash.FreemodeMale01;

            Client = null;

            LastVehicle = null;

            TaxiFare = TaxiJob.MinFare;
            TaxiPassenger = null;
            TaxiDriver = null;

            IsInFishingZone = false;
            CatchingFish = Fish.None;

            Group = Group.None;

            FollowingPlayer = Character.None;

            InCallWith = Character.None;
            BeingCalledBy = Character.None;
            CallingPlayer = Character.None;
        }

        public void Insert()
        {
            Id = DatabaseManager.GetNextId("characters");
            DatabaseManager.CharacterTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = Builders<Character>.Filter.Eq("_id", Id);
            DatabaseManager.CharacterTable.ReplaceOne(filter, this);
        }

        public static bool IsCharacterRegistered(string name)
        {
            var filter = Builders<Character>.Filter.Eq("CharacterName", name);

            if (DatabaseManager.CharacterTable.Find(filter).Count() > 0)
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
            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_BLEND_DATA, Client.handle, Model.FatherId,
                Model.MotherId, 0, Model.FatherId, Model.MotherId, 0, Model.ParentLean, Model.ParentLean, 0, false);

            API.shared.setPlayerClothes(Client, 2, Model.HairStyle, 0);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HAIR_COLOR, Client.handle, Model.HairColor);

            //API.shared.sendNativeToAllPlayers(Hash._SET_PED_EYE_COLOR, client.handle, this.model.eye_color);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, Client.handle, 2, Model.Eyebrows, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, Client.handle, 2, 1, Model.HairColor,
                Model.HairColor);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, Client.handle, 0, Model.Blemishes, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, Client.handle, 1, Model.FacialHair, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, Client.handle, 1, 1, Model.HairColor,
                Model.HairColor);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, Client.handle, 3, Model.Ageing, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, Client.handle, 8, Model.Lipstick, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, Client.handle, 8, 2, Model.LipstickColor,
                Model.LipstickColor);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, Client.handle, 4, Model.Makeup, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, Client.handle, 4, 0, Model.MakeupColor,
                Model.MakeupColor);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, Client.handle, 5, Model.Blush, 1.0f);
            API.shared.sendNativeToAllPlayers(Hash._SET_PED_HEAD_OVERLAY_COLOR, Client.handle, 5, 2, Model.BlushColor,
                Model.BlushColor);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, Client.handle, 6, Model.Complexion, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, Client.handle, 7, Model.SunDamage, 1.0f);

            API.shared.sendNativeToAllPlayers(Hash.SET_PED_HEAD_OVERLAY, Client.handle, 9, Model.MolesFreckles, 1.0f);

            if (IsInPoliceUniform == false)
            {
                API.shared.setPlayerClothes(Client, 4, Model.PantsStyle, Model.PantsVar - 1); // Pants
                API.shared.setPlayerClothes(Client, 6, Model.ShoeStyle, Model.ShoeVar - 1); // Shoes
                API.shared.setPlayerClothes(Client, 7, Model.AccessoryStyle, Model.AccessoryVar - 1); // Accessories
                API.shared.setPlayerClothes(Client, 8, Model.UndershirtStyle, Model.UndershirtVar - 1); //undershirt
                API.shared.setPlayerClothes(Client, 11, Model.TopStyle, Model.TopVar - 1); //top

                //API.shared.setPlayerAccessory(client, 0, this.model.hat_style, this.model.hat_var - 1); // hats
                //API.shared.setPlayerAccessory(client, 1, this.model.glasses_style, this.model.glasses_var - 1); // glasses
                //API.shared.setPlayerAccessory(client, 2, this.model.ear_style, this.model.ear_var - 1); // earings

                //Work around until setPlayerAccessory is fixed.
                API.shared.sendNativeToAllPlayers(Hash.SET_PED_PROP_INDEX, Client.handle, 0, Model.HatStyle,
                    Model.HatVar - 1, true);
                API.shared.sendNativeToAllPlayers(Hash.SET_PED_PROP_INDEX, Client.handle, 1, Model.GlassesStyle,
                    Model.GlassesVar - 1, true);
                API.shared.sendNativeToAllPlayers(Hash.SET_PED_PROP_INDEX, Client.handle, 2, Model.EarStyle,
                    Model.EarVar - 1, true);
            }
            else
            {
                API.shared.setPlayerClothes(Client, 4, Model.Gender == GenderMale ? 35 : 34, 0); // Pants
                API.shared.setPlayerClothes(Client, 6, 24, 0); // Shoes
                API.shared.setPlayerClothes(Client, 7, 0, 0); // Accessories
                API.shared.setPlayerClothes(Client, 8, Model.Gender == GenderMale ? 58 : 35, 0); //undershirt
                API.shared.setPlayerClothes(Client, 11, Model.Gender == GenderMale ? 55 : 48, 0); //top

                //API.shared.setPlayerAccessory(client, 0, this.model.hat_style, this.model.hat_var - 1); // hats
                //API.shared.setPlayerAccessory(client, 1, this.model.glasses_style, this.model.glasses_var - 1); // glasses
                //API.shared.setPlayerAccessory(client, 2, this.model.ear_style, this.model.ear_var - 1); // earings

                //Work around until setPlayerAccessory is fixed.
                API.shared.sendNativeToAllPlayers(Hash.SET_PED_PROP_INDEX, Client.handle, 0,
                    Model.Gender == GenderMale ? 46 : 45, 0, true);
                API.shared.sendNativeToAllPlayers(Hash.SET_PED_PROP_INDEX, Client.handle, 1, 0, 0, true);
                API.shared.sendNativeToAllPlayers(Hash.SET_PED_PROP_INDEX, Client.handle, 2, Model.EarStyle,
                    Model.Gender == GenderMale ? 33 : 0, 0, true);
            }
        }

        public void update_nametag()
        {
            API.shared.setPlayerNametag(Client, CharacterName + " (" + PlayerManager.GetPlayerId(this) + ")");
        }

        public void StartTrackingTimePlayed()
        {
            TimeLoggedIn = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        }

        public long GetTimePlayed()
        {
            TimePlayed += new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - TimeLoggedIn;
            return TimePlayed;
        }

        public int GetPlayingHours()
        {
            TimePlayed += new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - TimeLoggedIn;
            return (int) TimePlayed / 3600;
        }

        public string rp_name()
        {
            return CharacterName.Replace("_", " ");
        }
    }
}
