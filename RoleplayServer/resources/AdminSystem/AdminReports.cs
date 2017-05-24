using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.AdminSystem
{
    public class AdminReports
    {
        public static List<AdminReports> Reports = new List<AdminReports>();

        [BsonId]
        public ObjectId Id { get; set; }

        public string Type { get; set; }
        public string Name { get; set; }
        public string ReportMessage { get; set; }

        public AdminReports(string type, string name, string reportmessage, string target)
        {
            Type = type;
            Name = name;
            ReportMessage = reportmessage;
        }

        public void Update()
        {
            var filter = MongoDB.Driver.Builders<AdminReports>.Filter.Eq("Id", Id);
            DatabaseManager.ReportTable.ReplaceOne(filter, this);
        }

        public void Insert()
        {
            DatabaseManager.ReportTable.InsertOne(this);
        }

        public static void Delete(AdminReports name)
        {

            Reports.Remove(name);
            var filter = MongoDB.Driver.Builders<AdminReports>.Filter.Eq("Id", name.Id);
            DatabaseManager.ReportTable.DeleteOne(filter);
        }

        public static void InsertReport(string type, string name, string ReportMessage, string target)
        {
            var report = new AdminReports(type, name, ReportMessage, target);
            report.Insert();
            Reports.Add(report);
        }

        public static bool ReportExists(string reportName)
        {
            return Reports.Count(i => string.Equals(i.Name, reportName, StringComparison.OrdinalIgnoreCase)) > 0;
        }
    }
}
