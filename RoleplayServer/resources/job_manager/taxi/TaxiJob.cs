using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using System.Timers;

namespace RoleplayServer
{
    public class TaxiJob : Script
    {
        public const int MIN_FARE = 5;
        public const int MAX_FARE = 50;

        public static List<Character> on_duty_drivers = new List<Character>();
        public static List<Character> taxi_requests = new List<Character>();

        public TaxiJob()
        {
            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;
            API.onPlayerExitVehicle += API_onPlayerExitVehicle;
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "update_taxi_destination":
                    Character character = API.getEntityData(player.handle, "Character");
                    API.triggerClientEvent(character.taxi_driver.client, "set_taxi_waypoint", (Vector3)arguments[0]);

                    API.sendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You have successfully set your destination.");
                    API.sendChatMessageToPlayer(character.taxi_driver.client, "[TAXI] " + character.rp_name() + " has set the destination.");
                    break;
            }
        }

        private void API_onPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            Character character = API.getEntityData(player.handle, "Character");
            Vehicle veh = VehicleManager.getVehFromNetHandle(vehicle);

            if(veh != null)
            {
                if (on_duty_drivers.Contains(character) && veh.job.type == JobManager.TaxiJob)
                {
                    API.sendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You have left your taxi. Please return to it within 60 seconds or you will be taken off-duty and it will respawn.");

                    veh.respawn_timer = new System.Timers.Timer();
                    veh.respawn_timer.Interval = 1000 * 60;
                    veh.respawn_timer.Elapsed += delegate { RespawnTaxi(character, veh); };
                    veh.respawn_timer.Start();
                }

                if(veh.driver != null && character.taxi_driver != null)
                {
                    if (veh.driver == character.taxi_driver)
                    {
                        veh.driver.money += character.total_fare;
                        character.money -= character.total_fare;

                        veh.driver.save();
                        character.save();

                        API.sendChatMessageToPlayer(player, "~y~[TAXI] You have been charged $" + character.total_fare + " for your taxi ride.");
                        API.sendChatMessageToPlayer(veh.driver.client, "~y~[TAXI] You have been paid $" + character.total_fare + " for your services.");

                        API.triggerClientEvent(player, "update_fare_display", 0, 0, "");
                        API.triggerClientEvent(veh.driver.client, "update_fare_display", 0, 0, "");

                        veh.driver.taxi_passenger = null;
                        character.taxi_driver = null;
                        character.taxi_timer.Stop();
                        character.total_fare = 0;
                    }
                }
            }
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            Character character = API.getEntityData(player.handle, "Character");
            Vehicle veh = VehicleManager.getVehFromNetHandle(vehicle);

            //Cancel taxi car respawn 
            if (on_duty_drivers.Contains(character) && veh.job.type == JobManager.TaxiJob)
            {
                if (veh.respawn_timer.Enabled == true && API.getPlayerVehicleSeat(player) == -1)
                {
                    API.sendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You have returned to your taxi and will no longer be taken off-duty.");
                    veh.respawn_timer.Stop();
                }
            }

            //Check for passengers entering available cabs
            if (API.getPlayerVehicleSeat(player) != -1)
            {
                if (veh.job.type == JobManager.TaxiJob)
                {
                    if (veh.driver == null)
                    {
                        API.sendChatMessageToPlayer(player, Color.Yellow, "[TAXI] This taxi currently has no driver.");
                        API.warpPlayerOutOfVehicle(player, vehicle);
                        return;
                    }

                    if (veh.driver.taxi_passenger == null && on_duty_drivers.Contains(veh.driver) && taxi_requests.Contains(character))
                    {
                        /*if (!taxi_requests.Contains(character))
                        {
                            API.sendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You must have an active taxi request to ride in a taxi. ( /requesttaxi )");
                            API.warpPlayerOutOfVehicle(player, vehicle);
                            return;
                        }

                        if (veh.driver.taxi_passenger != null)
                        {
                            API.sendChatMessageToPlayer(player, Color.Yellow, "[TAXI] This driver already has an active fare.");
                            return;
                        }*/

                       
                        veh.driver.taxi_passenger = character;
                        character.taxi_driver = veh.driver;
                        taxi_requests.Remove(character);

                        taxiPictureNotification(player, veh.driver.rp_name() + " has accepted your taxi request.", subject: "~y~Request Accepted");
                        sendMessageToOnDutyDrivers(veh.driver.rp_name() + " has accepted " + character.rp_name() + "'s taxi request.");
                        API.sendChatMessageToPlayer(player, "[TAXI] Please set a destination on your map and then type: /setdestination");

                        API.triggerClientEvent(player, "update_fare_display", veh.driver.taxi_fare, 0, "");
                        API.triggerClientEvent(veh.driver.client, "update_fare_display", veh.driver.taxi_fare, 0, "");
                    }
                    else if(veh.driver.taxi_passenger == character)
                    {
                        API.triggerClientEvent(player, "update_fare_display", veh.driver.taxi_fare, 0, "");
                        API.triggerClientEvent(veh.driver.client, "update_fare_display", veh.driver.taxi_fare, 0, "");

                        API.sendChatMessageToPlayer(player, "[TAXI] Please set a destination on your map and then type: /setdestination");
                    }
                }
            }
        }

        public void RespawnTaxi(Character character, Vehicle veh)
        {
            if (API.isPlayerConnected(character.client))
            {
                API.sendChatMessageToPlayer(character.client, Color.Yellow, "[TAXI] You were out of your taxi for too long and have taken off-duty. The taxi has been respawned.");

                if (on_duty_drivers.Contains(character))
                {
                    on_duty_drivers.Remove(character);
                }
                sendMessageToOnDutyDrivers(character.rp_name() + " has gone off of taxi duty.");
            }

            API.setVehicleEngineStatus(veh.net_handle, false);
            veh.respawn_timer.Stop();
            VehicleManager.respawn_vehicle(veh);
        }

        [Command("taxiduty")]
        public void taxiduty_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.job_one.type != JobManager.TaxiJob)
            {
                API.sendPictureNotificationToPlayer(player, "You must be a taxi driver to use this command.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (!API.isPlayerInAnyVehicle(player))
            {
                API.sendPictureNotificationToPlayer(player, "You must be in a taxi to use this command.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (API.getPlayerVehicleSeat(player) != -1)
            {
                API.sendPictureNotificationToPlayer(player, "You must be in the driver seat of a taxi to use this command.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            Vehicle veh = VehicleManager.getVehFromNetHandle(API.getPlayerVehicle(player));

            if(veh.job == null)
            {
                API.sendPictureNotificationToPlayer(player, "You must be driving a taxi car to go on taxi duty.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (veh.job.type != JobManager.TaxiJob)
            {
                API.sendPictureNotificationToPlayer(player, "You must be driving a taxi car to go on taxi duty.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (!on_duty_drivers.Contains(character))
            {
                on_duty_drivers.Add(character);
                sendMessageToOnDutyDrivers(character.rp_name() + " is now on taxi duty.");
                API.sendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You are now on taxi duty. If you leave your vehicle you will be taken off duty automatically.");
            }
            else
            {
                on_duty_drivers.Remove(character);
                sendMessageToOnDutyDrivers(character.rp_name() + " has gone off of taxi duty.");
                API.sendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You have gone off of taxi duty.");
            }
        }

        [Command("setfare")]
        public void setfare_cmd(Client player, int fare_price)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if(character.job_one.type != JobManager.TaxiJob)
            {
                API.sendPictureNotificationToPlayer(player, "You must be a taxi driver to use this command.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            if(fare_price < MIN_FARE || fare_price > MAX_FARE)
            {
                API.sendPictureNotificationToPlayer(player, "Your fair price must be between $" + MIN_FARE + " and $" + MAX_FARE + ".", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            if(character.taxi_passenger != null)
            {
                API.sendPictureNotificationToPlayer(player, "You can't change your fare while you have a passenger!", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            character.taxi_fare = fare_price;
            character.save();
            API.sendChatMessageToPlayer(player, Color.Yellow, "[TAXI] You have changed your taxi fare to $" + fare_price + ".");
        }

        [Command("requesttaxi")]
        public void requesttaxi_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (taxi_requests.Contains(character))
            {
                taxiPictureNotification(player, "Our system shows you already have a taxi request submitted. If this is incorrect, use /canceltaxi");
                return;
            }

            if(character.taxi_driver != null)
            {
                taxiPictureNotification(player, "Our logs show you are already riding in a taxi.");
                return;
            }

            if(on_duty_drivers.Count == 0)
            {
                taxiPictureNotification(player, "Downtown Cab Co currently has no on duty drivers. Sorry for the inconvience.");
                return;
            }

            taxi_requests.Add(character);
            taxiPictureNotification(player, "Your taxi request has been submitted. Please wait patiently for a response.");
            
            foreach(Character c in on_duty_drivers)
            {
                taxiPictureNotification(c.client, character.rp_name() + " has requested a taxi. (" +  (c.client.position.DistanceTo(character.client.position) / 1000).ToString()+ "KM away) ((ID: " + PlayerManager.getPlayerId(character) + "))");
            }

            return;
        }

        [Command("setdestination")]
        public void setdestination_cmd(Client player)
        {
            if (!API.isPlayerInAnyVehicle(player))
            {
                API.sendPictureNotificationToPlayer(player, "You must be inside a taxi to use this command.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            Character character = API.getEntityData(player.handle, "Character");
            Vehicle veh = VehicleManager.getVehFromNetHandle(API.getPlayerVehicle(player));

            if(veh.driver.taxi_passenger != character)
            {
                API.sendPictureNotificationToPlayer(player, "You are not inside a taxi which has accepted your ride request.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            API.triggerClientEvent(player, "get_waypoint_position");

            character.taxi_start = character.client.position;

            character.taxi_timer = new Timer();
            character.taxi_timer.Interval = 2000;
            character.taxi_timer.Elapsed += delegate { TaxiMeterTimer(character); };
            character.taxi_timer.Start();
        }

        public void TaxiMeterTimer(Character c)
        {
            c.total_fare = (int)Math.Round(c.client.position.DistanceTo(c.taxi_start) / 100) * c.taxi_driver.taxi_fare;
            string fare_msg = "";

            if(c.total_fare > c.money)
            {
                c.total_fare = c.money;
                fare_msg = "(Client money maxed out)";
            }

            API.triggerClientEvent(c.client, "update_fare_display", c.taxi_driver.taxi_fare, c.total_fare, fare_msg);
            API.triggerClientEvent(c.taxi_driver.client, "update_fare_display", c.taxi_driver.taxi_fare, c.total_fare, fare_msg);
        }

        [Command("acceptfare")]
        public void acceptfare_cmd(Client player, string id)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if(character.job_one.type != JobManager.TaxiJob)
            {
                API.sendPictureNotificationToPlayer(player, "You must be a taxi driver to use this command.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            Client passenger_client = PlayerManager.parseClient(id);

            if (passenger_client == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character passenger = API.getEntityData(passenger_client.handle, "Character");

            if (!taxi_requests.Contains(passenger))
            {
                taxiPictureNotification(player, "This customer has already had their fare accepted or does not have an active fare.");
                return;
            }

            character.taxi_passenger = passenger;
            passenger.taxi_driver = character;
            taxi_requests.Remove(passenger);

            taxiPictureNotification(passenger.client, character.rp_name() + " has accepted your taxi request. Please stay at your current location.");
            taxiPictureNotification(player, "You have accepted " + passenger.rp_name() + "'s taxi request. Follow your waypoint to their location.");

            API.triggerClientEvent(player, "set_taxi_waypoint", passenger.client.position);
        }

        public static bool isOnTaxiDuty(Character c)
        {
            return on_duty_drivers.Contains(c);
        }

        public void taxiPictureNotification(Client player, string body, string sender = "Downton Cab Co", string subject = "Dispatch Message")
        {
            API.sendPictureNotificationToPlayer(player, body, "CHAR_TAXI", 0, 1, sender, subject);
        }

        public void sendMessageToOnDutyDrivers(string body, string sender = "Downtown Cab Co", string subject = "Dispatch Message")
        {
            foreach(Character c in on_duty_drivers)
            {
                taxiPictureNotification(c.client, body, sender, subject);
            }
        }
    }
}
