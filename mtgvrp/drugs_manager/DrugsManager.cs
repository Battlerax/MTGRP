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
    // TODO: Add Effects. (Added to Coke + Weed,Speed) Rem : Heroin and Meth.
    // TODO: Make the whole crate thing a bit more exciting. (Added crowbar)
    // TODO: Improve crowbar mechanics. (Cooldown if player is unable to crack open the crate.)

    internal class DrugsManager : Script
    {

       
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
            DebugManager.DebugMessage("[DrugsM]: Drugs have successfully booted up.");
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

            boostArmor(sender, cokeVal);

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
            if (!CheckForCorrectAmount(weedVal, drugCheck))
            {
                API.sendChatMessageToPlayer(sender, "You don't have enough weed to smoke!");
                return;
            }

            boostHealth(sender, weedVal);

            ChatManager.RoleplayMessage(playerChar, "has smoked some weed.", ChatManager.RoleplayMe);
            API.sendChatMessageToPlayer(sender, "You smoked " + weedVal + " grams of weed.");


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
            if (!CheckForCorrectAmount(speedVal, drugCheck))
            {
                API.sendChatMessageToPlayer(sender, "You don't have enough speed to take!");
                return;
            }

            tempBoostHealth(sender,speedVal);
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
                    spawnDrop(sender, weed);
                    break;

                case "coke":
                    IInventoryItem cocaine = new Cocaine();
                    cocaine.Amount = drugAmount;
                    spawnDrop(sender,cocaine);

                    break;

                case "meth":
                    IInventoryItem meth = new Meth();
                    meth.Amount = drugAmount;
                    spawnDrop(sender,meth);
                    break;

                case "heroin":
                    IInventoryItem heroin = new Heroin();
                    heroin.Amount = drugAmount;
                    spawnDrop(sender,heroin);
          
                    break;

                case "speed":
                    IInventoryItem speed = new Speed();
                    speed.Amount = drugAmount;
                    spawnDrop(sender,speed);

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
        public void spawnDrop(Client sender, IInventoryItem drug)
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

        // DRUG EFFECTS - ADD ALL EFFECTS HERE!

        public void boostArmor(Client sender, int amount)
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


        public void tempBoostHealth(Client sender, int amount)
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

        // EFFECTS END HERE



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