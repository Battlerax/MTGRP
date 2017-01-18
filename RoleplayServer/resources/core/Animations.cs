using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;

namespace RoleplayServer
{
    public class Animations : Script
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

        public Dictionary<string, string> AnimationDictionary = new Dictionary<string, string>
        {
            {"middlefinger", "mp_player_intfinger mp_player_int_finger"},
            {"guitar", "anim@mp_player_intcelebrationmale@air_guitar air_guitar"},
            {"dj", "anim@mp_player_intcelebrationmale@dj dj"},
            {"facepalm", "anim@mp_player_intcelebrationmale@face_palm face_palm"},
            {"knuckle", "anim@mp_player_intcelebrationmale@knuckle_crunch knuckle_crunch"},
            {"no", "anim@mp_player_intcelebrationmale@no_way no_way"},
            {"peace", "anim@mp_player_intcelebrationmale@peace peace"},
            {"photo", "anim@mp_player_intcelebrationmale@photography photography"},
            {"rock", "anim@mp_player_intcelebrationmale@rock rock"},
            {"salute", "anim@mp_player_intcelebrationmale@salute salute"},
            {"slowclap", "anim@mp_player_intcelebrationmale@slow_clap slow_clap"},
            {"surrender", "anim@mp_player_intcelebrationmale@surrender surrender"},
            {"thumbs", "anim@mp_player_intcelebrationmale@thumbs_up thumbs_up"},
            {"wank", "anim@mp_player_intcelebrationmale@wank wank"},
            {"wave", "anim@mp_player_intcelebrationmale@wave wave"},
            {"handsup", "missminuteman_1ig_2 handsup_base"},
            {"chat", "missfbi3_party_d stand_talk_loop_a_male1"},
        };

        [Command("anim", "~y~USAGE:/anim [animation]\n" + "~y~/anim stop to stop the anim.\n"
                                                        + "~y~Animations: middlefinger, guitar, dj, facepalm, knuckle.\n"
                                                        + "~y~Animations: no, peace, photo, rock, salute, slowclap.\n"
                                                        + "~y~Animations: surrender, thumbs, wank, wave, handsup, chat.\n")]
        public void anim_cmd(Client player, string anim)
        {
            if (anim == "stop")
            {
                API.stopPlayerAnimation(player);
            }
            else if (!AnimationDictionary.ContainsKey(anim))
            {
                var flag = 0;
                if (anim == "handsup") flag = 1;

                API.playPlayerAnimation(player, flag, AnimationDictionary[anim].Split()[0], AnimationDictionary[anim].Split()[1]);
            }
        }

    }
}
