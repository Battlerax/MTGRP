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

        [Command("gotopos")]
        public void gotopos_cmd(Client player, double x, double y, double z)
        {
            Vector3 pos = new Vector3(x, y, z);
            API.setEntityPosition(player, pos);
            API.sendChatMessageToPlayer(player, "Teleported");
        }

        [Command("goto")]
        public static void goto_cmd(Client player, string id)
        {
            Client receiver = PlayerManager.parseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.admin_level == 0)
                return;

            if (receiver == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            Vector3 PlayerPos = API.shared.getEntityPosition(receiver);
            API.shared.setEntityPosition(player, new Vector3(PlayerPos.X, PlayerPos.Y + 1, PlayerPos.Z));
            API.shared.sendChatMessageToPlayer(player, "You have teleported to " + PlayerManager.getName(receiver) +" (ID:" + id +").");

        }

        [Command("agiveweapon")]
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
            API.sendChatMessageToPlayer(player, "You have given Player ID: " + id + " a weapon.");
        }

        [Command("sethealth")]
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
            API.sendChatMessageToPlayer(player, "You have set Player ID: " + id + "'s health to " + health + ".");
        }

        [Command("setarmour")]
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
            API.sendChatMessageToPlayer(player, "You have set Player ID: " + id + "'s armour to " + armour + ".");
        }

        [Command("spec")]
        public static void spec_cmd(Client player, string id)
        {
            Client target = PlayerManager.parseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.admin_level == 0)
                return;

            if (target == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            account.is_spectating = true;
            API.shared.setPlayerToSpectatePlayer(player, target);
            API.shared.sendChatMessageToPlayer(player, "You are now spectating " + PlayerManager.getName(target) + " (ID:" + id + "). Use /specoff to stop spectating this player.");

        }

        [Command("specoff")]
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

        [Command("slap")]
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
