using System;
using System.Collections.Generic;
using MongoDB.Driver;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.group_manager
{
    public class Group
    {
        public int Id { get; set; }

        public string GroupName { get; set; }
        public int GroupType { get; set; }
        public int GroupCommandType { get; set; }

        public string GroupMotd { get; set; }

        public List<string> GroupRankNames = new List<string> { "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10" };
        public List<string> GroupDivisions = new List<string> { "D1", "D2", "D3", "D4", "D5" };

        public DateTime GroupDisbandDate { get; set; }

        public Group()
        {
            Id = 0;
            GroupName = "None";
            GroupType = 0;
            GroupCommandType = 0;
            GroupMotd = "Welcome To Group";

           

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

    }
}
