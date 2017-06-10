using System;
using System.Security.Cryptography;
using GTANetworkServer;
using MongoDB.Driver;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources.player_manager.login
{
    class LoginManager : Script
    {
        public static int LoginDimension = 100;

        public static RNGCryptoServiceProvider Randomizer = new RNGCryptoServiceProvider();

        public LoginManager()
        {
            DebugManager.DebugMessage("[LoginM] Initalizing Login Manager...");

            API.onPlayerConnected += OnPlayerConnected;
            API.onPlayerFinishedDownload += OnPlayerFinishedDownload;
            API.onClientEventTrigger += API_onClientEventTrigger;

            //API.onChatCommand += OnChatCommandHandler;
            API.onChatMessage += OnPlayerChat;

            DebugManager.DebugMessage("[LoginM] Login Manager initalized.");
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "create_admin_pin":
                {
                    var adminPin = Convert.ToString(arguments[0]);

                    if (adminPin.Length != 6)
                    {
                        API.sendChatMessageToPlayer(player, Color.AdminOrange,
                            "Your pin must be exactly 6 characters long.");
                        API.triggerClientEvent(player, "create_admin_pin");
                    }

                    Account account = API.getEntityData(player.handle, "Account");

                    if (account.AdminLevel < 1)
                    {
                        API.sendChatMessageToPlayer(player, Color.AdminOrange, "You are not an admin anymore.");
                        prepare_character_menu(player);
                        return;
                    }

                    account.AdminPin = adminPin;
                    account.Save();
                    API.sendChatMessageToPlayer(player, Color.AdminOrange, "Admin pin successully set to: " + adminPin);
                    prepare_character_menu(player);
                    break;
                }
                case "admin_pin_check":
                {
                    var adminPin = Convert.ToString(arguments[0]);

                    Account account = API.getEntityData(player, "Account");

                    if (account.AdminLevel == 0)
                    {
                        API.sendChatMessageToPlayer(player, Color.AdminOrange, "You are not an admin anymore.");
                        prepare_character_menu(player);
                        return;
                    }

                    if (account.AdminPin.Equals(adminPin))
                    {
                        API.sendChatMessageToPlayer(player, Color.AdminOrange, "You have successfully logged in.");
                        prepare_character_menu(player);
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, Color.AdminOrange, "Incorrect pin.");
                        //TO DO: SEND TO ADMIN THEY GOT IT WRONG 
                        API.triggerClientEvent(player, "admin_pin_check");
                    }
                    break;
                }
                case "attempt_login":
                {
                    var inputPass = (string) arguments[0];


                    if (inputPass.Length < 8)
                    {
                        API.triggerClientEvent(player, "login_error", "Password entered is too short.");
                        return;
                    }

                    Account account = API.getEntityData(player.handle, "Account");

                    if (account.is_registered())
                    {
                        if (account.IsLoggedIn)
                        {
                            API.triggerClientEvent(player, "login_error", "You are already logged in.");
                            return;
                        }

                        account.load_by_name();

                        inputPass = inputPass + account.Salt;

                        //Hash password
                        var password = System.Text.Encoding.UTF8.GetBytes(inputPass);
                        password = new SHA256Managed().ComputeHash(password);
                        var hashedPass = System.Text.Encoding.UTF8.GetString(password);

                        if (hashedPass == account.Password)
                        {
                            int result = DateTime.Compare(account.TempBanExpiration, DateTime.Now);

                            if (account.IsTempbanned == true && result == 1)
                            {
                                API.sendChatMessageToPlayer(player, "~r~You are temp-banned from this server. You will be unbanned in " + (account.TempBanExpiration - DateTime.Now).TotalDays + " days.");
                                API.sendNotificationToPlayer(player, "~r~You are temp-banned from this server. You will be unbanned in " + (account.TempBanExpiration - DateTime.Now).TotalDays + " days.");
                                API.kickPlayer(player);
                                AdminSystem.AdminCommands.sendtoAllAdmins(account.AccountName + "attempted to log in to a temp-banned account.");
                                return;
                            }
                            if (account.IsBanned == true)
                            {
                                API.sendChatMessageToPlayer(player, "~r~You are banned from this server. Visit MT-Gaming.com to submit an unban appeal. ");
                                API.sendNotificationToPlayer(player, "~r~You are banned from this server. Visit MT-Gaming.com to submit an unban appeal.");
                                API.kickPlayer(player);
                                AdminSystem.AdminCommands.sendtoAllAdmins(account.AccountName + "attempted to log in to a banned account.");
                                return;
                            }

                               
                            API.sendChatMessageToPlayer(player, "~g~ You have successfully logged in!");

                            account.IsLoggedIn = true;

                            if (account.AdminLevel > 0)
                            {
                                API.sendChatMessageToPlayer(player, Color.AdminOrange,
                                    "Welcome back Admin " + account.AdminName);
                                    API.shared.triggerClientEvent(player, "hide_login_browser");

                                if (account.AdminPin.Equals(string.Empty))
                                {
                                    API.sendChatMessageToPlayer(player, Color.AdminOrange,
                                        "You do not have an admin pin set. Please choose one now: ");
                                    API.triggerClientEvent(player, "create_admin_pin");
                                }
                                else
                                {
                                    API.sendChatMessageToPlayer(player, Color.AdminOrange,
                                        "Pleae login with your admin pin to continue.");
                                    API.triggerClientEvent(player, "admin_pin_check");
                                }
                               
                            }
                            else
                            {
                                prepare_character_menu(player);
                            }
                        }
                        else
                        {
                            API.triggerClientEvent(player, "login_error", "Incorrect password");
                        }
                    }
                    else
                    {
                        if (inputPass.Length < 8)
                        {
                            API.triggerClientEvent(player,
                                "Please choose a password that is at least 8 characters long.");
                            return;
                        }

                        if (account.IsLoggedIn)
                        {
                            API.triggerClientEvent(player, "ERROR: You are already logged in!");
                            return;
                        }

                        //Get a random salt
                        var salt = new byte[32];
                        Randomizer.GetBytes(salt);

                        //Add salt to the end of input_pass
                        inputPass = inputPass + System.Text.Encoding.UTF8.GetString(salt);

                        //Convert inputted password to bytes
                        var password = System.Text.Encoding.UTF8.GetBytes(inputPass);
                        password = new SHA256Managed().ComputeHash(password);

                        account.Password = System.Text.Encoding.UTF8.GetString(password);
                        account.Salt = System.Text.Encoding.UTF8.GetString(salt);

                        account.Register();

                        API.sendChatMessageToPlayer(player,
                            "You have successfully registered! Please select a character slot below to get started!");
                        prepare_character_menu(player);
                    }
                        break;
                }
            }
        }

        public void OnPlayerChat(Client player, string message, CancelEventArgs e)
        {
            Account account = player.GetAccount();
            if(account.IsLoggedIn == false)
            {
                e.Cancel = true;
            }
        }

        public void OnPlayerConnected(Client player)
        {
            DebugManager.DebugMessage("[LoginM] " + player.name + " has connected to the server. (IP: " + player.address + ")");
        }

        public void OnPlayerFinishedDownload(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
      
            API.setEntityDimension(player.handle, player.handle.Value);
            
            if(account.is_registered())
            {
                API.sendChatMessageToPlayer(player, "This account is already registered. Use /login [password] to continue to character selection.");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "This account is unregistered! Use /register [password] to register it.");
            }

            API.triggerClientEvent(player, "onPlayerConnectedEx", account.is_registered());
        }

       
        [Command("login")]
        public void login_cmd(Client player, string inputPass)
        {
            if (inputPass.Length < 8)
            {
                API.sendChatMessageToPlayer(player, "The password you entered to log in is too short.");
                return;
            }

            Account account = API.getEntityData(player.handle, "Account");

            if (!account.is_registered())
            {
                API.sendChatMessageToPlayer(player, "~r~ ERROR: You are not registered~");
                return;
            }

            if (account.IsLoggedIn)
            {
                API.sendChatMessageToPlayer(player, "~r~ ERROR: You are already logged in!");
                return;
            }

            account.load_by_name();

            inputPass = inputPass + account.Salt;

            //Hash password
            var password = System.Text.Encoding.UTF8.GetBytes(inputPass);
            password = new SHA256Managed().ComputeHash(password);
            var hashedPass = System.Text.Encoding.UTF8.GetString(password);

            if (hashedPass == account.Password)
            {
                int result = DateTime.Compare(account.TempBanExpiration, DateTime.Now);

                if (account.IsTempbanned == true && result == 1)
                {
                    API.sendChatMessageToPlayer(player, "~r~You are temp-banned from this server. You will be unbanned in " + (account.TempBanExpiration - DateTime.Now).TotalDays + " days.");
                    API.sendNotificationToPlayer(player, "~r~You are temp-banned from this server. You will be unbanned in " + (account.TempBanExpiration - DateTime.Now).TotalDays + " days.");
                    API.kickPlayer(player);
                    AdminSystem.AdminCommands.sendtoAllAdmins(account.AccountName + "attempted to log in to a temp-banned account.");
                    return;
                }
                if (account.IsBanned == true)
                {
                    API.sendChatMessageToPlayer(player, "~r~You are banned from this server for the following reason: ");
                    API.sendChatMessageToPlayer(player, account.BanReason);
                    API.sendNotificationToPlayer(player, "~r~You are banned from this server for the following reason: ");
                    API.sendNotificationToPlayer(player, account.BanReason);
                    API.kickPlayer(player);
                    AdminSystem.AdminCommands.sendtoAllAdmins(account.AccountName + "attempted to log in to a banned account.");
                    return;
                }

                API.sendChatMessageToPlayer(player, "~g~ You have successfully logged in!");

                account.IsLoggedIn = true;

                if (account.AdminLevel > 0)
                {
                    API.sendChatMessageToPlayer(player, Color.AdminOrange, "Welcome back Admin " + account.AdminName);
                }

                prepare_character_menu(player);
            }
            else
            {
                API.sendChatMessageToPlayer(player, "~r~ ERROR: Incorrect password entered.");
            }

        }


        [Command("register", GreedyArg =true)]
        public void register_cmd(Client player, string inputPass)
        {
            if(inputPass.Length < 8)
            {
                API.sendChatMessageToPlayer(player, "Please choose a password that is at least 6 characters long.");
                return;
            }

            Account account = API.getEntityData(player.handle, "Account");

            if (account.is_registered())
            {
                API.sendChatMessageToPlayer(player, "~r~ ERROR: You are already registered~");
                return;
            }

            if (account.IsLoggedIn)
            {
                API.sendChatMessageToPlayer(player, "~r~ ERROR: You are already logged in!");
                return;
            }

            //Get a random salt
            var salt = new byte[32];
            Randomizer.GetBytes(salt);

            //Add salt to the end of input_pass
            inputPass = inputPass + System.Text.Encoding.UTF8.GetString(salt);

            //Convert inputted password to bytes
            var password = System.Text.Encoding.UTF8.GetBytes(inputPass);
            password = new SHA256Managed().ComputeHash(password);

            account.Password = System.Text.Encoding.UTF8.GetString(password);
            account.Salt = System.Text.Encoding.UTF8.GetString(salt);

            account.Register();

            API.sendChatMessageToPlayer(player, "You have successfully registered! Please select a character slot below to get started!");
            prepare_character_menu(player);
        }


        public static void prepare_character_menu(Client player)
        {
            API.shared.triggerClientEvent(player, "hide_login_browser");

            Account account = API.shared.getEntityData(player.handle, "Account");

            var filter = Builders<Character>.Filter.Eq("AccountId", account.Id.ToString());
            var charactersFound = DatabaseManager.CharacterTable.Find(filter).ToList();

            var charCount = 0;

            foreach(var c in charactersFound)
            {
                API.shared.setEntitySyncedData(player.handle, "char_name_" + charCount, c.CharacterName);
                API.shared.setEntitySyncedData(player.handle, "char_info_" + charCount, "SQL: " + c.Id);
                charCount++;
            }

            API.shared.setEntitySyncedData(player.handle, "char_name_" + charCount, "Create new character");
            API.shared.setEntitySyncedData(player.handle, "char_info_" + charCount, "Begin the creation of a new character");
            charCount++;

            API.shared.setEntitySyncedData(player.handle, "char_count", charCount);
            API.shared.triggerClientEvent(player, "showCharacterSelection");
        }
    }
}
