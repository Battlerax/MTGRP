using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.job_manager.fisher
{
    public class Fisher : Script
    {
        public static List<Fish> FishTypes;
        private readonly Random _random = new Random();

        public Fisher()
        {
            FishTypes = new List<Fish>
            {
                new Fish("Large Mouth Bass", 50, 1, 15, false, 100),
                new Fish("Pacific Cod", 100, 10, 25, false, 80),
                new Fish("Chinook Salmon", 200, 25, 50, false, 70),
                new Fish("Atlantic Mackerel", 250, 25, 25, false, 60),
                new Fish("Bluefin Tuna", 300, 35, 70, true, 50),
                new Fish("Common Carp", 350, 15, 30, false, 40),
                new Fish("Pacific Herring", 400, 10, 15, false, 30),
                new Fish("Marlin", 500, 75, 150, true, 30),
                new Fish("Shortfin Mako Shark", 550, 125, 200, true, 15),
                new Fish("Dungeness Crab", 600, 1, 3, true, 10),
                new Fish("Great White Shark", 1000, 1500, 1500, true, 5),
            };

            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "caught_fish":

                    Character c = API.getEntityData(player.handle, "Character");

                    var catchStrength = (int) arguments[0];

                    var strengthDifference = catchStrength - c.PerfectCatchStrength;

                    if (strengthDifference < -25)
                    {
                        c.CatchingFish = Fish.None;
                        c.PerfectCatchStrength = 0;

                        API.sendChatMessageToPlayer(player, Color.AdminOrange, "The fish managed to get away...");
                        API.stopPlayerAnimation(player);
                    }
                    else if (strengthDifference > 30)
                    {
                        API.sendChatMessageToPlayer(player, "You snapped your fishing rod!");
                        API.stopPlayerAnimation(player);
                    }
                    else
                    {
                        var weight = MapValue(0, 100, c.CatchingFish.MinWeight, c.CatchingFish.MaxWeight, catchStrength);

                        API.sendChatMessageToPlayer(player, Color.White,
                            "You caught a " + c.CatchingFish.Name + " that weighs " + (int)weight + " pounds. It is worth about $" + c.CatchingFish.calculate_value((int)weight));
                        API.stopPlayerAnimation(player);

                        c.FishOnHand.Add(c.CatchingFish, (int) weight);
                    }
                    break;

                case "snapped_rod":
                    API.sendChatMessageToPlayer(player, "You snapped your fishing rod!");
                    API.stopPlayerAnimation(player);
                    break;
            }
        }

        [Command("fish")]
        public void fish_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.JobOne.Type != JobManager.FisherJob)
            {
                API.sendPictureNotificationToPlayer(player, "You must be a fisher man to use this command.",
                    "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            var isOnLastBoat = false;
            var isLastVehicleBoat = false;

            if (character.LastVehicle != null)
            {
                isOnLastBoat = API.fetchNativeFromPlayer<bool>(player, Hash.IS_PED_ON_SPECIFIC_VEHICLE, player,
                    character.LastVehicle.NetHandle);
                isLastVehicleBoat = API.getVehicleClass(character.LastVehicle.VehModel) == 14;
            }

            if (character.IsInFishingZone == false && (isOnLastBoat == false || isLastVehicleBoat == false))
            {
                API.sendPictureNotificationToPlayer(player,
                    "You are not in a fishing zone or on the last boat you drove.", "CHAR_BLOCKED", 0, 0, "Server",
                    "~r~Command Error");
                return;
            }

            API.playPlayerScenario(player, "WORLD_HUMAN_STAND_FISHING");
            ChatManager.RoleplayMessage(character, "casts out their fishing rod and begins to fish.",
                ChatManager.RoleplayMe);

            character.CatchTimer = new Timer() {Interval = (_random.Next(1, 5) * 1000)};
            character.CatchTimer.Elapsed += delegate
            {
                StartCatchFish(character, isOnLastBoat == false || isLastVehicleBoat == false);
            };
            character.CatchTimer.Start();
        }

        [Command("viewfish")]
        public void viewfish_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            API.sendChatMessageToPlayer(player, Color.White, "----------------------------------------------");
            foreach (var f in character.FishOnHand)
            {
                API.sendChatMessageToPlayer(player, Color.Grey, f.Key.Name + " - " + f.Value + " pounds");
            }
            API.sendChatMessageToPlayer(player, Color.White, "----------------------------------------------");
        }

        [Command("sellfish")]
        public void sellfish_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.JobZoneType != 2)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You are not near the sell fish point!");
                return; 
            }

            var job = JobManager.GetJobById(character.JobZone);

            if (job == null)
            {
                API.sendChatMessageToPlayer(player, "null job");
                return;
            }

            if (job.Type != JobManager.FisherJob)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You are not near the sell fish point!");
                return;
            }

            var totalValue = character.FishOnHand.Sum(f => f.Key.calculate_value(f.Value));

            InventoryManager.GiveInventoryItem(character, new Money(), totalValue);
            character.FishOnHand.Clear();
            API.sendChatMessageToPlayer(player, "You have sold all of your fish for $" + totalValue);
        }

        public void StartCatchFish(Character c, bool boatFishing)
        {
            c.CatchTimer.Stop();
            c.CatchingFish = random_catch(boatFishing);
            c.PerfectCatchStrength = _random.Next(25, 95);

            API.sendChatMessageToPlayer(c.Client, Color.AdminOrange,
                "* You begin to feel a fish tugging at your line! Control your reeling strength by tapping space.");
            API.triggerClientEvent(c.Client, "start_fishing", c.PerfectCatchStrength);
        }

        public Fish random_catch(bool inBoat)
        {
            var catchPool = new List<Fish>();
            var dropChance = _random.Next(1, 100);

            foreach (var f in FishTypes)
            {
                if (inBoat == false && f.RequiresBoat == true)
                {
                    continue;
                }

                if (f.Rarity >= dropChance)
                {
                    catchPool.Add(f);
                }
            }

            return catchPool[_random.Next(catchPool.Count)];
        }

        public double MapValue(double a0, double a1, double b0, double b1, double a)
        {
            return b0 + (b1 - b0) * ((a - a0) / (a1 - a0));
        }
    }
}