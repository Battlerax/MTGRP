using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.group_manager;
using mtgvrp.group_manager.lspd;
using mtgvrp.inventory;
using mtgvrp.job_manager;
using mtgvrp.job_manager.fisher;
using mtgvrp.job_manager.taxi;
using mtgvrp.vehicle_manager;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Vehicle = mtgvrp.vehicle_manager.Vehicle;

namespace mtgvrp.player_manager
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

        public int BankBalance { get; set; }

        public PedHash Skin { get; set; }
        public bool HasSkin { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }

        public List<int> Outfit = new List<int>();
        public List<int> OutfitVariation = new List<int>();

        public int Age { get; set; }
        public string Birthday { get; set; }
        public string Birthplace { get; set; }

        public string PlayerCrimes { get; set; }

        [BsonIgnore]
        public Client Client { get; set; }

        [BsonIgnore]
        public Vehicle LastVehicle { get; set; }

        [BsonIgnore]
        public bool TaxiDuty = false;

        //Reports
        public bool IsOnAsk { get; set; }
        public bool HasActiveAsk { get; set; }
        public bool HasActiveReport { get; set; }
        public bool ReportCreated { get; set; }

        [BsonIgnore]
        public Timer ReportTimer { get; set; }
        public double ReportMuteExpires { get; set; }

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

        //Chat mutes
        public double NMutedExpiration { get; set; }
        public double VMutedExpiration { get; set; }
        [BsonIgnore] public Timer NMutedTimer { get; set; }
        [BsonIgnore] public Timer VMutedTimer { get; set; }

        //Job zone related
        [BsonIgnore]
        public int JobZone { get; set; }

        [BsonIgnore]
        public int JobZoneType { get; set; }

        //Garbage Related

        [BsonIgnore]
        public DateTime CanPickupTrash { get; set; }
        [BsonIgnore]
        public bool IsOnGarbageRun { get; set; }
        [BsonIgnore] public Timer GarbageTimeLeftTimer { get; set; }
        private int _garbagetime;

        public int GarbageTimeLeft
        {
            get { return _garbagetime; }
            set
            {
                if (Client != null)
                    API.shared.triggerClientEvent(Client, "update_garbage_time", value / 1000);

                _garbagetime = value;
            }
        }

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
        public DateTime NextFishTime { get; set; }

        [BsonIgnore]
        public Timer CatchTimer { get; set; }

        [BsonIgnore]
        public Fish CatchingFish { get; set; }

        [BsonIgnore]
        public int PerfectCatchStrength { get; set; }

        public Dictionary<Fish, int> FishOnHand = new Dictionary<Fish, int>();

        public int TrasureFound { get; set; }
        public double CanScuba { get; set; }
        //Mechanic related
        public double FixcarPrevention { get; set; }

        //Phone
        [BsonIgnore]
        public Character InCallWith { get; set; }
        [BsonIgnore]
        public Character BeingCalledBy { get; set; }
        [BsonIgnore]
        public Character CallingPlayer { get; set; }
        [BsonIgnore]
        public System.Threading.Timer CallingTimer;

        //Dropcar
        public bool IsOnDropcar { get; set; }
        public double DropcarReset { get; set; }

        [BsonIgnore]
        public bool Calling911 { get; set; }

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
        public bool IsOnPoliceDuty { get; set; }
        public string BadgeNumber { get; set; }
        public Vector3 BeaconPosition { get; set; }
        public bool BeaconSet { get; set; }
        [BsonIgnore] public Timer BeaconTimer { get; set; }
        [BsonIgnore] public Timer BeaconResetTimer { get; set; }
        public Client BeaconCreator{ get; set; }

        [BsonIgnore]
        public bool IsViewingMdc { get; set; }

        private int _time;

        [BsonIgnore] public Timer JailTimeLeftTimer { get; set; }
        [BsonIgnore] public Timer JailTimer { get; set; }
        public bool IsJailed { get; set; }
        public int SentTicketAmount { get; set; }

        public int JailTimeLeft
        {
            get { return _time; }
            set
            {
                if (Client != null)
                    API.shared.triggerClientEvent(Client, "update_jail_time", value/1000);

                _time = value;
            }
        }

        [BsonIgnore] public Timer TicketTimer { get; set; }
        public bool SentTicket { get; set; }
        public int TicketBalance { get; set; }
        public int UnpaidTickets { get; set; }
        public bool RadioToggle { get; set; }

        //LSNN Related
        public bool IsWatchingBroadcast { get; set; }
        public bool HasMic { get; set; }
        public bool HasLottoTicket { get; set; }
        public bool HasCamera { get; set; }
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
        public bool CanDoAnim { get; set; }

        [BsonIgnore]
        public int MaxInvStorage => 100; //TODO: change this later on.

        [BsonIgnore]
        public bool IsTied;

        [BsonIgnore]
        public bool IsBlindfolded;

        [BsonIgnore]
        public bool IsRagged;

        [BsonIgnore]
        public DateTime NextHotWire;

        [BsonIgnore]
        public GrandTheftMultiplayer.Server.Elements.Object MegaPhoneObject = null;

        [BsonIgnore]
        public GrandTheftMultiplayer.Server.Elements.Object MicObject = null;

        [BsonIgnore]
        public bool IsScubaDiving = false;

        [BsonIgnore]
        public GrandTheftMultiplayer.Server.Elements.Object GarbageBag = null;

        public enum TruckingStages
        {
            None,
            GettingTrailer,
            HeadingForWoodSupplies,
            HeadingForFuelSupplies,
            DeliveringWood,
            DeliveringFuel,
            HeadingBack
        }
        [BsonIgnore]
        public TruckingStages TruckingStage = TruckingStages.None;

        //Hunting Related
        public DateTime LastRedeemedDeerTag;
        public DateTime LastRedeemedBoarTag;

        //DMV
        [BsonIgnore] public DateTime TimeStartedDmvTest;
        [BsonIgnore] public bool IsInDmvTest;

        [BsonIgnore]
        public int AfkTimer;

        public Character()
        {
            Id = 0;
            AccountId = "none";
            IsCreated = false;
            CharacterName = "Default_Name";

            LastPos = new Vector3(0.0, 0.0, 0.0);
            LastRot = new Vector3(0.0, 0.0, 0.0);

            BankBalance = 0;

            Skin = PedHash.FreemodeMale01;

            Client = null;

            LastVehicle = null;

            JobOne = Job.None;

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

            Health = 100;
            Armor = 0;
            RadioToggle = true;
            CanDoAnim = true;

            IsOnGarbageRun = false;
        }

        public void Insert()
        {
            Task.Run(() =>
            {
                Id = DatabaseManager.GetNextId("characters");
                DatabaseManager.CharacterTable.InsertOne(this);
            });
        }

        public void Save()
        {

            Health = API.shared.getPlayerHealth(Client);
            Armor = API.shared.getPlayerArmor(Client);
            Skin = (PedHash) Client.model;
            LastPos = Client.position;
            LastRot = Client.rotation;
            GetTimePlayed(); //Update time played before save.

            var task = Task.Run(() =>
            {
                LogManager.Log(LogManager.LogTypes.Connection,
                    $"Trying to save character {this.CharacterName} with ID {Id}.");

                var filter = Builders<Character>.Filter.Eq("_id", Id);
                var res = DatabaseManager.CharacterTable.ReplaceOne(filter, this);
                if (!res.IsAcknowledged || (res.IsModifiedCountAvailable && res.ModifiedCount == 0))
                {
                    LogManager.Log(LogManager.LogTypes.Connection,
                        $"Character {this.CharacterName} ERRORED while saving.");
                }
            });

            task.ContinueWith(
                a => LogManager.Log(LogManager.LogTypes.Connection,
                    $"The character {CharacterName} was saved sucessfully."),
                TaskContinuationOptions.OnlyOnRanToCompletion);

            task.ContinueWith(
                a => LogManager.Log(LogManager.LogTypes.Connection,
                    $"The character {CharacterName} was NOT saved sucessfully. {a.Exception?.Flatten().ToString()}"),
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public List<Vehicle> OwnedVehicles => VehicleManager.Vehicles.Where(x => x.OwnerId == Id).ToList();

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
            update_ped(Client);
            foreach (var p in API.shared.getAllPlayers())
            {
                if (p == null)
                    return;

                if (p.position.DistanceTo(Client.position) <= 500f)
                {
                    update_ped(p);
                }
            }
        }

        public void update_ped(Client player)
        {
            API.shared.sendNativeToPlayer(player, Hash.SET_PED_HEAD_BLEND_DATA, Client.handle, Model.FatherId,
                Model.MotherId, 0, Model.FatherId, Model.MotherId, 0, Model.ParentLean, Model.ParentLean, 0, false);

            API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 2, Model.HairStyle, 0, 0);
            API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 2, Model.HairStyle, 0, 0);
            API.shared.sendNativeToPlayer(player, Hash._SET_PED_HAIR_COLOR, Client.handle, Model.HairColor);

            //API.shared.sendNativeToPlayer(player, Hash._SET_PED_EYE_COLOR, client.handle, this.model.eye_color);

            API.shared.sendNativeToPlayer(player, Hash.SET_PED_HEAD_OVERLAY, Client.handle, 2, Model.Eyebrows, 1.0f);
            API.shared.sendNativeToPlayer(player, Hash._SET_PED_HEAD_OVERLAY_COLOR, Client.handle, 2, 1, Model.HairColor,
                Model.HairColor);

            API.shared.sendNativeToPlayer(player, Hash.SET_PED_HEAD_OVERLAY, Client.handle, 0, Model.Blemishes, 1.0f);

            API.shared.sendNativeToPlayer(player, Hash.SET_PED_HEAD_OVERLAY, Client.handle, 1, Model.FacialHair, 1.0f);
            API.shared.sendNativeToPlayer(player, Hash._SET_PED_HEAD_OVERLAY_COLOR, Client.handle, 1, 1, Model.HairColor,
                Model.HairColor);

            API.shared.sendNativeToPlayer(player, Hash.SET_PED_HEAD_OVERLAY, Client.handle, 3, Model.Ageing, 1.0f);

            API.shared.sendNativeToPlayer(player, Hash.SET_PED_HEAD_OVERLAY, Client.handle, 8, Model.Lipstick, 1.0f);
            API.shared.sendNativeToPlayer(player, Hash._SET_PED_HEAD_OVERLAY_COLOR, Client.handle, 8, 2, Model.LipstickColor,
                Model.LipstickColor);

            API.shared.sendNativeToPlayer(player, Hash.SET_PED_HEAD_OVERLAY, Client.handle, 4, Model.Makeup, 1.0f);
            API.shared.sendNativeToPlayer(player, Hash._SET_PED_HEAD_OVERLAY_COLOR, Client.handle, 4, 0, Model.MakeupColor,
                Model.MakeupColor);

            API.shared.sendNativeToPlayer(player, Hash.SET_PED_HEAD_OVERLAY, Client.handle, 5, Model.Blush, 1.0f);
            API.shared.sendNativeToPlayer(player, Hash._SET_PED_HEAD_OVERLAY_COLOR, Client.handle, 5, 2, Model.BlushColor,
                Model.BlushColor);

            API.shared.sendNativeToPlayer(player, Hash.SET_PED_HEAD_OVERLAY, Client.handle, 6, Model.Complexion, 1.0f);

            API.shared.sendNativeToPlayer(player, Hash.SET_PED_HEAD_OVERLAY, Client.handle, 7, Model.SunDamage, 1.0f);

            API.shared.sendNativeToPlayer(player, Hash.SET_PED_HEAD_OVERLAY, Client.handle, 9, Model.MolesFreckles, 1.0f);

            if (IsOnGarbageRun == true)
            {
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 4, Model.Gender == GenderMale ? 36 : 35, 0, 0); //Garbage pants
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 8, Model.Gender == GenderMale ? 59 : 36, 0, 0); //Garbage vest
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 11, Model.Gender == GenderMale ? 56 : 49, 0, 0); //Garbage shirt

                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 6, Model.ShoeStyle, Model.ShoeVar - 1, 0); // Shoes
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 7, Model.AccessoryStyle, Model.AccessoryVar - 1, 0); // Accessories
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 3, 0, 0, 0); //Torso


                API.shared.sendNativeToPlayer(player, Hash.SET_PED_PROP_INDEX, Client.handle, 1, 0, 0, true);
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_PROP_INDEX, Client.handle, 2, Model.EarStyle,
                    Model.Gender == GenderMale ? 33 : 0, 0, true);
            }
            else if(IsInPoliceUniform == true)
            {
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 4, Model.Gender == GenderMale ? 35 : 34, 0, 0); // Pants
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 6, 24, 0, 0); // Shoes
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 7, 0, 0, 0); // Accessories
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 8, Model.Gender == GenderMale ? 58 : 35, 0, 0); //undershirt
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 11, Model.Gender == GenderMale ? 55 : 48, 0, 0); //top
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 3, 0, 0, 0); //Torso

                //API.shared.setPlayerAccessory(client, 0, this.model.hat_style, this.model.hat_var - 1); // hats
                //API.shared.setPlayerAccessory(client, 1, this.model.glasses_style, this.model.glasses_var - 1); // glasses
                //API.shared.setPlayerAccessory(client, 2, this.model.ear_style, this.model.ear_var - 1); // earings

                //Work around until setPlayerAccessory is fixed.
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_PROP_INDEX, Client.handle, 0,
                    Model.Gender == GenderMale ? 46 : 45, 0, true);
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_PROP_INDEX, Client.handle, 1, 0, 0, true);
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_PROP_INDEX, Client.handle, 2, Model.EarStyle,
                    Model.Gender == GenderMale ? 33 : 0, 0, true);
            }
            else if (HasSkin)
            {
                API.shared.setPlayerSkin(Client, Skin);
            }
            else
            {
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 4, Model.PantsStyle, Model.PantsVar - 1, 0); // Pants
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 6, Model.ShoeStyle, Model.ShoeVar - 1, 0); // Shoes
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 7, Model.AccessoryStyle, Model.AccessoryVar - 1, 0); // Accessories
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 8, Model.UndershirtStyle, Model.UndershirtVar - 1, 0); //undershirt
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 11, Model.TopStyle, Model.TopVar - 1, 0); //top
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_COMPONENT_VARIATION, Client.handle, 3, Model.TorsoStyle, Model.TorsoVar, 0); //Torso

                //API.shared.setPlayerAccessory(client, 0, this.model.hat_style, this.model.hat_var - 1); // hats
                //API.shared.setPlayerAccessory(client, 1, this.model.glasses_style, this.model.glasses_var - 1); // glasses
                //API.shared.setPlayerAccessory(client, 2, this.model.ear_style, this.model.ear_var - 1); // earings

                //Work around until setPlayerAccessory is fixed.
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_PROP_INDEX, Client.handle, 0, Model.HatStyle,
                    Model.HatVar - 1, true);
                API.shared.sendNativeToPlayer(player, Hash.SET_PED_PROP_INDEX, Client.handle, 1, Model.GlassesStyle,
                    Model.GlassesVar - 1, true);

                if (Model.EarStyle == 255)
                    API.shared.sendNativeToPlayer(player, Hash.CLEAR_PED_PROP, Client.handle, 2);
                else
                    API.shared.sendNativeToPlayer(player, Hash.SET_PED_PROP_INDEX, Client.handle, 2, Model.EarStyle,
                    Model.EarVar - 1, true);
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
            if(TimeLoggedIn > 0)
            {
                TimePlayed += new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - TimeLoggedIn;
                StartTrackingTimePlayed();
            }
            return TimePlayed;
        }

        public int GetPlayingHours()
        {
            if(TimeLoggedIn > 0)
            {
                TimePlayed += new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() - TimeLoggedIn;
                StartTrackingTimePlayed();
            }
            return (int) TimePlayed / 3600;
        }

        public string rp_name()
        {
            return CharacterName.Replace("_", " ");
        }

        //Criminal Records
        public void RecordCrime(string recordingOfficerId, Crime crime)
        {
            var record = new CriminalRecord(this.Id.ToString(), recordingOfficerId, crime, true);
            record.Insert();
        }

        public List<CriminalRecord> GetCriminalRecord(int amountToSkip = 0)
        {
            var filter = Builders<CriminalRecord>.Filter.Eq("CharacterId", Id.ToString());
            return DatabaseManager.CriminalRecordTable.Find(filter).SortByDescending(x => x.DateTime).Skip(amountToSkip).Limit(10).ToList();
        }

        public long GetCrimesNumber()
        {
            var filter = Builders<CriminalRecord>.Filter.Eq("CharacterId", Id.ToString());
            return DatabaseManager.CriminalRecordTable.Find(filter).Count();
        }

        public int HasActiveCriminalRecord()
        {
            var crimesList = GetCriminalRecord();
            return crimesList.FindAll(c => c.ActiveCrime == true).Count;
        }
    }
}
