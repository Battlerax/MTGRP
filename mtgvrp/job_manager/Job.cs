using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.player_manager;
using MongoDB.Driver;

namespace mtgvrp.job_manager
{
    public class Job
    {
        public static readonly Job None = new Job();

        public int Id { get; set; }

        public string Name { get; set; }
        public JobManager.JobTypes Type { get; set; }

        public MarkerZone JoinPos { get; set; }
        public MarkerZone MiscOne { get; set; }
        public MarkerZone MiscTwo { get; set; }

        public List<Rectangle2DColShape> JobZones = new List<Rectangle2DColShape>();
        public List<List<Vector3>> JobRoutes = new List<List<Vector3>>();

        public Job()
        {
            MiscOne = MarkerZone.None;
            MiscTwo = MarkerZone.None;
        }

        public void add_job_route(List<Vector3> list)
        {
            JobRoutes.Add(list);
        }

        public void remove_job_rout(int index)
        {
            JobRoutes.RemoveAt(index);
        }

        public void add_job_zone(float x1, float y1, float width, float height)
        {
            JobZones.Add(API.shared.create2DColShape(x1, y1, width, height));
        }

        public void register_job_zone_events(int index)
        {
            JobZones[index].onEntityEnterColShape += (shape, entity) =>
            {
                foreach (var c in PlayerManager.Players)
                {
                    if (c.Client.handle != entity) continue;

                    if (Type == JobManager.JobTypes.Fisher)
                    {
                        c.IsInFishingZone = true;
                    }
                }
            };

            JobZones[index].onEntityExitColShape += (shape, entity) =>
            {
                foreach (var c in PlayerManager.Players)
                {
                    if (c.Client.handle != entity) continue;

                    if (Type == JobManager.JobTypes.Fisher)
                    {
                        c.IsInFishingZone = false;
                    }
                }
            };
        }

        public void remove_job_zone(int index)
        {
            JobZones.RemoveAt(index);
        }

        public void register_job_marker_events()
        {
            JoinPos.ColZone.onEntityEnterColShape += (shape, entity) =>
            {
                foreach (var c in PlayerManager.Players)
                {
                    if (c.Client != entity) { continue;}

                    c.JobZone = Id;
                    c.JobZoneType = 1;
                }
            };

            if (MiscOne != MarkerZone.None)
            {
                MiscOne.ColZone.onEntityEnterColShape += (shape, entity) =>
                {
                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.Client != entity) { continue; }

                        c.JobZone = Id;
                        c.JobZoneType = 2;
                    }
                };
            }

            if (MiscTwo != MarkerZone.None)
            {
                MiscTwo.ColZone.onEntityEnterColShape += (shape, entity) =>
                {
                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.Client != entity) { continue; }

                        c.JobZone = Id;
                        c.JobZoneType = 3;
                    }
                };
            }

            JoinPos.ColZone.onEntityExitColShape += (shape, entity) =>
            {
                foreach (var c in PlayerManager.Players)
                {
                    if (c.Client != entity) { continue; }

                    c.JobZone = 0;
                    c.JobZoneType = 0;
                }
            };

            if (MiscOne != MarkerZone.None)
            {
                MiscOne.ColZone.onEntityExitColShape += (shape, entity) =>
                {
                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.Client != entity) { continue; }

                        c.JobZone = 0;
                        c.JobZoneType = 0;
                    }
                };
            }

            if (MiscTwo != MarkerZone.None)
            {
                MiscTwo.ColZone.onEntityExitColShape += (shape, entity) =>
                {
                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.Client != entity) { continue; }

                        c.JobZone = 0;
                        c.JobZoneType = 0;
                    }
                };
            }
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
                case JobManager.JobTypes.Taxi: return 198;
                case JobManager.JobTypes.Fisher: return 410;
                case JobManager.JobTypes.Lumberjack: return 77;
                default: return 1;
            }
        }
    }
}
