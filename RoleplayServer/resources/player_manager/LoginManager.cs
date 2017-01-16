using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Security.Cryptography;
using MongoDB.Driver;
using System.Collections.Generic;

namespace RoleplayServer
{
    class LoginManager : Script
    {
        public static int LOGIN_DIMENSION = 100;

        public static RNGCryptoServiceProvider randomizer = new RNGCryptoServiceProvider();

        public LoginManager()
        {
            DebugManager.debugMessage("[LoginM] Initalizing Login Manager...");

            API.onPlayerConnected += OnPlayerConnected;
            API.onPlayerFinishedDownload += OnPlayerFinishedDownload;

            //API.onChatCommand += OnChatCommandHandler;
            API.onChatMessage += OnPlayerChat;

            DebugManager.debugMessage("[LoginM] Login Manager initalized.");
        }

        public void OnPlayerChat(Client player, string message, CancelEventArgs e)
        {
            Account account = API.getEntityData(player, "Account");
            if(account.is_logged_in == false)
            {
                e.Cancel = true;
            }
        }

        public void OnPlayerConnected(Client player)
        {
            DebugManager.debugMessage("[LoginM] " + player.name + " has connected to the server. (IP: " + player.address + ")");
        }

        public void OnPlayerFinishedDownload(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
      
            API.setEntityDimension(player.handle, LOGIN_DIMENSION);
            
            if(account.is_registered() == true)
            {
                API.sendChatMessageToPlayer(player, "This account is already registered. Use /login [password] to continue to character selection.");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "This account is unregistered! Use /register [password] to register it.");
            }

            API.triggerClientEvent(player, "onPlayerConnectedEx");
        }

       
        [Command("login")]
        public void login_cmd(Client player, string input_pass)
        {
            if (input_pass.Length < 8)
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

            if (account.is_logged_in)
            {
                API.sendChatMessageToPlayer(player, "~r~ ERROR: You are already logged in!");
                return;
            }

            account.load_by_name();

            input_pass = input_pass + account.salt;

            //Hash password
            byte[] password = System.Text.Encoding.UTF8.GetBytes(input_pass);
            password = new SHA256Managed().ComputeHash(password);
            string hashed_pass = System.Text.Encoding.UTF8.GetString(password);

            if (hashed_pass == account.password)
            {
                API.sendChatMessageToPlayer(player, "~g~ You have successfully logged in!");

                account.is_logged_in = true;

                if (account.admin_level > 0)
                {
                    API.sendChatMessageToPlayer(player, Color.AdminOrange, "Welcome back Admin " + account.admin_name);
                }

                prepare_character_menu(player);
            }
            else
            {
                API.sendChatMessageToPlayer(player, "~r~ ERROR: Incorrect password entered.");
            }

        }


        [Command("register", GreedyArg =true)]
        public void register_cmd(Client player, string input_pass)
        {
            if(input_pass.Length < 8)
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

            if (account.is_logged_in)
            {
                API.sendChatMessageToPlayer(player, "~r~ ERROR: You are already logged in!");
                return;
            }

            //Get a random salt
            byte[] salt = new byte[32];
            randomizer.GetBytes(salt);

            //Add salt to the end of input_pass
            input_pass = input_pass + System.Text.Encoding.UTF8.GetString(salt);

            //Convert inputted password to bytes
            byte[] password = System.Text.Encoding.UTF8.GetBytes(input_pass);
            password = new SHA256Managed().ComputeHash(password);

            account.password = System.Text.Encoding.UTF8.GetString(password);
            account.salt = System.Text.Encoding.UTF8.GetString(salt);

            account.register();

            API.sendChatMessageToPlayer(player, "You have successfully registered! Please select a character slot below to get started!");
            prepare_character_menu(player);
            return;
        }


        public static void prepare_character_menu(Client player)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            FilterDefinition<Character> filter = Builders<Character>.Filter.Eq("account_id", account._id.ToString());
            List<Character> characters_found = DatabaseManager.character_table.Find(filter).ToList<Character>();

            int char_count = 0;

            foreach(Character c in characters_found)
            {
                API.shared.setEntitySyncedData(player.handle, "char_name_" + char_count, c.character_name);
                API.shared.setEntitySyncedData(player.handle, "char_info_" + char_count, "SQL: " + c._id);
                char_count++;
            }

            API.shared.setEntitySyncedData(player.handle, "char_name_" + char_count, "Create new character");
            API.shared.setEntitySyncedData(player.handle, "char_info_" + char_count, "Begin the creation of a new character");
            char_count++;

            API.shared.setEntitySyncedData(player.handle, "char_count", char_count);
            API.shared.triggerClientEvent(player, "showCharacterSelection");
        }
    }
}
