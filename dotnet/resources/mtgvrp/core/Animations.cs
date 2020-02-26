// Animations
// Author: Toro
// Date: 18/04/2017
// TODO: Need to check all animations to see if they work.
// May need to add some other animations that will be needed.
// Will wait till release to make changes that the community suggests

using System;

using GTANetworkAPI;

using mtgvrp.player_manager;

namespace mtgvrp.core
{
    class Animations : Script
    {
        public Animations()
        {
        }

        [Flags]
        public enum AnimationFlags
        {
            Loop = 1 << 0,
            StopOnLastFrame = 1 << 1,
            OnlyAnimateUpperBody = 1 << 4,
            AllowPlayerControl = 1 << 5,
            Cancellable = 1 << 7
        }

        private void playPlayerAnimationEx(Player player, int flag, string animDict, string animName)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "setPlayerIntoAnim");
            NAPI.Player.PlayPlayerAnimation(player, flag, animDict, animName);
        }

        [RemoteEvent("stopPlayerAnims")]
        public void StopPlayerAnims(Player player, params object[] arguments)
        {
            stopanim(player);
        }
        
        [Command("stopanim")]
        public void stopanim(Player player)
        {
            Character character = player.GetCharacter();
            if (character.IsCuffed || character.IsTied) { return; }
            NAPI.Player.StopPlayerAnimation(player);
            //API.StopPedAnimation(player);
            character.AreHandsUp = false;
        }

        [Command("hide", "~y~Syntax /Hide 1 - 13")]
        public void hide(Player player, int number)
        {           
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {               
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@female@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@female@idle_a", "idle_c");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@female@react_cowering", "base_back_left");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@male@base", "base");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@male@idle_a", "idle_b");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@male@idle_b", "idle_d");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@male@react_cowering", "base_back_left");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower_stand@female@base", "base");
                        break;
                    case 9:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@female@base", "base");
                        break;
                    case 10:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower_stand@female@idle_a", "idle_c");
                        break;
                    case 11:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower_stand@male@base", "base");
                        break;
                    case 12:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower_stand@male@idle_a", "idle_b");
                        break;
                    case 13:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cower_stand@male@react_cowering", "base_right");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /Hide 1 - 13");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("lookout", "~y~Syntax /lookout 1 - 4")]
        public void lookout(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cross_road@female@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cross_road@female@idle_a", "idle_c");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cross_road@male@base", "base");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_cross_road@male@idle_a", "idle_e");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /lookout 1 - 4");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        
        [Command("investigate", "~y~Syntax /investigate 1 - 9")]
        public void investigate(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_police_investigate@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_police_investigate@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_police_investigate@idle_b", "idle_f");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@medic@standing@kneel@base", "base");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@medic@standing@kneel@idle_a", "idle_a");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@medic@standing@tendtodead@base", "base");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@medic@standing@tendtodead@idle_a", "idle_a");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@medic@standing@timeofdeath@base", "base");
                        break;
                    case 9:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@medic@standing@timeofdeath@idle_a", "idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /investigate 1 - 9");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("drink", "~y~Syntax /drink 1 - 12")]
        public void drink(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:// can in right hand
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_drinking@beer@female@base", "static");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_drinking@beer@male@base", "static");
                        break;
                    case 3://Coffee in right hand
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_aa_coffee@base", "base");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_aa_coffee@idle_a", "idle_a");
                        break;
                    case 5:// beer in right hand 683570518 
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@beer@female@base", "base");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@beer@female@idle_a", "idle_f");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@beer@male@base", "base");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@beer@male@idle_a", "idle_a");
                        break;
                    case 9://coffee
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@coffee@female@base", "base");
                        break;
                    case 10:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@coffee@female@idle_a", "idle_a");
                        break;
                    case 11:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@coffee@male@base", "base");
                        break;
                    case 12:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@coffee@male@idle_a", "idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /drink 1 - 12");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("crossarms", "~y~Syntax /crossarms 1 - 13")]
        public void crossarms(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {                
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_idles_cop@female@static", "static");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_idles_cop@male@static", "static");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@female@idle_a", "idle_b");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@female@idle_b", "idle_e");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@male@idle_a", "idle_b");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@male@idle_b", "idle_e");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_arms_crossed@base", "base");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_arms_crossed@idle_a", "idle_a");
                        break;
                    case 9:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@male@idle_b", "idle_e");
                        break;
                    case 10:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_c@base", "base");
                        break;
                    case 11:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_c@idle_a", "idle_a");
                        break;
                    case 12:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_c@idle_b", "idle_d");
                        break;
                    case 13:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@heists@heist_corona@single_team", "single_team_loop_boss");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /crossarms 1 - 13");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("idle", "~y~Syntax /idle 1 - 28")]
        public void idle(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_music_listen@female@base", "static");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_music_listen@male@base", "static");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_rain@female@base", "static");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_rain@male_a@base", "static");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_texting@female@base", "static");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_texting@male@base", "static");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_movie_studio_light@base", "base");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_movie_studio_light@idle_a", "idle_b");
                        break;
                    case 9:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_parking_meter@female@base", "base_female");
                        break;
                    case 10:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_parking_meter@female@idle_a", "idle_b_female");
                        break;
                    case 11:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_parking_meter@male@base", "base");
                        break;
                    case 12:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_parking_meter@male@idle_a", "idle_a");
                        break;
                    case 13:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_aa_coffee@base", "base");
                        break;
                    case 14:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_golf_player@male@base", "base");
                        break;
                    case 15:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_golf_player@male@idle_a", "idle_a");
                        break;
                    case 16:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_golf_player@male@idle_b", "idle_d");
                        break;
                    case 17:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_arm_side@base", "base");
                        break;
                    case 18:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_arm_side@idle_a", "idle_a");
                        break;
                    case 19:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_hold_arm@base", "base");
                        break;
                    case 20:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_hold_arm@idle_a", "idle_a");
                        break;
                    case 21:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_a@base", "base");
                        break;
                    case 22:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_a@idle_a", "idle_b");
                        break;
                    case 23:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_a@idle_a", "idle_a");
                        break;
                    case 24:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_b@base", "base");
                        break;
                    case 25:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_b@idle_a", "idle_b");
                        break;
                    case 26:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hiker_standing@female@base", "base");
                        break;
                    case 27:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_prostitute@hooker@base", "base");
                        break;
                    case 28:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_prostitute@hooker@idle_a", "idle_a");
                        break;

                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /idle 1 - 28");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("lean", "~y~Syntax /lean 1 - 38")]
        public void lean(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_bum_bin@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_bum_bin@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_bum_bin@idle_b", "idle_b");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_bum_shopping_cart@male@base", "base");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_bum_shopping_cart@male@idle_a", "idle_a");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@coffee@base", "base");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@coffee@idle_a", "idle_a");
                        break;
                    case 8://Right hand
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@smoke@base", "base");
                        break;
                    case 9:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@smoke@idle_a", "idle_a");
                        break;
                    case 10:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@hand_up@base", "base");
                        break;
                    case 11:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@hand_up@idle_a", "idle_a");
                        break;
                    case 12:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@hand_up@idle_b", "idle_e");
                        break;
                    case 13:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@holding_elbow@base", "base");
                        break;
                    case 14:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@holding_elbow@idle_a", "idle_a");
                        break;
                    case 15:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@holding_elbow@idle_b", "idle_e");
                        break;
                    case 16:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@mobile@base", "base");
                        break;
                    case 17:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@mobile@idle_a", "idle_a");
                        break;
                    case 18:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@texting@base", "base");
                        break;
                    case 19:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@texting@idle_a", "idle_a");
                        break;
                    case 20:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@coffee@base", "base");
                        break;
                    case 21:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@coffee@idle_a", "idle_a");
                        break;
                    case 22:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@beer@base", "base");
                        break;
                    case 23:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@beer@idle_a", "idle_b");
                        break;
                    case 24:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@foot_up@base", "base");
                        break;
                    case 25:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@foot_up@idle_a", "idle_a");
                        break;
                    case 26:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@foot_up@idle_b", "idle_d");
                        break;
                    case 27:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@hands_together@base", "base");
                        break;
                    case 28:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@hands_together@idle_a", "idle_c");
                        break;
                    case 29:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@hands_together@idle_b", "idle_e");
                        break;
                    case 30:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@legs_crossed@base", "base");
                        break;
                    case 31:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@legs_crossed@idle_a", "idle_c");
                        break;
                    case 32:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@legs_crossed@idle_b", "idle_d");
                        break;
                    case 33:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@mobile@base", "base");
                        break;
                    case 34:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@mobile@idle_a", "idle_a");
                        break;
                    case 35:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@smoking@base", "base");
                        break;
                    case 36:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@smoking@idle_a", "idle_a");
                        break;
                    case 37:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@texting@base", "base");
                        break;
                    case 38:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@texting@idle_a", "idle_b");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /lean 1 - 38");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("reach", "~y~Syntax /reach 1 - 2")]
        public void reach(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_movie_bulb@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_movie_bulb@bidle_a", "idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /reach 1 - 2");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("workout", "~y~Syntax /workout 1 - 12")]
        public void workout(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_muscle_chin_ups@male@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@prop_human_muscle_chin_ups@male@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_flex@arms_at_side@base", "base");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_flex@arms_at_side@idle_a", "idle_a");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_flex@arms_in_front@base", "base");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_flex@arms_in_front@idle_a", "idle_b");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_free_weights@male@barbell@base", "base");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_free_weights@male@barbell@idle_a", "idle_a");
                        break;
                    case 9:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_push_ups@male@base", "base");
                        break;
                    case 10:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_push_ups@male@idle_a", "idle_a");
                        break;
                    case 11:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_sit_ups@male@base", "base");
                        break;
                    case 12:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_sit_ups@male@idle_a", "idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /workout 1 - 12");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("smoke", "~y~Syntax /smoke 1 - 13")]
        public void smoke(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_aa_smoke@male@idle_a", "idle_c");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_aa_smoke@male@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_prostitute@french@base", "idle_a");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@female@base", "base");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@female@idle_a", "idle_a");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@male@male_a@base", "base");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@male@male_a@idle_a", "idle_c");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@male@male_b@base", "base");
                        break;
                    case 9:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@male@male_b@idle_a", "idle_b");
                        break;
                    case 10://Cigar object for pot
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking_pot@female@base", "base");
                        break;
                    case 11:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking_pot@female@idle_a", "idle_b");
                        break;
                    case 12:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking_pot@male@base", "base");
                        break;
                    case 13:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking_pot@male@idle_a", "idle_c");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /smoke 1 - 13");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("binoculars", "~y~Syntax /binoculars 1 - 6")]
        public void binoculars(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1://Added to right hand, same for the rest
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@idle_a", "idle_b");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@idle_b", "idle_f");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@idle_b", "idle_f");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@idle_b", "idle_f");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@idle_b", "idle_f");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /binoculars 1 - 6");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("hobo", "~y~Syntax /hobo 1 - 9")]
        public void hobo(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1://On right hand, todo: add other signs to the the different animations
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@idle_b", "idle_d");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@depressed@base", "base");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@depressed@idle_a", "idle_a");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_wash@male@high@base", "base");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_wash@male@high@idle_a", "idle_a");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_wash@male@low@base", "base");
                        break;
                    case 9:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_wash@male@low@idle_a", "idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /hobo 1 - 9");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("fallover", "~y~Syntax /fallover 1 - 7")]
        public void fallover(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@idle_b", "idle_d");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "combat@damage@writhe", "writhe_loop");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "combat@damage@writheidle_a", "writhe_idle_a");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "combat@damage@writheidle_b", "writhe_idle_e");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "combat@damage@writheidle_c", "writhe_idle_g");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /fallover 1 - 7");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("laydown", "~y~Syntax /laydown 1 - 12")]
        public void laydown(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_slumped@male@laying_on_left_side@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_slumped@male@laying_on_left_side@idle_a", "idle_b");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_slumped@male@laying_on_right_side@idle_a", "idle_a");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_slumped@male@laying_on_right_side@idle_b", "idle_d");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@female@back@base", "base");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@female@back@idle_a", "idle_a");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@female@front@base", "base");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@female@front@idle_a", "idle_c");
                        break;
                    case 9:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@male@back@base", "base");
                        break;
                    case 10:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@male@back@idle_a", "idle_a");
                        break;
                    case 11:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@male@front@base", "base");
                        break;
                    case 12:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@male@front@idle_a", "idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /laydown 1 - 12");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("drunk", "~y~Syntax /drunk 1 - 2")]
        public void drunk(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@drunk@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@drunk@idle_a", "idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /drunk 1 - 2");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("twitchy", "~y~Syntax /twitchy 1 - 4")]
        public void twitchy(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@twitchy@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@twitchy@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_power_walker@female@base", "base");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_prostitute@crackhooker@base", "base");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /twitchy 1 - 4");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("signal", "~y~Syntax /signal 1 - 3")]
        public void signal(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_car_park_attendant@male@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_car_park_attendant@male@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_janitor@male@base", "base");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /signal 1 - 3");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("cheer", "~y~Syntax /cheer 1 - 8")]
        public void cheer(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@female_a", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@female_b", "base");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@female_c", "base");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@female_d", "base");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@male_a", "base");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@male_b", "base");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@male_d", "base");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@male_e", "base");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /cheer 1 - 8");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("clipboard", "~y~Syntax /clipboard 1 - 3")]
        public void clipboard(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_clipboard@male@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_clipboard@male@idle_a", "idle_c");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_clipboard@male@idle_b", "idle_d");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /clipboard 1 - 3");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("drugdeal", "~y~Syntax /drugdeal 1 - 3")]
        public void drugdeal(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_drug_dealer_hard@male@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_drug_dealer_hard@male@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_drug_dealer_hard@male@idle_b", "idle_d");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /drugdeal 1 - 3");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("gardening", "~y~Syntax /gardening 1 - 4")]
        public void gardening(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_gardener_plant@female@base", "base_female");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_gardener_plant@female@idle_a", "idle_a_female");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_gardener_plant@male@base", "base");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_gardener_plant@male@idle_a", "idle_b");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /gardening 1 - 4");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("guard", "~y~Syntax /guard 1 - 9")]
        public void guard(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_guard_patrol@male@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_guard_patrol@male@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_guard_patrol@male@idle_b", "idle_e");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_stand_fire@male@base", "base");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_stand_guard@male@base", "base");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_stand_guard@male@idle_a", "idle_a");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_stand_guard@male@idle_b", "idle_d");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@amb@code_human_patrol@male@2h@base", "base");
                        break;
                    case 9:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@amb@code_human_patrol@male@2h@idle_a", "idle_c");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /guard 1 - 9");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("hammer", "~y~Syntax /hammer 1 - 2")]
        public void hammer(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hammering@male@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_hammering@male@idle_a", "idle_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /hammer 1 - 2");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("jog", "~y~Syntax /jog 1 - 6")]
        public void jog(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@female@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@female@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@male@fitbase", "base");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@male@fitidle_a", "idle_a");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@male@idle_a", "idle_a");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@male@idle_b", "idle_d");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /hammer 1 - 6");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("guitar", "~y~Syntax /guitar 1 - 3")]
        public void guitar(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1://Acoustic guitar
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_musician@guitar@male@base", "base");
                        break;
                    case 2://Eletric guitar
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_musician@guitar@male@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@mp_player_intcelebrationfemale@air_guitar", "air_guitar");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /guitar 1 - 3");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("getjiggy", "~y~Syntax /getjiggy 1 - 4")]
        public void getjiggy(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_partying@female@partying_beer@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_partying@female@partying_cellphone@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_partying@male@partying_beer@base", "base");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_partying@male@partying_beer@idle_a", "idle_b");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /getjiggy 1 - 4");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("sit", "~y~Syntax /sit 1 - 9")]
        public void sit(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_picnic@female@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_picnic@female@idle_a", "idle_a");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_picnic@male@base", "base");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_picnic@male@idle_a", "idle_a");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_stupor@male@base", "base");
                        break;
                    case 6:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_stupor@male@idle_a", "idle_c");
                        break;
                    case 7:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@heists@heist_safehouse_intro@phone_couch@male", "phone_couch_male_idle");
                        break;
                    case 8:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@heists@heist_safehouse_intro@phone_couch@female", "phone_couch_female_idle");
                        break;
                    case 9:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@mp_rollarcoaster", "idle_b_player_two");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /sit 1 - 9");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("mech", "~y~Syntax /mech 1 - 5")]
        public void mech(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_vehicle_mechanic@male@base", "base");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_vehicle_mechanic@male@idle_a", "idle_b");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_welding@male@base", "base");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_welding@male@idle_a", "idle_a");
                        break;
                    case 5:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /mech 1 - 5");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("yoga", "~y~Syntax /yoga 1 - 2")]
        public void yoga(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_yoga@female@base", "base_b");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "amb@world_human_yoga@male@base", "base_a");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /yoga 1 - 2");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("bonghit", "~y~Syntax /bonghit 1 - 4")]
        public void bonghit(Player player, int number)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@safehouse@bong", "bong_stage1");
                        break;
                    case 2:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@safehouse@bong", "bong_stage2");
                        break;
                    case 3:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@safehouse@bong", "bong_stage3");
                        break;
                    case 4:
                        playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@safehouse@bong", "bong_stage4");
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, "~y~Syntax /bonghit 1 - 4");
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }

        [Command("middlefinger")]
        public void middlefinger(Player player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@mp_player_intcelebrationmale@finger", "finger");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("salute")]
        public void salute(Player player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@mp_player_intcelebrationmale@salute", "salute");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("slowclap")]
        public void slowclap(Player player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@mp_player_intcelebrationmale@slow_clap", "slow_clap");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("facepalm")]
        public void facepalm(Player player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@mp_player_intcelebrationmale@face_palm", "face_palm");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("handsup")]// allowed tp be cuffed
        public void handsup(Player player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "mp_am_hold_up", "handsup_base");
                character.AreHandsUp = true;
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("restrained")]// allowed to be cuffed
        public void restrained(Player player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You can not do an animation whilst inside a vehicle.");
                return;
            }
            Character character = player.GetCharacter();
            if (character.CanDoAnim == true)
            {
                playPlayerAnimationEx(player, (int)(AnimationFlags.Loop), "anim@move_m@prisoner_cuffed_rc", "aim_low_loop");
                character.AreHandsUp = true;
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
    }
}
