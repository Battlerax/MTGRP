using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.inventory;

namespace mtgvrp.job_manager.scuba
{
    class ScubaManager : Script
    {
        public ScubaManager()
        {
            API.onPlayerDisconnected += API_onPlayerDisconnected;
        }

        private void API_onPlayerDisconnected(Client player, string reason)
        {
            if (player.GetCharacter().IsScubaDiving)
            {
                CancelScuba(player);
            }
        }

        [Command("equipscuba")]
        public void EquipScuba(Client player)
        {
            var character = player.GetCharacter();
            var item = InventoryManager.DoesInventoryHaveItem<ScubaItem>(character);
            if (item.Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have a scuba kit.");
                return;
            }

            if (character.IsScubaDiving)
            {
                API.sendChatMessageToPlayer(player, "You already have the kit on.");
                return;
            }

            //Create the objects for the player.
            var head = API.createObject(239157435, player.position, new Vector3());
            API.attachEntityToEntity(head, player, "SKEL_Head", new Vector3(0, 0, 0), new Vector3(180, 90, 0));
            var tank = API.createObject(1593773001, player.position, new Vector3());
            API.attachEntityToEntity(tank, player, "SKEL_Spine3", new Vector3(-0.3, -0.23, 0), new Vector3(180, 90, 0));
            API.setEntityData(player, "SCUBA_TANK", tank);
            API.setEntityData(player, "SCUBA_HEAD", head);

            //Set the variable.
            character.IsScubaDiving = true;

            //Create the timer.
            API.setEntityData(player, "SCUBA_TIMER",
                new Timer(delegate { RefreshScuba(player); }, null, 1000, 1000));

            //Set the scuba state as true.
            API.sendNativeToPlayer(player, Hash.SET_ENABLE_SCUBA, player.handle, true);

            //Show remaining oxygen.
            API.triggerClientEvent(player, "UPDATE_SCUBA_PERCENTAGE",
                "Oxygen Remaining: " + Math.Round((item[0].OxygenRemaining / ScubaItem.MaxOxygen) * 100f) + "%");
        }

        [Command("dequipscuba")]
        public void DequipScuba(Client player)
        {
            var character = player.GetCharacter();

            if (!character.IsScubaDiving)
            {
                API.sendChatMessageToPlayer(player, "You aren't scubadiving.");
                return;
            }

            CancelScuba(player);
            API.sendChatMessageToPlayer(player, "You have dequiped the scuba set.");
        }

        public void RefreshScuba(Client player)
        {
            var character = player.GetCharacter();
            var scubaitem = InventoryManager.DoesInventoryHaveItem<ScubaItem>(character);

            if (scubaitem.Length == 0)
            {
                CancelScuba(player);
                API.sendChatMessageToPlayer(player, "The scuba set has been removed.");
                return;
            }

            if (character.IsScubaDiving != true)
            {
                CancelScuba(player);
                API.sendChatMessageToPlayer(player, "The scuba set has been removed.");
                return;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (API.fetchNativeFromPlayer<float>(player, Hash.GET_ENTITY_SUBMERGED_LEVEL, player.handle) == 1f)
            {
                scubaitem[0].OxygenRemaining--;
                if (scubaitem[0].OxygenRemaining <= 0)
                {
                    CancelScuba(player);
                    InventoryManager.DeleteInventoryItem<ScubaItem>(character);
                    API.sendChatMessageToPlayer(player, "Your oxygen have run out.");
                    return;
                }
                
                API.sendNativeToPlayer(player, Hash.SET_PED_MAX_TIME_UNDERWATER, player.handle, 3600.0f);
                API.triggerClientEvent(player, "UPDATE_SCUBA_PERCENTAGE",
                    "Oxygen Remaining: " + Math.Round((scubaitem[0].OxygenRemaining / ScubaItem.MaxOxygen) * 100f) + "%");
                return;
            }
        }

        void CancelScuba(Client player)
        {
            var character = player.GetCharacter();

            //Cancel timer.
            Timer timer = API.getEntityData(player, "SCUBA_TIMER");
            if (timer != null)
            {
                timer.Dispose();
                API.resetEntityData(player, "SCUBA_TIMER");
            }

            //Remove clothes
            GTANetworkServer.Object head = API.getEntityData(player, "SCUBA_HEAD");
            GTANetworkServer.Object tank = API.getEntityData(player, "SCUBA_TANK");
            if (head != null && API.doesEntityExist(head))
            {
                head.detach();
                head.delete();
                API.resetEntityData(player, "SCUBA_HEAD");
            }
            if (tank != null && API.doesEntityExist(tank))
            {
                tank.detach();
                tank.delete();
                API.resetEntityData(player, "SCUBA_TANK");
            }

            //Set scuba state
            API.sendNativeToPlayer(player, Hash.SET_ENABLE_SCUBA, player.handle, false);

            //Remove exygen
            API.triggerClientEvent(player, "UPDATE_SCUBA_PERCENTAGE", "none");

            //Set the variable.
            character.IsScubaDiving = false;

            //Set normal underwater time.
            API.sendNativeToPlayer(player, Hash.SET_PED_MAX_TIME_UNDERWATER, player.handle, 60);
        }
    }
}