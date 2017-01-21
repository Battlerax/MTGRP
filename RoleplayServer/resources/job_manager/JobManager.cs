using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;
using RoleplayServer.resources.player_manager;
using RoleplayServer.resources.vehicle_manager;

namespace RoleplayServer.resources.job_manager
{
    public class JobManager : Script
    {
        public const int TaxiJob = 1;

        public static List<Job> Jobs = new List<Job>();

        public JobManager()
        {
            DebugManager.DebugMessage("[JobM] Initalizing job manager...");

            API.onEntityEnterColShape += API_onEntityEnterColShape;
            API.onEntityExitColShape += API_onEntityExitColShape;

            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;

            load_jobs();

            DebugManager.DebugMessage("[JobM] Job Manager initalized!");
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            Character character = API.getEntityData(player, "Character");
            var veh = VehicleManager.GetVehFromNetHandle(vehicle);

            if(veh.JobId != 0)
            {
                if (API.getPlayerVehicleSeat(player) == -1 && veh.Job != character.JobOne)
                {
                    API.warpPlayerOutOfVehicle(player, vehicle);
                    API.sendPictureNotificationToPlayer(player, "This vehicle is only available to " + veh.Job.Name, "CHAR_BLOCKED", 0, 1, "Server", "~r~Vehicle Locked");
                }
            }
        }

        [Command("joinjob")]
        public void joinjob_cmd(Client player)
        {
            Character character = API.getEntityData(player, "Character");
            if(character.JobZoneType != 1)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You are not near a job joining location.");
                return;
            }

            var job = GetJobById(character.JobZone);

            character.JobOneId = job.Id;
            character.JobOne = job;
            character.Save();
            API.sendChatMessageToPlayer(player, Color.White, "You have become a " + job.Name);
        }

        [Command("quitjob")]
        public void quitjob_cmd(Client player)
        {
            Character character = API.getEntityData(player, "Character");

            if(character.JobOneId == 0)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You do not have a job to quit.");
                return;
            }

            API.sendChatMessageToPlayer(player, Color.Grey, "You have quit your job as a " + character.JobOne.Name);
            character.JobOneId = 0;
            character.JobOne = null;
            character.Save();
        }

        [Command("jobtypes")]
        public void jobtypes_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 4)
                return;

            API.sendChatMessageToPlayer(player, Color.White, "-----------------------------------------");
            API.sendChatMessageToPlayer(player, Color.Grey, "Type 1 - Taxi Driver");
            API.sendChatMessageToPlayer(player, Color.White, "-----------------------------------------");
        }

        [Command("createjob", GreedyArg = true)]
        public void createjob_cmd(Client player, int type, string name)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 4)
                return;

            var job = new Job
            {
                Type = type,
                Name = name,
                JoinPos = new MarkerZone(player.position, player.rotation, player.dimension)
                {
                    LabelText = name + "~n~/joinjob"
                }
            };


            job.JoinPos.Create();

            job.Insert();
            API.sendChatMessageToPlayer(player, Color.Grey, "You have created job " + job.Id + " ( " + job.Name + ", Type: " + job.Type + " ). Use /editjob to edit it.");
        }

        public static Job GetJobById(int id)
        {
            if (id == 0 || id > Jobs.Count )
                return null;

            return (Job)Jobs.ToArray().GetValue(id - 1);
        }

        public static void SendPictureNotificationToJob(Job job, string body, string pic, int flash, int iconType, string sender, string subject)
        {
            foreach(var c in PlayerManager.Players)
            {
                if(c.JobOne == job)
                {
                    API.shared.sendPictureNotificationToPlayer(c.Client, body, pic, flash, iconType, sender, subject);
                }
            }
        }

        public void load_jobs()
        {
            Jobs = DatabaseManager.JobTable.Find(Builders<Job>.Filter.Empty).ToList();

            foreach(var j in Jobs)
            {
                j.JoinPos = new MarkerZone(j.JoinPos.Location, j.JoinPos.Rotation, j.JoinPos.Dimension,
                    j.JoinPos.ColZoneSize)
                {
                    LabelText = "~g~" + j.Name + "~n~/joinjob",
                    BlipSprite = j.sprite_type()
                };
                j.JoinPos.Create();
            }

            DebugManager.DebugMessage("Loaded " + Jobs.Count + " jobs from the database.");
        }

        private void API_onEntityExitColShape(ColShape colshape, NetHandle entity)
        {
            if (API.getEntityType(entity) != EntityType.Player)
                return;

            foreach (var j in Jobs)
            {
                if (j.JoinPos.ColZone == colshape)
                {
                    Character c = API.getEntityData(entity, "Character");
                    c.JobZone = j.Id;
                    c.JobZoneType = 1;
                    break;
                }
                else if (j.MiscOne.ColZone == colshape)
                {
                    Character c = API.getEntityData(entity, "Character");
                    c.JobZone = j.Id;
                    c.JobZoneType = 2;
                    break;
                }
                else if (j.MiscTwo.ColZone == colshape)
                {
                    Character c = API.getEntityData(entity, "Character");
                    c.JobZone = j.Id;
                    c.JobZoneType = 3;
                    break;
                }
            }
        }

        private void API_onEntityEnterColShape(ColShape colshape, NetHandle entity)
        {
            if (API.getEntityType(entity) != EntityType.Player)
                return;

            foreach (var j in Jobs)
            {
                if (j.JoinPos.ColZone == colshape || j.MiscOne.ColZone == colshape || j.MiscTwo.ColZone == colshape)
                {
                    Character c = API.getEntityData(entity, "Character");
                    c.JobZone = 0;
                    c.JobZoneType = 0;
                    break;
                }
            }
        }
    }
}
