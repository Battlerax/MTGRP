using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;

namespace RoleplayServer
{
    public class JobManager : Script
    {
        public const int TaxiJob = 1;

        public static List<Job> jobs = new List<Job>();

        public JobManager()
        {
            DebugManager.debugMessage("[JobM] Initalizing job manager...");

            API.onEntityEnterColShape += API_onEntityEnterColShape;
            API.onEntityExitColShape += API_onEntityExitColShape;

            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;

            load_jobs();

            DebugManager.debugMessage("[JobM] Job Manager initalized!");
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            Character character = API.getEntityData(player, "Character");
            Vehicle veh = VehicleManager.getVehFromNetHandle(vehicle);

            if(veh.job_id != 0)
            {
                if (API.getPlayerVehicleSeat(player) == -1 && (veh.job != character.job_one))
                {
                    API.warpPlayerOutOfVehicle(player, vehicle);
                    API.sendPictureNotificationToPlayer(player, "This vehicle is only available to " + veh.job.name, "CHAR_BLOCKED", 0, 1, "Server", "~r~Vehicle Locked");
                }
            }
        }

        [Command("joinjob")]
        public void joinjob_cmd(Client player)
        {
            Character character = API.getEntityData(player, "Character");
            if(character.job_zone_type != 1)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You are not near a job joining location.");
                return;
            }

            Job job = getJobById(character.job_zone);

            character.job_one_id = job._id;
            character.job_one = job;
            character.save();
            API.sendChatMessageToPlayer(player, Color.White, "You have become a " + job.name);
        }

        [Command("quitjob")]
        public void quitjob_cmd(Client player)
        {
            Character character = API.getEntityData(player, "Character");

            if(character.job_one_id == 0)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You do not have a job to quit.");
                return;
            }

            API.sendChatMessageToPlayer(player, Color.Grey, "You have quit your job as a " + character.job_one.name);
            character.job_one_id = 0;
            character.job_one = null;
            character.save();
            return;
        }

        [Command("jobtypes")]
        public void jobtypes_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.admin_level < 4)
                return;

            API.sendChatMessageToPlayer(player, Color.White, "-----------------------------------------");
            API.sendChatMessageToPlayer(player, Color.Grey, "Type 1 - Taxi Driver");
            API.sendChatMessageToPlayer(player, Color.White, "-----------------------------------------");
        }

        [Command("createjob", GreedyArg = true)]
        public void createjob_cmd(Client player, int type, string name)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.admin_level < 4)
                return;

            Job job = new Job();

            job.type = type;
            job.name = name;

            job.join_pos = new MarkerZone(player.position, player.rotation, player.dimension);
            job.join_pos.label_text = name + "~n~/joinjob";
            job.join_pos.create();

            job.insert();
            API.sendChatMessageToPlayer(player, Color.Grey, "You have created job " + job._id + " ( " + job.name + ", Type: " + job.type + " ). Use /editjob to edit it.");
        }

        public static Job getJobById(int id)
        {
            if (id == 0 || id > jobs.Count )
                return null;

            return (Job)jobs.ToArray().GetValue(id - 1);
        }

        public static void sendPictureNotificationToJob(Job job, string body, string pic, int flash, int iconType, string sender, string subject)
        {
            foreach(Character c in PlayerManager.players)
            {
                if(c.job_one == job)
                {
                    API.shared.sendPictureNotificationToPlayer(c.client, body, pic, flash, iconType, sender, subject);
                }
            }
        }

        public void load_jobs()
        {
            jobs = DatabaseManager.job_table.Find<Job>(Builders<Job>.Filter.Empty).ToList<Job>();

            foreach(Job j in jobs)
            {
                j.join_pos = new MarkerZone(j.join_pos.location, j.join_pos.rotation, j.join_pos.dimension, j.join_pos.col_zone_size);
                j.join_pos.label_text = "~g~" + j.name + "~n~/joinjob";
                j.join_pos.blip_sprite = j.sprite_type();
                j.join_pos.create();
            }

            DebugManager.debugMessage("Loaded " + jobs.Count + " jobs from the database.");
        }

        private void API_onEntityExitColShape(ColShape colshape, NetHandle entity)
        {
            if (API.getEntityType(entity) != EntityType.Player)
                return;

            foreach (Job j in jobs)
            {
                if (j.join_pos.col_zone == colshape)
                {
                    Character c = API.getEntityData(entity, "Character");
                    c.job_zone = j._id;
                    c.job_zone_type = 1;
                    break;
                }
                else if (j.misc_one.col_zone == colshape)
                {
                    Character c = API.getEntityData(entity, "Character");
                    c.job_zone = j._id;
                    c.job_zone_type = 2;
                    break;
                }
                else if (j.misc_two.col_zone == colshape)
                {
                    Character c = API.getEntityData(entity, "Character");
                    c.job_zone = j._id;
                    c.job_zone_type = 3;
                    break;
                }
            }
        }

        private void API_onEntityEnterColShape(ColShape colshape, NetHandle entity)
        {
            if (API.getEntityType(entity) != EntityType.Player)
                return;

            foreach (Job j in jobs)
            {
                if (j.join_pos.col_zone == colshape || j.misc_one.col_zone == colshape || j.misc_two.col_zone == colshape)
                {
                    Character c = API.getEntityData(entity, "Character");
                    c.job_zone = 0;
                    c.job_zone_type = 0;
                    break;
                }
            }
        }
    }
}
