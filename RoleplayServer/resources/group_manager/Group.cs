using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;
using RoleplayServer.resources.player_manager;
using MongoDB.Bson.Serialization.Attributes;

namespace RoleplayServer.resources.group_manager
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
        public Dictionary<string, Tuple<Vector3, Vector3, VehicleHash, int, int>> groupVehicles = new Dictionary<string, Tuple<Vector3, Vector3, VehicleHash, int, int>>();


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
        public bool LockerSet { get; set; }
        public MarkerZone FrontDesk { get; set; }
        public MarkerZone Locker { get; set; }
        public MarkerZone ArrestLocation { get; set; }

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
