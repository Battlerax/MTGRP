// Animations
// Author: Toro
// Date: 18/04/2017
// TODO: Need to check all animations to see if they work.
// May need to add some other animations that will be needed.
// Will wait till release to make changes that the community suggests

using System;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.player_manager;

namespace mtgvrp.core
{
    class Animations : Script
    {

        [Flags]
        public enum AnimationFlags
        {
            Loop = 1 << 0,
            StopOnLastFrame = 1 << 1,
            OnlyAnimateUpperBody = 1 << 4,
            AllowPlayerControl = 1 << 5,
            Cancellable = 1 << 7
        }
        //--------------------------------------
        [Command("item")]//test cmd needs removing
        public void item(Client player)
        {
            NetHandle WeaponObject = API.createObject(-121802573, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            API.attachEntityToEntity(WeaponObject, player, "24817", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            API.setEntityData(player, "Guitar1", WeaponObject);
        }
        
        [Command("pedanim")]//test cmd needs removing
        public void pedanim(Client player)
        {
            API.playPedAnimation(player, true, "amb@world_human_aa_coffee@idle_a", "idle_a");
        }
        [Command("stoppedanim")]//test cmd needs removing
        public void stoppedanim(Client player)
        {
            API.stopPedAnimation(player);
        }
        //--------------------------------------

        [Command("stopanim")]
        public void stopanim(Client player, string id)
        {
            API.stopPlayerAnimation(player);
            API.stopPedAnimation(player);
            Character character = API.getEntityData(player.handle, "Character");
            character.AreHandsUp = false;
            API.getEntityData(player, "AnimObject");
        }

        [Command("hide", "~y~Syntax /Hide 1 - 13")]
        public void hide(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@female@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@female@idle_a", "idle_c");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@female@react_cowering", "base_back_left");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@male@base", "base");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@male@idle_a", "idle_b");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@male@idle_b", "idle_d");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@male@react_cowering", "base_back_left");
                        break;
                    case 8:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower_stand@female@base", "base");
                        break;
                    case 9:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower@female@base", "base");
                        break;
                    case 10:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower_stand@female@idle_a", "idle_c");
                        break;
                    case 11:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower_stand@male@base", "base");
                        break;
                    case 12:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower_stand@male@idle_a", "idle_b");
                        break;
                    case 13:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cower_stand@male@react_cowering", "base_right");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /Hide 1 - 13");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("lookout", "~y~Syntax /lookout 1 - 4")]
        public void lookout(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cross_road@female@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cross_road@female@idle_a", "idle_c");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cross_road@male@base", "base");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_cross_road@male@idle_a", "idle_e");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /lookout 1 - 4");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("crowdcontrol", "~y~Syntax /crowdcontrol 1 - 3")]
        public void crowdcontrol(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_police_investigate@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_police_investigate@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_police_investigate@idle_b", "idle_f");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /crowdcontrol 1 - 3");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("investigate", "~y~Syntax /investigate 1 - 9")]
        public void investigate(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_police_investigate@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_police_investigate@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_police_investigate@idle_b", "idle_f");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@medic@standing@kneel@base", "base");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@medic@standing@kneel@idle_a", "idle_a");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@medic@standing@tendtodead@base", "base");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@medic@standing@tendtodead@idle_a", "idle_a");
                        break;
                    case 8:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@medic@standing@timeofdeath@base", "base");
                        break;
                    case 9:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@medic@standing@timeofdeath@idle_a", "idle_a");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /investigate 1 - 9");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("drink", "~y~Syntax /drink 1 - 12")]
        public void drink(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:// can in right hand
                        NetHandle can = API.createObject(-1321253704, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(can, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", can);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_drinking@beer@female@base", "static");
                        break;
                    case 2:
                        NetHandle can2 = API.createObject(-1321253704, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(can2, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", can2);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_drinking@beer@male@base", "static");
                        break;
                    case 3://Coffee in right hand
                        NetHandle coffee = API.createObject(-163314598, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(coffee, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", coffee);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_aa_coffee@base", "base");
                        break;
                    case 4:
                        NetHandle coffee2 = API.createObject(-163314598, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(coffee2, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", coffee2);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_aa_coffee@idle_a", "idle_a");
                        break;
                    case 5:// beer in right hand 683570518 
                        NetHandle beer = API.createObject(683570518, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(beer, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", beer);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@beer@female@base", "base");
                        break;
                    case 6:
                        NetHandle beer1 = API.createObject(683570518, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(beer1, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", beer1);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@beer@female@idle_a", "idle_f");
                        break;
                    case 7:
                        NetHandle beer2 = API.createObject(683570518, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(beer2, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", beer2);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@beer@male@base", "base");
                        break;
                    case 8:
                        NetHandle beer3 = API.createObject(683570518, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(beer3, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", beer3);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@beer@male@idle_a", "idle_a");
                        break;
                    case 9://coffee
                        NetHandle coffee3 = API.createObject(-163314598, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(coffee3, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", coffee3);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@coffee@female@base", "base");
                        break;
                    case 10:
                        NetHandle coffee4 = API.createObject(-163314598, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(coffee4, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", coffee4);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@coffee@female@idle_a", "idle_a");
                        break;
                    case 11:
                        NetHandle coffee5 = API.createObject(-163314598, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(coffee5, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", coffee5);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@coffee@male@base", "base");
                        break;
                    case 12:
                        NetHandle coffee6 = API.createObject(-163314598, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(coffee6, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", coffee6);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_drinking@coffee@male@idle_a", "idle_a");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /drink 1 - 12");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("crossarms", "~y~Syntax /crossarms 1 - 15")]
        public void crossarms(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_idles_cop@female@static", "static");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_idles_cop@male@static", "static");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@female@base", "base");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@female@idle_a", "idle_b");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@female@idle_b", "idle_e");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@male@base", "base");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@male@idle_a", "idle_b");
                        break;
                    case 8:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@male@idle_b", "idle_e");
                        break;
                    case 9:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_arms_crossed@base", "base");
                        break;
                    case 10:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_arms_crossed@idle_a", "idle_a");
                        break;
                    case 11:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cop_idles@male@idle_b", "idle_e");
                        break;
                    case 12:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_c@base", "base");
                        break;
                    case 13:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_c@idle_a", "idle_a");
                        break;
                    case 14:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_c@idle_b", "idle_d");
                        break;
                    case 15:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@heists@heist_corona@single_team", "single_team_loop_boss");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /crossarms 1 - 15");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("idle", "~y~Syntax /idle 1 - 28")]
        public void idle(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_music_listen@female@base", "static");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_music_listen@male@base", "static");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_rain@female@base", "static");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_rain@male_a@base", "static");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_texting@female@base", "static");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@code_human_wander_texting@male@base", "static");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_movie_studio_light@base", "base");
                        break;
                    case 8:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_movie_studio_light@idle_a", "idle_b");
                        break;
                    case 9:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_parking_meter@female@base", "base_female");
                        break;
                    case 10:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_parking_meter@female@idle_a", "idle_b_female");
                        break;
                    case 11:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_parking_meter@male@base", "base");
                        break;
                    case 12:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_parking_meter@male@idle_a", "idle_a");
                        break;
                    case 13:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_aa_coffee@base", "base");
                        break;
                    case 14:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_golf_player@male@base", "base");
                        break;
                    case 15:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_golf_player@male@idle_a", "idle_a");
                        break;
                    case 16:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_golf_player@male@idle_b", "idle_d");
                        break;
                    case 17:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_arm_side@base", "base");
                        break;
                    case 18:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_arm_side@idle_a", "idle_a");
                        break;
                    case 19:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_hold_arm@base", "base");
                        break;
                    case 20:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@female_hold_arm@idle_a", "idle_a");
                        break;
                    case 21:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_a@base", "base");
                        break;
                    case 22:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_a@idle_a", "idle_b");
                        break;
                    case 23:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_a@idle_a", "idle_a");
                        break;
                    case 24:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_b@base", "base");
                        break;
                    case 25:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hang_out_street@male_b@idle_a", "idle_b");
                        break;
                    case 26:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hiker_standing@female@base", "base");
                        break;
                    case 27:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_prostitute@hooker@base", "base");
                        break;
                    case 28:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_prostitute@hooker@idle_a", "idle_a");
                        break;

                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /idle 1 - 28");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("lean", "~y~Syntax /lean 1 - 38")]
        public void lean(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_bum_bin@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_bum_bin@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_bum_bin@idle_b", "idle_b");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_bum_shopping_cart@male@base", "base");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_bum_shopping_cart@male@idle_a", "idle_a");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@coffee@base", "base");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@coffee@idle_a", "idle_a");
                        break;
                    case 8://Right hand
                        NetHandle smoker = API.createObject(175300549, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(smoker, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", smoker);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@smoke@base", "base");
                        break;
                    case 9:
                        NetHandle smoker2 = API.createObject(175300549, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(smoker2, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", smoker2);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@smoke@idle_a", "idle_a");
                        break;
                    case 10:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@hand_up@base", "base");
                        break;
                    case 11:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@hand_up@idle_a", "idle_a");
                        break;
                    case 12:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@hand_up@idle_b", "idle_e");
                        break;
                    case 13:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@holding_elbow@base", "base");
                        break;
                    case 14:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@holding_elbow@idle_a", "idle_a");
                        break;
                    case 15:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@holding_elbow@idle_b", "idle_e");
                        break;
                    case 16:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@mobile@base", "base");
                        break;
                    case 17:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@mobile@idle_a", "idle_a");
                        break;
                    case 18:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@texting@base", "base");
                        break;
                    case 19:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@female@wall@back@texting@idle_a", "idle_a");
                        break;
                    case 20:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@coffee@base", "base");
                        break;
                    case 21:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@coffee@idle_a", "idle_a");
                        break;
                    case 22:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@beer@base", "base");
                        break;
                    case 23:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@beer@idle_a", "idle_b");
                        break;
                    case 24:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@foot_up@base", "base");
                        break;
                    case 25:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@foot_up@idle_a", "idle_a");
                        break;
                    case 26:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@foot_up@idle_b", "idle_d");
                        break;
                    case 27:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@hands_together@base", "base");
                        break;
                    case 28:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@hands_together@idle_a", "idle_c");
                        break;
                    case 29:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@hands_together@idle_b", "idle_e");
                        break;
                    case 30:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@legs_crossed@base", "base");
                        break;
                    case 31:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@legs_crossed@idle_a", "idle_c");
                        break;
                    case 32:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@legs_crossed@idle_b", "idle_d");
                        break;
                    case 33:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@mobile@base", "base");
                        break;
                    case 34:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@mobile@idle_a", "idle_a");
                        break;
                    case 35:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@smoking@base", "base");
                        break;
                    case 36:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@smoking@idle_a", "idle_a");
                        break;
                    case 37:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@texting@base", "base");
                        break;
                    case 38:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_leaning@male@wall@back@texting@idle_a", "idle_b");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /lean 1 - 38");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("reach", "~y~Syntax /reach 1 - 2")]
        public void reach(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_movie_bulb@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_movie_bulb@bidle_a", "idle_a");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /reach 1 - 2");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("workout", "~y~Syntax /workout 1 - 12")]
        public void workout(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_muscle_chin_ups@male@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@prop_human_muscle_chin_ups@male@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_flex@arms_at_side@base", "base");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_flex@arms_at_side@idle_a", "idle_a");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_flex@arms_in_front@base", "base");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_flex@arms_in_front@idle_a", "idle_b");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_free_weights@male@barbell@base", "base");
                        break;
                    case 8:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_muscle_free_weights@male@barbell@idle_a", "idle_a");
                        break;
                    case 9:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_push_ups@male@base", "base");
                        break;
                    case 10:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_push_ups@male@idle_a", "idle_a");
                        break;
                    case 11:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_sit_ups@male@base", "base");
                        break;
                    case 12:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_sit_ups@male@idle_a", "idle_a");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /workout 1 - 12");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("smoke", "~y~Syntax /smoke 1 - 13")]
        public void smoke(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        NetHandle smoker = API.createObject(175300549, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(smoker, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", smoker);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_aa_smoke@male@idle_a", "idle_c");
                        break;
                    case 2:
                        NetHandle smoker2 = API.createObject(175300549, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(smoker2, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", smoker2);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_aa_smoke@male@idle_a", "idle_a");
                        break;
                    case 3:
                        NetHandle smoker3 = API.createObject(175300549, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(smoker3, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", smoker3);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_prostitute@french@base", "idle_a");
                        break;
                    case 4:
                        NetHandle smoker4 = API.createObject(175300549, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(smoker4, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", smoker4);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@female@base", "base");
                        break;
                    case 5:
                        NetHandle smoker5 = API.createObject(175300549, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(smoker5, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", smoker5);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@female@idle_a", "idle_a");
                        break;
                    case 6:
                        NetHandle smoker6 = API.createObject(175300549, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(smoker6, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", smoker6);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@male@male_a@base", "base");
                        break;
                    case 7:
                        NetHandle smoker7 = API.createObject(175300549, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(smoker7, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", smoker7);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@male@male_a@idle_a", "idle_c");
                        break;
                    case 8:
                        NetHandle smoker8 = API.createObject(175300549, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(smoker8, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", smoker8);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@male@male_b@base", "base");
                        break;
                    case 9:
                        NetHandle smoker9 = API.createObject(175300549, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(smoker9, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", smoker9);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking@male@male_b@idle_a", "idle_b");
                        break;
                    case 10://Cigar object for pot
                        NetHandle pot1 = API.createObject(-1425058769, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(pot1, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", pot1);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking_pot@female@base", "base");
                        break;
                    case 11:
                        NetHandle pot2 = API.createObject(-1425058769, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(pot2, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", pot2);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking_pot@female@idle_a", "idle_b");
                        break;
                    case 12:
                        NetHandle pot3 = API.createObject(-1425058769, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(pot3, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", pot3);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking_pot@male@base", "base");
                        break;
                    case 13:
                        NetHandle pot4 = API.createObject(-1425058769, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(pot4, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", pot4);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_smoking_pot@male@idle_a", "idle_c");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /smoke 1 - 13");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("binoculars", "~y~Syntax /binoculars 1 - 6")]
        public void binoculars(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1://Added to right hand, same for the rest
                        NetHandle Binocu = API.createObject(985101275, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(Binocu, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", Binocu);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@base", "base");
                        break;
                    case 2:
                        NetHandle Binocu2 = API.createObject(985101275, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(Binocu2, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", Binocu2);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@idle_a", "idle_b");
                        break;
                    case 3:
                        NetHandle Binocu3 = API.createObject(985101275, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(Binocu3, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", Binocu3);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@idle_b", "idle_f");
                        break;
                    case 4:
                        NetHandle Binocu4 = API.createObject(985101275, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(Binocu4, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", Binocu4);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@idle_b", "idle_f");
                        break;
                    case 5:
                        NetHandle Binocu5 = API.createObject(985101275, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(Binocu5, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", Binocu5);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@idle_b", "idle_f");
                        break;
                    case 6:
                        NetHandle Binocu6 = API.createObject(985101275, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(Binocu6, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", Binocu6);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_binoculars@female@idle_b", "idle_f");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /binoculars 1 - 6");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("hobo", "~y~Syntax /hobo 1 - 9")]
        public void hobo(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1://On right hand, todo: add other signs to the the different animations
                        NetHandle hobo1 = API.createObject(-245386275, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(hobo1, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", hobo1);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@base", "base");
                        break;
                    case 2:
                        NetHandle hobo2 = API.createObject(-245386275, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(hobo2, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", hobo2);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@idle_a", "idle_a");
                        break;
                    case 3:
                        NetHandle hobo3 = API.createObject(-245386275, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(hobo3, player, "57005", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", hobo3);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@idle_b", "idle_d");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@depressed@base", "base");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@depressed@idle_a", "idle_a");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_wash@male@high@base", "base");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_wash@male@high@idle_a", "idle_a");
                        break;
                    case 8:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_wash@male@low@base", "base");
                        break;
                    case 9:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_wash@male@low@idle_a", "idle_a");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /hobo 1 - 9");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("fallover", "~y~Syntax /fallover 1 - 7")]
        public void fallover(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_freeway@male@idle_b", "idle_d");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "combat@damage@writhe", "writhe_loop");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "combat@damage@writheidle_a", "writhe_idle_a");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "combat@damage@writheidle_b", "writhe_idle_e");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "combat@damage@writheidle_c", "writhe_idle_g");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /fallover 1 - 7");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("laydown", "~y~Syntax /laydown 1 - 12")]
        public void laydown(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_slumped@male@laying_on_left_side@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_slumped@male@laying_on_left_side@idle_a", "idle_b");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_slumped@male@laying_on_right_side@idle_a", "idle_a");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_slumped@male@laying_on_right_side@idle_b", "idle_d");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@female@back@base", "base");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@female@back@idle_a", "idle_a");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@female@front@base", "base");
                        break;
                    case 8:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@female@front@idle_a", "idle_c");
                        break;
                    case 9:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@male@back@base", "base");
                        break;
                    case 10:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@male@back@idle_a", "idle_a");
                        break;
                    case 11:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@male@front@base", "base");
                        break;
                    case 12:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_sunbathe@male@front@idle_a", "idle_a");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /laydown 1 - 12");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("drunk", "~y~Syntax /drunk 1 - 2")]
        public void drunk(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@drunk@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@drunk@idle_a", "idle_a");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /drunk 1 - 2");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("twitchy", "~y~Syntax /twitchy 1 - 4")]
        public void twitchy(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@twitchy@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_bum_standing@twitchy@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_power_walker@female@base", "base");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_prostitute@crackhooker@base", "base");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /twitchy 1 - 4");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("signal", "~y~Syntax /signal 1 - 3")]
        public void signal(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_car_park_attendant@male@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_car_park_attendant@male@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_janitor@male@base", "base");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /signal 1 - 3");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("cheer", "~y~Syntax /cheer 1 - 8")]
        public void cheer(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@female_a", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@female_b", "base");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@female_c", "base");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@female_d", "base");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@male_a", "base");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@male_b", "base");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@male_d", "base");
                        break;
                    case 8:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_cheering@male_e", "base");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /cheer 1 - 8");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("clipboard", "~y~Syntax /clipboard 1 - 3")]
        public void clipboard(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_clipboard@male@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_clipboard@male@idle_a", "idle_c");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_clipboard@male@idle_b", "idle_d");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /clipboard 1 - 3");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("drugdeal", "~y~Syntax /drugdeal 1 - 3")]
        public void drugdeal(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_drug_dealer_hard@male@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_drug_dealer_hard@male@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_drug_dealer_hard@male@idle_b", "idle_d");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /drugdeal 1 - 3");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("gardening", "~y~Syntax /gardening 1 - 4")]
        public void gardening(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_gardener_plant@female@base", "base_female");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_gardener_plant@female@idle_a", "idle_a_female");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_gardener_plant@male@base", "base");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_gardener_plant@male@idle_a", "idle_b");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /gardening 1 - 4");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("guard", "~y~Syntax /guard 1 - 9")]
        public void guard(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_guard_patrol@male@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_guard_patrol@male@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_guard_patrol@male@idle_b", "idle_e");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_stand_fire@male@base", "base");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_stand_guard@male@base", "base");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_stand_guard@male@idle_a", "idle_a");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_stand_guard@male@idle_b", "idle_d");
                        break;
                    case 8:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@amb@code_human_patrol@male@2h@base", "base");
                        break;
                    case 9:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@amb@code_human_patrol@male@2h@idle_a", "idle_c");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /guard 1 - 9");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("hammer", "~y~Syntax /hammer 1 - 2")]
        public void hammer(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hammering@male@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_hammering@male@idle_a", "idle_a");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /hammer 1 - 2");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("jog", "~y~Syntax /jog 1 - 6")]
        public void jog(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@female@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@female@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@male@fitbase", "base");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@male@fitidle_a", "idle_a");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@male@idle_a", "idle_a");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_jog_standing@male@idle_b", "idle_d");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /hammer 1 - 6");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("guitar", "~y~Syntax /guitar 1 - 3")]
        public void guitar(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1://Acoustic guitar
                        NetHandle Guitar1 = API.createObject(-121802573, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(Guitar1, player, "24817", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", Guitar1);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_musician@guitar@male@base", "base");
                        break;
                    case 2://Eletric guitar
                        NetHandle Guitar2 = API.createObject(-121802573, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.attachEntityToEntity(Guitar2, player, "24817", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                        API.setEntityData(player, "AnimObject", Guitar2);
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_musician@guitar@male@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@mp_player_intcelebrationfemale@air_guitar", "air_guitar");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /guitar 1 - 3");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("getjiggy", "~y~Syntax /getjiggy 1 - 4")]
        public void getjiggy(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_partying@female@partying_beer@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_partying@female@partying_cellphone@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_partying@male@partying_beer@base", "base");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_partying@male@partying_beer@idle_a", "idle_b");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /getjiggy 1 - 4");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("sit", "~y~Syntax /sit 1 - 9")]
        public void sit(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_picnic@female@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_picnic@female@idle_a", "idle_a");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_picnic@male@base", "base");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_picnic@male@idle_a", "idle_a");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_stupor@male@base", "base");
                        break;
                    case 6:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_stupor@male@idle_a", "idle_c");
                        break;
                    case 7:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@heists@heist_safehouse_intro@phone_couch@male", "phone_couch_male_idle");
                        break;
                    case 8:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@heists@heist_safehouse_intro@phone_couch@female", "phone_couch_female_idle");
                        break;
                    case 9:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@mp_rollarcoaster", "idle_b_player_two");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /sit 1 - 9");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("mech", "~y~Syntax /mech 1 - 5")]
        public void mech(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_vehicle_mechanic@male@base", "base");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_vehicle_mechanic@male@idle_a", "idle_b");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_welding@male@base", "base");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_welding@male@idle_a", "idle_a");
                        break;
                    case 5:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /mech 1 - 5");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("yoga", "~y~Syntax /yoga 1 - 2")]
        public void yoga(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_yoga@female@base", "base_b");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "amb@world_human_yoga@male@base", "base_a");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /yoga 1 - 2");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("bonghit", "~y~Syntax /bonghit 1 - 4")]
        public void bonghit(Client player, int number)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                switch (number)
                {
                    case 1:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@safehouse@bong", "bong_stage1");
                        break;
                    case 2:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@safehouse@bong", "bong_stage2");
                        break;
                    case 3:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@safehouse@bong", "bong_stage3");
                        break;
                    case 4:
                        API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@safehouse@bong", "bong_stage4");
                        break;
                    default:
                        API.sendChatMessageToPlayer(player, "~y~Syntax /bonghit 1 - 4");
                        break;
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }

        [Command("middlefinger")]
        public void middlefinger(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@mp_player_intcelebrationmale@finger", "finger");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("salute")]
        public void salute(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@mp_player_intcelebrationmale@salute", "salute");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("slowclap")]
        public void slowclap(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@mp_player_intcelebrationmale@slow_clap", "slow_clap");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("facepalm")]
        public void facepalm(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@mp_player_intcelebrationmale@face_palm", "face_palm");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("handsup")]// allowed tp be cuffed
        public void handsup(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "mp_am_hold_up", "handsup_base");
                character.AreHandsUp = true;
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
        [Command("restrained")]// allowed to be cuffed
        public void restrained(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");
            if (character.CanDoAnim == true)
            {
                API.playPlayerAnimation(player, (int)(AnimationFlags.Loop), "anim@move_m@prisoner_cuffed_rc", "aim_low_loop");
                character.AreHandsUp = true;
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You're unable to do an animation right now.");
            }
        }
    }
}