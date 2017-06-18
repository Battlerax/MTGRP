using GTANetworkServer;
using mtgvrp.player_manager;

namespace mtgvrp.core
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
