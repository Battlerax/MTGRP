using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using MongoDB.Bson;
using RoleplayServer.resources.core;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.property_system.businesses
{
    class Bank : Script
    {
        [Command("deposit")]
        public void deposit_cmd(Client player, int amount)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null || prop?.Type != PropertyManager.PropertyTypes.Bank)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a bank interaction.");
                return;
            }

            var character = player.GetCharacter();
            if (Money.GetCharacterMoney(character) >= amount)
            {
                character.BankBalance += amount;
                InventoryManager.DeleteInventoryItem(character, typeof(Money), amount);
                ChatManager.RoleplayMessage(player, "deposits some money into their bank account.",
                    ChatManager.RoleplayMe);
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount.");
            }
        }

        [Command("withdraw")]
        public void withdraw_cmd(Client player, int amount)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null || prop?.Type != PropertyManager.PropertyTypes.Bank)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a bank interaction.");
                return;
            }

            var character = player.GetCharacter();
            if (character.BankBalance >= amount)
            {
                character.BankBalance -= amount;
                InventoryManager.GiveInventoryItem(character, new Money(), amount);
                ChatManager.RoleplayMessage(player, "withdraws some money from their bank account.",
                    ChatManager.RoleplayMe);
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount in your bank.");
            }
        }

        [Command("wiretransfer")]
        public void wiretransfer_cmd(Client player, string id, int amount)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null || prop?.Type != PropertyManager.PropertyTypes.Bank)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a bank interaction.");
                return;
            }

            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.sendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }

            var character = player.GetCharacter();
            if (character.BankBalance >= amount)
            {
                target.GetCharacter().BankBalance += amount;
                character.BankBalance -= amount;
                ChatManager.RoleplayMessage(player, "wire transfers some money from their bank account.",
                    ChatManager.RoleplayMe);
                API.sendChatMessageToPlayer(target,
                    $"~r~* {character.rp_name()}~w~ has sent you a wire transfer of ~g~${amount}");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount in your bank.");
            }
        }

        [Command("balance")]
        public void balance_cmd(Client player)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null || prop?.Type != PropertyManager.PropertyTypes.Bank)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a bank interaction.");
                return;
            }

            API.sendChatMessageToPlayer(player,
                $"You have ~g~${player.GetCharacter().BankBalance}~w~ in your account.");
        }

        [Command("givecheck")]
        public void GiveCheck_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.sendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }

            if (player.position.DistanceTo(target.position) > 5.0)
            {
                API.sendChatMessageToPlayer(player, "Must be near the target.");
                return;
            }

            
        }
    }

    public class CheckItem : IInventoryItem
    {
        public ObjectId Id { get; set; }

        public bool CanBeGiven => false;
        public bool CanBeDropped => true;
        public bool CanBeStashed => false;
        public bool CanBeStacked => false;

        public bool IsBlocking => false;

        public int MaxAmount => -1;

        public int AmountOfSlots => 5;

        public string CommandFriendlyName => $"check_{FromName}";

        public string LongName => $"Check (${CheckAmount}) From {FromName}";

        public int Object => 0;

        public int Amount { get; set; }

        public int CheckAmount { get; set; }
        public int FromId { get; set; }
        public string FromName { get; set; }
    }
}