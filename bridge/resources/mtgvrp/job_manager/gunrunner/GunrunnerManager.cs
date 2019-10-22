using System;
using System.Collections.Generic;
using System.Timers;
using System.Linq;




using GTANetworkAPI;



using mtgvrp.player_manager;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.phone_manager;
using mtgvrp.core.Help;
using mtgvrp.group_manager;
using mtgvrp.weapon_manager;
using mtgvrp.property_system;

using Newtonsoft.Json;
using mtgvrp.database_manager;
using MongoDB.Driver;

/* GUN RUNNING ACTIVITY
 * ALEXANDER NORTON
 * MTGVRP
 */

namespace mtgvrp.job_manager.gunrunner
{
    class GunrunnerManager : Script
    {
        #region Class Variables

        private const int RenownForHQ = 500;
        private const int RenownForUpgradeHQ = 800;
        private const int RenownT1 = 100;
        private const int RenownT2 = 300;
        private const int RenownT3 = 800;
        private const int RenownT4 = 1100;
        private const int RenownT5 = 2000;
        private const int RenownLossDrop = 10;
        private const int RenownTimeLimitLoss = 5;
        private const int RenownForgetLoss = 5;


        public static Ped CurrentDealer = null;
        public static TextLabel DealerLabel = null;
        public static TextLabel DealerNameLabel = null;
        public static string CurrentStreet = String.Empty;
        public static string CurrentZone = String.Empty;
        public static int FlatRate = 200;
        public static List<NetHandle> WeaponCases = new List<NetHandle>();
        public Timer MoveDealerTimer = new Timer();
        public Timer ZoneTimer = new Timer();

        public static List<Container> Containers = new List<Container>();

        #region Weapons/prices
        //WEAPON
        private readonly string[][] Weapon_Melee =
        {
            new[] {"Bat", "300"},
            new[] {"Crowbar", "320"},
            new[] {"KnuckleDuster", "270"},
        };

        private readonly string[][] Weapon_Pistols =
        {
            new[] {"Pistol", "3000"},
            new[] {"CombatPistol", "4200"},
            new[] {"Pistol50", "6000"},
            new[] {"Revolver", "8200"},
            new[] {"FlareGun", "5000"},
        };

        private readonly string[][] Weapon_Shotguns =
        {
            new[] {"PumpShotgun", "7000"},
            new[] {"SawnoffShotgun", "7500"},
            new[] {"DoubleBarrelShotgun", "8300"},
            new[] {"AssaultShotgun", "9600"},
        };

        private readonly string[][] Weapon_Machineguns =
        {
            new[] {"MicroSMG", "9700"},
            new[] {"SMG", "10100"},
            new[] {"Gusenberg", "13100"},
            new[] {"CombatPDW", "11000"},
            new[] {"MG", "16000"},
        };

        private readonly string[][] Weapon_Assaultrifles =
        {
            new[] {"Armor", "3000"},
            new[] {"AssaultRifle", "14000"},
            new[] {"CarbineRifle", "14700"},
            new[] {"CompactRifle", "17500"},
            new[] {"SpecialCarbine", "17000"},
        };

        private readonly string[][] Weapon_Snipers =
        {
            new[] {"SniperRifle", "20000"},
            new[] {"HeavySniper", "25000"},
            new[] {"MarksmanRifle", "30000"},
        };

        #endregion

        public static List<Vector3> DealerLocations = new List<Vector3>()
        {
            new Vector3(413.1686, -980.6628, 29.43213),
        };

        #endregion

        #region Class Functions

        public GunrunnerManager()
        {
            CharacterMenu.OnCharacterLogin += CharacterMenu_OnCharacterLogin;

            //Load all containers.
            Containers = DatabaseManager.ContainersTable.Find(FilterDefinition<Container>.Empty).ToList();

            //Move the dealer every 10 hours
            MoveDealerTimer.Interval = TimeSpan.FromHours(10).TotalMilliseconds;
            MoveDealerTimer.Elapsed += MoveDealerTimer_Elapsed;
            MoveDealerTimer.Start();
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            #region Mapping
            API.CreateObject(1670285818, new Vector3(-194.2142f, -739.0701f, 14.61313f), new Vector3(0f, 0f, 0f));
            API.CreateObject(1437126442, new Vector3(-195.51f, -745.1301f, 16.07001f), new Vector3(0f, 0f, 0f));
            API.CreateObject(519594446, new Vector3(-192.9201f, -745.1301f, 16.07001f), new Vector3(0f, 0f, 0f));
            API.CreateObject(-1567006928, new Vector3(-194.25f, -733.72f, 14.81f), new Vector3(0f, 0f, 0f));
            API.CreateObject(-1326449699, new Vector3(-195.11f, -734.58f, 14.8101f), new Vector3(1.001791E-05f, 5.008957E-06f, 89.99955f));
            API.CreateObject(-483631019, new Vector3(-195.11f, -736.6801f, 14.8101f), new Vector3(1.00179E-05f, 5.008956E-06f, 89.99959f));
            API.CreateObject(-483631019, new Vector3(-195.11f, -738.3901f, 14.8101f), new Vector3(1.00179E-05f, 5.008956E-06f, 89.99959f));
            API.CreateObject(-1326449699, new Vector3(-195.11f, -740.48f, 14.8101f), new Vector3(1.001791E-05f, 5.008957E-06f, 89.99955f));
            API.CreateObject(1140820728, new Vector3(-194.2499f, -733.5973f, 15.72415f), new Vector3(0f, 0f, 22.00001f));
            API.CreateObject(1338930512, new Vector3(-194.1519f, -733.9401f, 15.72999f), new Vector3(0f, 0f, 14.99997f));
            API.CreateObject(-286280212, new Vector3(-193.7473f, -733.8964f, 15.72f), new Vector3(0f, 0f, -9.000026f));
            API.CreateObject(-1837476061, new Vector3(-195.2168f, -743.4985f, 14.81151f), new Vector3(1.00179E-05f, -5.008955E-06f, 133.749f));
            API.CreateObject(1885839156, new Vector3(-195.0601f, -742.5001f, 14.81f), new Vector3(1.00179E-05f, 5.008957E-06f, 89.99943f));
            API.CreateObject(305924745, new Vector3(-194.091f, -736.0001f, 17.25348f), new Vector3(1.00179E-05f, 5.008958E-06f, 89.99934f));
            API.CreateObject(305924745, new Vector3(-194.091f, -739.0001f, 17.25348f), new Vector3(1.00179E-05f, 5.008958E-06f, 89.99934f));
            API.CreateObject(305924745, new Vector3(-194.091f, -742.0001f, 17.25348f), new Vector3(1.00179E-05f, 5.008958E-06f, 89.99934f));
            API.CreateObject(1609935604, new Vector3(-192.95f, -735.15f, 16.12f), new Vector3(1.001791E-05f, 5.008958E-06f, 89.99973f));
            API.CreateObject(867556671, new Vector3(-194.0455f, -734.5662f, 14.81151f), new Vector3(0f, 0f, -172.0002f));
            API.CreateObject(148141454, new Vector3(-194.64f, -733.9501f, 15.75f), new Vector3(1.00179E-05f, 5.008955E-06f, 24.50006f));
            API.CreateObject(-1677239828, new Vector3(-195.28f, -737.5001f, 15.7223f), new Vector3(0f, 0f, 89.99936f));
            API.CreateObject(-1774732668, new Vector3(-195.19f, -738.29f, 15.86f), new Vector3(1.001786E-05f, 5.008954E-06f, 89.99914f));
            API.CreateObject(-1519426, new Vector3(-195.1801f, -736.8f, 15.7223f), new Vector3(0f, 0f, 89.99963f));
            API.CreateObject(1234788901, new Vector3(-195.06f, -735.74f, 15.92f), new Vector3(1.001791E-05f, -5.008954E-06f, 104.2494f));
            API.CreateObject(-1831680671, new Vector3(-195.41f, -739.09f, 16.62f), new Vector3(1.001789E-05f, 5.008953E-06f, -89.99973f));
            API.CreateObject(-2117059320, new Vector3(-192.95f, -737.72f, 16.30798f), new Vector3(1.001791E-05f, 5.008957E-06f, -89.99963f));
            API.CreateObject(1915724430, new Vector3(-195.06f, -744.9f, 14.81151f), new Vector3(1.001791E-05f, 5.008956E-06f, -89.99969f));
            API.CreateObject(-836132965, new Vector3(-194.97f, -740.34f, 15.72f), new Vector3(1.00179E-05f, 5.008956E-06f, -89.99969f));
            API.CreateObject(-483631019, new Vector3(-194.83f, -740.87f, 14.8101f), new Vector3(1.001784E-05f, 5.008952E-06f, -90.00054f));
            API.CreateObject(-483631019, new Vector3(-194.8301f, -739.81f, 14.81f), new Vector3(1.001784E-05f, 5.008952E-06f, -90.00054f));
            API.CreateObject(-671139745, new Vector3(-193.37f, -733.8f, 15.75f), new Vector3(1.00179E-05f, 5.008956E-06f, -96.5f));
            API.CreateObject(-483631019, new Vector3(-195.289f, -741.33f, 14.8101f), new Vector3(1.001779E-05f, 5.008955E-06f, 179.9996f));
            API.CreateObject(-483631019, new Vector3(-195.31f, -739.351f, 14.81f), new Vector3(1.001781E-05f, 5.008956E-06f, -0.0005550665f));
            API.CreateObject(-455361602, new Vector3(-192.95f, -743.87f, 16.21f), new Vector3(1.001784E-05f, 5.008955E-06f, -179.999f));
            API.CreateObject(-132789682, new Vector3(-193.42f, -733.24f, 15.84f), new Vector3(1.001787E-05f, 5.008954E-06f, -89.99982f));
            API.CreateObject(-1543942490, new Vector3(-194.8968f, -734.1437f, 15.72411f), new Vector3(0f, 0f, -158.0002f));
            API.CreateObject(1915724430, new Vector3(-193.52f, -744.9f, 14.81151f), new Vector3(1.001791E-05f, 5.008956E-06f, -89.99963f));
            #endregion
        }

        private void CharacterMenu_OnCharacterLogin(object sender, CharacterMenu.CharacterLoginEventArgs e)
        {
            var cnt = Containers.FirstOrDefault(x => x.OwnerId == e.Character.Id);
            if (cnt != null)
                e.Character.Container = cnt;
        }

        [RemoteEvent("update_location")]
        public void UpdateLocation(Client player, params object[] arguments)
        {
            CurrentStreet = (string)arguments[0];
            CurrentZone = (string)arguments[1];
        }

        [RemoteEvent("gunrun_menu_closed")]
        public void GunRunMenuClosed(Client player, params object[] arguments)
        {
            if ((int)arguments[0] != 0)
            {

                dynamic weapons = JsonConvert.DeserializeObject((string)arguments[1]);
                if (Money.GetCharacterMoney(player.GetCharacter()) < (int)arguments[0])
                {
                    player.SendChatMessage("Yuri Orlov says: Not enough money? Come back when you have enough.");
                    return;
                }

                InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(Money), FlatRate + (int)arguments[0]);

                foreach (var w in weapons)
                {
                    string weapon = w.ToString().Trim();

                    switch (InventoryManager.GiveInventoryItem(player.GetCharacter(), new WeaponCase(API.WeaponNameToModel(weapon), player.GetCharacter())))
                    {
                        case InventoryManager.GiveItemErrors.Success:
                            player.GetCharacter().WeaponsBought++;
                            break;

                        case InventoryManager.GiveItemErrors.NotEnoughSpace:
                            NAPI.Chat.SendChatMessageToPlayer(player,
                                $"[BUSINESS] You dont have enough space for that item.");
                            break;
                    }
                }
                ChatManager.RoleplayMessage(player.GetCharacter(), "has bought some weapons from Yuri_Orlov.", ChatManager.RoleplayMe);
                player.SendChatMessage("~r~You have bought some weapons. Sell them within the next 24 hours or risk losing renown!");
                player.SendChatMessage("Yuri Orlov says: Keep those safe, sell them soon and I'll have more for you when you're done.");
                player.GetCharacter().WeaponSellTimeLimit = TimeManager.GetTimeStampPlus(TimeSpan.FromHours(24));
                if (!player.GetCharacter().IsGunrunner)
                {
                    player.GetCharacter().IsGunrunner = true;
                    player.SendNotification("You have become a gunrunner. Earn renown by selling weapons to other players.", true);
                    player.SendChatMessage("~g~You have become a gunrunner. Earn renown by selling weapons to other players. You can deploy a headquarters once you've reached 500 renown.");
                }
            }
            else
            {
                player.SendChatMessage("Yuri Orlov says: Not buying? No problem. Come back when you want some selling to do.");
            }
        }

        [RemoteEvent("fetch_weapon_list")]
        public void FetchWeaponList(Client player, params object[] arguments)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

            if (character == null || account == null)
                return;

            int renown = character.Renown;
            NAPI.ClientEvent.TriggerClientEvent(player, "send_renown", renown);
            if (renown < 100) { NAPI.ClientEvent.TriggerClientEvent(player, "send_melee_list", NAPI.Util.ToJson(Weapon_Melee)); }
            if (renown < 300 && renown >= 100) { NAPI.ClientEvent.TriggerClientEvent(player, "send_pistol_list", NAPI.Util.ToJson(Weapon_Melee), NAPI.Util.ToJson(Weapon_Pistols)); }
            if (renown < 800 && renown >= 300) { NAPI.ClientEvent.TriggerClientEvent(player, "send_shotgun_list", NAPI.Util.ToJson(Weapon_Melee), NAPI.Util.ToJson(Weapon_Pistols), NAPI.Util.ToJson(Weapon_Shotguns)); }
            if (renown < 1100 && renown >= 800) { NAPI.ClientEvent.TriggerClientEvent(player, "send_machinegun_list", NAPI.Util.ToJson(Weapon_Melee), NAPI.Util.ToJson(Weapon_Pistols), NAPI.Util.ToJson(Weapon_Shotguns), NAPI.Util.ToJson(Weapon_Machineguns)); }
            if (renown < 2000 && renown >= 1100) { NAPI.ClientEvent.TriggerClientEvent(player, "send_assaultrifle_list", NAPI.Util.ToJson(Weapon_Melee), NAPI.Util.ToJson(Weapon_Pistols), NAPI.Util.ToJson(Weapon_Shotguns), NAPI.Util.ToJson(Weapon_Machineguns), NAPI.Util.ToJson(Weapon_Assaultrifles)); }
            if (renown >= 2000) { NAPI.ClientEvent.TriggerClientEvent(player, "send_sniper_list", NAPI.Util.ToJson(Weapon_Melee), NAPI.Util.ToJson(Weapon_Pistols), NAPI.Util.ToJson(Weapon_Shotguns), NAPI.Util.ToJson(Weapon_Machineguns), NAPI.Util.ToJson(Weapon_Assaultrifles), NAPI.Util.ToJson(Weapon_Snipers)); }
        }

        [RemoteEvent("CONTAINER_PLACED")]
        public void ContainerPlaced(Client player, params object[] arguments)
        {
            var obj = (NetHandle)arguments[0];
            var c = player.GetCharacter();
            c.Container.Position = NAPI.Entity.GetEntityPosition(obj);
            c.Container.Rotation = API.GetEntityRotation(obj);
            c.Container.Save();
        }

        public static bool IsInContainerZone(Client player)
        {
            foreach (var c in ContainerZone.GetAllContainerZones())
            {
                if (c.Position.DistanceTo(player.Position) <= c.Radius)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsOverlapping(Vector3 position, Vector3 rotation)
        {
            foreach (var c in Containers)
            {
                if (position.DistanceTo(c.Position) < 10f)
                {
                    return true;
                }
            }
            return false;
        }

        public static void load_all_containers()
        {
            foreach (var c in Containers)
            {
                c.Deploy();
            }
        }

        public static void load_all_container_zones()
        {
            foreach (var c in ContainerZone.GetAllContainerZones())
            {
                c.Deploy();
            }
        }

        private void DeleteZoneMarker(Client player)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "delete_zone_marker");
            ZoneTimer.Stop();
        }

        private void MoveDealerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            MoveDealer();
        }

        public static void SendChatToAllRunners(string message)
        {
            foreach (var p in PlayerManager.Players)
            {
                if (!p.IsGunrunner) { return; }
                API.Shared.SendChatMessageToPlayer(p.Client, $"{message}");
            }
        }

        public static void SendTextToAllRunners(string message)
        {
            foreach (var p in PlayerManager.Players)
            {
                if (p.IsGunrunner)
                {
                    SendTextToRunner(p.Client, $"{message}");
                }
            }
        }

        public static void SendTextToRunner(Client player, string message)
        {
            var p = player.GetCharacter();
            var playerPhone = InventoryManager.DoesInventoryHaveItem<Phone>(p);

            if (playerPhone.Length == 0) { return; }
            if (playerPhone[0].IsOn)
            {
                Phone.LogMessage("ANONYMOUS", playerPhone[0].PhoneNumber, message);
                API.Shared.SendChatMessageToPlayer(player, core.Color.Sms, "You've received an SMS.");
            }
        }

        public static void CreateMovingMessage(Client player, Vector3 newLoc)
        {
            var p = player.GetCharacter();

            API.Shared.TriggerClientEvent(player, "get_street_name", newLoc);

            if (!p.IsGunrunner /*&& p.GetPlayingHours() >= 4*/)
            {
                SendTextToRunner(player,
                $"Interested in the weapon dealing business? Meet me at '{CurrentZone}, {CurrentStreet}' and we can get " +
                $"started. There's big money to be made.. -Orlov'");
            }
            else if (p.IsGunrunner)
            {
                SendTextToRunner(player, $"I have more weapons for you. Meet me at '{CurrentZone}, {CurrentStreet}'. -Orlov");
            }
        }

        public static void MoveDealer()
        {
            Random rand = new Random();
            int r = rand.Next(DealerLocations.Count);

            if (CurrentDealer != null) { API.Shared.DeleteEntity(CurrentDealer); }
            if (DealerLabel != null) { DealerLabel.Delete(); }
            if (DealerNameLabel != null) { DealerNameLabel.Delete(); }
            foreach(var o in WeaponCases) { API.Shared.DeleteEntity(o); }

            DealerLabel = API.Shared.CreateTextLabel("~g~/gunrun\n/intervene", DealerLocations[r], 25f, 1f, 1, new GTANetworkAPI.Color(1,1,1), true);
            DealerNameLabel = API.Shared.CreateTextLabel("Yuri_Orlov", DealerLocations[r] + new Vector3(0, 0, 1f), 25f, 0.5f, 1, new GTANetworkAPI.Color(1, 1, 1), true);
            CurrentDealer = API.Shared.CreatePed(PedHash.RoccoPelosi, DealerLocations[r], 180);
            NetHandle WeaponCase = API.Shared.CreateObject((int)API.Shared.GetHashKey("prop_gun_case_01"), 
                DealerLocations[r] - new Vector3(-1f, 0, 1f), new Vector3(0, 0, 180));
            WeaponCases.Add(WeaponCase);
            WeaponCase = API.Shared.CreateObject((int)API.Shared.GetHashKey("prop_idol_case_02"),
                DealerLocations[r] - new Vector3(0.7f, 0, 1f), new Vector3(0, 0, 180));
            WeaponCases.Add(WeaponCase);

            foreach (var p in PlayerManager.Players)
            {
                CreateMovingMessage(p.Client, DealerLocations[r]);
            }
        }
        #endregion

        #region Player Commands
        [Command("gunrun"), Help(HelpManager.CommandGroups.JobsGeneral, "Start a gun run.", null)]
        public void gunrun_cmd(Client player)
        {
            var character = player.GetCharacter();
            var account = player.GetAccount();

            if (character.Group?.CommandType == Group.CommandTypeLspd)
            {
                player.SendChatMessage("Members of the LSPD cannot use this command.");
                return;
            }

            if (player.Position.DistanceTo(CurrentDealer.Position) > 5f)
            {
                player.SendChatMessage("You must be at a weapon dealer location.");
                return;
            }
            /*
            if (character.GetPlayingHours() < 4)
            {
                player.SendChatMessage("You must have 4 playing hours to do this.");
                return;
            }
            */
            if (character.WeaponsBought != character.WeaponsSold)
            {
                player.SendChatMessage("Yuri Orlov says: You haven't sold all your weapons! Come back when you've sold all of them.");
                return;
            }

            if (character.WeaponSellTimeLimit < TimeManager.GetTimeStamp && character.WeaponsBought > 0)
            {
                player.SendChatMessage($"Yuri Orlov says: You're late! Late deliveries disrupt my business. Be quicker next time..");
                player.SendChatMessage($"~r~You've lost 5 renown for not selling your weapons within 24 hours.");
                character.Renown -= RenownTimeLimitLoss;
            }

            player.SendChatMessage($"Buy weapons from Yuri to be sold on to other players using /give. ~r~You will be charged a flat rate of ${FlatRate}.");
            character.WeaponsBought = 0;
            character.WeaponsSold = 0;
            NAPI.ClientEvent.TriggerClientEvent(player, "open_gunrun_menu");
        }

        [Command("gunrunstats"), Help(HelpManager.CommandGroups.JobsGeneral, "List your gunrunnning statistics.", null)]
        public void gunrunstats_cmd(Client player)
        {
            var character = player.GetCharacter();

            if (!character.IsGunrunner)
            {
                player.SendChatMessage("You are not a gunrunner! Find the gun dealer to become one.");
                return;
            }

            player.SendChatMessage("===========================");
            player.SendChatMessage("GUNRUNNING STATS");
            player.SendChatMessage("===========================");
            player.SendChatMessage($"~g~Weapons sold:~w~ {character.TotalWeaponsSold}");
            player.SendChatMessage($"~g~Weapons last bought:~w~ {character.WeaponsBought}");
            player.SendChatMessage($"~g~Weapons last sold:~w~ {character.WeaponsSold}");
            player.SendChatMessage($"~g~Renown:~w~ {character.Renown}");
        }

        [Command("forgetallweapons"), Help(HelpManager.CommandGroups.JobsGeneral, "Forget all of your gunrunning weapons if you've somehow lost them.", null)]
        public void forgetallweapons_cmd(Client player)
        {
            var character = player.GetCharacter();
            
            if (!character.IsGunrunner)
            {
                player.SendChatMessage("You must be a gunrunner to use this command.");
                return;
            }
            
            if (player.Position.DistanceTo(CurrentDealer.Position) > 5f)
            {
                player.SendChatMessage("You must be at a weapon dealer location.");
                return;
            }

            if (character.WeaponsBought == 0)
            {
                player.SendChatMessage("You have no weapons to forget.");
                return;
            }

            player.SendChatMessage($"~r~You've lost {character.WeaponsBought - character.WeaponsSold * 3} renown for losing {character.WeaponsBought - character.WeaponsSold} weapons.");
            player.SendChatMessage("Yuri_Orlov says: You've lost me a lot of business!");
            character.Renown -= character.WeaponsBought - character.WeaponsSold * RenownForgetLoss;
            character.WeaponsSold = 0;
            character.WeaponsBought = 0;
        }

        [Command("intervene"), Help(HelpManager.CommandGroups.JobsGeneral, "Intervene with the top gunrunner by finding their current location.", null)]
        public void intervene_cmd(Client player)
        {
            var character = player.GetCharacter();
            
            if (!character.IsGunrunner)
            {
                player.SendChatMessage("You must be a gunrunner to do this.");
                return;
            }
            
            if (character.Container == null)
            {
                player.SendChatMessage("You must own a gunrunning HQ to do this.");
                return;
            }

            if (!character.Container.CanIntervene)
            {
                player.SendChatMessage("Your HQ must be upgraded before you can use this ability.");
                return;
            }

            if (player.Position.DistanceTo(CurrentDealer.Position) > 5f)
            {
                player.SendChatMessage("You must be at the gun dealer to use this ability.");
            }

            if (character.InterveneTimeLimit > TimeManager.GetTimeStamp)
            {
                player.SendChatMessage($@"You can only do this every 12 hours. Please wait {TimeManager.SecondsToMinutes(character.InterveneTimeLimit - TimeManager.GetTimeStamp)} more minutes before doing this again.");
                return;
            }

            Character MostRenown = character;
            foreach (var p in PlayerManager.Players)
            {
                if (p.IsGunrunner && p.Renown > MostRenown.Renown)
                {
                    MostRenown = p;
                }
            }

            if (MostRenown == character)
            {
                player.SendChatMessage("You have the most renown and cannot use this ability right now.");
            }
            else
            {
                player.SendChatMessage($"Yuri_Orlov says: {MostRenown.CharacterName} is doing quite well for themselves. Here's where they are..");
                player.SendChatMessage("A waypoint has been set to the position of the current gun dealer with the most renown.");
                NAPI.ClientEvent.TriggerClientEvent(player, "intervene_track_player", MostRenown.Client.Position);
                character.InterveneTimeLimit = TimeManager.GetTimeStampPlus(TimeSpan.FromHours(12));
            }
        }

        [Command("upgradehq"), Help(HelpManager.CommandGroups.JobsGeneral, "Upgrade your HQ.", new[] { "Option: 'dealertracker', 'movelocation', 'intervene'" })]
        public void upgradehq_cmd(Client player, string option)
        {
            var character = player.GetCharacter();

            if (character.Container == null)
            {
                player.SendChatMessage("You must own a gunrunning HQ to do this.");
                return;
            }

            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null || prop?.Type != PropertyManager.PropertyTypes.Container || prop.OwnerId != character.Id)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at a gunrunning headquarters or you are not the owner of these headquarters.");
                return;
            }

            if (character.Renown < character.Container.RenownToNextUpgrade)
            {
                player.SendChatMessage($"You need {character.Container.RenownToNextUpgrade - character.Renown} more renown for your next HQ upgrade.");
                return;
            }

            if (option != "dealertracker" && option != "movelocation" && option != "intervene")
            {
                player.SendChatMessage("Incorrect option. Choose: 'dealertracker' 'movelocation' intervene'.");
                return;
            }

            switch (option)
            {
                case "dealertracker":
                    player.SendChatMessage("~g~You've upgraded your HQ!~w~ You can now find the exact location of the weapon dealer with /trackdealer.");
                    character.Container.CanTrackLocation = true;
                    character.Container.RenownToNextUpgrade += RenownForUpgradeHQ;
                    break;

                case "movelocation":
                    player.SendChatMessage("~g~You've upgraded your HQ!~w~ You can now move your HQ to a new location with /movehq.");
                    character.Container.CanBeMoved = true;
                    character.Container.RenownToNextUpgrade += RenownForUpgradeHQ;
                    break;

                case "intervene":
                    player.SendChatMessage("~g~You've upgraded your HQ!~w~ You can now speak to the gun dealer to gain information about other gunrunners.");
                    character.Container.CanIntervene = true;
                    character.Container.RenownToNextUpgrade += RenownForUpgradeHQ;
                    break;
            }
        }

        [Command("trackdealer"), Help(HelpManager.CommandGroups.JobsGeneral, "Track the current location of Yuri_Orlov, the gun dealer.", null)]
        public void trackdealer_cmd(Client player)
        {
            var character = player.GetCharacter();

            if (character.Container == null)
            {
                player.SendChatMessage("You must own a gunrunning HQ to do this.");
                return;
            }

            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null || prop?.Type != PropertyManager.PropertyTypes.Container || prop.OwnerId != character.Id)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at a gunrunning headquarters or you are not the owner of these headquarters.");
                return;
            }

            if (!character.Container.CanTrackLocation)
            {
                player.SendChatMessage("Your HQ must be upgraded to use this ability.");
                return;
            }

            player.SendChatMessage("Weapon dealer location tracked. A waypoint has been set on your map.");
            NAPI.ClientEvent.TriggerClientEvent(player, "track_weapon_dealer", CurrentDealer.Position.X, CurrentDealer.Position.Y);
        }

        [Command("movehq"), Help(HelpManager.CommandGroups.JobsGeneral, "Move your headquarters to your current location.", null)]
        public void movehq_cmd(Client player)
        {
            var character = player.GetCharacter();

            if (character.Container == null)
            {
                player.SendChatMessage("You must own a gunrunning HQ to do this.");
                return;
            }

            if (!character.Container.CanBeMoved)
            {
                player.SendChatMessage("Your HQ must be upgraded to use this ability.");
                return;
            }

            if (!IsInContainerZone(player))
            {
                player.SendChatMessage("You must be inside a container zone to place a container.");
                return;
            }

            if (IsOverlapping(player.Position, player.Rotation))
            {
                player.SendChatMessage("You can't place a container too close to another container.");
                return;
            }

            character.Container.Position = player.Position;
            character.Container.Rotation = player.Rotation;
            character.Container.Respawn();
            character.Container.Save();
            NAPI.Entity.SetEntityPosition(player, player.Position + new Vector3(0, 0, 5));
            NAPI.ClientEvent.TriggerClientEvent(player, "PLACE_OBJECT_ON_GROUND_PROPERLY", character.Container.ContainerObject, "CONTAINER_PLACED");
            player.SendChatMessage("Container moved.");
        }

        [Command("deployhq"), Help(HelpManager.CommandGroups.JobsGeneral, "Place the gunrunning container HQ.")]
        public void deployhq_cmd(Client player)
        {
            var character = player.GetCharacter();
            
            if (!character.IsGunrunner)
            {
                player.SendChatMessage("You must be a gunrunner to do this.");
                return;
            }
            
            if (character.Container != null)
            {
                player.SendChatMessage("You already own a HQ.");
                return;
            }

            if (character.Renown < RenownForHQ)
            {
                player.SendChatMessage("You don't have enough renown to do this.");
                return;
            }

            if (!IsInContainerZone(player))
            {
                player.SendChatMessage("You must be inside a container zone to place a container.");
                return;
            }

            if (IsOverlapping(player.Position, player.Rotation))
            {
                player.SendChatMessage("You can't place a container too close to another container.");
                return;
            }

            character.Container = new Container(character, player.Position, player.Rotation);
            character.Container.Deploy();
            character.Container.Insert();
            NAPI.Entity.SetEntityPosition(player, player.Position + new Vector3(0, 0, 5));
            NAPI.ClientEvent.TriggerClientEvent(player, "PLACE_OBJECT_ON_GROUND_PROPERLY", character.Container.ContainerObject, "CONTAINER_PLACED");
            player.SendChatMessage("Container placed. You now have your very own gun running headquarters.");
        }


        [Command("openweaponcase"), Help(HelpManager.CommandGroups.JobsGeneral, "Open a weapon case. NOTE: Opening your own weapon case will cause you to lose renown.",
            new[] { "The /inv command name of the weapon case you want to open." })]
        public void openweaponcase_cmd(Client player, string weaponcase)
        {
            var character = player.GetCharacter();
            WeaponCase item;

            if (weaponcase.ToLower().Equals("armor") || weaponcase.ToLower().Equals("armour"))
            {
                // Needed to apply armour.
                weaponcase = "0";
            }

            var sendersItem = InventoryManager.DoesInventoryHaveItem(character, weaponcase);
            if (sendersItem.GetType() != typeof(WeaponCase)) return;
            item = (WeaponCase)sendersItem[0];
            if (character.IsGunrunner && item.OwnerId == character.Id)
            {
                InventoryManager.DeleteInventoryItem<WeaponCase>(player.GetCharacter(), 1, x => x.CommandFriendlyName == weaponcase);
                // TODO : VERY DIRTY FIX - PATCH THIS OUT LATER!
                if (item.WeaponHash == 0)
                {
                    API.SetPlayerArmor(player, 100);
                    character.Renown -= RenownLossDrop;
                    character.WeaponsSold += 1;
                    player.SendChatMessage("You've opened your own weapon case as a gunrunner. You have lost some renown for doing so.");
                    return;
                }
                WeaponManager.CreateWeapon(player, item.WeaponHash, WeaponTint.Normal, true);
                character.Renown -= RenownLossDrop;
                character.WeaponsSold += 1; 
                player.SendChatMessage("You've opened your own weapon case as a gunrunner. You have lost some renown for doing so.");
                return;
            }

            InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(WeaponCase), 1);
            if (item.WeaponHash == 0)
            {
                API.SetPlayerArmor(player, 100);
                player.SendChatMessage($"Weapon case opened. The armour has been applied.");

                return;
            }
            WeaponManager.CreateWeapon(player, item.WeaponHash, WeaponTint.Normal, true);
            player.SendChatMessage($"Weapon case opened. The weapon has been added to your inventory.");
        }

        /*
       [Command("sellweapon"), Help(HelpManager.CommandGroups.JobsGeneral, "Start a gun run.", new[] { "Target player.", "The weapon case to sell", "Amount being sold for" })]
       public void sellcase_cmd(Client player, string target, string weaponcase, string price)
       {
           var character = player.GetCharacter();
           var account = player.GetAccount();

           var receiver = PlayerManager.ParseClient(target);

           if (receiver == null) { return; }

           var sendersItem = InventoryManager.DoesInventoryHaveItem(character, weaponcase);
           if (sendersItem.Length != 1 || sendersItem[0].Amount < 1 || 1 <= 0)
           {
               NAPI.Notification.SendNotificationToPlayer(player, "You don't have that weapon.");
               return;
           }

           if (player == receiver)
           {
               player.SendChatMessage("You can't sell weapons to yourself.");
               return;
           }

           if (character.Group.CommandType == Group.CommandTypeLspd)
           {
               player.SendChatMessage("Members of the LSPD cannot use this command.");
               return;
           }

           if (player.position.DistanceTo(receiver.position) > 5f)
           {
               player.SendChatMessage("You are too far away from the target player.");
               return;
           }

           if (character.GetPlayingHours() < 4)
           {
               player.SendChatMessage("You must have 4 playing hours to do this.");
               return;
           }

           player.SendChatMessage($"You have offered to sell a {weaponcase} to {receiver.GetCharacter().CharacterName} for ${price}");
           receiver.SendChatMessage($"{character.CharacterName} has offered to sell you a {weaponcase} for ${price}. /acceptweapon to accept.");
           NAPI.Data.SetEntityData(receiver, "CAN_ACCEPT_WEAPON", character);
           NAPI.Data.SetEntityData(receiver, "ACCEPT_WEAPON_PRICE", price);
           NAPI.Data.SetEntityData(receiver, "WEAPON_BEING_GIVEN", sendersItem);
       }

       [Command("acceptweapon"), Help(HelpManager.CommandGroups.JobsGeneral, "Start a gun run.")]
       public void acceptweapon_cmd(Client player)
       {
           var character = player.GetCharacter();
           var account = player.GetAccount();

           var senderchar = NAPI.Data.GetEntityData(player, "CAN_ACCEPT_WEAPON");
           var price = NAPI.Data.GetEntityData(player, "ACCEPT_WEAPON_PRICE");
           var weapon = NAPI.Data.GetEntityData(player, "WEAPON_BEING_GIVEN");

           if (senderchar == null)
           {
               player.SendChatMessage("There are no weapons to accept.");
               return;
           }

           if (character.Group.CommandType == Group.CommandTypeLspd)
           {
               player.SendChatMessage("Members of the LSPD cannot use this command.");
               return;
           }

           if (player.position.DistanceTo(senderchar.position) > 5f)
           {
               player.SendChatMessage("You are too far away from the target player.");
               return;
           }

           if (character.GetPlayingHours() < 4)
           {
               player.SendChatMessage("You must have 4 playing hours to do this.");
               return;
           }

           if (Money.GetCharacterMoney(player.GetCharacter()) < price)
           {
               NAPI.Chat.SendChatMessageToPlayer(player, "You don't have enough money.");
               return;
           }

           WeaponManager.CreateWeapon(player, weapon[0].WeaponHash, WeaponTint.Normal, true);
           InventoryManager.DeleteInventoryItem(senderchar, weapon[0].GetType());
           InventoryManager.DeleteInventoryItem(character, typeof(Money), price);

           player.SendChatMessage($"You have bought a weapon from {senderchar.CharacterName}.");
           player.SendChatMessage($"You have sold a weapon to {character.CharacterName}.");

           senderchar.Renown = senderchar.Renown + 3;
       }
       */
        #endregion

        #region Admin Commands
        [Command("removehq"), Help(HelpManager.CommandGroups.AdminLevel5, "Remove a players gunrunning HQ.", new[] { "Target player." })]
        public void removehq_cmd(Client player, string target)
        {
            var account = player.GetAccount();

            if (account.AdminLevel < 5)
            {
                player.SendChatMessage("You do not have permission to do this.");
                return;
            }

            var receiver = PlayerManager.ParseClient(target);

            if (receiver == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "That player is not connected.");
                return;
            }

            Character receiverChar = receiver.GetCharacter();
            receiverChar.Container.Remove();
            receiverChar.Container = null;
            player.SendChatMessage("Container removed.");
        }

        [Command("createcontainerzone"), Help(HelpManager.CommandGroups.AdminLevel5, "Create a container zone where containers are permitted to be placed.", null)]
        public void createcontainerzone_cmd(Client player)
        {
            var account = player.GetAccount();

            if (account.AdminLevel < 5)
            {
                player.SendChatMessage("You do not have permission to do this.");
                return;
            }

            var containerZone = new ContainerZone(player.Position, player.Rotation);
            containerZone.Insert();
            containerZone.Deploy();
            NAPI.ClientEvent.TriggerClientEvent(player, "create_zone_marker", player.Position, 30);
            player.SendChatMessage("Container zone created. The radius is displayed for 2 seconds. Use /editcontainerradius to change it.");
            ZoneTimer = new Timer { Interval = 2000 };
            ZoneTimer.Elapsed += delegate { DeleteZoneMarker(player); };
            ZoneTimer.Start();
        }

        [Command("editcontainerradius"), Help(HelpManager.CommandGroups.AdminLevel5, "Edit a container zone radius", new[] { "Radius to be set." })]
        public void editcontainerradius_cmd(Client player, string containerid, string radius)
        {
            var account = player.GetAccount();

            if (account.AdminLevel < 5)
            {
                player.SendChatMessage("You do not have permission to do this.");
                return;
            }

            var containerZone = ContainerZone.GetAllContainerZones()[int.Parse(containerid)];
            containerZone.Radius = int.Parse(radius);
            containerZone.Save();
            NAPI.ClientEvent.TriggerClientEvent(player, "create_zone_marker", player.Position, int.Parse(radius));
            player.SendChatMessage("Container raidus edited. The radius will be displayed for 2 seconds.");
            ZoneTimer = new Timer { Interval = 2000 };
            ZoneTimer.Elapsed += delegate { DeleteZoneMarker(player); };
            ZoneTimer.Start();
        }

        [Command("removecontainerzone"), Help(HelpManager.CommandGroups.AdminLevel5, "Remove a container zone.", new[] { "Target container ID." })]
        public void removecontainerzone_cmd(Client player, string containerid)
        {
            var account = player.GetAccount();

            if (account.AdminLevel < 5)
            {
                player.SendChatMessage("You do not have permission to do this.");
                return;
            }

            var containerZone = ContainerZone.GetAllContainerZones()[int.Parse(containerid)];
            containerZone.Remove();
            player.SendChatMessage("Container zone removed.");
        }

        [Command("setplayerrenown"), Help(HelpManager.CommandGroups.AdminLevel5, "Set a player's renown.", new[] { "Target player ID." })]
        public void setplayerrenown_cmd(Client player, string target, int amount)
        {
            var account = player.GetAccount();

            if (account.AdminLevel < 5)
            {
                return;
            }

            var receiver = PlayerManager.ParseClient(target);

            if (receiver == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "That player is not connected.");
                return;
            }

            receiver.GetCharacter().Renown = amount;
            player.SendChatMessage($"You have set {receiver.GetCharacter().CharacterName}'s renown to {amount}.");
        }

        #endregion

    }
}
