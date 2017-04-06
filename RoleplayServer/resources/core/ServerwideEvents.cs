using System;
using GTANetworkServer;
using GTANetworkShared;

namespace RoleplayServer.resources.core
{
    public class ServerwideEvents : Script
    {
        public ServerwideEvents()
        {
            API.onUpdate += OnUpdateHandler;
        }

        public void payCheck(Client player, int amount)
        {
            var currentMoney = API.getEntityData(player, "Money"); //replace with mongodb stuff
            API.setEntityData(player, "Money", currentMoney + amount); 
            API.sendChatMessageToPlayer(player, "Paycheck received! You have earned " + amount + "$.");
            int money = API.getEntityData(player, "Money");
            API.triggerClientEvent(player, "update_money_display", money);
        }

        public DateTime LastAnnounce;
        public void OnUpdateHandler()
        {
            if (DateTime.Now.Subtract(LastAnnounce).TotalSeconds >= 5)
            {
                var allPlayers = API.getAllPlayers();
                foreach (Client player in allPlayers)
                {
                    if (API.getEntityData(player, "LOGGED_IN") == true)
                    {
                        var currentHours = API.getEntityData(player, "playinghours");
                        var currentSeconds = API.getEntityData(player, "playerseconds");
                        API.setEntityData(player, "playerseconds", currentSeconds + 5);
                        if (currentSeconds >= 3600)
                        {
                            API.setEntityData(player, "playinghours", currentHours + 1);
                            payCheck(player, 500); //amount will be determined by taxation. Taxation is changed by admin command.
                            API.setEntityData(player, "playerseconds", 0);
                        }
                    }
                }
                LastAnnounce = DateTime.Now;
            }
        }
    }
}
