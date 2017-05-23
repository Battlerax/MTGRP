using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.core
{
    public static class ClientEx
    {
        public static Character GetCharacter(this Client player)
        {
            return (Character)API.shared.getEntityData(player, "Character");
        }
        public static Account GetAccount(this Client player)
        {
            return (Account)API.shared.getEntityData(player, "Account");
        }
    }
}
