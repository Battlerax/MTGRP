using System;
using System.Security.Cryptography;

using GTANetworkAPI;

using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.database_manager;
using MongoDB.Driver;

using Color = mtgvrp.core.Color;

namespace mtgvrp.player_manager.login
{
    class LoginManager : Script
    {
        public static int LoginDimension = 100;

        public static RNGCryptoServiceProvider Randomizer = new RNGCryptoServiceProvider();

        public LoginManager()
        {
            DebugManager.DebugMessage("[LoginM] Initalizing Login Manager...");

            Event.OnPlayerConnected += OnPlayerConnected;
            Event.OnClientEventTrigger += API_onClientEventTrigger;

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
                        API.SendChatMessageToPlayer(player, Color.AdminOrange,
                            "Your pin must be exactly 6 characters long.");
                        API.TriggerClientEvent(player, "create_admin_pin");
                    }

                    Account account = player.GetAccount();

                    if (account.AdminLevel < 1)
                    {
                        API.SendChatMessageToPlayer(player, Color.AdminOrange, "You are not an admin anymore.");
                        prepare_character_menu(player);
                        return;
                    }

                    account.AdminPin = adminPin;
                    account.Save();
                    API.SendChatMessageToPlayer(player, Color.AdminOrange, "Admin pin successully set to: " + adminPin);
                    prepare_character_menu(player);
                    break;
                }
                case "admin_pin_check":
                {
                    var adminPin = Convert.ToString(arguments[0]);

                    Account account = player.GetAccount();

                    if (account.AdminLevel == 0)
                    {
                        API.SendChatMessageToPlayer(player, Color.AdminOrange, "You are not an admin anymore.");
                        prepare_character_menu(player);
                        return;
                    }

                    if (account.AdminPin.Equals(adminPin))
                    {
                        API.SendChatMessageToPlayer(player, Color.AdminOrange, "You have successfully logged in.");
                        prepare_character_menu(player);
                    }
                    else
                    {
                        API.SendChatMessageToPlayer(player, Color.AdminOrange, "Incorrect pin.");
                        //TO DO: SEND TO ADMIN THEY GOT IT WRONG 
                        API.TriggerClientEvent(player, "admin_pin_check");
                    }
                    break;
                }
                case "attempt_login":
                {
                    var inputPass = (string) arguments[0];


                    if (inputPass.Length < 8)
                    {
                        API.TriggerClientEvent(player, "login_error", "Password entered is too short.");
                        return;
                    }

                    Account account = player.GetAccount();

                    if (account.is_registered())
                    {
                        if (account.IsLoggedIn)
                        {
                            API.TriggerClientEvent(player, "login_error", "You are already logged in.");
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
                            if (account.IsTempbanned == true && account.TempBanExpiration < DateTime.Now)
                            {
                                API.SendChatMessageToPlayer(player, "~r~You are temp-banned from this server. You will be unbanned in " + (account.TempBanExpiration - DateTime.Now).TotalDays + " days.");
                                API.SendNotificationToPlayer(player, "~r~You are temp-banned from this server. You will be unbanned in " + (account.TempBanExpiration - DateTime.Now).TotalDays + " days.");
                                API.KickPlayer(player);
                                AdminSystem.AdminCommands.SendtoAllAdmins(account.AccountName + "attempted to log in to a temp-banned account.");
                                return;
                            }
                            if (account.IsBanned == true)
                            {
                                API.SendChatMessageToPlayer(player, "~r~You are banned from this server. Visit MT-Gaming.com to submit an unban appeal. ");
                                API.SendNotificationToPlayer(player, "~r~You are banned from this server. Visit MT-Gaming.com to submit an unban appeal.");
                                API.KickPlayer(player);
                                AdminSystem.AdminCommands.SendtoAllAdmins(account.AccountName + "attempted to log in to a banned account.");
                                return;
                            }

                               
                            API.SendChatMessageToPlayer(player, "~g~ You have successfully logged in!");
                            LogManager.Log(LogManager.LogTypes.Connection, player.SocialClubName + " has logged in to the server. (IP: " + player.Address + ")");

                                account.IsLoggedIn = true;

                            if (account.AdminLevel > 0)
                            {
                                API.SendChatMessageToPlayer(player, Color.AdminOrange,
                                    "Welcome back Admin " + account.AdminName);
                                API.Shared.TriggerClientEvent(player, "hide_login_browser");

                                if (account.AdminPin.Equals(string.Empty))
                                {
                                    API.SendChatMessageToPlayer(player, Color.AdminOrange,
                                        "You do not have an admin pin set. Please choose one now: ");
                                    API.TriggerClientEvent(player, "create_admin_pin");
                                }
                                else
                                {
                                    API.SendChatMessageToPlayer(player, Color.AdminOrange,
                                        "Pleae login with your admin pin to continue.");
                                    API.TriggerClientEvent(player, "admin_pin_check");
                                }
                               
                            }
                            else
                            {
                                prepare_character_menu(player);
                            }
                        }
                        else
                        {
                            API.TriggerClientEvent(player, "login_error", "Incorrect password");
                        }
                    }
                    else
                    {
                        if (inputPass.Length < 8)
                        {
                            API.TriggerClientEvent(player,
                                "Please choose a password that is at least 8 characters long.");
                            return;
                        }

                        if (account.IsLoggedIn)
                        {
                            API.TriggerClientEvent(player, "ERROR: You are already logged in!");
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

                        API.SendChatMessageToPlayer(player,
                            "You have successfully registered! Please select a character slot below to get started!");
                        LogManager.Log(LogManager.LogTypes.Connection, player.SocialClubName + " has registered in to the server. (IP: " + player.Address + ")");
                            prepare_character_menu(player);
                    }
                        break;
                }
            }
        }

        public void OnPlayerConnected(Client player, CancelEventArgs e)
        {
            DebugManager.DebugMessage("[LoginM] " + player.SocialClubName + " has connected to the server [NOT LOGGED IN]. (IP: " + player.Address + ")");
            LogManager.Log(LogManager.LogTypes.Connection, player.SocialClubName + " has connected to the server [NOT LOGGED IN]. (IP: " + player.Address + ")");

            Account account = player.GetAccount();
            API.SetEntitySharedData(player, "REG_DIMENSION", 1000);
            API.SetEntityDimension(player, 1000);

            if (account.is_registered())
            {
                API.SendChatMessageToPlayer(player, "This account is already registered. Use /login [password] to continue to character selection.");
            }
            else
            {
                API.SendChatMessageToPlayer(player, "This account is unregistered! Use /register [password] to register it.");
            }

            API.SendChatMessageToPlayer(player, "Press ~g~F12~w~ to disable CEF and login manually.");
            API.TriggerClientEvent(player, "onPlayerConnectedEx", account.is_registered());
        }
       
        [Command("login"), Help(HelpManager.CommandGroups.General, "Used to login.", "Your password")]
        public void login_cmd(Client player, string inputPass)
        {
            if (inputPass.Length < 8)
            {
                API.SendChatMessageToPlayer(player, "The password you entered to log in is too short.");
                return;
            }

            Account account = player.GetAccount();

            if (!account.is_registered())
            {
                API.SendChatMessageToPlayer(player, "~r~ ERROR: You are not registered~");
                return;
            }

            if (account.IsLoggedIn)
            {
                API.SendChatMessageToPlayer(player, "~r~ ERROR: You are already logged in!");
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
                if (account.IsTempbanned == true && account.TempBanExpiration < DateTime.Now)
                {
                    API.SendChatMessageToPlayer(player, "~r~You are temp-banned from this server. You will be unbanned in " + (account.TempBanExpiration - DateTime.Now).TotalDays + " days.");
                    API.SendNotificationToPlayer(player, "~r~You are temp-banned from this server. You will be unbanned in " + (account.TempBanExpiration - DateTime.Now).TotalDays + " days.");
                    API.KickPlayer(player);
                    AdminSystem.AdminCommands.SendtoAllAdmins(account.AccountName + "attempted to log in to a temp-banned account.");
                    return;
                }
                if (account.IsBanned == true)
                {
                    API.SendChatMessageToPlayer(player, "~r~You are banned from this server for the following reason: ");
                    API.SendChatMessageToPlayer(player, account.BanReason);
                    API.SendNotificationToPlayer(player, "~r~You are banned from this server for the following reason: ");
                    API.SendNotificationToPlayer(player, account.BanReason);
                    API.KickPlayer(player);
                    AdminSystem.AdminCommands.SendtoAllAdmins(account.AccountName + "attempted to log in to a banned account.");
                    return;
                }

                API.SendChatMessageToPlayer(player, "~g~ You have successfully logged in!");

                account.IsLoggedIn = true;

                if (account.AdminLevel > 0)
                {
                    API.SendChatMessageToPlayer(player, Color.AdminOrange,
                        "Welcome back Admin " + account.AdminName);
                    API.Shared.TriggerClientEvent(player, "hide_login_browser");

                    if (account.AdminPin.Equals(string.Empty))
                    {
                        API.SendChatMessageToPlayer(player, Color.AdminOrange,
                            "You do not have an admin pin set. Please choose one now: ");
                        API.TriggerClientEvent(player, "create_admin_pin");
                    }
                    else
                    {
                        API.SendChatMessageToPlayer(player, Color.AdminOrange,
                            "Please login with your admin pin to continue.");
                        API.TriggerClientEvent(player, "admin_pin_check");
                    }

                }
                else
                {
                    prepare_character_menu(player);
                }
            }
            else
            {
                API.SendChatMessageToPlayer(player, "~r~ ERROR: Incorrect password entered.");
            }

        }


        [Command("register", GreedyArg =true), Help(HelpManager.CommandGroups.General, "Used to create an account", "Your desired password")]
        public void register_cmd(Client player, string inputPass)
        {
            if(inputPass.Length < 8)
            {
                API.SendChatMessageToPlayer(player, "Please choose a password that is at least 6 characters long.");
                return;
            }

            Account account = player.GetAccount();

            if (account.is_registered())
            {
                API.SendChatMessageToPlayer(player, "~r~ ERROR: You are already registered~");
                return;
            }

            if (account.IsLoggedIn)
            {
                API.SendChatMessageToPlayer(player, "~r~ ERROR: You are already logged in!");
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

            API.SendChatMessageToPlayer(player, "You have successfully registered! Please select a character slot below to get started!");
            prepare_character_menu(player);
        }


        public static void prepare_character_menu(Client player)
        {
            API.Shared.TriggerClientEvent(player, "hide_login_browser");

            Account account = player.GetAccount();

            var filter = Builders<Character>.Filter.Eq("AccountId", account.Id.ToString());
            var charactersFound = DatabaseManager.CharacterTable.Find(filter).ToList();

            var charCount = 0;

            foreach(var c in charactersFound)
            {
                API.Shared.SetEntitySharedData(player.Handle, "char_name_" + charCount, c.CharacterName);
                API.Shared.SetEntitySharedData(player.Handle, "char_info_" + charCount, "SQL: " + c.Id);
                charCount++;
            }

            API.Shared.SetEntitySharedData(player.Handle, "char_name_" + charCount, "Create new character");
            API.Shared.SetEntitySharedData(player.Handle, "char_info_" + charCount, "Begin the creation of a new character");
            charCount++;

            API.Shared.SetEntitySharedData(player.Handle, "char_count", charCount);
            API.Shared.TriggerClientEvent(player, "showCharacterSelection");
        }
    }
}
