using System;
using System.Collections.Generic;
using System.Linq;

using GTANetworkAPI;

using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.database_manager;
using mtgvrp.phone_manager;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using MongoDB.Driver;

namespace mtgvrp.group_manager.lspd.MDC
{
    public class Bolo
    {
        public int Id;
        public string ReportingOfficer;
        public int Priority;
        public DateTime Time;
        public string Info;

        public Bolo(string reportingOfficer, int priority, string info)
        {
            ReportingOfficer = reportingOfficer;
            Priority = priority;
            Info = info;
            Time = TimeWeatherManager.CurrentTime;
        }
    }

    public class EmergencyCall
    {
        public string PhoneNumber;
        public DateTime Time;
        public string Info;
        public string Location;

        public EmergencyCall(string phoneNumber, string info, string location)
        {
            PhoneNumber = phoneNumber;
            Info = info;
            Location = location;
            Time = TimeWeatherManager.CurrentTime;
        }
    }

    public class Mdc : Script
    {
        public List<Bolo> ActiveBolos = new List<Bolo>();
        public static List<EmergencyCall> Active911S = new List<EmergencyCall>();

        public Mdc()
        {
            Event.OnClientEventTrigger += MDC_onClientEventTrigger;
        }

        [Command("mdc"), Help(HelpManager.CommandGroups.LSPD, "Open the MDC. Must be in vehicle.")]
        public void mdc_cmd(Client player)
        {
            var character = player.GetCharacter();
            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.SendChatMessageToPlayer(player, core.Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            var vehHandle = API.GetPlayerVehicle(player);
            var veh = VehicleManager.GetVehFromNetHandle(vehHandle);
            if(veh.Group.CommandType != Group.CommandTypeLspd)
            {
                API.SendChatMessageToPlayer(player, core.Color.White, "This vehicle is not equipped with a Mobile Database Computer.");
                return;
            }

            if(API.GetPlayerVehicleSeat(player) != -1 && API.GetPlayerVehicleSeat(player) != 0)
            {
                API.SendChatMessageToPlayer(player, core.Color.White, "You can only access the Mobile Database Computer from the front seats.");
                return;
            }

            if (character.IsViewingMdc)
            {
                API.Shared.TriggerClientEvent(character.Client, "hideMDC");
                ChatManager.RoleplayMessage(character, "logs off of the MDC.", ChatManager.RoleplayMe);
                character.IsViewingMdc = false;
            }
            else
            {
                API.Shared.TriggerClientEvent(character.Client, "showMDC");
                ChatManager.RoleplayMessage(character, "logs into the MDC.", ChatManager.RoleplayMe);
                character.IsViewingMdc = true;
            }
        }

        private void MDC_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "server_updateMdcAnnouncement":
                {

                    break;
                }
                case "server_removeBolo":
                {
                    ActiveBolos.RemoveAt((int) arguments[0]);
                    API.SendChatMessageToPlayer(player, "You removed Bolo # " + (int) arguments[0]);
                    break;
                }
                case "server_createBolo":
                {
                    Character character = player.GetCharacter();
                    var newBolo = new Bolo(character.CharacterName, Convert.ToInt32(arguments[1]),
                        Convert.ToString(arguments[0]));

                    ActiveBolos.Add(newBolo);
                    newBolo.Id = ActiveBolos.IndexOf(newBolo);

                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.IsViewingMdc)
                        {
                            SendBoloToClient(c.Client, newBolo);
                        }
                    }

                    API.SendChatMessageToPlayer(player, "Successfully submitted Bolo #" + newBolo.Id);
                    break;
                }
                case "requestMdcInformation":
                {
                    SendAll911ToClient(player);
                    SendAllBoloToClient(player);
                    break;
                }
                case "server_mdc_close":
                {
                    var character = player.GetCharacter();
                    ChatManager.RoleplayMessage(character, "logs off of the MDC.", ChatManager.RoleplayMe);
                    character.IsViewingMdc = false;
                    break;
                }
                case "MDC_SearchForCitizen":
                {
                    var name = (string) arguments[0];
                    var phone = (string) arguments[1];

                    //First if online.


                    Character foundPlayer = null;

                    foreach(var playerfound in PlayerManager.Players)
                        {
                            if (playerfound == null)
                            {
                                continue;
                            }
                            if (playerfound.CharacterName == name)
                            {
                                foundPlayer = playerfound;
                                break;
                            }
                        }

                    if(foundPlayer == null)
                        {
                            foundPlayer = PhoneManager.GetPlayerWithNumber(phone);
                        }

                    if (foundPlayer == null)
                    {
                            var filter = Builders<Character>.Filter.Eq(x => x.CharacterName, name) |
                                     (Builders<Character>.Filter.Eq("Inventory.PhoneNumber", phone));

                        //Not online.. check DB.
                        var found = DatabaseManager.CharacterTable.Find(filter);
                        if (found.Any())
                        {
                            foundPlayer = found.First();
                        }
                    }

                    //If still NULL
                    if (foundPlayer == null)
                    {
                        API.TriggerClientEvent(player, "MDC_SHOW_CITIZEN_INFO", "", "", "", "", "");
                        return;
                    }

                    //GET VEHICLES.
                    var vehicles = DatabaseManager.VehicleTable.Find(x => x.OwnerId == foundPlayer.Id).ToList();
                    var vehiclesList = vehicles.Where(x => x.IsRegistered).Select(x => new[]
                        {VehicleOwnership.returnCorrDisplayName(x.VehModel), x.LicensePlate}).ToArray();

                    //Get amount of crimes.
                    var amountOfPages = Math.Floor((foundPlayer.GetCrimesNumber() + 9d) / 10d);
                    var crimes = GetCrimeArray(foundPlayer);

                   //Store character.
                   player.SetData("MDC_LAST_CHECKED", foundPlayer);

                    //Send Event
                    API.TriggerClientEvent(player, "MDC_SHOW_CITIZEN_INFO", foundPlayer.rp_name(), foundPlayer.Birthday,
                        API.ToJson(vehiclesList), API.ToJson(crimes), amountOfPages);
                    break;
                }

                case "MDC_SearchForVehicle":
                {
                    var lic = (string) arguments[0];
                    vehicle_manager.Vehicle veh = VehicleManager.Vehicles.FirstOrDefault(x => x.LicensePlate == lic) ??
                                                  DatabaseManager.VehicleTable.Find(x => x.LicensePlate == lic)
                                                      .FirstOrDefault();

                    if (veh == null)
                    {
                        API.TriggerClientEvent(player, "MDC_SHOW_VEHICLE_INFO", "", "");
                        return;
                    }

                    API.TriggerClientEvent(player, "MDC_SHOW_VEHICLE_INFO", VehicleOwnership.returnCorrDisplayName(veh.VehModel),
                        veh.OwnerName, API.GetVehicleClassName(API.GetVehicleClass(veh.VehModel)));
                    break;
                }
                case "MDC_RequestNextCrimesPage":
                {
                    Character p = player.GetData("MDC_LAST_CHECKED");
                    if (p == null)
                        return;

                    var next = Convert.ToInt32(arguments[0]);
                    var crimes = GetCrimeArray(p, next);

                        API.TriggerClientEvent(player, "MDC_UPDATE_CRIMES", API.ToJson(crimes));
                    break;
                }
            }
        }

        //Crime Type, Crime Name, DateTime String, OfficerIssued, IsActive
        string[][] GetCrimeArray(Character c, int pageNumber = 1)
        {
            //Determine skip amount: 
            int toSkip = (pageNumber - 1) * 10;

            var crimesList = c.GetCriminalRecord(toSkip);
            var crimesArray = crimesList.Select(x => new[] {x.Crime.Type, x.Crime.Name, x.DateTime.ToString("F"), x.OfficerName, x.ActiveCrime.ToString()}).ToArray();
            return crimesArray;
        }

        public void SendBoloToClient(Client player, Bolo bolo)
        {
            //boloId, officer, time, priority, info
            API.TriggerClientEvent(player, "addBolo", bolo.Id, bolo.ReportingOfficer, bolo.Time.ToString(),
                bolo.Priority, bolo.Info);
        }

        public void Send911ToClient(Client player, EmergencyCall call)
        {
            API.TriggerClientEvent(player, "add911", call.PhoneNumber, call.Time.ToString(), call.Info, call.Location);
        }

        public static void Add911Call(string phoneNumber, string info, string location)
        {
            var emergencyCall = new EmergencyCall(phoneNumber, info, location);
            Active911S.Add(emergencyCall);
        }

        public void SendAllBoloToClient(Client player)
        {
            var orderedBolo = ActiveBolos.OrderByDescending(b => b.Time);

            foreach (var b in orderedBolo.Reverse())
            {
                SendBoloToClient(player, b);
            }
        }

        public void SendAll911ToClient(Client player)
        {
            var ordered911 = Active911S.OrderByDescending(c => c.Time).Take(20);
            foreach (var c in ordered911.Reverse())
            {
                Send911ToClient(player, c);
            }
        }
    }
}
