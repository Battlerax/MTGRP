using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Timers;


using GTANetworkAPI;





using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using Color = mtgvrp.core.Color;
using GameVehicle = mtgvrp.vehicle_manager.GameVehicle;

namespace mtgvrp.job_manager.taxi
{
    public class TaxiJob : Script
    {
        public const int MinFare = 5;
        public const int MaxFare = 50;

        public static List<Character> OnDutyDrivers => PlayerManager.Players.Where(x => x.TaxiDuty == true).ToList();
        public static List<Character> TaxiRequests = new List<Character>();

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, byte type, string reason)
        {
            var c = player.GetCharacter();
            if (c == null)
                return;

            if (TaxiRequests.Contains(c))
            {
                TaxiRequests.Remove(c);
                c.TaxiDriver = null;
                c.TaxiTimer.Stop();
                c.TotalFare = 0;
            }
        }

        [RemoteEvent("update_taxi_destination")]
        public void UpdateTaxiDestination(Player player, params object[] arguments)
        {
            Character character = player.GetCharacter();
            NAPI.ClientEvent.TriggerClientEvent(character.TaxiDriver.Player, "set_taxi_waypoint", (Vector3)arguments[0]);

            NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You have successfully set your destination.");
            NAPI.Chat.SendChatMessageToPlayer(character.TaxiDriver.Player, "[TAXI] " + character.rp_name() + " has set the destination.");
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            Character character = player.GetCharacter();
            var veh = VehicleManager.GetVehFromNetHandle(vehicle);

            if (character == null)
                return;

            if(veh != null)
            {
                if (OnDutyDrivers.Contains(character) && veh.Job.Type == JobManager.JobTypes.Taxi)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You have left your taxi. Please return to it within 60 seconds or you will be taken off-duty and it will respawn.");

                    veh.CustomRespawnTimer = new Timer {Interval = 1000 * 60};
                    veh.CustomRespawnTimer.Elapsed += delegate { RespawnTaxi(character, veh); };
                    veh.CustomRespawnTimer.Start();
                }

                if(veh.Driver != null && character.TaxiDriver != null)
                {
                    if (veh.Driver == character.TaxiDriver)
                    {
                        InventoryManager.GiveInventoryItem(veh.Driver, new Money(), character.TotalFare);
                        InventoryManager.DeleteInventoryItem(character, typeof(Money), character.TotalFare);

                        veh.Driver.Save();
                        character.Save();

                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~[TAXI] You have been charged $" + character.TotalFare + " for your taxi ride.");
                        NAPI.Chat.SendChatMessageToPlayer(veh.Driver.Player, "~y~[TAXI] You have been paid $" + character.TotalFare + " for your services.");

                        NAPI.ClientEvent.TriggerClientEvent(player, "update_fare_display", 0, 0, "");
                        NAPI.ClientEvent.TriggerClientEvent(veh.Driver.Player, "update_fare_display", 0, 0, "");

                        LogManager.Log(LogManager.LogTypes.Stats, $"[Job] {veh.Driver.CharacterName}[{veh.Driver.Player.GetAccount().AccountName}] has earned ${character.TotalFare} from a taxi fare. (Fare: {character.CharacterName})");
                        LogManager.Log(LogManager.LogTypes.Stats, $"[Job] {character.CharacterName}[{player.GetAccount().AccountName}] has paided ${character.TotalFare} for a taxi fare. (Driver: {veh.Driver.CharacterName})");

                        veh.Driver.TaxiPassenger = null;
                        character.TaxiDriver = null;
                        character.TaxiTimer.Stop();
                        character.TotalFare = 0;
                    }
                }
            }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seat)
        {
            Character character = player.GetCharacter();
            var veh = VehicleManager.GetVehFromNetHandle(vehicle);

            if(veh == null)
                return;

            //Cancel taxi car respawn 
            if (OnDutyDrivers.Contains(character) && veh.Job.Type == JobManager.JobTypes.Taxi)
            {
                if (veh.CustomRespawnTimer.Enabled && seat == 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You have returned to your taxi and will no longer be taken off-duty.");
                    veh.CustomRespawnTimer.Stop();
                }
            }

            //Check for passengers entering available cabs
            if (seat != 0)
            {
                if (veh.Job?.Type == JobManager.JobTypes.Taxi)
                {
                    if (veh.Driver.Player == player)
                    {
                        player.SendChatMessage("You cannot enter your own taxi.");
                        API.WarpPlayerOutOfVehicle(player);
                        return;
                    }
                    if (veh.Driver == null)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow, "[TAXI] This taxi currently has no driver.");
                        //API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));
                        Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player)); // CONV NOTE: delay fixme
                        return;
                    }

                    if (veh.Driver.TaxiPassenger == null && OnDutyDrivers.Contains(veh.Driver) && TaxiRequests.Contains(character))
                    {
                        /*if (!taxi_requests.Contains(character))
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You must have an active taxi request to ride in a taxi. ( /requesttaxi )");
                            API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));;
                            return;
                        }

                        if (veh.driver.taxi_passenger != null)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow, "[TAXI] This driver already has an active fare.");
                            return;
                        }*/

                       
                        veh.Driver.TaxiPassenger = character;
                        character.TaxiDriver = veh.Driver;
                        TaxiRequests.Remove(character);

                        TaxiPictureNotification(player, veh.Driver.rp_name() + " has accepted your taxi request.", subject: "~y~Request Accepted");
                        player.SendChatMessage(veh.Driver.rp_name() + " has accepted your taxi request.");
                        SendMessageToOnDutyDrivers(veh.Driver.rp_name() + " has accepted " + character.rp_name() + "'s taxi request.");
                        NAPI.Chat.SendChatMessageToPlayer(player, "[TAXI] Please set a destination on your map and then type: /setdestination");
                        player.SendChatMessage("[TAXI] Please set a destination on your map and then type: /setdestination");

                        NAPI.ClientEvent.TriggerClientEvent(player, "update_fare_display", veh.Driver.TaxiFare, 0, "");
                        NAPI.ClientEvent.TriggerClientEvent(veh.Driver.Player, "update_fare_display", veh.Driver.TaxiFare, 0, "");
                    }
                    else if(veh.Driver.TaxiPassenger == character)
                    {
                        NAPI.ClientEvent.TriggerClientEvent(player, "update_fare_display", veh.Driver.TaxiFare, 0, "");
                        NAPI.ClientEvent.TriggerClientEvent(veh.Driver.Player, "update_fare_display", veh.Driver.TaxiFare, 0, "");

                        NAPI.Chat.SendChatMessageToPlayer(player, "[TAXI] Please set a destination on your map and then type: /setdestination");
                    }
                }
            }
        }

        public void RespawnTaxi(Character character, GameVehicle veh)
        {
            if (API.IsPlayerConnected(character.Player))
            {
                NAPI.Chat.SendChatMessageToPlayer(character.Player, Color.Yellow, "[TAXI] You were out of your taxi for too long and have taken off-duty. The taxi has been respawned.");

                if (OnDutyDrivers.Contains(character))
                {
                    character.TaxiDuty = false;
                }
                SendMessageToOnDutyDrivers(character.rp_name() + " has gone off of taxi duty.");
            }

            NAPI.Vehicle.SetVehicleEngineStatus(veh.Entity, false);
            veh.CustomRespawnTimer.Stop();
            VehicleManager.respawn_vehicle(veh);
        }

        [Command("canceltaxi"), Help(HelpManager.CommandGroups.TaxiJob, "Cancel your taxi request.")]
        public void canceltaxi_cmd(Player player)
        {
            Character character = player.GetCharacter();

            if (!TaxiRequests.Contains(character))
            {
                TaxiPictureNotification(player, "Our system shows you do not have a taxi request submitted.");
                return;
            }

            TaxiRequests.Remove(character);
            player.SendChatMessage("Taxi request cancelled.");
        }

        [Command("taxiduty"), Help(HelpManager.CommandGroups.TaxiJob, "Toggle taxi duty.")]
        public void taxiduty_cmd(Player player)
        {
            Character character = player.GetCharacter();

            if (character.JobOne.Type != JobManager.JobTypes.Taxi)
            {
                NAPI.Notification.SendPictureNotificationToPlayer(player, "You must be a taxi driver to use this command.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (!NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                NAPI.Notification.SendPictureNotificationToPlayer(player, "You must be in a taxi to use this command.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (NAPI.Player.GetPlayerVehicleSeat(player) != -1)
            {
                NAPI.Notification.SendPictureNotificationToPlayer(player, "You must be in the driver seat of a taxi to use this command.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            var veh = VehicleManager.GetVehFromNetHandle(NAPI.Player.GetPlayerVehicle(player));

            if(veh.Job == null)
            {
                NAPI.Notification.SendPictureNotificationToPlayer(player, "You must be driving a taxi car to go on taxi duty.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (veh.Job.Type != JobManager.JobTypes.Taxi)
            {
                NAPI.Notification.SendPictureNotificationToPlayer(player, "You must be driving a taxi car to go on taxi duty.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (!OnDutyDrivers.Contains(character))
            {
                character.TaxiDuty = true;
                SendMessageToOnDutyDrivers(character.rp_name() + " is now on taxi duty.");
                NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You are now on taxi duty. If you leave your vehicle you will be taken off duty automatically.");
            }
            else
            {
                character.TaxiDuty = false;
                SendMessageToOnDutyDrivers(character.rp_name() + " has gone off of taxi duty.");
                NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You have gone off of taxi duty.");
            }
        }

        [Command("setfare"), Help(HelpManager.CommandGroups.TaxiJob, "Sets your taxi fare.", "The price you'd like to set as the fare.")]
        public void setfare_cmd(Player player, int farePrice)
        {
            Character character = player.GetCharacter();

            if(character.JobOne.Type != JobManager.JobTypes.Taxi)
            {
                NAPI.Notification.SendPictureNotificationToPlayer(player, "You must be a taxi driver to use this command.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            if(farePrice < MinFare || farePrice > MaxFare)
            {
                NAPI.Notification.SendPictureNotificationToPlayer(player, "Your fare price must be between $" + MinFare + " and $" + MaxFare + ".", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            if(character.TaxiPassenger != null)
            {
                NAPI.Notification.SendPictureNotificationToPlayer(player, "You can't change your fare while you have a passenger!", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            character.TaxiFare = farePrice;
            character.Save();
            NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You have changed your taxi fare to $" + farePrice + ".");
        }

        [Command("requesttaxi"), Help(HelpManager.CommandGroups.TaxiJob, "Request a taxi.")]
        public void requesttaxi_cmd(Player player)
        {
            Character character = player.GetCharacter();

            if (OnDutyDrivers.Contains(character))
            {
                player.SendChatMessage("You can't request a taxi while on taxi duty.");
                return;
            }

            if (TaxiRequests.Contains(character))
            {
                TaxiPictureNotification(player, "Our system shows you already have a taxi request submitted. If this is incorrect, use /canceltaxi");
                return;
            }

            if(character.TaxiDriver != null)
            {
                TaxiPictureNotification(player, "Our logs show you are already riding in a taxi.");
                return;
            }

            if(OnDutyDrivers.Count == 0)
            {
                TaxiPictureNotification(player, "Downtown Cab Co currently has no on duty drivers. Sorry for the inconvience.");
                return;
            }

            TaxiRequests.Add(character);
            TaxiPictureNotification(player, "Your taxi request has been submitted. Please wait patiently for a response.");
            player.SendChatMessage("Taxi request submitted.");
            
            foreach(var c in OnDutyDrivers)
            {
                TaxiPictureNotification(c.Player, character.rp_name() + " has requested a taxi. (" +  (c.Player.Position.DistanceTo(character.Player.Position) / 1000)+ "KM away) ((ID: " + PlayerManager.GetPlayerId(character) + "))");
                player.SendChatMessage(character.rp_name() + " has requested a taxi. (" + (c.Player.Position.DistanceTo(character.Player.Position) / 1000) + "KM away. /acceptfare to accept.");
            }
        }

        [Command("setdestination"), Help(HelpManager.CommandGroups.TaxiJob, "Sets the taxi destintion, after you get in.")]
        public void setdestination_cmd(Player player)
        {
            if (!NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                NAPI.Notification.SendPictureNotificationToPlayer(player, "You must be inside a taxi to use this command.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            Character character = player.GetCharacter();
            var veh = VehicleManager.GetVehFromNetHandle(NAPI.Player.GetPlayerVehicle(player));

            if(veh.Driver.TaxiPassenger != character)
            {
                NAPI.Notification.SendPictureNotificationToPlayer(player, "You are not inside a taxi which has accepted your ride request.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            NAPI.ClientEvent.TriggerClientEvent(player, "get_waypoint_position");

            character.TaxiStart = character.Player.Position;

            character.TaxiTimer = new Timer {Interval = 2000};
            character.TaxiTimer.Elapsed += delegate { TaxiMeterTimer(character); };
            character.TaxiTimer.Start();
        }

        public void TaxiMeterTimer(Character c)
        {
            c.TotalFare = (int)Math.Round(c.Player.Position.DistanceTo(c.TaxiStart) / 100) * c.TaxiDriver.TaxiFare;
            var fareMsg = "";

            if(c.TotalFare > Money.GetCharacterMoney(c))
            {
                c.TotalFare = Money.GetCharacterMoney(c);
                fareMsg = "(Player money maxed out)";
            }

            NAPI.ClientEvent.TriggerClientEvent(c.Player, "update_fare_display", c.TaxiDriver.TaxiFare, c.TotalFare, fareMsg);
            NAPI.ClientEvent.TriggerClientEvent(c.TaxiDriver.Player, "update_fare_display", c.TaxiDriver.TaxiFare, c.TotalFare, fareMsg);
        }

        [Command("acceptfare"), Help(HelpManager.CommandGroups.TaxiJob, "Accepts a taxi fare.", "Id of the player you'd like to accept.")]
        public void acceptfare_cmd(Player player, string id)
        {
            Character character = player.GetCharacter();
            if(character.JobOne.Type != JobManager.JobTypes.Taxi)
            {
                NAPI.Notification.SendPictureNotificationToPlayer(player, "You must be a taxi driver to use this command.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            var passengerClient = PlayerManager.ParseClient(id);

            if (passengerClient == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (passengerClient == player)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ You cannot accept your own fare.");
                return;
            }

            Character passenger = passengerClient.GetCharacter();

            if (!TaxiRequests.Contains(passenger))
            {
                TaxiPictureNotification(player, "This customer has already had their fare accepted or does not have an active fare.");
                return;
            }

            character.TaxiPassenger = passenger;
            passenger.TaxiDriver = character;
            TaxiRequests.Remove(passenger);

            TaxiPictureNotification(passenger.Player, character.rp_name() + " has accepted your taxi request. Please stay at your current location.");
            TaxiPictureNotification(player, "You have accepted " + passenger.rp_name() + "'s taxi request. Follow your waypoint to their location.");
            player.SendChatMessage("You have accepted " + passenger.rp_name() + "'s taxi request. Follow your waypoint to their location.");
            passenger.Player.SendChatMessage(character.rp_name() + " has accepted your taxi request. Please stay at your current location.");
            
            NAPI.ClientEvent.TriggerClientEvent(player, "set_taxi_waypoint", passenger.Player.Position);
        }

        public static bool IsOnTaxiDuty(Character c)
        {
            return OnDutyDrivers.Contains(c);
        }

        public void TaxiPictureNotification(Player player, string body, string sender = "Downton Cab Co", string subject = "Dispatch Message")
        {
            NAPI.Notification.SendPictureNotificationToPlayer(player, body, "CHAR_TAXI", 0, 1, sender, subject);
        }

        public void SendMessageToOnDutyDrivers(string body, string sender = "Downtown Cab Co", string subject = "Dispatch Message")
        {
            foreach(var c in OnDutyDrivers)
            {
                TaxiPictureNotification(c.Player, body, sender, subject);
            }
        }
    }
}
