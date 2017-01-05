using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;

namespace RoleplayServer
{
    public class Test : Script
    {
        private List<Vector3> ATMs;
        private List<int> Bank_Amount;
        public Test()
        {
            API.onClientEventTrigger += OnClientEventTrigger;
            API.onEntityEnterColShape += OnEntityEnterColShapeHandler;
            API.onEntityExitColShape += OnEntityExitColShapeHandler;
            API.onEntityDataChange += OnEntityDataChange;

            ATMs = new List<Vector3>();
            ATMs.Add(new Vector3(-1109.797f, -1690.808f, 4.375014f));
            ATMs.Add(new Vector3(-821.6062f, -1081.885f, 11.13243f));
            ATMs.Add(new Vector3(-537.8409f, -854.5145f, 29.28953f));
            ATMs.Add(new Vector3(-1315.744f, -834.6907f, 16.96173f));
            ATMs.Add(new Vector3(-1314.786f, -835.9669f, 16.96015f));
            ATMs.Add(new Vector3(-1571.018f, -547.3666f, 34.95734f));
            ATMs.Add(new Vector3(-721.1284f, -415.5296f, 34.98175f));
            ATMs.Add(new Vector3(-254.3758f, -692.4947f, 33.63751f));
            ATMs.Add(new Vector3(24.37422f, -946.0142f, 29.35756f));
            ATMs.Add(new Vector3(130.1186f, -1292.669f, 29.26953f));
            ATMs.Add(new Vector3(129.7023f, -1291.954f, 29.26953f));
            ATMs.Add(new Vector3(129.2096f, -1291.14f, 29.26953f));
            ATMs.Add(new Vector3(288.8256f, -1282.364f, 29.64128f));
            ATMs.Add(new Vector3(1077.768f, -776.4548f, 58.23997f));
            ATMs.Add(new Vector3(-867.5897f, -186.1757f, 37.84291f));
            ATMs.Add(new Vector3(-866.6556f, -187.7766f, 37.84278f));
            ATMs.Add(new Vector3(-1205.024f, -326.2916f, 37.83985f));
            ATMs.Add(new Vector3(-1570.167f, -546.7214f, 34.95663f));
            ATMs.Add(new Vector3(-57.64693f, -92.66162f, 57.77995f));
            ATMs.Add(new Vector3(527.3583f, -160.6381f, 57.0933f));
            ATMs.Add(new Vector3(-165.1658f, 234.8314f, 94.92194f));
            ATMs.Add(new Vector3(-165.1503f, 232.7887f, 94.92194f));
            ATMs.Add(new Vector3(-2072.445f, -317.3048f, 13.31597f));
            ATMs.Add(new Vector3(-3241.082f, 997.5428f, 12.55044f));
            ATMs.Add(new Vector3(-1091.462f, 2708.637f, 18.95291f));
            ATMs.Add(new Vector3(5.132f, -919.7711f, 29.55953f));
            ATMs.Add(new Vector3(-660.703f, -853.971f, 24.484f));
            ATMs.Add(new Vector3(-2293.827f, 354.817f, 174.602f));
            ATMs.Add(new Vector3(-2294.637f, 356.553f, 174.602f));
            ATMs.Add(new Vector3(-2295.377f, 358.241f, 174.648f));
            ATMs.Add(new Vector3(-1409.782f, -100.41f, 52.387f));
            ATMs.Add(new Vector3(-1410.279f, -98.649f, 52.436f));
            ATMs.Add(new Vector3(1172.492f, 2702.492f, 38.17477f));
            ATMs.Add(new Vector3(1171.537f, 2702.492f, 38.17542f));
            ATMs.Add(new Vector3(1822.637f, 3683.131f, 34.27678f));
            ATMs.Add(new Vector3(1686.753f, 4815.806f, 42.00874f));
            ATMs.Add(new Vector3(1701.209f, 6426.569f, 32.76408f));
            ATMs.Add(new Vector3(-95.54314f, 6457.19f, 31.46093f));
            ATMs.Add(new Vector3(-97.23336f, 6455.469f, 31.46682f));
            ATMs.Add(new Vector3(-386.7451f, 6046.102f, 31.50172f));
            ATMs.Add(new Vector3(-1091.42f, 2708.629f, 18.95568f));
            for (int i = 0; i < ATMs.Count; i++)
            {
                API.createTextLabel("~g~ATM\n~w~(/atm)", ATMs[i], 25.0f, 0.5f);
                API.createSphereColShape(ATMs[i], 2);
            }

            // Withdraw/deposit amounts for bank menu
            Bank_Amount = new List<int>();
            Bank_Amount.Add(100);
            Bank_Amount.Add(200);
            Bank_Amount.Add(300);
            Bank_Amount.Add(400);
            Bank_Amount.Add(500);
            Bank_Amount.Add(600);
            Bank_Amount.Add(700);
            Bank_Amount.Add(800);
            Bank_Amount.Add(900);
            Bank_Amount.Add(1000);
            Bank_Amount.Add(2000);
            Bank_Amount.Add(3000);
            Bank_Amount.Add(4000);
            Bank_Amount.Add(5000);
            Bank_Amount.Add(6000);
            Bank_Amount.Add(7000);
            Bank_Amount.Add(8000);
            Bank_Amount.Add(9000);
            Bank_Amount.Add(10000);
            Bank_Amount.Add(20000);
            Bank_Amount.Add(30000);
            Bank_Amount.Add(40000);
            Bank_Amount.Add(50000);
            Bank_Amount.Add(60000);
            Bank_Amount.Add(70000);
            Bank_Amount.Add(80000);
            Bank_Amount.Add(90000);
            Bank_Amount.Add(100000);
            Bank_Amount.Add(-1); // this is the "All" option
        }

        // Commands
        [Command("atm")]
        public void atm_cmd(Client player)
        {
            if (API.getEntitySyncedData(player, "IsNearATM") > 0)
            {
                Character character = API.shared.getEntityData(player.handle, "Character");
                ChatManager.RoleplayMessage(character, "slides in their card and inputs their PIN number.", ChatManager.ROLEPLAY_ME, 10);
                API.sendChatMessageToPlayer(player, "~y~[Bank of Los Santos]~w~ Account balance: ~g~$" + character.bank_balance + "~w~.");
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
                int realIsNearATM = API.getEntityData(entity, "IsNearATM_server");

                if (value != realIsNearATM)
                {
                    API.setEntitySyncedData(entity, "IsNearATM", realIsNearATM);
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
                        string option = (string)(arguments[0]);
                      
                        Character character = API.shared.getEntityData(player.handle, "Character");
                        switch (option)
                        {
                            case "Withdraw cash":
                                int withdraw_index = (int)arguments[1];
                                if (Bank_Amount[withdraw_index] > character.bank_balance)
                                {
                                    API.sendChatMessageToPlayer(player, "~r~ERROR:~w~ You do not have ~g~$" + Bank_Amount[withdraw_index] + "~w~ in your account. Current balance: ~g~$" + character.bank_balance + "~w~.");
                                    break;
                                }

                                if (Bank_Amount[withdraw_index] == -1)
                                {
                                    character.money += character.bank_balance;      
                                    API.sendChatMessageToPlayer(player, "~y~[Bank of Los Santos]~w~ You have withdrawn ~g~$" + character.bank_balance + "~w~. New balance: ~g~$0~w~.");
                                    character.bank_balance = 0;
                                }
                                else
                                {
                                    character.bank_balance -= Bank_Amount[withdraw_index];
                                    character.money += Bank_Amount[withdraw_index];
                                    API.sendChatMessageToPlayer(player, "~y~[Bank of Los Santos]~w~ You have withdrawn ~g~$" + Bank_Amount[withdraw_index] + "~w~. New balance: ~g~$" + character.bank_balance + "~w~.");
                                }
                                
                                //character.save();
                                break;
                            case "Deposit cash":
                                int deposit_index = (int)arguments[1];
                                if (Bank_Amount[deposit_index] > character.money)
                                {
                                    API.sendChatMessageToPlayer(player, "~r~ERROR:~w~ You do not have ~g~$" + Bank_Amount[deposit_index] + "~w~ on hand. Current money on hand: ~g~$" + character.money + "~w~.");
                                    break;
                                }

                                if (Bank_Amount[deposit_index] == -1)
                                {
                                    character.bank_balance += character.money;
                                    API.sendChatMessageToPlayer(player, "~y~[Bank of Los Santos]~w~ You have deposited ~g~$" + character.money + "~w~. New balance: ~g~$" + character.bank_balance + "~w~.");
                                    character.money = 0;
                                }
                                else
                                {
                                    character.bank_balance += Bank_Amount[deposit_index];
                                    character.money -= Bank_Amount[deposit_index];
                                    API.sendChatMessageToPlayer(player, "~y~[Bank of Los Santos]~w~ You have deposited ~g~$" + Bank_Amount[deposit_index] + "~w~. New balance: ~g~$" + character.bank_balance + "~w~.");
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
