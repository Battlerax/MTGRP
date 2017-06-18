using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.player_manager;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace mtgvrp.group_manager
{
    public class Group
    {
        public static readonly int CommandTypeLspd = 1;
        public static readonly int CommandTypeLsnn = 2;

        public static readonly Group None = new Group();

        [BsonId]
        public int Id { get; set; }

        public string Name { get; set; }
        public int Type { get; set; }
        public int CommandType { get; set; }

        public string Motd { get; set; }

        public List<string> RankNames = new List<string> { "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10" };
        public List<string> Divisions = new List<string> { "D1", "D2", "D3", "D4", "D5" };
       
        public List<List<string>> DivisionRanks = new List<List<string>>
        {
            new List<string> {"DR1", "DR2", "DR3", "DR4", "DR5"},
            new List<string> {"DR1", "DR2", "DR3", "DR4", "DR5"},
            new List<string> {"DR1", "DR2", "DR3", "DR4", "DR5"},
            new List<string> {"DR1", "DR2", "DR3", "DR4", "DR5"},
            new List<string> {"DR1", "DR2", "DR3", "DR4", "DR5"},
        };

        public DateTime DisbandDate { get; set; }

        //Group misc
        public int LottoSafe { get; set; }
        public int LottoPrice { get; set; }
        public int FactionPaycheckBonus { get; set; }
        public bool LockerSet { get; set; }
        public MarkerZone FrontDesk { get; set; }
        public MarkerZone Locker { get; set; }
        public MarkerZone ArrestLocation { get; set; }

        public int MapIconId { get; set; }
        public Vector3 MapIconPos { get; set; }
        public string MapIconText { get; set; }
        [BsonIgnore]
        public Blip MapIcon { get; set; }

        public Group()
        {
            Id = 0;
            Name = "None";
            Type = 0;
            CommandType = 0;
            Motd = "Welcome To Group";


            Locker = MarkerZone.None;
            ArrestLocation = MarkerZone.None;
            FrontDesk = MarkerZone.None;

            MapIconId = 0;
            MapIconPos = new Vector3();
            MapIconText = string.Empty;
            FactionPaycheckBonus = 0;
        }

        public void Insert()
        {
            Id = DatabaseManager.GetNextId("groups");
            DatabaseManager.GroupTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = Builders<Group>.Filter.Eq("Id", Id);
            DatabaseManager.GroupTable.ReplaceOne(filter, this);
        }

        public void UpdateMapIcon()
        {
            if (MapIcon == null && MapIconId != 0)
            {
                MapIcon = API.shared.createBlip(MapIconPos);
                API.shared.setBlipSprite(MapIcon, MapIconId);
                API.shared.setBlipName(MapIcon, MapIconText);
            }
            else if(MapIcon != null)
            {
                API.shared.setBlipPosition(MapIcon, MapIconPos);
                API.shared.setBlipSprite(MapIcon, MapIconId);
                API.shared.setBlipName(MapIcon, MapIconText);
            }
        }

        public void register_markerzones()
        {
            //Create locker.
            Locker.Create();

            if (Locker != MarkerZone.None)
            {

                Locker.ColZone.onEntityEnterColShape += (shape, entity) =>
                {
                    if (API.shared.getEntityType(entity) != EntityType.Player)
                    {
                        return;
                    }
                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.Client != entity) { continue; }
                        c.LockerZoneGroup = this;
                    }
                };
                Locker.ColZone.onEntityExitColShape += (shape, entity) =>
                {
                    if (API.shared.getEntityType(entity) != EntityType.Player)
                    {
                        return;
                    }
                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.Client != entity)
                        {
                            continue;
                        }
                        c.LockerZoneGroup = Group.None;
                    }
                };
            }

            FrontDesk.Create();

            if (FrontDesk != MarkerZone.None)
            {

                FrontDesk.ColZone.onEntityEnterColShape += (shape, entity) =>
                {
                    if (API.shared.getEntityType(entity) != EntityType.Player)
                    {
                        return;
                    }
                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.Client != entity) { continue; }
                    }
                };
                FrontDesk.ColZone.onEntityExitColShape += (shape, entity) =>
                {
                    if (API.shared.getEntityType(entity) != EntityType.Player)
                    {
                        return;
                    }
                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.Client != entity)
                        {
                            continue;
                        }
                    }
                };
            }

            ArrestLocation.Create();

            if (ArrestLocation != MarkerZone.None)
            {

                ArrestLocation.ColZone.onEntityEnterColShape += (shape, entity) =>
                {
                    if (API.shared.getEntityType(entity) != EntityType.Player)
                    {
                        return;
                    }
                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.Client != entity) { continue; }
                    }
                };
                ArrestLocation.ColZone.onEntityExitColShape += (shape, entity) =>
                {
                    if (API.shared.getEntityType(entity) != EntityType.Player)
                    {
                        return;
                    }
                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.Client != entity)
                        {
                            continue;
                        }
                    }
                };
            }
        }

    }
}
