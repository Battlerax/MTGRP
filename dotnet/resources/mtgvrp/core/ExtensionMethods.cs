

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GTANetworkAPI;
using mtgvrp.player_manager;

namespace mtgvrp.core
{
    public static class Ex
    {
        public static Character GetCharacter(this Player player)
        {
            if (!API.Shared.HasEntityData(player, "Character")) return null;

            return (Character) player.GetData<Character>("Character");
        }
        public static Account GetAccount(this Player player)
        {
            if (!API.Shared.HasEntityData(player, "Account")) return null;

            return (Account)player.GetData<Account>("Account");
        }

        public static vehicle_manager.GameVehicle GetVehicle(this Entity veh)
        {
            if (!API.Shared.HasEntityData(veh, "Vehicle")) return null;

            return (vehicle_manager.GameVehicle)API.Shared.GetEntityData(veh, "Vehicle");
        }

        public static vehicle_manager.GameVehicle GetVehicle(this Vehicle veh)
        {
            if (!API.Shared.HasEntityData(veh, "Vehicle")) return null;

            return (vehicle_manager.GameVehicle)API.Shared.GetEntityData(veh, "Vehicle");
        }

        public static T CastTo<T>(this object obj)
        {
            return (T) obj;
        }

        public static bool IsInteger(this string i)
        {
            int t;
            return int.TryParse(i, out t);
        }

        // TODO: remove this shit - austin
        public static Quaternion ToQuat(this Vector3 vector)
        {
            return new Quaternion(vector.X, vector.Y, vector.Z, 0);
        }
    }

    static class EnumExtensions
    {
        public static IEnumerable<Enum> GetFlags(this Enum value)
        {
            return GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());
        }

        public static IEnumerable<Enum> GetIndividualFlags(this Enum value)
        {
            return GetFlags(value, GetFlagValues(value.GetType()).ToArray());
        }

        private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
        {
            ulong bits = Convert.ToUInt64(value);
            List<Enum> results = new List<Enum>();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                ulong mask = Convert.ToUInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;
                if ((bits & mask) == mask)
                {
                    results.Add(values[i]);
                    bits -= mask;
                }
            }
            if (bits != 0L)
                return Enumerable.Empty<Enum>();
            if (Convert.ToUInt64(value) != 0L)
                return results.Reverse<Enum>();
            if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<Enum>();
        }

        private static IEnumerable<Enum> GetFlagValues(Type enumType)
        {
            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    //yield return value;
                    continue; // skip the zero value
                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }
    }
}
