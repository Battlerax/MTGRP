using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.AdminSystem
{
    public class AdminReports
    {
        public static List<AdminReports> Reports = new List<AdminReports>();

        [BsonId]
        public ObjectId Id { get; set; }

        public int Type { get; set; }
        public string Name { get; set; }
        public string ReportMessage { get; set; }
        public string Target { get; set; }

        public AdminReports(int type, string name, string reportmessage, string target = null)
        {
            Type = type;
            Name = name;
            ReportMessage = reportmessage;
            Target = target;
        }

        public static void Delete(AdminReports name)
        {

            Reports.Remove(name);
        }

        public static void InsertReport(int type, string name, string reportMessage, string target = null)
        {
            var report = new AdminReports(type, name, reportMessage, target);
            Reports.Add(report);
        }

        public static bool ReportExists(string reportName)
        {
            return Reports.Count(i => string.Equals(i.Name, reportName, StringComparison.OrdinalIgnoreCase)) > 0;
        }
    }
}
