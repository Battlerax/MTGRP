using GTANetworkServer;
using mtgvrp.player_manager;

namespace mtgvrp.core
{
    public static class ClientEx
    {
        public static Character GetCharacter(this Client player)
        {
            if (!API.shared.hasEntityData(player, "Character")) return null;

            return (Character)API.shared.getEntityData(player, "Character");
        }
        public static Account GetAccount(this Client player)
        {
            if (!API.shared.hasEntityData(player, "Account")) return null;

            return (Account)API.shared.getEntityData(player, "Account");
        }
    }
}
