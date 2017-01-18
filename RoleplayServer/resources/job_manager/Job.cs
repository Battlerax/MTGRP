using GTANetworkShared;
using GTANetworkServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace RoleplayServer
{
    public class Job
    {
        public int _id { get; set; }

        public string name { get; set; }
        public int type { get; set; }

        public MarkerZone join_pos { get; set; }
        public MarkerZone misc_one { get; set; }
        public MarkerZone misc_two { get; set; }

        public Job()
        {
            
        }

        public void insert()
        {
            _id = DatabaseManager.getNextId("jobs");
            DatabaseManager.job_table.InsertOne(this);
        }

        public void save()
        {
            FilterDefinition<Job> filter = Builders<Job>.Filter.Eq("_id", _id);
            DatabaseManager.job_table.ReplaceOne(filter, this);
        }

        public int sprite_type()
        {
            switch (this.type)
            {
                case JobManager.TaxiJob: return 198;
                default: return 1;
            }
        }
    }
}
