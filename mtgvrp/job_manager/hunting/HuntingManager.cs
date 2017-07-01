using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using System.Timers;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.property_system;

namespace mtgvrp.job_manager.hunting
{
   

    public class HuntingManager : Script
    {
        public static List<HuntingAnimal> SpawnedAnimals = new List<HuntingAnimal>();

        public enum AnimalTypes
        {
            Deer,
            Boar,
        }

        public enum AnimalState
        {
            Fleeing,
            Grazing,
            Wandering,
        }

        public static Vector3[] AnimalSpawns =
        {
            new Vector3(-1725.521, 4699.659, 33.80555),
            new Vector3(-1690.836, 4682.494, 24.47228),
            new Vector3(-1661.219, 4650.042, 26.12522),
            new Vector3(-1613.11, 4632.693, 46.37965),
            new Vector3(-1569.1, 4688.946, 48.04772),
            new Vector3(-1549.585, 4766.055, 60.47577),
            new Vector3(-1461.021, 4702.999, 39.26906),
            new Vector3(-1397.87, 4637.824, 72.12587),
            new Vector3(-617.851, 5762.557, 31.45378),
            new Vector3(-613.3984, 5863.435, 22.00531),
            new Vector3(-512.6949, 5940.441, 34.46115),
            new Vector3(-363.9753, 5921.967, 43.97315),
            new Vector3(-384.0528, 5866.263, 49.28809),
            new Vector3(-374.6584, 5798.462, 62.83068),
            new Vector3(-448.7513, 5565.69, 71.9878),
            new Vector3(-551.2087, 5167.825, 97.50465),
            new Vector3(-603.5089, 5154.867, 110.1652),
            new Vector3(-711.7279, 5149.544, 114.7229),
            new Vector3(-711.3442, 5075.649, 138.9036),
            new Vector3(-672.9759, 5042.516, 152.8032),
            new Vector3(-661.6283, 4974.586, 172.7258),
            new Vector3(-600.277, 4918.438, 176.7214),
            new Vector3(-588.3793, 4889.981, 181.3767),
            new Vector3(-549.8376, 4838.274, 183.2239),
            new Vector3(-478.639, 4831.655, 209.2594),
            new Vector3(-399.3954, 4865.303, 203.7752),
            new Vector3(-411.9441, 4946.082, 177.4535),
            new Vector3(-414.8653, 5074.294, 149.0627),
        };

        public HuntingManager()
        {
            API.onResourceStart += OnHuntingManagerStart;
            API.onPlayerWeaponAmmoChange += OnPlayerWeaponAmmoChange;
            API.onPlayerWeaponSwitch += OnPlayerWeaponSwitch;
            API.onClientEventTrigger += OnClientEventTrigger;
        }

        public void OnHuntingManagerStart()
        {
            foreach (var spawn in AnimalSpawns)
            {
                new HuntingAnimal(spawn, AnimalTypes.Deer, AnimalState.Wandering).UpdateState = true;
                new HuntingAnimal(spawn, AnimalTypes.Boar, AnimalState.Wandering).UpdateState = true;
            }
        }

        public void OnPlayerWeaponSwitch(Client player, WeaponHash oldWeapon)
        {
            if (oldWeapon == WeaponHash.SniperRifle)
            {
                foreach (var a in SpawnedAnimals)
                {
                    API.triggerClientEvent(player, "toggle_animal_invincible", a.handle, true);
                }
            }
            else if (API.shared.getPlayerCurrentWeapon(player) == WeaponHash.SniperRifle)
            {
                foreach (var a in SpawnedAnimals)
                {
                    API.triggerClientEvent(player, "toggle_animal_invincible", a.handle, false);
                }
            }
        }

        public void OnPlayerWeaponAmmoChange(Client player, WeaponHash weapon, int oldAmmo)
        {
            if (weapon == WeaponHash.SniperRifle)
            {
                var c = player.GetCharacter();
                var ammo = InventoryManager.DoesInventoryHaveItem(c, typeof(AmmoItem));

                if (ammo.Length > 0)
                {
                    InventoryManager.DeleteInventoryItem(c, typeof(AmmoItem), 1);

                    if (ammo[0].Amount == 0)
                    {
                        API.sendChatMessageToPlayer(player,
                        "~r~[ERROR]~w~ You've run out of 5.56 ammo and your gun was destroyed.");
                        InventoryManager.DeleteInventoryItem(c, typeof(Weapon), 1,
                            w => w.CommandFriendlyName == "SniperRifle");
                    }
                }

                foreach (var a in SpawnedAnimals)
                {
                    if (player.position.DistanceTo(API.getEntityPosition(a.handle)) < 80f)
                    {
                        a.State = AnimalState.Fleeing;
                        a.FleeingPed = player;
                        a.UpdateState = true;
                        a.StateChangeTick = 0;
                    }
                }
            }
        }

        public void OnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            if (eventName == "update_animal_position")
            {
                API.shared.setEntityPosition((NetHandle)arguments[0], (Vector3)arguments[1]);
            }
        }

        public static Vector3 RandomFarawayDestination(Vector3 currentPos)
        {
            var choices = Array.FindAll(AnimalSpawns, spawn => spawn.DistanceTo(currentPos) > 5);
            return choices[Init.Random.Next(choices.Length)];
        }

        [Command("pickupdeer")]
        public void pickupdeer_cmd(Client player)
        {
            var character = player.GetCharacter();
      
            var tags = InventoryManager.DoesInventoryHaveItem(character, typeof(HuntingTag));
            if (tags.Cast<HuntingTag>().Any(i => i.Type == HuntingManager.AnimalTypes.Deer && i.ValidDate == DateTime.Today.Date))
            {
                foreach (var a in SpawnedAnimals)
                {
                    if (player.position.DistanceTo(API.getEntityPosition(a.handle)) < 2.0)
                    {
                        bool isDead = API.fetchNativeFromPlayer<bool>(player, Hash.IS_PED_DEAD_OR_DYING, a.handle, 1);
                        if (isDead)
                        {
                            var animalItem = new AnimalItem();
                            animalItem.Type = AnimalTypes.Deer;
                            switch (InventoryManager.GiveInventoryItem(character, animalItem, 1, false))
                            {
                                case InventoryManager.GiveItemErrors.Success:
                                    API.sendChatMessageToPlayer(player, Color.White,
                                        "You pickup the Deer carcass from the ground.");
                                    ChatManager.RoleplayMessage(character, "picks up the Deer carcass from the ground.",
                                        ChatManager.RoleplayMe);
                                    a.Respawn();
                                    break;

                                case InventoryManager.GiveItemErrors.NotEnoughSpace:
                                    API.sendChatMessageToPlayer(player, Color.White,
                                        "~r~[ERROR] You do not have enough inventory space for this. (Need " +
                                        animalItem.AmountOfSlots + ")");
                                    break;

                                case InventoryManager.GiveItemErrors.MaxAmountReached:
                                    API.sendChatMessageToPlayer(player, Color.White,
                                        "~r~[ERROR]~w~ You can only carry one of these at a time.");
                                    break;
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, Color.White, "~r~[ERROR]~w~ This dear is not dead!");
                        }
                        return;
                    }
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, Color.White, "~r~[ERROR]~w~ You don't have a valid Deer tag for today.");
            }
        }

        [Command("pickupboar")]
        public void pickupboar_cmd(Client player)
        {
            var character = player.GetCharacter();

            var tags = InventoryManager.DoesInventoryHaveItem(character, typeof(HuntingTag));
            if (tags.Cast<HuntingTag>().Any(i => i.Type == HuntingManager.AnimalTypes.Boar && i.ValidDate == DateTime.Today.Date))
            {
                foreach (var a in SpawnedAnimals)
                {
                    if (player.position.DistanceTo(API.getEntityPosition(a.handle)) < 2.0)
                    {
                        bool isDead = API.fetchNativeFromPlayer<bool>(player, Hash.IS_PED_DEAD_OR_DYING, a.handle, 1);
                        if (isDead)
                        {
                            var animalItem = new AnimalItem();
                            animalItem.Type = AnimalTypes.Boar;
                            switch (InventoryManager.GiveInventoryItem(character, animalItem))
                            {
                                case InventoryManager.GiveItemErrors.Success:
                                    API.sendChatMessageToPlayer(player, Color.White,
                                        "You pickup the Boar carcass from the ground.");
                                    ChatManager.RoleplayMessage(character, "picks up the Boar carcass from the ground.",
                                        ChatManager.RoleplayMe);
                                    a.Respawn();
                                    break;

                                case InventoryManager.GiveItemErrors.NotEnoughSpace:
                                    API.sendChatMessageToPlayer(player, Color.White,
                                        "~r~[ERROR] You do not have enough inventory space for this. (Need " +
                                        animalItem.AmountOfSlots + ")");
                                    break;

                                case InventoryManager.GiveItemErrors.MaxAmountReached:
                                    API.sendChatMessageToPlayer(player, Color.White,
                                        "~r~[ERROR]~w~ You can only carry one of these at a time.");
                                    break;
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(player, Color.White, "~r~[ERROR]~w~ This Boar is not dead!");
                        }
                        return;
                    }
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, Color.White, "~r~[ERROR]~w~ You don't have a valid Boar tag for today.");
            }
        }

        [Command("redeemdeertag")]
        public void redeemdeertag_cmd(Client player)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null || prop?.Type != PropertyManager.PropertyTypes.HuntingStation)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a hunting shop interaction point.");
                return;
            }

            var character = player.GetCharacter();
            if (character.LastRedeemedDeerTag == DateTime.Today.Date)
            {
                API.sendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ You already redeemed a Deer tag today.");
                return;
            }

            var tags = InventoryManager.DoesInventoryHaveItem(character, typeof(HuntingTag));
            if (tags.Cast<HuntingTag>().Any(i => i.Type == HuntingManager.AnimalTypes.Deer && i.ValidDate == DateTime.Today.Date))
            {
                var carcass = InventoryManager.DoesInventoryHaveItem(character, typeof(AnimalItem));
                if (carcass.Cast<AnimalItem>().Any(i => i.Type == AnimalTypes.Deer))
                {

                    InventoryManager.DeleteInventoryItem<AnimalItem>(character, 1, i => i.CommandFriendlyName == "Deer");
                    InventoryManager.DeleteInventoryItem<HuntingTag>(character, 1,
                        i => i.CommandFriendlyName == "Deer_Tag");
                    InventoryManager.GiveInventoryItem(character, new Money(), 2000);
                    character.LastRedeemedDeerTag = DateTime.Today.Date;
                    API.sendChatMessageToPlayer(player, Color.White, "You redeemed your Deer carcass for ~g~$2500!");
                    ChatManager.RoleplayMessage(character, "redeems their Deer carcass.", ChatManager.RoleplayMe);
                }
                else API.sendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ You do not have a Deer to redeem.");
            }
            else API.sendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ You do not have a Deer tag for today.");
        }

        [Command("redeemboartag")]
        public void redeemboartag_cmd(Client player)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null || prop?.Type != PropertyManager.PropertyTypes.HuntingStation)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a hunting shop interaction point.");
                return;
            }

            var character = player.GetCharacter();
            if (character.LastRedeemedBoarTag == DateTime.Today.Date)
            {
                API.sendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ You already redeemed a Boar tag today.");
                return;
            }

            var tags = InventoryManager.DoesInventoryHaveItem(character, typeof(HuntingTag));
            if (tags.Cast<HuntingTag>().Any(i => i.Type == HuntingManager.AnimalTypes.Boar && i.ValidDate == DateTime.Today.Date))
            {
                var carcass = InventoryManager.DoesInventoryHaveItem(character, typeof(AnimalItem));
                if (carcass.Cast<AnimalItem>().Any(i => i.Type == AnimalTypes.Boar))
                {

                    InventoryManager.DeleteInventoryItem<AnimalItem>(character, 1, i => i.CommandFriendlyName == "Boar");
                    InventoryManager.DeleteInventoryItem<HuntingTag>(character, 1,
                        i => i.CommandFriendlyName == "Boar_Tag");
                    InventoryManager.GiveInventoryItem(character, new Money(), 2000);
                    character.LastRedeemedBoarTag = DateTime.Today.Date;
                    API.sendChatMessageToPlayer(player, Color.White, "You redeemed your Boar carcass for ~g~$2500!");
                    ChatManager.RoleplayMessage(character, "redeems their Boar carcass.", ChatManager.RoleplayMe);
                }
                else API.sendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ You do not have a Boar to redeem.");
            }
            else API.sendChatMessageToPlayer(player, Color.White, "~r~ERROR:~w~ You do not have a Boar tag for today.");
        }
    }



    public class HuntingAnimal
    {
        public NetHandle handle;
        public Vector3 Spawn;
        public HuntingManager.AnimalTypes Type;
        public HuntingManager.AnimalState State;

        public Timer StateTimer;
        public int StateChangeTick;
        public Vector3 Destination;
        public bool UpdateState;
        public Client FleeingPed;

        public HuntingAnimal(Vector3 spawn, HuntingManager.AnimalTypes type, HuntingManager.AnimalState state)
        {
            handle = API.shared.createPed((type == HuntingManager.AnimalTypes.Deer) ? (PedHash.Deer) : (PedHash.Boar),
                spawn, 0, 0);
            Spawn = spawn;
            Type = type;
            State = state;

            StateTimer = new Timer()
            {
                Interval = 1000,
                AutoReset = true
            };
            StateTimer.Elapsed += delegate { AnimalAI(this); };
            StateTimer.Start();

            HuntingManager.SpawnedAnimals.Add(this);
        }

        public void AnimalAI(HuntingAnimal animal)
        {
            API.shared.setEntityPositionFrozen(handle, false);

            var playersInRadius = API.shared.getPlayersInRadiusOfPosition(500f, API.shared.getEntityPosition(handle));

            if (playersInRadius.Count > 0)
            {
                API.shared.triggerClientEvent(playersInRadius[0], "update_animal_position", handle);

                var tooClosePlayers = API.shared.getPlayersInRadiusOfPosition(50f, API.shared.getEntityPosition(handle));
                if (tooClosePlayers.Count > 0 && State != HuntingManager.AnimalState.Fleeing)
                {
                    State = HuntingManager.AnimalState.Fleeing;
                    FleeingPed = tooClosePlayers.First();
                    UpdateState = true;
                }

                StateChangeTick++;

                if (State != HuntingManager.AnimalState.Fleeing)
                {
                    if (StateChangeTick > 15)
                    {
                        var nextStateChance = Init.Random.Next(100);
                        if (nextStateChance < 35) // Graze
                        {
                            State = HuntingManager.AnimalState.Grazing;
                            API.shared.playPedScenario(handle,
                                Type == HuntingManager.AnimalTypes.Deer ? "WORLD_DEER_GRAZING" : "WORLD_PIG_GRAZING");
                        }
                        else // Wander
                        {
                            State = HuntingManager.AnimalState.Wandering;
                            Destination = HuntingManager.RandomFarawayDestination(API.shared.getEntityPosition(handle));
                            UpdateState = true;
                        }

                        StateChangeTick = 0;
                    }
                }
                else
                {
                    if (StateChangeTick > 20)
                    {
                        State = HuntingManager.AnimalState.Grazing;
                        API.shared.playPedScenario(handle,
                            Type == HuntingManager.AnimalTypes.Deer ? "WORLD_DEER_GRAZING" : "WORLD_PIG_GRAZING");
                        StateChangeTick = 0;
                    }
                }
            }
            else
            {
                State = HuntingManager.AnimalState.Grazing;
                API.shared.playPedScenario(handle,
                    Type == HuntingManager.AnimalTypes.Deer ? "WORLD_DEER_GRAZING" : "WORLD_PIG_GRAZING");
            }
     
            if (UpdateState != true) return;
            switch (State)
            {
                case HuntingManager.AnimalState.Fleeing:
                    API.shared.sendNativeToAllPlayers(Hash.TASK_SMART_FLEE_PED, handle, FleeingPed.handle, 75f, 5000, 0, 0);
                    break;
                case HuntingManager.AnimalState.Grazing:
                    API.shared.playPedScenario(handle,
                        Type == HuntingManager.AnimalTypes.Deer ? "WORLD_DEER_GRAZING" : "WORLD_PIG_GRAZING");
                    break;
                case HuntingManager.AnimalState.Wandering:
                    API.shared.sendNativeToAllPlayers(Hash.TASK_WANDER_IN_AREA, handle, Destination.X,
                        Destination.Y, Destination.Z, 25, 0, 0);
                    break;

            }
            UpdateState = false;
        }

        public void Respawn()
        {
            API.shared.deleteEntity(handle);
            handle = API.shared.createPed((Type == HuntingManager.AnimalTypes.Deer) ? (PedHash.Deer) : (PedHash.Boar),
                Spawn, 0, 0);
        }
    }
}