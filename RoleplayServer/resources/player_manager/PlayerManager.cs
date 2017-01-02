using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace RoleplayServer
{
    class PlayerManager : Script
    {
        public List<Character> players = new List<Character>();

        public PlayerManager()
        {
            DebugManager.debugMessage("[PlayerM] Initalizing player manager...");

            API.onPlayerConnected += OnPlayerConnected;
            API.onPlayerDisconnected += OnPlayerDisconnected;

            DebugManager.debugMessage("[PlayerM] Player Manager initalized.");
        }

        public void OnPlayerConnected(Client player)
        {
            Account account = new RoleplayServer.Account();
            Character character = new Character();

            API.setEntityData(player.handle, "Character", character);
            API.setEntityData(player.handle, "Account", account);
            character.client = player;

            players.Add(character);
        }

        public void OnPlayerDisconnected(Client player, string reason)
        {
            //Save data
            Character character = API.getEntityData(player.handle, "Character");


            API.resetEntityData(player.handle, "Character");
            players.Remove(character);
        }

        public static bool IsPlayerRegistered(Client player)
        {
            return true;
            /*bool return_code = false;

            try
            {
                MySqlConnection conn = new MySqlConnection(DatabaseManager.connection_info);
                conn.Open();

                MySqlCommand query = new MySqlCommand();

                query.Connection = conn;
                query.CommandText = "SELECT * FROM account_directory WHERE socialClubName = @socialClubName";
                query.Prepare();

                query.Parameters.AddWithValue("@socialClubName", player.socialClubName);

                MySqlDataReader data = query.ExecuteReader();

                if (data.HasRows)
                {
                    return_code = true;
                }

                conn.Close();
            }
            catch (MySqlException e)
            {
                DebugManager.debugMessage("[PlayerM] An error occured when checking if a player was registered. (" + player.socialClubName + ")");
                DebugManager.debugMessage(e.ToString(), DebugManager.STACKTRACE_PRINT_LEVEL);
            }
            return return_code;*/
        }
    }
}
