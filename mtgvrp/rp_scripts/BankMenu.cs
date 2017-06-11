using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.core;
using RoleplayServer.inventory;
using RoleplayServer.player_manager;

namespace RoleplayServer.rp_scripts
{
    public class Test : Script
    {
        public List<Vector3> Atms;
        public List<int> BankAmount;
        public Test()
        {
            API.onClientEventTrigger += OnClientEventTrigger;
            API.onEntityEnterColShape += OnEntityEnterColShapeHandler;
            API.onEntityExitColShape += OnEntityExitColShapeHandler;
            API.onEntityDataChange += OnEntityDataChange;

            Atms = new List<Vector3>
            {
                new Vector3(-1109.797f, -1690.808f, 4.375014f),
                new Vector3(-821.6062f, -1081.885f, 11.13243f),
                new Vector3(-537.8409f, -854.5145f, 29.28953f),
                new Vector3(-1315.744f, -834.6907f, 16.96173f),
                new Vector3(-1314.786f, -835.9669f, 16.96015f),
                new Vector3(-1571.018f, -547.3666f, 34.95734f),
                new Vector3(-721.1284f, -415.5296f, 34.98175f),
                new Vector3(-254.3758f, -692.4947f, 33.63751f),
                new Vector3(24.37422f, -946.0142f, 29.35756f),
                new Vector3(130.1186f, -1292.669f, 29.26953f),
                new Vector3(129.7023f, -1291.954f, 29.26953f),
                new Vector3(129.2096f, -1291.14f, 29.26953f),
                new Vector3(288.8256f, -1282.364f, 29.64128f),
                new Vector3(1077.768f, -776.4548f, 58.23997f),
                new Vector3(-867.5897f, -186.1757f, 37.84291f),
                new Vector3(-866.6556f, -187.7766f, 37.84278f),
                new Vector3(-1205.024f, -326.2916f, 37.83985f),
                new Vector3(-1570.167f, -546.7214f, 34.95663f),
                new Vector3(-57.64693f, -92.66162f, 57.77995f),
                new Vector3(527.3583f, -160.6381f, 57.0933f),
                new Vector3(-165.1658f, 234.8314f, 94.92194f),
                new Vector3(-165.1503f, 232.7887f, 94.92194f),
                new Vector3(-2072.445f, -317.3048f, 13.31597f),
                new Vector3(-3241.082f, 997.5428f, 12.55044f),
                new Vector3(-1091.462f, 2708.637f, 18.95291f),
                new Vector3(5.132f, -919.7711f, 29.55953f),
                new Vector3(-660.703f, -853.971f, 24.484f),
                new Vector3(-2293.827f, 354.817f, 174.602f),
                new Vector3(-2294.637f, 356.553f, 174.602f),
                new Vector3(-2295.377f, 358.241f, 174.648f),
                new Vector3(-1409.782f, -100.41f, 52.387f),
                new Vector3(-1410.279f, -98.649f, 52.436f),
                new Vector3(1172.492f, 2702.492f, 38.17477f),
                new Vector3(1171.537f, 2702.492f, 38.17542f),
                new Vector3(1822.637f, 3683.131f, 34.27678f),
                new Vector3(1686.753f, 4815.806f, 42.00874f),
                new Vector3(1701.209f, 6426.569f, 32.76408f),
                new Vector3(-95.54314f, 6457.19f, 31.46093f),
                new Vector3(-97.23336f, 6455.469f, 31.46682f),
                new Vector3(-386.7451f, 6046.102f, 31.50172f),
                new Vector3(-1091.42f, 2708.629f, 18.95568f)
            };
            foreach (var t in Atms)
            {
                API.createTextLabel("~g~ATM\n~w~(/atm)", t, 25.0f, 0.5f);
                API.createSphereColShape(t, 2);
            }

            // Withdraw/deposit amounts for bank menu
            BankAmount = new List<int>
            {
                100,
                200,
                300,
                400,
                500,
                600,
                700,
                800,
                900,
                1000,
                2000,
                3000,
                4000,
                5000,
                6000,
                7000,
                8000,
                9000,
                10000,
                20000,
                30000,
                40000,
                50000,
                60000,
                70000,
                80000,
                90000,
                100000,
                -1
            };
            // this is the "All" option
        }

        // Commands
        [Command("atm")]
        public void atm_cmd(Client player)
        {
            if (API.getEntitySyncedData(player, "IsNearATM") > 0)
            {
                Character character = API.shared.getEntityData(player.handle, "Character");
                ChatManager.RoleplayMessage(character, "slides in their card and inputs their PIN number.", ChatManager.RoleplayMe);
                API.sendChatMessageToPlayer(player, "~y~[Bank of Los Santos]~w~ Account balance: ~g~$" + character.BankBalance + "~w~.");
            }
        }

        private void OnEntityEnterColShapeHandler(ColShape shape, NetHandle entity)
        {
            API.setEntitySyncedData(entity, "IsNearATM", 1);
            API.setEntityData(entity, "IsNearATM_server", 1);
        }
        private void OnEntityExitColShapeHandler(ColShape shape, NetHandle entity)
        {
            API.setEntitySyncedData(entity, "IsNearATM", 0);
            API.setEntityData(entity, "IsNearATM_server", 0);
        }
        private void OnEntityDataChange(NetHandle entity, string key, object oldValue)
        {
            // incase player modifies the synced data
            if (key == "IsNearATM")
            {
                int value = API.getEntitySyncedData(entity, key);
                int realIsNearAtm = API.getEntityData(entity, "IsNearATM_server");

                if (value != realIsNearAtm)
                {
                    API.setEntitySyncedData(entity, "IsNearATM", realIsNearAtm);
                }
            }
        }
        public void OnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "OnBankMenuTrigger":
                    {
                        // check to see if player has moved away from ATM
                        if(API.getEntityData(player, "IsNearATM_server") < 1)
                        {
                            API.sendChatMessageToPlayer(player, "You have moved too far away from the ATM.");
                            return;
                        }
                        var option = (string)arguments[0];
                      
                        Character character = API.shared.getEntityData(player.handle, "Character");
                        switch (option)
                        {
                            case "Withdraw cash":
                                var withdrawIndex = (int)arguments[1];
                                if (BankAmount[withdrawIndex] > character.BankBalance)
                                {
                                    API.sendChatMessageToPlayer(player, "~r~ERROR:~w~ You do not have ~g~$" + BankAmount[withdrawIndex] + "~w~ in your account. Current balance: ~g~$" + character.BankBalance + "~w~.");
                                    break;
                                }

                                if (BankAmount[withdrawIndex] == -1)
                                {
                                    InventoryManager.GiveInventoryItem(character, new Money(), character.BankBalance);
                                    API.sendChatMessageToPlayer(player, "~y~[Bank of Los Santos]~w~ You have withdrawn ~g~$" + character.BankBalance + "~w~. New balance: ~g~$0~w~.");
                                    character.BankBalance = 0;
                                }
                                else
                                {
                                    character.BankBalance -= BankAmount[withdrawIndex];
                                    InventoryManager.GiveInventoryItem(character, new Money(), BankAmount[withdrawIndex]);
                                    API.sendChatMessageToPlayer(player, "~y~[Bank of Los Santos]~w~ You have withdrawn ~g~$" + BankAmount[withdrawIndex] + "~w~. New balance: ~g~$" + character.BankBalance + "~w~.");
                                }
                                
                                //character.save();
                                break;
                            case "Deposit cash":
                                var depositIndex = (int)arguments[1];
                                if (BankAmount[depositIndex] > Money.GetCharacterMoney(character))
                                {
                                    API.sendChatMessageToPlayer(player, "~r~ERROR:~w~ You do not have ~g~$" + BankAmount[depositIndex] + "~w~ on hand. Current money on hand: ~g~$" + Money.GetCharacterMoney(character) + "~w~.");
                                    break;
                                }

                                if (BankAmount[depositIndex] == -1)
                                {
                                    character.BankBalance += Money.GetCharacterMoney(character);
                                    API.sendChatMessageToPlayer(player, "~y~[Bank of Los Santos]~w~ You have deposited ~g~$" + Money.GetCharacterMoney(character) + "~w~. New balance: ~g~$" + character.BankBalance + "~w~.");
                                    InventoryManager.SetInventoryAmmount(character, typeof(Money), 0);
                                }
                                else
                                {
                                    character.BankBalance += BankAmount[depositIndex];
                                    InventoryManager.DeleteInventoryItem(character, typeof(Money),
                                        BankAmount[depositIndex]);
                                    API.sendChatMessageToPlayer(player, "~y~[Bank of Los Santos]~w~ You have deposited ~g~$" + BankAmount[depositIndex] + "~w~. New balance: ~g~$" + character.BankBalance + "~w~.");
                                }

                                
                                //character.save();
                                break;
                        }
                    }
                    break;
            }
        }
    }
}
