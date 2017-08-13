using System;
using System.Collections.Generic;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using mtgvrp.core.Help;

namespace mtgvrp.property_system.businesses
{
    class Bank : Script
    {
        [Command("deposit"), Help(HelpManager.CommandGroups.General, "Command to use when putting money into your bank account.", new[] { "Amount" })]
        public void deposit_cmd(Client player, int amount)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null || prop?.Type != PropertyManager.PropertyTypes.Bank)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a bank interaction.");
                return;
            }

            var character = player.GetCharacter();
            if (Money.GetCharacterMoney(character) >= amount && amount > 0)
            {
                character.BankBalance += amount;
                InventoryManager.DeleteInventoryItem(character, typeof(Money), amount);
                ChatManager.RoleplayMessage(player, "deposits some money into their bank account.",
                    ChatManager.RoleplayMe);
                LogManager.Log(LogManager.LogTypes.Stats, $"[Bank] {player.GetCharacter().CharacterName}[{player.GetAccount().AccountName}] has deposited ${amount} into their bank account. CharacterMoney: ${Money.GetCharacterMoney(character)} | BankMoney: ${character.BankBalance}");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount.");
            }
        }

        [Command("withdraw"), Help(HelpManager.CommandGroups.General, "Command to use when taking money from your bank account.", new[] { "Amount" })]
        public void withdraw_cmd(Client player, int amount)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null || prop?.Type != PropertyManager.PropertyTypes.Bank)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a bank interaction.");
                return;
            }

            var character = player.GetCharacter();
            if (character.BankBalance >= amount && amount > 0)
            {
                character.BankBalance -= amount;
                InventoryManager.GiveInventoryItem(character, new Money(), amount, true);
                ChatManager.RoleplayMessage(player, "withdraws some money from their bank account.",
                    ChatManager.RoleplayMe);
                LogManager.Log(LogManager.LogTypes.Stats, $"[Bank] {player.GetCharacter().CharacterName}[{player.GetAccount().AccountName}] has withdrawn ${amount} from their bank account. CharacterMoney: ${Money.GetCharacterMoney(character)} | BankMoney: ${character.BankBalance}");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount in your bank.");
            }
        }

        [Command("wiretransfer"), Help(HelpManager.CommandGroups.General, "Command to transfer money from one account to another online players account.", new[] { "ID of target player.", "Amount." })]
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

            if (target == player)
            {
                API.sendChatMessageToPlayer(player, "You cannot wire transfer to yourself.");
                return;
            }

            var character = player.GetCharacter();
            if (character.BankBalance >= amount && amount > 0)
            {
                target.GetCharacter().BankBalance += amount;
                character.BankBalance -= amount;
                ChatManager.RoleplayMessage(player, "wire transfers some money from their bank account.",
                    ChatManager.RoleplayMe);
                API.sendChatMessageToPlayer(target,
                    $"~r~* {character.rp_name()}~w~ has sent you a wire transfer of ~g~${amount}");
                LogManager.Log(LogManager.LogTypes.Stats, $"[Bank] {player.GetCharacter().CharacterName}[{player.GetAccount().AccountName}] has wire transferred ${amount} to {target.GetCharacter().CharacterName}[{target.GetAccount().AccountName}] into their bank account.");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount in your bank.");
            }
        }

        [Command("balance"), Help(HelpManager.CommandGroups.General, "Used to see your current bank balance.", null)]
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

        [Command("givecheck"), Help(HelpManager.CommandGroups.General, "Hand another player a check, taking money from your bank.", new[] { "ID of target player.", "Amount" })]
        public void GiveCheck_cmd(Client player, string id, int amount)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.sendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }

            if (target == player)
            {
                API.sendChatMessageToPlayer(player, "You can't give a check to yourself.");
            }

            if (player.position.DistanceTo(target.position) > 5.0)
            {
                API.sendChatMessageToPlayer(player, "Must be near the target.");
                return;
            }
            Character c = player.GetCharacter();

            if (c.BankBalance < amount || amount <= 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount in your bank balance.");
                return;
            }

            //c.BankBalance -= amount;
            var item = InventoryManager.DoesInventoryHaveItem<CheckItem>(target.GetCharacter());
            if (item.Length == 0)
            {
                if(InventoryManager.GiveInventoryItem(target.GetCharacter(), new CheckItem() {CheckAmount = amount}) == InventoryManager.GiveItemErrors.Success)
                {
                    c.BankBalance -= amount;
                }
            }
            else
            {
                item[0].CheckAmount += amount;
            }

            ChatManager.RoleplayMessage(player, $"hands a check to {target.GetCharacter().rp_name()}.",
                ChatManager.RoleplayMe);

            LogManager.Log(LogManager.LogTypes.Stats, $"[Bank] {player.GetCharacter().CharacterName}[{player.GetAccount().AccountName}] has given a check worth ${amount} to {target.GetCharacter().CharacterName}[{target.GetAccount().AccountName}].");
        }

        [Command("redeemcheck"), Help(HelpManager.CommandGroups.General, "To cash in a check when at a bank.", null)]
        public void Redeemcheck_cmd(Client player)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null || prop?.Type != PropertyManager.PropertyTypes.Bank)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a bank interaction.");
                return;
            }

            Character c = player.GetCharacter();
            var item = InventoryManager.DoesInventoryHaveItem<CheckItem>(c);
            if (item.Length > 0)
            {
                c.BankBalance += item[0].CheckAmount;
                API.sendChatMessageToPlayer(player, $"You have redemeed ~g~${item[0].CheckAmount}~w~. Balance now is: ~g~${c.BankBalance}");
                InventoryManager.DeleteInventoryItem(c, typeof(CheckItem));
                LogManager.Log(LogManager.LogTypes.Stats, $"[Bank] {player.GetCharacter().CharacterName}[{player.GetAccount().AccountName}] has redeemed a check worth ${item[0].CheckAmount}.");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't have a check to redeem.");
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
        public bool CanBeStored => false;
        public bool IsBlocking => false;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>();

        public int AmountOfSlots => 0;

        public string CommandFriendlyName => "check";

        public string LongName => $"Check (${CheckAmount})";

        public int Object => 0;

        public int Amount { get; set; }

        public int CheckAmount { get; set; }
    }
}