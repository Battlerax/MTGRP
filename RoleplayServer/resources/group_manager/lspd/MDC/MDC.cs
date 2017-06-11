using System;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.player_manager;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using RoleplayServer.resources.core;

namespace RoleplayServer.resources.group_manager.lspd.MDC
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

    public class MDC : Script
    {
        public List<Bolo> ActiveBolos = new List<Bolo>();
        public static List<EmergencyCall> Active911s = new List<EmergencyCall>();

        public MDC()
        {
            API.onClientEventTrigger += MDC_onClientEventTrigger;
        }

        [Command("mdc")]
        public void mdc_cmd(Client player)
        {
            var character = player.GetCharacter();
            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (character.IsViewingMdc)
            {
                API.shared.triggerClientEvent(character.Client, "hideMDC");
                ChatManager.RoleplayMessage(character, "logs off of the MDC.", ChatManager.RoleplayMe);
                character.IsViewingMdc = false;
            }
            else
            {
                API.shared.triggerClientEvent(character.Client, "showMDC");
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
                    API.sendChatMessageToPlayer(player, "You removed Bolo # " + (int)arguments[0]);
                    break;
                }
                case "server_createBolo":
                {
                    Character character = API.getEntityData(player, "Character");
                    API.sendChatMessageToPlayer(player, "ARGS: " + arguments[0] + " " + arguments[1]);
                    var newBolo = new Bolo(character.CharacterName, Convert.ToInt32(arguments[1]), Convert.ToString(arguments[0]));

                    ActiveBolos.Add(newBolo);
                    newBolo.Id = ActiveBolos.IndexOf(newBolo);

                    foreach (var c in PlayerManager.Players)
                    {
                        if (c.IsViewingMdc)
                        {
                            SendBoloToClient(c.Client, newBolo);
                        }
                    }

                    API.sendChatMessageToPlayer(player, "Successfully submitted Bolo #" + newBolo.Id);
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
            }
            
        }


        public void SendBoloToClient(Client player, Bolo bolo)
        {
            //boloId, officer, time, priority, info
            API.triggerClientEvent(player, "addBolo", bolo.Id, bolo.ReportingOfficer, bolo.Time.ToString(), bolo.Priority, bolo.Info);
        }

        public void Send911ToClient(Client player, EmergencyCall call)
        {
            API.triggerClientEvent(player, "add911", call.PhoneNumber, call.Time.ToString(), call.Info, call.Location);
        }

        public static void Add911Call(string phoneNumber, string info, string location)
        {
            var emergencyCall = new EmergencyCall(phoneNumber, info, location);
            Active911s.Add(emergencyCall);
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
            var ordered911 = Active911s.OrderByDescending(c => c.Time).Take(20);
            foreach (var c in ordered911.Reverse())
            {
                Send911ToClient(player, c);
            }
        }
    }
}