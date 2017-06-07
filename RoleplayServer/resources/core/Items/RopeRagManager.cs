using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.core.Items
{
    class RopeRagManager : Script
    {
        [Command("tie")]
        public void tie_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.sendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                var tie = InventoryManager.DoesInventoryHaveItem<RopeItem>(player.GetCharacter());
                if (tie.Length == 0)
                {
                    API.sendChatMessageToPlayer(player, "You don't have a rope.");
                    return;
                }

                API.setEntityData(target, "IS_TIED", true);
                API.freezePlayer(target, true);
                ChatManager.RoleplayMessage(player, $"ties {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);

                InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(RopeItem), 1);
            }
            else
            {
                API.sendNotificationToPlayer(player, "You aren't near that player.");
            }
        }

        [Command("untie")]
        public void untie_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.sendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                if (!API.hasEntityData(target, "IS_TIED"))
                {
                    API.sendChatMessageToPlayer(player, "That player isn't tied.");
                    return;
                }
                API.freezePlayer(target, false);
                API.resetEntityData(target, "IS_TIED");
                ChatManager.RoleplayMessage(player, $"unties {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);
            }
            else
            {
                API.sendNotificationToPlayer(player, "You aren't near that player.");
            }
        }

        [Command("blindfold")]
        public void blindfold_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.sendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                var rag = InventoryManager.DoesInventoryHaveItem<RagsItem>(player.GetCharacter());
                if (rag.Length == 0)
                {
                    API.sendChatMessageToPlayer(player, "You don't have a rag.");
                    return;
                }

                API.setEntityData(target, "IS_BLINDFOLDED", true);
                API.triggerClientEvent(target, "blindfold_intiate");
                ChatManager.RoleplayMessage(player, $"blindfolds {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);

                InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(RagsItem), 1);
            }
            else
            {
                API.sendNotificationToPlayer(player, "You aren't near that player.");
            }
        }

        [Command("unblindfold")]
        public void unblindfold_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.sendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                if (!API.hasEntityData(target, "IS_BLINDFOLDED"))
                {
                    API.sendChatMessageToPlayer(player, "That player isn't blindfolded.");
                    return;
                }
                API.resetEntityData(target, "IS_BLINDFOLDED");
                API.triggerClientEvent(target, "blindfold_cancel");
                ChatManager.RoleplayMessage(player, $"unblindfolds {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);
            }
            else
            {
                API.sendNotificationToPlayer(player, "You aren't near that player.");
            }
        }

        [Command("rag")]
        public void rag_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.sendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                var rag = InventoryManager.DoesInventoryHaveItem<RagsItem>(player.GetCharacter());
                if (rag.Length == 0)
                {
                    API.sendChatMessageToPlayer(player, "You don't have a rag.");
                    return;
                }

                API.setEntityData(target, "IS_MOUTH_RAGGED", true);
                ChatManager.RoleplayMessage(player, $"rags {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);

                InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(RagsItem), 1);
            }
            else
            {
                API.sendNotificationToPlayer(player, "You aren't near that player.");
            }
        }

        [Command("unrag")]
        public void unrag_cmd(Client player, string id)
        {
            Client target = PlayerManager.ParseClient(id);
            if (target == null)
            {
                API.sendChatMessageToPlayer(player, "That target doesn't exist.");
                return;
            }
            if (player.position.DistanceTo(target.position) <= 5.0)
            {
                if (!API.hasEntityData(target, "IS_MOUTH_RAGGED"))
                {
                    API.sendChatMessageToPlayer(player, "That player isn't ragged.");
                    return;
                }
                API.resetEntityData(target, "IS_MOUTH_RAGGED");
                ChatManager.RoleplayMessage(player, $"unrags {target.GetCharacter().rp_name()}", ChatManager.RoleplayMe);
            }
            else
            {
                API.sendNotificationToPlayer(player, "You aren't near that player.");
            }
        }
    }
}
