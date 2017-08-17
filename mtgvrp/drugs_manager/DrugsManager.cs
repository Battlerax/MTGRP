using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using Newtonsoft.Json;

namespace mtgvrp.drugs_manager
{


    // TODO: Reduce the amount of code reusage.
    // TODO: Add Effects. (Added to Coke + Weed,Speed,Heroin) Rem : Meth.
    // TODO: Make the whole crate thing a bit more exciting. (Added crowbar)
    // TODO: Improve crowbar mechanics. (Cooldown if player is unable to crack open the crate.)

    internal class DrugsManager : Script
    {

        private const int visualPerAmount = 5;

        private const int HeroinDivider = 3;
        private const int MaxPermittedToleranceLevel = 15;
       
        private const int ArmorMultipler = 5;
        private const int HealthMultipler = 5;
        private const int TempHealthMultipler = 25;

        private const int MaxArmor = 100;
        private const int MaxHealth = 100;

        public const int MaxAirDropSize = 1000; 

        private const int CurrentDrugSize = 1;
        private const double TempTime = 0.5;

        private readonly Timer lowerTempVals = new Timer(ConvertMinToMilli(TempTime));


        // Current list of all airdrops. All airdrops will be removed on server restart to prevent them clogging up.
        private List<Airdrop> _airdrops = new List<Airdrop>();

        public DrugsManager()
        {
            DebugManager.DebugMessage("[DrugsM]: Drugs are booting up.");
            API.onClientEventTrigger += DrugsManagerClient;
            API.onResourceStart += startTimer;
            API.onPlayerDisconnected += clearEffects;
            DebugManager.DebugMessage("[DrugsM]: Drugs have successfully booted up.");
        }

        private void clearEffects(Client player, string reason)
        {
            API.triggerClientEvent(player,"clearAllEffects");
        }

        private void startTimer()
        {
            lowerTempVals.Elapsed += reducePlayerTempValues;
            lowerTempVals.Enabled = true;
        }

        private void reducePlayerTempValues(object sender, ElapsedEventArgs e)
        {
            foreach (Client c in API.getAllPlayers())
            {
                if(c == null) return;

                Character playerChar = c.GetCharacter();
                if (playerChar == null) return;

               
                if (playerChar.TempHealth > 0)
                {
                    if (playerChar.TempHealth < TempHealthMultipler)
                    {
                        int amountToTake = TempHealthMultipler - playerChar.TempHealth;
                        int playerRemHealth = API.getPlayerHealth(c) - amountToTake;

                        if (playerRemHealth <= 0)
                        {
                            API.setPlayerHealth(c, 1);
                            playerChar.TempHealth = 0;
                            return;
                        }
                        API.setPlayerHealth(c, API.getPlayerHealth(c) - amountToTake);
                        playerChar.TempHealth = 0;
                        return;
                    }
                    API.setPlayerHealth(c, API.getPlayerHealth(c) - TempHealthMultipler);
                    playerChar.TempHealth = playerChar.TempHealth - TempHealthMultipler;
                }
            }
        }

        private void DrugsManagerClient(Client sender, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case ("findGround"):

                    // Pulls the unique ID of the airdrop, then sets it's prop to the ground.

                    Guid dropId = JsonConvert.DeserializeObject<Guid>((string) arguments[1]);
                    Airdrop a = FindCorrectAirdrop(dropId);
                    Vector3 corrPosition = API.getEntityPosition(sender);
                    corrPosition.Z = (float) arguments[0];
                    PlaceAirDropProp(a,corrPosition);
                    // Just in case, API call appears not to work correctly from time to time...
                    API.sendNativeToAllPlayers(Hash.PLACE_OBJECT_ON_GROUND_PROPERLY,a.prop);
                    Vector3 markerPoint = API.getEntityPosition(sender);
                    a.marker = new MarkerZone(new Vector3(markerPoint.X,markerPoint.Y,markerPoint.Z+1), new Vector3()) { TextLabelText = "Drugs Crate - Locked"  };
                    a.marker.Create();
                    

                    break;
            }

        }
        
        

        // All drugs and current effects. If a new drug is added, add the command here! 


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
                API.sendChatMessageToPlayer(sender, "You don't have enough coke to sniff!");
                return;
            }

            BoostArmor(sender, cokeVal);

            ChatManager.RoleplayMessage(playerChar, "has sniffed some cocaine.", ChatManager.RoleplayMe);
            API.sendChatMessageToPlayer(sender, "You sniffed " + cokeVal + " grams of cocaine.");

            InventoryManager.DeleteInventoryItem(playerChar, typeof(Cocaine), cokeVal);
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
                API.sendChatMessageToPlayer(sender,"Taking more drugs in the middle of a trip is probably a bad idea. Wait it out.");
                return;
            }
            if (!CheckForCorrectAmount(weedVal, drugCheck))
            {
                API.sendChatMessageToPlayer(sender, "You don't have enough weed to smoke!");
                return;
            }

            boostHealth(sender, weedVal);

            API.triggerClientEvent(sender, "weedVisual",(visualPerAmount*weedVal) * 1000);
            ChatManager.RoleplayMessage(playerChar, "has smoked some weed.", ChatManager.RoleplayMe);
            API.sendChatMessageToPlayer(sender, "You smoked " + weedVal + " grams of weed.");
            playerChar.WeedTimer = new Timer {Interval = (weedVal * visualPerAmount) * 1000};
            playerChar.WeedTimer.Elapsed += delegate { clearWeedEffect(sender); };
            playerChar.WeedTimer.Start();   
            API.sendChatMessageToPlayer(sender,playerChar.WeedTimer.Interval.ToString());

            InventoryManager.DeleteInventoryItem(playerChar, typeof(Weed), weedVal);
        }

        private void clearWeedEffect(Client sender)
        {
            Character c = sender.GetCharacter();
            c.WeedTimer.Stop();
            API.triggerClientEvent(sender,"clearWeed");
        }

        [Command("takespeed", GreedyArg = true, Alias = "usespeed")]
        [Help(HelpManager.CommandGroups.General, "Take a few pills of speed.", "Amount of speed to take.")]
        public void speed_cmd(Client sender, string amount)
        {
            int speedVal;
            var playerChar = sender.GetCharacter();
            if (!int.TryParse(amount, out speedVal)) return;
            var drugCheck = InventoryManager.DoesInventoryHaveItem(playerChar, typeof(Speed));
            if (!CheckForCorrectAmount(speedVal, drugCheck))
            {
                API.sendChatMessageToPlayer(sender, "You don't have enough speed to take!");
                return;
            }

            TempBoostHealth(sender,speedVal);
            ChatManager.RoleplayMessage(playerChar, "has took some pills of speed.", ChatManager.RoleplayMe);

            API.sendChatMessageToPlayer(sender, "You took " + speedVal + " pills of speed.");
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
                API.sendChatMessageToPlayer(sender, "You don't have enough heroin to inject!");
                return;
            }

            ChatManager.RoleplayMessage(playerChar, "has injected some heroin into their vein.",
                ChatManager.RoleplayMe);

            API.sendChatMessageToPlayer(sender, "You injected " + heroinVal + " mg of heroin.");
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
                API.sendChatMessageToAll("debug : " + drugCheck.Length + drugCheck[0].Amount);
                API.sendChatMessageToPlayer(sender, "You don't have enough meth to take!");
                return;
            }

            ChatManager.RoleplayMessage(playerChar, "has took some pills of meth.", ChatManager.RoleplayMe);
            API.sendChatMessageToPlayer(sender, "You took " + methVal + " pills of meth.");
            InventoryManager.DeleteInventoryItem(playerChar, typeof(Meth), methVal);
        }



        // Used by LVL 5+ admins to drop drug crates. If a new drug is added, put it in the case list.

        [Command("dropcrate")]
        [Help(HelpManager.CommandGroups.AdminLevel4, "Drop a drug crate at your location.", "Type of Drug.","Amount of drug.")]
        public void cmd_dropbox(Client sender, String drug, String amount)
        {
            Account a = sender.GetAccount();
            int drugAmount;

            if (!int.TryParse(amount, out drugAmount)) return;
            if (a.AdminLevel < 5) return;

            if (FindNearestAirdrop(sender, 20) != null)
            {
                API.sendChatMessageToPlayer(sender, "Another crate is too close.");
                return;
            }

            if (drugAmount * CurrentDrugSize > MaxAirDropSize)
            {
                API.sendChatMessageToPlayer(sender,"You're only allowed up to " + MaxAirDropSize + " of a drug in one airdrop!");
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
                    API.sendChatMessageToPlayer(sender,"That's an incorrect drug name.");
                    return;

                
            }
        }


        [Command("opencrate")]
        public void openCrate(Client sender)
        {
            Airdrop closest = FindNearestAirdrop(sender);
            Character c = sender.GetCharacter();
            if (closest != null)
            {
                if (!closest.IsOpen)
                {
                    ChatManager.RoleplayMessage(c,"attempts to open the crate, but was unable to open it.",ChatManager.RoleplayMe);
                    API.sendChatMessageToPlayer(sender,"The crate is locked.");
                    return;
                }

                ChatManager.RoleplayMessage(c, "attempts to open the crate, peering inside.", ChatManager.RoleplayMe);

                InventoryManager.ShowInventoryManager(sender, sender.GetCharacter(), closest, "Inventory: ", "Crate: ");

            }
        }

        [Command("prycrate")]
        public void pryCrate(Client sender)
        {
            Character c = sender.GetCharacter();
            Airdrop closest = FindNearestAirdrop(sender);

            if (closest == null) return;

            if (closest.IsOpen)
            {
                API.sendChatMessageToPlayer(sender,"This crate is already open!");
                return;
            }

            if (!closest.IsOpen && InventoryManager.DoesInventoryHaveItem<Crowbar>(c).Length >= 1)
            {
                ChatManager.RoleplayMessage(c, "forces open the crate, using their crowbar to pry the lid off.",
                    ChatManager.RoleplayMe);
                closest.IsOpen = true;
                closest.marker.Destroy();
                closest.marker.TextLabelText = "Drugs Crate - Unlocked";
                closest.marker.Create();
                return;
            }

            ChatManager.RoleplayMessage(c,"attempts to pry open the crate lid with their hands, but is unable to.",ChatManager.RoleplayMe);
            API.sendChatMessageToPlayer(sender,"You need a crowbar for this!");

        }


        // Creates the drop, adds it to the list and calls for the prop to be set on the floor.
        public void SpawnDrop(Client sender, IInventoryItem drug)
        {
            Airdrop drop;
            drop = new Airdrop(drug, API.getEntityPosition(sender));
            _airdrops.Add(drop);
            API.triggerClientEvent(sender, "getClientGround",API.toJson(drop.id));
        }


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
            if (API.getPlayerArmor(sender) + amount * ArmorMultipler > MaxArmor)
                API.setPlayerArmor(sender, MaxArmor);
            else
                API.setPlayerArmor(sender, API.getPlayerArmor(sender) + amount * ArmorMultipler);
        }

        public void boostHealth(Client sender, int amount)
        {
            if (API.getPlayerHealth(sender) + amount * HealthMultipler > MaxHealth)
                API.setPlayerHealth(sender, MaxHealth);
            else
                API.setPlayerHealth(sender, API.getPlayerHealth(sender) + amount * HealthMultipler);
        }


        public void TempBoostHealth(Client sender, int amount)
        {
            Character c = sender.GetCharacter();

            if (API.getPlayerHealth(sender) + amount * TempHealthMultipler > MaxHealth)
            {
                c.TempHealth = MaxHealth - API.getPlayerHealth(sender);
                return;
            }

            API.setPlayerHealth(sender,API.getPlayerHealth(sender) + amount * TempHealthMultipler);
            c.TempHealth = amount * TempHealthMultipler;
        }


        public void MaxArmourAndHealth(Client sender, int amount)
        {
            Character c = sender.GetCharacter();

            if (amount <  c.HeroinTolerance / HeroinDivider)
            {
                API.sendChatMessageToPlayer(sender,"The heroin has no effect on you! You've built up too much of a tolerance!");
                return;
            }
        
            API.setPlayerArmor(sender,100);
            API.setPlayerHealth(sender,100);

            if (c.HeroinTolerance > MaxPermittedToleranceLevel)
            {
                ToleranceEffectRoll(c);
                return;
            }
            API.sendChatMessageToPlayer(sender,"Your tolerance levels have gone up from this... You'll need more in the future to get the same buzz.");
            c.HeroinTolerance = (c.HeroinTolerance + amount) / 2;

        


        }

        public void ToleranceEffectRoll(Character c)
        {
            Random r = new Random();



            int effectRoll = r.Next(1, 11);

            if (effectRoll == 1)
            {
                API.sendChatMessageToPlayer(c.Client,"You manage to kick part of your tolerance. You should be more careful of heroin usage...");
                c.HeroinTolerance = 2;
                return;
            }
            if (effectRoll > 1 && effectRoll < 6)
            {
                API.sendChatMessageToPlayer(c.Client,"You're really not feeling the effects anymore. Might be a good time to cutdown.");
                c.HeroinTolerance = c.HeroinTolerance - 5;
                return;
            }
            if (effectRoll >= 6 && effectRoll < 10)
            {
                API.sendChatMessageToPlayer(c.Client,"Your current usage limits are causing you serious pain.");
                API.setPlayerHealth(c.Client,API.getPlayerHealth(c.Client) - 10);
                // TODO: Add camera effects here!
                return;
            }
            if (effectRoll == 10)
            {
                API.sendChatMessageToPlayer(c.Client,"Your body is unable to take the heroin anymore, and begins to breakdown.");
                API.setPlayerHealth(c.Client,1);
                API.setPlayerArmor(c.Client,0);
               // TODO: Add Camera Effects here!
            }
        }


        #endregion


        // Airdrop helper methods.

        public void PlaceAirDropProp(Airdrop drop, Vector3 loc)
        {
            drop.prop = API.createObject(1885839156, drop.Loc, new Vector3());
        }

        private Airdrop FindNearestAirdrop(Client sender, float bound = 5)
        {
            Airdrop drop = null;
            foreach (var a in _airdrops)
            {
                var dist = a.Loc.DistanceTo(API.getEntityPosition(sender));
                if (dist < bound)
                    drop = a;
            }
            return drop;
        }



        public Airdrop FindCorrectAirdrop(Guid id)
        {
            foreach (var a in _airdrops)
                if (a.id == id)
                    return a;
            return null;
        }


        // Debug commands.


        [Command("giveAllDrug")]
        public void giveDrug(Client sender)
        {
            var playerChar = sender.GetCharacter();
            InventoryManager.GiveInventoryItem(playerChar, new Cocaine(), 10);
            InventoryManager.GiveInventoryItem(playerChar, new Speed(), 10);
            InventoryManager.GiveInventoryItem(playerChar, new Heroin(), 10);
            InventoryManager.GiveInventoryItem(playerChar, new Weed(), 10);
            InventoryManager.GiveInventoryItem(playerChar, new Meth(), 10);


        }

        public static double ConvertMinToMilli(double min)
        {
            return min * 60000;
        }


    }
}