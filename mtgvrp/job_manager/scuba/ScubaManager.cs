using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.player_manager;

namespace mtgvrp.job_manager.scuba
{
    class ScubaManager : Script
    {
        public ScubaManager()
        {
            API.onPlayerDisconnected += API_onPlayerDisconnected;
            API.onResourceStart += API_onResourceStart;
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            if (eventName == "SCUBA_ISUNDERWATER")
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
                    "Oxygen Remaining: " + Math.Round((scubaitem[0].OxygenRemaining / ScubaItem.MaxOxygen) * 100f) +
                    "%");
            }
        }

        private void API_onResourceStart()
        {
            //pick random 30 spots and spawn em there.
            var rnd = new Random();
            for (int i = 1; i <= 30; i++)
            {
                reset:
                var a = rnd.Next(0, 100);

                if (_treasureObjects.Any(x => x.position == _treasuresLocations[a][0]))
                {
                    goto reset;
                }

                _treasureObjects.Add(API.createObject(-994740387, _treasuresLocations[a][0], _treasuresLocations[a][1]));
            }
        }

        private readonly Vector3[][] _treasuresLocations = new Vector3[][]
        {
            new [] {new Vector3(1677.09, -3092.195, -68.47385), new Vector3(2.897643, 6.021974, 0.1524492)},
            new [] {new Vector3(1679.471, -3088.021, -67.738), new Vector3(-12.12189, 14.61349, -1.560027)},
            new [] {new Vector3(1680.416, -3078.392, -67.11446), new Vector3(13.29521, 4.579041, 0.5339493)},
            new [] {new Vector3(-687.8019, -2841.075, -16.08758), new Vector3(0, 0, 0)},
            new [] {new Vector3(-3179.913, 3028.562, -37.67741), new Vector3(-26.47854, -25.3989, 6.069763)},
            new [] {new Vector3(-3177.457, 3038.4, -35.37143), new Vector3(-10.84132, 18.14924, -1.736626)},
            new [] {new Vector3(-3180.512, 3048.733, -39.74689), new Vector3(-6.21134, 19.31663, -1.05808)},
            new [] {new Vector3(3888.208, 3038.528, -22.81003), new Vector3(5.775259, 13.57553, 0.6879818)},
            new [] {new Vector3(3890.131, 3048.454, -26.9518), new Vector3(-18.12252, -12.07423, 1.932579)},
            new [] {new Vector3(3893.667, 3050.441, -19.74888), new Vector3(-18.68619, -11.03161, 1.820491)},
            new [] {new Vector3(3895.624, 3051.604, -19.73941), new Vector3(-18.68619, -11.03161, 1.820491)},
            new [] {new Vector3(3408.317, 6312.32, -53.04272), new Vector3(9.031884, -21.27556, -1.699833)},
            new [] {new Vector3(3400.157, 6318.98, -50.99586), new Vector3(11.82685, -19.37822, -2.026275)},
            new [] {new Vector3(3392.913, 6330.798, -53.81186), new Vector3(17.64063, -16.12252, -2.517987)},
            new [] {new Vector3(3133.255, -268.9792, -22.85737), new Vector3(-4.999798, 18.82458, -0.829325)},
            new [] {new Vector3(3143.401, -255.4535, -27.0895), new Vector3(-2.804033, 14.1354, -0.3477242)},
            new [] {new Vector3(3156.862, -262.501, -8.134127), new Vector3(0, 0, 0)},
            new [] {new Vector3(3160.358, -290.2576, -8.949234), new Vector3(0, 0, 0)},
            new [] {new Vector3(3162.119, -307.4444, -28.67716), new Vector3(-1.89739, -5.051602, 0.08370537)},
            new [] {new Vector3(3162.872, -323.7694, -27.96595), new Vector3(0, 0, 0)},
            new [] {new Vector3(3165.095, -345.9052, -29.14786), new Vector3(-2.715434, 10.60218, -0.2520023)},
            new [] {new Vector3(3199.655, -374.752, -34.32474), new Vector3(29.63354, -4.413833, -1.168105)},
            new [] {new Vector3(3195.548, -382.471, -27.06491), new Vector3(25.68871, -4.354774, -0.9933795)},
            new [] {new Vector3(3198.412, -387.5429, -30.92855), new Vector3(-64.87336, -33.90033, 21.9241)},
            new [] {new Vector3(3143.29, -365.8204, -22.9013), new Vector3(0.6845084, 19.56757, 0.118037)},
            new [] {new Vector3(-2853.95, -424.0601, -39.98696), new Vector3(0, 0, 0)},
            new [] {new Vector3(-2843.167, -434.856, -40.64767), new Vector3(3.525461, 1.164736, 0.03584577)},
            new [] {new Vector3(-2831.509, -448.6629, -40.51773), new Vector3(0, 0, 0)},
            new [] {new Vector3(-2834.17, -470.5973, -35.41136), new Vector3(2.232773, -36.17342, -0.729292)},
            new [] {new Vector3(2679.283, -1357.308, -24.14859), new Vector3(-7.559061, 6.079813, -0.4020146)},
            new [] {new Vector3(2672.818, -1385.385, -22.6331), new Vector3(11.5992, 3.470054, 0.3525577)},
            new [] {new Vector3(2658.537, -1400.003, -21.6268), new Vector3(-7.193083, 7.02101, -0.4418502)},
            new [] {new Vector3(2666.719, -1393.919, -18.25154), new Vector3(46.78807, -8.147886, -3.529735)},
            new [] {new Vector3(2689.211, -1391.55, -13.98199), new Vector3(8.542039, 5.272766, 0.3940552)},
            new [] {new Vector3(2659.579, -1437.452, -16.9788), new Vector3(1.184798, 10.3957, 0.107784)},
            new [] {new Vector3(3270.153, 6411.063, -47.33008), new Vector3(-62.63033, 13.06737, -7.971577)},
            new [] {new Vector3(3274.515, 6417.231, -50.69934), new Vector3(-5.37865, 5.575721, -0.2621094)},
            new [] {new Vector3(3267.157, 6406.816, -48.94754), new Vector3(-4.256682, 7.108151, -0.2645032)},
            new [] {new Vector3(-3113.212, 3618.568, -20.69468), new Vector3(-13.25437, -10.46149, 1.218809)},
            new [] {new Vector3(-3151.669, 3625.203, -26.95417), new Vector3(-12.84072, -2.930674, 0.3298534)},
            new [] {new Vector3(-3178.28, 3655.156, -38.5682), new Vector3(-3.54159, -4.897265, 0.1514965)},
            new [] {new Vector3(-3221.136, 3666.319, -36.8802), new Vector3(-8.769338, 1.066828, -0.08180316)},
            new [] {new Vector3(-3247.354, 3668.261, -31.34065), new Vector3(-38.90972, -1.100474, 0.3887334)},
            new [] {new Vector3(-3266.339, 3674.716, -37.0201), new Vector3(-38.90972, -1.100474, 0.3887334)},
            new [] {new Vector3(-3260.508, 3675.974, -43.41463), new Vector3(14.74409, -34.07352, -4.540786)},
            new [] {new Vector3(-3294.217, 3684.058, -77.20634), new Vector3(-11.00326, -5.631691, 0.5428658)},
            new [] {new Vector3(-3247.376, 3689.795, -25.34903), new Vector3(-11.87058, -16.65053, 1.743182)},
            new [] {new Vector3(-3226.514, 3669.89, -20.32624), new Vector3(48.49942, -14.93642, -6.75879)},
            new [] {new Vector3(-3228.127, 3679.775, -31.2077), new Vector3(41.21493, 3.940362, 1.482173)},
            new [] {new Vector3(290.2921, 3978.223, -10.20682), new Vector3(-8.101846, 7.444748, -0.5279767)},
            new [] {new Vector3(275.9005, 3967.651, -1.854694), new Vector3(-3.681539, 22.12339, -0.7199718)},
            new [] {new Vector3(270.0861, 3979.805, -7.46975), new Vector3(-13.09327, 9.196983, -1.05769)},
            new [] {new Vector3(302.8917, 3955.07, -5.890894), new Vector3(-9.896611, -11.32068, 0.983316)},
            new [] {new Vector3(310.4549, 3969.998, 1.405255), new Vector3(9.013588, -1.448173, -0.1141525)},
            new [] {new Vector3(310.0066, 3974.342, -6.113909), new Vector3(27.09939, -15.05135, -3.647092)},
            new [] {new Vector3(327.5087, 3983.853, -3.245137), new Vector3(20.01167, 4.236127, 0.7477183)},
            new [] {new Vector3(347.1971, 3991.224, -6.601399), new Vector3(0, 0, 0)},
            new [] {new Vector3(353.1013, 3985.27, -6.390087), new Vector3(6.496686, 1.345961, 0.0763936)},
            new [] {new Vector3(345.9875, 3964.454, -3.97958), new Vector3(-4.437472, -2.488914, 0.09644537)},
            new [] {new Vector3(302.7115, 3936.802, -4.303668), new Vector3(-4.518922, -9.592679, 0.379369)},
            new [] {new Vector3(304.8608, 3931.843, -3.860817), new Vector3(1.124119, -9.06155, -0.08908048)},
            new [] {new Vector3(4140.373, 3580.959, -43.88686), new Vector3(25.07923, 8.695318, 1.93755)},
            new [] {new Vector3(4152.948, 3587.196, -44.50946), new Vector3(6.607016, 7.777958, 0.4496411)},
            new [] {new Vector3(4165.097, 3587.848, -47.02487), new Vector3(5.910738, 17.33158, 0.90164)},
            new [] {new Vector3(4180.551, 3571.368, -54.41982), new Vector3(0.3110801, 29.54741, 0.08203837)},
            new [] {new Vector3(4166.259, 3521.066, -45.9937), new Vector3(31.82917, 7.785298, 2.222986)},
            new [] {new Vector3(4152.74, 3514.908, -37.59468), new Vector3(-9.010566, 13.62984, -1.079018)},
            new [] {new Vector3(4132.903, 3512.231, -37.19473), new Vector3(-7.477607, 7.19277, -0.4706435)},
            new [] {new Vector3(-995.3552, 6382.048, -20.02521), new Vector3(-9.946545, -26.2159, 2.321605)},
            new [] {new Vector3(-989.428, 6384.23, -18.22164), new Vector3(3.753146, -31.57849, -1.061622)},
            new [] {new Vector3(-1000.893, 6435.843, -19.85126), new Vector3(16.12429, 9.959817, 1.414271)},
            new [] {new Vector3(-1013.243, 6504.479, -38.12735), new Vector3(-1.505564, 4.825318, -0.06343909)},
            new [] {new Vector3(-1007.824, 6535.076, -32.67678), new Vector3(17.30025, 5.808006, 0.8843156)},
            new [] {new Vector3(-990.3873, 6559.503, -26.35473), new Vector3(6.593242, -4.483918, -0.2584078)},
            new [] {new Vector3(1831.309, -2917.387, -36.96521), new Vector3(0, 0, 0)},
            new [] {new Vector3(1829.593, -2908.548, -33.69431), new Vector3(38.76239, 10.14348, 3.576531)},
            new [] {new Vector3(1821.289, -2921.747, -30.46195), new Vector3(-13.69983, -20.00603, 2.427608)},
            new [] {new Vector3(1788.235, -2931.907, -38.48955), new Vector3(21.97222, 3.925969, 0.7624301)},
            new [] {new Vector3(1768.01, -2969.897, -48.76947), new Vector3(-6.589567, -14.06846, 0.8139765)},
            new [] {new Vector3(1745.731, -2995.973, -56.35821), new Vector3(23.82305, 17.9871, 3.824306)},
            new [] {new Vector3(1781.053, -2994.707, -48.36283), new Vector3(-5.490724, -11.70208, 0.5630956)},
            new [] {new Vector3(1785.545, -2959.741, -44.59949), new Vector3(8.61081, 7.697156, 0.5803499)},
            new [] {new Vector3(2655.295, 6644.149, -24.11547), new Vector3(-46.68645, -1.674309, 0.7226112)},
            new [] {new Vector3(2675.705, 6656.159, -27.15726), new Vector3(35.46228, 14.50549, 4.660343)},
            new [] {new Vector3(2674.807, 6653.192, -16.48922), new Vector3(-18.71238, 8.716446, -1.438853)},
            new [] {new Vector3(2682.794, 6659.122, -22.4077), new Vector3(-65.18364, -2.720936, 1.739757)},
            new [] {new Vector3(258.0971, -2284.289, -5.91643), new Vector3(7.411272, 5.154916, 0.3340876)},
            new [] {new Vector3(225.762, -2285.861, -4.056532), new Vector3(6.30818, 3.103512, 0.17106)},
            new [] {new Vector3(264.5379, -2295.861, -12.91337), new Vector3(10.95253, 4.64333, 0.4454021)},
            new [] {new Vector3(-117.7277, -2868.986, -21.69338), new Vector3(28.52148, 13.72033, 3.502926)},
            new [] {new Vector3(-134.3521, -2849.701, -15.76051), new Vector3(4.855381, 7.960722, 0.3380497)},
            new [] {new Vector3(-130.937, -2865.447, -9.719579), new Vector3(4.007541, 0.9633103, 0.03370383)},
            new [] {new Vector3(-165.9869, -2863.513, -21.16403), new Vector3(5.691579, -0.9120921, -0.04534057)},
            new [] {new Vector3(-179.6838, -2865.847, -20.66009), new Vector3(4.962674, 3.39562, 0.1471904)},
            new [] {new Vector3(-192.3285, -2877.071, -21.41343), new Vector3(-14.00117, -5.55448, 0.6825889)},
            new [] {new Vector3(-223.6416, -2871.077, -20.07315), new Vector3(0, 0, 0)},
            new [] {new Vector3(-248.1568, -2864.851, -22.56376), new Vector3(3.609929, 8.188173, 0.2584739)},
            new [] {new Vector3(-168.0641, -2857.954, -20.30312), new Vector3(12.20617, -3.680465, -0.393662)},
            new [] {new Vector3(-140.2592, -2868.871, -9.566155), new Vector3(1.782233, 6.317934, 0.09836999)},
            new [] {new Vector3(-90.87718, -2874.256, -25.74241), new Vector3(0, 0, 0)},
            new [] {new Vector3(-159.6433, -2858.358, -13.95227), new Vector3(12.90809, 1.245686, 0.1409214)},
        };

        private readonly List<GrandTheftMultiplayer.Server.Elements.Object> _treasureObjects = new List<GrandTheftMultiplayer.Server.Elements.Object>();

        private void API_onPlayerDisconnected(Client player, string reason)
        {
            Character c = player.GetCharacter();

            if (c == null)
                return;

            if (c.IsScubaDiving)
            {
                CancelScuba(player);
            }
        }

        [Command("pickuptreasure"), Help(HelpManager.CommandGroups.ScubaActivity, "Pickup a treasure you found in the ocean while diving.")]
        public void Pickuptreasure(Client player)
        {
            var character = player.GetCharacter();
            if (!character.IsScubaDiving)
            {
                API.sendChatMessageToPlayer(player, "You already have the kit on.");
                return;
            }

            var itm = _treasureObjects.FirstOrDefault(x => x.position.DistanceTo(player.position) <= 5.0f);
            if (itm == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't near any treasure.");
                return;
            }

            var rnd = new Random();
            int amnt = rnd.Next(1000, 5000);
            InventoryManager.GiveInventoryItem(character, new Money(), amnt, true);
            API.sendChatMessageToPlayer(player, "You have found a treasure worth ~g~$" + amnt);
            LogManager.Log(LogManager.LogTypes.Stats, $"[Minigame] {character.CharacterName}[{player.GetAccount().AccountName}] has earned ${amnt} from a scuba treasure.");

            itm.delete();
            _treasureObjects.Remove(itm);

            //Add new one.
            reset:
            var a = rnd.Next(0, 100);

            if (_treasureObjects.Any(x => x.position == _treasuresLocations[a][0]))
            {
                goto reset;
            }

            _treasureObjects.Add(API.createObject(-994740387, _treasuresLocations[a][0], _treasuresLocations[a][1]));
        }

        [Command("togglescuba"), Help(HelpManager.CommandGroups.ScubaActivity, "Toggle your scuba kit on and off.")]
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
                CancelScuba(player);
                API.sendChatMessageToPlayer(player, "You have dequiped the scuba set.");
                character.update_ped();
                return;
            }

            //Set clothes.
            if (character.Model.Gender == 0)
            {
                API.setPlayerClothes(player, 4, 16, 0);   //Legs
                API.setPlayerClothes(player, 8, 57, 0);   //Undershirt
                API.setPlayerClothes(player, 11, 15, 0); //Tops
                API.setPlayerClothes(player, 3, 15, 0); //Torso
                API.setPlayerClothes(player, 6, 34, 0); //feet
            }
            else
            {
                API.setPlayerClothes(player, 4, 15, 0);   //Legs
                API.setPlayerClothes(player, 8, 3, 0);   //Undershirt
                API.setPlayerClothes(player, 11, 101, 0); //Tops
                API.setPlayerClothes(player, 3, 15, 0); //Torso
                API.setPlayerClothes(player, 6, 35, 0); //feet
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

            API.triggerClientEvent(player, "REQUEST_SCUBA_UNDERWATER");
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
            GrandTheftMultiplayer.Server.Elements.Object head = API.getEntityData(player, "SCUBA_HEAD");
            GrandTheftMultiplayer.Server.Elements.Object tank = API.getEntityData(player, "SCUBA_TANK");
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
            Init.SendEvent(player, "UPDATE_SCUBA_PERCENTAGE", "none");

            //Set the variable.
            character.IsScubaDiving = false;

            //Set normal underwater time.
            API.sendNativeToPlayer(player, Hash.SET_PED_MAX_TIME_UNDERWATER, player.handle, 60);
        }
    }
}