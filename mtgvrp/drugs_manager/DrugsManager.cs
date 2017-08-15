using System;
using System.Collections.Generic;
using System.IO;
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
    internal class DrugsManager : Script
    {
        private const int ArmorMultipler = 10;
        private const int HealthMultipler = 10;
        private const int MaxArmor = 100;
        private const int MaxHealth = 100;
        public const int MaxAirDropSize = 1000;
        private List<Airdrop> _airdrops = new List<Airdrop>();

        public DrugsManager()
        {
            DebugManager.DebugMessage("[DrugsM]: Drugs are booting up.");
            API.onClientEventTrigger += DrugsManagerClient;
            DebugManager.DebugMessage("[DrugsM]: Drugs have successfully booted up.");
        }

        private void DrugsManagerClient(Client sender, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case ("findGround"):

                    Guid dropId = JsonConvert.DeserializeObject<Guid>((string) arguments[1]);
                    Airdrop a = FindCorrectAirdrop(dropId);
                    Vector3 corrPosition = API.getEntityPosition(sender);
                    corrPosition.Z = (float) arguments[0];
                    PlaceAirDropProp(a,corrPosition);
                    

                    break;
            }

        }
        

        // TODO: Reduce the amount of code reusage.
        // TODO: Add Effects.
        // TODO: Comment this code!
        // TODO: Make the whole crate thing a bit more exciting. 


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

        [Command("opencrate")]
        public void openCrate(Client sender)
        {
            Airdrop closest = FindNearestAirdrop(sender);
            if (closest != null)
            {
                API.sendChatMessageToAll("found crate");
                InventoryManager.ShowInventoryManager(sender, sender.GetCharacter(), closest, "Inventory: ", "Crate: ");

            }
        }

        [Command("giveAdmin")]
        public void giveAdmin(Client sender)
        {
            Account a = sender.GetAccount();
            a.AdminLevel = 8;
        }


        [Command("dropbox")]
        [Help(HelpManager.CommandGroups.AdminLevel4, "Drop a box at your location.", "Type of Drug.","Amount of drug.")]
        public void cmd_dropbox(Client sender, String drug, String amount)
        {
            Account a = sender.GetAccount();
            int drugAmount;

            if (!int.TryParse(amount, out drugAmount)) return;
            if (a.AdminLevel < 4) return;


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

        public void spawnDrop(Client sender, IInventoryItem drug)
        {
            Airdrop drop;
            drop = new Airdrop(drug, API.getEntityPosition(sender));
            _airdrops.Add(drop);
            API.triggerClientEvent(sender, "getClientGround",API.toJson(drop.id));
        }

        public bool CheckForCorrectAmount(int value, IInventoryItem[] drug)
        {
            if (value < 1 || drug.Length < 1 || drug[0].Amount < value)
                return false;
            return true;
        }


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


    }
}