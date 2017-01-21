using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.job_manager
{
    public class Job
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int Type { get; set; }

        public MarkerZone JoinPos { get; set; }
        public MarkerZone MiscOne { get; set; }
        public MarkerZone MiscTwo { get; set; }

        public List<Rectangle2DColShape> JobZones = new List<Rectangle2DColShape>();
        public List<List<Vector3>> JobRoutes = new List<List<Vector3>>();

        public Job()
        {
            
        }

        public void add_job_route(List<Vector3> list)
        {
            JobRoutes.Add(list);
        }

        public void remove_job_rout(int index)
        {
            JobRoutes.RemoveAt(index);
        }

        public void add_job_zone(float x, float y, float width, float height)
        {
            JobZones.Add(API.shared.create2DColShape(x, y, width, height));
        }

        public void remove_job_zone(int index)
        {
            JobZones.RemoveAt(index);
        }

        public void Insert()
        {
            Id = DatabaseManager.GetNextId("jobs");
            DatabaseManager.JobTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = Builders<Job>.Filter.Eq("_id", Id);
            DatabaseManager.JobTable.ReplaceOne(filter, this);
        }

        public int sprite_type()
        {
            switch (Type)
            {
                case JobManager.TaxiJob: return 198;
                default: return 1;
            }
        }
    }
}
