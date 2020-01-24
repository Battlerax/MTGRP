using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using MongoDB.Bson.Serialization.Attributes;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;

using Object = GTANetworkAPI.Object;

namespace mtgvrp.Components.Groups
{
    public class Group
    {
        [BsonId]
        public int DatabaseId { get; set; }

        public int MemberCount { get; set; }
        public int LeaderId { get; set; }
        public int InvitePower { get; set; }
        public int Type { get; set; }
        public int RadioAccess { get; set; }
        public int OOCRadio { get; set; }
        public int DeptRadio { get; set; }
        public int RankLimit { get; set; }
        public int DivisionLimit { get; set; }
        public int PrisonAccess { get; set; }
        public int Tier { get; set; }

        public long Vault { get; set; }

        public string Name { get; set; }
        public string RadioColour { get; set; }
        public string OOCRadioColour { get; set; }

        public List<GroupDivision> Divisions { get; set; }
        public List<GameVehicle> Vehicles { get; set; }
        public List<GroupLocker> Lockers { get; set; }
        public List<Object> Objects { get; set; } = new List<Object>();

        [BsonIgnore]
        public List<Character> OnlineMembers { get; set; } = new List<Character>();

        /// <summary>
        /// Called after initial data was loaded from the database
        /// Additional data is loaded here
        /// </summary>
        public void Load()
        {
            // load divisions, lockers and vehicles @Dylan
            // with reference id being the DatabseId from this class

            // if grouptype = leo, load Jail by groupid
        }

        public bool TryGetRank(Character member, out GroupRank rank)
        {
            GroupDivision div = Divisions.Find(x => x.DatabaseId == member.DivisionId);
            rank = div.Ranks.Find(x => x.DatabaseId == member.RankId);
            return rank != null;
        }

        public bool TryGetDivision(Character member, out GroupDivision div)
        {
            div = Divisions.Find(x => x.DatabaseId == member.DivisionId);
            return div != null;
        }

        public string GetRankName(Character member)
        {
            GroupDivision div = Divisions.Find(x => x.DatabaseId == member.DivisionId);
            GroupRank rank = div.Ranks.Find(x => x.DatabaseId == member.RankId);
            if (rank != null) return rank.Name;
            else return "";
        }

        public string GetDivisionName(Character member)
        {
            GroupDivision div = Divisions.Find(x => x.DatabaseId == member.DivisionId);
            if (div != null) return div.Name;
            else return "";
        }
    }

    public enum GroupType
    {
        CIV = 0,
        LEO = 1,
        MED = 2,
        GOV = 3,
        ILLEGAL = 4,
    }

    /// <summary>
    /// LockerWeapon belongs to a specific locker.
    /// </summary>
    public class LockerWeapon
    {
        [BsonId]
        public int DatabaseId { get; set; }
        public int LockerId { get; set; }
        public int Hash { get; set; }
        public int Stock { get; set; }
        public int Cost { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    /// Lockers belong to the group, with a division access list being available
    /// </summary>
    public class GroupLocker
    {
        [BsonId]
        public int DatabaseId { get; set; }
        public int GroupId { get; set; }
        public int Stock { get; set; }
        public int Dimension { get; set; }

        public Vector3 Position { get; set; }

        public List<int> DivisionAccess = new List<int>(); // a list of division id's that can access the 
        public List<LockerWeapon> Weapons { get; set; }

        public void Load()
        {
            // load Weapons belonging to the locker by the groupid @Dylan
        }
    }

    public class GroupDivision
    {
        [BsonId]
        public int DatabaseId { get; set; }
        public int GroupId { get; set; }
        public string Name { get; set; }
        public int LeaderId { get; set; }

        public List<GroupRank> Ranks { get; set; }

        public void Load()
        {
            // Load ranks belonging to this division by this division's id @Dylan
        }
    }

    /// <summary>
    /// The rank belongs to a division, not specifically a group.
    /// </summary>
    public class GroupRank
    {
        [BsonId]
        public int DatabaseId { get; set; }
        public int DivisionId { get; set; }
        public string Name { get; set; }
        public int Pay { get; set; }
        public int Authority { get; set; }
    }

    /// <summary>
    /// Loaded if
    /// </summary>
    public class Jail
    {
        [BsonId]
        public int DatabaseId { get; set; }
        public int GroupId { get; set; }

        public Vector3 ExteriorPosition { get; set; }

        public List<Vector3> InteriorPositions { get; set; } // load the list of interior positions (if more than one cell)
    }
}
