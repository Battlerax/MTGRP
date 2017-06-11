using System;
using MongoDB.Bson;


namespace RoleplayServer.AdminSystem
{
    public class PlayerWarns
    {
        public ObjectId Id { get; set; }

        public string WarnReceiver { get; set; }
        public string WarnSender { get; set; }
        public string WarnReason { get; set; }
        public DateTime DateTime { get; set; }

        public PlayerWarns(string warnReceiverId, string warnSenderId, string reason)
        {
            WarnReceiver = warnReceiverId;
            WarnSender = warnSenderId;
            WarnReason = reason;
            DateTime = DateTime.Now;
        }

        public void Insert()
        {
            //DatabaseManager.PlayerWarnTable.InsertOne(this);
        }
    }
}
