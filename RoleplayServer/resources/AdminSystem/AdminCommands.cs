using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.core;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.player_manager;
using RoleplayServer.resources.vehicle_manager;
using RoleplayServer.resources.weapon_manager;

namespace RoleplayServer.resources.AdminSystem
{
    public class AdminCommands : Script 
    {

        public AdminCommands()
        {
            DebugManager.DebugMessage("[AdminSys] Initalizing Admin System...");


            DebugManager.DebugMessage("[AdminSys] Admin System initalized.");
        }

        [Command("gotopos")]
        public void gotopos_cmd(Client player, double x, double y, double z)
        {
            var pos = new Vector3(x, y, z);
            API.setEntityPosition(player, pos);
            API.sendChatMessageToPlayer(player, "Teleported");
        }

        [Command("goto")]
        public static void goto_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
                return;

            if (receiver == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            var playerPos = API.shared.getEntityPosition(receiver);
            API.shared.setEntityPosition(player, new Vector3(playerPos.X, playerPos.Y + 1, playerPos.Z));
            API.shared.sendChatMessageToPlayer(player, "You have teleported to " + PlayerManager.GetName(receiver) +" (ID:" + id +").");

        }

        [Command("agiveweapon")]
        public void agiveweapon_cmd(Client player, string id, WeaponHash weaponHash)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 3)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            WeaponManager.CreateWeapon(player, weaponHash, WeaponTint.Normal, false, true);
            API.sendChatMessageToPlayer(player, "You have given Player ID: " + id + " a " + weaponHash);
        }

        [Command("sethealth")]
        public void sethealth_cmd(Client player, string id, int health)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
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
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
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
            var target = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
                return;

            if (target == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            account.IsSpectating = true;
            API.shared.setPlayerToSpectatePlayer(player, target);
            API.shared.sendChatMessageToPlayer(player, "You are now spectating " + PlayerManager.GetName(target) + " (ID:" + id + "). Use /specoff to stop spectating this player.");

        }

        [Command("specoff")]
        public void specoff_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
                return;

            if (account.IsSpectating == false)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You are not specing anyone.");
                return;
            }
            account.IsSpectating = false;
            API.unspectatePlayer(player);
            API.sendChatMessageToPlayer(player, "You are no longer spectating anyone.");
        }

        [Command("slap")]
        public void slap_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            var playerPos = API.getEntityPosition(receiver);
            API.setEntityPosition(receiver, new Vector3(playerPos.X, playerPos.Y, playerPos.Z + 5));
            API.sendChatMessageToPlayer(receiver, "You have been slapped by an admin");
        }

        [Command("setmymoney")]
        public void setmymoney_cmd(Client player, int money)
        {
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");

            if (account.AdminLevel == 0)
                return;

            InventoryManager.SetInventoryAmmount(character, typeof(Money), money);
            API.sendChatMessageToPlayer(player, $"You have sucessfully changed your money to ${money}.");
        }

        [Command("showplayercars")]
        public void showplayercars_cmd(Client player, string id)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel == 0)
                return;

            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = API.getEntityData(player.handle, "Character");
            API.sendChatMessageToPlayer(player, "----------------------------------------------");
            API.sendChatMessageToPlayer(player, $"Vehicles Owned By {character.CharacterName}");
            foreach (var carid in character.OwnedVehicles)
            {
                var car = VehicleManager.Vehicles.SingleOrDefault(x => x.Id == carid);
                if (car == null)
                {
                    API.sendChatMessageToPlayer(player, $"(UNKNOWN VEHICLE) | ID ~r~{carid}~w~.");
                    continue;
                }
                API.sendChatMessageToPlayer(player, $"({API.getVehicleDisplayName(car.VehModel)}) | NetHandle ~r~{car.NetHandle.Value}~w~ | ID ~r~{car.Id}~w~.");
            }
            API.sendChatMessageToPlayer(player, "----------------------------------------------");
        }

        [Command("getplayercar")]
        public void getplayercar_cmd(Client player, string id, int nethandle)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel == 0)
                return;

            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = API.getEntityData(player.handle, "Character");
            var car = VehicleManager.Vehicles.Find(x => x.NetHandle.Value == nethandle && x.OwnerId == character.Id);
            API.setEntityPosition(car.NetHandle, player.position);
            API.sendChatMessageToPlayer(player, "Sucessfully teleported the car to you.");
        }

        //TODO: REMOVE THIS: 
        [Command("makemeadmin")]
        public void makemeadmin_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            account.AdminLevel = 7;
            API.sendChatMessageToPlayer(player, "You are now a king.");
        }
    }
}
