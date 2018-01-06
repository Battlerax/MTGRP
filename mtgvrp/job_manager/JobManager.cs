using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;


using GTANetworkAPI;



using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.database_manager;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using MongoDB.Driver;
using Color = mtgvrp.core.Color;

namespace mtgvrp.job_manager
{
    public class JobManager : Script
    {
        public enum JobTypes
        {
            None,
            Taxi,
            Fisher,
            Mechanic,
            Lumberjack,
            Garbageman,
            Trucker,
            DeliveryMan
        }

        public static List<Job> Jobs = new List<Job>();

        public JobManager()
        {
            DebugManager.DebugMessage("[JobM] Initalizing job manager...");

            Event.OnPlayerEnterVehicle += API_onPlayerEnterVehicle;

            Event.OnClientEventTrigger += ApiOnOnClientEventTrigger;

            load_jobs();

            DebugManager.DebugMessage("[JobM] Job Manager initalized!");
        }

        private void ApiOnOnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            if (eventName == "finish_job_zone_create")
            {
                Account account = player.GetAccount();
                if(account.AdminLevel < 4) { return;}

                Job job = API.GetEntityData(player.handle, "JOB_ZONE_CREATE");

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
                API.SendChatMessageToPlayer(player, "You have successfully added Job Zone " + job.JobZones.Count + " to Job " + job.Id);
            }
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle, int seat)
        {
            Character character = player.GetCharacter();
            var veh = VehicleManager.GetVehFromNetHandle(vehicle);

            if (veh?.JobId != 0)
            {
                if (seat == -1 && veh?.JobId != 0 && veh?.JobId != character.JobOneId && player.GetAccount().AdminDuty == false)
                {
                    API.SendPictureNotificationToPlayer(player, "This vehicle is only available to " + veh?.Job?.Name, "CHAR_BLOCKED", 0, 1, "Server", "~r~Vehicle Locked");
                    API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));;
                }
            }
        }

        [Command("joinjob"), Help(HelpManager.CommandGroups.JobsGeneral, "Joins a new job, must be near joinjob location.")]
        public void joinjob_cmd(Client player)
        {
            Character character = player.GetCharacter();
            if(character.JobZoneType != 1)
            {
                API.SendNotificationToPlayer(player, "~r~ERROR:~w~ You are not near a job joining location.");
                return;
            }

            var job = GetJobById(character.JobZone);

            if (job == Job.None)
            {
                API.SendChatMessageToPlayer(player, "null job");
                return;
            }

            if (character.JobOne != Job.None)
            {
                API.SendChatMessageToPlayer(player, "You already have a job. /quitjob first.");
                return;
            }

            character.JobOneId = job.Id;
            character.JobOne = job;
            character.Save();
            API.SendChatMessageToPlayer(player, Color.White, "You have become a " + job.Name);
        }

        [Command("quitjob"), Help(HelpManager.CommandGroups.JobsGeneral, "Exits your current job.")]
        public void quitjob_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if(character.JobOneId == 0)
            {
                API.SendNotificationToPlayer(player, "~r~ERROR:~w~You do not have a job to quit.");
                return;
            }

            API.SendChatMessageToPlayer(player, Color.Grey, "You have quit your job as a " + character.JobOne.Name);
            character.JobOneId = 0;
            character.JobOne = Job.None;
            character.Save();
        }

        [Command("jobtypes"), Help(HelpManager.CommandGroups.AdminLevel5, "View all available jobs.")]
        public void jobtypes_cmd(Client player)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 4)
                return;

            API.SendChatMessageToPlayer(player, Color.White, "-----------------------------------------");
            foreach (var job in JobManager.Jobs)
            {
                player.SendChatMessage($"Type {job.Type} - {job.Name}");
            }
            API.SendChatMessageToPlayer(player, Color.White, "-----------------------------------------");
        }

        [Command("createjob", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel5, "Creates a job in your position.", "The job type", "Name of the job")]
        public void createjob_cmd(Client player, JobTypes type, string name)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 4)
                return;

            var job = new Job
            {
                Type = type,
                Name = name,
                JoinPos = new MarkerZone(player.position, player.rotation, player.dimension)
                {
                    TextLabelText = name + "~n~/joinjob"
                }
            };


            job.JoinPos.ColZoneSize = 5;
            job.JoinPos.UseBlip = true;
            job.JoinPos.BlipSprite = job.sprite_type();

            job.JoinPos.Create();
            job.register_job_marker_events();

            job.Insert();
            Jobs.Add(job);
            API.SendChatMessageToPlayer(player, Color.Grey, "You have created job " + job.Id + " ( " + job.Name + ", Type: " + job.Type + " ). Use /editjob to edit it.");
        }

        [Command("editjob", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel5, "Edit an existing job.", "The id of the job", "Edit option", "Value")]
        public void editjob_cmd(Client player, int jobId, string option, string value = "None")
        {
            Account account = player.GetAccount();
            if(account.AdminLevel < 4)
                return;

            var job = GetJobById(jobId);
            if (job == Job.None)
            {
                API.SendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ Invalid job ID entered.");
                return;
            }

            switch (option)
            {
                case "jobname":
                    job.Name = value;
                    job.JoinPos.TextLabelText = "~g~" + job.Name + "~n~/joinjob";
                    job.JoinPos.Refresh();
                    job.Save();
                    API.SendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s name to " + job.Name);
                    break;
                case "type":
                    JobTypes test;
                    Enum.TryParse(value, out test);
                    job.Type = test;
                    job.Save();
                    API.SendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s type to " + job.Type);
                    break;
                case "joinpos_loc":
                    job.JoinPos.Location = player.position;
                    job.JoinPos.Rotation = player.rotation;
                    job.JoinPos.Dimension = player.dimension;
                    job.JoinPos.UseBlip = true;
                    job.JoinPos.Refresh();
                    job.Save();
                    job.register_job_marker_events();
                    API.SendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s location to your current position");
                    break;
                case "misc_one_loc":

                    if (job.MiscOne == MarkerZone.None)
                    {
                        job.MiscOne = new MarkerZone(player.position, player.rotation, player.dimension)
                        {
                            TextLabelText = job.Name + " Misc One"
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
                    job.register_job_marker_events();
                    API.SendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc one location to your current position");
                    break;

                case "misc_one_name":
                    if (job.MiscOne == MarkerZone.None)
                    {
                        job.MiscOne = new MarkerZone(player.position, player.rotation, player.dimension)
                        {
                            TextLabelText = job.Name + " Misc One"
                        };
                        job.MiscOne.Create();
                    }
                    else
                    {
                        job.MiscOne.TextLabelText = value;
                        job.MiscOne.Refresh();
                    }
                    job.Save();
                    API.SendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc one text to " + job.MiscOne.TextLabelText);
                    break;
                case "misc_one_blip":
                    if (job.MiscOne == MarkerZone.None)
                    {
                        API.SendChatMessageToPlayer(player, "The markerzone is not even there.");
                        return;
                    }
                    else
                    {
                        job.MiscOne.BlipSprite = Convert.ToInt32(value);
                        job.MiscOne.UseBlip = true;
                        job.MiscOne.Refresh();
                    }
                    job.Save();
                    API.SendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc one blip to " + value);
                    break;
                case "misc_two_loc":

                    if (job.MiscTwo == MarkerZone.None)
                    {
                        job.MiscTwo = new MarkerZone(player.position, player.rotation, player.dimension)
                        {
                            TextLabelText = job.Name + " Misc Two"
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
                    job.register_job_marker_events();
                    API.SendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc two location to your current position");
                    break;
                case "misc_two_name":
                    if (job.MiscTwo == MarkerZone.None)
                    {
                        job.MiscTwo = new MarkerZone(player.position, player.rotation, player.dimension)
                        {
                            TextLabelText = job.Name + " Misc Two"
                        };
                        job.MiscTwo.Create();
                    }
                    else
                    {
                        job.MiscTwo.TextLabelText = value;
                        job.MiscTwo.Refresh();
                    }
                    job.Save();
                    API.SendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc two text to " + job.MiscOne.TextLabelText);
                    break;
                case "misc_two_blip":
                    if (job.MiscTwo == MarkerZone.None)
                    {
                        API.SendChatMessageToPlayer(player, "The markerzone is not even there.");
                        return;
                    }
                    else
                    {
                        job.MiscTwo.BlipSprite = Convert.ToInt32(value);
                        job.MiscOne.UseBlip = true;
                        job.MiscTwo.Refresh();
                    }
                    job.Save();
                    API.SendChatMessageToPlayer(player, Color.White, "You have changed Job " + job.Id + "'s misc two blip to " + value);
                    break;
                default:
                    API.SendChatMessageToPlayer(player, Color.White, "Invalid option chosen. Valid options are:");
                    API.SendChatMessageToPlayer(player, Color.White, "jobname, type, joinpos_loc, misc_one_loc, misc_one_name, misc_one_blip, misc_two_loc, misc_two_name, misc_two_name");
                    break;
            }
        }

        [Command("createjobzone"), Help(HelpManager.CommandGroups.AdminLevel5, "Create a jobzone, for example where Fishermen do /fish", "Job Id", "Option to do")]
        public void createjobzone_cmd(Client player, int jobId, string option)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 4)
                return;

            var job = GetJobById(jobId);
            if (job == Job.None)
            {
                API.SendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ Invalid job ID entered.");
                return;
            }

            switch (option)
            {
                case "start":

                    API.SetEntityData(player.handle, "JOB_ZONE_CREATE", job);
                    API.TriggerClientEvent(player, "create_job_zone");
                    API.SendChatMessageToPlayer(player, "Creating zone...");
                    break;
                case "cancel":
                    API.TriggerClientEvent(player, "cancel_job_zone");
                    API.SendChatMessageToPlayer(player, "Job zone creation canceled.");
                    break;
                case "finish":
                    API.TriggerClientEvent(player, "finish_job_zone");
                    break;
                default:
                    API.SendChatMessageToPlayer(player, Color.White, "Invalid option. Valid options are: ~g~start, finish, cancel");
                    break;
            }
        }

        [Command("deletejobzone"), Help(HelpManager.CommandGroups.AdminLevel5, "Deletes a jobzone.", "The jobid", "The zoneid")]
        public void deletejobzone_cmd(Client player, int jobId, int zoneId)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 4)
                return;

            var job = GetJobById(jobId);
            if (job == Job.None)
            {
                API.SendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ Invalid job ID entered.");
                return;
            }

            if (zoneId < 0 || zoneId > job.JobZones.Count)
            {
                API.SendChatMessageToPlayer(player, Color.White, "~R~:ERROR:~W~ Invalid zone id. Valid IDs range from 0 to " + (job.JobZones.Count - 1));
                return;
            }

            job.remove_job_zone(zoneId - 1);
            job.Save();
            API.SendChatMessageToPlayer(player, Color.White, "You have successfully deleted Job " + job.Id + "'s Zone " + zoneId);
        }

        [Command("viewjobzone"), Help(HelpManager.CommandGroups.AdminLevel5, "View a jobzone", "Job Id", "Zone ID")]
        public void viewjobzone_cmd(Client player, int jobId, int zoneId)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 4)
                return;

            var job = GetJobById(jobId);
            if (job == Job.None)
            {
                API.SendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ Invalid job ID entered.");
                return;
            }

            if (zoneId < 0 || zoneId > job.JobZones.Count)
            {
                API.SendChatMessageToPlayer(player, Color.White, "~R~:ERROR:~W~ Invalid zone id. Valid IDs range from 0 to " + (job.JobZones.Count - 1));
                return;
            }

            if (API.GetEntityData(player.handle, "ZONE_MARKER_1") == null)
            {
                var topLeft = new Vector3(job.JobZones[zoneId - 1].X, job.JobZones[zoneId - 1].Y, player.position.Z);
                var topRight = topLeft.Add(new Vector3(job.JobZones[zoneId - 1].Width, 0.0, 0.0));
                var bottomLeft = topLeft.Add(new Vector3(0.0, job.JobZones[zoneId - 1].Height, 0.0));
                var bottomRight =
                    topLeft.Add(new Vector3(job.JobZones[zoneId - 1].Width, job.JobZones[zoneId - 1].Height, 0.0));

                API.SetEntityData(player.handle, "ZONE_MARKER_1",
                    API.CreateMarker(0, topLeft, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 255, 0,
                        player.dimension));
                API.SetEntityData(player.handle, "ZONE_MARKER_2",
                    API.CreateMarker(0, topRight, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 255, 0,
                        player.dimension));
                API.SetEntityData(player.handle, "ZONE_MARKER_3",
                    API.CreateMarker(0, bottomLeft, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 255, 0,
                        player.dimension));
                API.SetEntityData(player.handle, "ZONE_MARKER_4",
                    API.CreateMarker(0, bottomRight, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 255,
                        0, player.dimension));

                API.SendChatMessageToPlayer(player, "Viewing zone " + zoneId + " of Job " + job.Id);
            }
            else
            {
                API.DeleteEntity(API.GetEntityData(player.handle, "ZONE_MARKER_1"));
                API.DeleteEntity(API.GetEntityData(player.handle, "ZONE_MARKER_2"));
                API.DeleteEntity(API.GetEntityData(player.handle, "ZONE_MARKER_3"));
                API.DeleteEntity(API.GetEntityData(player.handle, "ZONE_MARKER_4"));
                API.ResetEntityData(player.handle, "ZONE_MARKER_1");
                API.ResetEntityData(player.handle, "ZONE_MARKER_2");
                API.ResetEntityData(player.handle, "ZONE_MARKER_3");
                API.ResetEntityData(player.handle, "ZONE_MARKER_4");
                API.SendChatMessageToPlayer(player, "No longer viewing job zone.");
            }
        }

        public static Job GetJobById(int id)
        {
            var job = Jobs.FirstOrDefault(x => x.Id == id);
            return job ?? Job.None;
        }

        public static void SendPictureNotificationToJob(Job job, string body, string pic, int flash, int iconType, string sender, string subject)
        {
            foreach(var c in PlayerManager.Players)
            {
                if(c.JobOne == job)
                {
                    API.Shared.SendPictureNotificationToPlayer(c.Client, body, pic, flash, iconType, sender, subject);
                }
            }
        }

        public void load_jobs()
        {
            Jobs = DatabaseManager.JobTable.Find(Builders<Job>.Filter.Empty).ToList();

            foreach(var j in Jobs)
            {
                j.JoinPos = new MarkerZone(j.JoinPos?.Location, j.JoinPos?.Rotation, j.JoinPos.Dimension)
                {
                    ColZoneSize = j.JoinPos.ColZoneSize,
                    ColZoneHeight = j.JoinPos.ColZoneHeight,
                    UseColZone = j.JoinPos.UseColZone,
                    BlipColor = j.JoinPos.BlipColor,
                    BlipSprite = j.sprite_type(),
                    BlipName = j.Name,
                    BlipRange = j.JoinPos.BlipRange,
                    BlipTransparency = j.JoinPos.BlipTransparency,
                    BlipShortRange = j.JoinPos.BlipShortRange,
                    BlipScale = j.JoinPos.BlipScale,
                    UseBlip = j.JoinPos.UseBlip,
                    MarkerColor = j.JoinPos.MarkerColor,
                    MarkerDirection = j.JoinPos.MarkerDirection,
                    MarkerType = j.JoinPos.MarkerType,
                    MarkerScale = j.JoinPos.MarkerScale,
                    UseMarker = j.JoinPos.UseMarker,
                    TextLabelColor = j.JoinPos.TextLabelColor,
                    TextLabelText = "~g~" + j.Name + "~n~/joinjob",
                    TextLabelSeeThrough = j.JoinPos.TextLabelSeeThrough,
                    TextLabelRange = j.JoinPos.TextLabelRange,
                    TextLabelSize = j.JoinPos.TextLabelSize,
                    UseText = j.JoinPos.UseText
                };

                j.JoinPos?.Create();

                if (j.MiscOne != MarkerZone.None)
                {
                    j.MiscOne = new MarkerZone(j.MiscOne?.Location, j.MiscOne?.Rotation, j.MiscOne.Dimension)
                    {
                        ColZoneSize = j.MiscOne.ColZoneSize,
                        ColZoneHeight = j.MiscOne.ColZoneHeight,
                        UseColZone = j.MiscOne.UseColZone,
                        BlipColor = j.MiscOne.BlipColor,
                        BlipSprite = j.MiscOne.BlipSprite,
                        BlipName = j.MiscOne.BlipName,
                        BlipRange = j.MiscOne.BlipRange,
                        BlipTransparency = j.MiscOne.BlipTransparency,
                        BlipShortRange = j.MiscOne.BlipShortRange,
                        BlipScale = j.MiscOne.BlipScale,
                        UseBlip = j.MiscOne.UseBlip,
                        MarkerColor = j.MiscOne.MarkerColor,
                        MarkerDirection = j.MiscOne.MarkerDirection,
                        MarkerType = j.MiscOne.MarkerType,
                        MarkerScale = j.MiscOne.MarkerScale,
                        UseMarker = j.MiscOne.UseMarker,
                        TextLabelColor = j.MiscOne.TextLabelColor,
                        TextLabelText = j.MiscOne.TextLabelText,
                        TextLabelSeeThrough = j.MiscOne.TextLabelSeeThrough,
                        TextLabelRange = j.MiscOne.TextLabelRange,
                        TextLabelSize = j.MiscOne.TextLabelSize,
                        UseText = j.MiscOne.UseText
                    };

                    j.MiscOne.Create();
                }

                if (j.MiscTwo != MarkerZone.None)
                {
                    j.MiscTwo = new MarkerZone(j.MiscTwo?.Location, j.MiscTwo?.Rotation, j.MiscTwo.Dimension)
                    {
                        ColZoneSize = j.MiscTwo.ColZoneSize,
                        ColZoneHeight = j.MiscTwo.ColZoneHeight,
                        UseColZone = j.MiscTwo.UseColZone,
                        BlipColor = j.MiscTwo.BlipColor,
                        BlipSprite = j.MiscTwo.BlipSprite,
                        BlipName = j.MiscTwo.BlipName,
                        BlipRange = j.MiscTwo.BlipRange,
                        BlipTransparency = j.MiscTwo.BlipTransparency,
                        BlipShortRange = j.MiscTwo.BlipShortRange,
                        BlipScale = j.MiscTwo.BlipScale,
                        UseBlip = j.MiscTwo.UseBlip,
                        MarkerColor = j.MiscTwo.MarkerColor,
                        MarkerDirection = j.MiscTwo.MarkerDirection,
                        MarkerType = j.MiscTwo.MarkerType,
                        MarkerScale = j.MiscTwo.MarkerScale,
                        UseMarker = j.MiscTwo.UseMarker,
                        TextLabelColor = j.MiscTwo.TextLabelColor,
                        TextLabelText = j.MiscTwo.TextLabelText,
                        TextLabelSeeThrough = j.MiscTwo.TextLabelSeeThrough,
                        TextLabelRange = j.MiscTwo.TextLabelRange,
                        TextLabelSize = j.MiscTwo.TextLabelSize,
                        UseText = j.MiscTwo.UseText
                    };

                    j.MiscTwo.Create();
                }

                for(var i = 0; i < j.JobZones.Count; i++)
                {
                    j.JobZones[i] = API.Create2DColShape(j.JobZones[i].X, j.JobZones[i].Y, j.JobZones[i].Width,
                        j.JobZones[i].Height);
                    j.register_job_zone_events(i);  
                }

                j.register_job_marker_events();
            }

            DebugManager.DebugMessage("Loaded " + Jobs.Count + " jobs from the database.");
        }

    }
}
