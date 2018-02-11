using System.Collections.Generic;
using GTANetworkAPI;
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

        public List<ColShape> JobZones = new List<ColShape>();
        public List<List<Vector3>> JobRoutes = new List<List<Vector3>>();

        public Job()
        {
            JoinPos = MarkerZone.None;
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
            ColShape shape = API.Shared.Create2DColShape(x1, y1, width, height);
            shape.SetData("Width", width);
            shape.SetData("Height", height); // TODO: convert to extension
            JobZones.Add(shape);
        }

        public void register_job_zone_events(int index)
        {
            JobZones[index].OnEntityEnterColShape += (shape, entity) =>
            {
                if (Type == JobManager.JobTypes.Fisher)
                {
                    var c = API.Shared.GetEntityData(entity, "Character");
                    if (c == null)
                        return;

                    c.IsInFishingZone = true;
                }

            };

            JobZones[index].OnEntityExitColShape += (shape, entity) =>
            {
                if (Type == JobManager.JobTypes.Fisher)
                {
                    var c = API.Shared.GetEntityData(entity, "Character");
                    if (c == null)
                        return;

                    c.IsInFishingZone = false;
                }
            };
        }

        public void remove_job_zone(int index)
        {
            JobZones.RemoveAt(index);
        }

        public void register_job_marker_events()
        {
            void ColShapeEvent(ColShape shape, Client entity)
            {
                var c = API.Shared.GetEntityData(entity, "Character");
                c.JobZone = Id;

                if(shape == JoinPos.ColZone)
                    c.JobZoneType = 1;
                else if(shape == MiscOne.ColZone)
                    c.JobZoneType = 2;
                else if (shape == MiscTwo.ColZone)
                    c.JobZoneType = 3;
                else
                    c.JobZoneType = 0;
            }

            void OnExitColShape(ColShape shape, Client entity)
            {
                var c = API.Shared.GetPlayerFromHandle(entity).GetCharacter();
                c.JobZone = 0;
                c.JobZoneType = 0;
            }


            JoinPos.ColZone.OnEntityEnterColShape -= ColShapeEvent;
            JoinPos.ColZone.OnEntityEnterColShape += ColShapeEvent;

            if (MiscOne != MarkerZone.None)
            {
                MiscOne.ColZone.OnEntityEnterColShape -= ColShapeEvent;
                MiscOne.ColZone.OnEntityEnterColShape += ColShapeEvent;
            }

            if (MiscTwo != MarkerZone.None)
            {
                MiscTwo.ColZone.OnEntityEnterColShape -= ColShapeEvent;
                MiscTwo.ColZone.OnEntityEnterColShape += ColShapeEvent;
            }

            JoinPos.ColZone.OnEntityEnterColShape -= OnExitColShape;
            JoinPos.ColZone.OnEntityEnterColShape += OnExitColShape;

            if (MiscOne != MarkerZone.None)
            {
                MiscOne.ColZone.OnEntityEnterColShape -= OnExitColShape;
                MiscOne.ColZone.OnEntityEnterColShape += OnExitColShape;
            }

            if (MiscTwo != MarkerZone.None)
            {
                MiscTwo.ColZone.OnEntityEnterColShape -= OnExitColShape;
                MiscTwo.ColZone.OnEntityEnterColShape += OnExitColShape;
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
                case JobManager.JobTypes.Mechanic: return 402;
                case JobManager.JobTypes.Lumberjack: return 77;
                case JobManager.JobTypes.Garbageman: return 318;
                case JobManager.JobTypes.Trucker: return 477;
                case JobManager.JobTypes.DeliveryMan: return 478;
                default: return 1;
            }
        }
    }
}
