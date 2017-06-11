using System;
using MongoDB.Bson;
using RoleplayServer.database_manager;
using System.Collections.Generic;
using MongoDB.Driver;

namespace RoleplayServer.group_manager.lspd
{
    public class CriminalRecord
    {

        public ObjectId Id { get; set; }

        public string CharacterId { get; set; }
        public string OfficerId { get; set; }
        public Crime Crime { get; set; }
        public DateTime DateTime { get; set; }
        public bool ActiveCrime { get; set; }

        public CriminalRecord(string characterId, string arrestingOfficerId, Crime crime, bool activecrime)
        {
            CharacterId = characterId;
            OfficerId = arrestingOfficerId;
            Crime = crime;
            DateTime = DateTime.Now;
            ActiveCrime = activecrime;
        }

        public void Insert()
        {
            DatabaseManager.CriminalRecordTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = Builders<CriminalRecord>.Filter.Eq("_id", Id);
            DatabaseManager.CriminalRecordTable.ReplaceOne(filter, this);
        }
    }
}
