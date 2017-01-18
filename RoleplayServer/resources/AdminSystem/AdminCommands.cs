using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;

namespace RoleplayServer
{
    public class AdminCommands : Script 
    {

        public AdminCommands()
        {
            DebugManager.debugMessage("[AdminSys] Initalizing Admin System...");


            DebugManager.debugMessage("[AdminSys] Admin System initalized.");
        }

        [Command("goto", GreedyArg = true)]
        public void goto_cmd(Client player, string id)
        {
            Client receiver = PlayerManager.parseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.admin_level == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            Vector3 PlayerPos = API.getEntityPosition(receiver);
            API.setEntityPosition(player, new Vector3(PlayerPos.X, PlayerPos.Y + 1, PlayerPos.Z));
            API.sendChatMessageToPlayer(player, "You have teleported to " + PlayerManager.getName(receiver) +" (ID:" + id +").");

        }

        [Command("agiveweapon", GreedyArg = true)]
        public void agiveweapon_cmd(Client player, string id, WeaponHash weaponHash, int ammo)
        {
            Client receiver = PlayerManager.parseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.admin_level == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.givePlayerWeapon(receiver, weaponHash, ammo, true, true);
            API.sendChatMessageToPlayer(player, "You have given Player ID:" + id + " a weapon.");
        }

        [Command("sethealth", GreedyArg = true)]
        public void sethealth_cmd(Client player, string id, int health)
        {
            Client receiver = PlayerManager.parseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.admin_level == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.setPlayerHealth(receiver, health);
            API.sendChatMessageToPlayer(player, "You have set Player ID:" + id + "'s health to " + health + ".");
        }

        [Command("setarmour", GreedyArg = true)]
        public void setarmour_cmd(Client player, string id, int armour)
        {
            Client receiver = PlayerManager.parseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.admin_level == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.setPlayerArmor(receiver, armour);
            API.sendChatMessageToPlayer(player, "You have set Player ID:" + id + "'s armour to " + armour + ".")
        }

        [Command("spec", GreedyArg = true)]
        public void spec_cmd(Client player, string id)
        {
            Client target = PlayerManager.parseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.admin_level == 0)
                return;

            if (target == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            account.is_spectating = true;
            API.setPlayerToSpectatePlayer(player, target);
            API.sendChatMessageToPlayer(player, "You are now spectating " + PlayerManager.getName(target) + " (ID:" + id + "). Use /specoff to stop spectating this player.");

        }

        [Command("specoff", GreedyArg = true)]
        public void specoff_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (account.admin_level == 0)
                return;

            if (account.is_spectating == false)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You are not specing anyone.");
                return;
            }
            account.is_spectating = false;
            API.unspectatePlayer(player);
            API.sendChatMessageToPlayer(player, "You are no longer spectating anyone.");
        }

        [Command("slap", GreedyArg = true)]
        public void slap_cmd(Client player, string id)
        {
            Client receiver = PlayerManager.parseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.admin_level == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            Vector3 PlayerPos = API.getEntityPosition(receiver);
            API.setEntityPosition(receiver, new Vector3(PlayerPos.X, PlayerPos.Y, PlayerPos.Z + 5));
            API.sendChatMessageToPlayer(receiver, "You have been slapped by an admin");
        }
    }
}
