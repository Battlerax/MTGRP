using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Xaml;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using GrandTheftMultiplayer.Server.Managers;
using MongoDB.Driver;
using mtgvrp.core.Help;
using mtgvrp.property_system;


namespace mtgvrp.drugs_manager
{
    class DrugsEffectsManager : Script
    {

        public DrugsEffectsManager()
        {
            DebugManager.DebugMessage("[DRUGSM] Drugs are booting up.");
            DebugManager.DebugMessage("[DRUGSM] Drugs have successfully booted up.");

        }

        // TODO: Reduce the amount of code reusage.
        // TODO: Add Effects.


        [Command("sniffcoke",GreedyArg = true, Alias = "usecoke"),Help(HelpManager.CommandGroups.General,"Sniff a line of coke.","Amount of coke to sniff.")]
        public void coke_cmd(Client sender, String amount)
        {
            int cokeVal;
            Character playerChar = sender.GetCharacter();
            if (!Int32.TryParse(amount, out cokeVal)) return;
            var drugCheck = InventoryManager.DoesInventoryHaveItem(playerChar, typeof(Cocaine));
            if (!CheckForCorrectAmount(cokeVal,drugCheck))
            {
                API.sendChatMessageToPlayer(sender, "You don't have enough coke to sniff!");
                return;

            }
            

            API.sendChatMessageToPlayer(sender, "You sniffed " + cokeVal + " grams of cocaine.");
            InventoryManager.DeleteInventoryItem(playerChar, typeof(Cocaine), cokeVal);


        }


        [Command("smokeweed", GreedyArg = true, Alias = "useweed"), Help(HelpManager.CommandGroups.General, "Smoke some weed.", "Amount of weed to smoke.")]
        public void weed_cmd(Client sender, String amount)
        {
            int weedVal;
            Character playerChar = sender.GetCharacter();
            if (!Int32.TryParse(amount, out weedVal)) return;
            var drugCheck = InventoryManager.DoesInventoryHaveItem(playerChar, typeof(Weed));
            if (!CheckForCorrectAmount(weedVal,drugCheck))
            {
                API.sendChatMessageToPlayer(sender, "You don't have enough weed to smoke!");
                return;

            }

            API.sendChatMessageToPlayer(sender, "You smoked " + weedVal + " grams of weed.");
            InventoryManager.DeleteInventoryItem(playerChar, typeof(Weed), weedVal);


        }

        [Command("takespeed", GreedyArg = true, Alias = "usespeed"), Help(HelpManager.CommandGroups.General, "Take a few pills of speed.", "Amount of speed to take.")]
        public void speed_cmd(Client sender, String amount)
        {
            int speedVal;
            Character playerChar = sender.GetCharacter();
            if (!Int32.TryParse(amount, out speedVal)) return;
            var drugCheck = InventoryManager.DoesInventoryHaveItem(playerChar, typeof(Speed));
            if (!CheckForCorrectAmount(speedVal,drugCheck))
            {
                API.sendChatMessageToPlayer(sender, "You don't have enough speed to take!");
                return;

            }

            API.sendChatMessageToPlayer(sender, "You took " + speedVal + " pills of speed.");
            InventoryManager.DeleteInventoryItem(playerChar, typeof(Speed), speedVal);


        }

        [Command("injectheroin", GreedyArg = true, Alias = "useheroin"), Help(HelpManager.CommandGroups.General, "Inject some heroin.", "Amount of heroin to inject.")]
        public void heroin_cmd(Client sender, String amount)
        {
            int heroinVal;
            Character playerChar = sender.GetCharacter();
            if (!Int32.TryParse(amount, out heroinVal)) return;
            var drugCheck = InventoryManager.DoesInventoryHaveItem(playerChar, typeof(Heroin));
            if (!CheckForCorrectAmount(heroinVal,drugCheck))
            {
                API.sendChatMessageToPlayer(sender, "You don't have enough heroin to inject!");
                return;
            }

            API.sendChatMessageToPlayer(sender, "You injected " + heroinVal + " mg of heroin.");
            InventoryManager.DeleteInventoryItem(playerChar, typeof(Heroin), heroinVal);


        }

        [Command("takemeth", GreedyArg = true, Alias = "usemeth"), Help(HelpManager.CommandGroups.General, "Take some meth pills.", "Amount of meth to take.")]
        public void meth_cmd(Client sender, String amount)
        {
            int methVal;
            Character playerChar = sender.GetCharacter();
            if (!Int32.TryParse(amount, out methVal)) return;
            var drugCheck = InventoryManager.DoesInventoryHaveItem(playerChar, typeof(Meth));
            if (!CheckForCorrectAmount(methVal,drugCheck))
            {
                API.sendChatMessageToAll("debug : " + drugCheck.Length + drugCheck[0].Amount );
                API.sendChatMessageToPlayer(sender, "You don't have enough meth to take!");
                return;

            }

            API.sendChatMessageToPlayer(sender, "You took " + methVal + " pills of meth.");
            InventoryManager.DeleteInventoryItem(playerChar, typeof(Meth), methVal);


        }

        [Command("giveAllDrug")]
        public void giveDrug(Client sender)
        {
            Character playerChar = sender.GetCharacter();
            InventoryManager.GiveInventoryItem(playerChar, new Cocaine(), 10);
            InventoryManager.GiveInventoryItem(playerChar, new Speed(), 10);
            InventoryManager.GiveInventoryItem(playerChar, new Heroin(), 10);
            InventoryManager.GiveInventoryItem(playerChar, new Weed(), 10);
            InventoryManager.GiveItemErrors x = InventoryManager.GiveInventoryItem(playerChar, new Meth(), 50);

            API.sendChatMessageToPlayer(sender,x.ToString());

            API.sendChatMessageToPlayer(sender,"done");

        }


        public bool CheckForCorrectAmount(int value, IInventoryItem[] drug)
        {
            if (value < 1 || drug.Length < 1 || drug[0].Amount < value)
            {
                return false;
            }
            return true;
        }

    }
}
