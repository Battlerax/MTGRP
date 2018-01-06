

using GTANetworkAPI;

using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.property_system.businesses;

namespace mtgvrp.core.Items
{
    class RopeRagManager : Script
    {
        [Command("tie"), Help.Help(HelpManager.CommandGroups.General, "Tie someone so he can't move.", "The plauyerid")]
        public void tie_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.SendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (target == player)
            {
                API.SendChatMessageToPlayer(player, "You can't do that on yourself");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                var tie = InventoryManager.DoesInventoryHaveItem<RopeItem>(player.GetCharacter());
                if (tie.Length == 0)
                {
                    API.SendChatMessageToPlayer(player, "You don't have a rope.");
                    return;
                }

                target.GetCharacter().IsTied = true;
                API.FreezePlayer(target, true);
                ChatManager.RoleplayMessage(player, $"ties {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);

                InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(RopeItem), 1);
            }
            else
            {
                API.SendNotificationToPlayer(player, "You aren't near that player.");
            }
        }

        [Command("untie"), Help.Help(HelpManager.CommandGroups.General, "Untie someone so can move again.", "The playerid")]
        public void untie_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.SendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (target == player)
            {
                API.SendChatMessageToPlayer(player, "You can't do that on yourself");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                if (!target.GetCharacter().IsTied)
                {
                    API.SendChatMessageToPlayer(player, "That player isn't tied.");
                    return;
                }
                API.FreezePlayer(target, false);
                target.GetCharacter().IsTied = false;
                ChatManager.RoleplayMessage(player, $"unties {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);
            }
            else
            {
                API.SendNotificationToPlayer(player, "You aren't near that player.");
            }
        }

        [Command("blindfold"), Help.Help(HelpManager.CommandGroups.General, "Blindfold someone so he couldn't see.", "The player id")]
        public void blindfold_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.SendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (target == player)
            {
                API.SendChatMessageToPlayer(player, "You can't do that on yourself");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                var rag = InventoryManager.DoesInventoryHaveItem<RagsItem>(player.GetCharacter());
                if (rag.Length == 0)
                {
                    API.SendChatMessageToPlayer(player, "You don't have a rag.");
                    return;
                }

                target.GetCharacter().IsBlindfolded = true;
                API.TriggerClientEvent(target, "blindfold_intiate");
                ChatManager.RoleplayMessage(player, $"blindfolds {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);

                InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(RagsItem), 1);
            }
            else
            {
                API.SendNotificationToPlayer(player, "You aren't near that player.");
            }
        }

        [Command("unblindfold"), Help.Help(HelpManager.CommandGroups.General, "Unlindfold someone so he could see again.", "The player id")]
        public void unblindfold_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.SendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (target == player)
            {
                API.SendChatMessageToPlayer(player, "You can't do that on yourself");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                if (!target.GetCharacter().IsBlindfolded)
                {
                    API.SendChatMessageToPlayer(player, "That player isn't blindfolded.");
                    return;
                }
                target.GetCharacter().IsBlindfolded = false;
                API.TriggerClientEvent(target, "blindfold_cancel");
                ChatManager.RoleplayMessage(player, $"unblindfolds {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);
            }
            else
            {
                API.SendNotificationToPlayer(player, "You aren't near that player.");
            }
        }

        [Command("rag"), Help.Help(HelpManager.CommandGroups.General, "Rag someone so he couldn't talk.", "The player id")]
        public void rag_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.SendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (target == player)
            {
                API.SendChatMessageToPlayer(player, "You can't do that on yourself");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                var rag = InventoryManager.DoesInventoryHaveItem<RagsItem>(player.GetCharacter());
                if (rag.Length == 0)
                {
                    API.SendChatMessageToPlayer(player, "You don't have a rag.");
                    return;
                }

                target.GetCharacter().IsRagged = true;
                ChatManager.RoleplayMessage(player, $"rags {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);

                InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(RagsItem), 1);
            }
            else
            {
                API.SendNotificationToPlayer(player, "You aren't near that player.");
            }
        }

        [Command("unrag"), Help.Help(HelpManager.CommandGroups.General, "Unrag someone so he couldn talk again.", "The player id")]
        public void unrag_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.SendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (target == player)
            {
                API.SendChatMessageToPlayer(player, "You can't do that on yourself");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                if (!target.GetCharacter().IsRagged)
                {
                    API.SendChatMessageToPlayer(player, "That player isn't ragged.");
                    return;
                }
                target.GetCharacter().IsRagged = false;
                ChatManager.RoleplayMessage(player, $"unrags {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);
            }
            else
            {
                API.SendNotificationToPlayer(player, "You aren't near that player.");
            }
        }

        [Command("usesprunk"), Help.Help(HelpManager.CommandGroups.General, "Use a sprunk item. Gets you +5 health.")]
        public void usesprunk_cmd(Client player)
        {
            var rag = InventoryManager.DoesInventoryHaveItem<SprunkItem>(player.GetCharacter());
            if (rag.Length == 0)
            {
                API.SendChatMessageToPlayer(player, "You don't have a sprunk.");
                return;
            }

            player.health += 5;
            if (player.health > 100) player.health = 100;
            ChatManager.RoleplayMessage(player, "drinks a sprunk.", ChatManager.RoleplayMe);
            InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(SprunkItem), 1);
        }
    }
}
