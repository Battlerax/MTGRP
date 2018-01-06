using System;
using System.Collections.Generic;
using System.Timers;


using GTANetworkAPI;

using mtgvrp.core;
using mtgvrp.core.Items;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using Color = mtgvrp.core.Color;

namespace mtgvrp.job_manager.fisher
{
    public class Fisher : Script
    {
        public static List<Fish> FishTypes;
        private readonly Random _random = new Random();

        public Fisher()
        {
            FishTypes = new List<Fish>
            {
                new Fish("Large Mouth Bass", 10, 1, 15, false, 100),
                new Fish("Pacific Cod", 20, 10, 25, false, 80),
                new Fish("Chinook Salmon", 40, 25, 50, false, 70),
                new Fish("Atlantic Mackerel", 50, 25, 26, false, 60),
                new Fish("Bluefin Tuna", 60, 35, 70, true, 50),
                new Fish("Common Carp", 70, 15, 30, false, 40),
                new Fish("Pacific Herring", 80, 10, 15, false, 30),
                new Fish("Marlin", 100, 75, 150, true, 30),
                new Fish("Shortfin Mako Shark", 110, 125, 200, true, 15),
                new Fish("Dungeness Crab", 120, 1, 3, true, 10),
                new Fish("Great White Shark", 200, 1500, 1501, true, 5),
            };

            Event.OnClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "caught_fish":

                    Character c = player.GetCharacter();

                    var catchStrength = (int) arguments[0];

                    var strengthDifference = catchStrength - c.PerfectCatchStrength;

                    if (strengthDifference < -25)
                    {
                        c.CatchingFish = Fish.None;
                        c.PerfectCatchStrength = 0;

                        API.SendChatMessageToPlayer(player, Color.AdminOrange, "The fish managed to get away...");
                        API.StopPlayerAnimation(player);
                    }
                    else if (strengthDifference > 30)
                    {
                        API.SendChatMessageToPlayer(player, "You snapped your fishing rod!");
                        API.StopPlayerAnimation(player);
                    }
                    else
                    {
                        var weight = MapValue(0, 100, c.CatchingFish.MinWeight, c.CatchingFish.MaxWeight, catchStrength);

                        API.SendChatMessageToPlayer(player, Color.White,
                            "You caught a " + c.CatchingFish.Name + " that weighs " + weight + " pounds. It is worth about $" + c.CatchingFish.calculate_value(weight));
                        API.StopPlayerAnimation(player);

                        var fish = (Fish) c.CatchingFish;
                        fish.ActualWeight = weight;

                        var status = InventoryManager.GiveInventoryItem(c, fish);
                        switch (status)
                        {
                            case InventoryManager.GiveItemErrors.MaxAmountReached:
                                API.SendChatMessageToPlayer(player, Color.White,
                                    "You have the max amount of this fish in your inventory.");
                                break;
                            case InventoryManager.GiveItemErrors.NotEnoughSpace:
                                API.SendChatMessageToPlayer(player, Color.White,
                                    "Your inventory does not have enough space for another fish. You throw the fish back into the water.");
                                break;
                        }
                    }
                    break;

                case "snapped_rod":
                    API.SendChatMessageToPlayer(player, "You snapped your fishing rod!");
                    InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(FishingRod), 1);
                    API.StopPlayerAnimation(player);
                    break;
            }
        }

        [Command("fish"), Help(HelpManager.CommandGroups.FisherJob, "Pretty obvious, its to fish!")]
        public void fish_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.JobOne.Type != JobManager.JobTypes.Fisher)
            {
                API.SendPictureNotificationToPlayer(player, "You must be a fisher man to use this command.",
                    "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            var isOnLastBoat = false;
            var isLastVehicleBoat = false;

            if (InventoryManager.DoesInventoryHaveItem<FishingRod>(character).Length < 1)
            {
                player.SendChatMessage("You don't own a fishing rod. Buy one from the boat shop.");
                return;
            }

            if (DateTime.Now < character.NextFishTime)
            {
                API.SendChatMessageToPlayer(player, "Wait 5 seconds before doing this again.");
                return;
            }

            if (API.IsPlayerInAnyVehicle(player))
            {
                API.SendChatMessageToPlayer(player, "You can't fish while driving a vehicle.");
                return;
            }

            if (character.LastVehicle != null)
            {
                isOnLastBoat = API.FetchNativeFromPlayer<bool>(player, Hash.IS_PED_ON_SPECIFIC_VEHICLE, player,
                    character.LastVehicle.NetHandle);
                isLastVehicleBoat = API.GetVehicleClass(character.LastVehicle.VehModel) == 14;
            }

            if (character.IsInFishingZone == false && (isOnLastBoat == false || isLastVehicleBoat == false))
            {
                API.SendPictureNotificationToPlayer(player,
                    "You are not in a fishing zone or on the last boat you drove.", "CHAR_BLOCKED", 0, 0, "Server",
                    "~r~Command Error");
                return;
            }

            character.NextFishTime = DateTime.Now.AddSeconds(5);
            API.PlayPlayerScenario(player, "WORLD_HUMAN_STAND_FISHING");
            ChatManager.RoleplayMessage(character, "casts out their fishing rod and begins to fish.",
                ChatManager.RoleplayMe);

            character.CatchTimer = new Timer() {Interval = (_random.Next(1, 5) * 1000)};
            character.CatchTimer.Elapsed += delegate
            {
                StartCatchFish(character, isOnLastBoat == false || isLastVehicleBoat == false);
            };
            character.CatchTimer.Start();
        }

        [Command("viewfish"), Help(HelpManager.CommandGroups.FisherJob, "View the fish in your inventory")]
        public void viewfish_cmd(Client player)
        {
            Character character = player.GetCharacter();

            API.SendChatMessageToPlayer(player, Color.White, "----------------------------------------------");
            foreach (var f in character.Inventory)
            {
                if (f.GetType() == typeof(Fish))
                {
                    var fish = (Fish) f;
                    API.SendChatMessageToPlayer(player, Color.Grey, fish.LongName + " (" + fish.ActualWeight + " lbs)");
                }
            }
            API.SendChatMessageToPlayer(player, Color.White, "----------------------------------------------");
        }

        [Command("sellfish"), Help(HelpManager.CommandGroups.FisherJob, "Sell the fish you currently have.")]
        public void sellfish_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.JobZoneType != 2)
            {
                API.SendChatMessageToPlayer(player, Color.White, "You are not near the sell fish point!");
                return; 
            }

            var job = JobManager.GetJobById(character.JobZone);

            if (job == null)
            {
                API.SendChatMessageToPlayer(player, "null job");
                return;
            }

            if (job.Type != JobManager.JobTypes.Fisher)
            {
                API.SendChatMessageToPlayer(player, Color.White, "You are not near the sell fish point!");
                return;
            }

            double totalValue = 0;

            foreach (var f in character.Inventory)
            {
                if (f.GetType() == typeof(Fish))
                {
                    var fish = (Fish) f;
                    totalValue += fish.calculate_value();
                }
            }

            InventoryManager.DeleteInventoryItem(character, typeof(Fish));
            InventoryManager.GiveInventoryItem(character, new Money(), (int)Math.Round(totalValue));
            API.SendChatMessageToPlayer(player, "You have sold all of your fish for $" + totalValue);
            LogManager.Log(LogManager.LogTypes.Stats, $"[Job] {character.CharacterName}[{player.GetAccount().AccountName}] has earned ${totalValue} from selling their fish.");
        }

        public void StartCatchFish(Character c, bool boatFishing)
        {
            c.CatchTimer.Stop();
            c.CatchingFish = random_catch(boatFishing);
            c.PerfectCatchStrength = _random.Next(25, 95);

            API.SendChatMessageToPlayer(c.Client, Color.AdminOrange,
                "* You begin to feel a fish tugging at your line! Control your reeling strength by tapping space bar.");
            API.TriggerClientEvent(c.Client, "start_fishing", c.PerfectCatchStrength);
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
