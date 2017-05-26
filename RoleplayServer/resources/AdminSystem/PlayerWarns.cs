using System;
using MongoDB.Bson;
using RoleplayServer.resources.database_manager;
using System.Collections.Generic;

namespace RoleplayServer.resources.AdminSystem
{
    public class PlayerWarns
    {
        public static List<PlayerWarns> Warns = new List<PlayerWarns>();

        public ObjectId Id { get; set; }

        public string AccountId { get; set; }
        public string WarnSender { get; set; }
        public string WarnReason { get; set; }
        public DateTime DateTime { get; set; }

        public PlayerWarns(string accountId, string warnSenderId, string reason)
        {
            AccountId = accountId;
            WarnSender = warnSenderId;
            WarnReason = reason;
            DateTime = DateTime.Now;
        }

        public void Insert()
        {
            DatabaseManager.PlayerWarnTable.InsertOne(this);
        }
    }
}
