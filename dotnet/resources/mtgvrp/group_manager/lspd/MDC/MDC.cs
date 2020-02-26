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
        }

        [Command("mdc"), Help(HelpManager.CommandGroups.LSPD, "Open the MDC. Must be in vehicle.")]
        public void mdc_cmd(Player player)
        {
            var character = player.GetCharacter();
            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            var vehHandle = NAPI.Player.GetPlayerVehicle(player);
            var veh = VehicleManager.GetVehFromNetHandle(vehHandle);
            if(veh.Group.CommandType != Group.CommandTypeLspd)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "This vehicle is not equipped with a Mobile Database Computer.");
                return;
            }

            if(NAPI.Player.GetPlayerVehicleSeat(player) != -1 && NAPI.Player.GetPlayerVehicleSeat(player) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "You can only access the Mobile Database Computer from the front seats.");
                return;
            }

            if (character.IsViewingMdc)
            {
                API.Shared.TriggerClientEvent(character.Player, "hideMDC");
                ChatManager.RoleplayMessage(character, "logs off of the MDC.", ChatManager.RoleplayMe);
                character.IsViewingMdc = false;
            }
            else
            {
                API.Shared.TriggerClientEvent(character.Player, "showMDC");
                ChatManager.RoleplayMessage(character, "logs into the MDC.", ChatManager.RoleplayMe);
                character.IsViewingMdc = true;
            }
        }

        [RemoteEvent("server_updateMdcAnnouncement")]
        public void UpdateMdcAnnouncement(Player player, params object[] arguments)
        {
            // i guess there was no code written for this... - austin
            // TODO: find out what this needs and add it i guess
        }

        [RemoteEvent("server_removeBolo")]
        public void RemoveBolo(Player player, params object[] arguments)
        {
            ActiveBolos.RemoveAt((int)arguments[0]);
            NAPI.Chat.SendChatMessageToPlayer(player, "You removed Bolo # " + (int)arguments[0]);
        }

        [RemoteEvent("server_createBolo")]
        public void CreateBolo(Player player, params object[] arguments)
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
                    SendBoloToClient(c.Player, newBolo);
                }
            }

            NAPI.Chat.SendChatMessageToPlayer(player, "Successfully submitted Bolo #" + newBolo.Id);
        }

        [RemoteEvent("requestMdcInformation")]
        public void RequestMdcInformation(Player player, params object[] arguments)
        {
            SendAll911ToClient(player);
            SendAllBoloToClient(player);
        }

        [RemoteEvent("server_mdc_close")]
        public void MdcClose(Player player, params object[] arguments)
        {
            var character = player.GetCharacter();
            ChatManager.RoleplayMessage(character, "logs off of the MDC.", ChatManager.RoleplayMe);
            character.IsViewingMdc = false;
        }

        [RemoteEvent("MDC_SearchForCitizen")]
        public void MDCSearchForCitizen(Player player, params object[] arguments)
        {
            var name = (string)arguments[0];
            var phone = (string)arguments[1];

            //First if online.


            Character foundPlayer = null;

            foreach (var playerfound in PlayerManager.Players)
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

            if (foundPlayer == null)
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
                NAPI.ClientEvent.TriggerClientEvent(player, "MDC_SHOW_CITIZEN_INFO", "", "", "", "", "");
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
            NAPI.ClientEvent.TriggerClientEvent(player, "MDC_SHOW_CITIZEN_INFO", foundPlayer.rp_name(), foundPlayer.Birthday,
            NAPI.Util.ToJson(vehiclesList), NAPI.Util.ToJson(crimes), amountOfPages);
        }

        [RemoteEvent("MDC_SearchForVehicle")]
        public void MDCSearchForVehicle(Player player, params object[] arguments)
        {
            var lic = (string)arguments[0];
            vehicle_manager.GameVehicle veh = VehicleManager.Vehicles.FirstOrDefault(x => x.LicensePlate == lic) ??
                                          DatabaseManager.VehicleTable.Find(x => x.LicensePlate == lic)
                                              .FirstOrDefault();

            if (veh == null)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "MDC_SHOW_VEHICLE_INFO", "", "");
                return;
            }

            NAPI.ClientEvent.TriggerClientEvent(player, "MDC_SHOW_VEHICLE_INFO", VehicleOwnership.returnCorrDisplayName(veh.VehModel),
                veh.OwnerName, API.GetVehicleClassName(NAPI.Vehicle.GetVehicleClass(veh.VehModel)));
        }

        [RemoteEvent("MDC_RequestNextCrimesPage")]
        public void MDCRequestNextCrimesPage(Player player, params object[] arguments)
        {
            Character p = player.GetData<Character>("MDC_LAST_CHECKED");
            if (p == null)
                return;

            var next = Convert.ToInt32(arguments[0]);
            var crimes = GetCrimeArray(p, next);

            NAPI.ClientEvent.TriggerClientEvent(player, "MDC_UPDATE_CRIMES", NAPI.Util.ToJson(crimes));
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

        public void SendBoloToClient(Player player, Bolo bolo)
        {
            //boloId, officer, time, priority, info
            NAPI.ClientEvent.TriggerClientEvent(player, "addBolo", bolo.Id, bolo.ReportingOfficer, bolo.Time.ToString(),
                bolo.Priority, bolo.Info);
        }

        public void Send911ToClient(Player player, EmergencyCall call)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "add911", call.PhoneNumber, call.Time.ToString(), call.Info, call.Location);
        }

        public static void Add911Call(string phoneNumber, string info, string location)
        {
            var emergencyCall = new EmergencyCall(phoneNumber, info, location);
            Active911S.Add(emergencyCall);
        }

        public void SendAllBoloToClient(Player player)
        {
            var orderedBolo = ActiveBolos.OrderByDescending(b => b.Time);

            foreach (var b in orderedBolo.Reverse())
            {
                SendBoloToClient(player, b);
            }
        }

        public void SendAll911ToClient(Player player)
        {
            var ordered911 = Active911S.OrderByDescending(c => c.Time).Take(20);
            foreach (var c in ordered911.Reverse())
            {
                Send911ToClient(player, c);
            }
        }
    }
}
