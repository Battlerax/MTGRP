using System;
using GTANetworkServer;
using GTANetworkShared;

namespace RoleplayServer
{
    class LoginManager : Script
    {
        public LoginManager()
        {
            DebugManager.debugMessage("[LoginM] Initalizing Login Manager...");

            API.onPlayerConnected += OnPlayerConnected;
            API.onPlayerFinishedDownload += OnPlayerFinishedDownload;

            DebugManager.debugMessage("[LoginM] Login Manager initalized.");
        }

        public void OnPlayerConnected(Client player)
        {
            DebugManager.debugMessage("[LoginM] " + player.name + " has connected to the server. (IP: " + player.address + ")");
        }

        public void OnPlayerFinishedDownload(Client player)
        {
            API.setPlayerToSpectator(player);
            if (PlayerManager.IsPlayerRegistered(player))
            {
                API.sendChatMessageToPlayer(player, "~g~ This account is already registered. Use /login [password] to access character selection.");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "~g~ You are not registered! Please use /register to create an account.");
            }

            API.triggerClientEvent(player, "onPlayerConnectedEx");
        }

       
        [Command("login")]
        public void login_cmd(Client player)
        {
            API.unspectatePlayer(player);
            API.triggerClientEvent(player, "login_finished");
        }
    }
}
