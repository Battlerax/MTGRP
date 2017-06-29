using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.player_manager;
using mtgvrp.rp_scripts;
using mtgvrp.vehicle_manager;
using MongoDB.Driver;

namespace mtgvrp.job_manager
{
    public class JobManager : Script
    {
        public enum JobTypes
        {
            None,
            Taxi,
            Fisher,
            Lumberjack
        }

        public static List<Job> Jobs = new List<Job>();

        public JobManager()
        {
            DebugManager.DebugMessage("[JobM] Initalizing job manager...");

            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;

            API.onClientEventTrigger += ApiOnOnClientEventTrigger;

            load_jobs();

            DebugManager.DebugMessage("[JobM] Job Manager initalized!");
        }

        private void ApiOnOnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            if (eventName == "finish_job_zone_create")
            {
                Account account = API.getEntityData(player.handle, "Account");
                if(account.AdminLevel < 4) { return;}

                Job job = API.getEntityData(player.handle, "JOB_ZONE_CREATE");

                var cornerStartPos = (Vector3) arguments[0];
                var xAdd = Convert.ToSingle(arguments[1]);
                var yAdd = Convert.ToSingle(arguments[2]);

                if (xAdd < 0)
                {
                    cornerStartPos = cornerStartPos.Add(new Vector3(xAdd, 0.0, 0.0));
                    xAdd = -xAdd;
                }

                if (yAdd < 0)
                {
                    cornerStartPos = cornerStartPos.Add(new Vector3(0.0, yAdd, 0.0));
                    yAdd = -yAdd;
                }

                job.add_job_zone(cornerStartPos.X, cornerStartPos.Y, xAdd, yAdd);
                job.register_job_zone_events(job.JobZones.Count - 1);
                job.Save();
                API.sendChatMessageToPlayer(player, "You have successfully added Job Zone " + job.JobZones.Count + " to Job " + job.Id);
            }
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            Character character = player.GetCharacter();
            var veh = VehicleManager.GetVehFromNetHandle(vehicle);

            if (veh?.JobId != 0)
            {
                if (API.getPlayerVehicleSeat(player) == -1 && veh?.JobId != 0 && veh?.JobId != character.JobOneId && player.GetAccount().AdminDuty == false)
                { 
                    API.warpPlayerOutOfVehicle(player);
                    API.sendPictureNotificationToPlayer(player, "This vehicle is only available to " + veh?.Job?.Name, "CHAR_BLOCKED", 0, 1, "Server", "~r~Vehicle Locked");
                }
            }
        }

        [Command("joinjob")]
        public void joinjob_cmd(Client player)
        {
            Character character = player.GetCharacter();
            if(character.JobZoneType != 1)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You are not near a job joining location.");
                return;
            }

            var job = GetJobById(character.JobZone);

            if (job == Job.None)
            {
                API.sendChatMessageToPlayer(player, "null job");
                return;
            }

            character.JobOneId = job.Id;
            character.JobOne = job;
            character.Save();
            API.sendChatMessageToPlayer(player, Color.White, "You have become a " + job.Name);
        }

        [Command("quitjob")]
        public void quitjob_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if(character.JobOneId == 0)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You do not have a job to quit.");
                return;
            }

            API.sendChatMessageToPlayer(player, Color.Grey, "You have quit your job as a " + character.JobOne.Name);
            character.JobOneId = 0;
            character.JobOne = Job.None;
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
        public void createjob_cmd(Client player, JobTypes type, string name)
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


            job.JoinPos.ColZoneSize = 5;
            job.JoinPos.Create();
            job.register_job_marker_events();
            job.Insert();
            Jobs.Add(job);
            API.sendChatMessageToPlayer(player, Color.Grey, "You have created job " + job.Id + " ( " + job.Name + ", Type: " + job.Type + " ). Use /editjob to edit it.");
        }

        [Command("editjob", GreedyArg = true)]
        public void editjob_cmd(Client player, int jobId, string option, string value = "None")
        {
            Account account = player.GetAccount();
            if(account.AdminLevel < 4)
                return;

            var job = GetJobById(jobId);
            if (job == Job.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ Invalid job ID entered.");
                return;
            }

            switch (option)
            {
                case "jobname":
                    job.Name = value;
                    job.JoinPos.LabelText = "~g~" + job.Name + "~n~/joinjob";
                    job.JoinPos.Refresh();
                    job.Save();
                    API.sendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s name to " + job.Name);
                    break;
                case "type":
                    JobTypes test;
                    Enum.TryParse(value, out test);
                    job.Type = test;
                    job.Save();
                    API.sendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s type to " + job.Type);
                    break;
                case "joinpos_loc":
                    job.JoinPos.Location = player.position;
                    job.JoinPos.Rotation = player.rotation;
                    job.JoinPos.Dimension = player.dimension;
                    job.JoinPos.Refresh();
                    job.Save();
                    API.sendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s location to your current position");
                    break;
                case "misc_one_loc":

                    if (job.MiscOne == MarkerZone.None)
                    {
                        job.MiscOne = new MarkerZone(player.position, player.rotation, player.dimension)
                        {
                            LabelText = job.Name + " Misc One"
                        };
                        job.MiscOne.Create();
                    }
                    else
                    {
                        job.MiscOne.Location = player.position;
                        job.MiscOne.Rotation = player.rotation;
                        job.MiscOne.Dimension = player.dimension;
                        job.MiscOne.Refresh();
                    }
                    job.Save();
                    API.sendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc one location to your current position");
                    break;

                case "misc_one_name":
                    if (job.MiscOne == MarkerZone.None)
                    {
                        job.MiscOne = new MarkerZone(player.position, player.rotation, player.dimension)
                        {
                            LabelText = job.Name + " Misc One"
                        };
                        job.MiscOne.Create();
                    }
                    else
                    {
                        job.MiscOne.LabelText = value;
                        job.MiscOne.Refresh();
                    }
                    job.Save();
                    API.sendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc one text to " + job.MiscOne.LabelText);
                    break;
                case "misc_one_blip":
                    if (job.MiscOne == MarkerZone.None)
                    {
                        API.sendChatMessageToPlayer(player, "The markerzone is not even there.");
                        return;
                    }
                    else
                    {
                        job.MiscOne.BlipSprite = Convert.ToInt32(value);
                        job.MiscOne.Refresh();
                    }
                    job.Save();
                    API.sendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc one blip to " + value);
                    break;
                case "misc_two_loc":

                    if (job.MiscTwo == MarkerZone.None)
                    {
                        job.MiscTwo = new MarkerZone(player.position, player.rotation, player.dimension)
                        {
                            LabelText = job.Name + " Misc Two"
                        };
                        job.MiscTwo.Create();
                    }
                    else
                    {
                        job.MiscTwo.Location = player.position;
                        job.MiscTwo.Rotation = player.rotation;
                        job.MiscTwo.Dimension = player.dimension;
                        job.MiscTwo.Refresh();
                    }
                    job.Save();
                    API.sendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc two location to your current position");
                    break;
                case "misc_two_name":
                    if (job.MiscTwo == MarkerZone.None)
                    {
                        job.MiscTwo = new MarkerZone(player.position, player.rotation, player.dimension)
                        {
                            LabelText = job.Name + " Misc Two"
                        };
                        job.MiscTwo.Create();
                    }
                    else
                    {
                        job.MiscTwo.LabelText = value;
                        job.MiscTwo.Refresh();
                    }
                    job.Save();
                    API.sendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc two text to " + job.MiscOne.LabelText);
                    break;
                case "misc_two_blip":
                    if (job.MiscTwo == MarkerZone.None)
                    {
                        API.sendChatMessageToPlayer(player, "The markerzone is not even there.");
                        return;
                    }
                    else
                    {
                        job.MiscTwo.BlipSprite = Convert.ToInt32(value);
                        job.MiscTwo.Refresh();
                    }
                    job.Save();
                    API.sendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc two blip to " + value);
                    break;
                default:
                    API.sendChatMessageToPlayer(player, Color.White, "Invalid option chosen. Valid options are:");
                    API.sendChatMessageToPlayer(player, Color.White, "jobname, type, joinpos_loc, misc_one_loc, misc_one_name, misc_one_blip, misc_two_loc, misc_two_name, misc_two_name");
                    break;
            }
        }

        [Command("createjobzone")]
        public void createjobzone_cmd(Client player, int jobId, string option)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 4)
                return;

            var job = GetJobById(jobId);
            if (job == Job.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ Invalid job ID entered.");
                return;
            }

            switch (option)
            {
                case "start":

                    API.setEntityData(player.handle, "JOB_ZONE_CREATE", job);
                    API.triggerClientEvent(player, "create_job_zone");
                    API.sendChatMessageToPlayer(player, "Creating zone...");
                    break;
                case "cancel":
                    API.triggerClientEvent(player, "cancel_job_zone");
                    API.sendChatMessageToPlayer(player, "Job zone creation canceled.");
                    break;
                case "finish":
                    API.triggerClientEvent(player, "finish_job_zone");
                    break;
                default:
                    API.sendChatMessageToPlayer(player, Color.White, "Invalid option. Valid options are: ~g~start, finish, cancel");
                    break;
            }
        }

        [Command("deletejobzone")]
        public void deletejobzone_cmd(Client player, int jobId, int zoneId)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 4)
                return;

            var job = GetJobById(jobId);
            if (job == Job.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ Invalid job ID entered.");
                return;
            }

            if (zoneId < 0 || zoneId > job.JobZones.Count)
            {
                API.sendChatMessageToPlayer(player, Color.White, "~R~:ERROR:~W~ Invalid zone id. Valid IDs range from 0 to " + (job.JobZones.Count - 1));
                return;
            }

            job.remove_job_zone(zoneId - 1);
            job.Save();
            API.sendChatMessageToPlayer(player, Color.White, "You have successfully deleted Job " + job.Id + "'s Zone " + zoneId);
        }

        [Command("viewjobzone")]
        public void viewjobzone_cmd(Client player, int jobId, int zoneId)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 4)
                return;

            var job = GetJobById(jobId);
            if (job == Job.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ Invalid job ID entered.");
                return;
            }

            if (zoneId < 0 || zoneId > job.JobZones.Count)
            {
                API.sendChatMessageToPlayer(player, Color.White, "~R~:ERROR:~W~ Invalid zone id. Valid IDs range from 0 to " + (job.JobZones.Count - 1));
                return;
            }

            if (API.getEntityData(player.handle, "ZONE_MARKER_1") == null)
            {
                var topLeft = new Vector3(job.JobZones[zoneId - 1].X, job.JobZones[zoneId - 1].Y, player.position.Z);
                var topRight = topLeft.Add(new Vector3(job.JobZones[zoneId - 1].Width, 0.0, 0.0));
                var bottomLeft = topLeft.Add(new Vector3(0.0, job.JobZones[zoneId - 1].Height, 0.0));
                var bottomRight =
                    topLeft.Add(new Vector3(job.JobZones[zoneId - 1].Width, job.JobZones[zoneId - 1].Height, 0.0));

                API.setEntityData(player.handle, "ZONE_MARKER_1",
                    API.createMarker(0, topLeft, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 255, 0,
                        player.dimension));
                API.setEntityData(player.handle, "ZONE_MARKER_2",
                    API.createMarker(0, topRight, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 255, 0,
                        player.dimension));
                API.setEntityData(player.handle, "ZONE_MARKER_3",
                    API.createMarker(0, bottomLeft, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 255, 0,
                        player.dimension));
                API.setEntityData(player.handle, "ZONE_MARKER_4",
                    API.createMarker(0, bottomRight, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 255,
                        0, player.dimension));

                API.sendChatMessageToPlayer(player, "Viewing zone " + zoneId + " of Job " + job.Id);
            }
            else
            {
                API.deleteEntity(API.getEntityData(player.handle, "ZONE_MARKER_1"));
                API.deleteEntity(API.getEntityData(player.handle, "ZONE_MARKER_2"));
                API.deleteEntity(API.getEntityData(player.handle, "ZONE_MARKER_3"));
                API.deleteEntity(API.getEntityData(player.handle, "ZONE_MARKER_4"));
                API.resetEntityData(player.handle, "ZONE_MARKER_1");
                API.resetEntityData(player.handle, "ZONE_MARKER_2");
                API.resetEntityData(player.handle, "ZONE_MARKER_3");
                API.resetEntityData(player.handle, "ZONE_MARKER_4");
                API.sendChatMessageToPlayer(player, "No longer viewing job zone.");
            }
        }

        public static Job GetJobById(int id)
        {
            if (id == 0 || id > Jobs.Count )
                return Job.None;

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

                if (j.MiscOne != MarkerZone.None)
                {
                    j.MiscOne.Create();
                }

                if (j.MiscTwo != MarkerZone.None)
                {
                    j.MiscTwo.Create();
                }

                for(var i = 0; i < j.JobZones.Count; i++)
                {
                    j.JobZones[i] = API.create2DColShape(j.JobZones[i].X, j.JobZones[i].Y, j.JobZones[i].Width,
                        j.JobZones[i].Height);
                    j.register_job_zone_events(i);  
                }

                j.register_job_marker_events();
            }

            DebugManager.DebugMessage("Loaded " + Jobs.Count + " jobs from the database.");
        }

    }
}
