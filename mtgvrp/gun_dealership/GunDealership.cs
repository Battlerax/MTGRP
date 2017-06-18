using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.weapon_manager;

public class GunDealership : Script
{

    public GunDealership()
    {


        API.onResourceStart += OnResourceStart;
        API.onClientEventTrigger += OnClientEvent;

    }


    public ColShape Infoammunation;


    public void OnResourceStart()
    {
        API.createMarker(1, new Vector3(842.6454, -1033.193, 27.19486), new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 255, 255, 255, 255);
        Infoammunation = API.createCylinderColShape(new Vector3(842.6454, -1033.193, 27.19486), 2f, 5f);

        Infoammunation.onEntityEnterColShape += (shape, entity) =>
        {
            Client player;
            if ((player = API.getPlayerFromHandle(entity)) != null)
            {
                API.triggerClientEvent(player, "openmenu");
            }
        };

        Infoammunation.onEntityExitColShape += (shape, entity) =>
        {
            Client player;
            if ((player = API.getPlayerFromHandle(entity)) != null)
            {
                API.triggerClientEvent(player, "closemenu");

            }
        };
    }

    public bool moneyCheck(Client sender, int weaponCost)
    {
        Character character = API.getEntityData(sender.handle, "Character");

        if (Money.GetCharacterMoney(character) - weaponCost < 0)
        {
            API.sendChatMessageToPlayer(sender, "~r~You cannot afford this weapon.");
            return false;
        }

        InventoryManager.DeleteInventoryItem(character, typeof(Money), weaponCost);
        return true;
    }

    public void OnClientEvent(Client player, string eventName, params object[] args)
    {
        Character character = API.getEntityData(player.handle, "Character");

        if (eventName == "clickeditem")
        {
            if ((string)args[0] == "Bat")
            {
                if (moneyCheck(player, 40) == true)
                {
                    buyWeapon(player, (string) args[0]);
                }
            }

            if ((string)args[0] == "Pistol")
            {
                if (moneyCheck(player, 4000) == true)
                {
                    buyWeapon(player, (string)args[0]);
                }
            }
            else if ((string)args[0] == "CombatPistol")
            {
                if (moneyCheck(player, 4700) == true)
                {
                    buyWeapon(player, (string)args[0]);
                }
            }
            else if ((string)args[0] == "HeavyPistol")
            {
                if (moneyCheck(player, 6500) == true)
                {
                    buyWeapon(player, (string)args[0]);
                }
            }
            else if ((string)args[0] == "Revolver")
            {
                if (moneyCheck(player, 8000) == true)
                {
                    buyWeapon(player, (string)args[0]);
                }
            }
            API.triggerClientEvent(player, "closemenu");
        }

    }

    public static void buyWeapon(Client player, string weaponName)
    {
        Character character = API.shared.getEntityData(player.handle, "Character");

        WeaponHash weapon = API.shared.weaponNameToModel(weaponName);
        WeaponManager.CreateWeapon(player, weapon, WeaponTint.Normal, true);
        
        player.sendChatMessage("You have bought a ~g~" + weaponName);
    }

}