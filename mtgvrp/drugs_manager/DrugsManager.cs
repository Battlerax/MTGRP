using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using GTANetworkAPI;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using Newtonsoft.Json;

namespace mtgvrp.drugs_manager
{


    // TODO: Reduce the amount of code reusage.
    // TODO: Add Effects. (Added to Coke + Weed,Speed,Heroin) Rem : Meth.
    // TODO: Improve crowbar mechanics. (Cooldown if player is unable to crack open the crate.)

    internal class DrugsManager : Script
    {

        private const int VisualPerAmount = 10;

        private const int HeroinDivider = 2;
        private const int MaxPermittedToleranceLevel = 15;
       
        private const int ArmorMultipler = 10;
        private const int HealthMultipler = 10;
        private const int TempHealthMultipler = 25;

        private const int MaxArmor = 100;
        private const int MaxHealth = 100;

        public const int MaxAirDropSize = 1000; 

        private const int CurrentDrugSize = 1;
        private const double TempTime = 1.5;



        private const int LowerHeroinTripTime = 60;
        private const int HigherHeroinTripTime = 120;

        private readonly Timer _lowerTempVals = new Timer(ConvertMinToMilli(TempTime));


        // Current list of all airdrops. All airdrops will be removed on server restart to prevent them clogging up.
        private List<Airdrop> _airdrops = new List<Airdrop>();

        public DrugsManager()
        {
            DebugManager.DebugMessage("[DrugsM]: Drugs are booting up.");
            Event.OnResourceStart += StartTimer;
            Event.OnPlayerDisconnected += ClearEffects;
            DebugManager.DebugMessage("[DrugsM]: Drugs have successfully booted up.");
        }


        private void ClearEffects(Client player, byte type, string reason)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "clearAllEffects");
        }

        // Ticks based on TempTime, reduces tempHealth of any player on server.

        private void StartTimer()
        {
            _lowerTempVals.Elapsed += ReducePlayerTempValues;
            _lowerTempVals.Enabled = true;
        }

       
        private void ReducePlayerTempValues(object sender, ElapsedEventArgs e)
        {
            foreach (Client c in API.GetAllPlayers())
            {
                if(c == null) return;

                Character playerChar = c.GetCharacter();
                if (playerChar == null) return;

            
                if (playerChar.TempHealth <= 0) continue;

                if (playerChar.TempHealth < TempHealthMultipler)
                {
                    int amountToTake = TempHealthMultipler - playerChar.TempHealth;
                    int playerRemHealth = API.GetPlayerHealth(c) - amountToTake;

                    // If a player is unable to "payback" their health, set them on 1 HP.
                    if (playerRemHealth <= 0)
                    {
                        API.SetPlayerHealth(c, 1);
                        playerChar.TempHealth = 0;
                        return;
                    }
                    // If there's less than the Multiplier remaining, take that and set temphealth to 0.
                    API.SetPlayerHealth(c, API.GetPlayerHealth(c) - amountToTake);
                    playerChar.TempHealth = 0;
                    return;
                }
                // Else, just take the health and remove the multipler from temphealth.
                API.SetPlayerHealth(c, API.GetPlayerHealth(c) - TempHealthMultipler);
                playerChar.TempHealth = playerChar.TempHealth - TempHealthMultipler;
            }
        }


        #region  All drugs and current effects. If a new drug is added, add the command here! 


        // Process for a drug - Check their amount is a number, check they have the drug then pass it to CheckForCorrectAmount.

        [Command("sniffcoke", GreedyArg = true, Alias = "usecoke")]
        [Help(HelpManager.CommandGroups.General, "Sniff a line of coke.", "Amount of coke to sniff.")]
        public void coke_cmd(Client sender, string amount)
        {
            int cokeVal;
            var playerChar = sender.GetCharacter();
            if (!int.TryParse(amount, out cokeVal)) return;
            var drugCheck = InventoryManager.DoesInventoryHaveItem(playerChar, typeof(Cocaine));
            if (!CheckForCorrectAmount(cokeVal, drugCheck))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have enough coke to sniff!");
                return;
            }

            BoostArmor(sender, cokeVal);

            NAPI.ClientEvent.TriggerClientEvent(sender, "cokeVisual", (cokeVal * VisualPerAmount) * 1000);
            ChatManager.RoleplayMessage(playerChar, "has sniffed some cocaine.", ChatManager.RoleplayMe);
            NAPI.Chat.SendChatMessageToPlayer(sender, "You sniffed " + cokeVal + " grams of cocaine.");

            InventoryManager.DeleteInventoryItem(playerChar, typeof(Cocaine), cokeVal);
            playerChar.CocaineTimer = new Timer { Interval = (cokeVal * VisualPerAmount) * 1000 };
            playerChar.CocaineTimer.Elapsed += delegate { clearCocaineEffect(sender); };
            playerChar.CocaineTimer.Start();
        }


        [Command("smokeweed", GreedyArg = true, Alias = "useweed")]
        [Help(HelpManager.CommandGroups.General, "Smoke some weed.", "Amount of weed to smoke.")]
        public void weed_cmd(Client sender, string amount)
        {
            int weedVal;
            var playerChar = sender.GetCharacter();
            if (!int.TryParse(amount, out weedVal)) return;
            var drugCheck = InventoryManager.DoesInventoryHaveItem(playerChar, typeof(Weed));
            if (playerChar.WeedTimer != null && playerChar.WeedTimer.Enabled)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender,"Taking more drugs in the middle of a trip is probably a bad idea. Wait it out.");
                return;
            }
            if (!CheckForCorrectAmount(weedVal, drugCheck))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have enough weed to smoke!");
                return;
            }

            BoostHealth(sender, weedVal);

            NAPI.ClientEvent.TriggerClientEvent(sender, "weedVisual",(VisualPerAmount*weedVal) * 1000);
            ChatManager.RoleplayMessage(playerChar, "has smoked some weed.", ChatManager.RoleplayMe);
            NAPI.Chat.SendChatMessageToPlayer(sender, "You smoked " + weedVal + " grams of weed.");
            playerChar.WeedTimer = new Timer {Interval = (weedVal * VisualPerAmount) * 1000};
            playerChar.WeedTimer.Elapsed += delegate { clearWeedEffect(sender); };
            playerChar.WeedTimer.Start();   

            InventoryManager.DeleteInventoryItem(playerChar, typeof(Weed), weedVal);
        }


        [Command("takespeed", GreedyArg = true, Alias = "usespeed")]
        [Help(HelpManager.CommandGroups.General, "Take a few pills of speed.", "Amount of speed to take.")]
        public void speed_cmd(Client sender, string amount)
        {
            int speedVal;
            var playerChar = sender.GetCharacter();
            if (!int.TryParse(amount, out speedVal)) return;
            var drugCheck = InventoryManager.DoesInventoryHaveItem(playerChar, typeof(Speed));
            if (playerChar.Speedtimer != null && playerChar.Speedtimer.Enabled)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "Taking more drugs in the middle of a trip is probably a bad idea. Wait it out.");
                return;
            }
            if (!CheckForCorrectAmount(speedVal, drugCheck))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have enough speed to take!");
                return;
            }
            
            NAPI.ClientEvent.TriggerClientEvent(sender, "speedVisual",(VisualPerAmount*speedVal) * 1000);
            playerChar.Speedtimer = new Timer{Interval = speedVal * VisualPerAmount * 1000};
            playerChar.Speedtimer.Elapsed += delegate { ClearSpeedEffect(sender); };
            playerChar.Speedtimer.Start();
            TempBoostHealth(sender,speedVal);
            ChatManager.RoleplayMessage(playerChar, "has took some pills of speed.", ChatManager.RoleplayMe);

            NAPI.Chat.SendChatMessageToPlayer(sender, "You took " + speedVal + " pills of speed.");
            InventoryManager.DeleteInventoryItem(playerChar, typeof(Speed), speedVal);
        }

        [Command("injectheroin", GreedyArg = true, Alias = "useheroin")]
        [Help(HelpManager.CommandGroups.General, "Inject some heroin.", "Amount of heroin to inject.")]
        public void heroin_cmd(Client sender, string amount)
        {
            int heroinVal;
            var playerChar = sender.GetCharacter();
          
            if (!int.TryParse(amount, out heroinVal)) return;
            var drugCheck = InventoryManager.DoesInventoryHaveItem(playerChar, typeof(Heroin));
            if (!CheckForCorrectAmount(heroinVal, drugCheck))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have enough heroin to inject!");
                return;
            }

            if (playerChar.HeroinTimer != null && playerChar.HeroinTimer.Enabled)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender,"You're in the middle of a horrible trip. You're unable to inject anymore heroin!");
                return;
            }

            ChatManager.RoleplayMessage(playerChar, "has injected some heroin into their vein.",
                ChatManager.RoleplayMe);

            NAPI.Chat.SendChatMessageToPlayer(sender, "You injected " + heroinVal + " mg of heroin.");
            MaxArmourAndHealth(sender,heroinVal);
            InventoryManager.DeleteInventoryItem(playerChar, typeof(Heroin), heroinVal);
        }

        [Command("takemeth", GreedyArg = true, Alias = "usemeth")]
        [Help(HelpManager.CommandGroups.General, "Take some meth pills.", "Amount of meth to take.")]
        public void meth_cmd(Client sender, string amount)
        {
            int methVal;
            var playerChar = sender.GetCharacter();
            if (!int.TryParse(amount, out methVal)) return;
            var drugCheck = InventoryManager.DoesInventoryHaveItem(playerChar, typeof(Meth));
            if (!CheckForCorrectAmount(methVal, drugCheck))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have enough meth to take!");
                return;
            }

            ChatManager.RoleplayMessage(playerChar, "has took some pills of meth.", ChatManager.RoleplayMe);
            NAPI.Chat.SendChatMessageToPlayer(sender, "You took " + methVal + " pills of meth.");
            InventoryManager.DeleteInventoryItem(playerChar, typeof(Meth), methVal);
        }

        #endregion



        #region  Wipe Client Effects Calls

        private void clearWeedEffect(Client sender)
        {
            Character c = sender.GetCharacter();
            c.WeedTimer.Stop();
            NAPI.ClientEvent.TriggerClientEvent(sender, "clearWeed");
        }

        private void clearHeroinEffect(Client sender)
        {
            Character c = sender.GetCharacter();
            c.HeroinTimer.Stop();
            NAPI.ClientEvent.TriggerClientEvent(sender, "clearHeroin");
        }

        private void ClearSpeedEffect(Client sender)
        {
            Character c = sender.GetCharacter();
            c.Speedtimer.Stop();
            NAPI.ClientEvent.TriggerClientEvent(sender, "clearSpeed");
        }

        private void clearCocaineEffect(Client sender)
        {
            Character c = sender.GetCharacter();
            c.CocaineTimer.Stop();
            NAPI.ClientEvent.TriggerClientEvent(sender,"clearCoke");
        }
        #endregion


        // Used by LVL 5+ admins to drop drug crates. If a new drug is added, put it in the case list.


        #region Crate Commands

        
        [Command("dropcrate")]
        [Help(HelpManager.CommandGroups.AdminLevel5, "Drop a drug crate at your location.", "Type of Drug.","Amount of drug.")]
        public void cmd_dropbox(Client sender, String drug, String amount)
        {
            Account a = sender.GetAccount();
            int drugAmount;
       
            if (!int.TryParse(amount, out drugAmount)) return;
            if (a.AdminLevel < 5 || drugAmount < 1) return;

            if (NAPI.Player.IsPlayerInAnyVehicle(sender))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender,"Get out of the vehicle, and try again.");
                return;
            }
            

            if (FindNearestAirdrop(sender, 20) != null)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "Another crate is too close.");
                return;
            }

            if (drugAmount * CurrentDrugSize > MaxAirDropSize)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender,"You're only allowed up to " + MaxAirDropSize + " of a drug in one airdrop!");
                return;
            }


            switch (drug.ToLower())
            {
                case "weed":
                    IInventoryItem weed = new Weed();
                    weed.Amount = drugAmount;
                    SpawnDrop(sender, weed);
                    break;

                case "coke":
                    IInventoryItem cocaine = new Cocaine();
                    cocaine.Amount = drugAmount;
                    SpawnDrop(sender,cocaine);

                    break;

                case "meth":
                    IInventoryItem meth = new Meth();
                    meth.Amount = drugAmount;
                    SpawnDrop(sender,meth);
                    break;

                case "heroin":
                    IInventoryItem heroin = new Heroin();
                    heroin.Amount = drugAmount;
                    SpawnDrop(sender,heroin);
          
                    break;

                case "speed":
                    IInventoryItem speed = new Speed();
                    speed.Amount = drugAmount;
                    SpawnDrop(sender,speed);

                    break;

                default:
                    NAPI.Chat.SendChatMessageToPlayer(sender,"That's an incorrect drug name.");
                    return;

                
            }

            NAPI.Chat.SendChatMessageToPlayer(sender, "Crate dropped.");

        }


        [Command("deleteCrate")]
        [Help(HelpManager.CommandGroups.AdminLevel5, "Delete a drug crate at your location.", null)]
        public void cmd_deleteCrate(Client sender)
        {
            Account a = sender.GetAccount();
            if (a.AdminLevel < 5) return;

            Airdrop dropToDelete = FindNearestAirdrop(sender);

            if (dropToDelete == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender,"No airdrops nearby.");
                return;
            }

            dropToDelete.Delete();
            _airdrops.Remove(dropToDelete);
            NAPI.Chat.SendChatMessageToPlayer(sender,"Airdrop has been deleted.");
            LogManager.Log(LogManager.LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {a.AdminName} has deleted a crate.");


        }

        [Command("opencrate")]
        [Help(HelpManager.CommandGroups.General, "Open a drug crate at your location.", null)]

        public void OpenCrate(Client sender)
        {

            if (NAPI.Player.IsPlayerInAnyVehicle(sender))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You can't open a crate while you're in a vehicle!");
                return;
            }

            Airdrop closest = FindNearestAirdrop(sender);
            Character c = sender.GetCharacter();
            if (closest == null) return;

            if (!closest.IsOpen)
            {
                ChatManager.RoleplayMessage(c,"attempts to open the crate, but was unable to open it.",ChatManager.RoleplayMe);
                NAPI.Chat.SendChatMessageToPlayer(sender,"The crate is locked.");
                return;
            }

            ChatManager.RoleplayMessage(c, "attempts to open the crate, peering inside.", ChatManager.RoleplayMe);

            InventoryManager.ShowInventoryManager(sender, sender.GetCharacter(), closest, "Inventory: ", "Crate: ");
        }

        [Command("prycrate")]
        [Help(HelpManager.CommandGroups.General, "Attempt to pry open a drug crate at your location.", null)]

        public void PryCrate(Client sender)
        {

            if (NAPI.Player.IsPlayerInAnyVehicle(sender))
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You can't pry a crate while you're in a vehicle!");
                return;
            }

            Character c = sender.GetCharacter();
            Airdrop closest = FindNearestAirdrop(sender);

            if (closest == null) return;

            if (closest.IsOpen)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender,"This crate is already open!");
                return;
            }

            if (!closest.IsOpen && InventoryManager.DoesInventoryHaveItem<Crowbar>(c).Length >= 1)
            {
                ChatManager.RoleplayMessage(c, "forces open the crate, using their crowbar to pry the lid off.",
                    ChatManager.RoleplayMe);
                closest.UpdateMarkerOpen();
                LogManager.Log(LogManager.LogTypes.Commands,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] User {c.CharacterName} has opened a crate.");

                return;

            }

            ChatManager.RoleplayMessage(c,"attempts to pry open the crate lid with their hands, but is unable to.",ChatManager.RoleplayMe);
            NAPI.Chat.SendChatMessageToPlayer(sender,"You need a crowbar for this!");

        }

        [Command("alockcrate")]
        public void cmd_lockcrate(Client sender)
        {
            if (sender.GetAccount().AdminLevel < 5) return;

            Airdrop a = FindNearestAirdrop(sender); 
            if (a == null) return;
            if (!a.IsOpen) return;

            a.UpdateMarkerClose();
            Account acc = sender.GetAccount();
            ChatManager.NearbyMessage(sender, 10, "~p~" + acc.AdminName + " locks the crate.");



            LogManager.Log(LogManager.LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has locked a crate.");

        }


        [Command("aopencrate")]
        [Help(HelpManager.CommandGroups.AdminLevel5, "Force open a drug crate at your location.", null)]
        public void cmd_aopencrate(Client sender)
        {

            Account a = sender.GetAccount();
            if (a.AdminLevel < 5) return;

            if (a.AdminDuty)
            {
                Airdrop drop = FindNearestAirdrop(sender);
                if (drop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "No crate around.");
                    return;
                }

                if (drop.IsOpen)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "Crates already open!");
                    return;
                }

                ChatManager.NearbyMessage(sender, 10, "~p~" + a.AdminName + " opens the crate.");
                drop.IsOpen = true;
                drop.UpdateMarkerOpen();
                return;
            }

            NAPI.Chat.SendChatMessageToPlayer(sender, "You're not on admin duty!");
            LogManager.Log(LogManager.LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {a.AdminName} has admin opened a crate.");
        }



        // Creates the drop, adds it to the list and calls for the prop to be set on the floor.
        public void SpawnDrop(Client sender, IInventoryItem drug)
        {
            var drop = new Airdrop(drug, NAPI.Entity.GetEntityPosition(sender));
            _airdrops.Add(drop);
            PlaceAirDropProp(drop,NAPI.Entity.GetEntityPosition(sender));
            NAPI.ClientEvent.TriggerClientEvent(sender, "PLACE_OBJECT_ON_GROUND_PROPERLY", drop.prop, "");
            Vector3 crateLoc = NAPI.Entity.GetEntityPosition(drop.prop);
            drop.SetCorrectCrateLocation(crateLoc);
            drop.marker = new MarkerZone(crateLoc, new Vector3()) { TextLabelText = "Drugs Crate - Locked" };
            drop.marker.Create();


        }


#endregion
        public bool CheckForCorrectAmount(int value, IInventoryItem[] drug)
        {
            // Person has the drug, and they have the correct amount.
            if (value < 1 || drug.Length < 1 || drug[0].Amount < value)
                return false;
            return true;
        }

        #region DrugEffects

        

        public void BoostArmor(Client sender, int amount)
        {
            if (API.GetPlayerArmor(sender) + amount * ArmorMultipler > MaxArmor)
                API.SetPlayerArmor(sender, MaxArmor);
            else
                API.SetPlayerArmor(sender, API.GetPlayerArmor(sender) + amount * ArmorMultipler);
        }

        public void BoostHealth(Client sender, int amount)
        {
            if (API.GetPlayerHealth(sender) + amount * HealthMultipler > MaxHealth)
                API.SetPlayerHealth(sender, MaxHealth);
            else
                API.SetPlayerHealth(sender, API.GetPlayerHealth(sender) + amount * HealthMultipler);
        }


        public void TempBoostHealth(Client sender, int amount)
        {
            Character c = sender.GetCharacter();

            if (API.GetPlayerHealth(sender) + amount * TempHealthMultipler > MaxHealth)
            {
                c.TempHealth = MaxHealth - API.GetPlayerHealth(sender);
                API.SetPlayerHealth(sender,MaxHealth);
                return;
            }

            API.SetPlayerHealth(sender,API.GetPlayerHealth(sender) + amount * TempHealthMultipler);
            c.TempHealth = amount * TempHealthMultipler;
        }


        public void MaxArmourAndHealth(Client sender, int amount)
        {
            Character c = sender.GetCharacter();
            int tot = c.HeroinTolerance / HeroinDivider;
            if (amount <  tot)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender,"The heroin has no effect on you! You've built up too much of a tolerance!");
                return;
            }
        
            API.SetPlayerArmor(sender,100);
            API.SetPlayerHealth(sender,100);

            if (c.HeroinTolerance > MaxPermittedToleranceLevel)
            {
                ToleranceEffectRoll(c);
                return;
            }
            NAPI.Chat.SendChatMessageToPlayer(sender,"Your tolerance levels have gone up from this... You'll need more in the future to get the same buzz.");
            c.HeroinTolerance = (c.HeroinTolerance + 2);

        


        }

        [Command("tol")]
        public void callTolRoll(Client sender)
        {
            Character c = sender.GetCharacter();
            ToleranceEffectRoll(c);
        }
        public void ToleranceEffectRoll(Character c)
        {
            Random r = new Random();



            int effectRoll = r.Next(1, 11);

            if (effectRoll == 1)
            {
                NAPI.Chat.SendChatMessageToPlayer(c.Client,"You manage to kick part of your tolerance. You should be more careful of heroin usage...");
                c.HeroinTolerance = 2;
                return;
            }
            if (effectRoll > 1 && effectRoll < 6)
            {
                NAPI.Chat.SendChatMessageToPlayer(c.Client,"You're really not feeling the effects anymore. Might be a good time to cutdown.");
                c.HeroinTolerance = c.HeroinTolerance - 5;
                return;
            }
            if (effectRoll >= 6 && effectRoll < 10)
            {
                NAPI.Chat.SendChatMessageToPlayer(c.Client,"Your current usage limits are causing you serious pain.");
                API.SetPlayerHealth(c.Client,API.GetPlayerHealth(c.Client) - 10);
                if (c.Client == null)
                {
                    API.SendChatMessageToAll("lol null");
                }
                NAPI.ClientEvent.TriggerClientEvent(c.Client, "heroinVisual",LowerHeroinTripTime * 1000);       
                c.HeroinTimer = new Timer { Interval = LowerHeroinTripTime * 1000 };
                c.HeroinTimer.Elapsed += delegate { clearHeroinEffect(c.Client); };
                c.HeroinTimer.Start();

                return;
            }
            if (effectRoll == 10)
            {
                NAPI.Chat.SendChatMessageToPlayer(c.Client,"Your body is unable to take the heroin anymore, and begins to breakdown.");
                API.SetPlayerHealth(c.Client,1);
                API.SetPlayerArmor(c.Client,0);
                NAPI.ClientEvent.TriggerClientEvent(c.Client, "heroinVisual", HigherHeroinTripTime * 1000);
                c.HeroinTimer = new Timer { Interval = HigherHeroinTripTime * 1000 };
                c.HeroinTimer.Elapsed += delegate { clearHeroinEffect(c.Client); };
                c.HeroinTimer.Start();
            }
        }


        #endregion


        // Airdrop helper methods.

        public void PlaceAirDropProp(Airdrop drop, Vector3 loc)
        {
            drop.prop = API.CreateObject(1885839156, drop.Loc, new Vector3());
        }

        private Airdrop FindNearestAirdrop(Client sender, float bound = 5)
        {
            Airdrop drop = null;
            foreach (var a in _airdrops)
            {
                var dist = a.Loc.DistanceTo(NAPI.Entity.GetEntityPosition(sender));
                if (dist < bound)
                    drop = a;
            }
            return drop;
        }



        public Airdrop FindCorrectAirdrop(Guid id)
        {
            return _airdrops.FirstOrDefault(a => a.id == id);
        }





        public static double ConvertMinToMilli(double min)
        {
            return min * 60000;
        }


    }
}
