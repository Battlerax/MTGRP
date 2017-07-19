

using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using mtgvrp.player_manager;
using Object = System.Object;

namespace mtgvrp.core
{
    public static class Ex
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

        public static vehicle_manager.Vehicle GetVehicle(this NetHandle veh)
        {
            if (!API.shared.hasEntityData(veh, "Vehicle")) return null;

            return (vehicle_manager.Vehicle)API.shared.getEntityData(veh, "Vehicle");
        }

        public static T CastTo<T>(this object obj)
        {
            return (T) obj;
        }
    }
}
