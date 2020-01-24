using System;
using System.Threading.Tasks;
using System.Timers;


using GTANetworkAPI;


using mtgvrp.core;
using mtgvrp.core.Help;
using Color = mtgvrp.core.Color;

namespace mtgvrp.player_manager.player_interaction
{
    class PlayerInteraction : Script
    {
        public PlayerInteraction()
        {
        }

        [RemoteEvent("cancel_following")]
        public void CancelFollowing(Client player, params object[] arguments)
        {
            Character character = player.GetCharacter();

            if (character == null)
                return;

            if (character.FollowingPlayer != Character.None && character.IsBeingDragged == false)
            {

                character.FollowingTimer.Stop();
                character.FollowingPlayer = Character.None;
                NAPI.Chat.SendChatMessageToPlayer(player, "You have stopped following your target.");
            }
        }

        [RemoteEvent("player_interaction_menu")]
        public void PlayerInteractionMenu(Client player, params object[] arguments)
        {
            var option = Convert.ToString(arguments[0]);
            var interactHandle = (Entity)arguments[1];

            Client interactClient = NAPI.Player.GetPlayerFromHandle(interactHandle);
            Character interactCharacter = interactClient.GetCharacter();

            Character character = player.GetCharacter();

            switch (option)
            {
                case "follow":
                    {
                        character.FollowingPlayer = interactCharacter;
                        character.FollowingTimer = new Timer() { Interval = 1000 };
                        character.FollowingTimer.Elapsed += delegate { FollowPlayer(character, false); };
                        character.FollowingTimer.Start();
                        break;
                    }
                case "view_description":
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, "Not yet implemented. =(");
                        break;
                    }
                case "cuff":
                    {
                        if (interactCharacter.IsCuffed == false)
                        {

                            var isStunned = API.FetchNativeFromPlayer<bool>(interactClient,
                                Hash.IS_PED_BEING_STUNNED, interactHandle, 0);

                            if (interactCharacter.AreHandsUp == false && isStunned == false)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Color.White,
                                    "You cannot cuff a player unless their hands are up or they are stunned.");
                                return;
                            }

                            if (player.Position.DistanceTo(interactCharacter.Client.Position) > 3)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Color.White,
                                    "You are too far away to handcuff that player.");
                                return;
                            }

                            API.GivePlayerWeapon(player, WeaponHash.Unarmed, 1);
                            API.SendNativeToAllPlayers(Hash.SET_ENABLE_HANDCUFFS, interactHandle, true);
                            interactCharacter.IsCuffed = true;
                            NAPI.Player.FreezePlayer(interactCharacter.Client, true);
                            API.PlayPlayerAnimation(interactCharacter.Client, (int)(1 << 0 | 1 << 4 | 1 << 5),
                                "mp_arresting", "idle");

                            ChatManager.RoleplayMessage(player,
                                "places handcuffs onto " + interactCharacter.rp_name(), ChatManager.RoleplayMe);

                        }
                        else
                        {
                            if (player.Position.DistanceTo(interactCharacter.Client.Position) > 3)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Color.White,
                                    "You are too far away to unhandcuff that player.");
                                return;
                            }

                            NAPI.Player.FreezePlayer(interactCharacter.Client, false);
                            API.SendNativeToAllPlayers(Hash.SET_ENABLE_HANDCUFFS, interactHandle, false);
                            interactCharacter.IsCuffed = false;
                            API.StopPlayerAnimation(interactCharacter.Client);

                            ChatManager.RoleplayMessage(player,
                                "removes the handcuffs from " + interactCharacter.rp_name(), ChatManager.RoleplayMe);
                        }

                        break;
                    }
                case "drag":
                    {
                        if (player.Position.DistanceTo(interactCharacter.Client.Position) > 3)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Color.White,
                                "You are too far away from that player.");
                            return;
                        }

                        if (interactCharacter.FollowingPlayer == Character.None)
                        {
                            interactCharacter.FollowingPlayer = character;
                            interactCharacter.IsBeingDragged = true;
                            interactCharacter.FollowingTimer = new Timer() { Interval = 1000 };
                            interactCharacter.FollowingTimer.Elapsed += delegate { FollowPlayer(interactCharacter, true); };
                            interactCharacter.FollowingTimer.Start();

                            ChatManager.RoleplayMessage(player, "grabs " + interactCharacter.rp_name() + " and begins to drag them.", ChatManager.RoleplayMe);
                        }
                        else if (interactCharacter.FollowingPlayer == character)
                        {
                            interactCharacter.FollowingTimer.Stop();
                            interactCharacter.IsBeingDragged = false;
                            interactCharacter.FollowingPlayer = Character.None;
                            ChatManager.RoleplayMessage(player, "lets go of " + interactCharacter.rp_name(),
                                ChatManager.RoleplayMe);
                            NAPI.Chat.SendChatMessageToPlayer(player, "You have stopped dragging your target.");
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Color.White,
                                "That player is already being dragged by someone else.");
                        }
                        break;
                    }
            }
        }

        [Command("detain", GreedyArg = true), Help(HelpManager.CommandGroups.Vehicles, "Detain someone into your vehicle. (Must be inside the vehicle)", "The player id", "Seat number you'd like to detain to")]
        public void DetainPlayer(Client player, string id, string seatNumber)
        {

            var receiver = PlayerManager.ParseClient(id);
            Character character = receiver.GetCharacter();
    
            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (character.IsCuffed == false && character.IsTied == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Players must be tied/cuffed before you can detain them.");
                return;
            }

            if (int.Parse(seatNumber) > 2)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Seat number ranges from 0-2 (0 is the passenger seat).");
                return;
            }
            if (NAPI.Player.IsPlayerInAnyVehicle(player) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must be in a vehicle.");
                return;
            }

            if (NAPI.Entity.GetEntityPosition(player).DistanceToSquared(NAPI.Entity.GetEntityPosition(receiver)) > 10f)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "~r~You're too far away!");
                return;
            }

            NAPI.Player.SetPlayerIntoVehicle(receiver, NAPI.Player.GetPlayerVehicle(player), int.Parse(seatNumber));
            NAPI.Chat.SendChatMessageToPlayer(player, "~g~You have detained " + receiver.Name + " into a vehicle.");
            NAPI.Chat.SendChatMessageToPlayer(receiver, "~g~You were detained by " + player.Name + " into a vehicle.");


        }

        [Command("eject", GreedyArg = true), Help(HelpManager.CommandGroups.Vehicles, "Eject someone from your vehicle.", "The player you'd like to kick out")]
        public void ejectPlayer(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (NAPI.Player.IsPlayerInAnyVehicle(player) == false || NAPI.Player.GetPlayerVehicleSeat(player) != -1)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must be in the front seat of a vehicle to eject another player.");
                return;
            }

            if (NAPI.Player.IsPlayerInAnyVehicle(receiver) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Players must be in a vehicle to be ejected from a vehicle.");
                return;
            }

            if (NAPI.Player.GetPlayerVehicle(player) != NAPI.Player.GetPlayerVehicle(receiver))
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must be in the same vehicle as another player to eject them.");
                return;
            }

            //API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));
            Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player)); // CONV NOTE: delay fixme
            NAPI.Chat.SendChatMessageToPlayer(player, "You have ejected ~b~" + receiver.GetCharacter().rp_name() + "~w~ from your vehicle.");
            NAPI.Chat.SendChatMessageToPlayer(receiver, "~b~" + player.GetCharacter().rp_name() + "~w~ has ejected you from their vehicle.");
        }
 
        public void FollowPlayer(Character c, bool isDrag)
        {
            API.SendNativeToAllPlayers(Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY, c.Client,
                                    c.FollowingPlayer.Client, -1.0, 0.0, 0.0, 1, 1050, 2, true);
            if (isDrag == false)
            {
                NAPI.ClientEvent.TriggerClientEvent(c.Client, "player_interact_subtitle",
                    "You are following " + c.FollowingPlayer.rp_name() + ". Press SPACE to stop following.");
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEvent(c.Client, "player_interact_subtitle",
                    "You are being dragged by " + c.FollowingPlayer.rp_name() + ".");
            }
        }
    }
}
