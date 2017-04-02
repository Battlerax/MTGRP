using System;
using MongoDB.Bson;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.group_manager.lspd
{
    public class CriminalRecord
    {
        public ObjectId Id { get; set; }

        public string CharacterId { get; set; }
        public string OfficerId { get; set; }
        public Crime Crime { get; set; }
        public DateTime DateTime { get; set; }
        public bool ActiveCrime { get; set; }

        public CriminalRecord(string characterId, string arrestingOfficerId, Crime crime, bool Activecrime)
        {
            CharacterId = characterId;
            OfficerId = arrestingOfficerId;
            Crime = crime;
            DateTime = DateTime.Now;
            ActiveCrime = Activecrime;
        }

        public void Insert()
        {
            DatabaseManager.CriminalRecordTable.InsertOne(this);
        }
    }
}
